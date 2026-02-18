// Note: Geometry uses WriteableBitmapEx for base algorithms. 
// See the WriteableBitmapEx source in this project for respective license

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TEdit.Geometry;

public static class Fill
{
    public static IEnumerable<Vector2Int32> FillRectangleVectorCenter(Vector2Int32 start, Vector2Int32 end, Vector2Int32 size)
    {
        var offsetStart = new Vector2Int32(start.X - size.X / 2, start.Y - size.Y / 2);
        var offsetEnd = new Vector2Int32(end.X - size.X / 2, end.Y - size.Y / 2);
        return FillRectangleVector(offsetStart, offsetEnd, size);
    }

    public static IEnumerable<Vector2Int32> FillRectangleVector(Vector2Int32 start, Vector2Int32 end, Vector2Int32 size)
    {
        for (int y = end.Y; y < end.Y + size.Y; y++)
        {
            for (int x = end.X; x < end.X + size.X; x++)
            {
                // skip coordinates inside start rectangle
                if (x > start.X && 
                    x < (start.X + size.X) &&
                    y > start.Y && 
                    y < (start.Y + size.Y))
                {
                    continue;
                }

                yield return new Vector2Int32(x, y);
            }
        }
    }

    public static IEnumerable<Vector2Int32> FillRectangleCentered(Vector2Int32 center, Vector2Int32 size)
    {
        var offset = new Vector2Int32(center.X - size.X/2, center.Y - size.Y/2);
        return FillRectangle(offset, size);
    }

    public static IEnumerable<Vector2Int32> FillRectangle(Vector2Int32 offset, Vector2Int32 size)
    {
        for (int y = offset.Y; y < offset.Y + size.Y; y++)
        {
            for (int x = offset.X; x < offset.X + size.X; x++)
            {
                yield return new Vector2Int32(x, y);
            }
        }
    }

    public static IEnumerable<Vector2Int32> FillEllipse(Vector2Int32 offset, Vector2Int32 size)
    {
        // Calc center and radius
        int xr = (size.X - offset.X) >> 1;
        int yr = (size.Y - offset.Y) >> 1;
        int xc = offset.X + xr;
        int yc = offset.Y + yr;

        var center = new Vector2Int32(xc, yc);
        var radius = new Vector2Int32(xr, yr);

        Debug.WriteLine($"Center: {center}, Radius: {radius}");

        return FillEllipseCentered(center, radius);
    }

