using System;
using System.Windows.Media;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class TileProperty : ColorProperty
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

        private bool _isSolid;
        public bool IsSolid
        {
            get { return _isSolid; }
            set
            {
                if (_isSolid != value)
                {
                    _isSolid = value;
                    RaisePropertyChanged("IsSolid");
                }
            }
        }

        private bool _IsSolidTop;
        public bool IsSolidTop
        {
            get { return this._IsSolidTop; }
            set
            {
                if (this._IsSolidTop != value)
                {
                    this._IsSolidTop = value;
                    this.RaisePropertyChanged("IsSolidTop");
                }
            }
        }


    }

    [Serializable]
    public class ItemProperty : ObservableObject
    {
        private int _id;
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged("Id");
                }
            }
        }

        private string _name;
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
}