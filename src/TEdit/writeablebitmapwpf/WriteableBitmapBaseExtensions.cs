#region Header

//
//   Project:           WriteableBitmapEx - Silverlight WriteableBitmap extensions
//   Description:       Collection of draw extension methods for the Silverlight WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2010-09-17 22:20:49 +0200 (Pt, 17 wrz 2010) $
//   Changed in:        $Revision: 61101 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapBaseExtensions.cs $
//   Id:                $Id: WriteableBitmapBaseExtensions.cs 61101 2010-09-17 20:20:49Z unknown $
//
//
//   Copyright © 2009-2010 Rene Schulte and WriteableBitmapEx Contributors
//
//   This Software is weak copyleft open source. Please read the License.txt for details.
//

#endregion

using System.Collections.Generic;

namespace System.Windows.Media.Imaging
{
    /// <summary>
    /// Collection of draw extension methods for the Silverlight WriteableBitmap class.
    /// </summary>
    public static partial class WriteableBitmapExtensions
    {
        #region Fields

        private const int SizeOfArgb = 4;

        #endregion

        #region Methods

        #region General

        /// <summary>
        /// Fills the whole WriteableBitmap with a color.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="color">The color used for filling.</param>
        public static void Clear(this WriteableBitmap bmp, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int col = (color.A << 24)
                      | ((byte) ((color.R*a) >> 8) << 16)
                      | ((byte) ((color.G*a) >> 8) << 8)
                      | ((byte) ((color.B*a) >> 8));

            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
#if SILVERLIGHT
         var pixels = bmp.Pixels;
         var len = w*SizeOfArgb;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                int len = bmp.BackBufferStride;
#endif

                // Fill first line
                for (int x = 0; x < w; x++)
                {
                    pixels[x] = col;
                }

                // Copy first line
                int blockHeight = 1;
                int y = 1;
                while (y < h)
                {
#if SILVERLIGHT
                 Buffer.BlockCopy(pixels, 0, pixels, y*len, blockHeight*len);
#else
                    NativeMethods.CopyUnmanagedMemory((IntPtr) pixels, 0, (IntPtr) pixels, y*len, blockHeight*len);
#endif
                    y += blockHeight;
                    blockHeight = Math.Min(2*blockHeight, h - y);
                }

#if !SILVERLIGHT
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Fills the whole WriteableBitmap with an empty color (0).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        public static void Clear(this WriteableBitmap bmp)
        {
#if SILVERLIGHT
          Array.Clear(bmp.Pixels, 0, bmp.Pixels.Length);
#else
            bmp.Lock();
            NativeMethods.SetUnmanagedMemory(bmp.BackBuffer, 0, bmp.BackBufferStride*bmp.PixelHeight);
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Clones the specified WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <returns>A copy of the WriteableBitmap.</returns>
        public static WriteableBitmap Clone(this WriteableBitmap bmp)
        {
#if SILVERLIGHT
         var result = new WriteableBitmap(bmp.PixelWidth, bmp.PixelHeight);
         Buffer.BlockCopy(bmp.Pixels, 0, result.Pixels, 0, bmp.Pixels.Length * SizeOfArgb);
         return result;
#else
            var result = new WriteableBitmap(bmp);
            bmp.WritePixels(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight), bmp.BackBuffer,
                            bmp.PixelWidth*bmp.PixelHeight*SizeOfArgb, bmp.BackBufferStride);
            return result;
#endif
        }

        #endregion

        #region ForEach

        /// <summary>
        /// Applies the given function to all the pixels of the bitmap in 
        /// order to set their color.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="func">The function to apply. With parameters x, y and a color as a result</param>
        public static void ForEach(this WriteableBitmap bmp, Func<int, int, Color> func)
        {
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
                int index = 0;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Color color = func(x, y);
                        // Add one to use mul and cheap bit shift for multiplicaltion
                        int a = color.A + 1;
                        pixels[index++] = (color.A << 24)
                                          | ((byte) ((color.R*a) >> 8) << 16)
                                          | ((byte) ((color.G*a) >> 8) << 8)
                                          | ((byte) ((color.B*a) >> 8));
                    }
                }
#if !SILVERLIGHT
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, w, h));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Applies the given function to all the pixels of the bitmap in 
        /// order to set their color.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="func">The function to apply. With parameters x, y, source color and a color as a result</param>
        public static void ForEach(this WriteableBitmap bmp, Func<int, int, Color, Color> func)
        {
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
                int index = 0;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int c = pixels[index];
                        Color color = func(x, y,
                                           Color.FromArgb((byte) (c >> 24), (byte) (c >> 16), (byte) (c >> 8),
                                                          (byte) (c)));
                        // Add one to use mul and cheap bit shift for multiplicaltion
                        int a = color.A + 1;
                        pixels[index++] = (color.A << 24)
                                          | ((byte) ((color.R*a) >> 8) << 16)
                                          | ((byte) ((color.G*a) >> 8) << 8)
                                          | ((byte) ((color.B*a) >> 8));
                    }
                }
