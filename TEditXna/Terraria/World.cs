using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using BCCL.MvvmLight;
using BCCL.MvvmLight.Threading;
using BCCL.Utility;
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

                                    if (curTile.Color > 0)
                                    {
                                        bw.Write(true);
                                        bw.Write(curTile.Color);
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

                                if ((int)curTile.Liquid > 0)
                                {
                                    bw.Write(true);
                                    bw.Write(curTile.Liquid);
                                    bw.Write(curTile.IsLava);
                                    bw.Write(curTile.IsHoney);
                                }
                                else
                                    bw.Write(false);

                                bw.Write(curTile.HasWire);
                                bw.Write(curTile.HasWire2);
                                bw.Write(curTile.HasWire3);
                                bw.Write(curTile.HalfBrick);
                                bw.Write(curTile.Slope);
                                bw.Write(curTile.Actuator);
                                bw.Write(curTile.InActive);

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

        public void ResetTime()
        {
            DayTime = true;
            Time = 13500.0;
            MoonPhase = 0;
            BloodMoon = false;
        }


        public static List<string> Log = new List<string>();

        public static void DebugLog(string message)
        {
            Log.Add(message);

            if (Log.Count > 100)
            {
                Log.RemoveAt(0);
            }
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

                        w.Rand = new Random(w.WorldId);

                        w.LeftWorld = (float)b.ReadInt32();
                        w.RightWorld = (float)b.ReadInt32();
                        w.TopWorld = (float)b.ReadInt32();
                        w.BottomWorld = (float)b.ReadInt32();
                        w.TilesHigh = b.ReadInt32();
                        w.TilesWide = b.ReadInt32();

                        //if (w.TilesHigh > 10000 || w.TilesWide > 10000 || w.TilesHigh <= 0 || w.TilesWide <= 0)
                        //    throw new FileLoadException(string.Format("Invalid File: {0}", filename));

                        if (w.Version >= 63)
                            w.MoonType = (int)b.ReadByte();
                        else
                            w.MoonType = w.Rand.Next(MaxMoons);


                        if (w.Version >= 44)
                        {
                            w.TreeX[0] = b.ReadInt32();
                            w.TreeX[1] = b.ReadInt32();
                            w.TreeX[2] = b.ReadInt32();
                            w.TreeStyle[0] = b.ReadInt32();
                            w.TreeStyle[1] = b.ReadInt32();
                            w.TreeStyle[2] = b.ReadInt32();
                            w.TreeStyle[3] = b.ReadInt32();
                        }
                        if (w.Version >= 60)
                        {
                            w.CaveBackX[0] = b.ReadInt32();
                            w.CaveBackX[1] = b.ReadInt32();
                            w.CaveBackX[2] = b.ReadInt32();
                            w.CaveBackStyle[0] = b.ReadInt32();
                            w.CaveBackStyle[1] = b.ReadInt32();
                            w.CaveBackStyle[2] = b.ReadInt32();
                            w.CaveBackStyle[3] = b.ReadInt32();
                            w.IceBackStyle = b.ReadInt32();
                            if (w.Version >= 61)
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

                        if (w.Version >= 70)
                        {
                            w.IsEclipse = b.ReadBoolean();
                        }

                        w.DungeonX = b.ReadInt32();
                        w.DungeonY = b.ReadInt32();

                        if (w.Version >= 56)
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

                        if (w.Version >= 66)
                        {
                            w.DownedQueenBee = b.ReadBoolean();
                        }
                        if (w.Version >= 44)
                        {
                            w.DownedMechBoss1 = b.ReadBoolean();
                            w.DownedMechBoss2 = b.ReadBoolean();
                            w.DownedMechBoss3 = b.ReadBoolean();
                            w.DownedMechBossAny = b.ReadBoolean();
                        }
                        if (w.Version >= 64)
                        {
                            w.DownedPlantBoss = b.ReadBoolean();
                            w.DownedGolemBoss = b.ReadBoolean();
                        }
                        if (w.Version >= 29)
                        {
                            w.SavedGoblin = b.ReadBoolean();
                            w.SavedWizard = b.ReadBoolean();
                            if (w.Version >= 34)
                            {
                                w.SavedMech = b.ReadBoolean();
                            }
                            w.DownedGoblins = b.ReadBoolean();
                        }
                        if (w.Version >= 32)
                            w.DownedClown = b.ReadBoolean();
                        if (w.Version >= 37)
                            w.DownedFrost = b.ReadBoolean();
                        if (w.Version >= 56)
                            w.DownedPirates = b.ReadBoolean();


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

                        if (w.Version >= 53)
                        {
                            w.TempRaining = b.ReadBoolean();
                            w.TempRainTime = b.ReadInt32();
                            w.TempMaxRain = b.ReadSingle();
                        }
                        if (w.Version >= 54)
                        {
                            w.OreTier1 = b.ReadInt32();
                            w.OreTier2 = b.ReadInt32();
                            w.OreTier3 = b.ReadInt32();
                        }
                        else if (w.Version < 23 || w.AltarCount != 0)
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

                        if (w.Version >= 55)
                        {
                            w.BgTree = b.ReadByte();
                            w.BgCorruption = b.ReadByte();
                            w.BgJungle = b.ReadByte();
                        }
                        if (w.Version >= 60)
                        {
                            w.BgSnow = b.ReadByte();
                            w.BgHallow = b.ReadByte();
                            w.BgCorruption = b.ReadByte();
                            w.BgDesert = b.ReadByte();
                            w.BgOcean = b.ReadByte();
                        }

                        if (w.Version >= 60)
                        {
                            w.CloudBgActive = (float)b.ReadInt32();
                        }
                        else
                        {
                            w.CloudBgActive = -w.Rand.Next(8640, 86400);
                        }

                        if (w.Version >= 62)
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
                                var tile = new Tile();

                                tile.IsActive = b.ReadBoolean();

                                if (!tile.IsActive)
                                    DebugLog(string.Format("Reading Empty Tile [{0},{1}]", x, y));

                                TileProperty tileProperty = null;
                                if (tile.IsActive)
                                {
                                    tile.Type = b.ReadByte();
                                    tileProperty = TileProperties[tile.Type];

                                    DebugLog(string.Format("Reading Tile {2} [{0},{1}] {3}", x, y, tile.Type, tileProperty.IsFramed ? "Framed" : ""));

                                    if (tile.Type == 127)
                                        tile.IsActive = false;

                                    if (tileProperty.IsFramed)
                                    {
                                        if (w.Version < 28 && tile.Type == 4)
                                        {
                                            // torches didn't have extra in older versions.
                                            tile.U = 0;
                                            tile.V = 0;
                                        }
                                        else if (w.Version < 40 && tile.Type == 19)
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
                                    }
                                    else
                                    {
                                        tile.U = -1;
                                        tile.V = -1;
                                    }

                                    if (w.Version >= 48 && b.ReadBoolean())
                                    {
                                        tile.Color = b.ReadByte();
                                    }
                                }

                                //skip obsolete hasLight
                                if (w.Version <= 25)
                                    b.ReadBoolean();

                                if (b.ReadBoolean())
                                {
                                    tile.Wall = b.ReadByte();
                                    if (w.Version >= 48 && b.ReadBoolean())
                                        tile.WallColor = b.ReadByte();
                                }

                                if (b.ReadBoolean())
                                {
                                    tile.Liquid = b.ReadByte();
                                    tile.IsLava = b.ReadBoolean();
                                    if (w.Version >= 51)
                                    {
                                        tile.IsHoney = b.ReadBoolean();
                                    }
                                }

                                if (w.Version >= 33)
                                {
                                    tile.HasWire = b.ReadBoolean();
                                }
                                if (w.Version >= 43)
                                {
                                    tile.HasWire2 = b.ReadBoolean();
                                    tile.HasWire3 = b.ReadBoolean();
                                }

                                if (w.Version >= 41)
                                {
                                    tile.HalfBrick = b.ReadBoolean();

                                    if (tileProperty == null || !tileProperty.IsSolid)
                                        tile.HalfBrick = false;

                                    if (w.Version >= 49)
                                    {
                                        tile.Slope = b.ReadByte();

                                        if (tileProperty == null || !tileProperty.IsSolid)
                                            tile.Slope = 0;
                                    }
                                }
                                if (w.Version >= 42)
                                {
                                    tile.Actuator = b.ReadBoolean();
                                    tile.InActive = b.ReadBoolean();
                                }


                                // read complete, start compression
                                w.Tiles[x, y] = tile;

                                if (w.Version >= 25)
                                {
                                    int rle = b.ReadInt16();

                                    if (rle < 0)
                                        throw new ApplicationException("Invalid Tile Data!");

                                    DebugLog(string.Format("RLE {0}", rle));
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

                        if (w.Version < 67)
                            w.FixSunflowers();
                        int chestSize = Chest.MaxItems;
                        if (w.Version < 58)
                            chestSize = 20;
                        w.Chests.Clear();
                        OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading Chests..."));
                        for (int i = 0; i < 1000; i++)
                        {
                            if (b.ReadBoolean())
                            {
                                var chest = new Chest(b.ReadInt32(), b.ReadInt32());
                                for (int slot = 0; slot < Chest.MaxItems; slot++)
                                {
                                    if (slot < chestSize)
                                    {

                                        int stackSize = w.Version < 59 ? b.ReadByte() : (int)b.ReadInt16();
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

                                if (w.Tiles[sign.X, sign.Y].IsActive && (int)w.Tiles[sign.X, sign.Y].Type == 55 && (int)w.Tiles[sign.X, sign.Y].Type == 85)
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
                                w.CharacterNames.Add(new NpcName(124, "Nancy"));

                            if (w.Version >= 65)
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

        public void FixNpcs()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                ()=>{
                int[] npcids = {17,18,19,20,22,54,38,107,108,124,160,178,207,208,209,227,228,229};

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