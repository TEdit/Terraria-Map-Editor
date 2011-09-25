using System;
using System.Windows.Media;
using System.Reflection;
using TEdit.Common;
using TEdit.TerrariaWorld.Structures;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class FrameProperty : TileFrameProperty
    {

        public FrameProperty()
        {
            ID = 0;
            UpperLeft = new PointShort(0, 0);
        }

        // If object is Null/Empty/Whitespace, looks for any data in the parent, creating parental inheritence of Tiles/Frames properties
        public T InheritCheck<T>(T chk, string parentVar)
        {
            T temp = chk;
            if (String.IsNullOrWhiteSpace(temp.ToString()) && Parent != null)
                temp = (T)typeof(TileProperty).GetProperty(parentVar).GetValue(Parent, null);

            return temp;
        }

        // Must be here and referencing TileProperty to prevent circuir references within TileFrameProperty
        // (this means some overrides below)
        private TileProperty _parent;
        public TileProperty Parent { get { return _parent; } }
        // Special SetParent method for ref passing
        public void SetParent (ref TileProperty tile) { StandardSet<TileProperty>(ref _parent, ref tile, "Parent"); }

        // InheritCheck overrides
        public override string Name
        {
            get { return InheritCheck<string>(_name, "Name"); }
            set { StandardSet<string>(ref _name, ref value, "Name"); }
        }
        public override Color Color {
            get { return InheritCheck<Color>(_color, "Color"); }
            set { StandardSet<Color>(ref _color, ref value, "Color"); }
        }
        public override bool IsSolid
        {
            get { return InheritCheck<bool>(_isSolid, "IsSolid"); }
            set { StandardSet<bool>(ref _isSolid, ref value, "IsSolid"); }
        }
        public override bool IsSolidTop
        {
            get { return InheritCheck<bool>(_isSolidTop, "IsSolidTop"); }
            set { StandardSet<bool>(ref _isSolidTop, ref value, "IsSolidTop"); }
        }
        public override bool IsHouseItem
        {
            get { return InheritCheck<bool>(_isHouseItem, "IsHouseItem"); }
            set { StandardSet<bool>(ref _isHouseItem, ref value, "IsHouseItem"); }
        }
        public override PointShort UpperLeft
        {
            get { return InheritCheck<PointShort>(_upperLeft, "UpperLeft"); }
            set { StandardSet<PointShort>(ref _upperLeft, ref value, "UpperLeft"); }
        }
        public override PointShort Size
        {
            get { return InheritCheck<PointShort>(_size, "Size"); }
            set { StandardSet<PointShort>(ref _size, ref value, "Size"); }
        }
        public override FrameDirection? Direction
        {
            get { return InheritCheck<FrameDirection?>(_direction, "Direction"); }
            set { StandardSet<FrameDirection?>(ref _direction, ref value, "Direction"); }
        }
        public override string Variety
        {
            get { return InheritCheck<string>(_variety, "Variety"); }
            set { StandardSet<string>(ref _variety, ref value, "Variety"); }
        }
        public override FramePlacement Placement
        {
            get { return InheritCheck<FramePlacement>(_placement, "Placement"); }
            set { StandardSet<FramePlacement>(ref _placement, ref value, "Placement"); }
        }
        public virtual byte LightBrightness
        {
            get { return InheritCheck<byte>(_lightBrightness, "LightBrightness"); }
            set { StandardSet<byte>(ref _lightBrightness, ref value, "LightBrightness"); }
        }

        public override string ToString()
        {
            string n = Name;
            if (!String.IsNullOrEmpty(Variety)) n += " - " + Variety;
            if (!String.IsNullOrEmpty(((DisplayFrameDirection?)Direction).ToString())) n += " (" + ((DisplayFrameDirection?)Direction).ToString() + ")";

            return n;
        }
    }
}