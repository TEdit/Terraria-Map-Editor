// Note: Geometry uses WriteableBitmapEx for base algorithms. 
// See the WriteableBitmapEx source in this project for respective license

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TEdit.Geometry.Primitives;

namespace TEdit.Geometry
{
    public static class Fill
    {
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
            int pn = points.Count;
            int pnh = points.Count >> 1;
            int[] intersectionsX = new int[pnh];

            // Find y min and max (slightly faster than scanning from 0 to height)
            int yMin = int.MaxValue;
            int yMax = int.MinValue;
            for (int i = 0; i < pn; i++)
            {
                int py = points[i].Y;
                if (py < yMin) yMin = py;
                if (py > yMax) yMax = py;
            }

            // Scan line from min to max
            for (int y = yMin; y <= yMax; y++)
            {
                // Initial point x, y
                float vxi = points[0].X;
                float vyi = points[0].Y;

                // Find all intersections
                // Based on http://alienryderflex.com/polygon_fill/
                int intersectionCount = 0;
                for (int i = 1; i < pn; i++)
                {
                    // Next point x, y
                    float vxj = points[i].X;
                    float vyj = points[i].Y;

                    // Is the scanline between the two points
                    if (vyi < y && vyj >= y
                     || vyj < y && vyi >= y)
                    {
                        // Compute the intersection of the scanline with the edge (line between two points)
                        intersectionsX[intersectionCount++] = (int)(vxi + (y - vyi) / (vyj - vyi) * (vxj - vxi));
                    }
                    vxi = vxj;
                    vyi = vyj;
                }

                // Sort the intersections from left to right using Insertion sort 
                // It's faster than Array.Sort for this small data set
                int t, j;
                for (int i = 1; i < intersectionCount; i++)
                {
                    t = intersectionsX[i];
                    j = i;
                    while (j > 0 && intersectionsX[j - 1] > t)
                    {
                        intersectionsX[j] = intersectionsX[j - 1];
                        j = j - 1;
                    }
                    intersectionsX[j] = t;
                }

                // Fill the pixels between the intersections
                for (int i = 0; i < intersectionCount - 1; i += 2)
                {
                    int x0 = intersectionsX[i];
                    int x1 = intersectionsX[i + 1];

                    // Fill the pixels
                    for (int x = x0; x <= x1; x++)
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
}
