using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaMapEditor.Common
{
    public static class Utility
    {
        public static bool IsNumeric(string val, System.Globalization.NumberStyles numberStyle)
        {
            Int32 result;
            return Int32.TryParse(val, numberStyle,
                System.Globalization.CultureInfo.CurrentCulture, out result);
        }

        public static System.Drawing.Color AlphaBlend(System.Drawing.Color background, System.Drawing.Color color)
        {
            int r = (int)((color.A / 255F) * (float)color.R + (1F - color.A / 255F) * (float)background.R);
            int g = (int)((color.A / 255F) * (float)color.G + (1F - color.A / 255F) * (float)background.G);
            int b = (int)((color.A / 255F) * (float)color.B + (1F - color.A / 255F) * (float)background.B);
            return System.Drawing.Color.FromArgb(r, g, b);
        }


        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp =
                  typeof(System.Windows.Forms.Control).GetProperty(
                        "DoubleBuffered",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }
    }
}
