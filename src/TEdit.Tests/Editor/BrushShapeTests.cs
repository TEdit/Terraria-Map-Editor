using System;
using System.Collections.Generic;
using System.Linq;
using TEdit.Editor;
using TEdit.Geometry;
using TEdit.Common.Geometry;
using Xunit;
using Xunit.Abstractions;

namespace TEdit.Tests.Editor;

public class BrushShapeTests
{
    private readonly ITestOutputHelper _output;

    public BrushShapeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(11, 11)]
    [InlineData(20, 20)]
    [InlineData(21, 21)]
    [InlineData(20, 10)]
    public void CrossShape_EndpointsShouldMatchRectangleCorners(int width, int height)
    {
        var brush = new BrushSettings();
        brush.Width = width;
        brush.Height = height;

        var center = new Vector2Int32(0, 0);

        // Get rectangle points to find actual corners
        brush.Shape = BrushShape.Square;
        var rectPoints = brush.GetShapePoints(center);
        int rectMinX = rectPoints.Min(p => p.X);
        int rectMaxX = rectPoints.Max(p => p.X);
        int rectMinY = rectPoints.Min(p => p.Y);
        int rectMaxY = rectPoints.Max(p => p.Y);

        _output.WriteLine($"Rectangle bounds for {width}x{height}: X=[{rectMinX},{rectMaxX}] Y=[{rectMinY},{rectMaxY}]");

        // Get cross points
        brush.Shape = BrushShape.Cross;
        var crossPoints = brush.GetShapePoints(center);

        int crossMinX = crossPoints.Min(p => p.X);
        int crossMaxX = crossPoints.Max(p => p.X);
        int crossMinY = crossPoints.Min(p => p.Y);
        int crossMaxY = crossPoints.Max(p => p.Y);

        _output.WriteLine($"Cross bounds for {width}x{height}: X=[{crossMinX},{crossMaxX}] Y=[{crossMinY},{crossMaxY}]");

        // Cross shape endpoints should reach the rectangle corners
        Assert.Equal(rectMinX, crossMinX);
        Assert.Equal(rectMaxX, crossMaxX);
        Assert.Equal(rectMinY, crossMinY);
        Assert.Equal(rectMaxY, crossMaxY);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(11, 11)]
    [InlineData(20, 20)]
    public void CrossShape_PreviewAndPaintUseSamePoints(int width, int height)
    {
        var brush = new BrushSettings();
        brush.Width = width;
        brush.Height = height;
        brush.Shape = BrushShape.Cross;

        var center = new Vector2Int32(0, 0);

        // Preview path: GetShapePoints directly
        var previewPoints = new HashSet<Vector2Int32>(brush.GetShapePoints(center));

        // Paint path: EnsureCacheValid + StampOffsets
        var paintBuffer = new List<Vector2Int32>();
        brush.StampOffsets(center, paintBuffer);
        var paintPoints = new HashSet<Vector2Int32>(paintBuffer);

        _output.WriteLine($"Preview points: {previewPoints.Count}, Paint points: {paintPoints.Count}");

        // Both should produce identical points (before ThickenLine, which is applied to paint but not preview in GetShapePoints)
        // Actually both paths apply ThickenLine - let's check
        var onlyInPreview = previewPoints.Except(paintPoints).ToList();
        var onlyInPaint = paintPoints.Except(previewPoints).ToList();

        if (onlyInPreview.Count > 0)
            _output.WriteLine($"Only in preview: {string.Join(", ", onlyInPreview.Take(10))}");
        if (onlyInPaint.Count > 0)
            _output.WriteLine($"Only in paint: {string.Join(", ", onlyInPaint.Take(10))}");

        Assert.Equal(previewPoints.Count, paintPoints.Count);
        Assert.True(previewPoints.SetEquals(paintPoints), "Preview and paint points should be identical");
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(20, 20)]
    public void CrossShape_VisualizeBounds(int width, int height)
    {
        var brush = new BrushSettings();
        brush.Width = width;
        brush.Height = height;
        var center = new Vector2Int32(0, 0);

        // Rectangle
        brush.Shape = BrushShape.Square;
        var rectPoints = new HashSet<Vector2Int32>(brush.GetShapePoints(center));

        // Cross
        brush.Shape = BrushShape.Cross;
        var crossPoints = new HashSet<Vector2Int32>(brush.GetShapePoints(center));

        int minX = Math.Min(rectPoints.Min(p => p.X), crossPoints.Min(p => p.X)) - 1;
        int maxX = Math.Max(rectPoints.Max(p => p.X), crossPoints.Max(p => p.X)) + 1;
        int minY = Math.Min(rectPoints.Min(p => p.Y), crossPoints.Min(p => p.Y)) - 1;
        int maxY = Math.Max(rectPoints.Max(p => p.Y), crossPoints.Max(p => p.Y)) + 1;

        _output.WriteLine($"Visualization for {width}x{height} brush:");
        _output.WriteLine($"R=rect only, X=cross only, B=both, .=empty");
        for (int y = minY; y <= maxY; y++)
        {
            var row = new char[maxX - minX + 1];
            for (int x = minX; x <= maxX; x++)
            {
                bool inRect = rectPoints.Contains(new Vector2Int32(x, y));
                bool inCross = crossPoints.Contains(new Vector2Int32(x, y));
                row[x - minX] = (inRect, inCross) switch
                {
                    (true, true) => 'B',
                    (true, false) => 'R',
                    (false, true) => 'X',
                    _ => '.'
                };
            }
            _output.WriteLine($"  y={y,3}: {new string(row)}");
        }
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(20, 20)]
    [InlineData(11, 11)]
    [InlineData(20, 10)]
    public void LineShape_PreviewOffsetMatchesBrushOffset(int width, int height)
    {
        var brush = new BrushSettings();
        brush.Width = width;
        brush.Height = height;

        foreach (var shape in new[] { BrushShape.Left, BrushShape.Right, BrushShape.Cross })
        {
            brush.Shape = shape;
            var center = new Vector2Int32(0, 0);
            var points = brush.GetShapePoints(center);
            points = BrushSettings.ThickenLine(points, brush.Outline);

            int minX = points.Min(p => p.X);
            int minY = points.Min(p => p.Y);
            int previewOffsetX = -minX;
            int previewOffsetY = -minY;

            _output.WriteLine($"{shape} {width}x{height}: PreviewOffset=({previewOffsetX},{previewOffsetY}) Brush.Offset=({brush.OffsetX},{brush.OffsetY})");

            Assert.Equal(brush.OffsetX, previewOffsetX);
            Assert.Equal(brush.OffsetY, previewOffsetY);
        }
    }

    [Theory]
    [InlineData(BrushShape.Left, 10, 10)]
    [InlineData(BrushShape.Right, 10, 10)]
    [InlineData(BrushShape.Left, 20, 20)]
    [InlineData(BrushShape.Right, 20, 20)]
    public void DiagonalShape_EndpointsShouldMatchRectangleCorners(BrushShape shape, int width, int height)
    {
        var brush = new BrushSettings();
        brush.Width = width;
        brush.Height = height;

        var center = new Vector2Int32(0, 0);

        // Get rectangle bounds
        brush.Shape = BrushShape.Square;
        var rectPoints = brush.GetShapePoints(center);
        int rectMinX = rectPoints.Min(p => p.X);
        int rectMaxX = rectPoints.Max(p => p.X);
        int rectMinY = rectPoints.Min(p => p.Y);
        int rectMaxY = rectPoints.Max(p => p.Y);

        // Get diagonal points
        brush.Shape = shape;
        var diagPoints = brush.GetShapePoints(center);

        int diagMinX = diagPoints.Min(p => p.X);
        int diagMaxX = diagPoints.Max(p => p.X);
        int diagMinY = diagPoints.Min(p => p.Y);
        int diagMaxY = diagPoints.Max(p => p.Y);

        _output.WriteLine($"{shape} bounds for {width}x{height}: X=[{diagMinX},{diagMaxX}] Y=[{diagMinY},{diagMaxY}]");
        _output.WriteLine($"Rectangle bounds: X=[{rectMinX},{rectMaxX}] Y=[{rectMinY},{rectMaxY}]");

        Assert.Equal(rectMinX, diagMinX);
        Assert.Equal(rectMaxX, diagMaxX);
        Assert.Equal(rectMinY, diagMinY);
        Assert.Equal(rectMaxY, diagMaxY);
    }
}
