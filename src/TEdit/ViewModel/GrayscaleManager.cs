using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TEdit.ViewModel
{
    public static class GrayscaleManager
    {
        /// <summary>
        /// Creates a Texture2D filled with a single color (including transparent).
        /// </summary>
        public static Texture2D CreateSolidTexture(GraphicsDevice device, int width, int height, Color color)
        {
            var tex = new Texture2D(device, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++) data[i] = color;
            tex.SetData(data);
            return tex;
        }

        public static Microsoft.Xna.Framework.Color ToGrayscale(Microsoft.Xna.Framework.Color c)
        {
            // Calculate luminance
            byte gray = (byte)(c.R * 0.299 + c.G * 0.587 + c.B * 0.114);
            return new Microsoft.Xna.Framework.Color(gray, gray, gray, c.A);
        }

        public static class GrayscaleCache
        {
            // Key: (Texture2D, Rectangle)
            private static readonly Dictionary<(Texture2D, Rectangle), Texture2D> _cache = [];

            public static Texture2D GetOrCreate(GraphicsDevice device, Texture2D source, Rectangle sourceRect)
            {
                var key = (source, sourceRect);
                if (_cache.TryGetValue(key, out var grayTex))
                    return grayTex;
                // Create grayscale
                grayTex = ToGrayscale(device, source, sourceRect);
                _cache[key] = grayTex;
                return grayTex;
            }

            public static void Clear()
            {
                foreach (var tex in _cache.Values)
                    tex.Dispose();
                _cache.Clear();
            }

            private static Texture2D ToGrayscale(GraphicsDevice device, Texture2D source, Rectangle sourceRect)
            {
                Color[] data = new Color[sourceRect.Width * sourceRect.Height];
                source.GetData(0, sourceRect, data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                {
                    byte gray = (byte)(data[i].R * 0.299 + data[i].G * 0.587 + data[i].B * 0.114);
                    data[i] = new Color(gray, gray, gray, data[i].A);
                }
                Texture2D grayTex = new(device, sourceRect.Width, sourceRect.Height);
                grayTex.SetData(data);
                return grayTex;
            }
        }
    }
}
