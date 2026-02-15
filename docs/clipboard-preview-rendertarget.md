# Clipboard Preview with RenderTarget2D (Pre-Rendered Bitmap)

**Status:** Deferred - Future Implementation

## Overview

Render clipboard buffers to an offscreen `RenderTarget2D` texture, then convert to `WriteableBitmap` for display. This creates static preview thumbnails that can be used in the sidebar clipboard list.

## Use Cases

- Sidebar clipboard list thumbnails
- Export preview images
- Clipboard item tooltips

## Benefits

- Textured preview visible in sidebar without world context
- Can be cached and reused
- Works for clipboard items loaded from files (no world context needed)

## Shared Infrastructure with Live Render

Both approaches share the same refactored drawing methods. **No delegates** for performance - use nullable parameters instead:

```csharp
// Core drawing methods accept optional overlay parameters
private void DrawTileTexturesCore(
    bool drawInverted,
    ClipboardBuffer clipboardOverlay = null,
    Vector2Int32? clipboardAnchor = null,
    Rectangle? customBounds = null,
    float? customZoom = null,
    Vector2? customScroll = null)

// Fast inline tile accessor (branch prediction makes null check free)
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

For **live render**: clipboard overlay with world fallback at edges
For **RenderTarget2D**: clipboard only with custom bounds/zoom/scroll

## Implementation

### Phase 1: Add RenderTarget2D Rendering to WorldRenderXna

**File:** `src/TEdit/View/WorldRenderXna.xaml.cs`

```csharp
public WriteableBitmap RenderTileDataToTexture(ITileData tileData, int maxSize = 256)
{
    // Calculate scale to fit within maxSize (16 pixels per tile)
    int pixelWidth = Math.Min(tileData.Size.X * 16, maxSize * 16);
    int pixelHeight = Math.Min(tileData.Size.Y * 16, maxSize * 16);
    float scale = Math.Min((float)maxSize / tileData.Size.X, (float)maxSize / tileData.Size.Y);
    scale = Math.Min(scale, 1.0f);

    var device = xnaViewport.GraphicsService.GraphicsDevice;
    var renderTarget = new RenderTarget2D(device, pixelWidth, pixelHeight);

    // Set render target
    device.SetRenderTarget(renderTarget);
    device.Clear(Color.Transparent);

    // Begin sprite batch
    _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                       SamplerState.PointClamp, DepthStencilState.Default,
                       RasterizerState.CullNone);

    // Draw using shared core methods with clipboard-only accessors
    var bounds = new Rectangle(0, 0, tileData.Size.X, tileData.Size.Y);
    DrawTileWallsCore(false, bounds, scale * 16, Vector2.Zero,
                      (x, y) => GetClipboardTile(tileData, x, y),
                      (x, y, dir) => GetClipboardNeighbor(tileData, x, y, dir));
    DrawTileTexturesCore(false, bounds, scale * 16, Vector2.Zero,
                         (x, y) => GetClipboardTile(tileData, x, y),
                         (x, y, dir) => GetClipboardNeighbor(tileData, x, y, dir));

    _spriteBatch.End();

    // Reset render target
    device.SetRenderTarget(null);

    // Convert to WriteableBitmap
    var bitmap = RenderTargetToWriteableBitmap(renderTarget);
    renderTarget.Dispose();

    return bitmap;
}

private Tile GetClipboardTile(ITileData data, int x, int y)
{
    if (x >= 0 && x < data.Size.X && y >= 0 && y < data.Size.Y)
        return data.Tiles[x, y];
    return null;
}