    public static IEnumerable<Vector2Int32> FillEllipseCentered(Vector2Int32 center, Vector2Int32 radius)
    {
        int xr = radius.X;
        int yr = radius.Y;
        int xc = center.X;
        int yc = center.Y;

        if (xr <= 0 || yr <= 0)
        {
            yield return new Vector2Int32(xc, yc);
            yield break;
        }

        if (xr >= 1 && yr >= 1)
        {

            // Init vars
            int uy, ly, lx, rx;
            int x = xr;
            int y = 0;
            int xrSqTwo = (xr * xr) << 1;
            int yrSqTwo = (yr * yr) << 1;
            int xChg = yr * yr * (1 - (xr << 1));
            int yChg = xr * xr;
            int err = 0;
            int xStopping = yrSqTwo * xr;
            int yStopping = 0;

            // Draw first set of points counter clockwise where tangent line slope > -1.
            while (xStopping >= yStopping)
            {
                // Draw 4 quadrant points at once
                uy = yc + y; // Upper half
                ly = yc - y; // Lower half
                //if (uy < 0) uy = 0; // Clip
                //if (uy >= h) uy = h - 1; // ...
                //if (ly < 0) ly = 0;
                //if (ly >= h) ly = h - 1;
                //uh = uy*w; // Upper half
                //lh = ly*w; // Lower half

                rx = xc + x;
                lx = xc - x;
                //if (rx < 0) rx = 0; // Clip
                //if (rx >= w) rx = w - 1; // ...
                //if (lx < 0) lx = 0;
                //if (lx >= w) lx = w - 1;

                // Draw line
                for (int i = lx; i <= rx; i++)
                {
                    yield return new Vector2Int32(i, uy); // Quadrant II to I (Actually two octants)
                    yield return new Vector2Int32(i, ly); // Quadrant III to IV
                }

                y++;
                yStopping += xrSqTwo;
                err += yChg;
                yChg += xrSqTwo;
                if ((xChg + (err << 1)) > 0)
                {
                    x--;
                    xStopping -= yrSqTwo;
                    err += xChg;
                    xChg += yrSqTwo;
                }
            }

            // ReInit vars
            x = 0;
            y = yr;
            uy = yc + y; // Upper half
            ly = yc - y; // Lower half
            //if (uy < 0) uy = 0; // Clip
            //if (uy >= h) uy = h - 1; // ...
            //if (ly < 0) ly = 0;
            //if (ly >= h) ly = h - 1;
            //uh = uy*w; // Upper half
            //lh = ly*w; // Lower half
            xChg = yr * yr;
            yChg = xr * xr * (1 - (yr << 1));
            err = 0;
            xStopping = 0;
            yStopping = xrSqTwo * yr;

            // Draw second set of points clockwise where tangent line slope < -1.
            while (xStopping <= yStopping)
            {
                // Draw 4 quadrant points at once
                rx = xc + x;
                lx = xc - x;
                //if (rx < 0) rx = 0; // Clip
                //if (rx >= w) rx = w - 1; // ...
                //if (lx < 0) lx = 0;
                //if (lx >= w) lx = w - 1;

                // Draw line
                for (int i = lx; i <= rx; i++)
                {
                    yield return new Vector2Int32(i, uy); // Quadrant II to I (Actually two octants)
                    yield return new Vector2Int32(i, ly); // Quadrant III to IV
                }

                x++;
                xStopping += yrSqTwo;
                err += xChg;
                xChg += yrSqTwo;
                if ((yChg + (err << 1)) > 0)
                {
                    y--;
                    uy = yc + y; // Upper half
                    ly = yc - y; // Lower half
                    //if (uy < 0) uy = 0; // Clip
                    //if (uy >= h) uy = h - 1; // ...
                    //if (ly < 0) ly = 0;
                    //if (ly >= h) ly = h - 1;
                    //uh = uy*w; // Upper half
                    //lh = ly*w; // Lower half
                    yStopping -= xrSqTwo;
                    err += yChg;
                    yChg += xrSqTwo;
                }
            }
        }
    }

    public static IEnumerable<Vector2Int32> FillPolygon(IList<Vector2Int32> points)
    {
        if (points == null || points.Count < 3)
            yield break;

        int pn = points.Count;
        int[] intersectionsX = new int[pn]; // each edge can produce at most one intersection per scanline

        // Find y min and max
        int yMin = int.MaxValue;
        int yMax = int.MinValue;
        for (int i = 0; i < pn; i++)
        {
            int py = points[i].Y;
            if (py < yMin) yMin = py;
            if (py > yMax) yMax = py;
        }

        // Scan line from min to max
        // Based on http://alienryderflex.com/polygon_fill/
        for (int y = yMin; y <= yMax; y++)
        {
            int intersectionCount = 0;

            // Walk all edges, including the closing edge (last → first)
            int j = pn - 1;
            for (int i = 0; i < pn; i++)
            {
                float yi = points[i].Y;
                float yj = points[j].Y;

                if ((yi < y && yj >= y) || (yj < y && yi >= y))
                {
                    float xi = points[i].X;
                    float xj = points[j].X;
                    intersectionsX[intersectionCount++] = (int)(xi + (y - yi) / (yj - yi) * (xj - xi));
                }
                j = i;
            }

            // Insertion sort
            for (int i = 1; i < intersectionCount; i++)
            {
                int t = intersectionsX[i];
                int k = i;
                while (k > 0 && intersectionsX[k - 1] > t)
                {
                    intersectionsX[k] = intersectionsX[k - 1];
                    k--;
                }
                intersectionsX[k] = t;
            }

            // Fill between pairs of intersections
            for (int i = 0; i < intersectionCount - 1; i += 2)
            {
                for (int x = intersectionsX[i]; x <= intersectionsX[i + 1]; x++)
                {
                    yield return new Vector2Int32(x, y);
                }
            }
        }
    }

    public static IEnumerable<Vector2Int32> FillTriangle(Vector2Int32 v1, Vector2Int32 v2, Vector2Int32 v3)
    {
        return FillPolygon(new[] { v1, v2, v3 });
    }

    public static IEnumerable<Vector2Int32> FillQuad(Vector2Int32 v1, Vector2Int32 v2, Vector2Int32 v3, Vector2Int32 v4)
    {
        return FillPolygon(new[] { v1, v2, v3, v4 });
    }

