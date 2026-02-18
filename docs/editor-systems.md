# Editor Systems — Technical Reference

This document describes the internal architecture of TEdit's editor subsystems as they exist today. Its purpose is to guide a future refactoring that extracts a `TEdit.Drawing` library decoupled from WPF rendering.

> **Source of truth:** All descriptions reference the code in the `feature/performance-improvements` branch. Line numbers are approximate — use symbol search to locate definitions.

---

## 1. Tile Data Model

### Tile

`TEdit.Terraria.Tile` is the fundamental data unit. Every cell in the world grid is a `Tile` value stored in `World.Tiles[x, y]` (a `Tile[,]` array). Key fields:

| Field | Type | Purpose |
|---|---|---|
| `IsActive` | `bool` | Whether a foreground tile exists |
| `Type` | `ushort` | Tile type ID (indexes into `WorldConfiguration.TileProperties`) |
| `Wall` | `ushort` | Background wall type ID |
| `U`, `V` | `short` | Frame coordinates (sprite sheet offset for framed tiles) |
| `LiquidAmount` | `byte` | 0–255 liquid fill level |
| `LiquidType` | `LiquidType` | Water, Lava, Honey, Shimmer, None |
| `WireRed/Blue/Green/Yellow` | `bool` | Wiring layers |
| `TileColor`, `WallColor` | `byte` | Paint color indices |
| `BrickStyle` | `BrickStyle` | Full, HalfBrick, SlopeTopRight, etc. |
| `Actuator`, `InActive` | `bool` | Actuator presence and activation state |
| `InvisibleBlock/Wall`, `FullBrightBlock/Wall` | `bool` | Echo/Illuminant coatings |

`Tile` implements `ICloneable` and supports value-equality via `GetHashCode`/`Equals`, which is critical for the undo buffer's tile-keyed compression (see section 5).

### World.Tiles

`World.Tiles` is a `Tile[width, height]` 2D array. Access is always `Tiles[x, y]` (column-major). Bounds checking uses `World.ValidTileLocation(Vector2Int32)` or `ValidTileLocation(int x, int y)`.

### TilePicker

**File:** `src/TEdit.Editor/TilePicker.cs`

`TilePicker` is a reactive settings object that controls *what* gets painted. It holds:

- **PaintMode** — which category of tile data to modify (see below)
- **Tile/Wall type** — the tile and wall IDs to paint
- **Style flags** — `TileStyleActive`, `WallStyleActive`, `BrickStyleActive`, `ExtrasActive`, `TilePaintActive`, `WallPaintActive`
- **Mask settings** — `TileMaskMode`/`WallMaskMode` (`Off`, `Match`, `Empty`, `NotMatching`) with corresponding mask type IDs
- **Wire settings** — per-color active flags, replace mode
- **Liquid settings** — type and amount
- **Coating settings** — echo/illuminant for tiles and walls
- **IsEraser** — inverts tile/wall placement to deletion

### PaintMode Enum

**File:** `src/TEdit.Editor/PaintMode.cs`

```csharp
public enum PaintMode
{
    TileAndWall,  // Place/erase tiles and walls
    Wire,         // Place/erase/replace wiring
    Liquid,       // Place/erase liquids
    Track,        // Minecart track placement and hammering
    Sprites       // Erase-only mode for framed tiles
}
```

`PaintMode` determines which branch of `WorldEditor.SetPixel` executes.

---

## 2. Tool Architecture

### ITool Interface

**File:** `src/TEdit/Editor/Tools/ITool.cs`

```csharp
public interface ITool
{
    ToolType ToolType { get; }
    BitmapImage Icon { get; }
    SymbolRegular SymbolIcon { get; }
    ImageSource? VectorIcon { get; }
    bool IsActive { get; set; }
    string Name { get; }
    string Title { get; }
    bool PreviewIsTexture { get; }
    void MouseDown(TileMouseState e);
    void MouseMove(TileMouseState e);
    void MouseUp(TileMouseState e);
    void MouseWheel(TileMouseState e);
    double PreviewScale { get; }
    int PreviewOffsetX { get; }
    int PreviewOffsetY { get; }
    WriteableBitmap PreviewTool();
}
```

**WPF coupling:** `ITool` returns `WriteableBitmap` and `BitmapImage` — WPF types that would need abstraction for cross-platform use.

