using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.TerrariaWorld
{
    public class Tile
    {
        public Tile()
        {
            IsActive = false;
            Type = 0;
            Frame = new PointShort(-1, -1);
            Wall = 0;
            IsLighted = false;
            Liquid = 0;
            IsLava = false;
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
            return Type.ToString();
        }
    }
}