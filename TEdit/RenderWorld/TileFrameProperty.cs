using System;
using System.Windows.Media;
using TEdit.TerrariaWorld.Structures;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class TileFrameProperty : ColorProperty // : TileItemBase : ObservableObject
    {

        public override string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, ref value, "Name"); }
        }

        protected internal bool _isSolid;
        public virtual bool IsSolid
        {
            get { return _isSolid; }
            set { SetProperty(ref _isSolid, ref value, "IsSolid"); }
        }

        protected internal bool _isSolidTop;
        public virtual bool IsSolidTop
        {
            get { return _isSolidTop; }
            set { SetProperty(ref _isSolidTop, ref value, "IsSolidTop"); }
        }

        protected internal bool _isHouseItem;
        public virtual bool IsHouseItem
        {
            get { return _isHouseItem; }
            set { SetProperty(ref _isHouseItem, ref value, "IsHouseItem"); }
        }

        protected internal PointShort _upperLeft;
        public virtual PointShort UpperLeft
        {
            get { return _upperLeft; }
            set { SetProperty(ref _upperLeft, ref value, "UpperLeft"); }
        }

        protected internal PointShort _size;
        public virtual PointShort Size
        {
            get { return _size; }
            set { SetProperty(ref _size, ref value, "Size"); }
        }

        protected internal FrameDirection? _direction;
        public virtual FrameDirection? Direction
        {
            get { return _direction; }
            set { SetProperty(ref _direction, ref value, "Direction"); }
        }

        protected internal string _variety;
        public virtual string Variety
        {
            get { return _variety; }
            set { SetProperty(ref _variety, ref value, "Variety"); }
        }

        protected internal FramePlacement _placement;
        public virtual FramePlacement Placement
        {
            get { return _placement; }
            set { SetProperty(ref _placement, ref value, "Placement"); }
        }

        protected internal TileNumArray _growsOn;
        public virtual TileNumArray GrowsOn
        {
            get { return _growsOn; }
            set { SetProperty(ref _growsOn, ref value, "GrowsOn"); }
        }

        protected internal TileNumArray _hangsOn;
        public virtual TileNumArray HangsOn
        {
            get { return _hangsOn; }
            set { SetProperty(ref _hangsOn, ref value, "HangsOn"); }
        }

        protected internal byte _lightBrightness;
        public virtual byte LightBrightness
        {
            get { return _lightBrightness; }
            set { SetProperty(ref _lightBrightness, ref value, "LightBrightness"); }
        }

        protected internal ushort _contactDmg;
        public virtual ushort ContactDmg
        {
            get { return _contactDmg; }
            set { SetProperty(ref _contactDmg, ref value, "ContactDmg"); }
        }


    }
}