### BaseTool

**File:** `src/TEdit/Editor/Tools/BaseTool.cs`

Abstract base implementing `ITool`. Provides:

- **`_wvm`** — reference to `WorldViewModel` (the central WPF coordinator)
- **`_preview`** — a `WriteableBitmap` for cursor preview rendering
- **Input helpers** — `GetModifiers()` (Win32 `GetKeyState` P/Invoke), `GetActiveActions(TileMouseState)`, `IsActionActive()`
- Default no-op implementations for all mouse events

### Tool Hierarchy

```
ITool
  └─ BaseTool (abstract, ReactiveObject)
       ├─ BrushToolBase (drawing logic, spray, shape dispatch)
       │    └─ BrushTool (concrete, Name="Brush")
       ├─ FillTool (flood fill)
       ├─ SpriteTool2 (sprite placement)
       ├─ PasteTool (clipboard paste)
       ├─ SelectionTool, PickerTool, PointTool, etc.
       └─ ...
```

### Mouse Event Lifecycle

The WPF `ScrollViewer` / `XnaContentHost` captures mouse events over the map canvas and dispatches them to the active tool via `WorldViewModel`:

1. **MouseDown** — Start a new stroke. Allocate/reset `CheckTiles`, determine drawing mode from input bindings (`editor.draw`, `editor.draw.constrain`, `editor.draw.line`).
2. **MouseMove** — Continue the stroke. For brush tools, calls `ProcessDraw` or `ProcessSprayMove` on each move.
3. **MouseUp** — End the stroke. Final draw, then `UndoManager.SaveUndo()` to flush the undo buffer.

---

## 3. Brush System

### BrushSettings

**File:** `src/TEdit.Editor/BrushSettings.cs`

`BrushSettings` is a reactive object that defines brush geometry. It lives on `WorldViewModel.Brush` (WPF path) and `WorldEditor.Brush` (API path).

**Core properties:**

| Property | Type | Description |
|---|---|---|
| `Width`, `Height` | `int` | Brush dimensions (1–400, lockable) |
| `Shape` | `BrushShape` | Geometry type (see below) |
| `Outline` | `int` | Border thickness for hollow mode |
| `IsOutline` | `bool` | Enable hollow drawing |
| `Rotation` | `double` | Degrees rotation |
| `FlipHorizontal/Vertical` | `bool` | Mirror transforms |
| `IsSpray` | `bool` | Enable spray (random subset) mode |
| `SprayDensity` | `int` | 1–100% of shape points to paint per tick |
| `SprayTickMs` | `int` | 10–100ms interval between spray applications |
| `HasTransform` | `bool` | Computed: true if rotation or flip is active |

### BrushShape Enum

**File:** `src/TEdit.Editor/BrushShape.cs`

```
Square, Round, Right (diagonal \), Left (diagonal /), Star, Triangle, Crescent, Donut, Cross (X)
```

**Shape categories:**
- **Simple rect:** `Square` (or any shape when `Width<=1` or `Height<=1`) — uses fast `Fill.FillRectangleCentered`
- **Ellipse-based:** `Round`, `Crescent`, `Donut` — use `Fill.FillEllipseCentered` and variants; transforms use pixel-level inverse mapping
- **Polygon:** `Square`, `Star`, `Triangle` with transforms — vertices are transformed then re-filled via `Fill.FillPolygon`
- **Line shapes:** `Right`, `Left`, `Cross` — endpoints transformed, then `Shape.DrawLine` connects them

### Shape Geometry Dispatch

`BrushSettings.GetShapePoints(center, width, height)` is the unified entry point:

1. **Line shapes** → `GetLineShapePoints()` — transform endpoints, draw lines
2. **Polygon + transform** → `GetTransformedPolygonPoints()` — transform vertices, `Fill.FillPolygon`
3. **All others** → switch on `Shape` using `Fill.*` methods, then optionally `Fill.ApplyTransform`

### Shape Cache

Brush offsets are computed once and cached as center-relative `Vector2Int32[]` arrays:

- `_cachedOffsets` — all shape points relative to `(0,0)`
- `_cachedInteriorOffsets` — interior points for outline mode (computed via parametric shrink for Square/Round, or `ErodeShape` for others)
- `_cacheValid` — invalidated by any property change (via `BrushChange()`)
- `EnsureCacheValid()` — lazily rebuilds on next access

