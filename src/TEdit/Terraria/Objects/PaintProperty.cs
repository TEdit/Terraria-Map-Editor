using System.Windows.Media;
using GalaSoft.MvvmLight;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace TEdit.Terraria.Objects
{
    public class PaintProperty : ObservableObject
    {
        private Color _color;
        private XnaColor _paintColor;
        private int _id;
        private string _name;

        public PaintProperty()
        {
            _color = Colors.Magenta;
            _id = -1;
            _name = "UNKNOWN";
        }

        public PaintProperty(int id, string name, Color color)
        {
            _color = color;
            _id = id;
            _name = name;
        }


        public Color Color
        {
            get { return _color; }
            set
            {
                Set(nameof(Color), ref _color, value);
                _paintColor = new XnaColor(value.R, value.G, value.B, value.A);
                RaisePropertyChanged("PaintColor");
            }
        }

        public XnaColor PaintColor => _paintColor;

        public int Id
        {
            get { return _id; }
            set { Set(nameof(Id), ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(nameof(Name), ref _name, value); }
        }
    }
}