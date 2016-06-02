using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Windows;
using System.Xml.Linq;
using TEdit.Utility;
using TEditXna.Helper;
using TEditXNA.Terraria.Objects;
using TEdit.Geometry.Primitives;
using Vector2 = TEdit.Geometry.Primitives.Vector2;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace TEditXNA.Terraria
{

    public partial class World
    {
        public static uint CompatibleVersion = 168;
        public static short TileCount = 446;
        public static short SectionCount = 10;

        public static bool[] TileFrameImportant;

        private static void SaveV2(World world, BinaryWriter bw)
        {
            world.Validate();

            // initialize tileframeimportance array if empty
            if (TileFrameImportant == null || TileFrameImportant.Length < TileCount)
            {
                TileFrameImportant = new bool[TileCount];
                for (int i = 0; i < TileCount; i++)
                {
                    if (World.TileProperties.Count > i)
                    {
                        TileFrameImportant[i] = World.TileProperties[i].IsFramed;
                    }
                }
            }

            int[] sectionPointers = new int[SectionCount];

            OnProgressChanged(null, new ProgressChangedEventArgs(0, "Save headers..."));
            sectionPointers[0] = SaveSectionHeader(world, bw);
            sectionPointers[1] = SaveHeaderFlags(world, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(0, "Save UndoTiles..."));
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
            sectionPointers[6] = SaveTileEntities(world, bw);
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

            byte[] tileData = new byte[13];
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
                tileData[dataIndex++] = tile.Wall;

                // save tile wall color
                if (tile.WallColor != 0)
                {
                    // set header3 bit[4] for wall color active
                    header3 = (byte)(header3 | 16);
                    tileData[dataIndex++] = tile.WallColor;
                }
            }

            // liquid data
            if (tile.LiquidAmount != 0)
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
                bw.Write(npc.Name);
                bw.Write(npc.DisplayName);
                bw.Write(npc.Position.X);
                bw.Write(npc.Position.Y);
                bw.Write(npc.IsHomeless);
                bw.Write(npc.Home.X);
                bw.Write(npc.Home.Y);
            }
            bw.Write(false);

            return (int)bw.BaseStream.Position;
        }

        public static int SaveMobs(IEnumerable<NPC> mobs, BinaryWriter bw)
        {
            foreach (NPC mob in mobs)
            {
                bw.Write(true);
                bw.Write(mob.Name);                
                bw.Write(mob.Position.X);
                bw.Write(mob.Position.Y);                
            }
            bw.Write(false);

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
            bw.Write((int)world.FileRevision+1);
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
            bw.Write(world.WorldId);
            bw.Write((int)world.LeftWorld);
            bw.Write((int)world.RightWorld);
            bw.Write((int)world.TopWorld);
            bw.Write((int)world.BottomWorld);
            bw.Write(world.TilesHigh);
            bw.Write(world.TilesWide);
            bw.Write(world.ExpertMode);
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
            bw.Write(world.InvasionSizeStart);
            bw.Write(world.CultistDelay);
            bw.Write((Int16)world.NumberOfMobs);
            foreach (int count in world.KilledMobs)
            {
                bw.Write(count);
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

            if (world.UnknownData != null && world.UnknownData.Length > 0)
                bw.Write(world.UnknownData);

            return (int)bw.BaseStream.Position;
        }

        public static int SaveTileEntities(World w, BinaryWriter bw)
        {
            bw.Write(w.TileEntitiesNumber);

            foreach(TileEntity tentity in w.TileEntities)
            {
                bw.Write(tentity.Type);
                bw.Write(tentity.Id);
                bw.Write(tentity.PosX);
                bw.Write(tentity.PosY);
                switch (tentity.Type)
                {
                    case 0: //it is a dummy                        
                        bw.Write(tentity.Npc);
                        break;
                    case 1: //it is a item frame                        
                        bw.Write(tentity.ItemNetId);
                        bw.Write(tentity.Prefix);
                        bw.Write(tentity.Stack);
                        break;
                    case 2: //it is a logic sensor
                        bw.Write(tentity.LogicCheck);
                        bw.Write(tentity.On);
                        break;
                }
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

            OnProgressChanged(null, new ProgressChangedEventArgs(0, "Loading UndoTiles..."));
            w.Tiles = LoadTileData(b, w.TilesWide, w.TilesHigh);
            if (b.BaseStream.Position != sectionPointers[2])
                throw new FileFormatException("Unexpected Position: Invalid Tile Data");

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
            if(w.Version >= 140)
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

            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Verifying File..."));
            LoadFooter(b, w);

            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Load Complete."));
        }

        public static Tile[,] LoadTileData(BinaryReader r, int maxX, int maxY)
        {
            var tiles = new Tile[maxX, maxY];

            int rle;
            for (int x = 0; x < maxX; x++)
            {
                OnProgressChanged(null,
                    new ProgressChangedEventArgs(x.ProgressPercentage(maxX), "Loading UndoTiles..."));

                for (int y = 0; y < maxY; y++)
                {
                    Tile tile = DeserializeTileData(r, out rle);

                    tiles[x, y] = tile;
                    while (rle > 0)
                    {
                        y++;

                        if (y > maxY)
                            throw new FileFormatException(string.Format("Invalid Tile Data: RLE Compression outside of bounds [{0},{1}]", x, y));

                        tiles[x, y] = (Tile)tile.Clone();
                        rle--;
                    }
                }
            }

            return tiles;
        }

        public static Tile DeserializeTileData(BinaryReader r, out int rle)
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
                if (brickStyle != 0 && World.TileProperties.Count > tile.Type && World.TileProperties[tile.Type].IsSolid)
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
            }

            // get bit[6,7] shift to 0,1 for RLE encoding type
            // 0 = no RLE compression
            // 1 = byte RLE counter
            // 2 = int16 RLE counter
            // 3 = ERROR
            byte rleStorageType = (byte)((header1 & 192) >> 6);

            // read RLE distance
            if (rleStorageType == 0)
            {
                rle = 0;
            }
            else if (rleStorageType != 1)
            {
                rle = r.ReadInt16();
            }
            else
            {
                rle = r.ReadByte();
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
                npc.Name = r.ReadString();
                npc.DisplayName = r.ReadString();
                npc.Position = new Vector2(r.ReadSingle(), r.ReadSingle());
                npc.IsHomeless = r.ReadBoolean();
                npc.Home = new Vector2Int32(r.ReadInt32(), r.ReadInt32());

                if (NpcIds.ContainsKey(npc.Name))
                    npc.SpriteId = NpcIds[npc.Name];

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
                npc.Name = r.ReadString();                    
                npc.Position = new Vector2(r.ReadSingle(), r.ReadSingle());

                if (NpcIds.ContainsKey(npc.Name))
                    npc.SpriteId = NpcIds[npc.Name];

                w.Mobs.Add(npc);
                totalMobs++;
                flag = r.ReadBoolean();
            }
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

        public static void LoadTileEntities(BinaryReader r, World w)
        {
            w.TileEntitiesNumber = r.ReadInt32();

            for (int counter = 0; counter < w.TileEntitiesNumber; counter++ )
            {
                TileEntity entity = new TileEntity();
                entity.Type = r.ReadByte();
                entity.Id = r.ReadInt32();
                entity.PosX = r.ReadInt16();
                entity.PosY = r.ReadInt16();
                switch (entity.Type)
                {
                    case 0: //it is a dummy
                        entity.Npc = r.ReadInt16();
                        break;
                    case 1: //it is a item frame
                        entity.ItemNetId = r.ReadInt16();
                        entity.Prefix = r.ReadByte();
                        entity.Stack = r.ReadInt16();
                        break;
                    case 2: //it is a logic sensor
                        entity.LogicCheck = r.ReadByte();
                        entity.On = r.ReadBoolean();
                        break;
                }
                w.TileEntities.Add(entity);
            }
        }

        public static void LoadHeaderFlags(BinaryReader r, World w, int expectedPosition)
        {
            w.Title = r.ReadString();
            w.WorldId = r.ReadInt32();
            w.LeftWorld = (float)r.ReadInt32();
            w.RightWorld = (float)r.ReadInt32();
            w.TopWorld = (float)r.ReadInt32();
            w.BottomWorld = (float)r.ReadInt32();
            w.TilesHigh = r.ReadInt32();
            w.TilesWide = r.ReadInt32();

            if (w.Version >= 147)
            {
                w.ExpertMode = r.ReadBoolean();
                w.CreationTime = r.ReadInt64();
            }
            else
            {
                w.CreationTime = DateTime.Now.ToBinary();
            }

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
            if (w.Version >= 147) w.DownedSlimeKingBoss = r.ReadBoolean();
            w.SavedGoblin = r.ReadBoolean();
            w.SavedWizard = r.ReadBoolean();
            w.SavedMech = r.ReadBoolean();
            w.DownedGoblins = r.ReadBoolean();
            w.DownedClown = r.ReadBoolean();
            w.DownedFrost = r.ReadBoolean();
            w.DownedPirates = r.ReadBoolean();

            w.ShadowOrbSmashed = r.ReadBoolean();
            w.SpawnMeteor = r.ReadBoolean();
            w.ShadowOrbCount = r.ReadByte();
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
            w.OreTier1 = r.ReadInt32();
            w.OreTier2 = r.ReadInt32();
            w.OreTier3 = r.ReadInt32();
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

            if (w.Version >= 99)
            {
                w.SavedAngler = r.ReadBoolean();
            }

            if (w.Version >= 101)
            {
                w.AnglerQuest = r.ReadInt32();
            }

            if (w.Version >= 104)
            {
                w.SavedStylist = r.ReadBoolean();
            }

            if (w.Version >= 140)
            {
                w.SavedTaxCollector = r.ReadBoolean();
                w.InvasionSizeStart = r.ReadInt32();
                w.CultistDelay = r.ReadInt32();
                int numberOfMobs = r.ReadInt16();
                w.NumberOfMobs = numberOfMobs;
                for (int counter = 0; counter < numberOfMobs; counter++)
                {
                    w.KilledMobs.Add(r.ReadInt32());
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
            if(versionNumber > 140)
            {
                UInt64 versionTypecheck = r.ReadUInt64();
                if (versionTypecheck != 0x026369676f6c6572ul )
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
