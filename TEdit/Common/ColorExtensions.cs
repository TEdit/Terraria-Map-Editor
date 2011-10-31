using System;

namespace TEdit.Common.Structures
{
    // All of these colors are practically all the same; just treat it as one unified class
    public class Color
    {
        private System.Windows.Media.Color _color = new System.Windows.Media.Color();

        // Method remappings
        public byte A {
            get { return _color.A; }
            set { _color.A = value; }
        }
        public byte R {
            get { return _color.R; }
            set { _color.R = value; }
        }
        public byte G {
            get { return _color.G; }
            set { _color.G = value; }
        }
        public byte B {
            get { return _color.B; }
            set { _color.B = value; }
        }

        public static Color Add     (Color c1, Color c2) { return System.Windows.Media.Color.Add(c1, c2); }
        public static Color Subtract(Color c1, Color c2) { return System.Windows.Media.Color.Subtract(c1, c2); }
        public static bool  AreClose(Color c1, Color c2) { return System.Windows.Media.Color.AreClose(c1, c2); }

        public void Clamp() { _color.Clamp(); }

        public bool Equals(Color c)  { return _color.Equals((System.Windows.Media.Color)c); }
        public override bool Equals(Object o) { return _color.Equals(o); }
        public static bool Equals(Color c1, Color c2) { return System.Windows.Media.Color.Equals((System.Windows.Media.Color)c1, (System.Windows.Media.Color)c2); }

        public static Color FromArgb(byte a, byte r, byte g, byte b) { return System.Windows.Media.Color.FromArgb(a, r, g, b); }
        public static Color FromArgb(int i)                          { return System.Drawing.Color.FromArgb(i); }
        public static Color FromArgb(int i, Color c)                 { return System.Drawing.Color.FromArgb(i, c); }
        public static Color FromArgb(int r, int g, int b)            { return System.Drawing.Color.FromArgb(r, g, b); }

        public static Color FromAValues(float a, float[] values, Uri profileUri) { return System.Windows.Media.Color.FromAValues(a, values, profileUri); }
        public static Color FromValues (         float[] values, Uri profileUri) { return System.Windows.Media.Color.FromValues    (values, profileUri); }

        public static Color FromKnownColor(System.Drawing.KnownColor color) { return System.Drawing.Color.FromKnownColor(color); }
        public static Color FromName      (string name)                     { return System.Drawing.Color.FromName(name);        }

        public static Color FromNonPremultiplied(int r, int g, int b, int a)             { return Microsoft.Xna.Framework.Color.FromNonPremultiplied(r, g, b, a); }
        public static Color FromNonPremultiplied(Microsoft.Xna.Framework.Vector4 vector) { return Microsoft.Xna.Framework.Color.FromNonPremultiplied(vector);     }

        public static Color FromRgb(byte r, byte g, byte b) { return System.Windows.Media.Color.FromRgb(r, g, b); }
        
        public static Color FromScRgb(float a, float r, float g, float b) { return System.Windows.Media.Color.FromScRgb(a, r, g, b); }

        public float GetBrightness() { System.Drawing.Color c = (Color)_color; return c.GetBrightness(); }
        public float GetHue()        { System.Drawing.Color c = (Color)_color; return c.GetHue();        }
        public float GetSaturation() { System.Drawing.Color c = (Color)_color; return c.GetSaturation(); }

        public static Color Lerp(Color value1, Color value2, float amount) { return Microsoft.Xna.Framework.Color.Lerp(value1, value2, amount); }

        public float ToArgb() { System.Drawing.Color c = (Color)_color; return c.ToArgb(); }
        public System.Drawing.KnownColor ToKnownColor() { System.Drawing.Color c = (Color)_color; return c.ToKnownColor(); }

        public override int GetHashCode() { return _color.GetHashCode(); }

        public static Color Multiply(Color color, float coefficient) { return System.Windows.Media.Color.Multiply(color, coefficient); }

        public override string ToString() { return _color.ToString(); }

        public Microsoft.Xna.Framework.Vector3 ToVector3() { Microsoft.Xna.Framework.Color c = (Color)_color; return c.ToVector3(); }
        public Microsoft.Xna.Framework.Vector4 ToVector4() { Microsoft.Xna.Framework.Color c = (Color)_color; return c.ToVector4(); }

        // Constructors
        public Color() { }
        public Color(System.Windows.Media.Color c) { _color = c; }
        // everything else magically converts via implicit operators
        public Color(byte r, byte g, byte b)             { _color = System.Windows.Media.Color.FromRgb(r, g, b); }
        public Color(byte r, byte g, byte b, byte a)     { _color = System.Windows.Media.Color.FromArgb(a, r, g, b); }
        public Color(float r, float g, float b)          { _color = (Color)new Microsoft.Xna.Framework.Color(r, g, b);    }
        public Color(float r, float g, float b, float a) { _color = (Color)new Microsoft.Xna.Framework.Color(r, g, b, a); }
        public Color(Microsoft.Xna.Framework.Vector3 v)  { _color = (Color)new Microsoft.Xna.Framework.Color(v); }
        public Color(Microsoft.Xna.Framework.Vector4 v)  { _color = (Color)new Microsoft.Xna.Framework.Color(v); }

        // Equality
        private static bool MatchFields(Color a, Color m) {
            return (a.A == m.A && a.R == m.R && a.G == m.G && a.B == m.B);
        }
        public static bool operator ==(Color a, Color b) {
            return MatchFields(a, b);
        }
        public static bool operator !=(Color a, Color b) {
            return !(a == b);
        }

        // Implicit magic
        public static implicit operator Color(System.Windows.Media.Color c) {
            return new Color(c.R, c.G, c.B, c.A);
        }
        public static implicit operator System.Windows.Media.Color(Color c) {
            return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static implicit operator Color(System.Drawing.Color c) {
            return new Color(c.R, c.G, c.B, c.A);
        }
        public static implicit operator System.Drawing.Color(Color c) {
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static implicit operator Color(Microsoft.Xna.Framework.Color c) {
            return new Color(c.R, c.G, c.B, c.A);
        }
        public static implicit operator Microsoft.Xna.Framework.Color(Color c) {
            return new Microsoft.Xna.Framework.Color(c.R, c.G, c.B, c.A);
        }

        // "Extensions"
        public Color AlphaBlend(Color color, bool selfIsBack = true) {
            return selfIsBack ? AlphaBlend(_color, color) : AlphaBlend(color, _color);
        }
        public static Color AlphaBlend(Color background, Color color) {
            // short-circuits
            if (color.A == 0)   return background;
            if (color.A == 255) return color;

            var r = (byte)((color.A/255F)*color.R + (1F - color.A/255F)*background.R);
            var g = (byte)((color.A/255F)*color.G + (1F - color.A/255F)*background.G);
            var b = (byte)((color.A/255F)*color.B + (1F - color.A/255F)*background.B);
            return Color.FromArgb(255, r, g, b);
        }
    }
}