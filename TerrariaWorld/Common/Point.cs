using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Common
{
    public class Point
    {
        public Point()
        {

        }

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; set; }

        public int Y { get; set; }

        public override string ToString()
        {
            return String.Format("({0},{1})", this.X, this.Y);
        }
    }
}
