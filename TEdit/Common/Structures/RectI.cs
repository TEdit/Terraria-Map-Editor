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

        public RectI(PointInt32 topLeft, SizeInt32 size)
        {
            _topLeft = topLeft;
            _bottomRight = new PointInt32(size.Width + topLeft.X - 1, size.Height + topLeft.Y - 1);
        }

        public RectI(PointInt32 topLeft, PointInt32 bottomRight)
        {
            _topLeft = topLeft;
            _bottomRight = bottomRight;
        }

        public RectI(int bottom, int right) {
            _topLeft = new PointInt32();
            _bottomRight = new PointInt32(bottom, right);
        }

        public RectI(int X, int Y, SizeInt32 size)
        {
            _topLeft = new PointInt32(X, Y);
            _bottomRight = new PointInt32(size.Width + X - 1, size.Height + Y - 1);
        }

        public RectI(int left, int right, int top, int bottom)
        {
            _topLeft = new PointInt32(left, top);
            _bottomRight = new PointInt32(right, bottom);
        }

        public int X
        {
            get { return TopLeft.X; }
            set { _topLeft.X = value; }
        }

        public int Y
        {
            get { return TopLeft.Y; }
            set { _topLeft.Y = value; }
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

        public int Width
        {
            get { return BottomRight.X - TopLeft.X + 1; }
            set { _bottomRight.X = value + TopLeft.X - 1; }
        }

        public int Height
        {
            get { return BottomRight.Y - TopLeft.Y + 1; }
            set { _bottomRight.Y = value + TopLeft.Y - 1; }
        }

        public int Total {
            get { return Width * Height; }
        }

        public int W
        {
            get { return Width; }
            set { Width = value; }
        }

        public int H
        {
            get { return Height; }
            set { Height = value; }
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

        public PointInt32 TopRight
        {
            get { return new PointInt32(Right, Top); }
            set { _topLeft.Y = value.Y; _bottomRight.X = value.X; }
        }

        public PointInt32 BottomLeft
        {
            get { return new PointInt32(Left, Bottom); }
            set { _topLeft.X = value.X; _bottomRight.Y = value.Y; }
        }

        public SizeInt32 Size
        {
            get { return new SizeInt32(this.Width, this.Height); }
            set { _bottomRight = new SizeInt32(value.W + TopLeft.X - 1, value.H + TopLeft.Y - 1); }
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
                if (shiftTop)    Height += bound.Top - Top;
                Top    = bound.Top;
                changed = true;
            }
            if (Left   < bound.Left) {
                if (shiftLeft)   Width += bound.Left - Left;
                Left   = bound.Left;
                changed = true;
            }
            if (Bottom > bound.Bottom) {
                if (shiftBottom) Top  -= Bottom - bound.Bottom;
                Bottom = bound.Bottom;
                changed = true;
            }
            if (Right  > bound.Right) {
                if (shiftRight)  Left -= Right  - bound.Right;
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

        public static RectI operator +(RectI a, PointInt32 b) { return new RectI(a.TopLeft + b, a.Size); }
        public static RectI operator +(PointInt32 a, RectI b) { return b+a; }
        public static RectI operator +(RectI a,  SizeInt32 b) { return new RectI(a.TopLeft, a.Size + b); }
        public static RectI operator +( SizeInt32 a, RectI b) { return b+a; }
        public static RectI operator +(RectI a,      RectI b) { return new RectI(a.TopLeft + b.TopLeft, a.Size + b.Size); }

        public static RectI operator -(RectI a, PointInt32 b) { return new RectI(a.TopLeft - b, a.Size); }
        public static RectI operator -(PointInt32 a, RectI b) { return new RectI(a - b.TopLeft, b.Size); }
        public static RectI operator -(RectI a, SizeInt32 b)  { return new RectI(a.TopLeft, a.Size - b); }
        public static RectI operator -(SizeInt32 a, RectI b)  { return new RectI(b.TopLeft, a - b.Size); }
        public static RectI operator -(RectI a, RectI b)      { return new RectI(a.TopLeft - b.TopLeft, a.Size - b.Size); }

        public static RectI operator *(RectI a, PointInt32 b) { return new RectI(a.TopLeft * b, a.Size); }
        public static RectI operator *(PointInt32 a, RectI b) { return b*a; }
        public static RectI operator *(RectI a, SizeInt32 b)  { return new RectI(a.TopLeft, a.Size * b); }
        public static RectI operator *(SizeInt32 a, RectI b)  { return b*a; }
        public static RectI operator *(RectI a, RectI b)      { return new RectI(a.TopLeft * b.TopLeft, a.Size * b.Size); }

        public static RectI operator /(RectI a, PointInt32 b) { return new RectI(a.TopLeft / b, a.Size); }
        public static RectI operator /(PointInt32 a, RectI b) { return new RectI(a / b.TopLeft, b.Size); }
        public static RectI operator /(RectI a, SizeInt32 b)  { return new RectI(a.TopLeft, a.Size / b); }
        public static RectI operator /(SizeInt32 a, RectI b)  { return new RectI(b.TopLeft, a / b.Size); }
        public static RectI operator /(RectI a, RectI b)      { return new RectI(a.TopLeft / b.TopLeft, a.Size / b.Size); }

        public override int GetHashCode()
        {
            int result = 17;
            result = result*37 + TopLeft.GetHashCode();
            result = result*37 + BottomRight.GetHashCode();
            return result;
        }

        // Q: How many rectangles are in .NET?
        // A: Eleventy billion.  All invented by Microsoft.

        // Q: Do they all mean the same thing?
        // A: Yes.

        // Q: Soooo, didn't you just create two more rectangle classes?
        // Q: Yeah?  So?  Why stop there?

        // one-way; reverse is not guaranteed to not lose data
        public static implicit operator RectF(RectI rect)
        {
            return new RectF(rect.Left, rect.Right, rect.Top, rect.Bottom);
        }

        // two-way
        public static implicit operator RectI(Microsoft.Xna.Framework.Rectangle rect)
        {
            return new RectI(rect.Left, rect.Right, rect.Top, rect.Bottom);
        }
        public static implicit operator Microsoft.Xna.Framework.Rectangle(RectI rect)
        {
            return new Microsoft.Xna.Framework.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        // two-way
        public static implicit operator RectI(System.Drawing.Rectangle rect)
        {
            return new RectI(rect.Left, rect.Right, rect.Top, rect.Bottom);
        }
        public static implicit operator System.Drawing.Rectangle(RectI rect)
        {
            return new System.Drawing.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        // one-way; reverse is not guaranteed to not lose data
        public static implicit operator System.Drawing.RectangleF(RectI rect)
        {
            return new System.Drawing.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public static explicit operator RectI(System.Drawing.RectangleF rect)
        {
            return new RectI((int)rect.Left, (int)rect.Right, (int)rect.Top, (int)rect.Bottom);
        }

        // two-way
        public static implicit operator RectI(System.Windows.Int32Rect rect)
        {
            return new RectI(rect.X, rect.Y, new SizeInt32(rect.Width, rect.Height));
        }
        public static implicit operator System.Windows.Int32Rect(RectI rect)
        {
            return new System.Windows.Int32Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        // one-way; reverse is not guaranteed to not lose data
        public static implicit operator System.Windows.Rect(RectI rect)
        {
            return new System.Windows.Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public static explicit operator RectI(System.Windows.Rect rect)
        {
            return new RectI((int)rect.Left, (int)rect.Right, (int)rect.Top, (int)rect.Bottom);
        }

        #endregion
    }
}