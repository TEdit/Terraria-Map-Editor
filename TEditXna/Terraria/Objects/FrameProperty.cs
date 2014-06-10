using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;

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
            Anchor = FrameAnchor.None;
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
        private FrameAnchor _anchor;
        
        private Color _color;
        private WriteableBitmap _image;
        private string _variety;
         

        public string Variety
        {
            get { return _variety; }
            set { Set("Variety", ref _variety, value); }
        }

        public FrameAnchor Anchor
        {
            get { return _anchor; }
            set { Set("Anchor", ref _anchor, value); }
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