#region Header

//
//   Project:           WriteableBitmapEx - Silverlight WriteableBitmap extensions
//   Description:       Collection of draw extension methods for the Silverlight WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2010-09-17 22:20:49 +0200 (Pt, 17 wrz 2010) $
//   Changed in:        $Revision: 61101 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapShapeExtensions.cs $
//   Id:                $Id: WriteableBitmapShapeExtensions.cs 61101 2010-09-17 20:20:49Z unknown $
//
//
//   Copyright © 2009-2010 Rene Schulte and WriteableBitmapEx Contributors
//
//   This Software is weak copyleft open source. Please read the License.txt for details.
//

#endregion

namespace System.Windows.Media.Imaging
{
    /// <summary>
    /// Collection of draw extension methods for the Silverlight WriteableBitmap class.
    /// </summary>
    public static partial class WriteableBitmapExtensions
    {
        #region Methods

        #region DrawLine

        public enum EFLAMode
        {
            Div,
            Mul,
            Add,
            AddFixed,
            AddFixedPreCal
        }

        public static void DrawLineEFLA(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color, EFLAMode mode)
        {
            int a = color.A + 1;
            int col = (color.A << 24)
                      | ((byte) ((color.R*a) >> 8) << 16)
                      | ((byte) ((color.G*a) >> 8) << 8)
                      | ((byte) ((color.B*a) >> 8));
            DrawLineEFLA(bmp, x1, y1, x2, y2, col, mode);
        }

        public static void DrawLineEFLA(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color, EFLAMode mode)
        {
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
            bmp.Lock();

            bool yLonger;
            int incrementVal, endVal;
            int shortLen;
            int longLen;

            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                int x, y;

                switch (mode)
                {
                    case EFLAMode.Div:
                        yLonger = false;

                        shortLen = y2 - y1;
                        longLen = x2 - x1;

                        if (Math.Abs(shortLen) > Math.Abs(longLen))
                        {
                            int swap = shortLen;
                            shortLen = longLen;
                            longLen = swap;
                            yLonger = true;
                        }

                        if (longLen < 0) incrementVal = -1;
                        else incrementVal = 1;

                        double divDiff;
                        if (shortLen == 0) divDiff = longLen;
                        else divDiff = longLen/(double) shortLen;
                        if (yLonger)
                        {
                            for (int i = 0; i != longLen; i += incrementVal)
                            {
                                x = x1 + (int) (i/divDiff);
                                y = y1 + i;
                                pixels[y*w + x] = color;
                            }
                        }
                        else
                        {
                            for (int i = 0; i != longLen; i += incrementVal)
                            {
                                x = x1 + i;
                                y = y1 + (int) (i/divDiff);
                                pixels[y*w + x] = color;
                            }
                        }
                        break;
                    case EFLAMode.Mul:
                        yLonger = false;
                        shortLen = y2 - y1;
                        longLen = x2 - x1;

                        if (Math.Abs(shortLen) > Math.Abs(longLen))
                        {
                            int swap = shortLen;
                            shortLen = longLen;
                            longLen = swap;
                            yLonger = true;
                        }

                        if (longLen < 0) incrementVal = -1;
                        else incrementVal = 1;

                        double multDiff;
                        if (longLen == 0.0) multDiff = shortLen;
                        else multDiff = shortLen/(double) longLen;
                        if (yLonger)
                        {
                            for (int i = 0; i != longLen; i += incrementVal)
                            {
                                x = x1 + (int) (i*multDiff);
                                y = y1 + i;
                                pixels[y*w + x] = color;
                            }
                        }
                        else
                        {
                            for (int i = 0; i != longLen; i += incrementVal)
                            {
                                x = x1 + i;
                                y = y1 + (int) (i*multDiff);
                                pixels[y*w + x] = color;
                            }
                        }
                        break;
                    case EFLAMode.Add:
                        yLonger = false;

                        shortLen = y2 - y1;
                        longLen = x2 - x1;
                        if (Math.Abs(shortLen) > Math.Abs(longLen))
                        {
                            int swap = shortLen;
                            shortLen = longLen;
                            longLen = swap;
                            yLonger = true;
                        }

                        endVal = longLen;
                        if (longLen < 0)
                        {
                            incrementVal = -1;
                            longLen = -longLen;
                        }
                        else incrementVal = 1;

                        double decInc;
                        if (longLen == 0) decInc = shortLen;
                        else decInc = (shortLen/(double) longLen);
                        double j = 0.0;
                        if (yLonger)
                        {
                            for (int i = 0; i != endVal; i += incrementVal)
                            {
                                x = x1 + (int) j;
                                y = y1 + i;
                                pixels[y*w + x] = color;
                                j += decInc;
                            }
                        }
                        else
                        {
                            for (int i = 0; i != endVal; i += incrementVal)
                            {
                                x = x1 + i;
                                y = y1 + (int) j;
                                pixels[y*w + x] = color;
                                j += decInc;
                            }
                        }
                        break;
                    case EFLAMode.AddFixed:
                        yLonger = false;
                        shortLen = y2 - y1;
                        longLen = x2 - x1;
                        if (Math.Abs(shortLen) > Math.Abs(longLen))
                        {
                            int swap = shortLen;
                            shortLen = longLen;
                            longLen = swap;
                            yLonger = true;
                        }
                        endVal = longLen;
                        if (longLen < 0)
                        {
                            incrementVal = -1;
                            longLen = -longLen;
                        }
                        else incrementVal = 1;

                        int decIncI = 0;
                        if (longLen == 0) decInc = 0;
                        else decIncI = (shortLen << 16)/longLen;
                        int jI = 0;
                        if (yLonger)
                        {
                            for (int i = 0; i != endVal; i += incrementVal)
                            {
                                x = x1 + (jI >> 16);
                                y = y1 + i;
                                pixels[y*w + x] = color;
                                jI += decIncI;
                            }
                        }
                        else
                        {
                            for (int i = 0; i != endVal; i += incrementVal)
                            {
                                x = x1 + i;
                                y = y1 + (jI >> 16);
                                pixels[y*w + x] = color;
                                jI += decIncI;
                            }
                        }

                        break;
                    case EFLAMode.AddFixedPreCal:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("mode");
                }
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
        }

