using System.Collections.Generic;
using System.Diagnostics;
using TEdit.Geometry.Primitives;

namespace TEdit.Geometry
{
    public class Shape
    {
        public static IEnumerable<Vector2Int32> DrawLineTool(Vector2Int32 begin, Vector2Int32 end)
        {
            int y0 = begin.Y;
            int x0 = begin.X;
            int y1 = end.Y;
            int x1 = end.X;

            int dy = y1 - y0;
            int dx = x1 - x0;
            int stepx, stepy;

            if (dy < 0)
            {
                dy = -dy;
                stepy = -1;
            }
            else
            {
                stepy = 1;
            }
            if (dx < 0)
            {
                dx = -dx;
                stepx = -1;
            }
            else
            {
                stepx = 1;
            }
            dy <<= 1;
            dx <<= 1;

            //y0 *= width;
            //y1 *= width;
            yield return new Vector2Int32(x0, y0);
            if (dx > dy)
            {
                int fraction = dy - (dx >> 1);
                while (x0 != x1)
                {
                    if (fraction >= 0)
                    {
                        y0 += stepy;
                        fraction -= dx;
                    }
                    x0 += stepx;
                    fraction += dy;
                    yield return new Vector2Int32(x0, y0);
                }
            }
            else
            {
                int fraction = dx - (dy >> 1);
                while (y0 != y1)
                {
                    if (fraction >= 0)
                    {
                        x0 += stepx;
                        fraction -= dy;
                    }
                    y0 += stepy;
                    fraction += dx;
                    yield return new Vector2Int32(x0, y0);
                }
            }
        }

        public static IEnumerable<Vector2Int32> DrawLine(Vector2Int32 start, Vector2Int32 end)
        {
            // Distance start and end point
            int dx = end.X - start.X;
            int dy = end.Y - start.Y;

            // Determine slope (absoulte value)
            int len = dy >= 0 ? dy : -dy;
            int lenx = dx >= 0 ? dx : -dx;
            if (lenx > len)
            {
                len = lenx;
            }

            // Prevent divison by zero
            if (len != 0)
            {
                // Init steps and start
                float incx = dx / (float)len;
                float incy = dy / (float)len;
                float x = start.X;
                float y = start.Y;

                // Walk the line!
                for (int i = 0; i < len; i++)
                {

                    yield return new Vector2Int32((int)x, (int)y);

                    x += incx;
                    y += incy;
                }
            }
        }