**Stamping:** `StampOffsets(center, output)` translates cached offsets to absolute coordinates, writing into a pre-allocated `List<Vector2Int32>` to avoid GC pressure.

### Outline Mode

When `IsOutline` is true, the brush draws a hollow shape:
- **Border** gets tiles (with walls suppressed if `WallStyleActive`)
- **Interior** gets walls (with tiles erased)
- Interior offsets are computed differently per shape type:
  - `Square`/`Round`: parametric — generate a smaller shape with `Width - Outline*2`
  - `Star`/`Triangle`/`Crescent`/`Donut`/line shapes: morphological erosion via `ErodeShape()` (separable Chebyshev distance)

### Line Thickness

`BrushSettings.ThickenLine(points, thickness)` expands line points by a circular kernel (Euclidean distance `<= r`). Used for line-shape brushes and their outline-mode erosion.

---

## 4. Drawing Pipeline

### Overview

```
MouseDown/Move/Up
  → ProcessDraw(tile)
    → DrawLine(to) or DrawLineP2P(endpoint)
      → [Simple rect path]  FillRectangle / FillRectangleLine
      → [Line shape path]   FillLineShapeSweep
      → [General path]      FillShape (per line point)
        → StampOffsets → FillSolid / FillHollowCached
          → CheckTiles dedup
          → UndoManager.SaveTile
          → WorldEditor.SetPixel (modify tile data)
          → WorldViewModel.UpdateRenderPixel (WPF render)
          → BlendRules.ResetUVCache (UV frame recalc)
```

### ProcessDraw

**File:** `src/TEdit/Editor/Tools/BrushTool.cs:301`

Routes based on drawing mode:
- **Constrain** (`_isConstraining`): locks to horizontal or vertical axis, uses `DrawLine`
- **Line** (`_isLineMode`): point-to-point from `_startPoint` to current, uses `DrawLineP2P`
- **Freehand** (`_isDrawing`): continuous from `_startPoint`, uses `DrawLine`, advances `_startPoint`

### DrawLine

Generates the pixel path between two points using `Shape.DrawLineTool` (Bresenham), then dispatches based on brush type:

1. **Simple rect** → `FillRectangleLine(prev, cur)` for each consecutive pair (union of rectangles along the path)
2. **Line shape (no outline)** → `FillLineShapeSweep(from, to)` — fills the quadrilateral swept by the line-shape brush
3. **General** → `FillShape(point)` for each point on the line

### FillShape

Stamps the cached brush offsets at the given center, then:
- If `IsOutline`: calls `FillHollowCached` using pre-built `_interiorOffsetSet`
- Otherwise: calls `FillSolid`

### FillLineShapeSweep

For line-shaped brushes (`Right`, `Left`, `Cross`), instead of stamping at discrete points, computes the quadrilateral swept between the brush at `from` and `to` positions:
1. Get segment endpoints relative to center
2. Apply flip/rotation transforms
3. Form quad vertices: `[from+start, from+end, to+end, to+start]`
4. Fill quad with `Fill.FillPolygon`
5. Apply thickness with `ThickenLine`
6. Pass to `FillSolid`

### FillSolid

**File:** `src/TEdit/Editor/Tools/BrushTool.cs:504`

The innermost drawing loop:

```csharp
for (int i = 0; i < count; i++)
{
    Vector2Int32 pixel = area[i];
    if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

    int index = pixel.X + pixel.Y * tilesWide;
    if (_wvm.CheckTiles[index] != generation)       // dedup check
    {
        _wvm.CheckTiles[index] = generation;         // mark visited
        if (_wvm.Selection.IsValid(pixel))            // selection mask
        {
            _wvm.UndoManager.SaveTile(pixel);         // save to undo buffer
            _wvm.SetPixel(pixel.X, pixel.Y);          // modify tile data
            BlendRules.ResetUVCache(_wvm, ...);        // recalc UV frames
        }
    }
}
```

### FillHollow / FillHollowCached

