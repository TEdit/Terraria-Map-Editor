using System.Windows;
using TEdit.Common.Structures;

namespace TEdit.Common
{
    public static class Int32RectExtensions
    {
        public static bool Contains(this Int32Rect rect, PointInt32 point)
        {
            int xabs = (point.X - rect.X);
            int yabs = (point.Y - rect.Y);

            if (xabs < 0)
                return false;

            if (yabs < 0)
                return false;

            return xabs < rect.Width && yabs < rect.Height;
        }

        public static int Left(this Int32Rect rect)
        {
            return rect.X;
        }

        public static int Right(this Int32Rect rect)
        {
            return rect.X + rect.Width - 1;
        }

        public static int Top(this Int32Rect rect)
        {
            return rect.Y;
        }

        public static int Bottom(this Int32Rect rect)
        {
            return rect.Y + rect.Height - 1;
        }

        public static PointInt32 TopLeft(this Int32Rect rect)
        {
            return new PointInt32(rect.X, rect.Y);
        }

        public static PointInt32 BottomRight(this Int32Rect rect)
        {
            return new PointInt32(rect.X + rect.Width - 1, rect.Y + rect.Height - 1);
        }

        public static PointInt32 TopRight(this Int32Rect rect)
        {
            return new PointInt32(rect.X + rect.Width - 1, rect.Y);
        }

        public static PointInt32 BottomLeft(this Int32Rect rect)
        {
            return new PointInt32(rect.X, rect.Y + rect.Height - 1);
        }

        public static SizeInt32 Size(this Int32Rect rect)
        {
            return new SizeInt32(rect.Width, rect.Height);
        }

    }
}