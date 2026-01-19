using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using TEdit.Common;
using TEdit.Configuration;
using TEdit.Terraria;

namespace TEdit5.Controls.WorldRenderEngine.Layers;


public class RasterTileRenderer
{
    private static TEditColor GetBackgroundColor(World _world, int y)
    {
        if (_world == null) { return TEditColor.White; }
        if (y < 80)
            return WorldConfiguration.GlobalColors["Space"];
        else if (y > _world.TilesHigh - 192)
            return WorldConfiguration.GlobalColors["Hell"];
        else if (y > _world.RockLevel)
            return WorldConfiguration.GlobalColors["Rock"];
        else if (y > _world.GroundLevel)
            return WorldConfiguration.GlobalColors["Earth"];
        else
            return WorldConfiguration.GlobalColors["Sky"];
    }

    public static SKColor GetBlockColor(World _world, int currentBlockX, int currentBlockY)
    {
        var block = _world.Tiles[currentBlockX, currentBlockY];
        var bgColor = GetBackgroundColor(_world, currentBlockY);
        return PixelMap.GetTileColor(block, bgColor).ToSKColor().WithAlpha(255);
    }

    public static SKBitmap CreateBitmapTile(World _world, int xTile, int yTile, int tileSizeX, int tileSizeY)
    {
        ArgumentNullException.ThrowIfNull(_world);

        var bmp = new SKBitmap(tileSizeX, tileSizeY, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

        var world = _world;
        int worldWidth = world.TilesWide;
        int worldHeight = world.TilesHigh;

        var blocks = world.Tiles;

        int blockX = xTile * tileSizeX;
        int blockY = yTile * tileSizeY;

        for (int x = 0; x + blockX < worldWidth && x < tileSizeX; x++)
        {
            for (int y = 0; y + blockY < worldHeight && y < tileSizeY; y++)
            {
                var currentBlockX = x + blockX;
                var currentBlockY = y + blockY;

                var block = blocks[currentBlockX, currentBlockY];
                //if (block.IsActive)
                {
                    var bgColor = GetBackgroundColor(_world, currentBlockY);
                    var tileColor = PixelMap.GetTileColor(block, bgColor).ToSKColor().WithAlpha(255);

                    bmp.SetPixel(
                        x,
                        y,
                        tileColor);
                }
            }
        }

        //bmp.SetImmutable();


        return bmp;
    }

    public static void UpdateBitmapTileRegion(SKBitmap bmp, World _world, int xTile, int yTile, int tileSizeX, int tileSizeY, SKRectI dirtyRegion)
    {
        ArgumentNullException.ThrowIfNull(_world);
        ArgumentNullException.ThrowIfNull(bmp);

        int worldWidth = _world.TilesWide;
        int worldHeight = _world.TilesHigh;
        var blocks = _world.Tiles;

        int blockX = xTile * tileSizeX;
        int blockY = yTile * tileSizeY;

        int startX = Math.Max(0, dirtyRegion.Left);
        int endX = Math.Min(tileSizeX, dirtyRegion.Right);
        int startY = Math.Max(0, dirtyRegion.Top);
        int endY = Math.Min(tileSizeY, dirtyRegion.Bottom);

        for (int x = startX; x < endX && x + blockX < worldWidth; x++)
        {
            for (int y = startY; y < endY && y + blockY < worldHeight; y++)
            {
                var currentBlockX = x + blockX;
                var currentBlockY = y + blockY;

                var block = blocks[currentBlockX, currentBlockY];
                var bgColor = GetBackgroundColor(_world, currentBlockY);
                var tileColor = PixelMap.GetTileColor(block, bgColor).ToSKColor().WithAlpha(255);

                bmp.SetPixel(x, y, tileColor);
            }
        }
    }
}

public class WorldPixelsCustomDrawOp : ICustomDrawOperation
{
    private readonly Vector _offset;
    private readonly World? _world;
    private readonly IRasterTileCache _tileCache;
    private readonly double _zoom = 1f;

    public WorldPixelsCustomDrawOp(
        Rect bounds,
        Vector offset,
        World? world,
        IRasterTileCache tileCache,
        double tileScale = 1f
        )
    {
        Bounds = bounds;
        _offset = offset;
        _world = world;
        this._tileCache = tileCache;
        _zoom = tileScale;
        LoadContent();
    }

    public void LoadContent()
    {

    }

