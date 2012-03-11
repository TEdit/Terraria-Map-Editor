using System.ComponentModel;
using System.Windows;
using BCCL.Geometry;
using BCCL.MvvmLight;
using BCCL.Utility;
using Microsoft.Xna.Framework;
using TEditXNA.Terraria.Objects;
using BCCL.Geometry.Primitives;
using Vector2 = BCCL.Geometry.Primitives.Vector2;

namespace TEditXNA.Terraria
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    public partial class World : ObservableObject
    {
        private static object _fileLock = new object();
        /// <summary>
        /// Triggered when an operation reports progress.
        /// </summary>
        public static event ProgressChangedEventHandler ProgressChanged;

        private static void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(sender, e);
        }

        public World()
        {
            NPCs.Clear();
            Signs.Clear();
            Chests.Clear();
            CharacterNames.Clear();
        }

        public World(int height, int width, string title, int seed = -1)
            : this()
        {
            TilesWide = width;
            TilesHigh = height;
            Title = title;
            var r = seed <= 0 ? new Random((int)DateTime.Now.Ticks) : new Random(seed);
            WorldId = r.Next(int.MaxValue);
            _npcs.Clear();
            _signs.Clear();
            _chests.Clear();
            _charNames.Clear();
        }

        public Tile[,] Tiles;

        private readonly ObservableCollection<NPC> _npcs = new ObservableCollection<NPC>();

        public ObservableCollection<NPC> NPCs
        {
            get { return _npcs; }
        }

        private readonly ObservableCollection<Sign> _signs = new ObservableCollection<Sign>();

        public ObservableCollection<Sign> Signs
        {
            get { return _signs; }
        }

        private readonly ObservableCollection<Chest> _chests = new ObservableCollection<Chest>();

        public ObservableCollection<Chest> Chests
        {
            get { return _chests; }
        }

        private readonly ObservableCollection<NpcName> _charNames = new ObservableCollection<NpcName>();

        public ObservableCollection<NpcName> CharacterNames
        {
            get { return _charNames; }
        }

        public uint Version;
        private string _title;
        private int _spawnX;
        private int _spawnY;
        private double _groundLevel;
        private double _rockLevel;
        private double _time;
        private bool _dayTime;
        private int _moonPhase;
        private bool _bloodMoon;
        private int _dungeonX;
        private int _dungeonY;
        private bool _downedBoss1;
        private bool _downedBoss2;
        private bool _downedBoss3;
        private bool _savedGoblin;
        private bool _savedWizard;
        private bool _downedGoblins;
        private bool _savedMech;
        private bool _downedClown;
        private bool _downedFrost;
        private bool _shadowOrbSmashed;
        private bool _spawnMeteor;
        private int _shadowOrbCount;
        private int _altarCount;
        private bool _hardMode;
        private double _invasionX;
        private int _invasionType;
        private int _invasionSize;
        private int _invasionDelay;
        private int _worldId;
        private float _leftWorld;
        private float _rightWorld;
        private float _topWorld;
        private float _bottomWorld;
        private int _tilesHigh;
        private int _tilesWide;

        #region Properties

        public int TilesWide
        {
            get { return _tilesWide; }
            set { Set("TilesWide", ref _tilesWide, value); }
        }

        public int TilesHigh
        {
            get { return _tilesHigh; }
            set { Set("TilesHigh", ref _tilesHigh, value); }
        }

        public float BottomWorld
        {
            get { return _bottomWorld; }
            set { Set("BottomWorld", ref _bottomWorld, value); }
        }

        public float TopWorld
        {
            get { return _topWorld; }
            set { Set("TopWorld", ref _topWorld, value); }
        }

        public float RightWorld
        {
            get { return _rightWorld; }
            set { Set("RightWorld", ref _rightWorld, value); }
        }

        public float LeftWorld
        {
            get { return _leftWorld; }
            set { Set("LeftWorld", ref _leftWorld, value); }
        }

        public int WorldId
        {
            get { return _worldId; }
            set { Set("WorldId", ref _worldId, value); }
        }

        public bool DownedFrost
        {
            get { return _downedFrost; }
            set { Set("DownedFrost", ref _downedFrost, value); }
        }

        public string Title
        {
            get { return _title; }
            set { Set("Title", ref _title, value); }
        }

        public int SpawnX
        {
            get { return _spawnX; }
            set { Set("SpawnX", ref _spawnX, value); }
        }

        public int SpawnY
        {
            get { return _spawnY; }
            set { Set("SpawnY", ref _spawnY, value); }
        }

        public double GroundLevel
        {
            get { return _groundLevel; }
            set
            {
                Set("GroundLevel", ref _groundLevel, value);
                if (_groundLevel > _rockLevel)
                    RockLevel = _groundLevel;
            }
        }

        public double RockLevel
        {
            get { return _rockLevel; }
            set
            {
                Set("RockLevel", ref _rockLevel, value);
                if (_groundLevel > _rockLevel)
                    GroundLevel = _rockLevel;
            }
        }

        public double Time
        {
            get { return _time; }
            set { Set("Time", ref _time, value); }
        }

        public bool DayTime
        {
            get { return _dayTime; }
            set { Set("DayTime", ref _dayTime, value); }
        }

        public int MoonPhase
        {
            get { return _moonPhase; }
            set { Set("MoonPhase", ref _moonPhase, value); }
        }

        public bool BloodMoon
        {
            get { return _bloodMoon; }
            set { Set("BloodMoon", ref _bloodMoon, value); }
        }

        public int DungeonX
        {
            get { return _dungeonX; }
            set { Set("DungeonX", ref _dungeonX, value); }
        }

        public int DungeonY
        {
            get { return _dungeonY; }
            set { Set("DungeonY", ref _dungeonY, value); }
        }

        public bool DownedBoss1
        {
            get { return _downedBoss1; }
            set { Set("DownedBoss1", ref _downedBoss1, value); }
        }

        public bool DownedBoss2
        {
            get { return _downedBoss2; }
            set { Set("DownedBoss2", ref _downedBoss2, value); }
        }

        public bool DownedBoss3
        {
            get { return _downedBoss3; }
            set { Set("DownedBoss3", ref _downedBoss3, value); }
        }

        public bool SavedGoblin
        {
            get { return _savedGoblin; }
            set { Set("SavedGoblin", ref _savedGoblin, value); }
        }

        public bool SavedWizard
        {
            get { return _savedWizard; }
            set { Set("SavedWizard", ref _savedWizard, value); }
        }

        public bool DownedGoblins
        {
            get { return _downedGoblins; }
            set { Set("DownedGoblins", ref _downedGoblins, value); }
        }

        public bool SavedMech
        {
            get { return _savedMech; }
            set { Set("SavedMech", ref _savedMech, value); }
        }

        public bool DownedClown
        {
            get { return _downedClown; }
            set { Set("DownedClown", ref _downedClown, value); }
        }

        public bool ShadowOrbSmashed
        {
            get { return _shadowOrbSmashed; }
            set { Set("ShadowOrbSmashed", ref _shadowOrbSmashed, value); }
        }

        public bool SpawnMeteor
        {
            get { return _spawnMeteor; }
            set { Set("SpawnMeteor", ref _spawnMeteor, value); }
        }

        public int ShadowOrbCount
        {
            get { return _shadowOrbCount; }
            set
            {
                Set("ShadowOrbCount", ref _shadowOrbCount, value);
                ShadowOrbSmashed = _shadowOrbCount > 0;
            }
        }

        public int AltarCount
        {
            get { return _altarCount; }
            set { Set("AltarCount", ref _altarCount, value); }
        }

        public bool HardMode
        {
            get { return _hardMode; }
            set { Set("HardMode", ref _hardMode, value); }
        }

        public double InvasionX
        {
            get { return _invasionX; }
            set { Set("InvasionX", ref _invasionX, value); }
        }

        public int InvasionType
        {
            get { return _invasionType; }
            set { Set("InvasionType", ref _invasionType, value); }
        }

        public int InvasionSize
        {
            get { return _invasionSize; }
            set { Set("InvasionSize", ref _invasionSize, value); }
        }

        public int InvasionDelay
        {
            get { return _invasionDelay; }
            set { Set("InvasionDelay", ref _invasionDelay, value); }
        }

        #endregion

        public bool ValidTileLocation(Vector2Int32 point)
        {
            if (point.X < 0)
                return false;
            if (point.Y < 0)
                return false;
            if (point.Y >= _tilesHigh)
                return false;
            if (point.X >= _tilesWide)
                return false;

            return true;
        }

        public Chest GetChestAtTile(int x, int y)
        {
            return Chests.FirstOrDefault(c => (c.X == x || c.X == x - 1) && (c.Y == y || c.Y == y - 1));
        }

        public Sign GetSignAtTile(int x, int y)
        {
            return Signs.FirstOrDefault(c => (c.X == x || c.X == x - 1) && (c.Y == y || c.Y == y - 1));
        }

        public void Save(string filename, bool resetTime = false)
        {
            lock (_fileLock)
            {
                if (resetTime)
                {
                    OnProgressChanged(this, new ProgressChangedEventArgs(0, "Resetting Time..."));
                    ResetTime();
                }

                if (filename == null)
                    return;

                string temp = filename + ".tmp";
                using (var fs = new FileStream(temp, FileMode.Create))
                {
                    using (var bw = new BinaryWriter(fs))
                    {
                        bw.Write(World.CompatibleVersion);
                        bw.Write(Title);
                        bw.Write(WorldId);
                        bw.Write((int)LeftWorld);
                        bw.Write((int)RightWorld);
                        bw.Write((int)TopWorld);
                        bw.Write((int)BottomWorld);
                        bw.Write(TilesHigh);
                        bw.Write(TilesWide);
                        bw.Write(SpawnX);
                        bw.Write(SpawnY);
                        bw.Write(GroundLevel);
                        bw.Write(RockLevel);
                        bw.Write(Time);
                        bw.Write(DayTime);
                        bw.Write(MoonPhase);
                        bw.Write(BloodMoon);
                        bw.Write(DungeonX);
                        bw.Write(DungeonY);
                        bw.Write(DownedBoss1);
                        bw.Write(DownedBoss2);
                        bw.Write(DownedBoss3);
                        bw.Write(SavedGoblin);
                        bw.Write(SavedWizard);
                        bw.Write(SavedMech);
                        bw.Write(DownedGoblins);
                        bw.Write(DownedClown);
                        bw.Write(DownedFrost);
                        bw.Write(ShadowOrbSmashed);
                        bw.Write(SpawnMeteor);
                        bw.Write((byte)ShadowOrbCount);
                        bw.Write(AltarCount);
                        bw.Write(HardMode);
                        bw.Write(InvasionDelay);
                        bw.Write(InvasionSize);
                        bw.Write(InvasionType);
                        bw.Write(InvasionX);


                        for (int x = 0; x < TilesWide; ++x)
                        {
                            OnProgressChanged(this, new ProgressChangedEventArgs(x.ProgressPercentage(TilesWide), "Saving Tiles..."));

                            int rle = 0;
                            for (int y = 0; y < TilesHigh; y = y + rle + 1)
                            {

                                var curTile = Tiles[x, y];
                                bw.Write(curTile.IsActive);
                                if (curTile.IsActive)
                                {
                                    bw.Write(curTile.Type);
                                    if (TileProperties[curTile.Type].IsFramed)
                                    {
                                        bw.Write(curTile.U);
                                        bw.Write(curTile.V);

                                        // TODO: Let Validate handle these
                                        //validate chest entry exists
                                        if (curTile.Type == 21)
                                        {
                                            if (GetChestAtTile(x, y) == null)
                                            {
                                                Chests.Add(new Chest(x, y));
                                            }
                                        }
                                        //validate sign entry exists
                                        else if (curTile.Type == 55 || curTile.Type == 85)
                                        {
                                            if (GetSignAtTile(x, y) == null)
                                            {
                                                Signs.Add(new Sign(x, y, string.Empty));
                                            }
                                        }
                                    }
                                }
                                if ((int)curTile.Wall > 0)
                                {
                                    bw.Write(true);
                                    bw.Write(curTile.Wall);
                                }
                                else
                                    bw.Write(false);

                                if ((int)curTile.Liquid > 0)
                                {
                                    bw.Write(true);
                                    bw.Write(curTile.Liquid);
                                    bw.Write(curTile.IsLava);
                                }
                                else
                                    bw.Write(false);

                                bw.Write(curTile.HasWire);

                                int rleTemp = 1;
                                while (y + rleTemp < TilesHigh && curTile.Equals(Tiles[x, (y + rleTemp)]))
                                    ++rleTemp;
                                rle = rleTemp - 1;
                                bw.Write((short)rle);
                            }
                        }
                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Saving Chests..."));
                        for (int i = 0; i < 1000; ++i)
                        {
                            if (i >= Chests.Count)
                            {
                                bw.Write(false);
                            }
                            else
                            {
                                Chest curChest = Chests[i];
                                bw.Write(true);
                                bw.Write(curChest.X);
                                bw.Write(curChest.Y);
                                for (int j = 0; j < Chest.MaxItems; ++j)
                                {
                                    if (curChest.Items.Count > j)
                                    {
                                        bw.Write((byte)curChest.Items[j].StackSize);
                                        if (curChest.Items[j].StackSize > 0)
                                        {
                                            bw.Write(curChest.Items[j].NetId); // TODO Verify
                                            bw.Write(curChest.Items[j].Prefix);
                                        }
                                    }
                                    else
                                        bw.Write((byte)0);
                                }
                            }
                        }
                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Saving Signs..."));
                        for (int i = 0; i < 1000; ++i)
                        {
                            if (i >= Signs.Count || string.IsNullOrWhiteSpace(Signs[i].Text))
                            {
                                bw.Write(false);
                            }
                            else
                            {
                                var curSign = Signs[i];
                                bw.Write(true);
                                bw.Write(curSign.Text);
                                bw.Write(curSign.X);
                                bw.Write(curSign.Y);
                            }
                        }
                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Saving NPC Data..."));
                        foreach (NPC curNpc in NPCs)
                        {
                            bw.Write(true);
                            bw.Write(curNpc.Name);
                            bw.Write(curNpc.Position.X);
                            bw.Write(curNpc.Position.Y);
                            bw.Write(curNpc.IsHomeless);
                            bw.Write(curNpc.Home.X);
                            bw.Write(curNpc.Home.Y);
                        }
                        bw.Write(false);

                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Saving NPC Names..."));
                        bw.Write(CharacterNames.FirstOrDefault(c => c.Id == 17).Name);
                        bw.Write(CharacterNames.FirstOrDefault(c => c.Id == 18).Name);
                        bw.Write(CharacterNames.FirstOrDefault(c => c.Id == 19).Name);
                        bw.Write(CharacterNames.FirstOrDefault(c => c.Id == 20).Name);
                        bw.Write(CharacterNames.FirstOrDefault(c => c.Id == 22).Name);
                        bw.Write(CharacterNames.FirstOrDefault(c => c.Id == 54).Name);
                        bw.Write(CharacterNames.FirstOrDefault(c => c.Id == 38).Name);
                        bw.Write(CharacterNames.FirstOrDefault(c => c.Id == 107).Name);
                        bw.Write(CharacterNames.FirstOrDefault(c => c.Id == 108).Name);
                        bw.Write(CharacterNames.FirstOrDefault(c => c.Id == 124).Name);

                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Saving Validation Data..."));
                        bw.Write(true);
                        bw.Write(Title);
                        bw.Write(WorldId);
                        bw.Close();
                        fs.Close();

                        // make a backup of current file if it exists
                        if (File.Exists(filename))
                        {
                            string backup = filename + ".TEdit";
                            File.Copy(filename, backup, true);
                        }
                        // replace actual file with temp save file
                        File.Copy(temp, filename, true);
                        // delete temp save file
                        File.Delete(temp);
                        OnProgressChanged(null, new ProgressChangedEventArgs(0, "World Save Complete."));
                    }
                }
            }
        }

        public void ResetTime()
        {
            DayTime = true;
            Time = 13500.0;
            MoonPhase = 0;
            BloodMoon = false;
        }

        public static World LoadWorld(string filename)
        {
            var w = new World();
            try
            {
                lock (_fileLock)
                {
                    using (var b = new BinaryReader(File.OpenRead(filename)))
                    {
                        w.Version = b.ReadUInt32(); //now we care about the version
                        w.Title = b.ReadString();

                        w.WorldId = b.ReadInt32();
                        w.LeftWorld = (float)b.ReadInt32();
                        w.RightWorld = (float)b.ReadInt32();
                        w.TopWorld = (float)b.ReadInt32();
                        w.BottomWorld = (float)b.ReadInt32();

                        w.TilesHigh = b.ReadInt32();
                        w.TilesWide = b.ReadInt32();

                        if (w.TilesHigh > 10000 || w.TilesWide > 10000 || w.TilesHigh <= 0 || w.TilesWide <= 0)
                            throw new FileLoadException(string.Format("Invalid File: {0}", filename));

                        w.SpawnX = b.ReadInt32();
                        w.SpawnY = b.ReadInt32();
                        w.GroundLevel = (int)b.ReadDouble();
                        w.RockLevel = (int)b.ReadDouble();

                        // read world flags
                        w.Time = b.ReadDouble();
                        w.DayTime = b.ReadBoolean();
                        w.MoonPhase = b.ReadInt32();
                        w.BloodMoon = b.ReadBoolean();
                        w.DungeonX = b.ReadInt32();
                        w.DungeonY = b.ReadInt32();
                        w.DownedBoss1 = b.ReadBoolean();
                        w.DownedBoss2 = b.ReadBoolean();
                        w.DownedBoss3 = b.ReadBoolean();
                        if (w.Version >= 29)
                        {
                            w.SavedGoblin = b.ReadBoolean();
                            w.SavedWizard = b.ReadBoolean();
                            if (w.Version >= 34)
                                w.SavedMech = b.ReadBoolean();
                            w.DownedGoblins = b.ReadBoolean();
                        }
                        if (w.Version >= 32)
                            w.DownedClown = b.ReadBoolean();
                        if (w.Version >= 37)
                            w.DownedFrost = b.ReadBoolean();
                        w.ShadowOrbSmashed = b.ReadBoolean();
                        w.SpawnMeteor = b.ReadBoolean();
                        w.ShadowOrbCount = (int)b.ReadByte();
                        if (w.Version >= 23)
                        {
                            w.AltarCount = b.ReadInt32();
                            w.HardMode = b.ReadBoolean();
                        }
                        w.InvasionDelay = b.ReadInt32();
                        w.InvasionSize = b.ReadInt32();
                        w.InvasionType = b.ReadInt32();
                        w.InvasionX = b.ReadDouble();



                        w.Tiles = new Tile[w.TilesWide, w.TilesHigh];
                        for (int x = 0; x < w.TilesWide; ++x)
                        {
                            OnProgressChanged(null, new ProgressChangedEventArgs(x.ProgressPercentage(w.TilesWide), "Loading Tiles..."));

                            Tile prevtype = new Tile();
                            for (int y = 0; y < w.TilesHigh; y++)
                            {

                                var tile = new Tile();

                                tile.IsActive = b.ReadBoolean();

                                if (tile.IsActive)
                                {

                                    tile.Type = b.ReadByte();
                                    var tileProperty = TileProperties[tile.Type];
                                    if (string.Equals(tileProperty.Name, "UNKNOWN", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        throw new ArgumentOutOfRangeException(string.Format("Unknown tile tile: {0}, please add tile id {0} too your settings.xml.\r\nBE SURE TO INCLUDE THE isFramed PROPERTY (sprites=true, blocks=false).\r\nYou are seeing this message due to an update or mod.", tile.Type));
                                    }


                                    if (tile.Type == (int)sbyte.MaxValue)
                                        tile.IsActive = false;

                                    if (tileProperty.IsFramed)
                                    {
                                        // torches didn't have extra in older versions.
                                        if (w.Version < 28 && tile.Type == 4)
                                        {
                                            tile.U = 0;
                                            tile.V = 0;
                                        }
                                        else
                                        {
                                            tile.U = b.ReadInt16();
                                            tile.V = b.ReadInt16();
                                            //if (tile.Type == 128) //armor stand
                                            //    tile.Frame = new PointShort((short)(tile.Frame.X % 100), tile.Frame.Y);

                                            if ((int)tile.Type == 144) //timer
                                                tile.V = 0;
                                        }
                                    }
                                    else
                                    {
                                        tile.U = -1;
                                        tile.V = -1;
                                    }
                                }
                                if (w.Version <= 25)
                                    b.ReadBoolean(); //skip obsolete hasLight
                                if (b.ReadBoolean())
                                {
                                    tile.Wall = b.ReadByte();
                                }
                                //else
                                //    tile.Wall = 0;
                                if (b.ReadBoolean())
                                {
                                    tile.Liquid = b.ReadByte();
                                    tile.IsLava = b.ReadBoolean();
                                }

                                if (w.Version >= 33)
                                    tile.HasWire = b.ReadBoolean();
                                //else
                                //    tile.HasWire = false;
                                w.Tiles[x, y] = tile;

                                var ptype = (Tile)prevtype.Clone();
                                prevtype = (Tile)tile.Clone();
                                if (w.Version >= 25) //compression ftw :)
                                {
                                    int rle = b.ReadInt16();
                                    if (rle > 0)
                                    {
                                        for (int r = y + 1; r < y + rle + 1; r++)
                                        {
                                            var tcopy = (Tile)tile.Clone();
                                            w.Tiles[x, r] = tcopy;
                                        }
                                        y += rle;
                                    }
                                }
                            }
                        }
                        w.Chests.Clear();
                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading Chests..."));
                        for (int i = 0; i < 1000; i++)
                        {
                            if (b.ReadBoolean())
                            {
                                var chest = new Chest(b.ReadInt32(), b.ReadInt32());
                                for (int slot = 0; slot < Chest.MaxItems; slot++)
                                {
                                    var stackSize = b.ReadByte();
                                    chest.Items[slot].StackSize = stackSize;
                                    if (chest.Items[slot].StackSize > 0)
                                    {
                                        if (w.Version >= 38)
                                            chest.Items[slot].NetId = b.ReadInt32();
                                        else
                                            chest.Items[slot].SetFromName(b.ReadString());

                                        chest.Items[slot].StackSize = stackSize;
                                        // Read prefix
                                        if (w.Version >= 36)
                                            chest.Items[slot].Prefix = b.ReadByte();
                                    }
                                }
                                w.Chests.Add(chest);
                            }
                        }
                        w.Signs.Clear();
                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading Signs..."));
                        for (int i = 0; i < 1000; i++)
                        {
                            if (b.ReadBoolean())
                            {
                                Sign sign = new Sign();
                                sign.Text = b.ReadString();
                                sign.X = b.ReadInt32();
                                sign.Y = b.ReadInt32();
                                w.Signs.Add(sign);
                            }
                        }
                        w.NPCs.Clear();
                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading NPC Data..."));
                        while (b.ReadBoolean())
                        {
                            var npc = new NPC();
                            npc.Name = b.ReadString();
                            npc.Position = new Vector2(b.ReadSingle(), b.ReadSingle());
                            npc.IsHomeless = b.ReadBoolean();
                            npc.Home = new Vector2Int32(b.ReadInt32(), b.ReadInt32());
                            npc.SpriteId = 0;
                            if (NpcIds.ContainsKey(npc.Name))
                                npc.SpriteId = NpcIds[npc.Name];

                            w.NPCs.Add(npc);
                        }
                        // if (version>=0x1f) read the names of the following npcs:
                        // merchant, nurse, arms dealer, dryad, guide, clothier, demolitionist,
                        // tinkerer and wizard
                        // if (version>=0x23) read the name of the mechanic


                        if (w.Version >= 31)
                        {
                            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading NPC Names..."));
                            w.CharacterNames.Add(new NpcName(17, b.ReadString()));
                            w.CharacterNames.Add(new NpcName(18, b.ReadString()));
                            w.CharacterNames.Add(new NpcName(19, b.ReadString()));
                            w.CharacterNames.Add(new NpcName(20, b.ReadString()));
                            w.CharacterNames.Add(new NpcName(22, b.ReadString()));
                            w.CharacterNames.Add(new NpcName(54, b.ReadString()));
                            w.CharacterNames.Add(new NpcName(38, b.ReadString()));
                            w.CharacterNames.Add(new NpcName(107, b.ReadString()));
                            w.CharacterNames.Add(new NpcName(108, b.ReadString()));
                            if (w.Version >= 35)
                                w.CharacterNames.Add(new NpcName(124, b.ReadString()));
                            else
                            {
                                w.CharacterNames.Add(new NpcName(124, "Nancy"));
                            }
                        }
                        else
                        {
                            w.CharacterNames.Add(new NpcName(17, "Harold"));
                            w.CharacterNames.Add(new NpcName(18, "Molly"));
                            w.CharacterNames.Add(new NpcName(19, "Dominique"));
                            w.CharacterNames.Add(new NpcName(20, "Felicitae"));
                            w.CharacterNames.Add(new NpcName(22, "Steve"));
                            w.CharacterNames.Add(new NpcName(54, "Fitz"));
                            w.CharacterNames.Add(new NpcName(38, "Gimut"));
                            w.CharacterNames.Add(new NpcName(107, "Knogs"));
                            w.CharacterNames.Add(new NpcName(108, "Fizban"));
                            w.CharacterNames.Add(new NpcName(124, "Nancy"));
                        }
                        if (w.Version >= 7)
                        {
                            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Validating File..."));
                            bool validation = b.ReadBoolean();
                            string checkTitle = b.ReadString();
                            int checkVersion = b.ReadInt32();
                            if (validation && checkTitle == w.Title && checkVersion == w.WorldId)
                            {
                                //w.loadSuccess = true;
                            }
                            else
                            {
                                b.Close();
                                throw new FileLoadException(string.Format("Error reading world file validation parameters! {0}", filename));
                            }
                        }
                        OnProgressChanged(null, new ProgressChangedEventArgs(0, "World Load Complete."));

                    }
                }
            }
            catch (Exception err)
            {
                string msg = "There was an error reading the world file, do you wish to force it to load anyway?\r\n\r\n" +
                             "WARNING: This may have unexpected results including corrupt world files and program crashes.\r\n\r\n" +
                             "The error is :\r\n";
                if (MessageBox.Show(msg + err, "World File Error", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                    return null;
            }
            return w;
        }
    }
}