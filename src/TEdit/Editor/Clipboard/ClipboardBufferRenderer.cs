using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Utility;

namespace TEdit.Editor.Clipboard;

public class ClipboardBufferRenderer
{
    public static int ClipboardRenderSize = 512;

    public static WriteableBitmap RenderBuffer(ClipboardBuffer buffer, out double RenderScale)
    {
        int previewX = buffer.Size.X;
        int previewY = buffer.Size.Y;

        double scale = Math.Max((double)buffer.Size.X / ClipboardRenderSize, (double)buffer.Size.Y / ClipboardRenderSize);


        if (scale > 1.0)
        {
            previewX = (int)Calc.Clamp((float)Math.Min(ClipboardRenderSize, buffer.Size.X / scale), 1, ClipboardRenderSize);
            previewY = (int)Calc.Clamp((float)Math.Min(ClipboardRenderSize, buffer.Size.Y / scale), 1, ClipboardRenderSize);
        }
        else
            scale = 1;

        var bmp = new WriteableBitmap(previewX, previewY, 96, 96, PixelFormats.Bgra32, null);
        for (int x = 0; x < previewX; x++)
        {
            int tileX = (int)Calc.Clamp((float)(scale * x), 0, buffer.Size.X - 1);

            for (int y = 0; y < previewY; y++)
            {
                int tileY = (int)Calc.Clamp((float)(scale * y), 0, buffer.Size.Y - 1);

                var color = Render.PixelMap.GetTileColor(buffer.Tiles[tileX, tileY], Microsoft.Xna.Framework.Color.Transparent);
                bmp.SetPixel(x, y, color.A, color.R, color.G, color.B);
            }
        }
        RenderScale = scale;
        return bmp;
    }
}
