using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEditXNA.Terraria;

namespace TEditXna.Render
{
    public class RenderMiniMap
    {
        public static int Resolution = 20;

        public static WriteableBitmap Render(World w)
        {
            WriteableBitmap bmp = new WriteableBitmap(w.TilesWide / Resolution, w.TilesHigh / Resolution, 96, 96, PixelFormats.Bgra32, null);

            UpdateMinimap(w, ref bmp);

            return bmp;
        }

        private static int XnaColorToWindowsInt(Microsoft.Xna.Framework.Color color)
        {
            byte a = color.A;
            byte b = color.B;
            byte g = color.G;
            byte r = color.R;

            int xnacolor = (a << 24)
          | ((byte)((r * a) >> 8) << 16)
          | ((byte)((g * a) >> 8) << 8)
          | ((byte)((b * a) >> 8));

            return xnacolor;
        }

        public static void UpdateMinimap(World w, ref WriteableBitmap bmp)
        {
            bmp.Lock();
            unsafe
            {
                int pixelCount = bmp.PixelHeight * bmp.PixelWidth;
                var pixels = (int*)bmp.BackBuffer;

                for (int i = 0; i < pixelCount; i++)
                {
                    int x = i % bmp.PixelWidth;
                    int y = i / bmp.PixelWidth;

                    int worldX = x * Resolution;
                    int worldY = y * Resolution;



                    pixels[i] = XnaColorToWindowsInt(PixelMap.GetTileColor(w.Tiles[worldX, worldY], Microsoft.Xna.Framework.Color.Transparent));
                }
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
            bmp.Unlock();
        }
    }
}