        /// <summary>
        /// Draws a colored line by connecting two points using the Bresenham algorithm.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawLineBresenham(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int col = (color.A << 24)
                      | ((byte) ((color.R*a) >> 8) << 16)
                      | ((byte) ((color.G*a) >> 8) << 8)
                      | ((byte) ((color.B*a) >> 8));
            bmp.DrawLineBresenham(x1, y1, x2, y2, col);
        }

        /// <summary>
        /// Draws a colored line by connecting two points using the Bresenham algorithm.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawLineBresenham(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
#if SILVERLIGHT
         int[] pixels = bmp.Pixels;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
#endif
                // Distance start and end point
                int dx = x2 - x1;
                int dy = y2 - y1;

                // Determine sign for direction x
                int incx = 0;
                if (dx < 0)
                {
                    dx = -dx;
                    incx = -1;
                }
                else if (dx > 0)
                {
                    incx = 1;
                }

                // Determine sign for direction y
                int incy = 0;
                if (dy < 0)
                {
                    dy = -dy;
                    incy = -1;
                }
                else if (dy > 0)
                {
                    incy = 1;
                }

                // Which gradient is larger
                int pdx, pdy, odx, ody, es, el;
                if (dx > dy)
                {
                    pdx = incx;
                    pdy = 0;
                    odx = incx;
                    ody = incy;
                    es = dy;
                    el = dx;
                }
                else
                {
                    pdx = 0;
                    pdy = incy;
                    odx = incx;
                    ody = incy;
                    es = dx;
                    el = dy;
                }

                // Init start
                int x = x1;
                int y = y1;
                int error = el >> 1;
                if (y < h && y >= 0 && x < w && x >= 0)
                {
                    pixels[y*w + x] = color;
                }

