using System;
using System.Windows.Media;
using TEdit.Common;

namespace TEdit.RenderWorld
{
    [Serializable]
    public class TileColor : ObservableObject
    {
        private Color _Color;
        private byte _ID;
        private string _Name;

        public TileColor()
        {
        }

        public TileColor(byte id, Color color, string name)
        {
            _ID = id;
            _Name = name;
            _Color = color;
        }

        public byte ID
        {
            get { return _ID; }
            set
            {
                if (_ID != value)
                {
                    _ID = value;
                    RaisePropertyChanged("ID");
                }
            }
        }


        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        public Color Color
        {
            get { return _Color; }
            set
            {
                if (_Color != value)
                {
                    _Color = value;
                    RaisePropertyChanged("Color");
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0}|{1}|#{2:x2}{3:x2}{4:x2}{5:x2}", this.ID, this.Name, this.Color.A, this.Color.R, this.Color.G, this.Color.B);
        }

        public static TileColor FromString(string line)
        {
            string[] splitline = line.Split(new[] { ',', '|' });
            if (splitline.Length == 3)
            {
                byte id = 0;
                byte.TryParse(splitline[0], out id);

                string name = splitline[1];
                var color = (Color)ColorConverter.ConvertFromString(splitline[2]);

                return new TileColor(id, color, name);
            }
            return null;
        }
    }
}