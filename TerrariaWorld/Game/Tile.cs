using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Game
{
    public class Tile
    {
        public Tile()
        {
            this.IsActive = false;
            this.Type = 0;
            this.Frame = new Common.PointS(-1, -1);
            this.Wall = 0;
            this.IsLighted = false;
            this.Liquid = 0;
            this.IsLava = false;
        }

        public bool IsActive { get; set; }

        public byte Type { get; set; }

        public Common.PointS Frame { get; set; }

        public byte Wall { get; set; }

        public bool IsLighted { get; set; }

        public byte Liquid { get; set; }

        public bool IsLava { get; set; }

        public override string ToString()
        {
            return this.Type.ToString();
        }
    }
}