Two implementations:
- **`FillHollowCached`** — uses the pre-built `_interiorOffsetSet` (center-relative `HashSet<Vector2Int32>`). No per-call allocation. Used by `FillShape`.
- **`FillHollow`** (legacy) — takes explicit `area` and `interior` lists, builds a `HashSet` per call. Used by `FillRectangle`.

Both follow the same logic:
1. Paint **border** pixels (area minus interior) with tiles only (walls suppressed)
2. Paint **interior** pixels with walls only (tiles erased), then optionally walls

### CheckTiles Deduplication

`CheckTiles` is a flat `int[]` array indexed by `x + y * TilesWide`. Instead of clearing the entire array per stroke, a **generation counter** (`CheckTileGeneration`) is incremented at `MouseDown`. A tile is "visited" when `CheckTiles[index] == generation`. This avoids O(n) `Array.Clear` on every stroke.

Overflow handling: if `CheckTileGeneration` wraps to `<= 0`, it resets to 1 and clears the array.

### SetPixel (Tile Modification)

**File:** `src/TEdit.Editor/WorldEditor.cs:70`

`WorldEditor.SetPixel` is the single point where tile data is actually modified. It switches on `PaintMode`:

- **TileAndWall** — conditionally sets tile type, wall type, brick style, paint colors, actuators, coatings (each gated by its `*Active` flag and mask mode)
- **Wire** — sets wire flags, handles replace mode (swap one color for another), junction boxes
- **Liquid** — sets liquid amount and type
- **Track** — minecart rail placement with neighbor connection logic
- **Sprites** — erases framed tiles only

Mask modes (`MaskMode.Match`, `Empty`, `NotMatching`) filter which existing tiles are eligible for modification.

---

## 5. Undo/Redo System

### UndoManager

**File:** `src/TEdit/Editor/Undo/UndoManager.cs`

Orchestrates undo/redo operations with file-based storage.

**Lifecycle:**
1. During a stroke, `SaveTile(x, y)` is called per modified pixel → tiles accumulate in the current `UndoBuffer`
2. On `MouseUp`, `SaveUndo()` closes the buffer, writes it to disk, increments `_currentIndex`, creates a new buffer
3. `Undo()` reads the previous undo file, saves current state to a redo file, applies old tiles
4. `Redo()` reads the next redo file, saves current state to undo, applies redo tiles

**File naming:** `undo_temp_{index}` and `redo_temp_{index}` in a session-unique directory under `%TEMP%/TEdit/undo/`.

**Keep-alive:** A `Timer` touches an `alive.txt` file every 5 minutes. Old undo directories (>7 days or >20 minutes stale) are cleaned up on startup.

**Entity handling:** `SaveTile` also captures associated chests, signs, and tile entities at anchor positions. `ValidateAndRemoveChests` synchronizes entity collections when tiles change underneath them.

### UndoBuffer

**File:** `src/TEdit.Editor/Undo/UndoBuffer.cs`

Implements tile-keyed compression for efficient undo storage.

**Key insight:** Many tiles in a stroke are identical (e.g., painting 1000 tiles of stone). Instead of writing each `(tile, location)` pair individually, `UndoBuffer` groups locations by tile value:

```
Dictionary<Tile, HashSet<Vector2Int32>> _undoTiles
```

**Binary format (per unique tile group):**
```
[tile data bytes] [location count: int32] [x1: int32] [y1: int32] [x2: int32] [y2: int32] ...
```

**File structure:**
```
[unique tile group count: int32]      ← written at position 0 on Close()
[tile group 1]
[tile group 2]
...
[chest data]                           ← standard Terraria chest serialization
[sign data]
[tile entity data]
```

**Flush threshold:** Tiles are flushed to disk every 10,000 entries (`FlushSize`) to bound memory usage. The total unique-group count is tracked across flushes and written to the file header on `Close()`.

**Reading:** `ReadUndoTilesFromStream` yields `UndoTile` structs (tile + location), cloning the tile for each location to maintain independence.

### IUndoManager

**File:** `src/TEdit.Editor/Undo/IUndoManager.cs`

```csharp
public interface IUndoManager : IDisposable
{
    Task StartUndoAsync();
    void SaveTile(World world, Vector2Int32 location, bool removeEntities = false);
    void SaveTile(World world, int x, int y, bool removeEntities = false);
    Task SaveUndoAsync();
    Task UndoAsync(World world);
    Task RedoAsync(World world);
}
```

