using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using TEditXNA.Terraria;

namespace TEditXna.Editor
{
    public class MouseTile : ObservableObject
    {
        private TileMouseState _mouseState = new TileMouseState();
        private Tile _tile;
        private Vector2Short _uV;
        private string _tileExtras;
        private string _tileName;
        private string _wallName;
        private string _paint;

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

        public string Paint
        {
            get { return _paint; }
            set { Set("Paint", ref _paint, value); }
        }

        public string TileExtras
        {
            get { return _tileExtras; }
            set { Set("TileExtras", ref _tileExtras, value); }
        }

        public Vector2Short UV
        {
            get { return _uV; }
            set { Set("UV", ref _uV, value); }
        }

        public Tile Tile
        {
            get { return _tile; }
            set
            {
                Set("Tile", ref _tile, value);

                if (World.TileProperties.Count > _tile.Type)
                {
                    TEditXNA.Terraria.Objects.TileProperty tileProperty = World.TileProperties[_tile.Type];
                    if (!tileProperty.HasFrameName)
                    {
                        TileName = tileProperty.Name;
                    }
                    else
                    {
                        string frameNameKey = World.GetFrameNameKey(_tile.Type, _tile.U, _tile.V);
                        TileName = World.FrameNames.ContainsKey(frameNameKey) ? World.FrameNames[frameNameKey] : tileProperty.Name + "*";
                    }
                    TileName = _tile.IsActive ? $"{TileName} ({_tile.Type})" : "[empty]";
                }
                else
                    TileName = $"INVALID TILE ({_tile.Type})";

                if (World.WallProperties.Count > _tile.Wall)
                    WallName = $"{World.WallProperties[_tile.Wall].Name} ({_tile.Wall})";
                else
                    WallName = $"INVALID WALL ({_tile.Wall})";

                UV = new Vector2Short(_tile.U, _tile.V);
                if (_tile.LiquidAmount > 0)
                {
                    TileExtras = $"{_tile.LiquidType}: {_tile.LiquidAmount}";
                }
                else
                    TileExtras = string.Empty;
                
                if (_tile.TileColor > 0)
                {
                    if (_tile.WallColor > 0)
                        Paint =
                            $"Tile: {World.PaintProperties[_tile.TileColor].Name}, Wall: {World.PaintProperties[_tile.WallColor].Name}";
                    else
                        Paint = $"Tile: {World.PaintProperties[_tile.TileColor].Name}";
                }
                else if (_tile.WallColor > 0)
                {
                    Paint = $"Wall: {World.PaintProperties[_tile.WallColor].Name}";
                }
                else
                {
                    Paint = "None";
                }

                if (_tile.InActive)
                {
                    TileExtras += " Inactive";
                }

                if (_tile.Actuator)
                {
                    TileExtras += " Actuator";
                }

                if (_tile.WireRed || _tile.WireBlue || _tile.WireGreen || _tile.WireYellow)
                {
                    if (!string.IsNullOrWhiteSpace(TileExtras))
                        TileExtras += ", Wire ";
                    else
                        TileExtras += "Wire ";
                    if (_tile.WireRed)
                        TileExtras += "R";
                    if (_tile.WireGreen)
                        TileExtras += "G";
                    if (_tile.WireBlue)
                        TileExtras += "B";
                    if (_tile.WireYellow)
                        TileExtras += "Y";
                }
            }
        }
    }
}
