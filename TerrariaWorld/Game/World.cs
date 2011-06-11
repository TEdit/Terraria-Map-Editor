using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace TerrariaWorld.Game
{
    public partial class World
    {
        public static int MaxChests = 1000;
        public static int MaxSigns = 1000;
        public static int MaxNpcs = 1000;
        public static int CompatableVersion = 9;

        public World()
        {
            this.Header = new WorldHeader();
            this.ClearWorld();
        }

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

        public WorldHeader Header { get; set; }

        private Tile[,] _tiles;
        [Browsable(false)]
        public Tile[,] Tiles
        {
            get { return this._tiles; }
        }

        private Chest[] _chests;
        [Browsable(false)]
        public Chest[] Chests
        {
            get { return this._chests; }
        }

        private Sign[] _signs;
        [Browsable(false)]
        public Sign[] Signs
        {
            get { return this._signs; }
        }

        private NPC[] _npcs;
        [Browsable(false)]
        public NPC[] NPCs
        {
            get { return this._npcs; }
        }

    }
}