This interface is the decoupled contract used by `WorldEditor` and `DrawApi`. `UndoManagerWrapper` adapts the WPF-side `UndoManager` to this interface.

---

## 6. WorldEditor (API Path)

### WorldEditor

**File:** `src/TEdit.Editor/WorldEditor.cs`

`WorldEditor` is the **rendering-decoupled** alternative to the WPF tool pipeline. It lives in `TEdit.Editor` (no WPF dependency) and operates on `World`, `TilePicker`, `ISelection`, and `IUndoManager` directly.

**Key differences from BrushToolBase:**

| Aspect | BrushToolBase (WPF) | WorldEditor (Decoupled) |
|---|---|---|
| Assembly | `TEdit` (WPF app) | `TEdit.Editor` (netstandard-compatible) |
| Render update | `UpdateRenderPixel` + `BlendRules.ResetUVCache` | `NotifyTileChanged` delegate (nullable) |
| Undo | `UndoManager` directly | `IUndoManager` interface |
| CheckTiles | `WorldViewModel.CheckTiles` | Own `_checkTiles` array |
| Brush cache | Shared `BrushSettings` with cached offsets | Own `BrushSettings` instance |

**Methods mirror BrushToolBase:**
- `SetPixel(x, y, mode?, erase?)` — full tile modification logic (identical switch/case)
- `FillSolid(area)`, `FillHollow(area, interior)` — same dedup + undo + modify pattern
- `DrawLine(start, end)` — Bresenham + shape dispatch
- `BeginOperationAsync()` / `EndOperationAsync()` — bracket operations for undo grouping

### DrawApi

**File:** `src/TEdit/Scripting/Api/DrawApi.cs`

Scripting surface that wraps `WorldEditor` for user scripts. Provides a simplified API:

**Configuration methods:**
- `SetTile(type)`, `SetWall(type)`, `SetErase(bool)`, `SetPaintMode(string)`
- `SetBrush(width, height, shape)`, `SetRotation(degrees)`, `SetFlip(h, v)`
- `SetLiquid(type, amount)`, `SetWire(r, b, g, y)`
- `SetTileMask(mode, type)`, `SetWallMask(mode, type)`
- `SetBrushOutline(outline, enabled)`, `SetSpray(enabled, density, tickMs)`
- `SetTileColor(color)`, `SetWallColor(color)`, `SetBrickStyle(style)`
- `SetActuator(enabled, inactive)`, `SetTileCoating(echo, illuminant)`, `SetWallCoating(echo, illuminant)`
- `Reset()` — restore defaults

**Drawing methods:**
- `Pencil(x1, y1, x2, y2)` — 1px line
- `Brush(x1, y1, x2, y2)` — brush-width line using `WorldEditor.DrawLine`
- `Fill(x, y)` — scanline flood fill (reimplements `FillTool` without WPF dependencies)
- `Hammer(x1, y1, x2, y2)` — auto-slope tiles along a brush-width line

---

## 7. Rendering Coupling

These are the WPF-specific systems that the drawing pipeline currently depends on. They represent the primary barrier to extracting a `TEdit.Drawing` library.

### UpdateRenderPixel

**File:** `src/TEdit/ViewModel/WorldViewModel.Editor.cs:507`

Called after every `SetPixel` in the WPF path:

```csharp
public void UpdateRenderPixel(int x, int y)
{
    Color curBgColor = new Color(GetBackgroundColor(y).PackedValue);
    PixelMap.SetPixelColor(x, y, PixelMap.GetTileColor(tile, bgColor, showWalls, showTiles, ...));
    UpdateFilterOverlayPixel(x, y);
    BuffTileCache.UpdateTile(x, y, CurrentWorld.Tiles[x, y]);
}
```

This converts the modified tile into a pixel color and writes it to the `PixelMapManager` bitmap buffer. It depends on:
- `GetBackgroundColor(y)` — depth-based background color (Space/Sky/Earth/Rock/Hell zones)
- Layer visibility flags (`_showWalls`, `_showTiles`, `_showLiquid`, `_showRedWires`, etc.)
- `FilterOverlayMap` — darken/hide filter overlay
- `BuffTileCache` — secondary tile lookup cache for rendering

### BlendRules.ResetUVCache

