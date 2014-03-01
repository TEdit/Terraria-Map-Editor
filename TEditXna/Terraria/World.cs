using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using BCCL.MvvmLight;
using BCCL.MvvmLight.Threading;
using BCCL.Utility;
using TEditXna.Helper;
using TEditXNA.Terraria.Objects;
using BCCL.Geometry.Primitives;
using Vector2 = BCCL.Geometry.Primitives.Vector2;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace TEditXNA.Terraria
{

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

        public Vector2Int32 GetChestAnchor(int x, int y)
        {
            var tile = Tiles[x, y];

            int xShift = tile.U % 36 / 18;
            int yShift = tile.V % 36 / 18;

            return new Vector2Int32(x-xShift, y-yShift);
        }

        public Vector2Int32 GetSignAnchor(int x, int y)
        {
            var tile = Tiles[x, y];

            int xShift = tile.U % 36 / 18;
            int yShift = tile.V % 36 / 18;

            return new Vector2Int32(x - xShift, y - yShift);
        }

        public void Validate()
        {
            for (int x = 0; x < TilesWide; x++)
            {
                OnProgressChanged(this, new ProgressChangedEventArgs((int)(x / (float)TilesWide * 100.0), "Validating World..."));

                for (int y = 0; y < TilesHigh; y++)
                {
                    var curTile = Tiles[x, y];

                    if (curTile.Type == 127)
                        curTile.IsActive = false;

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
        }

        public void Save(string filename, bool resetTime = false)
        {
            lock (_fileLock)
            {
                OnProgressChanged(this, new ProgressChangedEventArgs(0, "Validating World..."));
                Validate();

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

                        bw.Write((byte)MoonType);
                        bw.Write(TreeX[0]);
                        bw.Write(TreeX[1]);
                        bw.Write(TreeX[2]);
                        bw.Write(TreeStyle[0]);
                        bw.Write(TreeStyle[1]);
                        bw.Write(TreeStyle[2]);
                        bw.Write(TreeStyle[3]);
                        bw.Write(CaveBackX[0]);
                        bw.Write(CaveBackX[1]);
                        bw.Write(CaveBackX[2]);
                        bw.Write(CaveBackStyle[0]);
                        bw.Write(CaveBackStyle[1]);
                        bw.Write(CaveBackStyle[2]);
                        bw.Write(CaveBackStyle[3]);
                        bw.Write(IceBackStyle);
                        bw.Write(JungleBackStyle);
                        bw.Write(HellBackStyle);

                        bw.Write(SpawnX);
                        bw.Write(SpawnY);
                        bw.Write(GroundLevel);
                        bw.Write(RockLevel);
                        bw.Write(Time);
                        bw.Write(DayTime);
                        bw.Write(MoonPhase);
                        bw.Write(BloodMoon);
                        bw.Write(IsEclipse);
                        bw.Write(DungeonX);
                        bw.Write(DungeonY);

                        bw.Write(IsCrimson);

                        bw.Write(DownedBoss1);
                        bw.Write(DownedBoss2);
                        bw.Write(DownedBoss3);
                        bw.Write(DownedQueenBee);
                        bw.Write(DownedMechBoss1);
                        bw.Write(DownedMechBoss2);
                        bw.Write(DownedMechBoss3);
                        bw.Write(DownedMechBossAny);
                        bw.Write(DownedPlantBoss);
                        bw.Write(DownedGolemBoss);
                        bw.Write(SavedGoblin);
                        bw.Write(SavedWizard);
                        bw.Write(SavedMech);
                        bw.Write(DownedGoblins);
                        bw.Write(DownedClown);
                        bw.Write(DownedFrost);
                        bw.Write(DownedPirates);

                        bw.Write(ShadowOrbSmashed);
                        bw.Write(SpawnMeteor);
                        bw.Write((byte)ShadowOrbCount);
                        bw.Write(AltarCount);
                        bw.Write(HardMode);
                        bw.Write(InvasionDelay);
                        bw.Write(InvasionSize);
                        bw.Write(InvasionType);
                        bw.Write(InvasionX);

                        bw.Write(TempRaining);
                        bw.Write(TempRainTime);
                        bw.Write((float)TempMaxRain);
                        bw.Write(OreTier1);
                        bw.Write(OreTier2);
                        bw.Write(OreTier3);
                        bw.Write((byte)BgTree);
                        bw.Write((byte)BgCorruption);
                        bw.Write((byte)BgJungle);
                        bw.Write((byte)BgSnow);
                        bw.Write((byte)BgHallow);
                        bw.Write((byte)BgCrimson);
                        bw.Write((byte)BgDesert);
                        bw.Write((byte)BgOcean);
                        bw.Write((int)CloudBgActive);
                        bw.Write((short)NumClouds);
                        bw.Write((float)WindSpeedSet);


                        for (int x = 0; x < TilesWide; ++x)
                        {
                            OnProgressChanged(this, new ProgressChangedEventArgs(x.ProgressPercentage(TilesWide), "Saving Tiles..."));

                            int rle = 0;
                            for (int y = 0; y < TilesHigh; y = y + rle + 1)
                            {

                                var curTile = Tiles[x, y];
                                WriteTileDataToStream(curTile, bw);

                                int rleTemp = 1;
                                while (y + rleTemp < TilesHigh && curTile.Equals(Tiles[x, (y + rleTemp)]))
                                    ++rleTemp;
                                rle = rleTemp - 1;
                                bw.Write((short)rle);
                            }
                        }
                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Saving Chests..."));
                        WriteChestDataToStream(Chests, bw);
                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Saving Signs..."));
                        WriteSignDataToStream(Signs, bw);
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

                        FixNpcs();

                        bw.Write(GetNpc(17).Name);
                        bw.Write(GetNpc(18).Name);
                        bw.Write(GetNpc(19).Name);
                        bw.Write(GetNpc(20).Name);
                        bw.Write(GetNpc(22).Name);
                        bw.Write(GetNpc(54).Name);
                        bw.Write(GetNpc(38).Name);
                        bw.Write(GetNpc(107).Name);
                        bw.Write(GetNpc(108).Name);
                        bw.Write(GetNpc(124).Name);
                        bw.Write(GetNpc(160).Name);
                        bw.Write(GetNpc(178).Name);
                        bw.Write(GetNpc(207).Name);
                        bw.Write(GetNpc(208).Name);
                        bw.Write(GetNpc(209).Name);
                        bw.Write(GetNpc(227).Name);
                        bw.Write(GetNpc(228).Name);
                        bw.Write(GetNpc(229).Name);

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

                _lastSave = File.GetLastWriteTimeUtc(filename);
            }
        }

        public static void WriteSignDataToStream(IList<Sign> signs, BinaryWriter bw)
        {
            for (int i = 0; i < 1000; ++i)
            {
                if (i >= signs.Count || string.IsNullOrWhiteSpace(signs[i].Text))
                {
                    bw.Write(false);
                }
                else
                {
                    var curSign = signs[i];
                    bw.Write(true);
                    bw.Write(curSign.Text);
                    bw.Write(curSign.X);
                    bw.Write(curSign.Y);
                }
            }
        }

        public static void WriteChestDataToStream(IList<Chest> chests, BinaryWriter bw)
        {
            for (int i = 0; i < 1000; ++i)
            {
                if (i >= chests.Count)
                {
                    bw.Write(false);
                }
                else
                {
                    Chest curChest = chests[i];
                    bw.Write(true);
                    bw.Write(curChest.X);
                    bw.Write(curChest.Y);
                    for (int j = 0; j < Chest.MaxItems; ++j)
                    {
                        if (curChest.Items.Count > j)
                        {
                            if (curChest.Items[j].NetId == 0)
                                curChest.Items[j].StackSize = 0;

                            bw.Write((short)curChest.Items[j].StackSize);
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
        }

        public static void WriteTileDataToStream(Tile curTile, BinaryWriter bw)
        {
            if (curTile.Type == 127)
                curTile.IsActive = false;

            bw.Write(curTile.IsActive);
            if (curTile.IsActive)
            {
                bw.Write(curTile.Type);
                if (TileProperties[curTile.Type].IsFramed)
                {
                    bw.Write(curTile.U);
                    bw.Write(curTile.V);
                }

                if (curTile.TileColor > 0)
                {
                    bw.Write(true);
                    bw.Write(curTile.TileColor);
                }
                else
                    bw.Write(false);
            }
            if ((int)curTile.Wall > 0)
            {
                bw.Write(true);
                bw.Write(curTile.Wall);

                if (curTile.WallColor > 0)
                {
                    bw.Write(true);
                    bw.Write(curTile.WallColor);
                }
                else
                    bw.Write(false);
            }
            else
                bw.Write(false);

            if ((int)curTile.LiquidAmount > 0)
            {
                bw.Write(true);
                bw.Write(curTile.LiquidAmount);
                bw.Write((bool)(curTile.LiquidType == LiquidType.Lava));
                bw.Write((bool)(curTile.LiquidType == LiquidType.Honey));
            }
            else
                bw.Write(false);

            bw.Write(curTile.WireRed);
            bw.Write(curTile.WireGreen);
            bw.Write(curTile.WireBlue);
            bw.Write((bool)(curTile.BrickStyle != 0));
            bw.Write((byte)curTile.BrickStyle);
            bw.Write(curTile.Actuator);
            bw.Write(curTile.InActive);
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
                        uint version = w.Version;
                        w.Title = b.ReadString();
                        w.WorldId = b.ReadInt32();

                        w.Rand = new Random(w.WorldId);

                        w.LeftWorld = (float)b.ReadInt32();
                        w.RightWorld = (float)b.ReadInt32();
                        w.TopWorld = (float)b.ReadInt32();
                        w.BottomWorld = (float)b.ReadInt32();
                        w.TilesHigh = b.ReadInt32();
                        w.TilesWide = b.ReadInt32();

                        //if (w.TilesHigh > 10000 || w.TilesWide > 10000 || w.TilesHigh <= 0 || w.TilesWide <= 0)
                        //    throw new FileLoadException(string.Format("Invalid File: {0}", filename));



                        if (version >= 63)
                            w.MoonType = (int)b.ReadByte();
                        else
                            w.MoonType = w.Rand.Next(MaxMoons);


                        if (version >= 44)
                        {
                            w.TreeX[0] = b.ReadInt32();
                            w.TreeX[1] = b.ReadInt32();
                            w.TreeX[2] = b.ReadInt32();
                            w.TreeStyle[0] = b.ReadInt32();
                            w.TreeStyle[1] = b.ReadInt32();
                            w.TreeStyle[2] = b.ReadInt32();
                            w.TreeStyle[3] = b.ReadInt32();
                        }
                        if (version >= 60)
                        {
                            w.CaveBackX[0] = b.ReadInt32();
                            w.CaveBackX[1] = b.ReadInt32();
                            w.CaveBackX[2] = b.ReadInt32();
                            w.CaveBackStyle[0] = b.ReadInt32();
                            w.CaveBackStyle[1] = b.ReadInt32();
                            w.CaveBackStyle[2] = b.ReadInt32();
                            w.CaveBackStyle[3] = b.ReadInt32();
                            w.IceBackStyle = b.ReadInt32();
                            if (version >= 61)
                            {
                                w.JungleBackStyle = b.ReadInt32();
                                w.HellBackStyle = b.ReadInt32();
                            }
                        }
                        else
                        {
                            w.CaveBackX[0] = w.TilesWide / 2;
                            w.CaveBackX[1] = w.TilesWide;
                            w.CaveBackX[2] = w.TilesWide;
                            w.CaveBackStyle[0] = 0;
                            w.CaveBackStyle[1] = 1;
                            w.CaveBackStyle[2] = 2;
                            w.CaveBackStyle[3] = 3;
                            w.IceBackStyle = 0;
                            w.JungleBackStyle = 0;
                            w.HellBackStyle = 0;
                        }

                        w.SpawnX = b.ReadInt32();
                        w.SpawnY = b.ReadInt32();
                        w.GroundLevel = (int)b.ReadDouble();
                        w.RockLevel = (int)b.ReadDouble();

                        // read world flags
                        w.Time = b.ReadDouble();
                        w.DayTime = b.ReadBoolean();
                        w.MoonPhase = b.ReadInt32();
                        w.BloodMoon = b.ReadBoolean();

                        if (version >= 70)
                        {
                            w.IsEclipse = b.ReadBoolean();
                        }

                        w.DungeonX = b.ReadInt32();
                        w.DungeonY = b.ReadInt32();

                        if (version >= 56)
                        {
                            w.IsCrimson = b.ReadBoolean();
                        }
                        else
                        {
                            w.IsCrimson = false;
                        }

                        w.DownedBoss1 = b.ReadBoolean();
                        w.DownedBoss2 = b.ReadBoolean();
                        w.DownedBoss3 = b.ReadBoolean();

                        if (version >= 66)
                        {
                            w.DownedQueenBee = b.ReadBoolean();
                        }
                        if (version >= 44)
                        {
                            w.DownedMechBoss1 = b.ReadBoolean();
                            w.DownedMechBoss2 = b.ReadBoolean();
                            w.DownedMechBoss3 = b.ReadBoolean();
                            w.DownedMechBossAny = b.ReadBoolean();
                        }
                        if (version >= 64)
                        {
                            w.DownedPlantBoss = b.ReadBoolean();
                            w.DownedGolemBoss = b.ReadBoolean();
                        }
                        if (version >= 29)
                        {
                            w.SavedGoblin = b.ReadBoolean();
                            w.SavedWizard = b.ReadBoolean();
                            if (version >= 34)
                            {
                                w.SavedMech = b.ReadBoolean();
                            }
                            w.DownedGoblins = b.ReadBoolean();
                        }
                        if (version >= 32)
                            w.DownedClown = b.ReadBoolean();
                        if (version >= 37)
                            w.DownedFrost = b.ReadBoolean();
                        if (version >= 56)
                            w.DownedPirates = b.ReadBoolean();


                        w.ShadowOrbSmashed = b.ReadBoolean();
                        w.SpawnMeteor = b.ReadBoolean();
                        w.ShadowOrbCount = (int)b.ReadByte();

                        if (version >= 23)
                        {
                            w.AltarCount = b.ReadInt32();
                            w.HardMode = b.ReadBoolean();
                        }

                        w.InvasionDelay = b.ReadInt32();
                        w.InvasionSize = b.ReadInt32();
                        w.InvasionType = b.ReadInt32();
                        w.InvasionX = b.ReadDouble();

                        if (version >= 53)
                        {
                            w.TempRaining = b.ReadBoolean();
                            w.TempRainTime = b.ReadInt32();
                            w.TempMaxRain = b.ReadSingle();
                        }
                        if (version >= 54)
                        {
                            w.OreTier1 = b.ReadInt32();
                            w.OreTier2 = b.ReadInt32();
                            w.OreTier3 = b.ReadInt32();
                        }
                        else if (version < 23 || w.AltarCount != 0)
                        {
                            w.OreTier1 = 107;
                            w.OreTier2 = 108;
                            w.OreTier3 = 111;
                        }
                        else
                        {
                            w.OreTier1 = -1;
                            w.OreTier2 = -1;
                            w.OreTier3 = -1;
                        }

                        if (version >= 55)
                        {
                            w.BgTree = b.ReadByte();
                            w.BgCorruption = b.ReadByte();
                            w.BgJungle = b.ReadByte();
                        }
                        if (version >= 60)
                        {
                            w.BgSnow = b.ReadByte();
                            w.BgHallow = b.ReadByte();
                            w.BgCorruption = b.ReadByte();
                            w.BgDesert = b.ReadByte();
                            w.BgOcean = b.ReadByte();
                        }

                        if (version >= 60)
                        {
                            w.CloudBgActive = (float)b.ReadInt32();
                        }
                        else
                        {
                            w.CloudBgActive = -w.Rand.Next(8640, 86400);
                        }

                        if (version >= 62)
                        {
                            w.NumClouds = b.ReadInt16();
                            w.WindSpeedSet = b.ReadSingle();
                        }

                        w.Tiles = new Tile[w.TilesWide, w.TilesHigh];
                        for (int i = 0; i < w.TilesWide; i++)
                        {
                            for (int j = 0; j < w.TilesHigh; j++)
                            {
                                w.Tiles[i, j] = new Tile();
                            }
                        }


                        for (int x = 0; x < w.TilesWide; ++x)
                        {
                            OnProgressChanged(null, new ProgressChangedEventArgs(x.ProgressPercentage(w.TilesWide), "Loading Tiles..."));

                            for (int y = 0; y < w.TilesHigh; y++)
                            {
                                var tile = ReadTileDataFromStream(b, version);

                                // read complete, start compression
                                w.Tiles[x, y] = tile;

                                if (version >= 25)
                                {
                                    int rle = b.ReadInt16();

                                    if (rle < 0)
                                        throw new ApplicationException("Invalid Tile Data!");

                                    if (rle > 0)
                                    {
                                        for (int k = y + 1; k < y + rle + 1; k++)
                                        {
                                            var tcopy = (Tile)tile.Clone();
                                            w.Tiles[x, k] = tcopy;
                                        }
                                        y = y + rle;
                                    }
                                }
                            }
                        }

                        if (version < 67)
                            w.FixSunflowers();
                        if (version < 72)
                            w.FixChand();

                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading Chests..."));
                        w.Chests.Clear();
                        w.Chests.AddRange(ReadChestDataFromStream(b, version));

                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading Signs..."));
                        w.Signs.Clear();

                        foreach (var sign in ReadSignDataFromStream(b))
                        {
                            if (w.Tiles[sign.X, sign.Y].IsActive && ((int)w.Tiles[sign.X, sign.Y].Type == 55 || (int)w.Tiles[sign.X, sign.Y].Type == 85))
                                w.Signs.Add(sign);
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


                        if (version >= 31)
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
                            if (version >= 35)
                                w.CharacterNames.Add(new NpcName(124, b.ReadString()));
                            else
                                w.CharacterNames.Add(new NpcName(124, "Nancy"));

                            if (version >= 65)
                            {
                                w.CharacterNames.Add(new NpcName(160, b.ReadString()));
                                w.CharacterNames.Add(new NpcName(178, b.ReadString()));
                                w.CharacterNames.Add(new NpcName(207, b.ReadString()));
                                w.CharacterNames.Add(new NpcName(208, b.ReadString()));
                                w.CharacterNames.Add(new NpcName(209, b.ReadString()));
                                w.CharacterNames.Add(new NpcName(227, b.ReadString()));
                                w.CharacterNames.Add(new NpcName(228, b.ReadString()));
                                w.CharacterNames.Add(new NpcName(229, b.ReadString()));
                            }
                            else
                            {
                                w.CharacterNames.Add(GetNewNpc(160));
                                w.CharacterNames.Add(GetNewNpc(178));
                                w.CharacterNames.Add(GetNewNpc(207));
                                w.CharacterNames.Add(GetNewNpc(208));
                                w.CharacterNames.Add(GetNewNpc(209));
                                w.CharacterNames.Add(GetNewNpc(227));
                                w.CharacterNames.Add(GetNewNpc(228));
                                w.CharacterNames.Add(GetNewNpc(229));
                            }
                        }
                        else
                        {
                            w.CharacterNames.Add(GetNewNpc(17));
                            w.CharacterNames.Add(GetNewNpc(18));
                            w.CharacterNames.Add(GetNewNpc(19));
                            w.CharacterNames.Add(GetNewNpc(20));
                            w.CharacterNames.Add(GetNewNpc(22));
                            w.CharacterNames.Add(GetNewNpc(54));
                            w.CharacterNames.Add(GetNewNpc(38));
                            w.CharacterNames.Add(GetNewNpc(107));
                            w.CharacterNames.Add(GetNewNpc(108));
                            w.CharacterNames.Add(GetNewNpc(124));
                            w.CharacterNames.Add(GetNewNpc(160));
                            w.CharacterNames.Add(GetNewNpc(178));
                            w.CharacterNames.Add(GetNewNpc(207));
                            w.CharacterNames.Add(GetNewNpc(208));
                            w.CharacterNames.Add(GetNewNpc(209));
                            w.CharacterNames.Add(GetNewNpc(227));
                            w.CharacterNames.Add(GetNewNpc(228));
                            w.CharacterNames.Add(GetNewNpc(229));

                        }
                        if (version >= 7)
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
                    w.LastSave = File.GetLastWriteTimeUtc(filename);
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

        private void FixChand()
        {
            for (int x = 5; x < TilesWide - 5; x++)
            {
                for (int y = 5; y < TilesHigh - 5; y++)
                {
                    if (Tiles[x, y].IsActive)
                    {
                        int tileType = Tiles[x, y].Type;
                        if (Tiles[x, y].IsActive && (tileType == 35 || tileType == 36 || tileType == 170 || tileType == 171 || tileType == 172))
                        {
                            FixChand(x, y);
                        }
                    }
                }
            }
        }

        public void FixChand(int x, int y)
        {
            int newPosition = 0;
            int type = Tiles[x, y].Type;
            if (Tiles[x, y].IsActive)
            {
                if (type == 35)
                {
                    newPosition = 1;
                }
                if (type == 36)
                {
                    newPosition = 2;
                }
                if (type == 170)
                {
                    newPosition = 3;
                }
                if (type == 171)
                {
                    newPosition = 4;
                }
                if (type == 172)
                {
                    newPosition = 5;
                }
            }
            if (newPosition > 0)
            {
                int xShift = x;
                int yShift = y;
                xShift = Tiles[x, y].U / 18;
                while (xShift >= 3)
                {
                    xShift = xShift - 3;
                }
                if (xShift >= 3)
                {
                    xShift = xShift - 3;
                }
                xShift = x - xShift;
                yShift = yShift + Tiles[x, y].V / 18 * -1;
                for (int x1 = xShift; x1 < xShift + 3; x1++)
                {
                    for (int y1 = yShift; y1 < yShift + 3; y1++)
                    {
                        if (Tiles[x1, y1] == null)
                        {
                            Tiles[x1, y1] = new Tile();
                        }
                        if (Tiles[x1, y1].IsActive && Tiles[x1, y1].Type == type)
                        {
                            Tiles[x1, y1].Type = 34;
                            Tiles[x1, y1].V = (short)(Tiles[x1, y1].V + newPosition * 54);
                        }
                    }
                }
            }
        }

        public static IEnumerable<Sign> ReadSignDataFromStream(BinaryReader b)
        {
            for (int i = 0; i < 1000; i++)
            {
                if (b.ReadBoolean())
                {
                    Sign sign = new Sign();
                    sign.Text = b.ReadString();
                    sign.X = b.ReadInt32();
                    sign.Y = b.ReadInt32();

                    yield return sign;
                }
            }
        }

        public static IEnumerable<Chest> ReadChestDataFromStream(BinaryReader b, uint version)
        {
            int chestSize = Chest.MaxItems;
            if (version < 58)
                chestSize = 20;

            for (int i = 0; i < 1000; i++)
            {
                if (b.ReadBoolean())
                {
                    var chest = new Chest(b.ReadInt32(), b.ReadInt32());
                    for (int slot = 0; slot < Chest.MaxItems; slot++)
                    {
                        if (slot < chestSize)
                        {

                            int stackSize = version < 59 ? b.ReadByte() : (int)b.ReadInt16();
                            chest.Items[slot].StackSize = stackSize;

                            if (chest.Items[slot].StackSize > 0)
                            {
                                if (version >= 38)
                                    chest.Items[slot].NetId = b.ReadInt32();
                                else
                                    chest.Items[slot].SetFromName(b.ReadString());

                                chest.Items[slot].StackSize = stackSize;
                                // Read prefix
                                if (version >= 36)
                                    chest.Items[slot].Prefix = b.ReadByte();
                            }
                        }
                    }
                    yield return chest;
                }
            }
        }

        public static Tile ReadTileDataFromStream(BinaryReader b, uint version)
        {
            Tile tile = new Tile();

            tile.IsActive = b.ReadBoolean();

            TileProperty tileProperty = null;

            if (tile.IsActive)
            {
                tile.Type = b.ReadByte();
                tileProperty = TileProperties[tile.Type];


                if (tile.Type == 127)
                    tile.IsActive = false;

                if (version < 72 && (tile.Type == 35 || tile.Type == 36 || tile.Type == 170 || tile.Type == 171 || tile.Type == 172))
                {
                    tile.U = b.ReadInt16();
                    tile.V = b.ReadInt16();
                }
                else if (!tileProperty.IsFramed)
                {
                    tile.U = -1;
                    tile.V = -1;
                }
                else if (version < 28 && tile.Type == 4)
                {
                    // torches didn't have extra in older versions.
                    tile.U = 0;
                    tile.V = 0;
                }
                else if (version < 40 && tile.Type == 19)
                {
                    tile.U = 0;
                    tile.V = 0;
                }
                else
                {
                    tile.U = b.ReadInt16();
                    tile.V = b.ReadInt16();

                    if (tile.Type == 144) //timer
                        tile.V = 0;

                }


                if (version >= 48 && b.ReadBoolean())
                {
                    tile.TileColor = b.ReadByte();
                }
            }

            //skip obsolete hasLight
            if (version <= 25)
                b.ReadBoolean();

            if (b.ReadBoolean())
            {
                tile.Wall = b.ReadByte();
                if (version >= 48 && b.ReadBoolean())
                    tile.WallColor = b.ReadByte();
            }

            if (b.ReadBoolean())
            {
                tile.LiquidType = LiquidType.Water;
                tile.LiquidAmount = b.ReadByte();
                if (b.ReadBoolean()) tile.LiquidType = LiquidType.Lava;
                if (version >= 51)
                {
                    if (b.ReadBoolean()) tile.LiquidType = LiquidType.Honey;
                }
            }

            if (version >= 33)
            {
                tile.WireRed = b.ReadBoolean();
            }
            if (version >= 43)
            {
                tile.WireGreen = b.ReadBoolean();
                tile.WireBlue = b.ReadBoolean();
            }

            if (version >= 41)
            {
                bool isHalfBrick = b.ReadBoolean();

                if (tileProperty == null || !tileProperty.IsSolid)
                    isHalfBrick = false;

                if (version >= 49)
                {
                    tile.BrickStyle = (BrickStyle)b.ReadByte();

                    if (tileProperty == null || !tileProperty.IsSolid)
                        tile.BrickStyle = 0;
                }
            }
            if (version >= 42)
            {
                tile.Actuator = b.ReadBoolean();
                tile.InActive = b.ReadBoolean();
            }
            return tile;
        }

        public void FixNpcs()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    int[] npcids = { 17, 18, 19, 20, 22, 54, 38, 107, 108, 124, 160, 178, 207, 208, 209, 227, 228, 229 };

                    foreach (var npcid in npcids)
                    {
                        if (CharacterNames.All(c => c.Id != npcid))
                            CharacterNames.Add(GetNewNpc(npcid));
                    }
                });
        }

        private void FixSunflowers()
        {
            for (int i = 5; i < TilesWide - 5; ++i)
            {
                for (int j = 5; (double)j < GroundLevel; ++j)
                {
                    if (Tiles[i, j].IsActive && (int)Tiles[i, j].Type == 27)
                    {
                        int u = (int)Tiles[i, j].U / 18;
                        int v = j + (int)Tiles[i, j].V / 18 * -1;
                        while (u > 1)
                            u -= 2;
                        int xStart = u * -1 + i;
                        int uStart = this.Rand.Next(3) * 36;
                        int uShift = 0;
                        for (int xx = xStart; xx < xStart + 2; ++xx)
                        {
                            for (int yy = v; yy < v + 4; ++yy)
                                Tiles[xx, yy].U = (short)(uShift + uStart);
                            uShift += 18;
                        }
                    }
                }
            }
        }
    }
}