    public static IEnumerable<Vector2Int32> FillStarCentered(Vector2Int32 center, int outerRadius, int innerRadius, int numPoints = 5)
    {
        if (outerRadius <= 0 || numPoints < 3) { yield return center; yield break; }
        if (innerRadius <= 0)
        {
            // Default to true {n/2} star polygon (pentagram for n=5)
            // where edges pass through opposite vertices
            innerRadius = (int)(outerRadius * Math.Cos(2 * Math.PI / numPoints) / Math.Cos(Math.PI / numPoints));
        }

        var vertices = new Vector2Int32[numPoints * 2];
        double angleStep = Math.PI / numPoints;
        double startAngle = -Math.PI / 2; // point up

        for (int i = 0; i < numPoints * 2; i++)
        {
            double angle = startAngle + i * angleStep;
            int r = (i % 2 == 0) ? outerRadius : innerRadius;
            vertices[i] = new Vector2Int32(
                center.X + (int)Math.Round(Math.Cos(angle) * r),
                center.Y + (int)Math.Round(Math.Sin(angle) * r));
        }

        foreach (var p in FillPolygon(vertices))
            yield return p;
    }

    public static IEnumerable<Vector2Int32> FillTriangleCentered(Vector2Int32 center, int halfWidth, int halfHeight)
    {
        if (halfWidth <= 0 || halfHeight <= 0) { yield return center; yield break; }

        // Equilateral triangle: height = halfWidth * sqrt(3) for 60-degree angles
        int eqHeight = (int)Math.Round(halfWidth * Math.Sqrt(3));
        // Centroid is at 1/3 from base, 2/3 from apex
        int apexY = center.Y - eqHeight * 2 / 3;
        int baseY = center.Y + eqHeight / 3;

        var top = new Vector2Int32(center.X, apexY);
        var bottomLeft = new Vector2Int32(center.X - halfWidth, baseY);
        var bottomRight = new Vector2Int32(center.X + halfWidth, baseY);

        foreach (var p in FillTriangle(top, bottomLeft, bottomRight))
            yield return p;
    }

    public static IEnumerable<Vector2Int32> FillCrescentCentered(Vector2Int32 center, int outerRadius, int innerRadius, int innerOffsetX)
    {
        if (outerRadius <= 0) { yield return center; yield break; }

        var outerPoints = new HashSet<Vector2Int32>(
            FillEllipseCentered(center, new Vector2Int32(outerRadius, outerRadius)));

        var innerCenter = new Vector2Int32(center.X + innerOffsetX, center.Y);
        var innerPoints = new HashSet<Vector2Int32>(
            FillEllipseCentered(innerCenter, new Vector2Int32(innerRadius, innerRadius)));

        foreach (var p in outerPoints)
        {
            if (!innerPoints.Contains(p))
                yield return p;
        }
    }

    public static IEnumerable<Vector2Int32> FillDonutCentered(Vector2Int32 center, int outerRadius, int innerRadius)
    {
        if (outerRadius <= 0) { yield return center; yield break; }

        var outerPoints = new HashSet<Vector2Int32>(
            FillEllipseCentered(center, new Vector2Int32(outerRadius, outerRadius)));

        var innerPoints = new HashSet<Vector2Int32>(
            FillEllipseCentered(center, new Vector2Int32(innerRadius, innerRadius)));

        foreach (var p in outerPoints)
        {
            if (!innerPoints.Contains(p))
                yield return p;
        }
    }

    public static IEnumerable<Vector2Int32> ApplyTransform(
        IEnumerable<Vector2Int32> points, Vector2Int32 center,
        double angleDegrees, bool flipX, bool flipY)
    {
        bool needsRotation = Math.Abs(angleDegrees) > 0.01;
        bool needsFlip = flipX || flipY;

        // Flip-only: simple forward transform (no gaps possible)
        if (!needsRotation)
        {
            foreach (var p in points)
            {
                int dx = p.X - center.X;
                int dy = p.Y - center.Y;
                if (flipX) dx = -dx;
                if (flipY) dy = -dy;
                yield return new Vector2Int32(center.X + dx, center.Y + dy);
            }
            yield break;
        }

        // Rotation: use inverse mapping to avoid gaps.
        // Collect source points into a bitmask, find bounding box,
        // compute output bounding box, then for each output pixel
        // apply inverse transform and check if source pixel exists.
        var sourceList = points as IList<Vector2Int32> ?? points.ToList();
        if (sourceList.Count == 0) yield break;

        int srcMinX = int.MaxValue, srcMinY = int.MaxValue;
        int srcMaxX = int.MinValue, srcMaxY = int.MinValue;
        foreach (var p in sourceList)
        {
            if (p.X < srcMinX) srcMinX = p.X;
            if (p.X > srcMaxX) srcMaxX = p.X;
            if (p.Y < srcMinY) srcMinY = p.Y;
            if (p.Y > srcMaxY) srcMaxY = p.Y;
        }

        int srcW = srcMaxX - srcMinX + 1;
        int srcH = srcMaxY - srcMinY + 1;
        var srcMask = new bool[srcW * srcH];
        foreach (var p in sourceList)
        {
            srcMask[(p.X - srcMinX) + (p.Y - srcMinY) * srcW] = true;
        }

        // Compute output bounding box by transforming source bbox corners
        double rad = angleDegrees * Math.PI / 180.0;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);

