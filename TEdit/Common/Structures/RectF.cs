using System;

namespace TEdit.Common.Structures
{
    [Serializable]
    public struct RectF
    {
        private PointFloat _bottomRight;
        private PointFloat _topLeft;

        public RectF(PointFloat topLeft, PointFloat bottomRight)
        {
            _topLeft = topLeft;
            _bottomRight = bottomRight;
        }

        public RectF(float left, float right, float top, float bottom)
        {
            _topLeft = new PointFloat(left, top);
            _bottomRight = new PointFloat(right, bottom);
        }

        public float Left
        {
            get { return TopLeft.X; }
        }

        public float Right
        {
            get { return BottomRight.X; }
        }

        public float Top
        {
            get { return TopLeft.Y; }
        }

        public float Bottom
        {
            get { return BottomRight.Y; }
        }

        public PointFloat TopLeft
        {
            get { return _topLeft; }
            set { _topLeft = value; }
        }

        public PointFloat BottomRight
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

        private static bool MatchFields(RectF a, RectF m)
        {
            return (a.TopLeft == m.TopLeft && a.BottomRight == m.BottomRight);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is RectF)
                return MatchFields(this, (RectF) obj);

            return false;
        }

        public bool Equals(RectF p)
        {
            return MatchFields(this, p);
        }

        public static bool operator ==(RectF a, RectF b)
        {
            return MatchFields(a, b);
        }

        public static bool operator !=(RectF a, RectF b)
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