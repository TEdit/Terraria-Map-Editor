using System;

namespace TEdit.Common.Structures
{
    [Serializable]
    public class SizeFloat : ObservableObject
    {
        private float _Height;
        private float _Width;

        public SizeFloat(SizeFloat s)
        {
            _Width = s.Width;
            _Height = s.Height;
        }

        public SizeFloat(float width, float height)
        {
            _Width = width;
            _Height = height;
        }

        public float Width
        {
            get { return _Width; }
            set { SetProperty(ref _Width, ref value, "Width"); }
        }

        public float Height
        {
            get { return _Height; }
            set { SetProperty(ref _Height, ref value, "Height"); }
        }

        public float W
        {
            get { return _Width; }
            set { SetProperty(ref _Width, ref value, "Width"); }
        }

        public float H
        {
            get { return _Height; }
            set { SetProperty(ref _Height, ref value, "Height"); }
        }

        public float Total
        {
            get { return _Width * _Height; }
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", Width, Height);
        }

        #region Operator Overrides

        private static bool MatchFields(SizeFloat a, SizeFloat m)
        {
            return (a.Width == m.Width && a.Height == m.Height);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is SizeFloat)
                return MatchFields(this, (SizeFloat) obj);

            return false;
        }

        public bool Equals(SizeFloat p)
        {
            return MatchFields(this, p);
        }

        public static bool operator ==(SizeFloat a, SizeFloat b)
        {
            return MatchFields(a, b);
        }

        public static bool operator !=(SizeFloat a, SizeFloat b)
        {
            return !(a == b);
        }

        public static SizeFloat operator +(SizeFloat a, SizeFloat b)
        {
            return new SizeFloat(a.Width + b.Width, a.Height + b.Height);
        }

        public static SizeFloat operator -(SizeFloat a, SizeFloat b)
        {
            return new SizeFloat(a.Width - b.Width, a.Height - b.Height);
        }

        public static SizeFloat operator /(SizeFloat a, SizeFloat b)
        {
            return new SizeFloat(a.Width/b.Width, a.Height/b.Height);
        }

        public static SizeFloat operator *(SizeFloat a, SizeFloat b)
        {
            return new SizeFloat(a.Width*b.Width, a.Height*b.Height);
        }

        /* float overloads */
        public static SizeFloat operator +(SizeFloat a, float b)
        {
            return new SizeFloat(a.Width + b, a.Height + b);
        }

        public static SizeFloat operator -(SizeFloat a, float b)
        {
            return new SizeFloat(a.Width - b, a.Height - b);
        }

        public static SizeFloat operator /(SizeFloat a, float b)
        {
            return new SizeFloat(a.Width/b, a.Height/b);
        }

        public static SizeFloat operator *(SizeFloat a, float b)
        {
            return new SizeFloat(a.Width*b, a.Height*b);
        }

        public static SizeFloat operator +(float a, SizeFloat b)
        {
            return new SizeFloat(a + b.Width, a + b.Height);
        }

        public static SizeFloat operator -(float a, SizeFloat b)
        {
            return new SizeFloat(a - b.Width, a - b.Height);
        }

        public static SizeFloat operator /(float a, SizeFloat b)
        {
            return new SizeFloat(a/b.Width, a/b.Height);
        }

        public static SizeFloat operator *(float a, SizeFloat b)
        {
            return new SizeFloat(a*b.Width, a*b.Height);
        }

        public override float GetHashCode()
        {
            float result = 17;
            result = result*37 + Width.GetHashCode();
            result = result*37 + Height.GetHashCode();
            return result;
        }

        // one-way; reverse is not guaranteed to not lose data
        public static implicit operator PointFloat(SizeFloat s)
        {
            return new PointFloat(s.Width, s.Height);
        }
        
        // two-way; reverse function is available on other struct
        public static implicit operator PointFloat(SizeFloat s)
        {
            return new PointFloat(s.Width, s.Height);
        }

        #endregion
    }
}