        int outMinX = int.MaxValue, outMinY = int.MaxValue;
        int outMaxX = int.MinValue, outMaxY = int.MinValue;
        int[] cornerXs = { srcMinX, srcMaxX, srcMinX, srcMaxX };
        int[] cornerYs = { srcMinY, srcMinY, srcMaxY, srcMaxY };
        for (int c = 0; c < 4; c++)
        {
            double dx = cornerXs[c] - center.X;
            double dy = cornerYs[c] - center.Y;
            if (flipX) dx = -dx;
            if (flipY) dy = -dy;
            int ox = center.X + (int)Math.Round(dx * cos - dy * sin);
            int oy = center.Y + (int)Math.Round(dx * sin + dy * cos);
            if (ox < outMinX) outMinX = ox;
            if (ox > outMaxX) outMaxX = ox;
            if (oy < outMinY) outMinY = oy;
            if (oy > outMaxY) outMaxY = oy;
        }

        // Inverse transform: reverse rotation then reverse flip
        // Forward: flip → rotate.  Inverse: unrotate → unflip.
        double invCos = Math.Cos(-rad);
        double invSin = Math.Sin(-rad);

        for (int oy = outMinY; oy <= outMaxY; oy++)
        {
            for (int ox = outMinX; ox <= outMaxX; ox++)
            {
                double dx = ox - center.X;
                double dy = oy - center.Y;

                // Inverse rotation
                double rx = dx * invCos - dy * invSin;
                double ry = dx * invSin + dy * invCos;

                // Inverse flip
                if (flipX) rx = -rx;
                if (flipY) ry = -ry;

                int sx = center.X + (int)Math.Round(rx);
                int sy = center.Y + (int)Math.Round(ry);

                // Check if source pixel exists
                int lx = sx - srcMinX;
                int ly = sy - srcMinY;
                if (lx >= 0 && lx < srcW && ly >= 0 && ly < srcH && srcMask[lx + ly * srcW])
                {
                    yield return new Vector2Int32(ox, oy);
                }
            }
        }
    }

    public static IEnumerable<Vector2Int32> FillBeziers(IList<Vector2Int32> points)
    {
        var polypoints = Spline.DrawBeziers(points).ToList();
        return FillPolygon(polypoints);
    }

    public static IEnumerable<Vector2Int32> FillCurve(IList<Vector2Int32> points, float tension)
    {
        var curve = Spline.DrawCurve(points, tension).ToList();
        return FillPolygon(curve);
    }

    public static IEnumerable<Vector2Int32> FillCurveClosed(IList<Vector2Int32> points, float tension)
    {
        var curve = Spline.DrawCurveClosed(points, tension).ToList();
        return FillPolygon(curve);
    }

    public static void Flood(Vector2Int32 start, Vector2Int32 minBound, Vector2Int32 maxBound, Func<Vector2Int32, bool> validation)
    {
        var ranges = new FloodFillRangeQueue();
        var points = new HashSet<Vector2Int32>();

        LinearFloodFill(ref start, ref minBound, ref maxBound, validation, ref ranges, ref points);

        while (ranges.Count > 0)
        {
            //**Get Next Range Off the Queue
            FloodFillRange range = ranges.Dequeue();

            //**Check Above and Below Each Pixel in the Floodfill Range
            int upY = range.Y - 1;//so we can pass the y coord by ref
            int downY = range.Y + 1;
            var curPoint = new Vector2Int32();
            for (int i = range.StartX; i <= range.EndX; i++)
            {
                //*Start Fill Upwards
                //if we're not above the top of the bitmap and the pixel above this one is within the color tolerance
                curPoint = new Vector2Int32(i, upY);
                if (range.Y > 0 && (!points.Contains(curPoint) && validation(curPoint)))
                    LinearFloodFill(ref curPoint, ref minBound, ref maxBound, validation, ref ranges, ref points);

                //*Start Fill Downwards
                //if we're not below the bottom of the bitmap and the pixel below this one is within the color tolerance
                curPoint = new Vector2Int32(i, downY);
                if (range.Y < (maxBound.Y - 1) && (!points.Contains(curPoint) && validation(curPoint)))
                    LinearFloodFill(ref curPoint, ref minBound, ref maxBound, validation, ref ranges, ref points);
            }
        }
    }

    private static void LinearFloodFill(ref Vector2Int32 start, ref Vector2Int32 minBound, ref Vector2Int32 maxBound, Func<Vector2Int32, bool> validation, ref FloodFillRangeQueue ranges, ref HashSet<Vector2Int32> points)
    {

        //FIND LEFT EDGE OF COLOR AREA
        int lFillLoc = start.X; //the location to check/fill on the left

        int x = start.X;
        int y = start.Y;
        points.Add(start);
        while (true)
        {
            points.Add(new Vector2Int32(lFillLoc, y));

            // Preform validation for next point
            lFillLoc--;
            var curPoint = new Vector2Int32(lFillLoc, y);
            if (lFillLoc <= minBound.X || !validation(curPoint) || points.Contains(curPoint))
                break;			 	 //exit loop if we're at edge of bitmap or match area

        }
        lFillLoc++;

        //FIND RIGHT EDGE OF COLOR AREA
        int rFillLoc = x; //the location to check/fill on the left

        while (true)
        {
            points.Add(new Vector2Int32(rFillLoc, y));

            rFillLoc++;
            var curPoint = new Vector2Int32(rFillLoc, y);
            if (rFillLoc >= maxBound.X || !validation(curPoint) || points.Contains(curPoint))
                break;			 	 //exit loop if we're at edge of bitmap or color area

        }
        rFillLoc--;

        var r = new FloodFillRange(lFillLoc, rFillLoc, y);
        ranges.Enqueue(ref r);
    }


    #region Flood Fill Helpers
    /// <summary>A queue of FloodFillRanges.</summary>
    internal class FloodFillRangeQueue
    {
        FloodFillRange[] array;
        int size;
        int head;

        /// <summary>
        /// Returns the number of items currently in the queue.
        /// </summary>
        public int Count
        {
            get { return size; }
        }

        public FloodFillRangeQueue()
            : this(10000)
        {

        }

        public FloodFillRangeQueue(int initialSize)
        {
            array = new FloodFillRange[initialSize];
            head = 0;
            size = 0;
        }

        /// <summary>Gets the <see cref="FloodFillRange"/> at the beginning of the queue.</summary>
        public FloodFillRange First
        {
            get { return array[head]; }
        }

        /// <summary>Adds a <see cref="FloodFillRange"/> to the end of the queue.</summary>
        public void Enqueue(ref FloodFillRange r)
        {
            if (size + head == array.Length)
            {
                var newArray = new FloodFillRange[2 * array.Length];
                Array.Copy(array, head, newArray, 0, size);
                array = newArray;
                head = 0;
            }
            array[head + (size++)] = r;
        }

        /// <summary>Removes and returns the <see cref="FloodFillRange"/> at the beginning of the queue.</summary>
        public FloodFillRange Dequeue()
        {
            var range = new FloodFillRange();
            if (size > 0)
            {
                range = array[head];
                array[head] = new FloodFillRange();
                head++;//advance head position
                size--;//update size to exclude dequeued item
            }
            return range;
        }

        /// <summary>Remove all FloodFillRanges from the queue.</summary>
        /*public void Clear() 
        {
            if (size > 0)
                Array.Clear(array, 0, size);
            size = 0;
        }*/

    }

    /// <summary>
    /// Represents a linear range to be filled and branched from.
    /// </summary>
    internal struct FloodFillRange
    {
        public int StartX;
        public int EndX;
        public int Y;

        public FloodFillRange(int startX, int endX, int y)
        {
            StartX = startX;
            EndX = endX;
            Y = y;
        }
    }

    #endregion
}
