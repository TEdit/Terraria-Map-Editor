using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using TEditWPF.Common;

namespace TEditWPF.TerrariaWorld
{
    [Export("World", typeof(World))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class World
    {
        const int MaxChests = 1000;
        const int MaxSigns = 1000;
        const int MaxNpcs = 1000;
        const int CompatableVersion = 9;

        public World()
        {
            this.Header = new WorldHeader();
            this.ClearWorld();
        }

        public WorldHeader Header { get; set; }

        public void ClearWorld()
        {
            this._tiles = new Tile[this.Header.MaxTiles.X, this.Header.MaxTiles.Y];
            this._chests = new Chest[World.MaxChests];
            this._signs = new Sign[World.MaxSigns];
            this._npcs = new NPC[World.MaxNpcs];
        }

        public void ResetTime()
        {
            this.Header.Time = 13500.0;
            this.Header.MoonPhase = 0;
            this.Header.IsBloodMoon = false;
        }

        private Tile[,] _tiles;
        public Tile[,] Tiles
        {
            get { return this._tiles; }
        }

        private Chest[] _chests = new Chest[World.MaxChests];
        public Chest[] Chests
        {
            get { return _chests; }
        }

        private Sign[] _signs = new Sign[World.MaxSigns];
        public Sign[] Signs
        {
            get { return _signs; }
        }

        private NPC[] _npcs = new NPC[World.MaxNpcs];
        public NPC[] Npcs
        {
            get { return _npcs; }
        }
    }


}