using System;
using Microsoft.Xna.Framework;

namespace TEditXna.Editor
{
    public static class RectangleExt
    {
        public static bool Intersect(this Rectangle selection, Rectangle world, out Rectangle intersection)
        {

            int x = Math.Max(selection.X, world.X);
            int y = Math.Max(selection.Y, world.Y);
            int x2 = Math.Min(selection.Right, world.Right);
            int y2 = Math.Min(selection.Bottom, world.Bottom);

            if (x2 - x > 0 || y2 - y > 0)
            {
                intersection = new Rectangle(x, y, x2 - x, y2 - y);
                return true;
            }

            intersection = new Rectangle();
            return false;
        }
    }
}