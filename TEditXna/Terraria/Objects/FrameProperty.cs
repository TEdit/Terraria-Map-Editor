using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;

namespace TEditXNA.Terraria.Objects
{
    public class FrameProperty : ObservableObject, ITile
    {
        public FrameProperty()
        {
            Id = 0;
            Name = "Default";
            Color = Colors.Magenta;
            UV = new Vector2Short(0,0);
            Direction = FrameDirection.None;
            Placement = FramePlacement.Any;
        }
        public FrameProperty(int id, string name, Vector2Short uv) : this()
        {
            Id = id;
            Name = name;
            UV = uv;
        }

        private string _name;
        private Vector2Short _uV;
        private int _id;
        private FrameDirection _direction;
        private FramePlacement _placement;
        private Color _color;
        private WriteableBitmap _image;

        public FramePlacement Placement
        {
            get { return _placement; }
            set { Set("Placement", ref _placement, value); }
        }

        public FrameDirection Direction
        {
            get { return _direction; }
            set { Set("Direction", ref _direction, value); }
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
        public Vector2Short UV
        {
            get { return _uV; }
            set { Set("UV", ref _uV, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }

        
         

        public WriteableBitmap Image
        {
            get { return _image; }
            set { Set("Image", ref _image, value); }
        }
    }
}