using System;

namespace TEdit.TerrariaWorld.Structures
{
    public class RectInt32
    {
        public RectInt32(PointInt32 topLeft, PointInt32 bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        public RectInt32(int left, int right, int top, int bottom)
        {
            TopLeft = new PointInt32(left, top);
            BottomRight = new PointInt32(right, bottom);
        }

        public RectInt32(PointInt32 location, int width, int height)
        {
            TopLeft = location;
            BottomRight = new PointInt32(location.X + width, location.Y + Height);
        }

        public int Left
        {
            get { return TopLeft.X; }
        }

        public int Right
        {
            get { return BottomRight.X; }
        }

        public int Top
        {
            get { return TopLeft.Y; }
        }

        public int Bottom
        {
            get { return BottomRight.Y; }
        }

        public int X
        {
            get { return Left; }
        }

        public int Y
        {
            get { return Top; }
        }

        public int Width
        {
            get { return Math.Abs(BottomRight.X - TopLeft.X); }
        }

        public int Height
        {
            get { return Math.Abs(BottomRight.Y - TopLeft.Y); }
        }

        public PointInt32 TopLeft { get; set; }

        public PointInt32 BottomRight { get; set; }

        public override string ToString()
        {
            return String.Format("[{0}, {1}]", TopLeft, BottomRight);
        }

        public bool Contains(PointInt32 point)
        {
            int xabs = (point.X - X);
            int yabs = (point.Y - Y);

            if (xabs < 0)
                return false;

            if (yabs < 0)
                return false;

            return xabs < Width && yabs < Height;
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
                return MatchFields(this, (RectInt32) obj);

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
            result = result*37 + TopLeft.GetHashCode();
            result = result*37 + BottomRight.GetHashCode();
            return result;
        }

        #endregion
    }
}