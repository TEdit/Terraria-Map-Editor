/*
	Copyright (C) 2008 Cory Nelson

	This software is provided 'as-is', without any express or implied
	warranty.  In no event will the authors be held liable for any damages
	arising from the use of this software.

	Permission is granted to anyone to use this software for any purpose,
	including commercial applications, and to alter it and redistribute it
	freely, subject to the following restrictions:

	1. The origin of this software must not be misrepresented; you must not
		claim that you wrote the original software. If you use this software
		in a product, an acknowledgment in the product documentation would be
		appreciated but is not required.
	2. Altered source versions must be plainly marked as such, and must not be
		misrepresented as being the original software.
	3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Windows.Markup;
using System.Windows.Media;

namespace TEdit.UI.Xaml
{
    [MarkupExtensionReturnType(typeof(Brush))]
    class HslBrushExtension : HslColorExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var brush = new SolidColorBrush(GetColor());

            brush.Freeze();
            return brush;
        }
    }

    [MarkupExtensionReturnType(typeof(Color))]
    class HslColorExtension : MarkupExtension
    {
        /// <summary>
        /// Hue (0.0 - 360.0)
        /// </summary>
        public double H { get; set; }

        /// <summary>
        /// Saturation (0.0 - 100.0)
        /// </summary>
        public double S { get; set; }

        /// <summary>
        /// Lightness (0.0 - 100.0)
        /// </summary>
        public double L { get; set; }

        /// <summary>
        /// Alpha (0.0 - 100.0)
        /// </summary>
        public double A { get; set; }

        public HslColorExtension()
        {
            H = 0.0;
            S = 0.0;
            L = 0.0;
            A = 100.0;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return GetColor();
        }

        protected Color GetColor()
        {
            // convert HSL to RGB

            double h = Math.Min(Math.Max(0.0, H * (1.0 / 360.0)), 1.0);
            double s = Math.Min(Math.Max(0.0, S * (1.0 / 100.0)), 1.0);
            double l = Math.Min(Math.Max(0.0, L * (1.0 / 100.0)), 1.0);
            float a = Math.Min(Math.Max(0.0f, (float)(A * (1.0 / 100.0))), 1.0f);

            double r, g, b;

            if (s < double.Epsilon)
            {
                r = l;
                g = l;
                b = l;
            }
            else
            {
                double tmp1, tmp2;

                if (l < 0.5) tmp2 = l * (1.0 + s);
                else tmp2 = (l + s) - (l * s);

                tmp1 = 2.0 * l - tmp2;

                r = h + 1.0 / 3.0;
                if (r > 1.0) --r;

                g = h;

                b = h - 1.0 / 3.0;
                if (b < 0.0) ++b;

                r = GetRGB(r, tmp1, tmp2);
                g = GetRGB(g, tmp1, tmp2);
                b = GetRGB(b, tmp1, tmp2);
            }

            // convert RGB to scRGB so we get a higher precision.

            float scr = GetScRGB(r);
            float scg = GetScRGB(g);
            float scb = GetScRGB(b);

            // return the final color.
            return Color.FromScRgb(a, scr, scg, scb);
        }

        static double GetRGB(double v, double tmp1, double tmp2)
        {
            if (v < 1.0 / 6.0)
                return tmp1 + (tmp2 - tmp1) * 6.0f * v;

            if (v < 0.5)
                return tmp2;

            if (v < 2.0 / 3.0)
                return tmp1 + (tmp2 - tmp1) * ((2.0 / 3.0) - v) * 6.0;

            return tmp1;
        }

        static float GetScRGB(double rgb)
        {
            if (rgb < double.Epsilon)
                return 0.0f;

            if (rgb <= 0.04045)
                return (float)(rgb * (1.0 / 12.92));

            if (rgb < 1.0)
                return (float)Math.Pow((rgb + 0.055) * (1.0 / 1.055), 2.4);

            return 1.0f;
        }
    }
}