# Brush & Render Speed Improvements

## Problem

Large brush operations cause visible lag — the brush cursor stutters and the XNA render loop starves. The root cause is that per-tile work during brush strokes (undo save, pixel modification, render updates, UV cache resets) all execute synchronously on the UI thread per mouse move event.

## Update Priority (User-Defined)

| Priority | Component | Why |
|----------|-----------|-----|
| **1 (Critical)** | PixelMap updates | Primary visual feedback that a draw operation occurred. Visible at all zoom levels. |
| **2 (Low)** | UV cache / RenderBlender | Only visible at texture zoom. Usually small tile counts at that zoom level. |
| **3 (Never critical)** | Minimap | Currently renders full map on UI thread via `WriteableBitmap`. Should never block. |

## Current Architecture

### Brush Stroke Hot Path (per `MouseMove`)

```
BrushTool.MouseMove
  → ProcessDraw → DrawLine → FillSolid
    FOR EACH pixel in brush area:
      1. UndoManager.SaveTile(pixel)              — lock + dictionary insert
      2. wvm.SetPixel(pixel.X, pixel.Y)            — modify world tile
         → WorldEditor.SetPixel
           → _notifyTileChanged(x, y, 1, 1)
             ├→ UpdateRenderPixel(x, y)            — GetTileColor + PixelMap.SetPixelColor
             │   ├→ FilterOverlay.SetMask(x, y)
             │   └→ BuffTileCache.UpdateTile(x, y)
             └→ rb.UpdateTile(x, y, 1, 1)          — ResetUVCache (3×3 neighborhood)
      3. BlendRules.ResetUVCache(wvm, x, y, 1, 1)  — SAME 3×3 reset AGAIN ← redundant
```

### Undo/Redo Render Path

```
WorldViewModel.Undo/Redo
  → UndoManager.Undo()              — returns List<Vector2Int32> of changed tiles
  → UpdateRenderPixels(changedTiles) — per-tile: GetTileColor + SetPixelColor + FilterOverlay + BuffTileCache
  → _renderBlender.UpdateTiles(tiles) — batch ResetUVCache for all tiles
```

### PixelMap Chunked Buffer Architecture

```
PixelMapManager
├── ColorBuffers[chunkIndex][pixelIndex]  — Color[] per chunk
├── BufferUpdated[chunkIndex]              — dirty flag per chunk
├── TileWidth/TileHeight                   — chunk size (100-256, divides world evenly)
└── SetPixelColor(x, y, color)            — writes single pixel, marks chunk dirty

XNA Render Loop (WorldRenderXna)
├── SyncChunks: if BufferUpdated[i] → Texture2D.SetData(ColorBuffers[i])
├── if zoomed out → DrawPixelTiles() — draws chunk textures
└── if zoomed in  → DrawTileBackgrounds() + tile textures
```

Key: `SetPixelColor` has **no locking** — it writes directly to a `Color[]` array and sets a boolean dirty flag. The XNA render loop picks up dirty chunks on the next frame via `Texture2D.SetData()`. This is already designed for async updates.

### Notification Delegate

```csharp
// WorldViewModel.cs line 1250-1255
NotifyTileChanged updateTiles = (x, y, width, height) =>
{
    UpdateRenderPixel(x, y);                        // PixelMap + FilterOverlay + BuffTileCache
    rb.UpdateTile(x, y, width, height);             // ResetUVCache
};
```

This fires for **every** `_notifyTileChanged` call in `WorldEditor.SetPixel` — per tile, not per stroke.

### Minimap

```csharp
// Called from _undoApplied in SaveUndo()
private void UpdateMinimap()
{
    RenderMiniMap.UpdateMinimap(CurrentWorld, ref _minimapImage, ...);
    // ^ iterates every pixel in minimap bitmap, Lock/Unlock on WriteableBitmap
}
```

## Proposed Improvements

### Phase 1: Eliminate Redundant UV Cache Resets

**Effort: Low | Impact: ~2× UV cache work eliminated**

`BrushTool.FillSolid` calls `BlendRules.ResetUVCache(wvm, x, y, 1, 1)` per tile at line 796. The `_notifyTileChanged` delegate also calls `rb.UpdateTile(x, y, 1, 1)` which does the same `ResetUVCache`. This doubles the UV cache work.

**Fix:** Remove the `BlendRules.ResetUVCache` call from BrushTool's `FillSolid` (and `FillHollowCached`, `FillHollow`). The notify path already handles it. Same applies to `CommitCadPath` and `DrawSingleTile`.

