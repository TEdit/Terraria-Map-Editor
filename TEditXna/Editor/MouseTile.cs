using BCCL.MvvmLight;
using TEditXNA.Terraria;

namespace TEditXna.Editor
{
    public class MouseTile : ObservableObject
    {
        private TileMouseState _mouseState = new TileMouseState();
        private Tile _tile;
        private string _tileLiquid;
        private string _tileName;
        private string _wallName;

        public TileMouseState MouseState
        {
            get { return _mouseState; }
            set { Set("MouseState", ref _mouseState, value); }
        }

        public string WallName
        {
            get { return _wallName; }
            set { Set("WallName", ref _wallName, value); }
        }

        public string TileName
        {
            get { return _tileName; }
            set { Set("TileName", ref _tileName, value); }
        }


        public string TileLiquid
        {
            get { return _tileLiquid; }
            set { Set("TileLiquid", ref _tileLiquid, value); }
        }

        public Tile Tile
        {
            get { return _tile; }
            set
            {
                Set("Tile", ref _tile, value);
                TileName = World.TileProperties[_tile.Type].Name;
                WallName = World.WallProperties[_tile.Wall].Name;

                if (_tile.Liquid > 0)
                    TileLiquid = _tile.IsLava ? "Lava: " + _tile.Liquid : "Water: " + _tile.Liquid;
                else
                    TileLiquid = string.Empty;
            }
        }
    }
}