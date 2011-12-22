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
            WorldID = r.Next(int.MaxValue);
            _npcs.Clear();
            _signs.Clear();
            _chests.Clear();
            _charNames.Clear();
        }

        public Tile[,] Tiles;

        private readonly IList<NPC> _npcs = new ObservableCollection<NPC>();
        public IList<NPC> NPCs
        {
            get { return _npcs; }
        }

        private readonly IList<Sign> _signs = new ObservableCollection<Sign>();
        public IList<Sign> Signs
        {
            get { return _signs; }
        }

        private readonly IList<Chest> _chests = new ObservableCollection<Chest>();
        public IList<Chest> Chests
        {
            get { return _chests; }
        }

        private readonly Dictionary<int, string> _charNames = new Dictionary<int, string>();
        public Dictionary<int, string> CharacterNames
        {
            get { return _charNames; }
        }

        public uint Version;
        private string _title;
        private int _spawnX;
        private int _spawnY;
        private int _groundLevel;
        private int _rockLevel;
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

        public int WorldID;
        public float LeftWorld;
        public float RightWorld;
        public float TopWorld;
        public float BottomWorld;
        public int TilesHigh;
        public int TilesWide;


        #region Properties

        
         

        public bool DownedFrost
        {
            get { return _downedFrost; }
            set { Set("IsDownedFrost", ref _downedFrost, value); }
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

        public int GroundLevel
        {
            get { return _groundLevel; }
            set { Set("GroundLevel", ref _groundLevel, value); }
        }

        public int RockLevel
        {
            get { return _rockLevel; }
            set { Set("RockLevel", ref _rockLevel, value); }
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
            set { Set("ShadowOrbCount", ref _shadowOrbCount, value); }
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
            if (resetTime)
            {
                DayTime = true;
                Time = 13500.0;
                MoonPhase = 0;
                BloodMoon = false;
            }

            if (filename == null)
                return;
            string temp = filename;
            using (var fs = new FileStream(temp, FileMode.Create))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(Version);
                    bw.Write(Title);
                    bw.Write(WorldID);
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
                        // statusText = "Saving world data: " + (object) (int) ((double) ((float) local_5 / (float) TilesWide) * 100.0 + 1.0) + "%";
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
                            rle = 1;
                            while (y + rle < TilesHigh && curTile.Equals(Tiles[x, (y + rle)]))
                                ++rle;
                            rle = rle - 1;
                            bw.Write((short)rle);
                        }
                    }
                    for (int i = 0; i < 1000; ++i)
                    {
                        if (Chests.Count <= i)
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
                                bw.Write((byte)curChest.Items[j].StackSize);
                                if (curChest.Items[j].StackSize > 0)
                                {
                                    bw.Write(curChest.Items[j].ItemName);
                                    bw.Write(curChest.Items[j].Prefix);
                                }
                            }
                        }
                    }
                    for (int i = 0; i < 1000; ++i)
                    {
                        if (Signs.Count <= i || string.IsNullOrWhiteSpace(Signs[i].Text))
                        {
                            bw.Write(false);
                        }
                        else
                        {
                            Sign curSign = Signs[i];
                            bw.Write(true);
                            bw.Write(curSign.Text);
                            bw.Write(curSign.X);
                            bw.Write(curSign.Y);
                        }
                    }
                    for (int i = 0; i < 200; ++i)
                    {
                        if (NPCs.Count <= i)
                        {
                            bw.Write(false);
                            break;
                        }

                        NPC curNpc = (NPC)NPCs[i];

                        bw.Write(true);
                        bw.Write(curNpc.Name);
                        bw.Write(curNpc.Position.X);
                        bw.Write(curNpc.Position.Y);
                        bw.Write(curNpc.IsHomeless);
                        bw.Write(curNpc.Home.X);
                        bw.Write(curNpc.Home.Y);

                    }

                    bw.Write(CharacterNames[17]);
                    bw.Write(CharacterNames[18]);
                    bw.Write(CharacterNames[19]);
                    bw.Write(CharacterNames[20]);
                    bw.Write(CharacterNames[22]);
                    bw.Write(CharacterNames[54]);
                    bw.Write(CharacterNames[38]);
                    bw.Write(CharacterNames[107]);
                    bw.Write(CharacterNames[108]);
                    bw.Write(CharacterNames[124]);
                    bw.Write(true);
                    bw.Write(Title);
                    bw.Write(WorldID);
                    bw.Close();
                    fs.Close();
                    if (File.Exists(filename))
                    {
                        //statusText = "Backing up world file...";
                        string backup = filename + ".TEdit";
                        File.Copy(filename, backup, true);
                    }
                    File.Copy(temp, filename, true);
                    File.Delete(temp);
                }
            }
        }

        public static World LoadWorld(string filename)
        {
            var w = new World();
            using (var b = new BinaryReader(File.OpenRead(filename)))
            {
                w.Version = b.ReadUInt32(); //now we care about the version
                w.Title = b.ReadString();

                w.WorldID = b.ReadInt32();
                w.LeftWorld = (float)b.ReadInt32();
                w.RightWorld = (float)b.ReadInt32();
                w.TopWorld = (float)b.ReadInt32();
                w.BottomWorld = (float)b.ReadInt32();

                w.TilesHigh = b.ReadInt32();
                w.TilesWide = b.ReadInt32();
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
                    // statusText = "Saving world data: " + (object) (int) ((double) ((float) local_5 / (float) TilesWide) * 100.0 + 1.0) + "%";
                    //int rle = 0;
                    for (int y = 0; y < w.TilesHigh; y++)
                    {
                        w.Tiles[x, y].IsActive = b.ReadBoolean();
                        if (w.Tiles[x, y].IsActive)
                        {
                            w.Tiles[x, y].Type = b.ReadByte();
                            if (w.Tiles[x, y].Type == 0x7f)
                                w.Tiles[x, y].IsActive = false;
                            if (TileProperties[w.Tiles[x, y].Type].IsFramed)
                            {
                                // torches didn't have extra in older versions.
                                if (w.Version < 0x1c && w.Tiles[x, y].Type == 4)
                                {
                                    w.Tiles[x, y].U = 0;
                                    w.Tiles[x, y].V = 0;
                                }
                                else
                                {
                                    w.Tiles[x, y].U = b.ReadInt16();
                                    w.Tiles[x, y].V = b.ReadInt16();
                                    if (w.Tiles[x, y].Type == 128) //armor stand
                                        w.Tiles[x, y].U %= 100;
                                    if (w.Tiles[x, y].Type == 144) //timer
                                        w.Tiles[x, y].V = 0;
                                }


                            }
                            else
                            {
                                w.Tiles[x, y].U = -1;
                                w.Tiles[x, y].V = -1;
                            }
                        }
                        if (w.Version <= 25)
                            b.ReadBoolean(); //skip obsolete hasLight
                        if (b.ReadBoolean())
                        {
                            w.Tiles[x, y].Wall = b.ReadByte();
                            w.Tiles[x, y].WallU = -1;
                            w.Tiles[x, y].WallV = -1;
                        }
                        else
                            w.Tiles[x, y].Wall = 0;
                        if (b.ReadBoolean())
                        {
                            w.Tiles[x, y].Liquid = b.ReadByte();
                            w.Tiles[x, y].IsLava = b.ReadBoolean();
                        }
                        else
                            w.Tiles[x, y].Liquid = 0;
                        if (w.Version >= 33)
                            w.Tiles[x, y].HasWire = b.ReadBoolean();
                        else
                            w.Tiles[x, y].HasWire = false;
                        if (w.Version >= 25) //compression ftw :)
                        {
                            int rle = b.ReadInt16();
                            for (int r = 1; r < rle + 1; r++)
                            {
                                w.Tiles[x, y + r] = w.Tiles[x, y + r].DeepCopy();
                            }
                        }
                    }
                    w.Chests.Clear();
                    for (int i = 0; i < 1000; i++)
                    {
                        if (b.ReadBoolean())
                        {
                            var chest = new Chest(b.ReadInt32(), b.ReadInt32());
                            for (int ii = 0; ii < Chest.MaxItems; ii++)
                            {
                                chest.Items[ii].StackSize = b.ReadByte();
                                if (chest.Items[ii].StackSize > 0)
                                {
                                    chest.Items[ii].ItemName = b.ReadString();
                                    if (w.Version >= 36) //item prefixes
                                        chest.Items[ii].Prefix = b.ReadByte(); //toss prefix
                                }
                            }
                            w.Chests.Add(chest);
                        }
                    }
                    w.Signs.Clear();
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
                    while (b.ReadBoolean())
                    {
                        var npc = new NPC();
                        npc.Name = b.ReadString();
                        npc.Position = new BCCL.Geometry.Primitives.Vector2(b.ReadSingle(), b.ReadSingle());
                        npc.IsHomeless = b.ReadBoolean();
                        npc.Home = new Vector2Int32(b.ReadInt32(), b.ReadInt32());
                        npc.SpriteId = 0;
                        if (npc.Name == "Merchant") npc.SpriteId = 17;
                        if (npc.Name == "Nurse") npc.SpriteId = 18;
                        if (npc.Name == "Arms Dealer") npc.SpriteId = 19;
                        if (npc.Name == "Dryad") npc.SpriteId = 20;
                        if (npc.Name == "Guide") npc.SpriteId = 22;
                        if (npc.Name == "Old Man") npc.SpriteId = 37;
                        if (npc.Name == "Demolitionist") npc.SpriteId = 38;
                        if (npc.Name == "Clothier") npc.SpriteId = 54;
                        if (npc.Name == "Goblin Tinkerer") npc.SpriteId = 107;
                        if (npc.Name == "Wizard") npc.SpriteId = 108;
                        if (npc.Name == "Mechanic") npc.SpriteId = 124;

                        w.NPCs.Add(npc);
                    }
                    // if (version>=0x1f) read the names of the following npcs:
                    // merchant, nurse, arms dealer, dryad, guide, clothier, demolitionist,
                    // tinkerer and wizard
                    // if (version>=0x23) read the name of the mechanic

                    if (w.Version >= 31)
                    {
                        w.CharacterNames.Add(17, b.ReadString());
                        w.CharacterNames.Add(18, b.ReadString());
                        w.CharacterNames.Add(19, b.ReadString());
                        w.CharacterNames.Add(20, b.ReadString());
                        w.CharacterNames.Add(22, b.ReadString());
                        w.CharacterNames.Add(54, b.ReadString());
                        w.CharacterNames.Add(38, b.ReadString());
                        w.CharacterNames.Add(107, b.ReadString());
                        w.CharacterNames.Add(108, b.ReadString());
                        if (w.Version >= 35)
                            w.CharacterNames.Add(124, b.ReadString());
                    }
                    if (w.Version >= 7)
                    {
                        bool validation = b.ReadBoolean();
                        string checkTitle = b.ReadString();
                        int checkVersion = b.ReadInt32();
                        if (validation && checkTitle == w.Title && checkVersion == w.WorldID)
                        {
                            //w.loadSuccess = true;
                        }
                        else
                        {
                            b.Close();
                            throw new FileLoadException("Error reading world file validation parameters!");
                        }
                    }

                }
            }
            return w;
        }
    }


}
