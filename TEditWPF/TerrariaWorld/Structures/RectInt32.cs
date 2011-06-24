using System;
using System.Windows;

namespace TEditWPF.TerrariaWorld.Structures
{
    public class RectInt32
    {
        public RectInt32(PointInt32 topLeft, PointInt32 bottomRight)
        {
            _topLeft = topLeft;
            _bottomRight = bottomRight;
        }

        public RectInt32(int left, int right, int top, int bottom)
        {
            _topLeft = new PointInt32(left, top);
            _bottomRight = new PointInt32(right, bottom);
        }

        public RectInt32(PointInt32 location, int width, int height)
        {
            _topLeft = location;
            _bottomRight = new PointInt32(location.X + width, location.Y + Height);
        }

        public int Left
        {
            get { return this.TopLeft.X; }
        }

        public int Right
        {
            get { return this.BottomRight.X; }
        }

        public int Top
        {
            get { return this.TopLeft.Y; }
        }

        public int Bottom
        {
            get { return this.BottomRight.Y; }
        }

        public int X
        {
            get { return this.Left; }
        }

        public int Y
        {
            get { return this.Top; }
        }

        public int Width
        {
            get { return Math.Abs(this.BottomRight.X - this.TopLeft.X); }
        }

        public int Height
        {
            get { return Math.Abs(this.BottomRight.Y - this.TopLeft.Y); }
        }

        private PointInt32 _topLeft;
        public PointInt32 TopLeft
        {
            get { return _topLeft; }
            set { _topLeft = value; }
        }

        private PointInt32 _bottomRight;
        public PointInt32 BottomRight
        {
            get { return _bottomRight; }
            set { _bottomRight = value; }
        }

        public override string ToString()
        {
            return String.Format("[{0}, {1}]", this.TopLeft, this.BottomRight);
        }

        public bool Contains(PointInt32 point)
        {
            int xabs = (point.X - this.X);
            int yabs = (point.Y - this.Y);

            if (xabs < 0)
                return false;

            if (yabs < 0)
                return false;

            return xabs < this.Width && yabs < this.Height;
        }

        #region Operator Overrides

        private static bool MatchFields(RectInt32 a, RectInt32 m)
        {
            return (a.TopLeft == m.TopLeft && a.BottomRight == m.BottomRight);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is RectInt32)
                return MatchFields(this, (RectInt32)obj);

            return false;
        }

        public bool Equals(RectInt32 p)
        {
            return MatchFields(this, p);
        }

        public static bool operator ==(RectInt32 a, RectInt32 b)
        {
            return MatchFields(a, b);
        }

        public static bool operator !=(RectInt32 a, RectInt32 b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result = result * 37 + TopLeft.GetHashCode();
            result = result * 37 + BottomRight.GetHashCode();
            return result;
        }

        #endregion
    }
}