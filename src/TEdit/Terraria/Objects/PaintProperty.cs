using System.Windows.Media;
using GalaSoft.MvvmLight;

namespace TEditXNA.Terraria.Objects
{
    public class PaintProperty : ObservableObject
    {
        private Color _color;
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
            set { Set("Color", ref _color, value); }
        }

        public int Id
        {
            get { return _id; }
            set { Set("Id", ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }
    }
}