#if !SILVERLIGHT
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, w, h));
            bmp.Unlock();
#endif
        }

        #endregion

        #region Get Pixel / Brightness

        /// <summary>
        /// Gets the color of the pixel at the x, y coordinate as integer.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate of the pixel.</param>
        /// <param name="y">The y coordinate of the pixel.</param>
        /// <returns>The color of the pixel at x, y.</returns>
        public static int GetPixeli(this WriteableBitmap bmp, int x, int y)
        {
#if SILVERLIGHT
          return bmp.Pixels[y * bmp.PixelWidth + x];
#else
            int c;
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                c = pixels[y*bmp.BackBufferStride/SizeOfArgb + x];
            }
            bmp.Unlock();
            return c;
#endif
        }

        /// <summary>
        /// Gets the color of the pixel at the x, y coordinate as a Color struct.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate of the pixel.</param>
        /// <param name="y">The y coordinate of the pixel.</param>
        /// <returns>The color of the pixel at x, y as a Color struct.</returns>
        public static Color GetPixel(this WriteableBitmap bmp, int x, int y)
        {
#if SILVERLIGHT
          var c = bmp.Pixels[y * bmp.PixelWidth + x];
#else
            int c;
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                c = pixels[y*bmp.BackBufferStride/SizeOfArgb + x];
            }
            bmp.Unlock();
#endif
            var a = (byte) (c >> 24);

            // Prevent division by zero
            int ai = a;
            if (ai == 0)
            {
                ai = 1;
            }

            // Scale inverse alpha to use cheap integer mul bit shift
            ai = ((255 << 8)/ai);
            return Color.FromArgb(a,
                                  (byte) ((((c >> 16) & 0xFF)*ai) >> 8),
                                  (byte) ((((c >> 8) & 0xFF)*ai) >> 8),
                                  (byte) ((((c & 0xFF)*ai) >> 8)));
        }

        /// <summary>
        /// Gets the brightness / luminance of the pixel at the x, y coordinate as byte.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate of the pixel.</param>
        /// <param name="y">The y coordinate of the pixel.</param>
        /// <returns>The brightness of the pixel at x, y.</returns>
        public static byte GetBrightness(this WriteableBitmap bmp, int x, int y)
        {
#if SILVERLIGHT
          var c = bmp.Pixels[y * bmp.PixelWidth + x];
#else
            int c;
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                c = pixels[y*bmp.BackBufferStride/SizeOfArgb + x];
            }
            bmp.Unlock();
