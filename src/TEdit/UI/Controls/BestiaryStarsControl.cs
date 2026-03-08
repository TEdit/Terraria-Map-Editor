using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TEdit.UI.Controls;

/// <summary>
/// Displays a right-handed interlaced pentagram where each of the 5 points
/// lights up gold for the star rating (1–5). Unlit points use the current foreground color.
/// Fits within a single icon-sized space.
/// </summary>
public class BestiaryStarsControl : Control
{
    public static readonly DependencyProperty StarsProperty =
        DependencyProperty.Register(nameof(Stars), typeof(int), typeof(BestiaryStarsControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public int Stars
    {
        get => (int)GetValue(StarsProperty);
        set => SetValue(StarsProperty, value);
    }

    // SVG viewBox after transform: approximately 0,0 to 132.3 x 125.8
    // The matrix transform from the SVG: (-0.30739529, 0, 0, -0.30739529, 142.53249, 145.80854)
    private static readonly Matrix SvgMatrix = new(-0.30739529, 0, 0, -0.30739529, 142.53249, 145.80854);

    // 5 point regions of the interlaced pentagram, ordered clockwise from top.
    // Source: Right-handed_interlaced_pentagram.svg paths with labels.
    // The x-flip in the matrix swaps left↔right, so SVG "upper_left" becomes visual upper-right, etc.
    private static readonly System.Windows.Media.Geometry[] PointGeometries;

    private static readonly Brush LitBrush = new SolidColorBrush(Color.FromRgb(255, 200, 50)); // gold

    static BestiaryStarsControl()
    {
        LitBrush.Freeze();

        // SVG path data for each point (in source coordinates, before matrix transform)
        string[] pathData =
        [
            // 1: top_point (polygon9)
            "M 278.699,325.342 l 18.267,-0.004 -48.408,148.987 -74.116,-228.15 14.777,-10.736 59.357,182.632 Z",
            // 2: upper_left_point → visual upper-right after x-flip (polygon7)
            "M 331.136,243.449 l 5.641,-17.375 126.735,92.078 -239.886,-0.013 -5.644,-17.371 192.035,-0.016 Z",
            // 3: lower_left_point → visual lower-right after x-flip (polygon5)
            "M 269.458,168.295 l -14.781,-10.734 126.735,-92.078 -74.141,228.141 -18.266,0 59.328,-182.641 Z",
            // 4: lower_right_point → visual lower-left after x-flip (polygon13)
            "M 304.102,223.849 l -155.369,-112.862 30.135,92.724 -14.777,10.742 -48.408,-148.988 194.064,141.014 Z",
            // 5: upper_right_point → visual upper-left after x-flip (polygon11)
            "M 242.436,187.911 l -155.351,112.887 97.498,-0.006 5.65,17.373 -156.654,0 194.034,-140.991 Z",
        ];

        PointGeometries = new System.Windows.Media.Geometry[5];
        for (int i = 0; i < 5; i++)
        {
            var parsed = System.Windows.Media.Geometry.Parse(pathData[i]);
            var geo = parsed.Clone();
            geo.Transform = new MatrixTransform(SvgMatrix);
            geo.Freeze();
            PointGeometries[i] = geo;
        }
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        int stars = Math.Clamp(Stars, 0, 5);
        if (stars == 0 && PointGeometries.Length == 0) return;

        // Compute the combined bounds of all geometries to scale into the control
        var bounds = PointGeometries[0].Bounds;
        for (int i = 1; i < 5; i++)
            bounds.Union(PointGeometries[i].Bounds);

        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        // Scale to fit control with a small margin
        double margin = 1;
        double availW = ActualWidth - margin * 2;
        double availH = ActualHeight - margin * 2;
        double scale = Math.Min(availW / bounds.Width, availH / bounds.Height);
        double offsetX = margin + (availW - bounds.Width * scale) / 2 - bounds.X * scale;
        double offsetY = margin + (availH - bounds.Height * scale) / 2 - bounds.Y * scale;

        dc.PushTransform(new TranslateTransform(offsetX, offsetY));
        dc.PushTransform(new ScaleTransform(scale, scale));

        // Use Foreground for unlit, fall back to a dim gray
        var unlitBrush = GetValue(ForegroundProperty) as Brush
            ?? new SolidColorBrush(Color.FromArgb(80, 128, 128, 128));

        for (int i = 0; i < 5; i++)
        {
            dc.DrawGeometry(i < stars ? LitBrush : unlitBrush, null, PointGeometries[i]);
        }

        dc.Pop();
        dc.Pop();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double w = double.IsInfinity(availableSize.Width) ? 20 : availableSize.Width;
        double h = double.IsInfinity(availableSize.Height) ? 20 : availableSize.Height;
        double s = Math.Min(w, h);
        return new Size(s, s);
    }
}
