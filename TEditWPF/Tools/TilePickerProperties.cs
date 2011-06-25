using TEditWPF.Common;
namespace TEditWPF.Tools
{
    public class TilePickerProperties : ObservableObject
    {
        private byte _Wall;
        public byte Wall
        {
            get { return this._Wall; }
            set
            {
                if (this._Wall != value)
                {
                    this._Wall = value;
                    this.RaisePropertyChanged("Wall");
                }
            }
        }

        private byte _Tile;
        public byte Tile
        {
            get { return this._Tile; }
            set
            {
                if (this._Tile != value)
                {
                    this._Tile = value;
                    this.RaisePropertyChanged("Tile");
                }
            }
        }

        private byte _Liquid;
        public byte Liquid
        {
            get { return this._Liquid; }
            set
            {
                if (this._Liquid != value)
                {
                    this._Liquid = value;
                    this.RaisePropertyChanged("Liquid");
                }
            }
        }
        



    }
}