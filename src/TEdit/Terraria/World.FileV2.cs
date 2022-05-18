using System.Collections.Generic;
using System.ComponentModel;
using TEdit.Utility;
using TEdit.Geometry.Primitives;
using Vector2 = TEdit.Geometry.Primitives.Vector2;
using System;
using System.IO;
using TEdit.Helper;
using System.Linq;

namespace TEdit.Terraria
{

    public partial class World
    {
        public static readonly uint CompatibleVersion;
        public const short TileCount = 623;
        public const short WallCount = 316;

        public const short KillTallyMax = 663;

        public const int MaxChests = 8000;
        public const int MaxSigns = 1000;

        public bool IsTModLoader { get; set; }

        public short GetSectionCount()
        {
            short numSections = 11;

            if (Version >= 220) { numSections = 11; }
            else if (Version >= 210) { numSections = 10; }
            else if (Version >= 189) { numSections = 10; }
            else if (Version >= 170) { numSections = 9; }
            else if (Version >= 140) { numSections = 8; }
            else if (Version > 140) { numSections = 7; }

            return numSections;
        }

        public bool[] TileFrameImportant { get; set; }

        public static bool[] SettingsTileFrameImportant { get; set; }

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

                w.TileFrameImportant = tileFrameImportant;

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

        internal static void SaveV2(World world, BinaryWriter bw, TextWriter debugger = null)
        {
            world.Validate();
            world.FileRevision++;

            int[] sectionPointers = new int[world.GetSectionCount()];
            bool[] tileFrameImportant = SaveConfiguration.GetTileFramesForVersion((int)world.Version);

            debugger?.WriteLine("{");


            OnProgressChanged(null, new ProgressChangedEventArgs(0, "Save headers..."));
            sectionPointers[0] = SaveSectionHeader(world, bw, tileFrameImportant, debugger);
            sectionPointers[1] = SaveHeaderFlags(world, bw, (int)world.Version, debugger);
            OnProgressChanged(null, new ProgressChangedEventArgs(0, "Save Tiles..."));
            sectionPointers[2] = SaveTiles(world.Tiles, (int)world.Version, world.TilesWide, world.TilesHigh, bw, tileFrameImportant, debugger);

            OnProgressChanged(null, new ProgressChangedEventArgs(91, "Save Chests..."));
            sectionPointers[3] = SaveChests(world.Chests, bw, (int)world.Version, world.Version < 226);
            OnProgressChanged(null, new ProgressChangedEventArgs(92, "Save Signs..."));
            sectionPointers[4] = SaveSigns(world.Signs, bw, world.Version < 226);
            OnProgressChanged(null, new ProgressChangedEventArgs(93, "Save NPCs..."));
            sectionPointers[5] = SaveNPCs(world.NPCs, bw, (int)world.Version);

            if (world.Version >= 140)
            {
                OnProgressChanged(null, new ProgressChangedEventArgs(94, "Save Mobs..."));
                sectionPointers[5] = SaveMobs(world.Mobs, bw, (int)world.Version);

                OnProgressChanged(null, new ProgressChangedEventArgs(95, "Save Tile Entities Section..."));
                sectionPointers[6] = SaveTileEntities(world.TileEntities, bw);
            }

            if (world.Version >= 170)
            {
                OnProgressChanged(null, new ProgressChangedEventArgs(96, "Save Weighted Pressure Plates..."));
                sectionPointers[7] = SavePressurePlate(world.PressurePlates, bw);
            }

            if (world.Version >= 189)
            {
                OnProgressChanged(null, new ProgressChangedEventArgs(97, "Save Town Manager..."));
                sectionPointers[8] = SaveTownManager(world.PlayerRooms, bw);
            }

            if (world.Version >= 210)
            {
                OnProgressChanged(null, new ProgressChangedEventArgs(98, "Save Bestiary..."));
                sectionPointers[9] = SaveBestiary(world.Bestiary, bw);
            }

            if (world.Version >= 220)
            {
                OnProgressChanged(null, new ProgressChangedEventArgs(99, "Save Creative Powers..."));
                sectionPointers[10] = SaveCreativePowers(world.CreativePowers, bw);
            }

            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Footers..."));
            SaveFooter(world, bw);
            UpdateSectionPointers(sectionPointers, bw);
            OnProgressChanged(null, new ProgressChangedEventArgs(100, "Save Complete."));

            debugger?.WriteLine("}");
        }

        public static int SaveTiles(Tile[,] tiles, int version, int maxX, int maxY, BinaryWriter bw, bool[] tileFrameImportant, TextWriter debugger = null)
        {
            debugger?.WriteLine("\"Tiles\": [");

            int maxTileId = SaveConfiguration.SaveVersions[version].MaxTileId;
            int maxWallId = SaveConfiguration.SaveVersions[version].MaxWallId;

            for (int x = 0; x < maxX; x++)
            {
                OnProgressChanged(null, new ProgressChangedEventArgs((int)(x.ProgressPercentage(maxX) * 0.9), "Saving Tiles..."));


                for (int y = 0; y < maxY; y++)
                {
                    Tile tile = tiles[x, y];


                    int dataIndex;
                    int headerIndex;

                    debugger?.WriteLine("{");

                    byte[] tileData = SerializeTileData(tile, version, maxTileId, maxWallId, tileFrameImportant, out dataIndex, out headerIndex, debugger);

                    // rle compression
                    byte header1 = tileData[headerIndex];

                    short rle = 0;
                    int nextY = y + 1;
                    int remainingY = maxY - y - 1;
                    while (remainingY > 0 && tile.Equals(tiles[x, nextY]) && tile.Type != 520 && tile.Type != 423)
                    {
                        rle = (short)(rle + 1);
                        remainingY--;
                        nextY++;
                    }

                    debugger?.Write(",\"RLE\": {0}", rle);

                    y = y + rle;

                    if (rle > 0)
                    {
                        // always write lower half
                        tileData[dataIndex++] = (byte)(rle & 0b_1111_1111); //255

                        if (rle <= 255)
                        {
                            // set bit[6] of header1 for byte size rle
                            header1 = (byte)(header1 | 0b_0100_0000); // 64
                        }
                        else
                        {
                            // set bit[7] of header1 for int16 size rle
                            header1 = (byte)(header1 | 0b_1000_0000); //128

                            // grab the upper half of the int16 and stick it in tiledata
                            tileData[dataIndex++] = (byte)((rle & 0b_1111_1111_0000_0000) >> 8); // 65280
                        }
                    }

                    tileData[headerIndex] = header1;
                    // end rle compression

                    debugger?.WriteLine(",\"TileDataSize\": {0}}},", rle);

                    bw.Write(tileData, headerIndex, dataIndex - headerIndex);
                }
            }

            debugger?.WriteLine("],");
            debugger?.WriteLine("\"SECTION_2\": {0},", bw.BaseStream.Position);

            return (int)bw.BaseStream.Position;
        }