Affects: `BrushTool.cs` lines 796, 839, 871, 548, 572.

### Phase 2: Batched Brush Stroke Updates

**Effort: Medium | Impact: Major — eliminates per-tile notification overhead**

Replace per-tile `_notifyTileChanged` during brush strokes with a single batched update after the fill loop.

#### Step 2a: Add `notify` parameter to `WorldEditor.SetPixel`

```csharp
public void SetPixel(int x, int y, PaintMode? mode = null, bool? erase = null, bool notify = true)
{
    // ... existing logic ...
    // At end, only notify if requested:
    if (notify) _notifyTileChanged?.Invoke(x, y, 1, 1);
}
```

Callers outside BrushTool continue to pass `notify: true` (default). BrushTool passes `notify: false`.

Note: `_notifyTileChanged` is called from multiple switch cases within SetPixel (SetTrack, SetPlatform, etc). May be cleaner to use a field toggle (`_suppressNotify`) than parameter threading.

#### Step 2b: BrushTool collects dirty bounds

```csharp
protected virtual void FillSolid(IList<Vector2Int32> area, int count)
{
    int minX = int.MaxValue, minY = int.MaxValue;
    int maxX = int.MinValue, maxY = int.MinValue;

    for (int i = 0; i < count; i++)
    {
        // ... existing validation + SetPixel(notify: false) ...
        if (pixel.X < minX) minX = pixel.X;
        if (pixel.Y < minY) minY = pixel.Y;
        if (pixel.X > maxX) maxX = pixel.X;
        if (pixel.Y > maxY) maxY = pixel.Y;
    }

    // Single batched update
    int w = maxX - minX + 1;
    int h = maxY - minY + 1;
    _wvm.UpdateRenderRegionSync(minX, minY, w, h);  // PixelMap only
    _wvm.QueueUVCacheReset(minX, minY, w, h);        // Deferred
}
```

#### Step 2c: New `UpdateRenderRegionSync` for PixelMap only

```csharp
public void UpdateRenderRegionSync(int x, int y, int width, int height)
{
    // Bounds check
    for (int ty = y; ty < y + height; ty++)
    {
        Color bgColor = new Color(GetBackgroundColor(ty).PackedValue);
        for (int tx = x; tx < x + width; tx++)
        {
            PixelMap.SetPixelColor(tx, ty,
                Render.PixelMap.GetTileColor(CurrentWorld.Tiles[tx, ty], bgColor, ...));
        }
    }
    // BuffTileCache update only for the region
    BuffTileCache.UpdateRegion(x, y, width, height, CurrentWorld);
}
```

**Optimization:** `GetBackgroundColor(y)` is computed once per row instead of once per tile.

### Phase 3: Deferred UV Cache Reset

**Effort: Low | Impact: Removes UV work from brush stroke entirely**

UV cache (`uvTileCache`, `uvWallCache`, `lazyMergeId`) only matters for texture-zoom rendering. At pixel-map zoom levels, this work is invisible.

#### Option A: Queue and reset on next texture render frame

```csharp
// WorldRenderXna maintains a dirty rect queue
private RectangleInt32? _pendingUVReset;

public void QueueUVCacheReset(int x, int y, int w, int h)
{
    if (_pendingUVReset == null)
        _pendingUVReset = new RectangleInt32(x, y, w, h);
    else
        _pendingUVReset = _pendingUVReset.Value.Union(x, y, w, h);
}

// In render loop, before drawing textures:
if (_pendingUVReset != null && AreTexturesVisible())
{
    BlendRules.ResetUVCache(_world, _tilePicker, ...);
    _pendingUVReset = null;
}
```

#### Option B: Skip UV reset entirely during brush strokes

The UV cache has a sentinel value (`0xFFFF`). When the texture renderer encounters it, it recalculates on demand. If we simply set `uvTileCache = 0xFFFF` when modifying a tile in `SetPixelAutomatic`, the render loop auto-repairs. This is what `ResetUVCache` already does — the question is whether the render loop handles the sentinel correctly without an explicit reset.

**Recommendation:** Option A is safer and explicit. Option B requires auditing the render loop.

### Phase 4: Async Minimap Updates

**Effort: Low | Impact: Removes minimap lag from UI thread**

Move minimap rendering to a background thread with coalesced updates.

