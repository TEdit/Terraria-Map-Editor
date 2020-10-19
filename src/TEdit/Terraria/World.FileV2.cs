using System.Collections.Generic;
using System.ComponentModel;
using TEdit.Utility;
using TEdit.Geometry.Primitives;
using Vector2 = TEdit.Geometry.Primitives.Vector2;
using System;
using System.IO;
using TEdit.Terraria;
using TEdit.Helper;

namespace TEdit.Terraria
{

    public partial class World
    {
        public const uint CompatibleVersion = 233;
        public const short SectionCount = 11;
        public const short TileCount = 623;
        public const short WallCount = 316;

        public const short KillTallyMax = 663;

        public const int MaxChests = 8000;
        public const int MaxSigns = 1000;

        public static bool[] TileFrameImportant;

        public static void ImportKillsAndBestiary(World world, string worldFileName)
        {
            World w = new World();

            // load npc and bestiary data to temp world
            using (var b = new BinaryReader(File.OpenRead(worldFileName)))
            {
                bool[] tileFrameImportant;
                int[] sectionPointers;

                w.Version = b.ReadUInt32();
                var curVersion = w.Version;


                if (w.Version < 87)
                    throw new FileFormatException("World file too old, please update it by loading in game.");
                if (w.Version > world.Version)
                    throw new FileFormatException("Source world version is greater than target world. Please reload both in game and resave");

                // reset the stream
                b.BaseStream.Position = (long)0;

                OnProgressChanged(null, new ProgressChangedEventArgs(0, "Loading File Header..."));
                // read section pointers and tile frame data
                if (!LoadSectionHeader(b, out tileFrameImportant, out sectionPointers, w))
                    throw new FileFormatException("Invalid File Format Section");

                TileFrameImportant = tileFrameImportant;

                // we should be at the end of the first section
                if (b.BaseStream.Position != sectionPointers[0])
                    throw new FileFormatException("Unexpected Position: Invalid File Format Section");

                // Load the flags
                LoadHeaderFlags(b, w, sectionPointers[1]);
                if (b.BaseStream.Position != sectionPointers[1])
                    throw new FileFormatException("Unexpected Position: Invalid Header Flags");

                if (w.Version >= 210 && sectionPointers.Length > 9)
                {
                    // skip to bestiary data
                    b.BaseStream.Position = sectionPointers[8];
                    LoadBestiary(b, w);
                    if (b.BaseStream.Position != sectionPointers[9])
                        throw new FileFormatException("Unexpected Position: Invalid Bestiary Section");
                }
            }

            // copy kill tally and bestiary to target world
            world.Bestiary = w.Bestiary;
            world.KilledMobs.Clear();
            world.KilledMobs.AddRange(w.KilledMobs);
        }

