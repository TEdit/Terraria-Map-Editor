using System;
using System.Windows.Media;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class ColorProperty : ObservableObject
    {
        private Color _color;
        private byte _id;
        private string _name;

        public ColorProperty()
        {
        }

        public ColorProperty(byte id, Color color, string name)
        {
            _id = id;
            _name = name;
            _color = color;
        }

        public byte ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged("ID");
                }
            }
        }


        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    RaisePropertyChanged("Color");
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0}|{1}|#{2:x2}{3:x2}{4:x2}{5:x2}", ID, Name, Color.A, Color.R, Color.G, Color.B);
        }
    }
}