#endif
            // Extract color components
            var r = (byte) (c >> 16);
            var g = (byte) (c >> 8);
            var b = (byte) (c);

            // Convert to gray with constant factors 0.2126, 0.7152, 0.0722
            return (byte) ((r*6966 + g*23436 + b*2366) >> 15);
        }

        #endregion

        #region SetPixel

        #region Without alpha

        /// <summary>
        /// Sets the color of the pixel using a precalculated index (faster).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="r">The red value of the color.</param>
        /// <param name="g">The green value of the color.</param>
        /// <param name="b">The blue value of the color.</param>
        public static void SetPixeli(this WriteableBitmap bmp, int index, byte r, byte g, byte b)
        {
            int argb = (255 << 24) | (r << 16) | (g << 8) | b;
#if SILVERLIGHT
         bmp.Pixels[index] = argb;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                pixels[index] = argb;
            }
            //TODO: compute dirty rect
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Sets the color of the pixel.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="r">The red value of the color.</param>
        /// <param name="g">The green value of the color.</param>
        /// <param name="b">The blue value of the color.</param>
        public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte r, byte g, byte b)
        {
            int argb = (255 << 24) | (r << 16) | (g << 8) | b;
#if SILVERLIGHT
          bmp.Pixels[y * bmp.PixelWidth + x] = argb;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                pixels[y*bmp.BackBufferStride/SizeOfArgb + x] = argb;
            }
            bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            bmp.Unlock();
#endif
        }

        #endregion

        #region With alpha

        /// <summary>
        /// Sets the color of the pixel including the alpha value and using a precalculated index (faster).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="a">The alpha value of the color.</param>
        /// <param name="r">The red value of the color.</param>
        /// <param name="g">The green value of the color.</param>
        /// <param name="b">The blue value of the color.</param>
        public static void SetPixeli(this WriteableBitmap bmp, int index, byte a, byte r, byte g, byte b)
        {
            int argb = (a << 24) | (r << 16) | (g << 8) | b;
#if SILVERLIGHT
          bmp.Pixels[index] = argb;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                pixels[index] = argb;
            }
            //TODO: compute dirty rect
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }


        /// <summary>
        /// Sets the color of the pixel including the alpha value.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="a">The alpha value of the color.</param>
        /// <param name="r">The red value of the color.</param>
        /// <param name="g">The green value of the color.</param>
        /// <param name="b">The blue value of the color.</param>
        public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte a, byte r, byte g, byte b)
        {
            int argb = (a << 24) | (r << 16) | (g << 8) | b;
#if SILVERLIGHT
          bmp.Pixels[y * bmp.PixelWidth + x] = argb;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                pixels[y*bmp.BackBufferStride/SizeOfArgb + x] = argb;
            }
            bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            bmp.Unlock();