                // Walk the line!
                for (int i = 0; i < el; i++)
                {
                    // Update error term
                    error -= es;

                    // Decide which coord to use
                    if (error < 0)
                    {
                        error += el;
                        x += odx;
                        y += ody;
                    }
                    else
                    {
                        x += pdx;
                        y += pdy;
                    }

                    // Set pixel
                    if (y < h && y >= 0 && x < w && x >= 0)
                    {
                        pixels[y*w + x] = color;
                    }
                }
#if !SILVERLIGHT
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Draws a colored line by connecting two points using a DDA algorithm (Digital Differential Analyzer).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawLineDDA(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int col = (color.A << 24)
                      | ((byte) ((color.R*a) >> 8) << 16)
                      | ((byte) ((color.G*a) >> 8) << 8)
                      | ((byte) ((color.B*a) >> 8));

            bmp.DrawLineDDA(x1, y1, x2, y2, col);
        }

        /// <summary>
        /// Draws a colored line by connecting two points using a DDA algorithm (Digital Differential Analyzer).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawLineDDA(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
#if SILVERLIGHT
         int[] pixels = bmp.Pixels;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
#endif
                // Distance start and end point
                int dx = x2 - x1;
                int dy = y2 - y1;

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
                    float incx = dx/(float) len;
                    float incy = dy/(float) len;
                    float x = x1;
                    float y = y1;

                    // Walk the line!
                    for (int i = 0; i < len; i++)
                    {
                        if (y < h && y >= 0 && x < w && x >= 0)
                        {
                            pixels[(int) y*w + (int) x] = color;
                        }
                        x += incx;
                        y += incy;
                    }
                }
#if !SILVERLIGHT
            }
            bmp.Unlock();
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
#endif
        }

        /// <summary>
        /// Draws a colored line by connecting two points using an optimized DDA.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawLine(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int col = (color.A << 24)
                      | ((byte) ((color.R*a) >> 8) << 16)
                      | ((byte) ((color.G*a) >> 8) << 8)
                      | ((byte) ((color.B*a) >> 8));

            bmp.DrawLine(x1, y1, x2, y2, col);
        }

        /// <summary>
        /// Draws a colored line by connecting two points using an optimized DDA.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawLine(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color)
        {
#if SILVERLIGHT
         DrawLine(bmp.Pixels, bmp.PixelWidth, bmp.PixelHeight, x1, y1, x2, y2, color);
#else
            bmp.Lock();
            DrawLine(bmp.BackBuffer, bmp.BackBufferStride/SizeOfArgb, bmp.PixelHeight, x1, y1, x2, y2, color);
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Draws a colored line by connecting two points using an optimized DDA. 
        /// Uses the pixels array and the width directly for best performance.
        /// </summary>
        /// <param name="pixels">An array containing the pixels as int RGBA value.</param>
        /// <param name="pixelWidth">The width of one scanline in the pixels array.</param>
        /// <param name="pixelHeight">The height of the bitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
#if SILVERLIGHT
      public static void DrawLine(int[] pixels, int pixelWidth, int pixelHeight, int x1, int y1, int x2, int y2, int color)
      {
#else
        private static void DrawLine(IntPtr ptr, int pixelWidth, int pixelHeight, int x1, int y1, int x2, int y2,
                                     int color)
        {
            unsafe
            {
                var pixels = (int*) ptr;
#endif
                // Distance start and end point
                int dx = x2 - x1;
                int dy = y2 - y1;

                const int PRECISION_SHIFT = 8;
                const int PRECISION_VALUE = 1 << PRECISION_SHIFT;

                // Determine slope (absoulte value)
                int lenX, lenY;
                int incy1;
                if (dy >= 0)
                {
                    incy1 = PRECISION_VALUE;
                    lenY = dy;
                }
                else
                {
                    incy1 = -PRECISION_VALUE;
                    lenY = -dy;
                }

                int incx1;
                if (dx >= 0)
                {
                    incx1 = 1;
                    lenX = dx;
                }
                else
                {
                    incx1 = -1;
                    lenX = -dx;
                }

                if (lenX > lenY)
                {
                    // x increases by +/- 1
                    // Init steps and start
                    int incy = (dy << PRECISION_SHIFT)/lenX;
                    int y = y1 << PRECISION_SHIFT;

                    // Walk the line!
                    for (int i = 0; i < lenX; i++)
                    {
                        // Check boundaries
                        y1 = y >> PRECISION_SHIFT;
                        if (x1 >= 0 && x1 < pixelWidth && y1 >= 0 && y1 < pixelHeight)
                        {
                            int i2 = y1*pixelWidth + x1;
                            pixels[i2] = color;
                        }
                        x1 += incx1;
                        y += incy;
                    }
                }
                else
                {
                    // Prevent divison by zero
                    if (lenY == 0)
                    {
                        return;
                    }

                    // Init steps and start
                    // since y increases by +/-1, we can safely add (*h) before the for() loop, since there is no fractional value for y
                    int incx = (dx << PRECISION_SHIFT)/lenY;
                    int x = x1 << PRECISION_SHIFT;
                    int y = y1 << PRECISION_SHIFT;
                    int index = (x1 + y1*pixelWidth) << PRECISION_SHIFT;

                    // Walk the line!
                    int inc = incy1*pixelWidth + incx;
                    for (int i = 0; i < lenY; i++)
                    {
                        x1 = x >> PRECISION_SHIFT;
                        y1 = y >> PRECISION_SHIFT;
                        if (x1 >= 0 && x1 < pixelWidth && y1 >= 0 && y1 < pixelHeight)
                        {
                            pixels[index >> PRECISION_SHIFT] = color;
                        }
                        x += incx;
                        y += incy1;
                        index += inc;
                    }
                }
#if !SILVERLIGHT
            }
#endif
        }

        #endregion

        #region Draw Shapes

        #region Polyline, Triangle, Quad

        /// <summary>
        /// Draws a polyline. Add the first point also at the end of the array if the line should be closed.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points of the polyline in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawPolyline(this WriteableBitmap bmp, int[] points, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int col = (color.A << 24)
                      | ((byte) ((color.R*a) >> 8) << 16)
                      | ((byte) ((color.G*a) >> 8) << 8)
                      | ((byte) ((color.B*a) >> 8));
            bmp.DrawPolyline(points, col);
        }

        /// <summary>
        /// Draws a polyline. Add the first point also at the end of the array if the line should be closed.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points of the polyline in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawPolyline(this WriteableBitmap bmp, int[] points, int color)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
#if SILVERLIGHT         
          int[] pixels = bmp.Pixels;
#else
            bmp.Lock();
            IntPtr pixels = bmp.BackBuffer;
#endif
            int x1 = points[0];
            int y1 = points[1];
            int x2, y2;
            for (int i = 2; i < points.Length; i += 2)
            {
                x2 = points[i];
                y2 = points[i + 1];
                DrawLine(pixels, w, h, x1, y1, x2, y2, color);
                x1 = x2;
                y1 = y2;
            }
#if !SILVERLIGHT
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="color">The color.</param>
        public static void DrawTriangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int x3, int y3,
                                        Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int col = (color.A << 24)
                      | ((byte) ((color.R*a) >> 8) << 16)
                      | ((byte) ((color.G*a) >> 8) << 8)
                      | ((byte) ((color.B*a) >> 8));
            bmp.DrawTriangle(x1, y1, x2, y2, x3, y3, col);
        }

        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="color">The color.</param>
        public static void DrawTriangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int x3, int y3,
                                        int color)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