        /// <summary>
        /// BitPack tile data and headers
        /// </summary>
        public static byte[] SerializeTileData(
            Tile tile, 
            int version,
            int maxTileId,
            int maxWallId,
            bool[] tileFrameImportant, 
            out int dataIndex, 
            out int headerIndex, 
            TextWriter debugger = null)
        {
            var size = (version > 222) ? 15 : 13; // packed size
            byte[] tileData = new byte[size];
            dataIndex = 3;

            byte header3 = (byte)0;
            byte header2 = (byte)0;
            byte header1 = (byte)0;

            // tile data
            debugger?.Write("\"IsActive\": {0},", tile.IsActive);

            if (tile.IsActive && tile.Type < maxTileId && tile.Type != (int)TileType.IceByRod)
            {
                // activate bit[1]
                header1 |= 0b_0000_0010;

                // save tile type as byte or int16
                tileData[dataIndex++] = (byte)tile.Type; // low byte
                if (tile.Type > 255)
                {
                    // write high byte
                    tileData[dataIndex++] = (byte)(tile.Type >> 8);

                    // set header1 bit[5] for int16 tile type
                    header1 |= 0b_0010_0000;
                }

                debugger?.Write("\"Type\": {0},", tile.Type);

                if (tileFrameImportant[tile.Type])
                {
                    // pack UV coords
                    tileData[dataIndex++] = (byte)(tile.U & 0xFF); // low byte
                    tileData[dataIndex++] = (byte)((tile.U & 0xFF00) >> 8); // high byte
                    tileData[dataIndex++] = (byte)(tile.V & 0xFF); // low byte
                    tileData[dataIndex++] = (byte)((tile.V & 0xFF00) >> 8); // high byte

                    debugger?.Write("\"U\": {0},", tile.U);
                    debugger?.Write("\"V\": {0},", tile.V);
                }


                if (tile.TileColor != 0)
                {
                    // set header3 bit[3] for tile color active
                    header3 |= 0b_0000_0100;
                    tileData[dataIndex++] = tile.TileColor;
                    debugger?.Write("\"TileColor\": {0},", tile.TileColor);
                }
            }

            // wall data
            if (tile.Wall != 0 && tile.Wall < maxWallId)
            {
                // set header1 bit[2] for wall active
                header1 |= 0b_0000_0100;
                tileData[dataIndex++] = (byte)tile.Wall;
                debugger?.Write("\"Wall\": {0},", tile.Wall);


                // save tile wall color
                if (tile.WallColor != 0)
                {
                    // set header3 bit[4] for wall color active
                    header3 |= 0b_0001_0000;
                    tileData[dataIndex++] = tile.WallColor;
                    debugger?.Write("\"WallColor\": {0},", tile.WallColor);

                }
            }

            // liquid data
            if (tile.LiquidAmount != 0 && tile.LiquidType != LiquidType.None)
            {
                // set bits[3,4] using left shift
                header1 = (byte)(header1 | (byte)((byte)tile.LiquidType << 3));
                tileData[dataIndex++] = tile.LiquidAmount;

                debugger?.Write("\"LiquidType\": \"{0}\",", tile.LiquidType.ToString());
                debugger?.Write("\"LiquidAmount\": {0},", tile.LiquidAmount);
            }

            // wire data
            if (tile.WireRed)
            {
                // red wire = header2 bit[1]
                header2 |= 0b_0000_0010;
                debugger?.Write("\"WireRed\": {0},", tile.WireRed);

            }
            if (tile.WireBlue)
            {
                // blue wire = header2 bit[2]
                header2 |= 0b_0000_0100;
                debugger?.Write("\"WireBlue\": {0},", tile.WireBlue);

            }
            if (tile.WireGreen)
            {
                // green wire = header2 bit[3]
                header2 |= 0b_0000_1000;
                debugger?.Write("\"WireGreen\": {0},", tile.WireGreen);
            }

            // brick style
            byte brickStyle = (byte)((byte)tile.BrickStyle << 4);

            // set bits[4,5,6] of header2
            header2 = (byte)(header2 | brickStyle);
            debugger?.Write("\"BrickStyle\": {0},", tile.BrickStyle);


            // actuator data
            if (tile.Actuator)
            {
                // set bit[1] of header3
                header3 |= 0b_0000_0010;
            }
            if (tile.InActive)
            {
                // set bit[2] of header3
                header3 |= 0b_0000_0100;
            }
            if (tile.WireYellow)
            {
                header3 |= 0b_0010_0000;
            }

            // wall high byte
            if (tile.Wall > 255 && version >= 222)
            {
                header3 |= 0b_0100_0000;
                tileData[dataIndex++] = (byte)(tile.Wall >> 8);
            }


            headerIndex = 2;
            if (header3 != 0)
            {
                // set header3 active flag bit[0] of header2
                header2 |= 0b_0000_0001;
                tileData[headerIndex--] = header3;
            }
            if (header2 != 0)
            {
                // set header2 active flag bit[0] of header1
                header1 |= 0b_0000_0001;
                tileData[headerIndex--] = header2;
            }

            tileData[headerIndex] = header1;
            return tileData;
        }