#endif
        }

        #endregion

        #region With System.Windows.Media.Color

        /// <summary>
        /// Sets the color of the pixel using a precalculated index (faster).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="color">The color.</param>
        public static void SetPixeli(this WriteableBitmap bmp, int index, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int argb = (color.A << 24)
                       | ((byte) ((color.R*a) >> 8) << 16)
                       | ((byte) ((color.G*a) >> 8) << 8)
                       | ((byte) ((color.B*a) >> 8));
#if SILVERLIGHT
          bmp.Pixels[index] = argb;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                pixels[index] = argb;
            }
            //TODO: compute dirty rect
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Sets the color of the pixel.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="color">The color.</param>
        public static void SetPixel(this WriteableBitmap bmp, int x, int y, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int a = color.A + 1;
            int argb = (color.A << 24)
                       | ((byte) ((color.R*a) >> 8) << 16)
                       | ((byte) ((color.G*a) >> 8) << 8)
                       | ((byte) ((color.B*a) >> 8));
#if SILVERLIGHT
          bmp.Pixels[y * bmp.PixelWidth + x] = argb;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                pixels[y*bmp.BackBufferStride/SizeOfArgb + x] = argb;
            }
            bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Sets the color of the pixel using an extra alpha value and a precalculated index (faster).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="a">The alpha value of the color.</param>
        /// <param name="color">The color.</param>
        public static void SetPixeli(this WriteableBitmap bmp, int index, byte a, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int ai = a + 1;
            int argb = (a << 24)
                       | ((byte) ((color.R*ai) >> 8) << 16)
                       | ((byte) ((color.G*ai) >> 8) << 8)
                       | ((byte) ((color.B*ai) >> 8));
#if SILVERLIGHT
          bmp.Pixels[index] = argb;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                pixels[index] = argb;
            }
            //TODO: compute dirty rect
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Sets the color of the pixel using an extra alpha value.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="a">The alpha value of the color.</param>
        /// <param name="color">The color.</param>
        public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte a, Color color)
        {
            // Add one to use mul and cheap bit shift for multiplicaltion
            int ai = a + 1;
            int argb = (a << 24)
                       | ((byte) ((color.R*ai) >> 8) << 16)
                       | ((byte) ((color.G*ai) >> 8) << 8)
                       | ((byte) ((color.B*ai) >> 8));
#if SILVERLIGHT
          bmp.Pixels[y * bmp.PixelWidth + x] = argb;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                pixels[y*bmp.BackBufferStride/SizeOfArgb + x] = argb;
            }
            bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Sets the color of the pixel using a precalculated index (faster).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="color">The color.</param>
        public static void SetPixeli(this WriteableBitmap bmp, int index, int color)
        {
#if SILVERLIGHT
          bmp.Pixels[index] = color;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                pixels[index] = color;
            }
            //TODO: compute dirty rect
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
#endif
        }

        /// <summary>
        /// Sets the color of the pixel.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="color">The color.</param>
        public static void SetPixel(this WriteableBitmap bmp, int x, int y, int color)
        {
#if SILVERLIGHT
          bmp.Pixels[y * bmp.PixelWidth + x] = color;
#else
            bmp.Lock();
            unsafe
            {
                var pixels = (int*) bmp.BackBuffer;
                pixels[y*bmp.BackBufferStride/SizeOfArgb + x] = color;
            }
            bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            bmp.Unlock();
#endif
        }

        #endregion

        #endregion

        /// <summary>
        /// Get the points to generate a "thick" line
        /// </summary>
        /// <param name="radius">Width of the line</param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="endMode">Line end mode, 0 = flat, 1 = point, 2= round, 3 = arrow</param>
        /// <returns></returns>
        public static int[] GetThickLinePolyPoints(int radius, int x1, int y1, int x2, int y2, int endRoundness = 1)
        {
            var polypoints = new List<int>();

            int dX = x2 - x1;
            int dY = y2 - y1;
            double dL = Math.Sqrt(dX*dX + dY*dY);
            double ratio = radius/dL;
            int eX = -(int) (ratio*dY);
            var eY = (int) (ratio*dX);

            endRoundness = Math.Max(endRoundness, 1);

            double endRoundnessAngle = Math.PI/endRoundness;

            for (int i = 0; i <= endRoundness; i++)
            {
                double theta = -endRoundnessAngle*i;
                Vector arc = RotateArc(new Vector(-eX, -eY), theta);
                polypoints.Add((int) arc.X + x1);
                polypoints.Add((int) arc.Y + y1);
            }

            for (int i = 0; i <= endRoundness; i++)
            {
                double theta = -endRoundnessAngle*i;
                Vector arc = RotateArc(new Vector(eX, eY), theta);
                polypoints.Add((int) arc.X + x2);
                polypoints.Add((int) arc.Y + y2);
            }

            polypoints.Add(polypoints[0]);
            polypoints.Add(polypoints[1]);

            return polypoints.ToArray();
        }

        public static Vector RotateArc(Vector arc, double angle)
        {
            var rotated = new Vector
                              {
                                  X = arc.X*Math.Cos(angle) - arc.Y*Math.Sin(angle),
                                  Y = arc.X*Math.Sin(angle) + arc.Y*Math.Cos(angle)
                              };
            return rotated;
        }

        public static Vector RotatePoint(Vector point, Vector origin, float angle)
        {
            Vector translated = point - origin;
            var rotated = new Vector
                              {
                                  X = translated.X*Math.Cos(angle) - translated.Y*Math.Sin(angle),
                                  Y = translated.X*Math.Sin(angle) - translated.Y*Math.Cos(angle)
                              };
            return rotated + origin;
        }

        public static Vector RotatePoint(int x, int y, float angle)
        {
            var rotated = new Vector
                              {
                                  X = x*Math.Cos(angle) - y*Math.Sin(angle),
                                  Y = x*Math.Sin(angle) - y*Math.Cos(angle)
                              };
            return rotated;
        }

        #endregion
    }
}