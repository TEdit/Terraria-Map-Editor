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
            Size      = new PointShort(1, 1);
            Placement = FramePlacement.Any;
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

        // XML conversions with defaults
        public bool       XMLConvertBool  (XAttribute attr) { return (bool?) attr ?? false; }
        public string     XMLConvertString(XAttribute attr) { return (string)attr ?? string.Empty; }
        public int        XMLConvertInt   (XAttribute attr) { return (int?)  attr ?? 0; }
        public float      XMLConvertFloat (XAttribute attr) { return (float?)attr ?? 0F; }
        public Color      XMLConvertColor (XAttribute attr)  { return (Color)ColorConverter.ConvertFromString((string)attr); }
        public PointShort XMLConvertPoint (XAttribute attr)
        {
            var ps = PointShort.TryParseInline((string)attr);
            if (attr.Name == "size" && ps == new PointShort(0, 0)) ps = new PointShort(1, 1);
            return ps;
        }
        public FrameDirection XMLConvertDir(XAttribute attr)
        {
            FrameDirection f = FrameDirection.None;
            f = f.Convert<FrameDirection>(attr);
            return f;
        }
        public FramePlacement XMLConvertPlace(XAttribute attr)
        {
            FramePlacement f = FramePlacement.Any;
            f = f.Convert<FramePlacement>(attr);
            return f;
        }
        public ObservableCollection<byte> XMLConvertOC(XAttribute attr)
        {
            var oc = new ObservableCollection<byte>();
            if (!string.IsNullOrWhiteSpace((string)attr))
            {
                string[] split = ((string)attr).Split(',');
                foreach (var s in split)
                {
                    oc.Add((byte)Convert.ChangeType(s, typeof(byte)));
                }
            }

            return oc;
        }
    
    }
}