using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaMapEditor.Renderer
{
    public class TileProperties
    {
        public TileProperties()
        {

        }

        public TileProperties(byte id, System.Drawing.Color color, string name)
        {
            this.ID = id;
            this.Name = name;
            this.Color = color;
        }

        public byte ID { get; set; }
        public string Name { get; set; }
        public System.Drawing.Color Color { get; set; }
    }
}
