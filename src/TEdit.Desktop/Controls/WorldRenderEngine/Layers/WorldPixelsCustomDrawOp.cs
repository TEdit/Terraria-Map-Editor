using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using TEdit.Common;
using TEdit.Configuration;
using TEdit.Terraria;

namespace TEdit.Desktop.Controls.WorldRenderEngine.Layers;

public class WorldPixelsCustomDrawOp : ICustomDrawOperation
{
    private readonly Vector _offset;
    private readonly World? World;
    private readonly IRasterTileCache _pixelTiles;
    private readonly double _zoom = 1f;

    public WorldPixelsCustomDrawOp(
        Rect bounds,
        Vector offset,
        World? world,
        IRasterTileCache _pixelTiles,
        double tileScale = 1f
        )
    {
        Bounds = bounds;
        _offset = offset;
        World = world;
        this._pixelTiles = _pixelTiles;
        _zoom = tileScale;
        LoadContent();
    }

    public void LoadContent()
    {

    }

    public void Dispose()
    {
        // No-op
    }

    public Rect Bounds { get; }
    public bool HitTest(Point p) => false;
    public bool Equals(ICustomDrawOperation other) => false;

    public void Render(ImmediateDrawingContext context)
    {
        if (World == null) { return; }

        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null) { return; }

        using var lease = leaseFeature.Lease();

        var canvas = lease.SkCanvas;
        DrawWorldPixelTiles(canvas);
    }

    private RasterTile GetTile(int x, int y)
    {
        var tilePos = new SKPointI(x, y);
        var blockPos = new SKPointI(x * _pixelTiles.TileSize, y * _pixelTiles.TileSize);

        if (blockPos.X > World.TilesWide || blockPos.Y > World.TilesHigh)
            return null;

        _pixelTiles.Tiles.TryGetValue(new SKPointI(x, y), out var tile);

        if (tile == null || tile.IsDirty)
        {
            tile = new RasterTile
            {
                Bitmap = CreateBitmapTile(x, y),
                TilePosition = tilePos,
                BlockPosition = blockPos,
                IsDirty = false
            };

            _pixelTiles.AddOrUpdate(tile);

        }

        return tile;
    }

    private TEditColor GetBackgroundColor(int y)
    {
        if (World == null) { return TEditColor.White; }
        if (y < 80)
            return WorldConfiguration.GlobalColors["Space"];
        else if (y > World.TilesHigh - 192)
            return WorldConfiguration.GlobalColors["Hell"];
        else if (y > World.RockLevel)
            return WorldConfiguration.GlobalColors["Rock"];
        else if (y > World.GroundLevel)
            return WorldConfiguration.GlobalColors["Earth"];
        else
            return WorldConfiguration.GlobalColors["Sky"];
    }

    private SKBitmap CreateBitmapTile(int xTile, int yTile)
    {
        var bmp = new SKBitmap(_pixelTiles.TileSize, _pixelTiles.TileSize, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

        var world = World;
        int worldWidth = world.TilesWide;
        int worldHeight = world.TilesHigh;

        var blocks = world.Tiles;

        int blockX = xTile * _pixelTiles.TileSize;
        int blockY = yTile * _pixelTiles.TileSize;

        for (int x = 0; x + blockX < worldWidth && x < _pixelTiles.TileSize; x++)
        {
            for (int y = 0; y + blockY < worldHeight && y < _pixelTiles.TileSize; y++)
            {
                var currentBlockX = x + blockX;
                var currentBlockY = y + blockY;

                var block = blocks[currentBlockX, currentBlockY];
                //if (block.IsActive)
                {
                    var bgColor = GetBackgroundColor(currentBlockY);
                    var tileColor = PixelMap.GetTileColor(block, bgColor).ToSKColor().WithAlpha(255);

                    bmp.SetPixel(
                        x,
                        y,
                        tileColor);
                }
            }
        }

        bmp.SetImmutable();

        return bmp;
    }

    private void DrawWorldPixelTiles(SKCanvas canvas)
    {
        if (World == null) { return; }

        canvas.Save();

        int numTilesX = (int)Math.Ceiling(Bounds.Width / _pixelTiles.TileSize / _zoom);
        int numTilesY = (int)Math.Ceiling(Bounds.Height / _pixelTiles.TileSize / _zoom);

        using var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            IsDither = false,
            Color = SKColors.White,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.SrcOver,
        };

        var xTileOffset = (int)(_offset.X / _pixelTiles.TileSize / _zoom);
        var yTileOffset = (int)(_offset.Y / _pixelTiles.TileSize / _zoom);

        for (int x = xTileOffset; x <= numTilesX + xTileOffset; x++)
        {
            for (int y = yTileOffset; y <= numTilesY + yTileOffset; y++)
            {
                var tileCanvasX = x * _zoom * _pixelTiles.TileSize - (int)_offset.X + (int)Bounds.X;
                var tileCanvasY = y * _zoom * _pixelTiles.TileSize - (int)_offset.Y + (int)Bounds.Y;
                var tileCanvasWidth = _pixelTiles.TileSize * _zoom;
                var tileCanvasHeight = _pixelTiles.TileSize * _zoom;

                var tile = GetTile(x, y);

                if (tile?.Bitmap != null)
                {
                    canvas.DrawBitmap(
                        tile.Bitmap,
                        new SKRect(0, 0, tile.Bitmap.Width, tile.Bitmap.Height),
                        new SKRect(
                            (float)tileCanvasX,
                            (float)tileCanvasY,
                            (float)(tileCanvasX + tileCanvasWidth),
                            (float)(tileCanvasY + tileCanvasHeight))
                        //paint
                        );
                }
            }
        }

        canvas.Restore();
    }
}
