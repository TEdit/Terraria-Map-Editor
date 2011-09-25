using System;
using System.Windows.Media;
using TEdit.TerrariaWorld.Structures;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class TileFrameProperty : ColorProperty // : OOProperty : ObservableObject
    {

        // The potential for circular references of Parent don't make this easy, so these have to be "less private"
        protected internal string _name;
        public virtual string Name
        {
            get { return _name; }
            set { StandardSet<string>(ref _name, ref value, "Name"); }
        }

        protected internal Color _color;

        protected internal bool _isSolid;
        public virtual bool IsSolid
        {
            get { return _isSolid; }
            set { StandardSet<bool>(ref _isSolid, ref value, "IsSolid"); }
        }

        protected internal bool _isSolidTop;
        public virtual bool IsSolidTop
        {
            get { return _isSolidTop; }
            set { StandardSet<bool>(ref _isSolidTop, ref value, "IsSolidTop"); }
        }

        protected internal bool _isHouseItem;
        public virtual bool IsHouseItem
        {
            get { return _isHouseItem; }
            set { StandardSet<bool>(ref _isHouseItem, ref value, "IsHouseItem"); }
        }

        protected internal PointShort _upperLeft;
        public virtual PointShort UpperLeft
        {
            get { return _upperLeft; }
            set { StandardSet<PointShort>(ref _upperLeft, ref value, "UpperLeft"); }
        }

        protected internal PointShort _size;
        public virtual PointShort Size
        {
            get { return _size; }
            set { StandardSet<PointShort>(ref _size, ref value, "Size"); }
        }

        protected internal FrameDirection? _direction;
        public virtual FrameDirection? Direction
        {
            get { return _direction; }
            set { StandardSet<FrameDirection?>(ref _direction, ref value, "Direction"); }
        }

        protected internal string _variety;
        public virtual string Variety
        {
            get { return _variety; }
            set { StandardSet<string>(ref _variety, ref value, "Variety"); }
        }

        protected internal FramePlacement _placement;
        public virtual FramePlacement Placement
        {
            get { return _placement; }
            set { StandardSet<FramePlacement>(ref _placement, ref value, "Placement"); }
        }

        protected internal byte _lightBrightness;
        public virtual byte LightBrightness
        {
            get { return _lightBrightness; }
            set { StandardSet<byte>(ref _lightBrightness, ref value, "LightBrightness"); }
        }

    }
}