using System;
using System.Windows.Media;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class ColorProperty : NamedBase
    {
        public ColorProperty()
        {
            Name = "UNKNOWN";
            Color = Colors.Magenta;
        }

        public ColorProperty(byte id) : this()
        {
            ID = id;
        }

        private Color _color;
        public Color Color
        {
            get { return _color; }
            set { SetProperty(ref _color, ref value, "Color"); }
        }

        public override string ToString()
        {
            return base.ToString() + String.Format("|{0}|#{1:x2}{2:x2}{3:x2}{4:x2}", ID, Color.A, Color.R, Color.G, Color.B);
        }
    }
}