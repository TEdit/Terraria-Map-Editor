using System;

namespace TEditXNA.Terraria
{
    [Serializable]
    public class Tile
    {
        public bool IsActive;
        public byte Type;
        public byte Wall;
        public byte Liquid;
        public bool IsLava;
        public Int16 U, V, WallU, WallV;
        public double Light;
        public bool HasWire;

        public bool Equals(Tile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.IsActive.Equals(IsActive) && other.Type == Type && other.Wall == Wall && other.Liquid == Liquid && other.IsLava.Equals(IsLava) && other.U == U && other.V == V && other.HasWire.Equals(HasWire);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Tile)) return false;
            return Equals((Tile) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = IsActive.GetHashCode();
                result = (result*397) ^ Type.GetHashCode();
                result = (result*397) ^ Wall.GetHashCode();
                result = (result*397) ^ Liquid.GetHashCode();
                result = (result*397) ^ IsLava.GetHashCode();
                result = (result*397) ^ U.GetHashCode();
                result = (result*397) ^ V.GetHashCode();
                result = (result*397) ^ HasWire.GetHashCode();
                return result;
            }
        }
    }
}