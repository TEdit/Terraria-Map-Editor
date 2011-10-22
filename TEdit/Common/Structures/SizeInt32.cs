using System;

namespace TEdit.Common.Structures
{
    [Serializable]
    public class SizeInt32 : ObservableObject
    {
        private int _Height;
        private int _Width;

        public SizeInt32(SizeInt32 s)
        {
            _Width = s.Width;
            _Height = s.Height;
        }

        public SizeInt32(int width, int height)
        {
            _Width = width;
            _Height = height;
        }

        public int Width
        {
            get { return _Width; }
            set { SetProperty(ref _Width, ref value, "Width"); }
        }

        public int Height
        {
            get { return _Height; }
            set { SetProperty(ref _Height, ref value, "Height"); }
        }

        public int W
        {
            get { return _Width; }
            set { SetProperty(ref _Width, ref value, "Width"); }
        }

        public int H
        {
            get { return _Height; }
            set { SetProperty(ref _Height, ref value, "Height"); }
        }

        public int Total
        {
            get { return _Width * _Height; }
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", Width, Height);
        }

        #region Operator Overrides

        private static bool MatchFields(SizeInt32 a, SizeInt32 m)
        {
            return (a.Width == m.Width && a.Height == m.Height);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is SizeInt32)
                return MatchFields(this, (SizeInt32) obj);

            return false;
        }

        public bool Equals(SizeInt32 p)
        {
            return MatchFields(this, p);
        }

        public static bool operator ==(SizeInt32 a, SizeInt32 b)
        {
            return MatchFields(a, b);
        }

        public static bool operator !=(SizeInt32 a, SizeInt32 b)
        {
            return !(a == b);
        }

        public static SizeInt32 operator +(SizeInt32 a, SizeInt32 b)
        {
            return new SizeInt32(a.Width + b.Width, a.Height + b.Height);
        }

        public static SizeInt32 operator -(SizeInt32 a, SizeInt32 b)
        {
            return new SizeInt32(a.Width - b.Width, a.Height - b.Height);
        }

        public static SizeInt32 operator /(SizeInt32 a, SizeInt32 b)
        {
            return new SizeInt32(a.Width/b.Width, a.Height/b.Height);
        }

        public static SizeInt32 operator *(SizeInt32 a, SizeInt32 b)
        {
            return new SizeInt32(a.Width*b.Width, a.Height*b.Height);
        }

        /* int overloads */
        public static SizeInt32 operator +(SizeInt32 a, int b)
        {
            return new SizeInt32(a.Width + b, a.Height + b);
        }

        public static SizeInt32 operator -(SizeInt32 a, int b)
        {
            return new SizeInt32(a.Width - b, a.Height - b);
        }

        public static SizeInt32 operator /(SizeInt32 a, int b)
        {
            return new SizeInt32(a.Width/b, a.Height/b);
        }

        public static SizeInt32 operator *(SizeInt32 a, int b)
        {
            return new SizeInt32(a.Width*b, a.Height*b);
        }

        public static SizeInt32 operator +(int a, SizeInt32 b)
        {
            return new SizeInt32(a + b.Width, a + b.Height);
        }

        public static SizeInt32 operator -(int a, SizeInt32 b)
        {
            return new SizeInt32(a - b.Width, a - b.Height);
        }

        public static SizeInt32 operator /(int a, SizeInt32 b)
        {
            return new SizeInt32(a/b.Width, a/b.Height);
        }

        public static SizeInt32 operator *(int a, SizeInt32 b)
        {
            return new SizeInt32(a*b.Width, a*b.Height);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result = result*37 + Width.GetHashCode();
            result = result*37 + Height.GetHashCode();
            return result;
        }

        // one-way; reverse is not guaranteed to not lose data
        public static implicit operator PointFloat(SizeInt32 s)
        {
            return new PointFloat(s.Width, s.Height);
        }
        
        // two-way; reverse function is available on other struct
        public static implicit operator PointInt32(SizeInt32 s)
        {
            return new PointInt32(s.Width, s.Height);
        }

        #endregion
    }
}