**File:** `src/TEdit/Render/BlendRules.cs`

```csharp
public static void ResetUVCache(WorldViewModel _wvm, int tileStartX, int tileStartY, int regionWidth, int regionHeight)
```

A **renderer-only** function that recalculates the rendered UV frame coordinates for tiles in the affected region and their neighbors. This is necessary because Terraria tiles auto-connect visually — placing a dirt tile next to another dirt tile changes how neighboring tiles render their sprite frames. It does **not** modify the tile data model (`Tile.U`/`Tile.V`) — it operates on the render cache only.

**Coupling:** The `WorldViewModel` overload delegates to a `(World, TilePicker)` overload and triggers `UpdateRenderPixel` for affected neighbors. This is the "Heathtech" system referenced in comments throughout the codebase. Because it is purely a rendering concern, it stays entirely in the UI layer.

### PixelMap / PixelMapManager

The minimap/overview rendering system. `PixelMapManager` manages a chunked bitmap buffer (`WriteableBitmap` tiles) that represents the entire world as 1px-per-tile. `PixelMap.GetTileColor` is a static method that converts a `Tile` to an XNA `Color`.

### WriteableBitmap (Preview)

`ITool.PreviewTool()` returns a `WriteableBitmap` for the cursor overlay. `BrushToolBase.PreviewTool()` generates this bitmap by rasterizing the brush shape.

### Dependencies Summary

| WPF Type | Used By | Refactoring Impact |
|---|---|---|
| `WriteableBitmap` | `ITool`, `BaseTool`, preview rendering | Abstract to `IBrushPreview` or platform bitmap |
| `BitmapImage` | `ITool.Icon` | Move to resource ID / platform abstraction |
| `PixelMapManager` | `UpdateRenderPixel`, full-world render | Replace with `INotifyTileChanged` callback |
| `BlendRules` | Render-only UV cache recalculation | Stays in UI; drawing library fires `INotifyTileChanged` so UI can call it |
| `WorldViewModel` | Central coordinator, owns tools/undo/world | Split editor state from UI state |
| Win32 P/Invoke | `BaseTool.GetModifiers()` | Replace with cross-platform input abstraction |
| `SymbolRegular` | `ITool.SymbolIcon` | UI-only, stays in app layer |

---

## 8. Performance Optimizations

### Generation Counter (CheckTiles)

Instead of clearing a `bool[]` or `HashSet` of visited tiles per stroke (O(world_size)), the system uses an `int[]` with a generation counter. On `MouseDown`, `CheckTileGeneration++` effectively invalidates all previous entries without touching the array. Cost: O(1) per stroke start.

Spray mode preserves the generation across the entire stroke — spray ticks don't reset it, preventing duplicate undo saves and redundant `SetPixel` calls for already-painted tiles.

### Cached Brush Offsets

`BrushSettings` caches the computed shape points as `Vector2Int32[]` arrays relative to `(0,0)`. These are rebuilt lazily (only when `_cacheValid` is false). `StampOffsets()` translates cached offsets to absolute coordinates without recomputing geometry.

For outline mode, `_interiorOffsetSet` (a `HashSet<Vector2Int32>`) is pre-built on `BrushChanged` so that `FillHollowCached` can do O(1) interior lookups instead of allocating a new `HashSet` per call.

### Pre-Allocated Buffers

`BrushToolBase` pre-allocates reusable buffers to avoid per-frame GC pressure:

```csharp
private readonly List<Vector2Int32> _stampBuffer = new(160_000);
private readonly List<Vector2Int32> _interiorBuffer = new(160_000);
private readonly List<Vector2Int32> _lineBuffer = new(800);
private readonly HashSet<Vector2Int32> _sweepSet = new(10_000);
```

These are cleared and reused on each draw call. The 160K capacity accommodates a 400x400 brush (max size).

### FillSolid with Count Parameter

`FillSolid(IList<Vector2Int32> area, int count)` accepts a `count` parameter so that spray mode can use partial Fisher-Yates shuffle to select a random subset without creating a new list:

```csharp
int count = Math.Max(1, _stampBuffer.Count * SprayDensity / 100);
for (int i = 0; i < count; i++)
{
    int j = _sprayRandom.Next(i, _stampBuffer.Count);
    (_stampBuffer[i], _stampBuffer[j]) = (_stampBuffer[j], _stampBuffer[i]);
}
FillSolid(_stampBuffer, count);
```