    public void Dispose()
    {
        _redHighlight?.Dispose();
        _greenBorder?.Dispose();
        _bitmapPaint?.Dispose();
    }

    public Rect Bounds { get; }
    public bool HitTest(Point p) => false;
    public bool Equals(ICustomDrawOperation other) => false;

    public void Render(ImmediateDrawingContext context)
    {
        if (_world == null) { return; }

        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null) { return; }

        using var lease = leaseFeature.Lease();

        var canvas = lease.SkCanvas;
        DrawWorldPixelTiles(canvas);
    }

    private RasterTile? GetTile(int x, int y)
    {
        var tile = _tileCache.GetTile(x, y);
        return tile;
    }

    SKPaint _greenBorder = new SKPaint
    {
        Color = SKColors.Green,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1f,
    };

    SKPaint _redHighlight = new SKPaint
    {
        Color = SKColors.Red.WithAlpha(128),
        Style = SKPaintStyle.Fill,
    };

    SKPaint _bitmapPaint = new SKPaint
    {
        IsAntialias = false,
        FilterQuality = SKFilterQuality.None,
        IsDither = false,
        Color = SKColors.White,
        Style = SKPaintStyle.Fill,
        BlendMode = SKBlendMode.SrcOver,
    };

    private void DrawWorldPixelTiles(SKCanvas canvas)
    {
        if (_world == null) { return; }

        canvas.Save();

        int tileSizeX = _tileCache.TileSizeX;
        int tileSizeY = _tileCache.TileSizeY;

        int numTilesX = (int)Math.Ceiling((double)Bounds.Width / tileSizeX / _zoom) + 1;
        int numTilesY = (int)Math.Ceiling((double)Bounds.Height / tileSizeY / _zoom) + 1;

        var xTileOffset = (int)Math.Floor((double)_offset.X / tileSizeX / _zoom);
        var yTileOffset = (int)Math.Floor((double)_offset.Y / tileSizeY / _zoom);

        var maxTileX = Math.Min(_tileCache.TilesX, numTilesX + xTileOffset);
        var maxTileY = Math.Min(_tileCache.TilesY, numTilesY + yTileOffset);

        int dirtyTilesProcessed = 0;
        const int maxDirtyTilesPerFrame = 10;

        for (int x = xTileOffset; x < maxTileX; x++)
        {
            for (int y = yTileOffset; y < maxTileY; y++)
            {
                var tileCanvasX = x * _zoom * tileSizeX - (int)_offset.X + (int)Bounds.X;
                var tileCanvasY = y * _zoom * tileSizeY - (int)_offset.Y + (int)Bounds.Y;
                var tileCanvasWidth = tileSizeX * _zoom;
                var tileCanvasHeight = tileSizeY * _zoom;

                var tile = GetTile(x, y);

                var canvasRect = new SKRect(
                            (float)tileCanvasX,
                            (float)tileCanvasY,
                            (float)(tileCanvasX + tileCanvasWidth),
                            (float)(tileCanvasY + tileCanvasHeight));

                // Update dirty tiles
                if (tile != null && tile.IsDirty && dirtyTilesProcessed < maxDirtyTilesPerFrame)
                {
                    dirtyTilesProcessed++;

                    if (tile.Bitmap == null)
                    {
                        // First time creation - full tile
                        tile.Bitmap = RasterTileRenderer.CreateBitmapTile(_world, x, y, tileSizeX, tileSizeY);
                    }
                    else if (tile.DirtyRegion.HasValue)
                    {
                        // Partial update - only dirty region
                        RasterTileRenderer.UpdateBitmapTileRegion(
                            tile.Bitmap, 
                            _world, 
                            x, 
                            y, 
                            tileSizeX, 
                            tileSizeY, 
                            tile.DirtyRegion.Value);
                    }
                    else
                    {
                        // Full tile update (fallback)
                        tile.Bitmap?.Dispose();
                        tile.Bitmap = RasterTileRenderer.CreateBitmapTile(_world, x, y, tileSizeX, tileSizeY);
                    }

                    tile.ClearDirtyRegion();
                }

                // Draw tile bitmap
                if (tile?.Bitmap != null)
                {
                    canvas.DrawBitmap(
                        tile.Bitmap,
                        new SKRect(0, 0, tile.Bitmap.Width, tile.Bitmap.Height),
                        canvasRect,
                        _bitmapPaint);
                }

                canvas.DrawRect(canvasRect, _greenBorder);
            }
        }

        canvas.Restore();
    }
}