#if SILVERLIGHT
         int[] pixels = bmp.Pixels;
#else
            bmp.Lock();
            IntPtr pixels = bmp.BackBuffer;
#endif

            DrawLine(pixels, w, h, x1, y1, x2, y2, color);
            DrawLine(pixels, w, h, x2, y2, x3, y3, color);
            DrawLine(pixels, w, h, x3, y3, x1, y1, color);
#if !SILVERLIGHT
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Draws a quad.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="x4">The x-coordinate of the 4th point.</param>
        /// <param name="y4">The y-coordinate of the 4th point.</param>
        /// <param name="color">The color.</param>
        public static void DrawQuad(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int x3, int y3, int x4,
                                    int y4, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int col = (color.A << 24)
                      | ((byte) ((color.R*a) >> 8) << 16)
                      | ((byte) ((color.G*a) >> 8) << 8)
                      | ((byte) ((color.B*a) >> 8));

            bmp.DrawQuad(x1, y1, x2, y2, x3, y3, x4, y4, col);
        }

        /// <summary>
        /// Draws a quad.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the 1st point.</param>
        /// <param name="y1">The y-coordinate of the 1st point.</param>
        /// <param name="x2">The x-coordinate of the 2nd point.</param>
        /// <param name="y2">The y-coordinate of the 2nd point.</param>
        /// <param name="x3">The x-coordinate of the 3rd point.</param>
        /// <param name="y3">The y-coordinate of the 3rd point.</param>
        /// <param name="x4">The x-coordinate of the 4th point.</param>
        /// <param name="y4">The y-coordinate of the 4th point.</param>
        /// <param name="color">The color.</param>
        public static void DrawQuad(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int x3, int y3, int x4,
                                    int y4, int color)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
