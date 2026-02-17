# Live In-Place Clipboard Preview Rendering

**Status:** Implementing Now

## Overview

Render clipboard tiles directly during the world render pass, rather than pre-rendering to a bitmap. This reuses 100% of existing rendering logic and gets biome/neighbor blending for free.

## Benefits

- Full tile textures with proper UV blending
- Biome-dependent rendering (tree styles based on target location)
- Seamless edge blending with existing world tiles
- Max preview size of 64x64 tiles
- No additional texture memory required

## How It Works

1. When paste tool is active and hovering over the world
2. After normal `DrawTileTextures()`, call `DrawClipboardPreviewTiles()`
3. For each tile in clipboard buffer (cx, cy):
   - Draw at world position `(anchorX + cx, anchorY + cy)`
   - Use existing tile rendering logic
4. For neighbor lookups: use clipboard tile if within buffer, else use world tile
5. Biome detection uses the paste target location naturally

## Implementation

### Phase 1: Add Clipboard Tile Accessor

**File:** `src/TEdit/View/WorldRenderXna.xaml.cs`

Add helper to get tile from clipboard overlay or world:

```csharp
private Tile GetTileForRendering(int worldX, int worldY, ClipboardBuffer clipboard, Vector2Int32 anchor)
{
    // Check if position is within clipboard bounds
    int cx = worldX - anchor.X;
    int cy = worldY - anchor.Y;

    if (cx >= 0 && cx < clipboard.Size.X && cy >= 0 && cy < clipboard.Size.Y)
    {
        var clipTile = clipboard.Tiles[cx, cy];
        if (!clipTile.IsEmpty || _wvm.Clipboard.PasteEmpty)
            return clipTile;
    }

    // Fall back to world tile
    if (worldX >= 0 && worldX < _wvm.CurrentWorld.TilesWide &&
        worldY >= 0 && worldY < _wvm.CurrentWorld.TilesHigh)
        return _wvm.CurrentWorld.Tiles[worldX, worldY];

    return null;
}
```

### Phase 2: Add Clipboard Preview Rendering Methods

**File:** `src/TEdit/View/WorldRenderXna.xaml.cs`

Add methods that render clipboard tiles using existing logic:

```csharp
private void DrawClipboardPreviewWalls()
private void DrawClipboardPreviewTiles()
private void DrawClipboardPreviewLiquids()
private void DrawClipboardPreviewWires()
```

Each method:

1. Gets current clipboard buffer and paste anchor (mouse position)
2. Iterates clipboard bounds (limited to 64x64 max)
3. For each tile, calls existing rendering logic with position offset
4. Uses `GetTileForRendering()` for neighbor lookups to blend with world

### Phase 3: Modify Existing Drawing Methods

**File:** `src/TEdit/View/WorldRenderXna.xaml.cs`

Refactor `DrawTileWalls()`, `DrawTileTextures()`, etc. to accept optional clipboard overlay parameters. **Avoid delegates** in tight render loops for performance:

```csharp
// Original signature preserved for normal rendering
public void DrawTileTextures(bool drawInverted = false)
{
    DrawTileTexturesCore(drawInverted, null, null);
}

// New core method with optional clipboard overlay (nullable params, not delegates)
private void DrawTileTexturesCore(
    bool drawInverted,
    ClipboardBuffer clipboardOverlay = null,
    Vector2Int32? clipboardAnchor = null)
{
    // At each tile position, use fast inline helper
    // Null check is essentially free after branch prediction
    var tile = GetTileAt(x, y, clipboardOverlay, clipboardAnchor);
}

// Fast inline tile accessor
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private Tile GetTileAt(int x, int y, ClipboardBuffer clip, Vector2Int32? anchor)
{
    if (clip != null)
    {
        int cx = x - anchor.Value.X;
        int cy = y - anchor.Value.Y;
        if (cx >= 0 && cx < clip.Size.X && cy >= 0 && cy < clip.Size.Y)
            return clip.Tiles[cx, cy];
    }
    return _wvm.CurrentWorld.Tiles[x, y];
}
```

**Why not delegates?** In a tight render loop iterating thousands of tiles, delegate invocation overhead (indirection, potential closures) adds up. Simple null checks with branch prediction are essentially free.

### Phase 4: Integrate into Render Loop

**File:** `src/TEdit/View/WorldRenderXna.xaml.cs`

After each normal draw call, add clipboard preview pass when paste tool active:

```csharp
// Around line 2366
if (_wvm.ShowTiles)
{
    _spriteBatch.Begin(...);
    DrawTileTextures();

    // Clipboard preview overlay
    if (IsPasteToolActive && _wvm.Clipboard.Buffer != null)
    {
        var anchor = GetClipboardAnchor();
        DrawClipboardPreviewTiles(anchor);
    }

    _spriteBatch.End();
}
```

### Phase 5: Add 64x64 Size Check

**File:** `src/TEdit/Editor/Clipboard/ClipboardManager.cs` or `src/TEdit/View/WorldRenderXna.xaml.cs`

Only use live preview for buffers <= 64x64:

```csharp
private bool ShouldUseLiveClipboardPreview()
{
    var buffer = _wvm.Clipboard.Buffer?.Buffer;
    if (buffer == null) return false;
    return buffer.Size.X <= 64 && buffer.Size.Y <= 64;
}
```

For larger buffers, fall back to current pixel-based preview.

### Phase 6: Update PasteTool Preview

**File:** `src/TEdit/Editor/Tools/PasteTool.cs`

When live preview is active, `PreviewTool()` returns null to skip the old bitmap preview:

```csharp
public override WriteableBitmap PreviewTool()
{
    // Live preview is rendered in WorldRenderXna, not here
    if (UseLivePreview && _wvm.Clipboard.Buffer != null)
        return null;  // Signal: don't draw bitmap preview

    // Fall back to pixel preview for large buffers
    return _wvm.Clipboard.Buffer?.Preview;
}
```

## Edge Cases

| Case | Handling |
| ---- | -------- |
| Buffer > 64x64 | Fall back to pixel-based preview bitmap |
| Cursor outside world bounds | Clip rendering to valid world area |
| Clipboard edge blending | Use world tile for out-of-buffer neighbors |
| Paste over existing tiles | Clipboard tiles render on top (later in draw order) |
| Empty clipboard tiles | Skip or show based on `PasteEmpty` setting |
| Performance | Limited to 64x64 = 4096 tiles max per frame |

## Files to Modify

1. `src/TEdit/View/WorldRenderXna.xaml.cs` - Add `DrawClipboardPreview*()` methods, refactor drawing core
2. `src/TEdit/Editor/Tools/PasteTool.cs` - Return null from `PreviewTool()` when live preview active

## Testing

1. Select paste tool with small clipboard (< 64x64)
2. Hover over different biomes - verify tree styles update dynamically
3. Hover at world edge - verify proper clipping
4. Paste near existing tiles - verify UV blending at edges
5. Test with walls, tiles, liquids, wires
6. Select paste tool with large clipboard (> 64x64) - verify pixel preview fallback
7. Test flip/rotate - verify preview updates correctly
