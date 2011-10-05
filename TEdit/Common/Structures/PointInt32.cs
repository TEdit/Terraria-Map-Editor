using System;

namespace TEdit.Common.Structures
{
    [Serializable]
    public struct PointInt32
    {
        private int _x;

        private int _y;

        public PointInt32(PointInt32 p)
        {
            _x = p.X;
            _y = p.Y;
        }

        public PointInt32(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }

        #region Operator Overrides

        private static bool MatchFields(PointInt32 a, PointInt32 m)
        {
            return (a.X == m.X && a.Y == m.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is PointInt32)
                return MatchFields(this, (PointInt32) obj);

            return false;
        }

        public bool Equals(PointInt32 p)
        {
            return MatchFields(this, p);
        }

        public static bool operator ==(PointInt32 a, PointInt32 b)
        {
            return MatchFields(a, b);
        }

        public static bool operator !=(PointInt32 a, PointInt32 b)
        {
            return !(a == b);
        }

        public static PointInt32 operator +(PointInt32 a, PointInt32 b)
        {
            return new PointInt32(a.X + b.X, a.Y + b.Y);
        }

        public static PointInt32 operator -(PointInt32 a, PointInt32 b)
        {
            return new PointInt32(a.X - b.X, a.Y - b.Y);
        }

        public static PointInt32 operator /(PointInt32 a, PointInt32 b)
        {
            return new PointInt32(a.X/b.X, a.Y/b.Y);
        }

        public static PointInt32 operator *(PointInt32 a, PointInt32 b)
        {
            return new PointInt32(a.X*b.X, a.Y*b.Y);
        }

        /* int overloads */
        public static PointInt32 operator +(PointInt32 a, int b)
        {
            return new PointInt32(a.X + b, a.Y + b);
        }

        public static PointInt32 operator -(PointInt32 a, int b)
        {
            return new PointInt32(a.X - b, a.Y - b);
        }

        public static PointInt32 operator /(PointInt32 a, int b)
        {
            return new PointInt32(a.X/b, a.Y/b);
        }

        public static PointInt32 operator *(PointInt32 a, int b)
        {
            return new PointInt32(a.X*b, a.Y*b);
        }

        public static PointInt32 operator +(int a, PointInt32 b)
        {
            return new PointInt32(a + b.X, a + b.Y);
        }

        public static PointInt32 operator -(int a, PointInt32 b)
        {
            return new PointInt32(a - b.X, a - b.Y);
        }

        public static PointInt32 operator /(int a, PointInt32 b)
        {
            return new PointInt32(a/b.X, a/b.Y);
        }

        public static PointInt32 operator *(int a, PointInt32 b)
        {
            return new PointInt32(a*b.X, a*b.Y);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result = result*37 + X.GetHashCode();
            result = result*37 + Y.GetHashCode();
            return result;
        }

        // one-way; reverse is not guaranteed to not lose data
        public static implicit operator PointFloat(PointInt32 p)
        { 
            return new PointFloat(p.X, p.Y);
        }

        // two-way; other function is available on other struct
        public static implicit operator SizeInt32(PointInt32 p)
        {
            return new SizeInt32(p.X, p.Y);
        }

        #endregion
    }
}