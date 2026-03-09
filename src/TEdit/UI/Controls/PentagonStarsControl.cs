using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TEdit.UI.Controls;

/// <summary>
/// Displays 1–5 small stars arranged in a pentagonal pattern (like the points of a 5-pointed star),
/// fitting within a single icon-sized space. Lit stars are gold; unlit use foreground color.
/// </summary>
public class PentagonStarsControl : Control
{
    public static readonly DependencyProperty StarsProperty =
        DependencyProperty.Register(nameof(Stars), typeof(int), typeof(PentagonStarsControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public int Stars
    {
        get => (int)GetValue(StarsProperty);
        set => SetValue(StarsProperty, value);
    }

    // Star geometry: 5-pointed star path centered at origin, radius ~4
    private static readonly System.Windows.Media.Geometry StarGeometry = System.Windows.Media.Geometry.Parse(
        "M 0,-4 L 1.18,-1.24 4,-1.24 1.9,0.47 2.94,3.24 0,1.53 -2.94,3.24 -1.9,0.47 -4,-1.24 -1.18,-1.24 Z");

    private static readonly Brush LitBrush = new SolidColorBrush(Color.FromRgb(255, 200, 50)); // gold

    static PentagonStarsControl()
    {
        LitBrush.Freeze();
        StarGeometry.Freeze();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        double cx = ActualWidth / 2;
        double cy = ActualHeight / 2;
        double halfSize = Math.Min(cx, cy);
        int stars = Math.Clamp(Stars, 0, 5);

        // Star geometry extends ±4 units. Larger stars, moderately spread from center.
        double scale = halfSize / 11.0;
        double radius = halfSize * 0.45;

        var unlitBrush = GetValue(ForegroundProperty) as Brush
            ?? new SolidColorBrush(Color.FromArgb(80, 128, 128, 128));

        // 5 positions arranged as a regular pentagon, top-center first
        for (int i = 0; i < 5; i++)
        {
            double angle = -Math.PI / 2 + i * 2 * Math.PI / 5;
            double x = cx + radius * Math.Cos(angle);
            double y = cy + radius * Math.Sin(angle);

            dc.PushTransform(new TranslateTransform(x, y));
            dc.PushTransform(new ScaleTransform(scale, scale));
            dc.DrawGeometry(i < stars ? LitBrush : unlitBrush, null, StarGeometry);
            dc.Pop();
            dc.Pop();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double w = double.IsInfinity(availableSize.Width) ? 20 : availableSize.Width;
        double h = double.IsInfinity(availableSize.Height) ? 20 : availableSize.Height;
        double s = Math.Min(w, h);
        return new Size(s, s);
    }
}
