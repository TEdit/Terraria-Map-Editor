using System.Windows.Media;
using TEditWPF.Common;

namespace TEditWPF.RenderWorld
{
    public class TileProperties : ObservableObject
    {
        public TileProperties()
        {

        }

        public TileProperties(byte id, Color color, string name)
        {
            this._ID = id;
            this._Name = name;
            this._Color = color;
        }

        private byte _ID;
        public byte ID
        {
            get { return this._ID; }
            set
            {
                if (this._ID != value)
                {
                    this._ID = value;
                    this.RaisePropertyChanged("ID");
                }
            }
        }


        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }

        private Color _Color;
        public Color Color
        {
            get { return this._Color; }
            set
            {
                if (this._Color != value)
                {
                    this._Color = value;
                    this.RaisePropertyChanged("Color");
                }
            }
        }


    }
}
