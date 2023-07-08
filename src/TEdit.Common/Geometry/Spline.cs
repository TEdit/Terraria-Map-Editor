using System;
using System.Collections.Generic;

namespace TEdit.Geometry;

public static class Spline
{
    public const float StepFactor = 2f;

    private static IEnumerable<Vector2Int32> ComputeBezierPoints(int x1, int y1, int cx1, int cy1, int cx2, int cy2, int x2, int y2)
    {
        // Determine distances between controls points (bounding rect) to find the optimal stepsize
        var minX = Math.Min(x1, Math.Min(cx1, Math.Min(cx2, x2)));
        var minY = Math.Min(y1, Math.Min(cy1, Math.Min(cy2, y2)));
        var maxX = Math.Max(x1, Math.Max(cx1, Math.Max(cx2, x2)));
        var maxY = Math.Max(y1, Math.Max(cy1, Math.Max(cy2, y2)));

        // Get slope
        var lenx = maxX - minX;
        var len = maxY - minY;
        if (lenx > len)
        {
            len = lenx;
        }

        //var list = new List<int>();

        if (len != 0)
        {
            // Init vars
            var step = StepFactor / len;
            int tx = x1;
            int ty = y1;

            yield return new Vector2Int32(tx, ty);

            // Interpolate
            for (var t = 0f; t <= 1; t += step)
            {
                var tSq = t * t;
                var t1 = 1 - t;
                var t1Sq = t1 * t1;

                tx = (int)(t1 * t1Sq * x1 + 3 * t * t1Sq * cx1 + 3 * t1 * tSq * cx2 + t * tSq * x2);
                ty = (int)(t1 * t1Sq * y1 + 3 * t * t1Sq * cy1 + 3 * t1 * tSq * cy2 + t * tSq * y2);

                yield return new Vector2Int32(tx, ty);
            }

            // Prevent rounding gap
            yield return new Vector2Int32(x2, y2);
        }
    }

    public static IEnumerable<Vector2Int32> DrawBeziers(IList<Vector2Int32> points)
    {
        int x1 = points[0].X;
        int y1 = points[0].Y;
        int x2, y2;
        for (int i = 1; i + 2 < points.Count; i += 3)
        {
            x2 = points[i + 2].X;
            y2 = points[i + 2].Y;
            foreach (var vector2Int32 in ComputeBezierPoints(x1, y1, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, x2, y2))
            {
                yield return vector2Int32;
            }
            x1 = x2;
            y1 = y2;
        }
    }

    private static IEnumerable<Vector2Int32> ComputeCardinalPoints(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, float tension)
    {
        // Determine distances between controls points (bounding rect) to find the optimal stepsize
        var minX = Math.Min(x1, Math.Min(x2, Math.Min(x3, x4)));
        var minY = Math.Min(y1, Math.Min(y2, Math.Min(y3, y4)));
        var maxX = Math.Max(x1, Math.Max(x2, Math.Max(x3, x4)));
        var maxY = Math.Max(y1, Math.Max(y2, Math.Max(y3, y4)));

        // Get slope
        var lenx = maxX - minX;
        var len = maxY - minY;
        if (lenx > len)
        {
            len = lenx;
        }

        // Prevent divison by zero
        if (len != 0)
        {
            // Init vars
            var step = StepFactor / len;

            // Calculate factors
            var sx1 = tension * (x3 - x1);
            var sy1 = tension * (y3 - y1);
            var sx2 = tension * (x4 - x2);
            var sy2 = tension * (y4 - y2);
            var ax = sx1 + sx2 + 2 * x2 - 2 * x3;
            var ay = sy1 + sy2 + 2 * y2 - 2 * y3;
            var bx = -2 * sx1 - sx2 - 3 * x2 + 3 * x3;
            var by = -2 * sy1 - sy2 - 3 * y2 + 3 * y3;

            yield return new Vector2Int32(x1, y1);
            // Interpolate
            for (var t = 0f; t <= 1; t += step)
            {
                var tSq = t * t;

                int tx = (int)(ax * tSq * t + bx * tSq + sx1 * t + x2);
                int ty = (int)(ay * tSq * t + by * tSq + sy1 * t + y2);
                yield return new Vector2Int32(tx, ty);

            }

            // Prevent rounding gap
            yield return new Vector2Int32(x3, y3);
        }
    }

    public static IEnumerable<Vector2Int32> DrawCurve(IList<Vector2Int32> points, float tension)
    {
        // First segment
        foreach (var point in ComputeCardinalPoints(points[0].X, points[0].Y, points[0].X, points[0].Y, points[1].X, points[1].Y, points[2].X, points[2].Y, tension))
        {
            yield return point;
        }

        // Middle segments
        int i;
        for (i = 1; i < points.Count - 2; i++)
        {
            foreach (var point in ComputeCardinalPoints(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, points[i + 2].X, points[i + 2].Y, tension))
            {
                yield return point;
            }
        }

        // Last segment
        foreach (var point in ComputeCardinalPoints(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, points[i + 1].X, points[i + 1].Y, tension))
        {
            yield return point;
        }
    }

    public static IEnumerable<Vector2Int32> DrawCurveClosed(IList<Vector2Int32> points, float tension)
    {
        int pn = points.Count;

        // First segment
        foreach (var point in ComputeCardinalPoints(points[pn - 1].X, points[pn - 1].Y, points[0].X, points[0].Y, points[1].X, points[1].Y, points[2].X, points[2].Y, tension))
        {
            yield return point;
        }

        // Middle segments
        int i;
        for (i = 1; i < points.Count - 2; i++)
        {
            foreach (var point in ComputeCardinalPoints(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, points[i + 2].X, points[i + 2].Y, tension))
            {
                yield return point;
            }
        }

        // Last segment
        foreach (var point in ComputeCardinalPoints(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, points[0].X, points[0].Y, tension))
        {
            yield return point;
        }

        // Last-to-first segment
        foreach (var point in ComputeCardinalPoints(points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, points[0].X, points[0].Y, points[1].X, points[1].Y, tension))
        {
            yield return point;
        }

    }
}
