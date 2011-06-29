using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Threading;
using TEdit.Common;

namespace TEdit.TerrariaWorld
{
    [Export("World", typeof (World))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class World : ObservableObject
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

        private WorldHeader _Header;
        public WorldHeader Header
        {
            get { return this._Header; }
            set
            {
                if (this._Header != value)
                {
                    this._Header = value;
                    this.RaisePropertyChanged("Header");
                }
            }
        }

        public Tile[,] Tiles
        {
            get { return _tiles; }
        }

        private readonly ObservableCollectionEx<Chest> _Chests = new ObservableCollectionEx<Chest>();
        public ObservableCollection<Chest> Chests
        {
            get { return _Chests; }
        }

        private readonly ObservableCollectionEx<Sign> _Signs = new ObservableCollectionEx<Sign>();
        public ObservableCollection<Sign> Signs
        {
            get { return _Signs; }
        }

        private readonly ObservableCollectionEx<NPC> _Npcs = new ObservableCollectionEx<NPC>();
        public ObservableCollection<NPC> Npcs
        {
            get { return _Npcs; }
        }

        public void ClearWorld()
        {
            _tiles = new Tile[Header.MaxTiles.X,Header.MaxTiles.Y];
            
            Chests.Clear();
            Signs.Clear();
            Npcs.Clear();
        }

        public void ResetTime()
        {
            Header.Time = 13500.0;
            Header.MoonPhase = 0;
            Header.IsBloodMoon = false;
        }
    }
}