        private static void SaveV2(World world, BinaryWriter bw)
        {
            world.Validate();

            // initialize tileframeimportance array if empty
            if (TileFrameImportant == null || TileFrameImportant.Length < TileCount)
            {
                TileFrameImportant = new bool[TileCount];
                for (int i = 0; i < TileCount; i++)
                {
                    if (TileProperties.Count > i)
                    {
                        TileFrameImportant[i] = TileProperties[i].IsFramed;
                    }
                }
            }

            int[] sectionPointers = new int[SectionCount];

            OnProgressChanged(null, new ProgressChangedEventArgs(0, "Save headers..."));
            sectionPointers[0] = SaveSectionHeader(world, bw);
            sectionPointers[1] = SaveHeaderFlags(world, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(0, "Save Tiles..."));
            sectionPointers[2] = SaveTiles(world.Tiles, world.TilesWide, world.TilesHigh, bw);

            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Chests..."));
            sectionPointers[3] = SaveChests(world.Chests, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Signs..."));
            sectionPointers[4] = SaveSigns(world.Signs, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save NPCs..."));
            sectionPointers[5] = SaveNPCs(world.NPCs, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Mobs..."));
            sectionPointers[5] = SaveMobs(world.Mobs, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Tile Entities Section..."));
            sectionPointers[6] = SaveTileEntities(world.TileEntities, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Weighted Pressure Plates..."));
            sectionPointers[7] = SavePressurePlate(world.PressurePlates, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Town Manager..."));
            sectionPointers[8] = SaveTownManager(world.PlayerRooms, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Bestiary..."));
            sectionPointers[9] = SaveBestiary(world.Bestiary, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Creative Powers..."));
            sectionPointers[10] = SaveCreativePowers(world.CreativePowers, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Footers..."));
            SaveFooter(world, bw);
            UpdateSectionPointers(sectionPointers, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Complete."));
        }

        public static int SaveTiles(Tile[,] tiles, int maxX, int maxY, BinaryWriter bw)
        {
            for (int x = 0; x < maxX; x++)
            {
                OnProgressChanged(null, new ProgressChangedEventArgs(x.ProgressPercentage(maxX), "Saving Tiles..."));


                for (int y = 0; y < maxY; y++)
                {
                    Tile tile = tiles[x, y];

                    int dataIndex;
                    int headerIndex;

                    byte[] tileData = SerializeTileData(tile, out dataIndex, out headerIndex);

                    // rle compression
                    byte header1 = tileData[headerIndex];

                    short rle = 0;
                    int nextY = y + 1;
                    int remainingY = maxY - y - 1;
                    while (remainingY > 0 && tile.Equals(tiles[x, nextY]))
                    {
                        rle = (short)(rle + 1);
                        remainingY--;
                        nextY++;
                    }

                    y = y + rle;

                    if (rle > 0)
                    {
                        tileData[dataIndex++] = (byte)(rle & 255);

                        if (rle <= 255)
                        {
                            // set bit[6] of header1 for byte size rle
                            header1 = (byte)(header1 | 64);
                        }
                        else
                        {
                            // set bit[7] of header1 for int16 size rle
                            header1 = (byte)(header1 | 128);

                            // grab the upper half of the int16 and stick it in tiledata
                            tileData[dataIndex++] = (byte)((rle & 65280) >> 8);
                        }
                    }

                    tileData[headerIndex] = header1;
                    // end rle compression

                    bw.Write(tileData, headerIndex, dataIndex - headerIndex);
                }
            }


            return (int)bw.BaseStream.Position;
        }

        /// <summary>
        /// BitPack tile data and headers
        /// </summary>
        public static byte[] SerializeTileData(Tile tile, out int dataIndex, out int headerIndex)
        {

            byte[] tileData = new byte[15];
            dataIndex = 3;

            byte header3 = (byte)0;
            byte header2 = (byte)0;
            byte header1 = (byte)0;

            // tile data
            if (tile.IsActive)
            {
                // activate bit[1]
                header1 = (byte)(header1 | 2);

                if (tile.Type == (int)TileType.IceByRod && tile.IsActive)
                {
                    tile.IsActive = false;
                }

                // save tile type as byte or int16
                tileData[dataIndex++] = (byte)tile.Type;
                if (tile.Type > 255)
                {
                    // write high byte
                    tileData[dataIndex++] = (byte)(tile.Type >> 8);

                    // set header1 bit[5] for int16 tile type
                    header1 = (byte)(header1 | 32);
                }

                if (TileFrameImportant[tile.Type])
                {
                    // pack UV coords
                    tileData[dataIndex++] = (byte)(tile.U & 255);
                    tileData[dataIndex++] = (byte)((tile.U & 65280) >> 8);
                    tileData[dataIndex++] = (byte)(tile.V & 255);
                    tileData[dataIndex++] = (byte)((tile.V & 65280) >> 8);
                }

                if (tile.TileColor != 0)
                {
                    // set header3 bit[3] for tile color active
                    header3 = (byte)(header3 | 8);
                    tileData[dataIndex++] = tile.TileColor;
                }
            }

            // wall data
            if (tile.Wall != 0)
            {
                // set header1 bit[2] for wall active
                header1 = (byte)(header1 | 4);
                tileData[dataIndex++] = (byte)tile.Wall;

                // save tile wall color
                if (tile.WallColor != 0)
                {
                    // set header3 bit[4] for wall color active
                    header3 = (byte)(header3 | 16);
                    tileData[dataIndex++] = tile.WallColor;
                }
            }

            // liquid data
            if (tile.LiquidAmount != 0 && tile.LiquidType != LiquidType.None)
            {
                // set bits[3,4] using left shift
                header1 = (byte)(header1 | (byte)((byte)tile.LiquidType << 3));
                tileData[dataIndex++] = tile.LiquidAmount;
            }

            // wire data
            if (tile.WireRed)
            {
                // red wire = header2 bit[1]
                header2 = (byte)(header2 | 2);
            }
            if (tile.WireBlue)
            {
                // blue wire = header2 bit[2]
                header2 = (byte)(header2 | 4);
            }
            if (tile.WireGreen)
            {
                // green wire = header2 bit[3]
                header2 = (byte)(header2 | 8);
            }

            // brick style
            byte brickStyle = (byte)((byte)tile.BrickStyle << 4);
            // set bits[4,5,6] of header2
            header2 = (byte)(header2 | brickStyle);

            // actuator data
            if (tile.Actuator)
            {
                // set bit[1] of header3
                header3 = (byte)(header3 | 2);
            }
            if (tile.InActive)
            {
                // set bit[2] of header3
                header3 = (byte)(header3 | 4);
            }
            if (tile.WireYellow)
            {
                header3 = (byte)(header3 | 32);
            }
            if (tile.Wall > 255)
            {
                tileData[dataIndex++] = (byte)(tile.Wall >> 8);
                header3 = (byte)(header3 | 64);
            }


            headerIndex = 2;
            if (header3 != 0)
            {
                // set header3 active flag bit[0] of header2
                header2 = (byte)(header2 | 1);
                tileData[headerIndex--] = header3;
            }
            if (header2 != 0)
            {
                // set header2 active flag bit[0] of header1
                header1 = (byte)(header1 | 1);
                tileData[headerIndex--] = header2;
            }

            tileData[headerIndex] = header1;
            return tileData;
        }

        public static int SaveChests(IList<Chest> chests, BinaryWriter bw)
        {
            bw.Write((Int16)chests.Count);
            bw.Write((Int16)Chest.MaxItems);

            foreach (Chest chest in chests)
            {
                bw.Write(chest.X);
                bw.Write(chest.Y);
                bw.Write(chest.Name ?? string.Empty);

                for (int slot = 0; slot < Chest.MaxItems; slot++)
                {
                    Item item = chest.Items[slot];
                    if (item != null)
                    {
                        bw.Write((short)item.StackSize);
                        if (item.StackSize > 0)
                        {
                            bw.Write(item.NetId);
                            bw.Write(item.Prefix);
                        }
                    }
                    else
                    {
                        bw.Write((short)0);
                    }
                }
            }

            return (int)bw.BaseStream.Position;
        }

        public static int SaveSigns(IList<Sign> signs, BinaryWriter bw)
        {
            bw.Write((Int16)signs.Count);

            foreach (Sign sign in signs)
            {
                if (sign.Text != null)
                {
                    bw.Write(sign.Text);
                    bw.Write(sign.X);
                    bw.Write(sign.Y);
                }
            }

            return (int)bw.BaseStream.Position;
        }

        public static int SaveNPCs(IEnumerable<NPC> npcs, BinaryWriter bw)
        {
            foreach (NPC npc in npcs)
            {
                bw.Write(true);
                bw.Write(npc.SpriteId);
                bw.Write(npc.DisplayName);
                bw.Write(npc.Position.X);
                bw.Write(npc.Position.Y);
                bw.Write(npc.IsHomeless);
                bw.Write(npc.Home.X);
                bw.Write(npc.Home.Y);

                BitsByte bitsByte = 0;
                bitsByte[0] = true;
                bw.Write(bitsByte);
                if (bitsByte[0])
                {
                    bw.Write(npc.TownNpcVariationIndex);
                }

            }
            bw.Write(false);

            return (int)bw.BaseStream.Position;
        }

        public static int SaveTownManager(IList<TownManager> rooms, BinaryWriter bw)
        {
            bw.Write(rooms.Count);
            foreach (TownManager room in rooms)
            {
                bw.Write(room.NpcId);
                bw.Write(room.Home.X);
                bw.Write(room.Home.Y);
            }
            return (int)bw.BaseStream.Position;
        }

        public static int SaveMobs(IEnumerable<NPC> mobs, BinaryWriter bw)
        {
            foreach (NPC mob in mobs)
            {
                bw.Write(true);
                bw.Write(mob.SpriteId);
                bw.Write(mob.Position.X);
                bw.Write(mob.Position.Y);
            }
            bw.Write(false);

            return (int)bw.BaseStream.Position;
        }

        public static int SavePressurePlate(IList<PressurePlate> plates, BinaryWriter bw)
        {
            bw.Write(plates.Count);

            foreach (PressurePlate plate in plates)
            {
                bw.Write(plate.PosX);
                bw.Write(plate.PosY);
            }

            return (int)bw.BaseStream.Position;
        }

        public static int SaveBestiary(Bestiary bestiary, BinaryWriter bw)
        {
            bestiary.Save(bw);
            return (int)bw.BaseStream.Position;
        }

        public static int SaveCreativePowers(CreativePowers powers, BinaryWriter bw)
        {
            powers.Save(bw);
            return (int)bw.BaseStream.Position;
        }

        public static int SaveFooter(World world, BinaryWriter bw)
        {
            bw.Write(true);
            bw.Write(world.Title);
            bw.Write(world.WorldId);

            return (int)bw.BaseStream.Position;
        }

        public static int UpdateSectionPointers(int[] sectionPointers, BinaryWriter bw)
        {
            bw.BaseStream.Position = 0x18L;
            bw.Write((short)sectionPointers.Length);

            for (int i = 0; i < sectionPointers.Length; i++)
            {
                bw.Write(sectionPointers[i]);
            }

            return (int)bw.BaseStream.Position;
        }

        public static int SaveSectionHeader(World world, BinaryWriter bw)
        {
            bw.Write(Math.Max(CompatibleVersion, world.Version));
            bw.Write((UInt64)0x026369676f6c6572ul);
            bw.Write((int)world.FileRevision + 1);
            bw.Write(Convert.ToUInt64(world.IsFavorite));
            bw.Write(SectionCount);

            // write section pointer placeholders
            for (int i = 0; i < SectionCount; i++)
            {
                bw.Write(0);
            }

            // write bitpacked tile frame importance
            WriteBitArray(bw, TileFrameImportant);

            return (int)bw.BaseStream.Position;
        }

        public static int SaveHeaderFlags(World world, BinaryWriter bw)
        {
            bw.Write(world.Title);
            bw.Write(world.Seed);
            bw.Write(world.WorldGenVersion);
            bw.Write(world.Guid.ToByteArray());
            bw.Write(world.WorldId);
            bw.Write((int)world.LeftWorld);
            bw.Write((int)world.RightWorld);
            bw.Write((int)world.TopWorld);
            bw.Write((int)world.BottomWorld);
            bw.Write(world.TilesHigh);
            bw.Write(world.TilesWide);
            bw.Write(world.GameMode);
            bw.Write(world.DrunkWorld);
            bw.Write(world.GooWorld);
            bw.Write(world.CreationTime);
            bw.Write((byte)world.MoonType);
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
            bw.Write(world.DownedSlimeKingBoss);
            bw.Write(world.SavedGoblin);
            bw.Write(world.SavedWizard);
            bw.Write(world.SavedMech);
            bw.Write(world.DownedGoblins);
            bw.Write(world.DownedClown);
            bw.Write(world.DownedFrost);
            bw.Write(world.DownedPirates);
            bw.Write(world.ShadowOrbSmashed);
            bw.Write(world.SpawnMeteor);
            bw.Write((byte)world.ShadowOrbCount);
            bw.Write(world.AltarCount);
            bw.Write(world.HardMode);
            bw.Write(world.InvasionDelay);
            bw.Write(world.InvasionSize);
            bw.Write(world.InvasionType);
            bw.Write(world.InvasionX);
            bw.Write(world.SlimeRainTime);
            bw.Write((byte)world.SundialCooldown);
            bw.Write(world.TempRaining);
            bw.Write(world.TempRainTime);
            bw.Write(world.TempMaxRain);
            bw.Write(world.SavedOreTiersCobalt);
            bw.Write(world.SavedOreTiersMythril);
            bw.Write(world.SavedOreTiersAdamantite);
            bw.Write(world.BgTree);
            bw.Write(world.BgCorruption);
            bw.Write(world.BgJungle);
            bw.Write(world.BgSnow);
            bw.Write(world.BgHallow);
            bw.Write(world.BgCrimson);
            bw.Write(world.BgDesert);
            bw.Write(world.BgOcean);
            bw.Write((int)world.CloudBgActive);
            bw.Write(world.NumClouds);
            bw.Write(world.WindSpeedSet);
            bw.Write(world.Anglers.Count);
            foreach (string angler in world.Anglers)
            {
                bw.Write(angler);
            }
            bw.Write(world.SavedAngler);
            bw.Write(world.AnglerQuest);
            bw.Write(world.SavedStylist);
            bw.Write(world.SavedTaxCollector);
            bw.Write(world.SavedGolfer);
            bw.Write(world.InvasionSizeStart);
            bw.Write(world.CultistDelay);
            bw.Write((short)KillTallyMax);
            for (int i = 0; i < KillTallyMax; i++)
            {
                if (world.KilledMobs.Count > i)
                {
                    bw.Write(world.KilledMobs[i]);
                }
                else
                {
                    bw.Write(0);
                }
            }

            bw.Write(world.FastForwardTime);
            bw.Write(world.DownedFishron);
            bw.Write(world.DownedMartians);
            bw.Write(world.DownedLunaticCultist);
            bw.Write(world.DownedMoonlord);
            bw.Write(world.DownedHalloweenKing);
            bw.Write(world.DownedHalloweenTree);
            bw.Write(world.DownedChristmasQueen);
            bw.Write(world.DownedSanta);
            bw.Write(world.DownedChristmasTree);
            bw.Write(world.DownedCelestialSolar);
            bw.Write(world.DownedCelestialVortex);
            bw.Write(world.DownedCelestialNebula);
            bw.Write(world.DownedCelestialStardust);
            bw.Write(world.CelestialSolarActive);
            bw.Write(world.CelestialVortexActive);
            bw.Write(world.CelestialNebulaActive);
            bw.Write(world.CelestialStardustActive);
            bw.Write(world.Apocalypse);
            bw.Write(world.PartyManual);
            bw.Write(world.PartyGenuine);
            bw.Write(world.PartyCooldown);
            bw.Write(world.PartyingNPCs.Count);
            foreach (int partier in world.PartyingNPCs)
            {
                bw.Write(partier);
            }

            bw.Write(world.SandStormHappening);
            bw.Write(world.SandStormTimeLeft);
            bw.Write(world.SandStormSeverity);
            bw.Write(world.SandStormIntendedSeverity);
            bw.Write(world.SavedBartender);
            bw.Write(world.DownedDD2InvasionT1);
            bw.Write(world.DownedDD2InvasionT2);
            bw.Write(world.DownedDD2InvasionT3);

            // 1.4 Journey's End
            bw.Write((byte)world.MushroomBg);
            bw.Write((byte)world.UnderworldBg);
            bw.Write((byte)world.BgTree2);
            bw.Write((byte)world.BgTree3);
            bw.Write((byte)world.BgTree4);
            bw.Write(world.CombatBookUsed);
            bw.Write(world.TempLanternNightCooldown);
            bw.Write(world.TempLanternNightGenuine);
            bw.Write(world.TempLanternNightManual);
            bw.Write(world.TempLanternNightNextNightIsGenuine);
            // tree tops
            bw.Write(world.TreeTopVariations.Count);
            for (int i = 0; i < world.TreeTopVariations.Count; i++)
            {
                bw.Write(world.TreeTopVariations[i]);
            }
            bw.Write(world.ForceHalloweenForToday);
            bw.Write(world.ForceXMasForToday);

            bw.Write(world.SavedOreTiersCopper);
            bw.Write(world.SavedOreTiersIron);
            bw.Write(world.SavedOreTiersSilver);
            bw.Write(world.SavedOreTiersGold);

            bw.Write(world.BoughtCat);
            bw.Write(world.BoughtDog);
            bw.Write(world.BoughtBunny);

            bw.Write(world.DownedEmpressOfLight);
            bw.Write(world.DownedQueenSlime);


            // unknown flags from data file
            if (world.UnknownData != null && world.UnknownData.Length > 0)
                bw.Write(world.UnknownData);

            return (int)bw.BaseStream.Position;
        }

        public static int SaveTileEntities(IList<TileEntity> tileEntities, BinaryWriter bw)
        {
            bw.Write(tileEntities.Count);

            foreach (TileEntity tentity in tileEntities)
            {
                tentity.Save(bw);
            }

            return (int)bw.BaseStream.Position;
        }

        public static void LoadV2(BinaryReader b, string filename, World w)
        {
            //throw new NotImplementedException("World Version > 87");

            bool[] tileFrameImportant;
            int[] sectionPointers;

            // reset the stream
            b.BaseStream.Position = (long)0;

            OnProgressChanged(null, new ProgressChangedEventArgs(0, "Loading File Header..."));
            // read section pointers and tile frame data
            if (!LoadSectionHeader(b, out tileFrameImportant, out sectionPointers, w))
                throw new FileFormatException("Invalid File Format Section");

            TileFrameImportant = tileFrameImportant;

            // we should be at the end of the first section
            if (b.BaseStream.Position != sectionPointers[0])
                throw new FileFormatException("Unexpected Position: Invalid File Format Section");

            // Load the flags
            LoadHeaderFlags(b, w, sectionPointers[1]);
            if (b.BaseStream.Position != sectionPointers[1])
                throw new FileFormatException("Unexpected Position: Invalid Header Flags");

            OnProgressChanged(null, new ProgressChangedEventArgs(0, "Loading Tiles..."));
            w.Tiles = LoadTileData(b, w.TilesWide, w.TilesHigh, (int)w.Version);

            if (b.BaseStream.Position != sectionPointers[2])
                b.BaseStream.Position = sectionPointers[2];
                //throw new FileFormatException("Unexpected Position: Invalid Tile Data");

            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading Chests..."));

            foreach (Chest chest in LoadChestData(b))
            {
                //Tile tile = w.Tiles[chest.X, chest.Y];
                //if (tile.IsActive && (tile.Type == 55 || tile.Type == 85))
                {
                    w.Chests.Add(chest);
                }
            }

            if (b.BaseStream.Position != sectionPointers[3])
                throw new FileFormatException("Unexpected Position: Invalid Chest Data");

            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading Signs..."));

            foreach (Sign sign in LoadSignData(b))
            {
                Tile tile = w.Tiles[sign.X, sign.Y];
                if (tile.IsActive && Tile.IsSign(tile.Type))
                {
                    w.Signs.Add(sign);
                }
            }

            if (b.BaseStream.Position != sectionPointers[4])
                throw new FileFormatException("Unexpected Position: Invalid Sign Data");

            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading NPCs..."));
            LoadNPCsData(b, w);
            if (w.Version >= 140)
            {
                OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading Mobs..."));
                LoadMobsData(b, w);
                if (b.BaseStream.Position != sectionPointers[5])
                    throw new FileFormatException("Unexpected Position: Invalid Mob and NPC Data");

                OnProgressChanged(null, new ProgressChangedEventArgs(100, "Loading Tile Entities Section..."));
                LoadTileEntities(b, w);
                if (b.BaseStream.Position != sectionPointers[6])
                    throw new FileFormatException("Unexpected Position: Invalid Tile Entities Section");
            }
            else
            {
                if (b.BaseStream.Position != sectionPointers[5])
                    throw new FileFormatException("Unexpected Position: Invalid NPC Data");
            }
            if (w.Version >= 170)
            {
                LoadPressurePlate(b, w);
                if (b.BaseStream.Position != sectionPointers[7])
                    throw new FileFormatException("Unexpected Position: Invalid Weighted Pressure Plate Section");
            }
            if (w.Version >= 189)
            {
                LoadTownManager(b, w);
                if (b.BaseStream.Position != sectionPointers[8])
                    throw new FileFormatException("Unexpected Position: Invalid Town Manager Section");
            }
            if (w.Version >= 210)
            {
                LoadBestiary(b, w);
                if (b.BaseStream.Position != sectionPointers[9])
                    throw new FileFormatException("Unexpected Position: Invalid Bestiary Section");
            }
            if (w.Version >= 220)
            {
                LoadCreativePowers(b, w);
                if (b.BaseStream.Position != sectionPointers[10])
                    throw new FileFormatException("Unexpected Position: Invalid Creative Powers Section");
            }

            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Verifying File..."));
            LoadFooter(b, w);

            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Load Complete."));
        }

        public static Tile[,] LoadTileData(BinaryReader r, int maxX, int maxY, int version)
        {
            var tiles = new Tile[maxX, maxY];

            int rle;
            for (int x = 0; x < maxX; x++)
            {
                OnProgressChanged(null,
                    new ProgressChangedEventArgs(x.ProgressPercentage(maxX), "Loading Tiles..."));

                for (int y = 0; y < maxY; y++)
                {
                    try
                    {
                        Tile tile = DeserializeTileData(r, version, out rle);


                        tiles[x, y] = tile;
                        while (rle > 0)
                        {
                            y++;

                            if (y >= maxY)
                            {
                                break;
                                throw new FileFormatException(
                                    $"Invalid Tile Data: RLE Compression outside of bounds [{x},{y}]");
                            }
                            tiles[x, y] = (Tile)tile.Clone();
                            rle--;
                        }
                    }
                    catch (Exception ex)
                    {
                        // forcing some recovery here

                        for (int x2 = 0; x2 < maxX; x2++)
                        {
                            for (int y2 = 0; y2 < maxY; y2++)
                            {
                                if (tiles[x2,y2] == null) tiles[x2,y2] = new Tile();
                            }
                        }
                        return tiles;
                    }
                }
            }

            return tiles;
        }

        public static Tile DeserializeTileData(BinaryReader r, int version, out int rle)
        {
            Tile tile = new Tile();

            rle = 0;
            int tileType = -1;
            // byte header4 = 0; // unused, future proofing
            byte header3 = 0;
            byte header2 = 0;
            byte header1 = r.ReadByte();

            // check bit[0] to see if header2 has data
            if ((header1 & 1) == 1)
            {
                header2 = r.ReadByte();

                // check bit[0] to see if header3 has data
                if ((header2 & 1) == 1)
                {
                    header3 = r.ReadByte();

                    // this doesn't exist yet
                    // if ((header3 & 1) == 1)
                    // {
                    //     header4 = r.ReadByte();
                    // }
                }
            }

            // check bit[1] for active tile
            if ((header1 & 2) == 2)
            {
                tile.IsActive = true;

                // read tile type

                if ((header1 & 32) != 32) // check bit[5] to see if tile is byte or little endian int16
                {
                    // tile is byte
                    tileType = r.ReadByte();
                }
                else
                {
                    // tile is little endian int16
                    byte lowerByte = r.ReadByte();
                    tileType = r.ReadByte();
                    tileType = tileType << 8 | lowerByte;
                }
                tile.Type = (ushort)tileType; // convert type to ushort after bit operations

                // read frame UV coords
                if (!TileFrameImportant[tileType])
                {
                    tile.U = -1;
                    tile.V = -1;
                }
                else
                {
                    // read UV coords
                    tile.U = r.ReadInt16();
                    tile.V = r.ReadInt16();

                    // reset timers
                    if (tile.Type == (int)TileType.Timer)
                    {
                        tile.V = 0;
                    }
                }

                // check header3 bit[3] for tile color
                if ((header3 & 8) == 8)
                {
                    tile.TileColor = r.ReadByte();
                }
            }

            // Read Walls
            if ((header1 & 4) == 4) // check bit[3] bit for active wall
            {
                tile.Wall = r.ReadByte();

                // check bit[4] of header3 to see if there is a wall color
                if ((header3 & 16) == 16)
                {
                    tile.WallColor = r.ReadByte();
                }
            }

            // check for liquids, grab the bit[3] and bit[4], shift them to the 0 and 1 bits
            byte liquidType = (byte)((header1 & 24) >> 3);
            if (liquidType != 0)
            {
                tile.LiquidAmount = r.ReadByte();
                tile.LiquidType = (LiquidType)liquidType;
            }

            // check if we have data in header2 other than just telling us we have header3
            if (header2 > 1)
            {
                // check bit[1] for red wire
                if ((header2 & 2) == 2)
                {
                    tile.WireRed = true;
                }
                // check bit[2] for blue wire
                if ((header2 & 4) == 4)
                {
                    tile.WireBlue = true;
                }
                // check bit[3] for green wire
                if ((header2 & 8) == 8)
                {
                    tile.WireGreen = true;
                }

                // grab bits[4, 5, 6] and shift 4 places to 0,1,2. This byte is our brick style
                byte brickStyle = (byte)((header2 & 112) >> 4);
                if (brickStyle != 0 && TileProperties.Count > tile.Type && TileProperties[tile.Type].HasSlopes)
                {
                    tile.BrickStyle = (BrickStyle)brickStyle;
                }
            }

            // check if we have data in header3 to process
            if (header3 > 0)
            {
                // check bit[1] for actuator
                if ((header3 & 2) == 2)
                {
                    tile.Actuator = true;
                }

                // check bit[2] for inactive due to actuator
                if ((header3 & 4) == 4)
                {
                    tile.InActive = true;
                }

                if ((header3 & 32) == 32)
                {
                    tile.WireYellow = true;
                }

                if (version >= 222)
                {
                    if ((header3 & 64) == 64)
                    {
                        tile.Wall = (ushort)(r.ReadByte() << 8 | tile.Wall);
                    }
                }
            }

            // get bit[6,7] shift to 0,1 for RLE encoding type
            // 0 = no RLE compression
            // 1 = byte RLE counter
            // 2 = int16 RLE counter
            // 3 = ERROR
            byte rleStorageType = (byte)((header1 & 192) >> 6);
            switch (rleStorageType)
            {
                case 0:
                    rle = 0;
                    break;
                case 1:
                    rle = r.ReadByte();
                    break;
                default:
                    rle = r.ReadInt16();
                    break;
            }


            return tile;
        }

        public static IEnumerable<Chest> LoadChestData(BinaryReader r)
        {
            int totalChests = r.ReadInt16();
            int maxItems = r.ReadInt16();

            // overflow item check?
            int itemsPerChest;
            int overflowItems;
            if (maxItems > Chest.MaxItems)
            {
                itemsPerChest = Chest.MaxItems;
                overflowItems = maxItems - Chest.MaxItems;
            }
            else
            {
                itemsPerChest = maxItems;
                overflowItems = 0;
            }


            // read chests
            for (int i = 0; i < totalChests; i++)
            {
                var chest = new Chest
                {
                    X = r.ReadInt32(),
                    Y = r.ReadInt32(),
                    Name = r.ReadString()
                };

                // read items in chest
                for (int slot = 0; slot < itemsPerChest; slot++)
                {
                    var stackSize = r.ReadInt16();
                    chest.Items[slot].StackSize = stackSize;

                    if (stackSize > 0)
                    {
                        int id = r.ReadInt32();
                        byte prefix = r.ReadByte();

                        chest.Items[slot].NetId = id;
                        chest.Items[slot].StackSize = stackSize;
                        chest.Items[slot].Prefix = prefix;

                    }
                }

                // dump overflow items
                for (int overflow = 0; overflow < overflowItems; overflow++)
                {
                    var stackSize = r.ReadInt16();
                    if (stackSize > 0)
                    {
                        r.ReadInt32();
                        r.ReadByte();
                    }
                }

                yield return chest;
            }

        }

        public static IEnumerable<Sign> LoadSignData(BinaryReader r)
        {
            short totalSigns = r.ReadInt16();

            for (int i = 0; i < totalSigns; i++)
            {
                string text = r.ReadString();
                int x = r.ReadInt32();
                int y = r.ReadInt32();
                yield return new Sign(x, y, text);
            }
        }

        public static void LoadNPCsData(BinaryReader r, World w)
        {
            int totalNpcs = 0;
            for (bool i = r.ReadBoolean(); i; i = r.ReadBoolean())
            {
                NPC npc = new NPC();
                if (w.Version >= 190)
                {
                    npc.SpriteId = r.ReadInt32();
                    if (NpcNames.ContainsKey(npc.SpriteId))
                        npc.Name = NpcNames[npc.SpriteId];
                }
                else
                {
                    npc.Name = r.ReadString();
                    if (NpcIds.ContainsKey(npc.Name))
                        npc.SpriteId = NpcIds[npc.Name];
                }
                npc.DisplayName = r.ReadString();
                npc.Position = new Vector2(r.ReadSingle(), r.ReadSingle());
                npc.IsHomeless = r.ReadBoolean();
                npc.Home = new Vector2Int32(r.ReadInt32(), r.ReadInt32());

                if (w.Version >= 213 && ((BitsByte)r.ReadByte())[0])
                {
                    npc.TownNpcVariationIndex = r.ReadInt32();
                }

                w.NPCs.Add(npc);
                totalNpcs++;
            }
        }

        public static void LoadMobsData(BinaryReader r, World w)
        {
            int totalMobs = 0;
            bool flag = r.ReadBoolean();
            while (flag)
            {
                NPC npc = new NPC();
                if (w.Version >= 190)
                {
                    npc.SpriteId = r.ReadInt32();
                }
                else
                {
                    npc.Name = r.ReadString();
                    if (NpcIds.ContainsKey(npc.Name))
                        npc.SpriteId = NpcIds[npc.Name];
                }
                npc.Position = new Vector2(r.ReadSingle(), r.ReadSingle());
                w.Mobs.Add(npc);
                totalMobs++;
                flag = r.ReadBoolean();
            }
        }

        public static void LoadTownManager(BinaryReader r, World w)
        {
            int totalRooms = r.ReadInt32();
            for (int i = 0; i < totalRooms; i++)
            {
                TownManager room = new TownManager();
                room.NpcId = r.ReadInt32();
                room.Home = new Vector2Int32(r.ReadInt32(), r.ReadInt32());
                w.PlayerRooms.Add(room);
            }
        }

        public static void LoadBestiary(BinaryReader r, World w)
        {
            w.Bestiary = new Bestiary();
            w.Bestiary.Load(r, w.Version);
        }

        public static void LoadCreativePowers(BinaryReader r, World w)
        {
            w.CreativePowers = new CreativePowers();
            w.CreativePowers.Load(r, w.Version);
        }


        public static void LoadFooter(BinaryReader r, World w)
        {
            if (!r.ReadBoolean())
                throw new FileFormatException("Invalid Footer");

            if (r.ReadString() != w.Title)
                throw new FileFormatException("Invalid Footer");

            if (r.ReadInt32() != w.WorldId)
                throw new FileFormatException("Invalid Footer");
        }

        public static List<TileEntity> LoadTileEntityData(BinaryReader r, uint version)
        {
            int numEntities = r.ReadInt32();
            var entities = new List<TileEntity>();
            for (int i = 0; i < numEntities; i++)
            {
                TileEntity entity = new TileEntity();
                entity.Load(r, version);
                entities.Add(entity);
            }
            return entities;
        }

        public static void LoadTileEntities(BinaryReader r, World w)
        {
            var entities = LoadTileEntityData(r, w.Version);
            w.TileEntitiesNumber = entities.Count;

            w.TileEntities.AddRange(entities);
        }
        public static void LoadPressurePlate(BinaryReader r, World w)
        {
            int count = r.ReadInt32();

            for (int counter = 0; counter < count; counter++)
            {
                PressurePlate plates = new PressurePlate();
                plates.PosX = r.ReadInt32();
                plates.PosY = r.ReadInt32();
                w.PressurePlates.Add(plates);
            }
        }

        public static void LoadHeaderFlags(BinaryReader r, World w, int expectedPosition)
        {
            w.Title = r.ReadString();
            if (w.Version >= 179)
            {
                if (w.Version == 179)
                    w.Seed = r.ReadInt32().ToString();
                else
                    w.Seed = r.ReadString();
                w.WorldGenVersion = r.ReadUInt64();
            }
            else
                w.Seed = "";
            if (w.Version >= 181)
            {
                w.Guid = new Guid(r.ReadBytes(16));
            }
            else
                w.Guid = Guid.NewGuid();
            w.WorldId = r.ReadInt32();
            w.LeftWorld = (float)r.ReadInt32();
            w.RightWorld = (float)r.ReadInt32();
            w.TopWorld = (float)r.ReadInt32();
            w.BottomWorld = (float)r.ReadInt32();
            w.TilesHigh = r.ReadInt32();
            w.TilesWide = r.ReadInt32();

            if (w.Version >= 209)
            {
                w.GameMode = r.ReadInt32();

                if (w.Version >= 222)
                {
                    w.DrunkWorld = r.ReadBoolean();
                }
                if (w.Version >= 227)
                {
                    w.GooWorld = r.ReadBoolean();
                }
            }
            else
            {
                w.GameMode = (w.Version < 112) ? 0 : r.ReadBoolean() ? 1 : 0; // 0 = normal, 1 = expert mode
                if (w.Version == 208 && r.ReadBoolean())
                {
                    w.GameMode = 2;
                }
            }

            w.CreationTime = w.Version < 141 ? DateTime.Now.ToBinary() : w.CreationTime = r.ReadInt64();

            w.MoonType = r.ReadByte();
            w.TreeX[0] = r.ReadInt32();
            w.TreeX[1] = r.ReadInt32();
            w.TreeX[2] = r.ReadInt32();
            w.TreeX2 = w.TreeX[2];
            w.TreeX1 = w.TreeX[1];
            w.TreeX0 = w.TreeX[0];
            w.TreeStyle0 = r.ReadInt32();
            w.TreeStyle1 = r.ReadInt32();
            w.TreeStyle2 = r.ReadInt32();
            w.TreeStyle3 = r.ReadInt32();
            w.CaveBackX[0] = r.ReadInt32();
            w.CaveBackX[1] = r.ReadInt32();
            w.CaveBackX[2] = r.ReadInt32();
            w.CaveBackX2 = w.CaveBackX[2];
            w.CaveBackX1 = w.CaveBackX[1];
            w.CaveBackX0 = w.CaveBackX[0];
            w.CaveBackStyle0 = r.ReadInt32();
            w.CaveBackStyle1 = r.ReadInt32();
            w.CaveBackStyle2 = r.ReadInt32();
            w.CaveBackStyle3 = r.ReadInt32();
            w.IceBackStyle = r.ReadInt32();
            w.JungleBackStyle = r.ReadInt32();
            w.HellBackStyle = r.ReadInt32();

            w.SpawnX = r.ReadInt32();
            w.SpawnY = r.ReadInt32();
            w.GroundLevel = r.ReadDouble();
            w.RockLevel = r.ReadDouble();
            w.Time = r.ReadDouble();
            w.DayTime = r.ReadBoolean();
            w.MoonPhase = r.ReadInt32();
            w.BloodMoon = r.ReadBoolean();
            w.IsEclipse = r.ReadBoolean();
            w.DungeonX = r.ReadInt32();
            w.DungeonY = r.ReadInt32();

            w.IsCrimson = r.ReadBoolean();

            w.DownedBoss1 = r.ReadBoolean();
            w.DownedBoss2 = r.ReadBoolean();
            w.DownedBoss3 = r.ReadBoolean();
            w.DownedQueenBee = r.ReadBoolean();
            w.DownedMechBoss1 = r.ReadBoolean();
            w.DownedMechBoss2 = r.ReadBoolean();
            w.DownedMechBoss3 = r.ReadBoolean();
            w.DownedMechBossAny = r.ReadBoolean();
            w.DownedPlantBoss = r.ReadBoolean();
            w.DownedGolemBoss = r.ReadBoolean();
            if (w.Version >= 147)
                w.DownedSlimeKingBoss = r.ReadBoolean();
            w.SavedGoblin = r.ReadBoolean();
            w.SavedWizard = r.ReadBoolean();
            w.SavedMech = r.ReadBoolean();
            w.DownedGoblins = r.ReadBoolean();
            w.DownedClown = r.ReadBoolean();
            w.DownedFrost = r.ReadBoolean();
            w.DownedPirates = r.ReadBoolean();

            w.ShadowOrbSmashed = r.ReadBoolean();
            w.SpawnMeteor = r.ReadBoolean();
            w.ShadowOrbCount = (int)r.ReadByte();
            w.AltarCount = r.ReadInt32();
            w.HardMode = r.ReadBoolean();
            w.InvasionDelay = r.ReadInt32();
            w.InvasionSize = r.ReadInt32();
            w.InvasionType = r.ReadInt32();
            w.InvasionX = r.ReadDouble();

            if (w.Version >= 147)
            {
                w.SlimeRainTime = r.ReadDouble();
                w.SundialCooldown = r.ReadByte();
            }

            w.TempRaining = r.ReadBoolean();
            w.TempRainTime = r.ReadInt32();
            w.TempMaxRain = r.ReadSingle();
            w.SavedOreTiersCobalt = r.ReadInt32();
            w.SavedOreTiersMythril = r.ReadInt32();
            w.SavedOreTiersAdamantite = r.ReadInt32();
            w.BgTree = r.ReadByte();
            w.BgCorruption = r.ReadByte();
            w.BgJungle = r.ReadByte();
            w.BgSnow = r.ReadByte();
            w.BgHallow = r.ReadByte();
            w.BgCrimson = r.ReadByte();
            w.BgDesert = r.ReadByte();
            w.BgOcean = r.ReadByte();
            w.CloudBgActive = (float)r.ReadInt32();
            w.NumClouds = r.ReadInt16();
            w.WindSpeedSet = r.ReadSingle();

            if (w.Version >= 95)
            {
                for (int i = r.ReadInt32(); i > 0; i--)
                {
                    w.Anglers.Add(r.ReadString());
                }
            }

            if (w.Version < 99)
                return;

            w.SavedAngler = r.ReadBoolean();


            if (w.Version < 101)
                return;
            w.AnglerQuest = r.ReadInt32();


            if (w.Version < 104)
                return;


            w.SavedStylist = r.ReadBoolean();

            if (w.Version >= 129)
            {
                w.SavedTaxCollector = r.ReadBoolean();
            }

            if (w.Version >= 201)
            {
                w.SavedGolfer = r.ReadBoolean();
            }

            if (w.Version >= 107)
            {
                w.InvasionSizeStart = r.ReadInt32();
            }
            w.CultistDelay = w.Version >= 108 ? r.ReadInt32() : 86400;
            int numberOfMobs = r.ReadInt16();
            w.NumberOfMobs = numberOfMobs;
            for (int counter = 0; counter < numberOfMobs; counter++)
            {
                if (counter < 663)
                    w.KilledMobs.Add(r.ReadInt32());
                else
                    r.ReadInt32();
            }
            w.FastForwardTime = r.ReadBoolean();
            w.DownedFishron = r.ReadBoolean();
            w.DownedMartians = r.ReadBoolean();
            w.DownedLunaticCultist = r.ReadBoolean();
            w.DownedMoonlord = r.ReadBoolean();
            w.DownedHalloweenKing = r.ReadBoolean();
            w.DownedHalloweenTree = r.ReadBoolean();
            w.DownedChristmasQueen = r.ReadBoolean();
            w.DownedSanta = r.ReadBoolean();
            w.DownedChristmasTree = r.ReadBoolean();
            w.DownedCelestialSolar = r.ReadBoolean();
            w.DownedCelestialVortex = r.ReadBoolean();
            w.DownedCelestialNebula = r.ReadBoolean();
            w.DownedCelestialStardust = r.ReadBoolean();
            w.CelestialSolarActive = r.ReadBoolean();
            w.CelestialVortexActive = r.ReadBoolean();
            w.CelestialNebulaActive = r.ReadBoolean();
            w.CelestialStardustActive = r.ReadBoolean();
            w.Apocalypse = r.ReadBoolean();

            if (w.Version >= 170)
            {
                w.PartyManual = r.ReadBoolean();
                w.PartyGenuine = r.ReadBoolean();
                w.PartyCooldown = r.ReadInt32();
                int numparty = r.ReadInt32();
                for (int counter = 0; counter < numparty; counter++)
                {
                    w.PartyingNPCs.Add(r.ReadInt32());
                }
            }
            if (w.Version >= 174)
            {
                w.SandStormHappening = r.ReadBoolean();
                w.SandStormTimeLeft = r.ReadInt32();
                w.SandStormSeverity = r.ReadSingle();
                w.SandStormIntendedSeverity = r.ReadSingle();
            }
            if (w.Version >= 178)
            {
                w.SavedBartender = r.ReadBoolean();
                w.DownedDD2InvasionT1 = r.ReadBoolean();
                w.DownedDD2InvasionT2 = r.ReadBoolean();
                w.DownedDD2InvasionT3 = r.ReadBoolean();
            }

            // 1.4 Journey's End
            if (w.Version > 194)
            {
                w.MushroomBg = r.ReadByte();
            }

            if (w.Version >= 215)
            {
                w.UnderworldBg = r.ReadByte();
            }


            if (w.Version >= 195)
            {
                w.BgTree2 = r.ReadByte();
                w.BgTree3 = r.ReadByte();
                w.BgTree4 = r.ReadByte();
            }
            else
            {
                w.BgTree2 = w.BgTree;
                w.BgTree3 = w.BgTree;
                w.BgTree4 = w.BgTree;
            }
            if (w.Version >= 204)
            {
                w.CombatBookUsed = r.ReadBoolean();
            }
            if (w.Version >= 207)
            {
                w.TempLanternNightCooldown = r.ReadInt32();
                w.TempLanternNightGenuine = r.ReadBoolean();
                w.TempLanternNightManual = r.ReadBoolean();
                w.TempLanternNightNextNightIsGenuine = r.ReadBoolean();
            }
            // tree tops
            if (w.Version >= 211)
            {
                int numTrees = r.ReadInt32();
                w.TreeTopVariations = new System.Collections.ObjectModel.ObservableCollection<int>(new int[numTrees]);
                for (int i = 0; i < numTrees; i++)
                {
                    w.TreeTopVariations[i] = r.ReadInt32();
                }
            }
            else
            {
                w.TreeTopVariations[0] = w.TreeStyle0;
                w.TreeTopVariations[1] = w.TreeStyle1;
                w.TreeTopVariations[2] = w.TreeStyle2;
                w.TreeTopVariations[3] = w.TreeStyle3;
                w.TreeTopVariations[4] = w.BgCorruption;
                w.TreeTopVariations[5] = w.JungleBackStyle;
                w.TreeTopVariations[6] = w.BgSnow;
                w.TreeTopVariations[7] = w.BgHallow;
                w.TreeTopVariations[8] = w.BgCrimson;
                w.TreeTopVariations[9] = w.BgDesert;
                w.TreeTopVariations[10] = w.BgOcean;
                w.TreeTopVariations[11] = w.MushroomBg;
                w.TreeTopVariations[12] = w.UnderworldBg;
            }
            if (w.Version >= 212)
            {
                w.ForceHalloweenForToday = r.ReadBoolean();
                w.ForceXMasForToday = r.ReadBoolean();
            }

            if (w.Version >= 216)
            {
                w.SavedOreTiersCopper = r.ReadInt32();
                w.SavedOreTiersIron = r.ReadInt32();
                w.SavedOreTiersSilver = r.ReadInt32();
                w.SavedOreTiersGold = r.ReadInt32();
            }
            else
            {
                w.SavedOreTiersCopper = -1;
                w.SavedOreTiersIron = -1;
                w.SavedOreTiersSilver = -1;
                w.SavedOreTiersGold = -1;
            }

            if (w.Version >= 217)
            {
                w.BoughtCat = r.ReadBoolean();
                w.BoughtDog = r.ReadBoolean();
                w.BoughtBunny = r.ReadBoolean();
            }

            if (w.Version >= 223)
            {
                w.DownedEmpressOfLight = r.ReadBoolean();
                w.DownedQueenSlime = r.ReadBoolean();
            }


            // a little future proofing, read any "unknown" flags from the end of the list and save them. We will write these back after we write our "known" flags.
            if (r.BaseStream.Position < expectedPosition)
            {
                w.UnknownData = r.ReadBytes(expectedPosition - (int)r.BaseStream.Position);
            }
        }

        public static bool LoadSectionHeader(BinaryReader r, out bool[] tileFrameImportant, out int[] sectionPointers, World w)
        {
            tileFrameImportant = null;
            sectionPointers = null;
            int versionNumber = r.ReadInt32();
            if (versionNumber > 140)
            {
                UInt64 versionTypecheck = r.ReadUInt64();
                if (versionTypecheck != 0x026369676f6c6572ul)
                    throw new FileFormatException("Invalid Header");

                w.FileRevision = r.ReadUInt32();
                UInt64 temp = r.ReadUInt64();//I have no idea what this is for...
                w.IsFavorite = ((temp & 1uL) == 1uL);
            }

            // read file section stream positions
            short fileSectionCount = r.ReadInt16();
            sectionPointers = new int[fileSectionCount];
            for (int i = 0; i < fileSectionCount; i++)
            {
                sectionPointers[i] = r.ReadInt32();
            }

            // Read tile frame importance from bit-packed data
            tileFrameImportant = ReadBitArray(r);

            return true;
        }

        /// <summary>
        /// Read an array of booleans from a bit-packed array.
        /// </summary>
        /// <param name="reader">BinaryReader at start of bit array.</param>
        /// <returns>Array of booleans</returns>
        public static bool[] ReadBitArray(BinaryReader reader)
        {
            // get the number of bits
            int length = reader.ReadInt16();

            // read the bit data
            var booleans = new bool[length];
            byte data = 0;
            byte bitMask = 128;
            for (int i = 0; i < length; i++)
            {
                // If we read the last bit mask (B1000000 = 0x80 = 128), read the next byte from the stream and start the mask over.
                // Otherwise, keep incrementing the mask to get the next bit.
                if (bitMask != 128)
                {
                    bitMask = (byte)(bitMask << 1);
                }
                else
                {
                    data = reader.ReadByte();
                    bitMask = 1;
                }

                // Check the mask, if it is set then set the current boolean to true
                if ((data & bitMask) == bitMask)
                {
                    booleans[i] = true;
                }
            }

            return booleans;
        }

        /// <summary>
        /// Write an array of booleans to a binary stream as a bit-packed array.
        /// </summary>
        /// <param name="writer">BinaryWriter stream.</param>
        /// <param name="values">Collection of booleans to write as a bit-packed array.</param>
        public static void WriteBitArray(BinaryWriter writer, bool[] values)
        {
            // write the number of bits
            writer.Write((Int16)values.Length);

            // write the bit data
            byte data = 0;
            byte bitMask = 1;
            for (int i = 0; i < values.Length; i++)
            {
                // Check if the current value is true, if it is set then set the bit for the current mask in the data byte.
                if (values[i])
                {
                    data = (byte)(data | bitMask);
                }

                // If we wrote the last bit mask (B1000000 = 0x80 = 128), write the data byte to the stream and start the mask over.
                // Otherwise, keep incrementing the mask to write the next bit.
                if (bitMask != 128)
                {
                    bitMask = (byte)(bitMask << 1);
                }
                else
                {
                    writer.Write(data);
                    data = 0;
                    bitMask = 1;
                }
            }

            // Write any remaning data in the buffer.
            if (bitMask != 1)
            {
                writer.Write(data);
            }
        }


    }
}
