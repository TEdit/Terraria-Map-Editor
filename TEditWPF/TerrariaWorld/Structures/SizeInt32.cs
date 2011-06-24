using System;
using TEditWPF.Common;

namespace TEditWPF.TerrariaWorld.Structures
{
    public class SizeInt32 : ObservableObject
    {
        public SizeInt32(int width, int height)
        {
            _Width = width;
            _Height = height;
        }

        private int _Width;
        public int Width
        {
            get { return this._Width; }
            set
            {
                if (this._Width != value)
                {
                    this._Width = value;
                    this.RaisePropertyChanged("Width");
                }
            }
        }

        private int _Height;
        public int Height
        {
            get { return this._Height; }
            set
            {
                if (this._Height != value)
                {
                    this._Height = value;
                    this.RaisePropertyChanged("Height");
                }
            }
        }


        public override string ToString()
        {
            return String.Format("({0}, {1})", this.Width, this.Height);
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
                return MatchFields(this, (SizeInt32)obj);

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
            return new SizeInt32(a.Width / b.Width, a.Height / b.Height);
        }

        public static SizeInt32 operator *(SizeInt32 a, SizeInt32 b)
        {
            return new SizeInt32(a.Width * b.Width, a.Height * b.Height);
        }

        public override int GetHashCode()
        {
            int result = 17;
            result = result * 37 + Width.GetHashCode();
            result = result * 37 + Height.GetHashCode();
            return result;
        }

        #endregion
    }

}