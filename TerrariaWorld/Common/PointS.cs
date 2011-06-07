using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Common
{
    public class PointS
    {
        public PointS()
        {
            this.X = -1;
            this.Y = -1;
        }

        public PointS(short x, short y)
        {
            this.X = x;
            this.Y = y;
        }

        public short X { get; set; }

        public short Y { get; set; }

        public override string ToString()
        {
            return String.Format("({0},{1})", this.X, this.Y);
        }
    }
}
