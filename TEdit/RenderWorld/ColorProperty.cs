using System;
using System.Windows.Media;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class TileProperty : TileFrameProperty
    {
        private bool _isFramed;
        public bool IsFramed
        {
            get { return _isFramed; }
            set
            {
                if (_isFramed != value)
                {
                    _isFramed = value;
                    RaisePropertyChanged("IsFramed");
                }
            }
        }

        private class _Frames : ObservableCollection<FrameProperty> {};
        public class Frames : ObservableCollection<FrameProperty> 
        {
            get { return _Frames; }
            set
            {
                if (!_Frames.Equal(value))
                {
                    _Frames.Clear();
                    foreach (FrameProperty f in value) {
                        f.Parent = this;
                        _Frames.Add(f);
                    }
                    RaisePropertyChanged("Frames");
                }
            }
        }
        
    }

    // So far, no unique properties for Frame tags, so this is merely a derived instance 
    [Serializable]
    public class FrameProperty : TileFrameProperty {}

    // Common items for both Tile and Frame tags, with parental inherience
    // (Using the public object for the set method, so that value ?? parent == parent will not change anything)
    [Serializable]
    public class TileFrameProperty : ColorProperty
    {
        private bool _IsSolid;
        public bool IsSolid
        {
            get { return _IsSolid ?? (this.Parent ? this.Parent.IsSolid : null); }
            set
            {
                if (IsSolid != value)
                {
                    _IsSolid = value;
                    RaisePropertyChanged("IsSolid");
                }
            }
        }

        private bool _IsSolidTop;
        public bool IsSolidTop
        {
            get { return _IsSolidTop ?? (this.Parent ? this.Parent.IsSolidTop : null); }
            set
            {
                if (IsSolidTop != value)
                {
                    _IsSolidTop = value;
                    RaisePropertyChanged("IsSolidTop");
                }
            }
        }

        private bool _IsHouseItem;
        public bool IsHouseItem
        {
            get { return _IsHouseItem ?? (this.Parent ? this.Parent.IsHouseItem : null); }
            set
            {
                if (IsHouseItem != value)
                {
                    IsHouseItem = value;
                    RaisePropertyChanged("IsHouseItem");
                }
            }
        }

        private bool _CanMixFrames;
        public bool CanMixFrames
        {
            get { return _CanMixFrames ?? (this.Parent ? this.Parent.CanMixFrames : null); }
            set
            {
                if (CanMixFrames != value)
                {
                    _CanMixFrames = value;
                    RaisePropertyChanged("CanMixFrames");
                }
            }
        }

        private string _Variety;
        public string Variety
        {
            get { return _Variety ?? (this.Parent ? this.Parent.Variety : null); }
            set
            {
                if (Variety != value)
                {
                    _Variety = value;
                    RaisePropertyChanged("Variety");
                }
            }
        }

        private char _Dir;
        public char Dir
        {
            get { return _Dir ?? (this.Parent ? this.Parent.Dir : null); }
            set
            {
                if (Dir != value)
                {
                    _Dir = value;
                    RaisePropertyChanged("Dir");
                }
            }
        }
        private byte[] _GrowsOn;
        public byte[] GrowsOn
        {
            get { return _GrowsOn ?? (this.Parent ? this.Parent.GrowsOn : null); }
            set
            {
                
                if (!GrowsOn.Equals(value.Count ? value : null))
                {
                    _GrowsOn = value;
                    RaisePropertyChanged("GrowsOn");
                }
            }
        }

        private byte[] _HangsOn;
        public byte[] HangsOn
        {
            get { return _HangsOn ?? (this.Parent ? this.Parent.HangsOn : null); }
            set
            {
                if (!HangsOn.Equals(value.Count ? value : null))
                {
                    _HangsOn = value;
                    RaisePropertyChanged("HangsOn");
                }
            }
        }

        private byte _LightBrightness;
        public byte LightBrightness
        {
            get { return _LightBrightness ?? (this.Parent ? this.Parent.LightBrightness : null); }
            set
            {
                if (LightBrightness != (byte)value)
                {
                    _LightBrightness = (byte)value;
                    RaisePropertyChanged("LightBrightness");
                }
            }
        }

        private ushort _ContactDmg;
        public ushort ContactDmg
        {
            get { return _ContactDmg ?? (this.Parent ? this.Parent.ContactDmg : null); }
            set
            {
                if (ContactDmg != (ushort)value)
                {
                    _ContactDmg = (ushort)value;
                    RaisePropertyChanged("ContactDmg");
                }
            }
        }

        private class _Size : SizeProperty {};
        public class Size : SizeProperty {}
        {
            get { return _Size ?? (this.Parent ? this.Parent.Size : null); }
            set
            {
                if (!Size.Equal(value))
                {
                    _Size = value;
                    RaisePropertyChanged("Size");
                }
            }
        }

        private class _UpperLeft : XYProperty {};
        public class UpperLeft : XYProperty {}
        {
            get { return _UpperLeft ?? (this.Parent ? this.Parent.UpperLeft : null); }
            set
            {
                if (!UpperLeft.Equal(value))
                {
                    _UpperLeft = value;
                    RaisePropertyChanged("UpperLeft");
                }
            }
        }

        private class _Placement : PlacementProperty {};
        public class Placement : PlacementProperty {}
        {
            get { return _Placement ?? (this.Parent ? this.Parent.Placement : null); }
            set
            {
                if (!Placement.Equal(value))
                {
                    _Placement = value;
                    RaisePropertyChanged("Placement");
                }
            }
        }

        public string FullID {
            get { return (this.Parent ? this.Parent.FullID + '.' : '') + this.ID.ToString(); }
            set {}
        }
        
        // This is basically just a placeholder for an object ref; hopefully object is generic enough...
        private object _Parent;
        public object Parent {
            get { return _Parent; }
            set { 
                if (!_Parent.Equal(value))
                {
                    _Parent = value;
                    RaisePropertyChanged("Parent");
                }                
            }
        }
        
    }

    [Serializable]
    public class ItemProperty : ObservableObject
    {
        private ushort _ID;
        public ushort ID
        {
            get { return _ID; }
            set
            {
                if (_ID != value)
                {
                    _ID = value;
                    RaisePropertyChanged("ID");
                }
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        private string _Type;
        public string Type
        {
            get { return _Type; }
            set
            {
                if (_Type != value)
                {
                    _Type = (string)value.ToLower;
                    RaisePropertyChanged("Type");
                }
            }
        }

        // (Should fix any old code trying use this like a string...)
        public override string ToString() { return Name; }
    }

    [Serializable]
    public class ColorProperty : ObservableObject
    {
        private Color _color;
        private byte _id;
        private string _name;

        public ColorProperty()
        {
        }

        public ColorProperty(byte id, Color color, string name)
        {
            _id = id;
            _name = name;
            _color = color;
        }

        public byte ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged("ID");
                }
            }
        }


        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    RaisePropertyChanged("Color");
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0}|{1}|#{2:x2}{3:x2}{4:x2}{5:x2}", ID, Name, Color.A, Color.R, Color.G, Color.B);
        }
    }

    [Serializable]
    public class SizeProperty : ObservableObject
    {
        private byte _W = 1;
        private byte _H = 1;

        public SizeProperty()
        {
        }

        public SizeProperty(string WxH)
        {
            string[] WH = WxH.Split( new[] { 'x', ', ', ',' } );
            _W = byte.Parse(WH[0] ?? 1);
            _H = byte.Parse(WH[1] ?? 1);
        }

        public SizeProperty(byte W, byte H)
        {
            _W = W ?? 1;
            _H = H ?? 1;
        }

        public byte W
        {
            get { return _W; }
            set
            {
                if (_W != value)
                {
                    _W = value;
                    RaisePropertyChanged("W");
                }
            }
        }

        public byte H
        {
            get { return _H; }
            set
            {
                if (_H != value)
                {
                    _H = value;
                    RaisePropertyChanged("H");
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0}x{1}", W, H);
        }
    }

    // Very similar to SizeProperty
    [Serializable]
    public class XYProperty : ObservableObject
    {
        private int _X = 0;
        private int _Y = 0;

        public XYProperty()
        {
        }

        public XYProperty(string XxY)
        {
            string[] XY = XxY.Split( new[] { ', ', ',', 'x' } );
            _X = int.Parse(XY[0] ?? 0);
            _Y = int.Parse(XY[1] ?? 0);
        }

        public XYProperty(int X, int Y)
        {
            _X = X ?? 0;
            _Y = Y ?? 0;
        }

        public byte X
        {
            get { return _X; }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    RaisePropertyChanged("X");
                }
            }
        }

        public byte Y
        {
            get { return _Y; }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    RaisePropertyChanged("Y");
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0},{1}", X, Y);
        }
    }


    [Serializable]
    public class PlacementProperty : ObservableObject
    {
        private byte _Wall    = 1;
        private byte _Ceiling = 1;
        private byte _Floor   = 1;
        private byte _Surface = 1;
        private byte _Float   = 1;
        // TODO: Consider merging growsOn/hangsOn here...

        public PlacementProperty()
        {
        }

        public PlacementProperty(string placement)
        {
            placement = placement.toLower;
            
            _Wall    = placement.Contains('wall')    ? 1 : 0;
            _Ceiling = placement.Contains('ceiling') ? 1 : 0;
            _Floor   = placement.Contains('floor')   ? 1 : 0;
            _Surface = placement.Contains('surface') ? 1 : 0;
            _Float   = placement == 'any'            ? 1 : 0;
            
            if (placement == 'cfboth') {
               _Ceiling = 2;
               _Floor   = 2;
            }
        }

        public byte Wall
        {
            get { return _Wall; }
            set
            {
                if (_Wall != value)
                {
                    _Wall = value;
                    RaisePropertyChanged("Wall");
                }
            }
        }

        public byte Ceiling
        {
            get { return _Ceiling; }
            set
            {
                if (_Ceiling != value)
                {
                    _Ceiling = value;
                    RaisePropertyChanged("Ceiling");
                }
            }
        }

        public byte Floor
        {
            get { return _Floor; }
            set
            {
                if (_Floor != value)
                {
                    _Floor = value;
                    RaisePropertyChanged("Floor");
                }
            }
        }

        public byte Surface
        {
            get { return _Surface; }
            set
            {
                if (_Surface != value)
                {
                    _Surface = value;
                    RaisePropertyChanged("Surface");
                }
            }
        }

        public byte Float
        {
            get { return _Float; }
            set
            {
                if (_Float != value)
                {
                    _Float = value;
                    RaisePropertyChanged("Float");
                }
            }
        }

        public override string ToString()
        {
            if (Ceiling == 2 && Floor == 2) return 'CFBoth';
            if (Float)                      return 'any';
            
            s = new String((Wall    ? 'Wall'    : '') +
                           (Floor   ? 'Floor'   : '') +
                           (Ceiling ? 'Ceiling' : '') +
                           (Surface ? 'Surface' : ''));
            return char.ToLower(s[0]) + s.Substring(1);
        }
    }


}
