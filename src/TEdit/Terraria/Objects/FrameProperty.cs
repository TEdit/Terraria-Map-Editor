using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.Common.Reactive;
using TEdit.Geometry;

namespace TEdit.Terraria.Objects
{
    public class FrameProperty : ObservableObject, ITile
    {
        public FrameProperty()
        {
            Id = 0;
            Name = "Default";
            Color = TEditColor.Magenta;
            UV = new Vector2Short(0,0);
            Size = new Vector2Short(1,1);
            Anchor = FrameAnchor.None;
        }

        public FrameProperty(int id, string name, Vector2Short uv, Vector2Short size) : this()
        {
            Id = id;
            Name = name;
            UV = uv;
            Size = size;
        }

        private string _name;
        private Vector2Short _uV;
        private Vector2Short _size;
        private int _id;
        private FrameAnchor _anchor;
        
        private TEditColor _color;
        private WriteableBitmap _image;
        private string _variety;
         

        public string Variety
        {
            get { return _variety; }
            set { Set(nameof(Variety), ref _variety, value); }
        }

        public FrameAnchor Anchor
        {
            get { return _anchor; }
            set { Set(nameof(Anchor), ref _anchor, value); }
        }

        public TEditColor Color
        {
            get { return _color; }
            set { Set(nameof(Color), ref _color, value); }
        }

        public int Id
        {
            get { return _id; }
            set { Set(nameof(Id), ref _id, value); }
        }
        public Vector2Short UV
        {
            get { return _uV; }
            set { Set(nameof(UV), ref _uV, value); }
        }

        public Vector2Short Size
        {
            get { return _size; }
            set { Set(nameof(Size), ref _size, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(nameof(Name), ref _name, value); }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Name);
            if (!string.IsNullOrWhiteSpace(Variety))
            {
                sb.Append(": ");
                sb.Append(Variety);
            }
            if (Anchor != FrameAnchor.None)
            {
                sb.Append(" [");
                sb.Append(Anchor.ToString());
                sb.Append(']');

            }
            return sb.ToString();
        }
    }
}
