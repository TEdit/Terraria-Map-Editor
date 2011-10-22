using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TEdit.Common.Structures
{
    [Serializable]
    public class TexturePlus : Texture2D
    {
        private int Bpp = 4;  // bytes per pixel (in this case)
        private BytePixels _dataBytes;

        public TexturePlus(Texture2D t2d)
            : base(t2d.GraphicsDevice, t2d.Width, t2d.Height)
        {
            var wh = t2d.Width * t2d.Height;
            var colors = new Color[wh];

            t2d.GetData<Color>(colors);
            base.SetData<Color>(colors);
            _dataBytes = new BytePixels(t2d.Width, t2d.Height, this.Convert2DData(colors));
        }

        public TexturePlus (GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice, width, height)
        {
            _dataBytes = new BytePixels(width, height, this.Convert2DData());
        }

        public TexturePlus (GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format)
            : base(graphicsDevice, width, height, mipMap, format)
        {
            _dataBytes = new BytePixels(width, height, this.Convert2DData());
        }

        private byte[] Convert2DData ()
        {
            var wh = this.Width * this.Height;
            var d = new byte[wh * 4];
            var colors = new Color[wh];
            
            this.GetData<Color>(colors);
            return Convert2DData(colors);
        }
        private byte[] Convert2DData(Color[] colors)
        {
            var d = new byte[colors.Length * 4];

            int ofs = 0;
            foreach (Color c in colors)
            {
                d[ofs++] = (byte)c.B;
                d[ofs++] = (byte)c.G;
                d[ofs++] = (byte)c.R;
                d[ofs++] = (byte)c.A;
            }
            return d;
        }

        // GetData and its various overloads

        /// <summary>Returns a copy of 2D texture data in an one-dimensional byte array.</summary>
        public BytePixels GetData()      { return _dataBytes; }

        /// <summary>Gets a copy of 2D texture data in an one-dimensional byte array.</summary>
        /// <param name="data">An one-dimensional byte array.</param>
        public void GetData(BytePixels data) { data = _dataBytes; }

        /// <summary>Gets a copy of 2D texture data in an one-dimensional byte array, specifying a source rectangle.</summary> 
        /// <param name="rect">The section of the texture to copy. <paramref name="null"/> indicates the data will be copied from the entire texture.</param>
        /// <param name="data">An one-dimensional byte array.</param>
        public void GetData(Microsoft.Xna.Framework.Rectangle? rect, BytePixels data) { GetData(rect, data, 0); }

        /// <summary>Gets a copy of 2D texture data in an one-dimensional byte array, specifying a start index, and number of elements.</summary> 
        /// <param name="data">An one-dimensional byte array.</param>
        /// <param name="startIndex">Index within the array of the first element (pixel) to get.</param>
        /// <param name="elementCount">Number of elements (pixels) to get.</param>
        public void GetData(BytePixels data, int startIndex, int elementCount) { Array.ConstrainedCopy(_dataBytes.Data, 0, data.Data, startIndex, elementCount); }

        /// <summary>Returns a copy of 2D texture data in an one-dimensional byte array, specifying a source rectangle.</summary> 
        /// <param name="rect">The section of the texture to copy. <paramref name="null"/> indicates the data will be copied from the entire texture.</param>
        public BytePixels GetData(Microsoft.Xna.Framework.Rectangle? rect)
        {
            if (rect != null)
            {
                var r = (Microsoft.Xna.Framework.Rectangle)rect;
                var data = new BytePixels(new SizeInt32(r.Width, r.Height), Bpp);
                GetData(r, data, 0, r.Width * r.Height);
                return data;
            }

            return GetData();
        }

        /// <summary>Gets a copy of 2D texture data in an one-dimensional byte array, specifying a source rectangle, and start index.</summary> 
        /// <param name="rect">The section of the texture to copy. <paramref name="null"/> indicates the data will be copied from the entire texture.</param>
        /// <param name="data">An one-dimensional byte array.</param>
        /// <param name="startIndex">Index within the array of the first element (pixel) to get.</param>
        public void GetData(Microsoft.Xna.Framework.Rectangle? rect, BytePixels data, int startIndex)
        {
            if (rect != null)
            {
                var r = (Microsoft.Xna.Framework.Rectangle)rect;
                GetData(r, data, startIndex, r.Width * r.Height);
                return;
            }

            GetData(data);
        }

        /// <summary>Gets a copy of 2D texture data in an one-dimensional byte array, specifying a source rectangle, start index, and number of elements.</summary> 
        /// <param name="rect">The section of the texture to copy. <paramref name="null"/> indicates the data will be copied from the entire texture.</param>
        /// <param name="data">An one-dimensional byte array.</param>
        /// <param name="startIndex">Index within the array of the first element (pixel) to get.</param>
        /// <param name="elementCount">Number of elements (pixels) to get.</param>
        public void GetData(Microsoft.Xna.Framework.Rectangle? rect, BytePixels data, int startIndex, int elementCount)
        {
            Microsoft.Xna.Framework.Rectangle r;
            if (rect != null) r = (Microsoft.Xna.Framework.Rectangle)rect;
            else
            {
                GetData(data, startIndex, elementCount);
                return;
            }

            var sOfs = (r.Y * this.Width) + r.X;
            sOfs *= Bpp;

            var dOfs = startIndex;
            var dataLeft = elementCount * Bpp;
            for (int y = 0; y < r.Height; y++)
            {
                var lineLen = r.Width * Bpp;
                lineLen = dataLeft < lineLen ? dataLeft : lineLen;

                Array.ConstrainedCopy(_dataBytes.Data, sOfs, data.Data, dOfs, lineLen);
                sOfs     += this.Width * Bpp;
                dOfs     += lineLen;
                dataLeft -= lineLen;

                if (dataLeft <= 0) break;
            }
        }

        // (weirdness with where doesn't allow us to override, and can't call a base if we do that, anyway...)
        public new void SetData<T>(T[] data) where T : struct
        {
            base.SetData(data);
            this.Convert2DData();
        }

        #region Operator Overrides

        public static TexturePlus operator +(TexturePlus a, TexturePlus b)
        {
            var tp = new TexturePlus(a.GraphicsDevice, a.Width + b.Width, a.Height);
            var wh = (a.Width + b.Width) * a.Height;
            
            var colors = new Color[wh];
            var ca = new Color[a.Width * a.Height];
            var cb = new Color[b.Width * a.Height];
            
            a.GetData<Color>(ca);
            colors.CopyTo(ca, 0);
            b.GetData<Color>(cb);
            colors.CopyTo(cb, a.Width * a.Height);

            tp.SetData(colors);
            return tp;
        }

        /// FIXME ///
        /* public static byte[] operator +(TexturePlus a, BytePixels b)
        {
            var img = new byte[(a.Width + a.Height) * 4 + b.Length];
            img.CopyTo(a.GetData(), 0);
            img.CopyTo(b, (a.Width + a.Height) * 4);
            return img;
        }
        public static byte[] operator +(BytePixels a, TexturePlus b)
        {
            var img = new byte[a.Length + (b.Width + b.Height) * 4];
            img.CopyTo(a, 0);
            img.CopyTo(b.GetData(), a.Length);
            return img;
        } */

        #endregion
    }
}