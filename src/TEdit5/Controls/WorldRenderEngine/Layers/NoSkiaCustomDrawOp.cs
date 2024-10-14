using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;

namespace TEdit5.Controls.WorldRenderEngine.Layers;

public class NoSkiaCustomDrawOp : ICustomDrawOperation
{
    private readonly IImmutableGlyphRunReference _noSkia;

    public NoSkiaCustomDrawOp(Rect bounds, GlyphRun noSkia)
    {
        Bounds = bounds;
        _noSkia = noSkia.TryCreateImmutableGlyphRunReference();
    }

    public void Dispose()
    {
        _noSkia?.Dispose();
    }

    public Rect Bounds { get; }
    public bool HitTest(Point p) => false;
    public bool Equals(ICustomDrawOperation other) => false;

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null)
        {
            context.DrawGlyphRun(Brushes.Black, _noSkia);
        }
        else
        {
            // do nothing
        }
    }
}
