using System;

namespace TEditXNA.Terraria
{
    [Serializable]
    public class Tile
    {
        public bool IsActive;
        public bool HasWire;
        public bool IsLava;
        
        public byte Type;
        public byte Wall;
        public byte Liquid;
        public Int16 U, V;

        #region BitFlags
        //private const int IsActivePosition = 0;
        //private const int IsLavaPosition = 1;
        //private const int HasWirePosition = 2;
        
        // Condense all bools to one byte, slow saves ~100mb memory in large world,
        //private byte _flags;
        //public bool IsActive
        //{
        //    set
        //    {
        //        if (value)
        //            _flags = (byte)(_flags | (1 << (IsActivePosition%8)));
        //        else
        //            _flags = (byte)(_flags & ~(1 << (IsActivePosition % 8)));
        //    }
        //    get
        //    {
        //        return (_flags & (1 << (IsActivePosition % 8))) != 0;
        //    }
        //}

        //public bool HasWire
        //{
        //    set
        //    {
        //        if (value)
        //            _flags = (byte)(_flags | (1 << (HasWirePosition % 8)));
        //        else
        //            _flags = (byte)(_flags & ~(1 << (HasWirePosition % 8)));
        //    }
        //    get
        //    {
        //        return (_flags & (1 << (HasWirePosition % 8))) != 0;
        //    }
        //}
        //public bool IsLava
        //{
        //    set
        //    {
        //        if (value)
        //            _flags = (byte)(_flags | (1 << (IsLavaPosition % 8)));
        //        else
        //            _flags = (byte)(_flags & ~(1 << (IsLavaPosition % 8)));
        //    }
        //    get
        //    {
        //        return (_flags & (1 << (IsLavaPosition % 8))) != 0;
        //    }
        //}
        #endregion

        public object Clone()
        {
            return MemberwiseClone();
        }

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
            if (obj.GetType() != typeof(Tile)) return false;
            return Equals((Tile)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = IsActive.GetHashCode();
                result = (result * 397) ^ Type.GetHashCode();
                result = (result * 397) ^ Wall.GetHashCode();
                result = (result * 397) ^ Liquid.GetHashCode();
                result = (result * 397) ^ IsLava.GetHashCode();
                result = (result * 397) ^ U.GetHashCode();
                result = (result * 397) ^ V.GetHashCode();
                result = (result * 397) ^ HasWire.GetHashCode();
                return result;
            }
        }
    }
}