using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Terraria;

namespace TEdit.Render;

public class RenderMiniMap
{
    public static int Resolution { get; set; } = 20;

    public static WriteableBitmap Render(World w)
    {
        // scale the minimap appropriatly
        // UI is 300x100 px
        var maxX = (int)Math.Ceiling(w.TilesWide / 300.0);
        var maxY = (int)Math.Ceiling(w.TilesHigh / 100.0);

        Resolution = Math.Max(maxX, maxY);

        WriteableBitmap bmp = new WriteableBitmap(w.TilesWide / Resolution, w.TilesHigh / Resolution, 96, 96, PixelFormats.Bgra32, null);

        UpdateMinimap(w, ref bmp);

        return bmp;
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
}
