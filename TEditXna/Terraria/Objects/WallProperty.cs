using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.MvvmLight;

namespace TEditXNA.Terraria.Objects
{
    public class WallProperty : ObservableObject, ITile
    {
        private Color _color;
        private int _id;
        private string _name;
        private bool _isHouse;

        public WallProperty()
        {
            _color = Colors.Magenta;
            _id = -1;
            _name = "UNKNOWN";
        }

        public WallProperty(int id, string name, Color color)
        {
            _color = color;
            _id = id;
            _name = name;
        }

        public bool IsHouse
        {
            get { return _isHouse; }
            set { Set("IsHouse", ref _isHouse, value); }
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

        private WriteableBitmap _image;
        public WriteableBitmap Image
        {
            get { return _image; }
            set { Set("Image", ref _image, value); }
        }
    }
}