#if SILVERLIGHT
         int[] pixels = bmp.Pixels;
#else
            bmp.Lock();
            IntPtr pixels = bmp.BackBuffer;
#endif

            DrawLine(pixels, w, h, x1, y1, x2, y2, color);
            DrawLine(pixels, w, h, x2, y2, x3, y3, color);
            DrawLine(pixels, w, h, x3, y3, x4, y4, color);
            DrawLine(pixels, w, h, x4, y4, x1, y1, color);

#if !SILVERLIGHT
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        #endregion

        #region Rectangle

        /// <summary>
        /// Draws a rectangle.
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color.</param>
        public static void DrawRectangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int col = (color.A << 24)
                      | ((byte) ((color.R*a) >> 8) << 16)
                      | ((byte) ((color.G*a) >> 8) << 8)
                      | ((byte) ((color.B*a) >> 8));

            bmp.DrawRectangle(x1, y1, x2, y2, col);
        }

        /// <summary>
        /// Draws a rectangle.
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color.</param>
        public static void DrawRectangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
#if SILVERLIGHT
         int[] pixels = bmp.Pixels;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
#endif
                // Check boundaries
                if (x1 < 0)
                {
                    x1 = 0;
                }
                if (y1 < 0)
                {
                    y1 = 0;
                }
                if (x2 < 0)
                {
                    x2 = 0;
                }
                if (y2 < 0)
                {
                    y2 = 0;
                }
                if (x1 >= w)
                {
                    x1 = w - 1;
                }
                if (y1 >= h)
                {
                    y1 = h - 1;
                }
                if (x2 >= w)
                {
                    x2 = w - 1;
                }
                if (y2 >= h)
                {
                    y2 = h - 1;
                }

                int startY = y1*w;
                int endY = y2*w;

                int offset2 = endY + x1;
                int endOffset = startY + x2;
                int startYPlusX1 = startY + x1;

                // top and bottom horizontal scanlines
                for (int x = startYPlusX1; x <= endOffset; x++)
                {
                    pixels[x] = color; // top horizontal line
                    pixels[offset2] = color; // bottom horizontal line
                    offset2++;
                }

                // offset2 == endY + x2

                // vertical scanlines
                endOffset = startYPlusX1 + w;
                offset2 -= w;

                for (int y = startY + x2 + w; y < offset2; y += w)
                {
                    pixels[y] = color; // right vertical line
                    pixels[endOffset] = color; // left vertical line
                    endOffset += w;
                }
#if !SILVERLIGHT
            }
            bmp.AddDirtyRect(new Int32Rect(x1, y1, x2 - x1, y2 - y1));
            bmp.Unlock();
#endif
        }

        #endregion

        #region Ellipse

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawEllipse(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int col = (color.A << 24)
                      | ((byte) ((color.R*a) >> 8) << 16)
                      | ((byte) ((color.G*a) >> 8) << 8)
                      | ((byte) ((color.B*a) >> 8));
            bmp.DrawEllipse(x1, y1, x2, y2, col);
        }

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawEllipse(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color)
        {
            // Calc center and radius
            int xr = (x2 - x1) >> 1;
            int yr = (y2 - y1) >> 1;
            int xc = x1 + xr;
            int yc = y1 + yr;
            bmp.DrawEllipseCentered(xc, yc, xr, yr, color);
        }

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf
        /// Uses a different parameter representation than DrawEllipse().
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="xc">The x-coordinate of the ellipses center.</param>
        /// <param name="yc">The y-coordinate of the ellipses center.</param>
        /// <param name="xr">The radius of the ellipse in x-direction.</param>
        /// <param name="yr">The radius of the ellipse in y-direction.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawEllipseCentered(this WriteableBitmap bmp, int xc, int yc, int xr, int yr, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int col = (color.A << 24)
                      | ((byte) ((color.R*a) >> 8) << 16)
                      | ((byte) ((color.G*a) >> 8) << 8)
                      | ((byte) ((color.B*a) >> 8));
            bmp.DrawEllipseCentered(xc, yc, xr, yr, col);
        }

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// Uses a different parameter representation than DrawEllipse().
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="xc">The x-coordinate of the ellipses center.</param>
        /// <param name="yc">The y-coordinate of the ellipses center.</param>
        /// <param name="xr">The radius of the ellipse in x-direction.</param>
        /// <param name="yr">The radius of the ellipse in y-direction.</param>
        /// <param name="color">The color for the line.</param>
        public static void DrawEllipseCentered(this WriteableBitmap bmp, int xc, int yc, int xr, int yr, int color)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
