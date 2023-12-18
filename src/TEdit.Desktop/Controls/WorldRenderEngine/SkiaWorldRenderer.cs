using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.Utilities;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TEdit.Common;
using TEdit.Terraria;

namespace TEdit.Desktop.Controls.WorldRenderEngine;

public class SkiaWorldRenderer : Control, IDisposable
{
    private readonly GlyphRun _noSkia;
    public SkiaWorldRenderer()
    {
        ClipToBounds = true;
        var text = "Current rendering API is not Skia";
        var glyphs = text.Select(ch => Typeface.Default.GlyphTypeface.GetGlyph(ch)).ToArray();
        _noSkia = new GlyphRun(Typeface.Default.GlyphTypeface, 12, text.AsMemory(), glyphs);
    }


    /// <summary>
    /// Defines the <see cref="World"/> property.
    /// </summary>
    public static readonly StyledProperty<World?> WorldProperty =
        AvaloniaProperty.Register<SkiaWorldRenderer, World?>(nameof(World));

    /// <summary>
    /// Comment
    /// </summary>
    public World? World
    {
        get { return GetValue(WorldProperty); }
        set { SetValue(WorldProperty, value); }
    }

    public void Dispose()
    {

    }

    public override void Render(DrawingContext context)
    {
        context.Custom(new NoSkiaCustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), _noSkia));
        context.Custom(new BackgroundGridCustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height)));
        context.Custom(new WorldPixelsCustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), World));

        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }
}

public static class SkiaTEditExtensions
{
    public static SKColor ToSKColor(this TEditColor color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }
}

public class WorldPixelsCustomDrawOp : ICustomDrawOperation
{
    private World? World;

    private static Dictionary<TEditColor, SKPaint> _colorPaint = new();

    private float _tileScale = 16;

    public WorldPixelsCustomDrawOp(Rect bounds, World? world)
    {
        Bounds = bounds;
        World = world;

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
        DrawBackgroundGrid(canvas);
    }

    private SKBitmap CreateViewportBitmap()
    {
        var world = World;
        var tiles = world.Tiles;
        int tileCount = tiles.Length;

        int tileWidth = world.TilesWide;
        int tileHeight = world.TilesHigh;

        int tileOffsetX = 500;
        int tileOffsetY = 1000;

        // +1 here to fill last pixel on screen
        var bmp = new SKBitmap(
            (int)(Bounds.Width / _tileScale) + 1,
            (int)(Bounds.Height / _tileScale) + 1);

        for (int x = 0; x + tileOffsetX < tileWidth && x < bmp.Width; x++)
        {
            for (int y = 0; y + tileOffsetY < tileHeight && y < bmp.Height; y++)
            {
                var tileX = x + tileOffsetX;
                var tileY = y + tileOffsetY;

                var tile = tiles[tileX, tileY];
                if (tile.IsActive)
                {
                    bmp.SetPixel(x, y, PixelMap.GetTileColor(tile, TEditColor.White).ToSKColor());
                }
            }
        }

        return bmp;
    }

    private SKPaint GetPaintColor(TEditColor color)
    {
        if (!_colorPaint.TryGetValue(color, out var paint))
        {
            paint = new SKPaint
            {
                Color = new SKColor(color.R, color.G, color.B, color.A),
                Style = SKPaintStyle.Fill
            };

            _colorPaint[color] = paint;
        }

        return paint;
    }

    private void DrawBackgroundGrid(SKCanvas canvas)
    {
        canvas.Save();

        var pixels = CreateViewportBitmap();

        // fix the zoom to _tileScale
        // this removes scaling issues when resising
        var canvasWidth = ((int)(Bounds.Width / _tileScale) * _tileScale) + _tileScale;
        var canvasHeight = ((int)(Bounds.Height / _tileScale) * _tileScale) + _tileScale;

        // draw bitmap to canvas
        canvas.DrawBitmap(pixels,
            new SKRect(0, 0, pixels.Width, pixels.Height),
            new SKRect(0, 0, canvasWidth, canvasHeight)
            );

        canvas.Restore();
    }
}
