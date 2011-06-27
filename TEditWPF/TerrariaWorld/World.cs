using System.ComponentModel.Composition;

namespace TEditWPF.TerrariaWorld
{
    [Export("World", typeof (World))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class World
    {
        public const int MaxChests = 1000;
        public const int MaxSigns = 1000;
        public const int MaxNpcs = 1000;
        public const int CompatableVersion = 9;
        private Chest[] _chests = new Chest[MaxChests];
        private NPC[] _npcs = new NPC[MaxNpcs];
        private Sign[] _signs = new Sign[MaxSigns];
        private Tile[,] _tiles;

        public World()
        {
            Header = new WorldHeader();
            ClearWorld();
        }

        public WorldHeader Header { get; set; }

        public Tile[,] Tiles
        {
            get { return _tiles; }
        }

        public Chest[] Chests
        {
            get { return _chests; }
        }

        public Sign[] Signs
        {
            get { return _signs; }
        }

        public NPC[] Npcs
        {
            get { return _npcs; }
        }

        public void ClearWorld()
        {
            _tiles = new Tile[Header.MaxTiles.X,Header.MaxTiles.Y];
            _chests = new Chest[MaxChests];
            _signs = new Sign[MaxSigns];
            _npcs = new NPC[MaxNpcs];
        }

        public void ResetTime()
        {
            Header.Time = 13500.0;
            Header.MoonPhase = 0;
            Header.IsBloodMoon = false;
        }
    }
}