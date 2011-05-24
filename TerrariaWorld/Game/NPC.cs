using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Game
{
    public class NPC
    {
        public string Name { get; set; }

        public Common.PointF Position { get; set; }

        public bool IsHomeless { get; set; }

        public Common.Point HomeTile { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
