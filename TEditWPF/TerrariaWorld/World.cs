using System.Collections.ObjectModel;
using System.ComponentModel;
using TEditWPF.Common;

namespace TEditWPF.TerrariaWorld
{
    public partial class World : ObservableObject
    {
        public static int MaxChests = 1000;
        public static int MaxSigns = 1000;
        public static int MaxNpcs = 1000;
        public static int CompatableVersion = 9;

        public World()
        {
            this.Header = new WorldHeader();

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

        public void ClearWorld()
        {
            this._tiles = new Tile[this.Header.MaxTiles.X, this.Header.MaxTiles.Y];
            this._Chests = new ObservableCollection<Chest>();
            this._Signs = new ObservableCollection<Sign>();
            this._Npcs = new ObservableCollection<NPC>();
        }

        public void ResetTime()
        {
            this.Header.Time = 13500.0;
            this.Header.MoonPhase = 0;
            this.Header.IsBloodMoon = false;
        }

        private Tile[,] _tiles;
        [Browsable(false)]
        public Tile[,] Tiles
        {
            get { return this._tiles; }
        }

        private ObservableCollection<Chest> _Chests = new ObservableCollection<Chest>();
        public ObservableCollection<Chest> Chests
        {
            get { return _Chests; }
        }

        private ObservableCollection<Sign> _Signs = new ObservableCollection<Sign>();
        public ObservableCollection<Sign> Signs
        {
            get { return _Signs; }
        }

        private ObservableCollection<NPC> _Npcs = new ObservableCollection<NPC>();
        public ObservableCollection<NPC> Npcs
        {
            get { return _Npcs; }
        }
    }
}