        public static int SaveChests(IList<Chest> chests, BinaryWriter bw, int version, bool useLegacyLimit = false)
        {
            Int16 count = useLegacyLimit ? (Int16)Math.Min(chests.Count, Chest.LegacyLimit) : (Int16)chests.Count;
            bw.Write(count);
            bw.Write((Int16)Chest.MaxItems);

            int written = 0;
            foreach (Chest chest in chests)
            {
                bw.Write(chest.X);
                bw.Write(chest.Y);
                bw.Write(chest.Name ?? string.Empty);

                for (int slot = 0; slot < Chest.MaxItems; slot++)
                {
                    Item item = chest.Items[slot];

                    // check if target version allows item.
                    if (item != null && item.NetId <= SaveConfiguration.SaveVersions[version].MaxItemId)
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
                written++;
                if (useLegacyLimit && written >= Chest.LegacyLimit) { break; }
            }

            return (int)bw.BaseStream.Position;
        }

        public static int SaveSigns(IList<Sign> signs, BinaryWriter bw, bool useLegacyLimit = false)
        {
            Int16 count = useLegacyLimit ? (Int16)Math.Min(signs.Count, Sign.LegacyLimit) : (Int16)signs.Count;
            bw.Write(count);

            int written = 0;
            foreach (Sign sign in signs)
            {
                if (sign.Text != null)
                {
                    bw.Write(sign.Text);
                    bw.Write(sign.X);
                    bw.Write(sign.Y);
                }
                written++;
                if (useLegacyLimit && written >= Sign.LegacyLimit) { break; }
            }

            return (int)bw.BaseStream.Position;
        }

        public static int SaveNPCs(IEnumerable<NPC> npcs, BinaryWriter bw, int version)
        {
            foreach (NPC npc in npcs)
            {
                bw.Write(true);

                if (version >= 190)
                {
                    bw.Write(npc.SpriteId);
                }
                else
                {
                    bw.Write(NpcNames[npc.SpriteId]);
                }

                bw.Write(npc.DisplayName);
                bw.Write(npc.Position.X);
                bw.Write(npc.Position.Y);
                bw.Write(npc.IsHomeless);
                bw.Write(npc.Home.X);
                bw.Write(npc.Home.Y);

                if (version >= 213)
                {
                    BitsByte bitsByte = 0;
                    bitsByte[0] = true;
                    bw.Write(bitsByte);
                    if (bitsByte[0])
                    {
                        bw.Write(npc.TownNpcVariationIndex);
                    }
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

        public static int SaveMobs(IEnumerable<NPC> mobs, BinaryWriter bw, int version)
        {
            foreach (NPC mob in mobs)
            {
                bw.Write(true);
                if (version >= 190)
                {
                    bw.Write(mob.SpriteId);
                }
                else
                {
                    bw.Write(NpcNames[mob.SpriteId]);
                }
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

        public static int SaveSectionHeader(World world, BinaryWriter bw, bool[] tileFrameImportant, TextWriter debugger = null)
        {
            bw.Write(world.Version);
            debugger?.WriteLine("\"Version\": {0},", world.Version);

            bw.Write((UInt64)0x026369676f6c6572ul);
            debugger?.WriteLine("\"Magic Word\": {0},", (UInt64)0x026369676f6c6572ul);


            bw.Write((int)world.FileRevision + 1);
            debugger?.WriteLine("\"FileRevision\": {0},", world.FileRevision);


            bw.Write(Convert.ToUInt64(world.IsFavorite));
            debugger?.WriteLine("\"IsFavorite\": {0},", world.IsFavorite);



            short sections = world.GetSectionCount();
            bw.Write(sections);
            debugger?.WriteLine("\"Sections\": {0},", sections);


            // write section pointer placeholders
            for (int i = 0; i < sections; i++)
            {
                bw.Write(0);
            }

            // write bitpacked tile frame importance           
            WriteBitArray(bw, tileFrameImportant);

            if (debugger != null)
            {
                debugger?.Write("\"TileFrameImportant\":[ ");
                for (int i = 0; i < tileFrameImportant.Length; i++)
                {
                    if (tileFrameImportant[i])
                    {
                        debugger?.Write(i);
                        debugger?.Write(',');
                    }
                }
                debugger?.WriteLine("],");

            }

            debugger?.WriteLine("\"SECTION_0\": {0},", (int)bw.BaseStream.Position);

            return (int)bw.BaseStream.Position;
        }

        public static int SaveHeaderFlags(World world, BinaryWriter bw, int version, TextWriter debugger = null)
        {
            bw.Write(world.Title);
            debugger?.WriteLine("\"Title\": {0},", world.Title);

            if (world.Version >= 179)
            {
                if (world.Version == 179)
                {
                    int.TryParse(world.Seed, out var seed);
                    bw.Write(seed);
                    debugger?.WriteLine("\"Seed\": {0},", seed);
                }
                else
                {
                    bw.Write(world.Seed);
                    debugger?.WriteLine("\"Seed\": {0},", world.Seed);
                }

                bw.Write(world.WorldGenVersion);
                debugger?.WriteLine("\"WorldGenVersion\": {0},", world.WorldGenVersion);

            }

            if (world.Version >= 181)
            {
                bw.Write(world.Guid.ToByteArray());
                debugger?.WriteLine("\"Guid\": \"{0}\",", world.Guid);

            }

            bw.Write(world.WorldId);
            debugger?.WriteLine("\"WorldId\": {0},", world.WorldId);

            bw.Write((int)world.LeftWorld);
            debugger?.WriteLine("\"LeftWorld\": {0},", world.LeftWorld);

            bw.Write((int)world.RightWorld);
            debugger?.WriteLine("\"RightWorld\": {0},", world.RightWorld);

            bw.Write((int)world.TopWorld);
            debugger?.WriteLine("\"TopWorld\": {0},", world.TopWorld);

            bw.Write((int)world.BottomWorld);
            debugger?.WriteLine("\"BottomWorld\": {0},", world.BottomWorld);

            bw.Write(world.TilesHigh);
            debugger?.WriteLine("\"TilesHigh\": {0},", world.TilesHigh);

            bw.Write(world.TilesWide);
            debugger?.WriteLine("\"TilesWide\": {0},", world.TilesWide);


            if (world.Version >= 209)
            {

                bw.Write(world.GameMode);
                debugger?.WriteLine("\"GameMode\": {0},", world.GameMode);


                if (world.Version >= 222)
                {
                    bw.Write(world.DrunkWorld);
                    debugger?.WriteLine("\"DrunkWorld\": {0},", world.DrunkWorld);
                }
                if (world.Version >= 227)
                {
                    bw.Write(world.GoodWorld);
                    debugger?.WriteLine("\"GoodWorld\": {0},", world.GoodWorld);
                }
                if (world.Version >= 238)
                {
                    bw.Write(world.TenthAnniversaryWorld);
                    debugger?.WriteLine("\"TenthAnniversaryWorld\": {0},", world.TenthAnniversaryWorld);
                }
                if (world.Version >= 239)
                {
                    bw.Write(world.DontStarveWorld);
                    debugger?.WriteLine("\"DontStarveWorld\": {0},", world.DontStarveWorld);
                }
                if (world.Version >= 241)
                {
                    bw.Write(world.NotTheBeesWorld);
                    debugger?.WriteLine("\"NotTheBeesWorld\": {0},", world.NotTheBeesWorld);
                }
            }
            else if (world.Version == 208)
            {
                bw.Write((bool)(world.GameMode == 2)); // true = Master: 2, else false
                debugger?.WriteLine("\"Legacy Master\": {0},", world.GameMode == 2);

            }
            else if (world.Version >= 112)
            {
                bw.Write((bool)(world.GameMode == 1)); // true = expert: 1, else false
                debugger?.WriteLine("\"Legacy Expert\": {0},", world.GameMode == 1);

            }

            if (world.Version >= 141)
            {
                bw.Write(world.CreationTime);
                debugger?.WriteLine("\"CreationTime\": {0},", world.CreationTime);
            }

            // check if target moonType is over max
            if (world.MoonType > SaveConfiguration.SaveVersions[version].MaxMoonId)
            {
                // target is out of range, reset to zero
                bw.Write((byte)0);
                debugger?.WriteLine("\"Moon\": 0,");

            }
            else
            {
                bw.Write((byte)world.MoonType);
                debugger?.WriteLine("\"Moon\": {0},", world.MoonType);
            }
            bw.Write(world.TreeX0);
            debugger?.WriteLine("\"TreeX0\": {0},", world.TreeX0);

            bw.Write(world.TreeX1);
            debugger?.WriteLine("\"TreeX1\": {0},", world.TreeX1);
            bw.Write(world.TreeX2);
            debugger?.WriteLine("\"TreeX2\": {0},", world.TreeX2);
            bw.Write(world.TreeStyle0);
            debugger?.WriteLine("\"TreeStyle0\": {0},", world.TreeStyle0);
            bw.Write(world.TreeStyle1);
            debugger?.WriteLine("\"TreeStyle1\": {0},", world.TreeStyle1);
            bw.Write(world.TreeStyle2);
            debugger?.WriteLine("\"TreeStyle2\": {0},", world.TreeStyle2);
            bw.Write(world.TreeStyle3);
            debugger?.WriteLine("\"TreeStyle3\": {0},", world.TreeStyle3);
            bw.Write(world.CaveBackX0);
            debugger?.WriteLine("\"CaveBackX0\": {0},", world.CaveBackX0);
            bw.Write(world.CaveBackX1);
            debugger?.WriteLine("\"CaveBackX1\": {0},", world.CaveBackX1);
            bw.Write(world.CaveBackX2);
            debugger?.WriteLine("\"CaveBackX2\": {0},", world.CaveBackX2);
            bw.Write(world.CaveBackStyle0);
            debugger?.WriteLine("\"CaveBackStyle0\": {0},", world.CaveBackStyle0);
            bw.Write(world.CaveBackStyle1);
            debugger?.WriteLine("\"CaveBackStyle1\": {0},", world.CaveBackStyle1);
            bw.Write(world.CaveBackStyle2);
            debugger?.WriteLine("\"CaveBackStyle2\": {0},", world.CaveBackStyle2);
            bw.Write(world.CaveBackStyle3);
            debugger?.WriteLine("\"CaveBackStyle3\": {0},", world.CaveBackStyle3);
            bw.Write(world.IceBackStyle);
            debugger?.WriteLine("\"IceBackStyle\": {0},", world.IceBackStyle);
            bw.Write(world.JungleBackStyle);
            debugger?.WriteLine("\"JungleBackStyle\": {0},", world.JungleBackStyle);
            bw.Write(world.HellBackStyle);
            debugger?.WriteLine("\"HellBackStyle\": {0},", world.HellBackStyle);
            bw.Write(world.SpawnX);
            debugger?.WriteLine("\"SpawnX\": {0},", world.SpawnX);
            bw.Write(world.SpawnY);
            debugger?.WriteLine("\"SpawnY\": {0},", world.SpawnY);
            bw.Write(world.GroundLevel);
            debugger?.WriteLine("\"GroundLevel\": {0},", world.GroundLevel);
            bw.Write(world.RockLevel);
            debugger?.WriteLine("\"RockLevel\": {0},", world.RockLevel);
            bw.Write(world.Time);
            debugger?.WriteLine("\"Time\": {0},", world.Time);
            bw.Write(world.DayTime);
            debugger?.WriteLine("\"DayTime\": {0},", world.DayTime);
            bw.Write(world.MoonPhase);
            debugger?.WriteLine("\"MoonPhase\": {0},", world.MoonPhase);
            bw.Write(world.BloodMoon);
            debugger?.WriteLine("\"BloodMoon\": {0},", world.BloodMoon);
            bw.Write(world.IsEclipse);
            debugger?.WriteLine("\"IsEclipse\": {0},", world.IsEclipse);
            bw.Write(world.DungeonX);
            debugger?.WriteLine("\"DungeonX\": {0},", world.DungeonX);
            bw.Write(world.DungeonY);
            debugger?.WriteLine("\"DungeonY\": {0},", world.DungeonY);
            bw.Write(world.IsCrimson);
            debugger?.WriteLine("\"IsCrimson\": {0},", world.IsCrimson);
            bw.Write(world.DownedBoss1);
            debugger?.WriteLine("\"DownedBoss1\": {0},", world.DownedBoss1);
            bw.Write(world.DownedBoss2);
            debugger?.WriteLine("\"DownedBoss2\": {0},", world.DownedBoss2);
            bw.Write(world.DownedBoss3);
            debugger?.WriteLine("\"DownedBoss3\": {0},", world.DownedBoss3);
            bw.Write(world.DownedQueenBee);
            debugger?.WriteLine("\"DownedQueenBee\": {0},", world.DownedQueenBee);
            bw.Write(world.DownedMechBoss1);
            debugger?.WriteLine("\"DownedMechBoss1\": {0},", world.DownedMechBoss1);
            bw.Write(world.DownedMechBoss2);
            debugger?.WriteLine("\"DownedMechBoss2\": {0},", world.DownedMechBoss2);
            bw.Write(world.DownedMechBoss3);
            debugger?.WriteLine("\"DownedMechBoss3\": {0},", world.DownedMechBoss3);
            bw.Write(world.DownedMechBossAny);
            debugger?.WriteLine("\"DownedMechBossAny\": {0},", world.DownedMechBossAny);
            bw.Write(world.DownedPlantBoss);
            debugger?.WriteLine("\"DownedPlantBoss\": {0},", world.DownedPlantBoss);
            bw.Write(world.DownedGolemBoss);
            debugger?.WriteLine("\"DownedGolemBoss\": {0},", world.DownedGolemBoss);

            if (world.Version >= 147)
            {
                bw.Write(world.DownedSlimeKingBoss);
                debugger?.WriteLine("\"DownedSlimeKingBoss\": {0},", world.DownedSlimeKingBoss);
            }

            bw.Write(world.SavedGoblin);
            debugger?.WriteLine("\"SavedGoblin\": {0},", world.SavedGoblin);
            bw.Write(world.SavedWizard);
            debugger?.WriteLine("\"SavedWizard\": {0},", world.SavedWizard);
            bw.Write(world.SavedMech);
            debugger?.WriteLine("\"SavedMech\": {0},", world.SavedMech);
            bw.Write(world.DownedGoblins);
            debugger?.WriteLine("\"DownedGoblins\": {0},", world.DownedGoblins);
            bw.Write(world.DownedClown);
            debugger?.WriteLine("\"DownedClown\": {0},", world.DownedClown);
            bw.Write(world.DownedFrost);
            debugger?.WriteLine("\"DownedFrost\": {0},", world.DownedFrost);
            bw.Write(world.DownedPirates);
            debugger?.WriteLine("\"DownedPirates\": {0},", world.DownedPirates);
            bw.Write(world.ShadowOrbSmashed);
            debugger?.WriteLine("\"ShadowOrbSmashed\": {0},", world.ShadowOrbSmashed);
            bw.Write(world.SpawnMeteor);
            debugger?.WriteLine("\"SpawnMeteor\": {0},", world.SpawnMeteor);
            bw.Write((byte)world.ShadowOrbCount);
            debugger?.WriteLine("\"ShadowOrbCount\": {0},", world.ShadowOrbCount);

            bw.Write(world.AltarCount);
            debugger?.WriteLine("\"AltarCount\": {0},", world.AltarCount);
            bw.Write(world.HardMode);
            debugger?.WriteLine("\"HardMode\": {0},", world.HardMode);
            bw.Write(world.InvasionDelay);
            debugger?.WriteLine("\"InvasionDelay\": {0},", world.InvasionDelay);
            bw.Write(world.InvasionSize);
            debugger?.WriteLine("\"InvasionSize\": {0},", world.InvasionSize);
            bw.Write(world.InvasionType);
            debugger?.WriteLine("\"InvasionType\": {0},", world.InvasionType);
            bw.Write(world.InvasionX);
            debugger?.WriteLine("\"InvasionX\": {0},", world.InvasionX);

            if (world.Version >= 147)
            {
                bw.Write(world.SlimeRainTime);
                debugger?.WriteLine("\"SlimeRainTime\": {0},", world.SlimeRainTime);
                bw.Write((byte)world.SundialCooldown);
                debugger?.WriteLine("\"SundialCooldown\": {0},", world.SundialCooldown);

            }

            bw.Write(world.TempRaining);
            debugger?.WriteLine("\"TempRaining\": {0},", world.TempRaining);
            bw.Write(world.TempRainTime);
            debugger?.WriteLine("\"TempRainTime\": {0},", world.TempRainTime);
            bw.Write(world.TempMaxRain);
            debugger?.WriteLine("\"TempMaxRain\": {0},", world.TempMaxRain);
            bw.Write(world.SavedOreTiersCobalt);
            debugger?.WriteLine("\"SavedOreTiersCobalt\": {0},", world.SavedOreTiersCobalt);
            bw.Write(world.SavedOreTiersMythril);
            debugger?.WriteLine("\"SavedOreTiersMythril\": {0},", world.SavedOreTiersMythril);
            bw.Write(world.SavedOreTiersAdamantite);
            debugger?.WriteLine("\"SavedOreTiersAdamantite\": {0},", world.SavedOreTiersAdamantite);
            bw.Write(world.BgTree);
            debugger?.WriteLine("\"BgTree\": {0},", world.BgTree);
            bw.Write(world.BgCorruption);
            debugger?.WriteLine("\"BgCorruption\": {0},", world.BgCorruption);
            bw.Write(world.BgJungle);
            debugger?.WriteLine("\"BgJungle\": {0},", world.BgJungle);
            bw.Write(world.BgSnow);
            debugger?.WriteLine("\"BgSnow\": {0},", world.BgSnow);
            bw.Write(world.BgHallow);
            debugger?.WriteLine("\"BgHallow\": {0},", world.BgHallow);
            bw.Write(world.BgCrimson);
            debugger?.WriteLine("\"BgCrimson\": {0},", world.BgCrimson);
            bw.Write(world.BgDesert);
            debugger?.WriteLine("\"BgDesert\": {0},", world.BgDesert);
            bw.Write(world.BgOcean);
            debugger?.WriteLine("\"BgOcean\": {0},", world.BgOcean);
            bw.Write((int)world.CloudBgActive);
            debugger?.WriteLine("\"CloudBgActive\": {0},", world.CloudBgActive);

            bw.Write(world.NumClouds);
            debugger?.WriteLine("\"NumClouds\": {0},", world.NumClouds);
            bw.Write(world.WindSpeedSet);
            debugger?.WriteLine("\"WindSpeedSet\": {0},", world.WindSpeedSet);

            if (world.Version >= 95)
            {
                bw.Write(world.Anglers.Count);
                debugger?.WriteLine("\"Anglers\": [", world.Anglers.Count);


                foreach (string angler in world.Anglers)
                {
                    bw.Write(angler);
                    debugger?.Write("{0},", angler);

                }

                debugger?.WriteLine("],");
            }

            if (world.Version < 99)
            {
                debugger?.WriteLine("\"SECTION_1\": {0}", bw.BaseStream.Position);
                return (int)bw.BaseStream.Position;
            }

            bw.Write(world.SavedAngler);
            debugger?.WriteLine("\"SavedAngler\": {0},", world.SavedAngler);

            if (world.Version < 101)
            {
                debugger?.WriteLine("\"SECTION_1\": {0}", bw.BaseStream.Position);
                return (int)bw.BaseStream.Position;
            }

            bw.Write(world.AnglerQuest);
            debugger?.WriteLine("\"AnglerQuest\": {0},", world.AnglerQuest);

            if (world.Version < 104)
            {
                debugger?.WriteLine("\"SECTION_1\": {0}", bw.BaseStream.Position);
                return (int)bw.BaseStream.Position;
            }

            bw.Write(world.SavedStylist);
            debugger?.WriteLine("\"SavedStylist\": {0},", world.SavedStylist);
            if (world.Version >= 129)
            {
                bw.Write(world.SavedTaxCollector);
                debugger?.WriteLine("\"SavedTaxCollector\": {0},", world.SavedTaxCollector);
            }
            if (world.Version >= 201)
            {
                bw.Write(world.SavedGolfer);
                debugger?.WriteLine("\"SavedGolfer\": {0},", world.SavedGolfer);
            }
            if (world.Version >= 107)
            {
                bw.Write(world.InvasionSizeStart);
                debugger?.WriteLine("\"InvasionSizeStart\": {0},", world.InvasionSizeStart);
            }

            if (world.Version >= 108)
            {
                bw.Write(world.CultistDelay);
                debugger?.WriteLine("\"CultistDelay\": {0},", world.CultistDelay);
            }

            bw.Write((short)KillTallyMax);
            debugger?.WriteLine("\"KillTallyMax\": {0},", KillTallyMax);

            debugger?.Write("\"KillTally\": [");

            for (int i = 0; i < KillTallyMax; i++)
            {
                if (world.KilledMobs.Count > i)
                {
                    bw.Write(world.KilledMobs[i]);
                    debugger?.Write("{0},", world.KilledMobs[i]);
                }
                else
                {
                    bw.Write(0);
                    debugger?.Write("0,");
                }
            }
            debugger?.WriteLine("],");

            bw.Write(world.FastForwardTime);
            debugger?.WriteLine("\"FastForwardTime\": {0},", world.FastForwardTime);
            bw.Write(world.DownedFishron);
            debugger?.WriteLine("\"DownedFishron\": {0},", world.DownedFishron);
            bw.Write(world.DownedMartians);
            debugger?.WriteLine("\"DownedMartians\": {0},", world.DownedMartians);
            bw.Write(world.DownedLunaticCultist);
            debugger?.WriteLine("\"DownedLunaticCultist\": {0},", world.DownedLunaticCultist);
            bw.Write(world.DownedMoonlord);
            debugger?.WriteLine("\"DownedMoonlord\": {0},", world.DownedMoonlord);
            bw.Write(world.DownedHalloweenKing);
            debugger?.WriteLine("\"DownedHalloweenKing\": {0},", world.DownedHalloweenKing);
            bw.Write(world.DownedHalloweenTree);
            debugger?.WriteLine("\"DownedHalloweenTree\": {0},", world.DownedHalloweenTree);
            bw.Write(world.DownedChristmasQueen);
            debugger?.WriteLine("\"DownedChristmasQueen\": {0},", world.DownedChristmasQueen);
            bw.Write(world.DownedSanta);
            debugger?.WriteLine("\"DownedSanta\": {0},", world.DownedSanta);
            bw.Write(world.DownedChristmasTree);
            debugger?.WriteLine("\"DownedChristmasTree\": {0},", world.DownedChristmasTree);
            bw.Write(world.DownedCelestialSolar);
            debugger?.WriteLine("\"DownedCelestialSolar\": {0},", world.DownedCelestialSolar);
            bw.Write(world.DownedCelestialVortex);
            debugger?.WriteLine("\"DownedCelestialVortex\": {0},", world.DownedCelestialVortex);
            bw.Write(world.DownedCelestialNebula);
            debugger?.WriteLine("\"DownedCelestialNebula\": {0},", world.DownedCelestialNebula);
            bw.Write(world.DownedCelestialStardust);
            debugger?.WriteLine("\"DownedCelestialStardust\": {0},", world.DownedCelestialStardust);
            bw.Write(world.CelestialSolarActive);
            debugger?.WriteLine("\"CelestialSolarActive\": {0},", world.CelestialSolarActive);
            bw.Write(world.CelestialVortexActive);
            debugger?.WriteLine("\"CelestialVortexActive\": {0},", world.CelestialVortexActive);
            bw.Write(world.CelestialNebulaActive);
            debugger?.WriteLine("\"CelestialNebulaActive\": {0},", world.CelestialNebulaActive);
            bw.Write(world.CelestialStardustActive);
            debugger?.WriteLine("\"CelestialStardustActive\": {0},", world.CelestialStardustActive);
            bw.Write(world.Apocalypse);
            debugger?.WriteLine("\"Apocalypse\": {0},", world.Apocalypse);

            if (world.Version >= 170)
            {
                bw.Write(world.PartyManual);
                debugger?.WriteLine("\"PartyManual\": {0},", world.PartyManual);
                bw.Write(world.PartyGenuine);
                debugger?.WriteLine("\"PartyGenuine\": {0},", world.PartyGenuine);
                bw.Write(world.PartyCooldown);
                debugger?.WriteLine("\"PartyCooldown\": {0},", world.PartyCooldown);

                bw.Write(world.PartyingNPCs.Count);
                debugger?.Write("\"PartyingNPCs\": [", world.PartyingNPCs.Count);

                foreach (int partier in world.PartyingNPCs)
                {
                    bw.Write(partier);
                    debugger?.Write("{0},", partier);

                }

                debugger?.WriteLine("],");
            }
            if (world.Version >= 174)
            {
                bw.Write(world.SandStormHappening);
                debugger?.WriteLine("\"SandStormHappening\": {0},", world.SandStormHappening);
                bw.Write(world.SandStormTimeLeft);
                debugger?.WriteLine("\"SandStormTimeLeft\": {0},", world.SandStormTimeLeft);
                bw.Write(world.SandStormSeverity);
                debugger?.WriteLine("\"SandStormSeverity\": {0},", world.SandStormSeverity);
                bw.Write(world.SandStormIntendedSeverity);
                debugger?.WriteLine("\"SandStormIntendedSeverity\": {0},", world.SandStormIntendedSeverity);
            }
            if (world.Version >= 178)
            {
                bw.Write(world.SavedBartender);
                debugger?.WriteLine("\"SavedBartender\": {0},", world.SavedBartender);
                bw.Write(world.DownedDD2InvasionT1);
                debugger?.WriteLine("\"DownedDD2InvasionT1\": {0},", world.DownedDD2InvasionT1);
                bw.Write(world.DownedDD2InvasionT2);
                debugger?.WriteLine("\"DownedDD2InvasionT2\": {0},", world.DownedDD2InvasionT2);
                bw.Write(world.DownedDD2InvasionT3);
                debugger?.WriteLine("\"DownedDD2InvasionT3\": {0},", world.DownedDD2InvasionT3);
            }

            // 1.4 Journey's End
            if (world.Version > 194)
            {
                bw.Write((byte)world.MushroomBg);
                debugger?.WriteLine("\"MushroomBg\": {0},", world.MushroomBg);

            }
            if (world.Version >= 215)
            {
                bw.Write((byte)world.UnderworldBg);
                debugger?.WriteLine("\"UnderworldBg\": {0},", world.UnderworldBg);

            }
            if (world.Version >= 195)
            {
                bw.Write((byte)world.BgTree2);
                debugger?.WriteLine("\"BgTree2\": {0},", world.BgTree2);

                bw.Write((byte)world.BgTree3);
                debugger?.WriteLine("\"BgTree3\": {0},", world.BgTree3);

                bw.Write((byte)world.BgTree4);
                debugger?.WriteLine("\"BgTree4\": {0},", world.BgTree4);

            }
            if (world.Version >= 204)
            {
                bw.Write(world.CombatBookUsed);
                debugger?.WriteLine("\"CombatBookUsed\": {0},", world.CombatBookUsed);
            }
            if (world.Version >= 207)
            {
                bw.Write(world.TempLanternNightCooldown);
                debugger?.WriteLine("\"TempLanternNightCooldown\": {0},", world.TempLanternNightCooldown);
                bw.Write(world.TempLanternNightGenuine);
                debugger?.WriteLine("\"TempLanternNightGenuine\": {0},", world.TempLanternNightGenuine);
                bw.Write(world.TempLanternNightManual);
                debugger?.WriteLine("\"TempLanternNightManual\": {0},", world.TempLanternNightManual);
                bw.Write(world.TempLanternNightNextNightIsGenuine);
                debugger?.WriteLine("\"TempLanternNightNextNightIsGenuine\": {0},", world.TempLanternNightNextNightIsGenuine);
            }
            if (world.Version >= 211)
            {
                // tree tops
                bw.Write(world.TreeTopVariations.Count);
                debugger?.Write("\"TreeTopVariations\": [", world.TreeTopVariations.Count);

                for (int i = 0; i < world.TreeTopVariations.Count; i++)
                {
                    bw.Write(world.TreeTopVariations[i]);
                    debugger?.Write("{0},", world.TreeTopVariations[i]);
                }
                debugger?.WriteLine("],");
            }
            if (world.Version >= 212)
            {
                bw.Write(world.ForceHalloweenForToday);
                debugger?.WriteLine("\"ForceHalloweenForToday\": {0},", world.ForceHalloweenForToday);
                bw.Write(world.ForceXMasForToday);
                debugger?.WriteLine("\"ForceXMasForToday\": {0},", world.ForceXMasForToday);
            }

            if (world.Version >= 216)
            {
                bw.Write(world.SavedOreTiersCopper);
                debugger?.WriteLine("\"SavedOreTiersCopper\": {0},", world.SavedOreTiersCopper);
                bw.Write(world.SavedOreTiersIron);
                debugger?.WriteLine("\"SavedOreTiersIron\": {0},", world.SavedOreTiersIron);
                bw.Write(world.SavedOreTiersSilver);
                debugger?.WriteLine("\"SavedOreTiersSilver\": {0},", world.SavedOreTiersSilver);
                bw.Write(world.SavedOreTiersGold);
                debugger?.WriteLine("\"SavedOreTiersGold\": {0},", world.SavedOreTiersGold);
            }

            if (world.Version >= 217)
            {
                bw.Write(world.BoughtCat);
                debugger?.WriteLine("\"BoughtCat\": {0},", world.BoughtCat);
                bw.Write(world.BoughtDog);
                debugger?.WriteLine("\"BoughtDog\": {0},", world.BoughtDog);
                bw.Write(world.BoughtBunny);
                debugger?.WriteLine("\"BoughtBunny\": {0},", world.BoughtBunny);
            }

            if (world.Version >= 223)
            {
                bw.Write(world.DownedEmpressOfLight);
                debugger?.WriteLine("\"DownedEmpressOfLight\": {0},", world.DownedEmpressOfLight);
                bw.Write(world.DownedQueenSlime);
                debugger?.WriteLine("\"DownedQueenSlime\": {0},", world.DownedQueenSlime);
            }

            if (world.Version >= 240)
            {
                bw.Write(world.DownedDeerclops);
                debugger?.WriteLine("\"DownedDeerclops\": {0},", world.DownedDeerclops);
            }


            // unknown flags from data file
            if (world.UnknownData != null && world.UnknownData.Length > 0)
            {
                bw.Write(world.UnknownData);
                debugger?.WriteLine("\"UnknownData\": {0},", string.Join("", world.UnknownData));
            }

            debugger?.WriteLine("\"SECTION_1\": {0}", bw.BaseStream.Position);
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

        public static void LoadV2(BinaryReader b, World w)
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

            w.TileFrameImportant = tileFrameImportant;

            // we should be at the end of the first section
            if (b.BaseStream.Position != sectionPointers[0])
                throw new FileFormatException("Unexpected Position: Invalid File Format Section");

            // Load the flags
            LoadHeaderFlags(b, w, sectionPointers[1]);
            if (b.BaseStream.Position != sectionPointers[1])
                throw new FileFormatException("Unexpected Position: Invalid Header Flags");

            OnProgressChanged(null, new ProgressChangedEventArgs(0, "Loading Tiles..."));
            w.Tiles = LoadTileData(b, w.TilesWide, w.TilesHigh, (int)w.Version, w.TileFrameImportant);

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

        public static Tile[,] LoadTileData(BinaryReader r, int maxX, int maxY, int version, bool[] tileFrameImportant)
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
                        Tile tile = DeserializeTileData(r, tileFrameImportant, version, out rle);


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
                    catch (Exception)
                    {
                        // forcing some recovery here

                        for (int x2 = 0; x2 < maxX; x2++)
                        {
                            for (int y2 = 0; y2 < maxY; y2++)
                            {
                                if (tiles[x2, y2] == null) tiles[x2, y2] = new Tile();
                            }
                        }
                        return tiles;
                    }
                }
            }

            return tiles;
        }

        public static Tile DeserializeTileData(BinaryReader r, bool[] tileFrameImportant, int version, out int rle)
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
                if (!tileFrameImportant[tileType])
                {
                    tile.U = 0;//-1;
                    tile.V = 0;//-1;
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
            {
                w.Guid = Guid.NewGuid();
            }
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

                if (w.Version >= 222) { w.DrunkWorld = r.ReadBoolean(); }
                if (w.Version >= 227) { w.GoodWorld = r.ReadBoolean(); }
                if (w.Version >= 238) { w.TenthAnniversaryWorld = r.ReadBoolean(); }
                if (w.Version >= 239) { w.DontStarveWorld = r.ReadBoolean(); }
                if (w.Version >= 241) { w.NotTheBeesWorld = r.ReadBoolean(); }
            }
            else if (w.Version == 208)
            {
                w.GameMode = r.ReadBoolean() ? 2 : 0;
            }
            else if (w.Version >= 112)
            {
                w.GameMode = r.ReadBoolean() ? 1 : 0;
            }
            else
            {
                w.GameMode = 0;
            }

            w.CreationTime = w.Version >= 141 ? r.ReadInt64() : DateTime.Now.ToBinary();

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

            if (w.Version >= 147) { w.DownedSlimeKingBoss = r.ReadBoolean(); }

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

            if (w.Version < 99) { return; }

            w.SavedAngler = r.ReadBoolean();


            if (w.Version < 101) { return; }
            w.AnglerQuest = r.ReadInt32();


            if (w.Version < 104) { return; }


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

            if (w.Version >= 240)
            {
                w.DownedDeerclops = r.ReadBoolean();
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
            int sectionCount = r.ReadInt16();
            sectionPointers = new int[sectionCount];
            for (int i = 0; i < sectionCount; i++)
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