        public static IEnumerable<Vector2Int32> DrawPolyLine(IList<Vector2Int32> points)
        {
            if (points.Count >= 2)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    foreach (var vector2Int32 in DrawLine(points[i], points[i + 1]))
                    {
                        yield return vector2Int32;
                    }
                }
            }
        }

        public static IEnumerable<Vector2Int32> DrawTriangle(Vector2Int32 v1, Vector2Int32 v2, Vector2Int32 v3)
        {
            foreach (var vector2Int32 in DrawLine(v1, v2))
            {
                yield return vector2Int32;
            }

            foreach (var vector2Int32 in DrawLine(v2, v3))
            {
                yield return vector2Int32;
            }

            foreach (var vector2Int32 in DrawLine(v3, v1))
            {
                yield return vector2Int32;
            }
        }

        public static IEnumerable<Vector2Int32> DrawQuad(Vector2Int32 v1, Vector2Int32 v2, Vector2Int32 v3, Vector2Int32 v4)
        {
            //var result = DrawLine(v1, v2).Concat(DrawLine(v2, v3)).Concat(DrawLine(v3, v4)).Concat(DrawLine(v4, v1));
            //return result; // cannot return concat

            foreach (var vector2Int32 in DrawLine(v1, v2))
            {
                yield return vector2Int32;
            }

            foreach (var vector2Int32 in DrawLine(v2, v3))
            {
                yield return vector2Int32;
            }

            foreach (var vector2Int32 in DrawLine(v3, v4))
            {
                yield return vector2Int32;
            }

            foreach (var vector2Int32 in DrawLine(v4, v1))
            {
                yield return vector2Int32;
            }
        }

        public static IEnumerable<Vector2Int32> DrawRectangle(Vector2Int32 offset, Vector2Int32 size)
        {
            for (int x = offset.X; x < offset.X + size.X; x++)
            {
                yield return new Vector2Int32(x, offset.Y);
                yield return new Vector2Int32(x, offset.Y + size.Y);
            }

            for (int y = offset.Y; y < offset.Y + size.Y; y++)
            {
                yield return new Vector2Int32(offset.X, y);
                yield return new Vector2Int32(offset.X + size.X, y);
            }
        }

        public static IEnumerable<Vector2Int32> DrawEllipse(Vector2Int32 offset, Vector2Int32 size)
        {
            // Calc center and radius
            int xr = (size.X - offset.X) >> 1;
            int yr = (size.Y - offset.Y) >> 1;
            int xc = offset.X + xr;
            int yc = offset.Y + yr;

            var center = new Vector2Int32(xc, yc);
            var radius = new Vector2Int32(xr, yr);

            Debug.WriteLine($"Center: {center}, Radius: {radius}");

            return DrawEllipseCentered(center, radius);
        }

        public static IEnumerable<Vector2Int32> DrawEllipseCentered(Vector2Int32 center, Vector2Int32 radius)
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
                    //if (uy < 0) uy = 0;          // Clip
                    //if (uy >= h) uy = h - 1;      // ...
                    //if (ly < 0) ly = 0;
                    //if (ly >= h) ly = h - 1;
                    //uh = uy * w;                  // Upper half
                    //lh = ly * w;                  // Lower half

                    rx = xc + x;
                    lx = xc - x;
                    if (rx < 0) rx = 0; // Clip
                    //if (rx >= w) rx = w - 1;      // ...
                    if (lx < 0) lx = 0;
                    //if (lx >= w) lx = w - 1;
                    yield return new Vector2Int32(rx, uy); // Quadrant I (Actually an octant)
                    yield return new Vector2Int32(lx, uy); // Quadrant II
                    yield return new Vector2Int32(rx, ly); // Quadrant III
                    yield return new Vector2Int32(lx, ly); // Quadrant IV

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
                //if (uy < 0) uy = 0;          // Clip
                //if (uy >= h) uy = h - 1;      // ...
                //if (ly < 0) ly = 0;
                //if (ly >= h) ly = h - 1;
                //uh = uy * w;                  // Upper half
                //lh = ly * w;                  // Lower half
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
                    //if (rx < 0) rx = 0;          // Clip
                    //if (rx >= w) rx = w - 1;      // ...
                    //if (lx < 0) lx = 0;
                    //if (lx >= w) lx = w - 1;
                    yield return new Vector2Int32(rx, uy); // Quadrant I (Actually an octant)
                    yield return new Vector2Int32(lx, uy); // Quadrant II
                    yield return new Vector2Int32(rx, ly); // Quadrant III
                    yield return new Vector2Int32(lx, ly); // Quadrant IV  

                    x++;
                    xStopping += yrSqTwo;
                    err += xChg;
                    xChg += yrSqTwo;
                    if ((yChg + (err << 1)) > 0)
                    {
                        y--;
                        uy = yc + y; // Upper half
                        ly = yc - y; // Lower half
                        //if (uy < 0) uy = 0;          // Clip
                        //if (uy >= h) uy = h - 1;      // ...
                        //if (ly < 0) ly = 0;
                        //if (ly >= h) ly = h - 1;
                        //uh = uy * w;                  // Upper half
                        //lh = ly * w;                  // Lower half
                        yStopping -= xrSqTwo;
                        err += yChg;
                        yChg += xrSqTwo;
                    }
                }
            }
        }
    }
}