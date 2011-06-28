using TEdit.TerrariaWorld.Structures;

namespace TEdit.TerrariaWorld
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

        public void UpdateTile(bool? isActive = null,
                               byte? wall = null,
                               byte? type = null,
                               byte? liquid = null,
                               bool? isLava = null,
                               PointShort? frame = null)
        {
            if (isActive != null)
                IsActive = (bool)isActive;

            if (wall != null)
                Wall = (byte) wall;

            if (type != null)
                Type = (byte)type;

            if (liquid != null)
                Liquid = (byte)liquid;

            if (isLava != null)
                IsLava = (bool)isLava;

            if (frame != null)
                Frame = (PointShort)frame;
        }

        public override string ToString()
        {
            return Type.ToString();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}