using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using TEdit.Common;
using TEdit.Common.Structures;
using TEdit.RenderWorld;
using TEdit.Tools;

namespace TEdit.TerrariaWorld
{
    [Export("World", typeof(World))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class World : ObservableObject
    {
        public const int MaxChests = 1000;
        public const int MaxSigns = 1000;
        public const int MaxNpcs = 1000;
        public const int CompatableVersion = 20;
        private readonly ObservableCollectionEx<Chest> _Chests = new ObservableCollectionEx<Chest>();
        private readonly ObservableCollectionEx<NPC> _Npcs = new ObservableCollectionEx<NPC>();
        private readonly ObservableCollectionEx<Sign> _Signs = new ObservableCollectionEx<Sign>();
        private WorldHeader _Header;
        private Chest[] _chests = new Chest[MaxChests];
        private NPC[] _npcs = new NPC[MaxNpcs];
        private Sign[] _signs = new Sign[MaxSigns];
        private Tile[,] _tiles;

        public World()
        {
            Header = new WorldHeader();
            ClearWorld();
        }

        public WorldHeader Header
        {
            get { return _Header; }
            set
            {
                if (_Header != value)
                {
                    _Header = value;
                    RaisePropertyChanged("Header");
                }
            }
        }

        public Tile[,] Tiles
        {
            get { return _tiles; }
        }

        public ObservableCollection<Chest> Chests
        {
            get { return _Chests; }
        }

        public ObservableCollection<Sign> Signs
        {
            get { return _Signs; }
        }

        public ObservableCollection<NPC> Npcs
        {
            get { return _Npcs; }
        }

        public Chest GetChestAtTile(int x, int y)
        {
            return Chests.FirstOrDefault(c => (c.Location.X == x || c.Location.X == x - 1) && (c.Location.Y == y || c.Location.Y == y - 1));
        }

        public Sign GetSignAtTile(int x, int y)
        {
            return Signs.FirstOrDefault(c => (c.Location.X == x || c.Location.X == x - 1) && (c.Location.Y == y || c.Location.Y == y - 1));
        }

        public void ClearWorld()
        {
            _tiles = new Tile[Header.MaxTiles.X, Header.MaxTiles.Y];

            Chests.Clear();
            Signs.Clear();
            Npcs.Clear();
        }

        public bool IsPointInWorld(int x, int y)
        {
            return (x >= 0 && y >= 0 &&
                    x < Header.MaxTiles.X && y < Header.MaxTiles.Y);
        }

        public void ResetTime()
        {
            Header.Time = 13500.0;
            Header.MoonPhase = 0;
            Header.IsBloodMoon = false;
        }

        public void SetTileXY(ref int x, ref int y, ref TilePicker tile, ref SelectionArea selection)
        {
            if (selection.IsValid(new PointInt32(x, y)))
            {
                Tile curTile = this.Tiles[x, y];

                if (tile.Tile.IsActive)
                {
                    if (!tile.TileMask.IsActive || (curTile.Type == tile.TileMask.Value && curTile.IsActive))
                    {
                        if (tile.IsEraser)
                        {
                            curTile.IsActive = false;
                        }
                        else
                        {
                            //TODO: i don't like redundant conditionals, but its a fix
                            if (!tile.TileMask.IsActive)
                                curTile.IsActive = true;

                            curTile.Type = tile.Tile.Value;

                            // if the tile is solid and there isn't a mask, remove the liquid
                            if (!tile.TileMask.IsActive && WorldSettings.Tiles[curTile.Type].IsSolid && curTile.Liquid > 0)
                                curTile.Liquid = 0;
                        }
                    }
                }


                if (tile.Wall.IsActive)
                {
                    if (!tile.WallMask.IsActive || (curTile.Wall == tile.WallMask.Value))
                    {
                        if (tile.IsEraser)
                            curTile.Wall = 0;
                        else
                            curTile.Wall = tile.Wall.Value;
                    }
                }

                if (tile.Liquid.IsActive && (!curTile.IsActive || !WorldSettings.Tiles[curTile.Type].IsSolid))
                {
                    if (tile.IsEraser)
                    {
                        curTile.Liquid = 0;
                        curTile.IsLava = false;
                    }
                    else
                    {
                        curTile.Liquid = 255;
                        curTile.IsLava = tile.Liquid.IsLava;
                    }
                }
            }
        }
    }
}