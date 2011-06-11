using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Common
{
    public class RectF
    {
        public RectF()
        {

        }

        public RectF(PointF topLeft, PointF bottomRight)
        {
            this.TopLeft = topLeft;
            this.BottomRight = bottomRight;
        }

        public RectF(float left, float right, float top, float bottom)
        {
            this.TopLeft = new PointF(left, top);
            this.BottomRight = new PointF(right, bottom);
        }

        public float Left
        {
            get { return this.TopLeft.X; }
        }

        public float Right
        {
            get { return this.BottomRight.X; }
        }

        public float Top
        {
            get { return this.TopLeft.Y; }
        }

        public float Bottom
        {
            get { return this.BottomRight.Y; }
        }

        public PointF TopLeft { get; set; }

        public PointF BottomRight { get; set; }

        public override string ToString()
        {
            return String.Format("[{0}, {1}]", this.TopLeft, this.BottomRight);
        }


    }
}
