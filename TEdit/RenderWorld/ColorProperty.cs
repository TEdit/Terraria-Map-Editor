using System;
using System.Windows.Media;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class ColorProperty : TileItemBase
    {

        public ColorProperty()
        {
        }

        public ColorProperty(byte id)
        {
            ID = id;
            Name = "UNKNOWN";
            Color = Colors.Magenta;
        }

        protected internal Color _color;
        public virtual Color Color
        {
            get { return _color; }
            set { StandardSet(ref _color, ref value, "Color"); }
        }

        public override string ToString()
        {
            return String.Format("{0}|{1}|#{2:x2}{3:x2}{4:x2}{5:x2}", ID, Name, Color.A, Color.R, Color.G, Color.B);
        }
    }
}