### Spray Interpolation

Spray mode uses a `Stopwatch` instead of a `DispatcherTimer` for frame-rate-independent timing. `ProcessSprayMove` interpolates spray positions along the mouse movement line, distributing ticks evenly to prevent clustering.

### Simple Rect Fast Path

`IsSimpleRectShape()` returns true when the brush is a non-transformed `Square` (or has width/height <= 1). This path uses `Fill.FillRectangleCentered` and `Fill.FillRectangleVectorCenter` which are simpler than the general shape pipeline.

For consecutive line segments, `FillRectangleLine` computes the union rectangle between two points rather than stamping at each point individually.

### Line Shape Sweep

`FillLineShapeSweep` fills the area swept by a line-shaped brush moving between two positions as a single polygon, rather than stamping at each intermediate point. This eliminates gaps during fast mouse movement and reduces redundant pixel writes.

---

## 9. Refactoring Considerations

### Goal

Extract a `TEdit.Drawing` library containing all tile-editing logic, decoupled from WPF rendering. This library would be used by:
- The WPF app (`TEdit`)
- The Avalonia app (`TEdit5`)
- The scripting API (`DrawApi`)
- Unit tests

### What Moves to TEdit.Drawing

These components have **no inherent WPF dependency** and can move with minimal changes:

| Component | Current Location | Notes |
|---|---|---|
| `WorldEditor` | `TEdit.Editor` | Already decoupled. Becomes the primary API. |
| `TilePicker` | `TEdit.Editor` | Already decoupled. Pure data. |
| `BrushSettings` | `TEdit.Editor` | Already decoupled. Shape geometry + cache. |
| `BrushShape`, `PaintMode`, etc. | `TEdit.Editor` | Enums, already decoupled. |
| `IUndoManager` | `TEdit.Editor` | Interface, already decoupled. |
| `UndoBuffer` | `TEdit.Editor` | Binary serialization, no UI dependency. |
| `Fill`, `Shape` (geometry) | `TEdit.Geometry` | Pure math, no dependencies. |
| `ISelection` | `TEdit.Editor` | Interface, already decoupled. |
| `DrawApi` | `TEdit` (scripting) | Depends only on `WorldEditor` + interfaces. |

### What Stays in UI Layer

| Component | Reason |
|---|---|
| `BaseTool` / `BrushToolBase` / `BrushTool` | Mouse event handling, `WriteableBitmap` preview, WPF input system |
| `ITool` interface | References `WriteableBitmap`, `BitmapImage`, `SymbolRegular` |
| `UndoManager` | File I/O tied to `WorldViewModel.TempPath`, WPF `MessageBox` on error |
| `FillTool` | Uses `_wvm.UpdateRenderPixel` directly in the fill loop |
| `PixelMap` / `BlendRules` | WPF rendering pipeline |
| `WorldViewModel.SetPixel` | Thin wrapper: delegates to `WorldEditor.SetPixel` + `UpdateRenderPixel` |

### Key Refactoring Steps

