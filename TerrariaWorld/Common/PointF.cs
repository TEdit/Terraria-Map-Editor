using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Common
{
    public class PointF
    {
        public PointF()
        {

        }

        public PointF(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public float X { get; set; }

        public float Y { get; set; }

        public override string ToString()
        {
            return String.Format("({0},{1})", this.X, this.Y);
        }

    }
}
