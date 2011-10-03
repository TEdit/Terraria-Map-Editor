using TEdit.Common;
using TEdit.Common.Structures;
using TEdit.TerrariaWorld;

namespace TEdit.Tools.History
{
    public class HistoryTile : ObservableObject
    {
        public HistoryTile(PointInt32 location, Tile tile)
        {
            Location = location;
            Tile = tile;
        }

        private PointInt32 _Location;
        public PointInt32 Location
        {
            get { return this._Location; }
            set
            {
                if (this._Location != value)
                {
                    this._Location = value;
                    this.RaisePropertyChanged("Location");
                }
            }
        }

        private Tile _Tile;
        public Tile Tile
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
    }
}