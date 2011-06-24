using System.Windows.Media;
using TEditWPF.Common;

namespace TEditWPF.RenderWorld
{
    public class TileProperties : ObservableObject
    {
        private Color _Color;
        private byte _ID;
        private string _Name;

        public TileProperties()
        {
        }

        public TileProperties(byte id, Color color, string name)
        {
            _ID = id;
            _Name = name;
            _Color = color;
        }

        public byte ID
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

        public Color Color
        {
            get { return _Color; }
            set
            {
                if (_Color != value)
                {
                    _Color = value;
                    RaisePropertyChanged("Color");
                }
            }
        }
    }
}