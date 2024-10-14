using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;

namespace TEdit5.Controls.WorldRenderEngine.Layers;

public class BackgroundGridCustomDrawOp : ICustomDrawOperation
{
    private static SKPaint _rowPaint;
    private static SKPaint _altPaint;
    private readonly int _cellSize = 16;

    public BackgroundGridCustomDrawOp(Rect bounds, int cellSize = 16)
    {
        Bounds = bounds;
        _cellSize = cellSize;
        LoadContent();
    }

    public void LoadContent()
    {
        _rowPaint = new SKPaint
        {
            Color = new SKColor(16, 16, 16),
            Style = SKPaintStyle.Fill
        };

        _altPaint = new SKPaint
        {
            Color = new SKColor(48, 48, 48),
            Style = SKPaintStyle.Fill
        };
    }

    public void Dispose()
    {
        try
        {
            _rowPaint?.Dispose();
            _altPaint?.Dispose();
        }
        catch (Exception ex)
        {
            // Debug.WriteLine(ex);
        }
    }

    public Rect Bounds { get; }
    public bool HitTest(Point p) => false;
    public bool Equals(ICustomDrawOperation other) => false;

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null) { return; }

        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;
        DrawBackgroundGrid(canvas);
    }

    private void DrawBackgroundGrid(SKCanvas canvas)
    {
        canvas.Save();

        int currentColor = 0;

        for (int y = 0; y < Bounds.Height; y += _cellSize)
        {
            var firstRowColor = currentColor;

            for (int x = 0; x < Bounds.Width; x += _cellSize)
            {
                canvas.DrawRect(x, y, _cellSize, _cellSize, currentColor == 0 ? _rowPaint : _altPaint);
                currentColor = currentColor == 0 ? 1 : 0;
            }

            if (firstRowColor == currentColor)
            {
                currentColor = currentColor == 0 ? 1 : 0;
            }
        }

        canvas.Restore();
    }
}
