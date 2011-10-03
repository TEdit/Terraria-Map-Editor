using TEdit.Common.Structures;

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
                IsActive = (bool) isActive;

            if (wall != null)
                Wall = (byte) wall;

            if (type != null)
                Type = (byte) type;

            if (liquid != null)
                Liquid = (byte) liquid;

            if (isLava != null)
                IsLava = (bool) isLava;

            if (frame != null)
                Frame = (PointShort) frame;
        }

        public override string ToString()
        {
            return Type.ToString();
        }

        public object Clone()
        {
            return MemberwiseClone();
        } 

        #region Operator Overrides

        private static bool matchFields(Tile a, Tile other)
        {
            return a.IsActive == other.IsActive &&
                   a.Type == other.Type &&
                   a.Wall == other.Wall &&
                   a.IsLighted == other.IsLighted &&
                   a.Liquid == other.Liquid &&
                   a.IsLava == other.IsLava &&
                   a.Frame == other.Frame;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Tile i = obj as Tile;
            if ((System.Object)i == null)
            {
                return false;
            }

            // Return true if the fields match:
            return matchFields(this, i);
        }

        public bool Equals(Tile p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return matchFields(this, p);
        }

        public static bool operator ==(Tile a, Tile b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return matchFields(a, b);
        }

        public static bool operator !=(Tile a, Tile b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {

            int result = 13;
            result = result*7 + IsActive.GetHashCode();
            result = result*7 + Type.GetHashCode();
            result = result*7 + Wall.GetHashCode();
            result = result*7 + IsLighted.GetHashCode();
            result = result*7 + Liquid.GetHashCode();
            result = result*7 + IsLava.GetHashCode();
            result = result*7 + Frame.GetHashCode();

            return result;
        }

        #endregion
			
    }
}