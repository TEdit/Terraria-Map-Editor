using System.Collections.ObjectModel;
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
        private readonly ObservableCollection<FrameProperty> _frames = new ObservableCollection<FrameProperty>();
        private bool _isSolid;
        private bool _isSolidTop;
        private bool _isLight;
        private FramePlacement _placement;
        private Vector2Short _textureGrid;

        public TileProperty()
        {
            _frameSize = new Vector2Short(1, 1);
            _color = Colors.Magenta;
            _id = -1;
            _name = "UNKNOWN";
            _isFramed = false;
        }

        public Vector2Short TextureGrid
        {
            get { return _textureGrid; }
            set { Set("TextureGrid", ref _textureGrid, value); }
        }
        public FramePlacement Placement
        {
            get { return _placement; }
            set { Set("Placement", ref _placement, value); }
        }

        public bool IsLight
        {
            get { return _isLight; }
            set { Set("IsLight", ref _isLight, value); }
        }

        public bool IsSolidTop
        {
            get { return _isSolidTop; }
            set { Set("IsSolidTop", ref _isSolidTop, value); }
        }

        public bool IsSolid
        {
            get { return _isSolid; }
            set { Set("IsSolid", ref _isSolid, value); }
        }

        public ObservableCollection<FrameProperty> Frames
        {
            get { return _frames; }
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