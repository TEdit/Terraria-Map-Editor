using System;

namespace TEdit.Common.Structures
{
    [Serializable]
    public struct RectF
    {
        private PointFloat _bottomRight;
        private PointFloat _topLeft;

        public RectF(RectF rect)
        {
            _topLeft = new PointFloat(rect.TopLeft);
            _bottomRight = new PointFloat(rect.BottomRight);
        }

        public RectF(PointFloat topLeft, SizeFloat size)
        {
            _topLeft = topLeft;
            _bottomRight = new PointFloat(size.Width + topLeft.X - 1, size.Height + topLeft.Y - 1);
        }

        public RectF(PointFloat topLeft, PointFloat bottomRight)
        {
            _topLeft = topLeft;
            _bottomRight = bottomRight;
        }

        public RectF(float bottom, float right) {
            _topLeft = new PointFloat();
            _bottomRight = new PointFloat(bottom, right);
        }

        public RectF(float X, float Y, SizeFloat size)
        {
            _topLeft = new PointFloat(X, Y);
            _bottomRight = new PointFloat(size.Width + X - 1, size.Height + Y - 1);
        }

        public RectF(float left, float right, float top, float bottom)
        {
            _topLeft = new PointFloat(left, top);
            _bottomRight = new PointFloat(right, bottom);
        }

        public float X
        {
            get { return TopLeft.X; }
            set { _topLeft.X = value; }
        }

        public float Y
        {
            get { return TopLeft.Y; }
            set { _topLeft.Y = value; }
        }

        public float Left
        {
            get { return TopLeft.X; }
            set { _topLeft.X = value; }
        }

        public float Right
        {
            get { return BottomRight.X; }
            set { _bottomRight.X = value; }
        }

        public float Top
        {
            get { return TopLeft.Y; }
            set { _topLeft.Y = value; }
        }

        public float Bottom
        {
            get { return BottomRight.Y; }
            set { _bottomRight.Y = value; }
        }

        public float Width
        {
            get { return BottomRight.X - TopLeft.X + 1; }
            set { _bottomRight.X = value + TopLeft.X - 1; }
        }

        public float Height
        {
            get { return BottomRight.Y - TopLeft.Y + 1; }
            set { _bottomRight.Y = value + TopLeft.Y - 1; }
        }

        public float Total {
            get { return Width * Height; }
        }

        public float W
        {
            get { return Width; }
            set { Width = value; }
        }

