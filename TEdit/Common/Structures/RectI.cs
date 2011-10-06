using System;

namespace TEdit.Common.Structures
{
    [Serializable]
    public struct RectI
    {
        private PointInt32 _bottomRight;
        private PointInt32 _topLeft;

        public RectI(RectI rect)
        {
            _topLeft = new PointInt32(rect.TopLeft);
            _bottomRight = new PointInt32(rect.BottomRight);
        }

        public RectI(PointInt32 topLeft, PointInt32 bottomRight)
        {
            _topLeft = topLeft;
            _bottomRight = bottomRight;
        }

        public RectI(int left, int right, int top, int bottom)
        {
            _topLeft = new PointInt32(left, top);
            _bottomRight = new PointInt32(right, bottom);
        }

        public int Left
        {
            get { return TopLeft.X; }
            set { _topLeft.X = value; }
        }

        public int Right
        {
            get { return BottomRight.X; }
            set { _bottomRight.X = value; }
        }

        public int Top
        {
            get { return TopLeft.Y; }
            set { _topLeft.Y = value; }
        }

        public int Bottom
        {
            get { return BottomRight.Y; }
            set { _bottomRight.Y = value; }
        }

        public PointInt32 TopLeft
        {
            get { return _topLeft; }
            set { _topLeft = value; }
        }

        public PointInt32 BottomRight
        {
            get { return _bottomRight; }
            set { _bottomRight = value; }
        }

        public bool Contains(int x, int y)
        {
            if (x < Left)
                return false;
            if (x > Right)
                return false;
            if (y < Top)
                return false;
            if (y > Bottom)
                return false;

            return true;
        }

        public override string ToString()
        {
            return String.Format("[{0}, {1}]", TopLeft, BottomRight);
        }

        #region Operator Overrides

        private static bool MatchFields(RectI a, RectI m)
        {
            return (a.TopLeft == m.TopLeft && a.BottomRight == m.BottomRight);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is RectI)
                return MatchFields(this, (RectI) obj);

            return false;
        }
        
        public bool Equals(RectI p)
        {
            return MatchFields(this, p);
        }

        public static bool operator ==(RectI a, RectI b)
        {
            return MatchFields(a, b);
        }

        public static bool operator !=(RectI a, RectI b)
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

        // one-way; reverse is not guaranteed to not lose data
        public static implicit operator RectF(RectI rect)
        {
            return new RectF(rect.Left, rect.Right, rect.Top, rect.Bottom);
        }

        #endregion
    }
}