using System.Windows.Media;
using TEdit.Common;

namespace TEdit.Render;

public static class ColorConvert
{
    private static TEditColor ColorFromString(string colorstring)
    {
        if (!string.IsNullOrWhiteSpace(colorstring))
        {
            var colorFromString = ColorConverter.ConvertFromString(colorstring);
            if (colorFromString != null)
            {
                var c = (Color)colorFromString;
                return TEditColor.FromNonPremultiplied(c.R, c.G, c.B, c.A);
            }
        }
        return TEditColor.Magenta;
    }

    private static Microsoft.Xna.Framework.Color XnaColorFromString(string colorstring)
    {
        if (!string.IsNullOrWhiteSpace(colorstring))
        {
            var colorFromString = ColorConverter.ConvertFromString(colorstring);
            if (colorFromString != null)
            {
                var c = (Color)colorFromString;
                return Microsoft.Xna.Framework.Color.FromNonPremultiplied(c.R, c.G, c.B, c.A);
            }
        }
        return Microsoft.Xna.Framework.Color.Magenta;
    }
}
