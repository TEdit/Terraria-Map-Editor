using System;
using BCCL.Geometry.Primitives;

namespace TEditXNA.Terraria
{
    [Serializable]
    public class Tile
    {
        public bool IsActive;
        public bool HasWire;
        public bool HasWire2;
        public bool HasWire3;
        public bool IsLava;
        public bool IsHoney;
        public byte Color;
        public byte Type;
        public byte Wall;
        public byte WallColor;
        public byte Liquid;
        public bool HalfBrick;
        public byte Slope;
        public bool Actuator;
        public bool InActive;
        public Int16 U, V;

        [NonSerialized] /* Heathtech */
        public ushort uvTileCache = 0xFFFF; //Caches the UV position of a tile, since it is costly to generate each frame
        [NonSerialized] /* Heathtech */
        public ushort uvWallCache = 0xFFFF; //Caches the UV position of a wall tile
        [NonSerialized] /* Heathtech */
        public byte lazyMergeId = 0xFF; //The ID here refers to a number that helps blocks know whether they are actually merged with a nearby tile
        [NonSerialized] /* Heathtech */
        public bool hasLazyChecked = false; //Whether the above check has taken place

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

        protected bool Equals(Tile other)
        {
            return IsActive.Equals(other.IsActive) &&
                Type == other.Type &&
                Color == other.Color &&
                U == other.U && V == other.V && 
                Liquid == other.Liquid &&               
                IsLava.Equals(other.IsLava) &&
                IsHoney.Equals(other.IsHoney) &&
                Wall == other.Wall && 
                WallColor == other.WallColor &&
                HasWire.Equals(other.HasWire) &&
                HasWire2.Equals(other.HasWire2) &&
                HasWire3.Equals(other.HasWire3) &&
                HalfBrick.Equals(other.HalfBrick) && 
                Slope == other.Slope &&
                Actuator.Equals(other.Actuator) && 
                InActive.Equals(other.InActive);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Tile)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = IsActive.GetHashCode();
                hashCode = (hashCode * 397) ^ HasWire.GetHashCode();
                hashCode = (hashCode * 397) ^ HasWire2.GetHashCode();
                hashCode = (hashCode * 397) ^ HasWire3.GetHashCode();
                hashCode = (hashCode * 397) ^ IsHoney.GetHashCode();
                hashCode = (hashCode * 397) ^ IsLava.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ Wall.GetHashCode();
                hashCode = (hashCode * 397) ^ Type.GetHashCode();
                hashCode = (hashCode * 397) ^ WallColor.GetHashCode();
                hashCode = (hashCode * 397) ^ Liquid.GetHashCode();
                hashCode = (hashCode * 397) ^ HalfBrick.GetHashCode();
                hashCode = (hashCode * 397) ^ Actuator.GetHashCode();
                hashCode = (hashCode * 397) ^ Slope.GetHashCode();
                hashCode = (hashCode * 397) ^ InActive.GetHashCode();
                hashCode = (hashCode * 397) ^ U.GetHashCode();
                hashCode = (hashCode * 397) ^ V.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Tile left, Tile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Tile left, Tile right)
        {
            return !Equals(left, right);
        }
    }
}