1. **Formalize a batched render notification interface.** `WorldEditor` already accepts a `NotifyTileChanged` delegate, but the WPF path also needs `BlendRules.ResetUVCache` (render-only UV recalculation), `UpdateRenderPixel` (pixel map update), and minimap refresh. A richer `IRenderNotifier` interface is needed — and critically, it must support **batching** to avoid per-pixel notification overhead.

   **The problem today:** `BrushToolBase.FillSolid` calls `UpdateRenderPixel` and `BlendRules.ResetUVCache` inside the per-pixel loop. A 200x200 brush stamp generates 40,000+ individual render notifications per mouse move. This is wasteful — the UI only needs to know the dirty region once per draw operation.

   **Notification granularity:** The right level is **per draw call** — one `MouseMove` produces one `DrawLine` which produces one notification. This is *not* per-pixel (too many) and *not* per-stroke/MouseDown-to-MouseUp (too coarse — a stroke contains many move events, and delaying render updates until MouseUp would make drawing feel unresponsive).

   **Proposed design — draw-call-level notification:**

   ```
   IRenderNotifier
     ├─ InvalidateRegion(RectangleInt32)  // a draw call changed tiles in this region
     ├─ InvalidateAll()                   // full re-render (world load, crop, etc.)
     └─ ScheduleMinimapRefresh()          // debounced minimap update
   ```

   Each draw method fires one notification when it completes:
   - `FillSolid` computes the bounding rectangle of all painted pixels → one `InvalidateRegion`
   - `DrawLine` (one mouse-move segment) → one `InvalidateRegion` covering the line bounds
   - `FillRectangleLine` → one `InvalidateRegion` for the sweep rectangle
   - Flood fill → one `InvalidateRegion` for the bounding rect of all filled scanlines
   - Spray tick → one `InvalidateRegion` per tick

   The UI implementation receives the dirty rectangle and performs one batched render update — a single `BlendRules.ResetUVCache` call for the region (plus a 1-tile margin for neighbor auto-connect), one `PixelMap` update pass over the affected area, and a debounced minimap refresh.

   This eliminates per-pixel overhead entirely. The drawing library never calls `UpdateRenderPixel` or `BlendRules` — it says "I changed tiles in this rectangle" and the platform handles it.

   `BlendRules.ResetUVCache` stays entirely in the UI layer — it is called by the UI's `IRenderNotifier` implementation, not by the drawing library.

2. **Replace BrushToolBase duplication.** Currently `BrushToolBase` (WPF) and `WorldEditor` duplicate the drawing pipeline (`FillSolid`, `FillHollow`, `DrawLine`). After refactoring, `BrushToolBase` should delegate to `WorldEditor` methods. Each draw method (`FillSolid`, `DrawLine`, etc.) computes its bounding rectangle and fires a single `InvalidateRegion` — no per-pixel render calls.

3. **Abstract the preview system.** `PreviewTool()` returns `WriteableBitmap`. Define an `IBrushPreview` interface returning platform-agnostic point lists; each platform renders them natively.

4. **Extract FillTool logic.** `DrawApi.Fill` already reimplements the flood fill without WPF dependencies. Consolidate into a single implementation in the drawing library.

5. **Adapt UndoManager.** `UndoManager` (WPF) wraps `UndoBuffer` with file management and WPF error dialogs. The file management logic can move to the library; error presentation stays in UI. `UndoManagerWrapper` already adapts to `IUndoManager`.

### Coupling Inventory

For quick reference, here are all the WPF-coupled call sites in the drawing hot path:

| Call Site | WPF Dependency | Resolution |
|---|---|---|
| `BrushToolBase.FillSolid` → `_wvm.SetPixel` | `WorldViewModel` | Use `WorldEditor.SetPixel` instead |
| `BrushToolBase.FillSolid` → `BlendRules.ResetUVCache` (per pixel) | Render-only UV recalc | Remove; `FillSolid` fires one `InvalidateRegion(bounds)` at end |
| `WorldViewModel.SetPixel` → `UpdateRenderPixel` (per pixel) | `PixelMapManager`, XNA `Color` | Remove; UI re-renders the region in `InvalidateRegion` handler |
| `FillTool.LinearFloodFill` → `_wvm.UpdateRenderPixel` (per pixel) | Same | Remove; flood fill fires `InvalidateRegion(bounds)` when complete |
| `FillTool.LinearFloodFill` → `BlendRules.ResetUVCache` (per scanline) | Render-only UV recalc | Remove; handled by `InvalidateRegion` handler |
| `BaseTool.GetModifiers` → Win32 `GetKeyState` | P/Invoke | Platform input abstraction |
| `BrushToolBase.PreviewTool` → `WriteableBitmap` | WPF imaging | Platform bitmap abstraction |

### Already Decoupled

The `TEdit.Editor` assembly is already largely decoupled. `WorldEditor`, `TilePicker`, `BrushSettings`, `IUndoManager`, `ISelection`, `UndoBuffer`, and the geometry types in `TEdit.Geometry` have no WPF references. The primary work is:
1. Consolidating the duplicated drawing logic from `BrushToolBase` into `WorldEditor`
2. Defining a rich `IRenderNotifier` interface so the drawing library can signal pixel changes, UV cache invalidation, chunk updates, and minimap refresh — without depending on any rendering implementation
3. Creating platform abstractions for the tool preview and input systems
