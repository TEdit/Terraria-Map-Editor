using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using TEdit.Common.Structures;
using TEdit.Common;
using System.Linq;
using System.Windows.Media;
namespace TEdit.RenderWorld
{
    [Serializable]
    public class FrameProperty : ColorProperty, ICloneable
    {
        // real variables with defaults
        private ObservableCollection<AttachGroup> _attachesTo = new ObservableCollection<AttachGroup>();
        private ObservableCollection<TileFrame> _canMorphFrom = new ObservableCollection<TileFrame>();
        private ObservableCollection<TileFrame> _noAttach     = new ObservableCollection<TileFrame>();
        private int _contactDamage        = 0;
        private FrameDirection _direction = 0;
        private bool _isHouseItem         = false;
        private bool _isSolid             = false;
        private bool _isSolidTop          = false;
        private float _lightBrightness    = 0;
        private FramePlacement _placement = FramePlacement.Any;
        private PointShort _size          = new PointShort(1, 1);
        private PointShort _upperLeft     = new PointShort();
        private string _variety           = String.Empty;

        public ObservableCollection<AttachGroup> AttachesTo
        {
            get { return _attachesTo; }
            set { _attachesTo.ReplaceRange(value); }
        }

        public ObservableCollection<TileFrame> CanMorphFrom
        {
            get { return _canMorphFrom; }
            set { _canMorphFrom.ReplaceRange(value); }
        }

        public ObservableCollection<TileFrame> NoAttach
        {
            get { return _noAttach; }
            set { _noAttach.ReplaceRange(value); }
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

            // go deep...
            clone._attachesTo   = new ObservableCollection<AttachGroup>(this.AttachesTo.DeepClone().ToList());
            clone._canMorphFrom = new ObservableCollection<TileFrame>  (this.CanMorphFrom.DeepClone().ToList());
            clone._noAttach     = new ObservableCollection<TileFrame>  (this.NoAttach.DeepClone().ToList());
            
            return clone;
        }
    
    }
}