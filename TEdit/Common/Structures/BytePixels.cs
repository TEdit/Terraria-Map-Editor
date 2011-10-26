using System;
using System.Windows.Media;
using TEdit.Common;

namespace TEdit.Common.Structures
{
    [Serializable]
    public class BytePixels : ObservableObject
    {
        private int _Bpp = 4;  // bytes per pixel (in this case)
        private SizeInt32 _size;
        private byte[] _data;

        public BytePixels(SizeInt32 size)
        {
            _size = new SizeInt32(size.W, size.H);
            _data = new byte[size.Total * _Bpp];
        }
        public BytePixels(SizeInt32 size, byte[] data)
        {
            _size = new SizeInt32(size.W, size.H);
            _data = (byte[])data.Clone();
            _Bpp  = _data.Length / _size.Total;
        }
        public BytePixels(SizeInt32 size, int Bpp)
        {
            _Bpp  = Bpp;
            _size = new SizeInt32(size.W, size.H);
            _data = new byte[size.Total * _Bpp];
        }
        public BytePixels(int w, int h, byte[] data)
        {
            _size = new SizeInt32(w, h);
            _data = (byte[])data.Clone();
            _Bpp  = _data.Length / _size.Total;
        }
        public BytePixels(TexturePlus tp)
        {
            _size = new SizeInt32(tp.Width, tp.Height);
            _data = (byte[])tp.GetData().Data.Clone();
            _Bpp  = _data.Length / _size.Total;
        }
        public BytePixels(Microsoft.Xna.Framework.Graphics.Texture2D t2d)
        {
            var tp = new TexturePlus(t2d);
            _size = new SizeInt32(tp.Width, tp.Height);
            _data = (byte[])tp.GetData().Data.Clone();
            _Bpp = _data.Length / _size.Total;
        }

        public SizeInt32 Size  { get { return _size; } }
        public int       Bpp   { get { return _Bpp;  } }
        public byte[]    Data  { get { return _data; } }  // should still allow access to individual bytes

        public byte[] GetData() { return (byte[])_data.Clone(); }

        /* 
        /// <summary>Gets a copy of all frames within 2D texture data into a two-dimensional byte array, using the specified frame size.</summary> 
        /// <param name="size">The pixel size of each frame.</param>
        public byte[,] GetData(SizeInt32 size)
        {

        } 
        */

        public void SetData(BytePixels src)
            { src.PutData(null, this); }
        public void SetData(BytePixels src, PointInt32 xy)
            { src.PutData(null, this, xy); }
        public void SetData(RectI? rect, BytePixels src)
            { src.PutData(rect, this, new PointInt32()); }
        public void SetData(RectI? rect, BytePixels src, PointInt32 xy)
            { src.PutData(rect, this, xy); }
        public void SetData(RectI? rect, byte[] src, int width, int height, PointInt32 xy)
        {
            var bp = new BytePixels(width, height, src);
            bp.PutData(rect, this, xy);
        }

        // PutData and its various overloads

        public void PutData(BytePixels dest)
            { PutData(null, dest); }
        public void PutData(BytePixels dest, PointInt32 xy)
            { PutData(null, dest, xy); }
        public void PutData(RectI? rect, BytePixels dest)
            { PutData(rect, dest, new PointInt32()); }

        /// <summary>Puts a copy of 2D pixel data in a BytePixels object, specifying a source rectangle and a destination X,Y point.</summary>
        /// <param name="rect">The section of the pixel data to copy. <paramref name="null"/> indicates the data will be copied from the entire object.</param>
        /// <param name="dest">The destination BytePixels object.</param>
        /// <param name="xy">The top-left corner of the destination.</param>
        public void PutData(RectI? rect, BytePixels dest, PointInt32 xy)
        {
            RectI r;
            if (rect != null) r = (RectI)rect;
            else              r = new RectI(new PointInt32(), this.Size);
            
            var sOfs = (r.Y * this.Size.W) + r.X;
            sOfs *= Bpp;

            var dOfs = (xy.Y * dest.Size.W) + xy.X;
            dOfs *= Bpp;  // note: we're not factoring for Bpp mismatches here...

            var dataLeft = r.Size.Total * Bpp;
            for (int y = 0; y < r.Height; y++)
            {
                Array.ConstrainedCopy(_data, sOfs, dest.Data, dOfs, r.Width * Bpp);
                sOfs += this.Size.W * Bpp;
                dOfs += dest.Size.W * Bpp;
            }
        }

        // Pixel-level functions
        public byte[] GetPixel(int o) {
            var b = new byte[Bpp];
            Array.ConstrainedCopy(_data, o * Bpp, b, 0, Bpp);
            return b;
        }
        public byte[] GetPixel(int x, int y) { return GetPixel(Size.W * y + x); }
        public byte[] GetPixel(PointInt32 xy) { return GetPixel(Size.W * xy.Y + xy.X); }

        public Color GetColor(int o) {
            var b = GetPixel(o);
            var c = Color.FromArgb(b[3], b[2], b[1], b[0]);  // assuming BGRA format for now
            return c;
        }
        public Color GetColor(int x, int y) { return GetColor(Size.W * y + x); }
        public Color GetColor(PointInt32 xy) { return GetColor(Size.W * xy.Y + xy.X); }

        public void SetPixel(int o, byte[] b)         { Array.ConstrainedCopy(b, 0, _data, o * Bpp, Bpp); }
        public void SetPixel(int x, int y, byte[] b)  { SetPixel(Size.W * y + x, b); }
        public void SetPixel(PointInt32 xy, byte[] b) { SetPixel(Size.W * xy.Y + xy.X, b); }
        public void SetPixel(int o, Color c) {
            var b = new byte[4];
            b[0] = c.B;
            b[1] = c.G;
            b[2] = c.R;
            b[3] = c.A;
            SetPixel(o, b);
        }
        public void SetPixel(int x, int y, Color c)  { SetPixel(Size.W * y + x, c); }
        public void SetPixel(PointInt32 xy, Color c) { SetPixel(Size.W * xy.Y + xy.X, c); }

        public BytePixels AlphaBlend(BytePixels bp, bool selfIsBack = true) {
            var blended = new BytePixels(Size, Bpp);

            for (int i = 0; i < Size.Total; i++) {
                blended.SetPixel(i, GetColor(i).AlphaBlend(bp.GetColor(i), selfIsBack));
            }

            return blended;
        }
        public BytePixels AlphaBlend(Color c, bool selfIsBack = true) {
            var blended = new BytePixels(Size, Bpp);

            for (int i = 0; i < Size.Total; i++) {
                blended.SetPixel(i, GetColor(i).AlphaBlend(c, selfIsBack));
            }

            return blended;
        }


        #region Operator Overrides

        public static BytePixels operator +(BytePixels a, BytePixels b)
        {
            var bp = new BytePixels(new SizeInt32(a.Size.W + b.Size.W, a.Size.H > b.Size.H ? a.Size.H : b.Size.H));
            bp.SetData(a);
            bp.SetData(b, new PointInt32(a.Size.W, 0));
            return bp;
        }
        public static BytePixels operator +(BytePixels a, byte[] b)
        {
            var bp = new BytePixels(new SizeInt32(a.Size.W + b.Length / a.Size.H / a.Bpp, a.Size.H), a.GetData());
            return a + bp;
        }
        public static BytePixels operator +(byte[] a, BytePixels b)
        {
            return b + a;
        }

        public static explicit operator byte[](BytePixels bp) {
            return bp.GetData();
        }

        #endregion

    }
}