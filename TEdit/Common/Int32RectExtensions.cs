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

        public static int GetRight(this Int32Rect rect)
        {
            return rect.X + rect.Width;
        }

        public static int GetBottom(this Int32Rect rect)
        {
            return rect.Y + rect.Height;
        }
    }
}