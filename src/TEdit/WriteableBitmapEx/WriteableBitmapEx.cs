using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace System.Windows.Media.Imaging;

public static class WriteableBitmapEx
{
    public static Texture2D ResourceToTexture2D(string resource, GraphicsDevice gd)
    {
        using (Stream stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resource))
        {
            return Texture2D.FromStream(gd, stream);
        }
    }

    public static WriteableBitmap Texture2DToWriteableBitmap(this Texture2D texture)
    {
        var bmp = new WriteableBitmap(texture.Width, texture.Height, 96, 96, PixelFormats.Bgra32, null);
        var pixelData = new int[texture.Width * texture.Height];
        texture.GetData(pixelData);
        //bmp.WritePixels();
        bmp.Lock();
        unsafe
        {
            var pixels = (int*)bmp.BackBuffer;
            for (int i = 0; i < pixelData.Length; i++)
            {
                pixels[i] = ColorToWindows(pixelData[i]);
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, texture.Width, texture.Height));
        bmp.Unlock();
        return bmp;
    }

    public static Texture2D ToTexture2D(this WriteableBitmap bmp, GraphicsDevice gd)
    {
        var result = new Texture2D(gd, bmp.PixelWidth, bmp.PixelHeight);
        var pixelData = new int[bmp.PixelWidth * bmp.PixelHeight];

        bool locked = false;
        if (!bmp.IsFrozen)
        {
            try { bmp.Lock(); locked = true; }
            catch (InvalidOperationException) { }
        }

        if (locked || !bmp.IsFrozen)
        {
            unsafe
            {
                var pixels = (int*)bmp.BackBuffer;
                for (int i = 0; i < pixelData.Length; i++)
                {
                    pixelData[i] = ColorToXna(pixels[i]);
                }
            }
            if (locked) { bmp.Unlock(); }
        }
        else
        {
            // Frozen bitmap — use safe copy path
            var rawPixels = new int[pixelData.Length];
            bmp.CopyPixels(rawPixels, bmp.PixelWidth * 4, 0);
            for (int i = 0; i < rawPixels.Length; i++)
            {
                pixelData[i] = ColorToXna(rawPixels[i]);
            }
        }

        result.SetData(pixelData);
        return result;
    }

    private static int ColorToXna(int color)
    {
        byte a = (byte)(color >> 24);
        byte r = (byte)(color >> 16);
        byte g = (byte)(color >> 8);
        byte b = (byte)(color >> 0);

        int xnacolor = (a << 24)
      | ((byte)((b * a) >> 8) << 16)
      | ((byte)((g * a) >> 8) << 8)
      | ((byte)((r * a) >> 8));

        return xnacolor;
    }

    private static int ColorToWindows(int color)
    {
        byte a = (byte)(color >> 24);
        byte b = (byte)(color >> 16);
        byte g = (byte)(color >> 8);
        byte r = (byte)(color >> 0);

        int xnacolor = (a << 24)
      | ((byte)((r * a) >> 8) << 16)
      | ((byte)((g * a) >> 8) << 8)
      | ((byte)((b * a) >> 8));

        return xnacolor;
    }

}