```csharp
private Timer _minimapTimer;
private bool _minimapDirty;

// Replace direct UpdateMinimap() calls with:
public void InvalidateMinimap()
{
    _minimapDirty = true;
}

// Timer callback (e.g., every 500ms):
private void MinimapTimerTick(object state)
{
    if (!_minimapDirty) return;
    _minimapDirty = false;

    // Render to byte[] on background thread
    var pixels = RenderMiniMap.RenderToBuffer(CurrentWorld, ...);

    // Blit to WriteableBitmap on UI thread
    Dispatcher.BeginInvoke(() =>
    {
        MinimapImage.Lock();
        Marshal.Copy(pixels, 0, MinimapImage.BackBuffer, pixels.Length);
        MinimapImage.AddDirtyRect(new Int32Rect(0, 0, MinimapImage.PixelWidth, MinimapImage.PixelHeight));
        MinimapImage.Unlock();
    });
}
```

### Phase 5: Parallel PixelMap Color Computation (Future)

**Effort: High | Impact: Significant for large operations**

`PixelMap.SetPixelColor` writes to `ColorBuffers[chunkIndex][pixelIndex]` — each pixel is an independent array write. `GetTileColor` reads from `World.Tiles[x, y]` (immutable during the update).

This means color computation for a dirty region could use `Parallel.For` per row:

```csharp
Parallel.For(y, y + height, ty =>
{
    Color bgColor = new Color(GetBackgroundColor(ty).PackedValue);
    for (int tx = x; tx < x + width; tx++)
    {
        PixelMap.SetPixelColor(tx, ty,
            PixelMap.GetTileColor(CurrentWorld.Tiles[tx, ty], bgColor, ...));
    }
});
```

**Caution:** `BufferUpdated[chunkIndex]` flag is per-chunk — concurrent writes to the same chunk from different rows could race on the flag. Since it's a `bool` set to `true`, this is benign (no false→true→false race), but should be verified. Could use `Volatile.Write` for correctness.

**Prerequisite:** World tiles must not be mutated during parallel computation. This is safe during undo/redo (mutation happens before render update) but needs care during brush strokes if we compute colors while the brush loop is still running.

## Summary: Per-Phase Work Reduction

| Phase | Work Eliminated Per Brush Stroke (50×50 brush) |
|-------|------------------------------------------------|
| **Phase 1** | 2,500 × ResetUVCache(3×3) = 22,500 cache writes removed |
| **Phase 2** | 2,500 × individual notify calls → 1 batched call. GetBackgroundColor: 2,500 → 50 (per-row). |
| **Phase 3** | Remaining UV cache work deferred to next texture render frame (or skipped at pixelmap zoom) |
| **Phase 4** | Full minimap re-render moved off UI thread entirely |
| **Phase 5** | PixelMap color computation parallelized across CPU cores |

## Priority Order

1. **Phase 1** — trivial fix, immediate 2× UV improvement
2. **Phase 2** — biggest single improvement, batched updates
3. **Phase 4** — easy win, minimap off UI thread
4. **Phase 3** — defer UV cache, removes remaining per-tile overhead
5. **Phase 5** — only if profiling shows PixelMap color computation is bottleneck

## Undo/Redo Render Path (Separate from Brush)

The undo/redo render path (`WorldViewModel.Commands.cs` lines 338-357) is already partially batched:
```csharp
UpdateRenderPixels(changedTiles);          // per-tile loop but single call
_renderBlender?.UpdateTiles(changedTiles); // batched ResetUVCache
```

This path benefits from:
- **Phase 4** — minimap moved to timer, not called from `_undoApplied`
- **Phase 5** — `UpdateRenderPixels` could use `Parallel.For` since tiles are already committed
- **Phase 2b/2c** — could use the same `UpdateRenderRegionSync` for bounding-rect optimization

## Files Affected

| File | Phases |
|------|--------|
| `src/TEdit/Editor/Tools/BrushTool.cs` | 1, 2 |
| `src/TEdit.Editor/WorldEditor.cs` | 2 |
| `src/TEdit/ViewModel/WorldViewModel.cs` | 2, 4 |
| `src/TEdit/ViewModel/WorldViewModel.Editor.cs` | 2, 5 |
| `src/TEdit/View/WorldRenderXna.xaml.cs` | 3 |
| `src/TEdit/Render/BlendRules.cs` | 1, 3 |
| `src/TEdit/Render/RenderMiniMap.cs` | 4 |
| `src/TEdit/Render/PixelMapManager.cs` | 5 |

## Success Criteria

- Brush cursor tracks mouse position without visible lag at 50×50 brush size
- PixelMap updates remain the fastest visual feedback path
- UV cache resets happen lazily, only when texture zoom is active
- Minimap never blocks UI thread
- No visual artifacts or stale rendering after brush strokes
