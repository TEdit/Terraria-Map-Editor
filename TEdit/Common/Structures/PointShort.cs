using System;

namespace TEdit.Common.Structures
{
    [Serializable]
    public struct PointShort
    {
        private short _x;

        private short _y;

        public PointShort(short x, short y)
        {
            _x = x;
            _y = y;
        }

        public short X
        {
            get { return _x; }
            set { _x = value; }
        }

        public short Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }

        public static bool TryParse(string point, out PointShort pointShort)
        {
            short x = 0;
            short y = 0;

            if (string.IsNullOrWhiteSpace(point))
            {
                pointShort = new PointShort(x, y);
                return false;
            }

            string[] split = point.Split(',');
            if (split.Length == 2)
            {
                short.TryParse(split[0], out x);
                short.TryParse(split[1], out y);
                pointShort = new PointShort(x, y);
                return true;
            }

            pointShort = new PointShort(x, y);
            return false;
        }

        public static PointShort TryParseInline(string point)
        {
            PointShort result;
            TryParse(point, out result);
            return result;
        }

        public static PointShort Parse(string point)
        {
            short x = 0;
            short y = 0;

            if (string.IsNullOrWhiteSpace(point))
            {
                throw new NullReferenceException("point cannot be null");
            }

            string[] split = point.Split(',');
            if (split.Length == 2)
            {
                x = short.Parse(split[0]);
                y = short.Parse(split[1]);
                return new PointShort(x, y);

            }

            throw new ArgumentOutOfRangeException("point", "Invalid point structure, must be in the form of x,y");
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
            int result = 13;
            result = result * 7 + X.GetHashCode();
            result = result * 7 + Y.GetHashCode();
            return result;
        }

        #endregion
    }
}