using System;
using System.Collections.ObjectModel;
using TEdit.Common.Structures;
using TEdit.Common;
using System.Linq;
namespace TEdit.RenderWorld
{
    [Serializable]
    public class FrameProperty : ColorProperty, ICloneable
    {
        private readonly ObservableCollection<byte> _attachesTo = new ObservableCollection<byte>();
        private readonly ObservableCollection<byte> _canReplace = new ObservableCollection<byte>();
        private readonly ObservableCollection<byte> _growsOn = new ObservableCollection<byte>();
        private int _contactDamage;
        private FrameDirection _direction;
        private bool _isHouseItem;
        private bool _isSolid;
        private bool _isSolidTop;
        private float _lightBrightness;
        private FramePlacement _placement;
        private PointShort _size;
        private PointShort _upperLeft;
        private string _variety;

        public FrameProperty()
        {
            ID = 0;
            UpperLeft = new PointShort(0, 0);
        }

        public ObservableCollection<byte> AttachesTo
        {
            get { return _attachesTo; }
        }

        public ObservableCollection<byte> CanReplace
        {
            get { return _canReplace; }
        }

        public ObservableCollection<byte> GrowsOn
        {
            get { return _growsOn; }
        }

        public int ContactDamage
        {
            get { return _contactDamage; }
            set { SetProperty(ref _contactDamage, ref value, "ContactDamage"); }
        }

        public float LightBrightness
        {
            get { return _lightBrightness; }
            set { SetProperty(ref _lightBrightness, ref value, "LightBrightness"); }
        }

        public FramePlacement Placement
        {
            get { return _placement; }
            set { SetProperty(ref _placement, ref value, "Placement"); }
        }

        public string Variety
        {
            get { return _variety; }
            set { SetProperty(ref _variety, ref value, "Variety"); }
        }

        public FrameDirection Direction
        {
            get { return _direction; }
            set { SetProperty(ref _direction, ref value, "Direction"); }
        }

        public PointShort Size
        {
            get { return _size; }
            set { SetProperty(ref _size, ref value, "Size"); }
        }

        public PointShort UpperLeft
        {
            get { return _upperLeft; }
            set { SetProperty(ref _upperLeft, ref value, "UpperLeft"); }
        }

        public bool IsHouseItem
        {
            get { return _isHouseItem; }
            set { SetProperty(ref _isHouseItem, ref value, "IsHouseItem"); }
        }

        public bool IsSolidTop
        {
            get { return _isSolidTop; }
            set { SetProperty(ref _isSolidTop, ref value, "IsSolidTop"); }
        }

        public bool IsSolid
        {
            get { return _isSolid; }
            set { SetProperty(ref _isSolid, ref value, "IsSolid"); }
        }


        public override string ToString()
        {
            return string.Format("{0} - {1}[{2}]", base.Name, Variety, Direction);
        }

        public object Clone()
        {
            var clone = (FrameProperty)this.MemberwiseClone();
            clone.AttachesTo.ReplaceRange(this.AttachesTo.ToList());
            clone.GrowsOn.ReplaceRange(this.AttachesTo.ToList());
            clone.CanReplace.ReplaceRange(this.AttachesTo.ToList());

            return clone;
        }
    }
}