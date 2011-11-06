using System;
using TEdit.Common;
using TEdit.Common.Structures;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class ColorProperty : XMLBase
    {
        public ColorProperty()
        {
            Name = "UNKNOWN";
        }

        public ColorProperty(byte id) : this()
        {
            ID = id;
        }

        private Color _color = Color.FromName("Magenta");
        public Color Color
        {
            get { return _color; }
            set { SetProperty(ref _color, ref value, "Color"); }
        }

        private TexturePlus _texture;
        public TexturePlus Texture
        {
            get { return _texture; }
            set { SetProperty(ref _texture, ref value, "Texture"); }
        }

        public override string ToString()
        {
            return base.ToString() + String.Format("|{0}|#{1:x2}{2:x2}{3:x2}{4:x2}", ID, Color.A, Color.R, Color.G, Color.B);
        }
    }
}