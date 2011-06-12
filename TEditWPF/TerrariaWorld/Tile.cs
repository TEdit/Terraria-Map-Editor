using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.TerrariaWorld
{
    public class Tile
    {
        public Tile()
        {
            this.IsActive = false;
            this.Type = 0;
            this.Frame = new PointShort(-1, -1);
            this.Wall = 0;
            this.IsLighted = false;
            this.Liquid = 0;
            this.IsLava = false;
        }

        public bool IsActive { get; set; }

        public byte Type { get; set; }

        public PointShort Frame { get; set; }

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