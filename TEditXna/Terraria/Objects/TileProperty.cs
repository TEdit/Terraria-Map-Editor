using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;

namespace TEditXNA.Terraria.Objects
{
    public class TileProperty : ObservableObject, ITile
    {
        private Color _color;
        private int _id;
        private string _name;
        private bool _isFramed;
        private Vector2Short _frameSize;

        public TileProperty()
        {
            _frameSize = new Vector2Short(1, 1);
            _color = Colors.Magenta;
            _id = -1;
            _name = "UNKNOWN";
            _isFramed = false;
        }

        public TileProperty(int id, string name, Color color, bool isFramed = false)
        {
            _color = color;
            _id = id;
            _name = name;
            _isFramed = isFramed;
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

        public Vector2Short FrameSize
        {
            get { return _frameSize; }
            set { Set("FrameSize", ref _frameSize, value); }
        }
        public bool IsFramed
        {
            get { return _isFramed; }
            set { Set("IsFramed", ref _isFramed, value); }
        }

        private WriteableBitmap _image;
        public WriteableBitmap Image
        {
            get { return _image; }
            set { Set("Image", ref _image, value); }
        }

    }
}