        public float H
        {
            get { return Height; }
            set { Height = value; }
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

        public PointFloat TopRight
        {
            get { return new PointFloat(Right, Top); }
            set { _topLeft.Y = value.Y; _bottomRight.X = value.X; }
        }

        public PointFloat BottomLeft
        {
            get { return new PointFloat(Left, Bottom); }
            set { _topLeft.X = value.X; _bottomRight.Y = value.Y; }
        }

        public SizeFloat Size
        {
            get { return new SizeFloat(this.Width, this.Height); }
            set { _bottomRight = new PointFloat(value.W + TopLeft.X - 1, value.H + TopLeft.Y - 1); }
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

        public bool Rebound(RectI bound, bool shiftTop = false, bool shiftLeft = false, bool shiftBottom = false, bool shiftRight = false) {  // shiftXXX = shift W/H or just truncate
            bool changed = false;

            if (Top    < bound.Top) {
                if (shiftTop) Height += bound.Top - Top;
                Top    = bound.Top;
                changed = true;
            }
            if (Left   < bound.Left) {
                if (shiftLeft) Width += bound.Left - Left;
                Left   = bound.Left;
                changed = true;
            }
            if (Bottom > bound.Bottom) {
                if (shiftBottom) Top  -= Bottom - bound.Bottom;
                Bottom = bound.Bottom;
                changed = true;
            }
            if (Right  > bound.Right) {
                if (shiftRight) Left -= Right  - bound.Right;
                Right  = bound.Right;
                changed = true;
            }

            // forced truncations
            if (Width > bound.Width) {
                Left  = bound.Left;
                Right = bound.Right;
                changed = true;
            }
            if (Height > bound.Height) {
                Top    = bound.Top;
                Bottom = bound.Bottom;
                changed = true;
            }

            return changed;
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

        public static RectF operator +(RectF a, PointFloat b) { return new RectF(a.TopLeft + b, a.Size); }
        public static RectF operator +(PointFloat a, RectF b) { return b+a; }
        public static RectF operator +(RectF a, SizeFloat b)  { return new RectF(a.TopLeft, a.Size + b); }
        public static RectF operator +(SizeFloat a, RectF b)  { return b+a; }
        public static RectF operator +(RectF a, RectF b)      { return new RectF(a.TopLeft + b.TopLeft, a.Size + b.Size); }

        public static RectF operator -(RectF a, PointFloat b) { return new RectF(a.TopLeft - b, a.Size); }
        public static RectF operator -(PointFloat a, RectF b) { return new RectF(a - b.TopLeft, b.Size); }
        public static RectF operator -(RectF a, SizeFloat b)  { return new RectF(a.TopLeft, a.Size - b); }
        public static RectF operator -(SizeFloat a, RectF b)  { return new RectF(b.TopLeft, a - b.Size); }
        public static RectF operator -(RectF a, RectF b)      { return new RectF(a.TopLeft - b.TopLeft, a.Size - b.Size); }

        public static RectF operator *(RectF a, PointFloat b) { return new RectF(a.TopLeft * b, a.Size); }
        public static RectF operator *(PointFloat a, RectF b) { return b*a; }
        public static RectF operator *(RectF a, SizeFloat b)  { return new RectF(a.TopLeft, a.Size * b); }
        public static RectF operator *(SizeFloat a, RectF b)  { return b*a; }
        public static RectF operator *(RectF a, RectF b)      { return new RectF(a.TopLeft * b.TopLeft, a.Size * b.Size); }

        public static RectF operator /(RectF a, PointFloat b) { return new RectF(a.TopLeft / b, a.Size); }
        public static RectF operator /(PointFloat a, RectF b) { return new RectF(a / b.TopLeft, b.Size); }
        public static RectF operator /(RectF a, SizeFloat b)  { return new RectF(a.TopLeft, a.Size / b); }
        public static RectF operator /(SizeFloat a, RectF b)  { return new RectF(b.TopLeft, a / b.Size); }
        public static RectF operator /(RectF a, RectF b)      { return new RectF(a.TopLeft / b.TopLeft, a.Size / b.Size); }

        public override int GetHashCode()
        {
            int result = 17;
            result = result*37 + TopLeft.GetHashCode();
            result = result*37 + BottomRight.GetHashCode();
            return result;
        }

        // S: Yo dawg, I herd you like rectangles, so I put a rectangle in your rectangle in
        //    this rectangle, so you can rectangle while you rectangle with this rectangle.

        // explicit; is not guaranteed to not lose data
        public static explicit operator RectI(RectF rect)
        {
            return new RectI((int)rect.Left, (int)rect.Right, (int)rect.Top, (int)rect.Bottom);
        }

        // one-way; reverse is not guaranteed to not lose data
        public static implicit operator RectF(Microsoft.Xna.Framework.Rectangle rect)
        {
            return new RectF(rect.Left, rect.Right, rect.Top, rect.Bottom);
        }
        public static explicit operator Microsoft.Xna.Framework.Rectangle(RectF rect)
        {
            return new Microsoft.Xna.Framework.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }

        // one-way; reverse is not guaranteed to not lose data
        public static implicit operator RectF(System.Drawing.Rectangle rect)
        {
            return new RectF(rect.Left, rect.Right, rect.Top, rect.Bottom);
        }
        public static explicit operator System.Drawing.Rectangle(RectF rect)
        {
            return new System.Drawing.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }

        // two-way
        public static implicit operator System.Drawing.RectangleF(RectF rect)
        {
            return new System.Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public static explicit operator RectF(System.Drawing.RectangleF rect)
        {
            return new RectF(rect.Left, rect.Right, rect.Top, rect.Bottom);
        }

        // one-way; reverse is not guaranteed to not lose data
        public static implicit operator RectF(System.Windows.Int32Rect rect)
        {
            return new RectF(rect.X, rect.Y, new SizeFloat(rect.Width, rect.Height));
        }
        public static explicit operator System.Windows.Int32Rect(RectF rect)
        {
            return new System.Windows.Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }

        // two-way
        public static implicit operator System.Windows.Rect(RectF rect)
        {
            return new System.Windows.Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public static implicit operator RectF(System.Windows.Rect rect)
        {
            // (Okay, technically, double->float might lose data.
            // Oh god, I might lose .000001 of a pixel!)
            return new RectF((float)rect.Left, (float)rect.Right, (float)rect.Top, (float)rect.Bottom);
        }

        #endregion
    }
}