using System.Collections.Generic;
using System.ComponentModel;
using TEdit.Utility;
using TEditXna.Helper;
using TEditXNA.Terraria.Objects;
using TEdit.Geometry.Primitives;
using Vector2 = TEdit.Geometry.Primitives.Vector2;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace TEditXNA.Terraria
{
    public partial class World
    {
        

        private static void SaveV1(World world, BinaryWriter bw)
        {
            bw.Write(CompatibleVersion);
            bw.Write(world.Title);
            bw.Write(world.WorldId);
            bw.Write((int) world.LeftWorld);
            bw.Write((int) world.RightWorld);
            bw.Write((int) world.TopWorld);
            bw.Write((int) world.BottomWorld);
            bw.Write(world.TilesHigh);
            bw.Write(world.TilesWide);

            bw.Write((byte) world.MoonType);
            bw.Write(world.TreeX0);
            bw.Write(world.TreeX1);
            bw.Write(world.TreeX2);
            bw.Write(world.TreeStyle0);
            bw.Write(world.TreeStyle1);
            bw.Write(world.TreeStyle2);
            bw.Write(world.TreeStyle3);
            bw.Write(world.CaveBackX0);
            bw.Write(world.CaveBackX1);
            bw.Write(world.CaveBackX2);
            bw.Write(world.CaveBackStyle0);
            bw.Write(world.CaveBackStyle1);
            bw.Write(world.CaveBackStyle2);
            bw.Write(world.CaveBackStyle3);
            bw.Write(world.IceBackStyle);
            bw.Write(world.JungleBackStyle);
            bw.Write(world.HellBackStyle);

            bw.Write(world.SpawnX);
            bw.Write(world.SpawnY);
            bw.Write(world.GroundLevel);
            bw.Write(world.RockLevel);
            bw.Write(world.Time);
            bw.Write(world.DayTime);
            bw.Write(world.MoonPhase);
            bw.Write(world.BloodMoon);
            bw.Write(world.IsEclipse);
            bw.Write(world.DungeonX);
            bw.Write(world.DungeonY);

            bw.Write(world.IsCrimson);

            bw.Write(world.DownedBoss1);
            bw.Write(world.DownedBoss2);
            bw.Write(world.DownedBoss3);
            bw.Write(world.DownedQueenBee);
            bw.Write(world.DownedMechBoss1);
            bw.Write(world.DownedMechBoss2);
            bw.Write(world.DownedMechBoss3);
            bw.Write(world.DownedMechBossAny);
            bw.Write(world.DownedPlantBoss);
            bw.Write(world.DownedGolemBoss);
            bw.Write(world.SavedGoblin);
            bw.Write(world.SavedWizard);
            bw.Write(world.SavedMech);
            bw.Write(world.DownedGoblins);
            bw.Write(world.DownedClown);
            bw.Write(world.DownedFrost);
            bw.Write(world.DownedPirates);

            bw.Write(world.ShadowOrbSmashed);
            bw.Write(world.SpawnMeteor);
            bw.Write((byte) world.ShadowOrbCount);
            bw.Write(world.AltarCount);
            bw.Write(world.HardMode);
            bw.Write(world.InvasionDelay);
            bw.Write(world.InvasionSize);
            bw.Write(world.InvasionType);
            bw.Write(world.InvasionX);

            bw.Write(world.TempRaining);
            bw.Write(world.TempRainTime);
            bw.Write(world.TempMaxRain);
            bw.Write(world.OreTier1);
            bw.Write(world.OreTier2);
            bw.Write(world.OreTier3);
            bw.Write(world.BgTree);
            bw.Write(world.BgCorruption);
            bw.Write(world.BgJungle);
            bw.Write(world.BgSnow);
            bw.Write(world.BgHallow);
            bw.Write(world.BgCrimson);
            bw.Write(world.BgDesert);
            bw.Write(world.BgOcean);
            bw.Write((int) world.CloudBgActive);
            bw.Write(world.NumClouds);
            bw.Write(world.WindSpeedSet);


            for (int x = 0; x < world.TilesWide; ++x)
            {
                OnProgressChanged(world,
                    new ProgressChangedEventArgs(x.ProgressPercentage(world.TilesWide), "Saving UndoTiles..."));

                int rle = 0;
                for (int y = 0; y < world.TilesHigh; y = y + rle + 1)
                {
                    Tile curTile = world.Tiles[x, y];
                    WriteTileDataToStreamV1(curTile, bw);

                    int rleTemp = 1;
                    while (y + rleTemp < world.TilesHigh && curTile.Equals(world.Tiles[x, (y + rleTemp)]))
                        ++rleTemp;
                    rle = rleTemp - 1;
                    bw.Write((short) rle);
                }
            }
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Saving Chests..."));
            WriteChestDataToStreamV1(world.Chests, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Saving Signs..."));
            WriteSignDataToStreamV1(world.Signs, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Saving NPC Data..."));
            foreach (NPC curNpc in world.NPCs)
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

            world.FixNpcs();

            bw.Write(world.GetNpc(17).Name);
            bw.Write(world.GetNpc(18).Name);
            bw.Write(world.GetNpc(19).Name);
            bw.Write(world.GetNpc(20).Name);
            bw.Write(world.GetNpc(22).Name);
            bw.Write(world.GetNpc(54).Name);
            bw.Write(world.GetNpc(38).Name);
            bw.Write(world.GetNpc(107).Name);
            bw.Write(world.GetNpc(108).Name);
            bw.Write(world.GetNpc(124).Name);
            bw.Write(world.GetNpc(160).Name);
            bw.Write(world.GetNpc(178).Name);
            bw.Write(world.GetNpc(207).Name);
            bw.Write(world.GetNpc(208).Name);
            bw.Write(world.GetNpc(209).Name);
            bw.Write(world.GetNpc(227).Name);
            bw.Write(world.GetNpc(228).Name);
            bw.Write(world.GetNpc(229).Name);

            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Saving Validation Data..."));
            bw.Write(true);
            bw.Write(world.Title);
            bw.Write(world.WorldId);
        }

        public static void WriteSignDataToStreamV1(IList<Sign> signs, BinaryWriter bw)
        {
            for (int i = 0; i < 1000; ++i)
            {
                if (i >= signs.Count || string.IsNullOrWhiteSpace(signs[i].Text))
                {
                    bw.Write(false);
                }
                else
                {
                    Sign curSign = signs[i];
                    bw.Write(true);
                    bw.Write(curSign.Text);
                    bw.Write(curSign.X);
                    bw.Write(curSign.Y);
                }
            }
        }

        public static void WriteChestDataToStreamV1(IList<Chest> chests, BinaryWriter bw)
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

        public static void WriteTileDataToStreamV1(Tile curTile, BinaryWriter bw)
        {
            if (curTile.Type == (int)TileType.IceByRod)
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
            if (curTile.Wall > 0)
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

            if (curTile.LiquidAmount > 0)
            {
                bw.Write(true);
                bw.Write(curTile.LiquidAmount);
                bw.Write(curTile.LiquidType == LiquidType.Lava);
                bw.Write(curTile.LiquidType == LiquidType.Honey);
            }
            else
                bw.Write(false);

            bw.Write(curTile.WireRed);
            bw.Write(curTile.WireGreen);
            bw.Write(curTile.WireBlue);
            bw.Write(curTile.BrickStyle != 0);
            bw.Write((byte)curTile.BrickStyle);
            bw.Write(curTile.Actuator);
            bw.Write(curTile.InActive);
        }

        private static void LoadV1(BinaryReader reader, string filename, World w)
        {
            uint version = w.Version;
            w.Title = reader.ReadString();
            w.WorldId = reader.ReadInt32();

            w.Rand = new Random(w.WorldId);

            w.LeftWorld = reader.ReadInt32();
            w.RightWorld = reader.ReadInt32();
            w.TopWorld = reader.ReadInt32();
            w.BottomWorld = reader.ReadInt32();
            w.TilesHigh = reader.ReadInt32();
            w.TilesWide = reader.ReadInt32();

            //if (w.TilesHigh > 10000 || w.TilesWide > 10000 || w.TilesHigh <= 0 || w.TilesWide <= 0)
            //    throw new FileLoadException(string.Format("Invalid File: {0}", filename));


            if (version >= 63)
                w.MoonType = reader.ReadByte();
            else
                w.MoonType = (byte)w.Rand.Next(MaxMoons);


            if (version >= 44)
            {
                w.TreeX0 = reader.ReadInt32();
                w.TreeX1 = reader.ReadInt32();
                w.TreeX2 = reader.ReadInt32();
                w.TreeStyle0 = reader.ReadInt32();
                w.TreeStyle1 = reader.ReadInt32();
                w.TreeStyle2 = reader.ReadInt32();
                w.TreeStyle3 = reader.ReadInt32();
            }
            if (version >= 60)
            {
                w.CaveBackX0 = reader.ReadInt32();
                w.CaveBackX1 = reader.ReadInt32();
                w.CaveBackX2 = reader.ReadInt32();
                w.CaveBackStyle0 = reader.ReadInt32();
                w.CaveBackStyle1 = reader.ReadInt32();
                w.CaveBackStyle2 = reader.ReadInt32();
                w.CaveBackStyle3 = reader.ReadInt32();
                w.IceBackStyle = reader.ReadInt32();
                if (version >= 61)
                {
                    w.JungleBackStyle = reader.ReadInt32();
                    w.HellBackStyle = reader.ReadInt32();
                }
            }
            else
            {
                w.CaveBackX[0] = w.TilesWide/2;
                w.CaveBackX[1] = w.TilesWide;
                w.CaveBackX[2] = w.TilesWide;
                w.CaveBackStyle0 = 0;
                w.CaveBackStyle1 = 1;
                w.CaveBackStyle2 = 2;
                w.CaveBackStyle3 = 3;
                w.IceBackStyle = 0;
                w.JungleBackStyle = 0;
                w.HellBackStyle = 0;
            }

            w.SpawnX = reader.ReadInt32();
            w.SpawnY = reader.ReadInt32();
            w.GroundLevel = (int) reader.ReadDouble();
            w.RockLevel = (int) reader.ReadDouble();

            // read world flags
            w.Time = reader.ReadDouble();
            w.DayTime = reader.ReadBoolean();
            w.MoonPhase = reader.ReadInt32();
            w.BloodMoon = reader.ReadBoolean();

            if (version >= 70)
            {
                w.IsEclipse = reader.ReadBoolean();
            }

            w.DungeonX = reader.ReadInt32();
            w.DungeonY = reader.ReadInt32();

            if (version >= 56)
            {
                w.IsCrimson = reader.ReadBoolean();
            }
            else
            {
                w.IsCrimson = false;
            }

            w.DownedBoss1 = reader.ReadBoolean();
            w.DownedBoss2 = reader.ReadBoolean();
            w.DownedBoss3 = reader.ReadBoolean();

            if (version >= 66)
            {
                w.DownedQueenBee = reader.ReadBoolean();
            }
            if (version >= 44)
            {
                w.DownedMechBoss1 = reader.ReadBoolean();
                w.DownedMechBoss2 = reader.ReadBoolean();
                w.DownedMechBoss3 = reader.ReadBoolean();
                w.DownedMechBossAny = reader.ReadBoolean();
            }
            if (version >= 64)
            {
                w.DownedPlantBoss = reader.ReadBoolean();
                w.DownedGolemBoss = reader.ReadBoolean();
            }
            if (version >= 29)
            {
                w.SavedGoblin = reader.ReadBoolean();
                w.SavedWizard = reader.ReadBoolean();
                if (version >= 34)
                {
                    w.SavedMech = reader.ReadBoolean();
                }
                w.DownedGoblins = reader.ReadBoolean();
            }
            if (version >= 32)
                w.DownedClown = reader.ReadBoolean();
            if (version >= 37)
                w.DownedFrost = reader.ReadBoolean();
            if (version >= 56)
                w.DownedPirates = reader.ReadBoolean();


            w.ShadowOrbSmashed = reader.ReadBoolean();
            w.SpawnMeteor = reader.ReadBoolean();
            w.ShadowOrbCount = reader.ReadByte();

            if (version >= 23)
            {
                w.AltarCount = reader.ReadInt32();
                w.HardMode = reader.ReadBoolean();
            }

            w.InvasionDelay = reader.ReadInt32();
            w.InvasionSize = reader.ReadInt32();
            w.InvasionType = reader.ReadInt32();
            w.InvasionX = reader.ReadDouble();

            if (version >= 53)
            {
                w.TempRaining = reader.ReadBoolean();
                w.TempRainTime = reader.ReadInt32();
                w.TempMaxRain = reader.ReadSingle();
            }
            if (version >= 54)
            {
                w.OreTier1 = reader.ReadInt32();
                w.OreTier2 = reader.ReadInt32();
                w.OreTier3 = reader.ReadInt32();
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
                w.BgTree = reader.ReadByte();
                w.BgCorruption = reader.ReadByte();
                w.BgJungle = reader.ReadByte();
            }
            if (version >= 60)
            {
                w.BgSnow = reader.ReadByte();
                w.BgHallow = reader.ReadByte();
                w.BgCorruption = reader.ReadByte();
                w.BgDesert = reader.ReadByte();
                w.BgOcean = reader.ReadByte();
            }

            if (version >= 60)
            {
                w.CloudBgActive = reader.ReadInt32();
            }
            else
            {
                w.CloudBgActive = -w.Rand.Next(8640, 86400);
            }

            if (version >= 62)
            {
                w.NumClouds = reader.ReadInt16();
                w.WindSpeedSet = reader.ReadSingle();
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
                OnProgressChanged(null,
                    new ProgressChangedEventArgs(x.ProgressPercentage(w.TilesWide), "Loading UndoTiles..."));

                for (int y = 0; y < w.TilesHigh; y++)
                {
                    Tile tile = ReadTileDataFromStreamV1(reader, version);

                    // read complete, start compression
                    w.Tiles[x, y] = tile;

                    if (version >= 25)
                    {
                        int rle = reader.ReadInt16();

                        if (rle < 0)
                            throw new ApplicationException("Invalid Tile Data!");

                        if (rle > 0)
                        {
                            for (int k = y + 1; k < y + rle + 1; k++)
                            {
                                var tcopy = (Tile) tile.Clone();
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
            ((ObservableCollection<Chest>)w.Chests).AddRange(ReadChestDataFromStreamV1(reader, version));

            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading Signs..."));
            w.Signs.Clear();

            foreach (Sign sign in ReadSignDataFromStreamV1(reader))
            {
                if (w.Tiles[sign.X, sign.Y].IsActive && Tile.IsSign(w.Tiles[sign.X, sign.Y].Type))
                {
                    w.Signs.Add(sign);
                }
            }

            w.NPCs.Clear();
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading NPC Data..."));
            while (reader.ReadBoolean())
            {
                var npc = new NPC();
                npc.Name = reader.ReadString();
                npc.Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                npc.IsHomeless = reader.ReadBoolean();
                npc.Home = new Vector2Int32(reader.ReadInt32(), reader.ReadInt32());
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
                w.CharacterNames.Add(new NpcName(17, reader.ReadString()));
                w.CharacterNames.Add(new NpcName(18, reader.ReadString()));
                w.CharacterNames.Add(new NpcName(19, reader.ReadString()));
                w.CharacterNames.Add(new NpcName(20, reader.ReadString()));
                w.CharacterNames.Add(new NpcName(22, reader.ReadString()));
                w.CharacterNames.Add(new NpcName(54, reader.ReadString()));
                w.CharacterNames.Add(new NpcName(38, reader.ReadString()));
                w.CharacterNames.Add(new NpcName(107, reader.ReadString()));
                w.CharacterNames.Add(new NpcName(108, reader.ReadString()));
                if (version >= 35)
                    w.CharacterNames.Add(new NpcName(124, reader.ReadString()));
                else
                    w.CharacterNames.Add(new NpcName(124, "Nancy"));

                if (version >= 65)
                {
                    w.CharacterNames.Add(new NpcName(160, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(178, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(207, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(208, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(209, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(227, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(228, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(229, reader.ReadString()));
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
                bool validation = reader.ReadBoolean();
                string checkTitle = reader.ReadString();
                int checkVersion = reader.ReadInt32();
                if (validation && checkTitle == w.Title && checkVersion == w.WorldId)
                {
                    //w.loadSuccess = true;
                }
                else
                {
                    reader.Close();
                    throw new FileLoadException(
                        $"Error reading world file validation parameters! {filename}");
                }
            }
            OnProgressChanged(null, new ProgressChangedEventArgs(0, "World Load Complete."));
        }

        public static IEnumerable<Sign> ReadSignDataFromStreamV1(BinaryReader b)
        {
            for (int i = 0; i < 1000; i++)
            {
                if (b.ReadBoolean())
                {
                    var sign = new Sign();
                    sign.Text = b.ReadString();
                    sign.X = b.ReadInt32();
                    sign.Y = b.ReadInt32();

                    yield return sign;
                }
            }
        }

        public static IEnumerable<Chest> ReadChestDataFromStreamV1(BinaryReader b, uint version)
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
                            int stackSize = version < 59 ? b.ReadByte() : b.ReadInt16();
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

        public static Tile ReadTileDataFromStreamV1(BinaryReader b, uint version)
        {
            var tile = new Tile();

            tile.IsActive = b.ReadBoolean();

            TileProperty tileProperty = null;

            if (tile.IsActive)
            {
                tile.Type = b.ReadByte();
                tileProperty = TileProperties[tile.Type];


                if (tile.Type == (int)TileType.IceByRod)
                    tile.IsActive = false;

                if (version < 72 &&
                    (tile.Type == 35 || tile.Type == 36 || tile.Type == 170 || tile.Type == 171 || tile.Type == 172))
                {
                    tile.U = b.ReadInt16();
                    tile.V = b.ReadInt16();
                }
                else if (!tileProperty.IsFramed)
                {
                    tile.U = -1;
                    tile.V = -1;
                }
                else if (version < 28 && tile.Type == (int)(TileType.Torch))
                {
                    // torches didn't have extra in older versions.
                    tile.U = 0;
                    tile.V = 0;
                }
                else if (version < 40 && tile.Type == (int)TileType.Platform)
                {
                    tile.U = 0;
                    tile.V = 0;
                }
                else
                {
                    tile.U = b.ReadInt16();
                    tile.V = b.ReadInt16();

                    if (tile.Type == (int)TileType.Timer)
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
    }
}