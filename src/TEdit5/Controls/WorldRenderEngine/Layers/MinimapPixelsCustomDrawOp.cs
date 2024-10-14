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

public class MinimapPixelsCustomDrawOp : ICustomDrawOperation
{
    private readonly World? World;

    public MinimapPixelsCustomDrawOp(
        Rect bounds,
        World? world)
    {
        Bounds = bounds;
        World = world;
    }

    public void Dispose() { }
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

    public TEditColor GetBackgroundColor(int y)
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

    private SKBitmap CreateBitmapTile()
    {
        var world = World;

        int worldWidth = world.TilesWide;
        int worldHeight = world.TilesHigh;

        int resolution = (int)Math.Ceiling(Math.Max(worldHeight / Bounds.Height, worldWidth / Bounds.Width));

        int bmpWidth = (int)(world.TilesWide / resolution);
        int bmpHeight = (int)(world.TilesHigh / resolution);

        var blocks = world.Tiles;

        var bmp = new SKBitmap(bmpWidth, bmpHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

        for (int x = 0; x * resolution < worldWidth && x < bmpWidth; x++)
        {
            for (int y = 0; y * resolution < worldHeight && y < bmpHeight; y++)
            {
                int worldX = x * resolution;
                int worldY = y * resolution;

                var block = blocks[worldX, worldY];
                //if (block.IsActive)
                {
                    var bgColor = GetBackgroundColor(worldY);
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

        using var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            IsDither = false,
            Color = SKColors.White,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.SrcOver,
        };

        var tile = CreateBitmapTile();

        if (tile != null)
        {
            double xOffset = (Bounds.Width - tile.Width) / 2;
            double yOffset = (Bounds.Height - tile.Height) / 2;
            double width = Math.Min(tile.Width, Bounds.Width);
            double height = Math.Min(tile.Height, Bounds.Height);


            canvas.DrawBitmap(
                tile,
                new SKRect(0, 0, tile.Width, tile.Height),
                new SKRect((int)xOffset, (int)yOffset, (int)width, (int)height)
                );
        }

        canvas.Restore();
    }
}
