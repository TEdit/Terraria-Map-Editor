using TEdit.Common;
using TEdit.TerrariaWorld;
using TEdit.TerrariaWorld.Structures;

namespace TEdit.Tools.History
{
    public class HistoryTile : ObservableObject
    {
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