# Undo/Redo Memory & Save Optimizations

## Problem

Large brush operations (50x50+) cause UI lag during undo buffer flushes. The current design serializes tiles to disk via `BinaryWriter` when the in-memory buffer exceeds `FlushSize` (10,000 tiles). Even with the recent async flush refactor (background serialization + I/O), the disk-based approach has overhead from serialization, allocation, and file I/O that accumulates during sustained painting.

The tile deduplication compression means the in-memory representation is already compact — many tiles share the same `Tile` struct value. This creates an opportunity to keep undo data in memory longer before spilling to disk.

## Current Architecture

```
UndoBuffer (per undo step)
├── _undoTiles: Dictionary<Tile, HashSet<Vector2Int32>>  — deduplicated tile data
├── _tileOrder: List<Tile>                                — insertion order
├── _writer: BinaryWriter                                 — disk file
├── _flushQueue: BlockingCollection<FlushBatch>           — async flush queue
└── _writerTask: Task                                     — background writer

UndoManager
├── _buffer: UndoBuffer          — current undo step
├── _pendingClose: Task          — tracks async close completion
├── SaveUndo() → CloseAsync()    — non-blocking, queues remaining data
├── Undo() → WaitForPendingClose — blocks only if serialization pending
└── Redo() → WaitForPendingClose — blocks only if serialization pending
```

### Flow: Brush Stroke
1. `MouseDown` → creates new undo buffer
2. `MouseMove` → `SaveTile()` per pixel → `UndoBuffer.Add()` → dictionary insert
3. When `_undoTiles` exceeds `FlushSize`: `SaveTileData()` snapshots collections, queues `FlushBatch` to background thread
4. `MouseUp` → `SaveUndo()` → `CloseAsync()` (non-blocking)
5. Background thread: serializes tiles via `World.SerializeTileData()`, writes to `BinaryWriter`

### Flow: Undo/Redo
1. `WaitForPendingClose()` — blocks if prior save still serializing
2. Opens undo file with `BinaryReader`
3. Iterates `ReadUndoTilesFromStream()` — deserializes tiles
4. Applies each tile to world, saves current state to redo buffer
5. Closes redo buffer (synchronous — file needed immediately)

## Proposed Improvements

### Phase 1: Hybrid Memory/Disk Undo

Keep the N most recent undo steps entirely in memory. Only serialize to disk when they age out or total memory exceeds a threshold.

**Rationale:** The most common undo is the most recent step. If it's in memory, undo is instant — no deserialization, no disk I/O. The deduplication compression means memory usage is already efficient.

#### Design

```
UndoBuffer
├── _undoTiles: Dictionary<Tile, HashSet<Vector2Int32>>  — same as now
├── _memoryStream: MemoryStream?                          — serialized in-memory (when closed)
├── _diskFile: string?                                    — disk path (when spilled)
├── State: enum { Collecting, InMemory, OnDisk }
```

- **Collecting**: actively receiving tiles via `Add()`. Same as current behavior.
- **InMemory**: closed, serialized to `MemoryStream`. Fast to read back for undo.
- **OnDisk**: spilled to disk file. Same as current behavior.

**UndoManager changes:**
- Track total memory of in-memory buffers (sum of `MemoryStream.Length`)
- When total exceeds threshold (e.g., 100 MB), spill oldest in-memory buffers to disk
- `Undo()` / `Redo()`: check buffer state — if `InMemory`, read from `MemoryStream` directly
- Keep at least the 3 most recent steps in memory regardless of size

**Memory estimation:** A typical undo step for a 50x50 brush stroke:
- ~2,500 tile locations, maybe 10-50 unique tiles after deduplication
- Each unique tile: ~15-20 bytes serialized + 8 bytes per location
- Total: ~20-25 KB per step (with deduplication)
- 100 MB threshold ≈ 4,000+ undo steps in memory

#### Close path changes
```
CloseAsync():
  SaveTileData() → serialize to MemoryStream instead of BinaryWriter
  Write chests/signs/entities to same MemoryStream
  State = InMemory
  // No disk I/O at all

SpillToDisk(UndoBuffer buffer):
  // Called by UndoManager when memory threshold exceeded
  Open FileStream
  Copy MemoryStream → FileStream
  Release MemoryStream
  State = OnDisk
```

### Phase 2: Pooled Buffers (RecyclableMemoryStream)

Replace raw `MemoryStream` and `byte[]` allocations with pooled alternatives to reduce GC pressure.

#### RecyclableMemoryStreamManager
- NuGet: `Microsoft.IO.RecyclableMemoryStream`
- Provides pool-backed `MemoryStream` that avoids Large Object Heap fragmentation
- Drop-in replacement for `MemoryStream` — same API
- Particularly beneficial here because undo buffers are created/disposed frequently

#### ArrayPool for serialization
- `World.SerializeTileData()` currently returns a fresh `byte[]` per call
- Could accept an `IBufferWriter<byte>` or `Span<byte>` from `ArrayPool<byte>.Shared`
- Eliminates per-tile allocation in the serialization hot path
- **Note:** This requires changes to `World.SerializeTileData()` signature — evaluate scope

#### Implementation
```csharp
private static readonly RecyclableMemoryStreamManager _streamPool = new();

// In SaveTileData():
var ms = _streamPool.GetStream("UndoBuffer");
// serialize to ms instead of BinaryWriter over FileStream
```

### Phase 3: Serialization Speed

#### IBufferWriter pattern
Instead of `BinaryWriter` wrapping a stream, write directly to `IBufferWriter<byte>`:
- Avoids per-write method call overhead
- Can batch multiple small writes into contiguous memory
- Compatible with both `MemoryStream` and `PipeWriter`

#### Potential: System.IO.Pipelines
For disk spill path, `Pipe` provides:
- Built-in backpressure (writer pauses if reader is slow)
- Zero-copy between serialization and I/O
- Automatic buffer management
- **Complexity cost** — may not be worth it if Phase 1 eliminates most disk I/O

## Priority Order

1. **Phase 1** — biggest user-facing impact. Eliminates disk I/O for common undo/redo.
2. **Phase 2 (RecyclableMemoryStream only)** — low effort, reduces GC pauses during painting.
3. **Phase 2 (ArrayPool)** — medium effort, requires `SerializeTileData` refactor.
4. **Phase 3** — only if profiling shows serialization is still a bottleneck after Phase 1.

## Risks

- **Memory pressure on 32-bit or low-RAM systems**: 100 MB threshold may need to be configurable or adaptive. Monitor with `GC.GetTotalMemory()`.
- **RecyclableMemoryStream lifecycle**: must ensure streams are properly disposed to return to pool. `using` pattern handles this.
- **SerializeTileData refactor scope**: changing the signature affects multiple callers (world save, clipboard, undo). Could introduce a parallel overload instead of modifying the existing one.

## Success Criteria

- Undo/redo of recent operations completes with zero disk I/O
- No measurable GC pause increase during sustained painting
- Memory usage stays within configurable bounds
- All existing undo/redo round-trip tests pass
