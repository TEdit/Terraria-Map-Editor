using System.Windows.Media;

namespace TEditWPF.RenderWorld
{
    public class TileProperties
    {
        public TileProperties()
        {

        }

        public TileProperties(byte id, Color color, string name)
        {
            this.ID = id;
            this.Name = name;
            this.Color = color;
        }

        public byte ID { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
    }
}