private Tile GetClipboardNeighbor(ITileData data, int x, int y, int direction)
{
    // Returns null for out-of-bounds (no world context)
    return GetClipboardTile(data, x, y);
}
```

### Phase 2: Update ClipboardBufferRenderer

**File:** `src/TEdit/Editor/Clipboard/ClipboardBufferRenderer.cs`

```csharp
public static WriteableBitmap RenderBufferTextured(
    ClipboardBuffer buffer,
    Func<ITileData, int, WriteableBitmap> texturedRenderer,
    out double renderScale,
    int maxTexturedSize = 256)
{
    // Use textured renderer if available and buffer small enough
    if (texturedRenderer != null &&
        buffer.Size.X <= maxTexturedSize &&
        buffer.Size.Y <= maxTexturedSize)
    {
        renderScale = 1.0;
        return texturedRenderer(buffer, maxTexturedSize);
    }

    // Fall back to pixel rendering
    return RenderBuffer(buffer, out renderScale);
}
```

### Phase 3: Update ClipboardBufferPreview

**File:** `src/TEdit/Editor/Clipboard/ClipboardBufferPreview.cs`

```csharp
public class ClipboardBufferPreview
{
    public ClipboardBufferPreview(
        ClipboardBuffer buffer,
        Func<ITileData, int, WriteableBitmap>? texturedRenderer = null)
    {
        Buffer = buffer;

        if (texturedRenderer != null && buffer.Size.X <= 256 && buffer.Size.Y <= 256)
        {
            Preview = ClipboardBufferRenderer.RenderBufferTextured(
                buffer, texturedRenderer, out var scale);
            PreviewScale = scale;
            IsTexturedPreview = true;
        }
        else
        {
            Preview = ClipboardBufferRenderer.RenderBuffer(buffer, out var scale);
            PreviewScale = scale;
            IsTexturedPreview = false;
        }
    }

    public ClipboardBuffer Buffer { get; }
    public WriteableBitmap Preview { get; }
    public double PreviewScale { get; }
    public bool IsTexturedPreview { get; }
}
```

### Phase 4: Wire Up Renderer

**File:** `src/TEdit/Editor/Clipboard/ClipboardManager.cs`

```csharp
public Func<ITileData, int, WriteableBitmap>? TexturedRenderer { get; set; }
```

**File:** `src/TEdit/MainWindow.xaml.cs` or `src/TEdit/ViewModel/WorldViewModel.cs`

After graphics initialized:

```csharp
_clipboard.TexturedRenderer = (data, maxSize) =>
    worldRenderXna.RenderTileDataToTexture(data, maxSize);
```

## Edge Cases

| Case | Handling |
| ---- | -------- |
| Buffer >= 256x256 | Fall back to pixel rendering |
| Textures not loaded | Fall back to pixel rendering |
| GraphicsDevice not ready | Fall back to pixel rendering |
| Buffer edges | Return null for neighbors (no seamless blending) |
| Biome-dependent tiles | Use default style (no world context) |

## Files to Modify

1. `src/TEdit/View/WorldRenderXna.xaml.cs` - Add `RenderTileDataToTexture()`, shared drawing core
2. `src/TEdit/Editor/Clipboard/ClipboardBufferRenderer.cs` - Add textured rendering path
3. `src/TEdit/Editor/Clipboard/ClipboardBufferPreview.cs` - Accept renderer, add `IsTexturedPreview`
4. `src/TEdit/Editor/Clipboard/ClipboardManager.cs` - Store and pass renderer
5. `src/TEdit/MainWindow.xaml.cs` - Wire up renderer after graphics init

## Coexistence with Live Render

The core drawing methods work with both approaches using nullable parameters (no delegates for performance):

```csharp
// Shared core method signature - no delegates
private void DrawTileTexturesCore(
    bool drawInverted,
    ClipboardBuffer clipboardOverlay = null,
    Vector2Int32? clipboardAnchor = null,
    Rectangle? customBounds = null,
    float? customZoom = null,
    Vector2? customScroll = null)
{
    // Use GetTileAt() inline helper for tile access
    // Null check is essentially free with branch prediction
}
```

**Live Render** calls with: clipboard + anchor (world fallback at edges)
**RenderTarget2D** calls with: clipboard + custom bounds/zoom/scroll

This design allows implementing live render first, then adding RenderTarget2D later with minimal additional changes.
