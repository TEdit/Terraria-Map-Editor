namespace TEditWPF.TerrariaWorld.Structures
{
    public struct PointShort
    {
        public PointShort(short x, short y)
        {
            _x = x;
            _y = y;
        }

        private short _x;
        public short X
        {
            get { return _x; }
            set { _x = value; }
        }

        private short _y;
        public short Y
        {
            get { return _y; }
            set { _y = value; }
        }

        #region Operator Overrides

        private static bool MatchFields(PointShort a, PointShort m)
        {
            return (a.X == m.X && a.Y == m.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is PointShort)
                return MatchFields(this, (PointShort)obj);

            return false;
        }

        public bool Equals(PointShort p)
        {
            return MatchFields(this, p);
        }

        public static bool operator ==(PointShort a, PointShort b)
        {
            return MatchFields(a, b);
        }

        public static bool operator !=(PointShort a, PointShort b)
        {
            return !(a == b);
        }

        public static PointShort operator +(PointShort a, PointShort b)
        {
            return new PointShort((short)(a.X + b.X), (short)(a.Y + b.Y));
        }

        public static PointShort operator -(PointShort a, PointShort b)
        {
            return new PointShort((short)(a.X - b.X), (short)(a.Y - b.Y));
        }

        public override int GetHashCode()
        {
            int result = 17;
            result = result * 37 + X.GetHashCode();
            result = result * 37 + Y.GetHashCode();
            return result;
        }

        #endregion
    }
}