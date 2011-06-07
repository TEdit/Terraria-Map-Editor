using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace TerrariaWorld.Game
{
    public partial class World
    {
        public static int MAXCHESTS = 1000;
        public static int MAXSIGNS = 1000;
        public static int MAXNPCS = 1000;
        public static int COMPATIBLEVERSION = 3;

        public World()
        {
            this.Header = new WorldHeader();
            this.ClearWorld();
        }

        public void ClearWorld()
        {
            this._Tiles = new Tile[this.Header.MaxTiles.X, this.Header.MaxTiles.Y];
            this._Chests = new Chest[World.MAXCHESTS];
            this._Signs = new Sign[World.MAXSIGNS];
            this._NPCs = new NPC[World.MAXNPCS];
        }

        public WorldHeader Header { get; set; }

        private Tile[,] _Tiles;
        [Browsable(false)]
        public Tile[,] Tiles
        {
            get { return this._Tiles; }
        }

        private Chest[] _Chests;
        [Browsable(false)]
        public Chest[] Chests
        {
            get { return this._Chests; }
        }

        private Sign[] _Signs;
        [Browsable(false)]
        public Sign[] Signs
        {
            get { return this._Signs; }
        }

        private NPC[] _NPCs;
        [Browsable(false)]
        public NPC[] NPCs
        {
            get { return this._NPCs; }
        }

    }
}