#if SILVERLIGHT
          int[] pixels = bmp.Pixels;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
#endif
                // Init vars
                int uh, lh, uy, ly, lx, rx;
                int x = xr;
                int y = 0;
                int xrSqTwo = (xr*xr) << 1;
                int yrSqTwo = (yr*yr) << 1;
                int xChg = yr*yr*(1 - (xr << 1));
                int yChg = xr*xr;
                int err = 0;
                int xStopping = yrSqTwo*xr;
                int yStopping = 0;

                // Draw first set of points counter clockwise where tangent line slope > -1.
                while (xStopping >= yStopping)
                {
                    // Draw 4 quadrant points at once
                    uy = yc + y; // Upper half
                    ly = yc - y; // Lower half
                    if (uy < 0) uy = 0; // Clip
                    if (uy >= h) uy = h - 1; // ...
                    if (ly < 0) ly = 0;
                    if (ly >= h) ly = h - 1;
                    uh = uy*w; // Upper half
                    lh = ly*w; // Lower half

                    rx = xc + x;
                    lx = xc - x;
                    if (rx < 0) rx = 0; // Clip
                    if (rx >= w) rx = w - 1; // ...
                    if (lx < 0) lx = 0;
                    if (lx >= w) lx = w - 1;
                    pixels[rx + uh] = color; // Quadrant I (Actually an octant)
                    pixels[lx + uh] = color; // Quadrant II
                    pixels[lx + lh] = color; // Quadrant III
                    pixels[rx + lh] = color; // Quadrant IV

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
                if (uy < 0) uy = 0; // Clip
                if (uy >= h) uy = h - 1; // ...
                if (ly < 0) ly = 0;
                if (ly >= h) ly = h - 1;
                uh = uy*w; // Upper half
                lh = ly*w; // Lower half
                xChg = yr*yr;
                yChg = xr*xr*(1 - (yr << 1));
                err = 0;
                xStopping = 0;
                yStopping = xrSqTwo*yr;

                // Draw second set of points clockwise where tangent line slope < -1.
                while (xStopping <= yStopping)
                {
                    // Draw 4 quadrant points at once
                    rx = xc + x;
                    lx = xc - x;
                    if (rx < 0) rx = 0; // Clip
                    if (rx >= w) rx = w - 1; // ...
                    if (lx < 0) lx = 0;
                    if (lx >= w) lx = w - 1;
                    pixels[rx + uh] = color; // Quadrant I (Actually an octant)
                    pixels[lx + uh] = color; // Quadrant II
                    pixels[lx + lh] = color; // Quadrant III
                    pixels[rx + lh] = color; // Quadrant IV

                    x++;
                    xStopping += yrSqTwo;
                    err += xChg;
                    xChg += yrSqTwo;
                    if ((yChg + (err << 1)) > 0)
                    {
                        y--;
                        uy = yc + y; // Upper half
                        ly = yc - y; // Lower half
                        if (uy < 0) uy = 0; // Clip
                        if (uy >= h) uy = h - 1; // ...
                        if (ly < 0) ly = 0;
                        if (ly >= h) ly = h - 1;
                        uh = uy*w; // Upper half
                        lh = ly*w; // Lower half
                        yStopping -= xrSqTwo;
                        err += yChg;
                        yChg += xrSqTwo;
                    }
                }
#if !SILVERLIGHT
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        #endregion

        #endregion

        #endregion
    }
}