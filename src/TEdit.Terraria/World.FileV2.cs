using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TEdit.Common.Exceptions;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Helper;
using TEdit.Utility;

namespace TEdit.Terraria;

public partial class World
{
    public bool IsTModLoader { get; set; }

    public short GetSectionCount() => ((int)Version >= 220) ? (short)11 : (short)10;

    public bool[] TileFrameImportant { get; set; }

    public static void ImportKillsAndBestiary(World world, string worldFileName, IProgress<ProgressChangedEventArgs>? progress = null)
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
                throw new TEditFileFormatException("World file too old, please update it by loading in game.");
            if (w.Version > world.Version)
                throw new TEditFileFormatException("Source world version is greater than target world. Please reload both in game and resave");

            // reset the stream
            b.BaseStream.Position = (long)0;

            progress?.Report(new ProgressChangedEventArgs(0, "Loading File Header..."));
            // read section pointers and tile frame data
            if (!LoadSectionHeader(b, out tileFrameImportant, out sectionPointers, w))
                throw new TEditFileFormatException("Invalid File Format Section");

            w.TileFrameImportant = tileFrameImportant;

            // we should be at the end of the first section
            if (b.BaseStream.Position != sectionPointers[0])
                throw new TEditFileFormatException("Unexpected Position: Invalid File Format Section");

            // Load the flags
            LoadHeaderFlags(b, w, sectionPointers[1]);
            if (b.BaseStream.Position != sectionPointers[1])
                throw new TEditFileFormatException("Unexpected Position: Invalid Header Flags");

            if (w.Version >= 210 && sectionPointers.Length > 9)
            {
                // skip to bestiary data
                b.BaseStream.Position = sectionPointers[8];
                LoadBestiary(b, w);
                if (b.BaseStream.Position != sectionPointers[9])
                    throw new TEditFileFormatException("Unexpected Position: Invalid Bestiary Section");
            }
        }

        // copy kill tally and bestiary to target world
        world.Bestiary = w.Bestiary;
        world.KilledMobs.Clear();
        world.KilledMobs.AddRange(w.KilledMobs);
    }

    public static void SaveV2(World world, BinaryWriter bw, bool incrementRevision = true, IProgress<ProgressChangedEventArgs>? progress = null)
    {
        world.Validate(progress);

        if (incrementRevision)
        {
            world.FileRevision++;
        }

        int[] sectionPointers = new int[world.GetSectionCount()];
        bool[] tileFrameImportant = WorldConfiguration.SaveConfiguration.GetTileFramesForVersion((int)world.Version);

        progress?.Report(new ProgressChangedEventArgs(0, "Save headers..."));
        sectionPointers[0] = SaveSectionHeader(world, bw, tileFrameImportant);
        sectionPointers[1] = SaveHeaderFlags(world, bw, (int)world.Version);
        progress?.Report(new ProgressChangedEventArgs(0, "Save Tiles..."));
        sectionPointers[2] = SaveTiles(world.Tiles, (int)world.Version, world.TilesWide, world.TilesHigh, bw, tileFrameImportant);

        progress?.Report(new ProgressChangedEventArgs(91, "Save Chests..."));
        sectionPointers[3] = SaveChests(world.Chests, bw, (int)world.Version);
        progress?.Report(new ProgressChangedEventArgs(92, "Save Signs..."));
        sectionPointers[4] = SaveSigns(world.Signs, bw, (int)world.Version);
        progress?.Report(new ProgressChangedEventArgs(93, "Save NPCs..."));

        sectionPointers[5] = SaveNPCs(world, bw, (int)world.Version);

        if (world.Version >= 140)
        {
            progress?.Report(new ProgressChangedEventArgs(94, "Save Mobs..."));
            sectionPointers[5] = SaveMobs(world.Mobs, bw, (int)world.Version);

            progress?.Report(new ProgressChangedEventArgs(95, "Save Tile Entities Section..."));
            sectionPointers[6] = SaveTileEntities(world.TileEntities, bw);
        }

        if (world.Version >= 170)
        {
            progress?.Report(new ProgressChangedEventArgs(96, "Save Weighted Pressure Plates..."));
            sectionPointers[7] = SavePressurePlate(world.PressurePlates, bw);
        }

        if (world.Version >= 189)
        {
            progress?.Report(new ProgressChangedEventArgs(97, "Save Town Manager..."));
            sectionPointers[8] = SaveTownManager(world.PlayerRooms, bw, (int)world.Version);
        }

        if (world.Version >= 210)
        {
            progress?.Report(new ProgressChangedEventArgs(98, "Save Bestiary..."));
            sectionPointers[9] = SaveBestiary(world.Bestiary, bw);
        }

        if (world.Version >= 220)
        {
            progress?.Report(new ProgressChangedEventArgs(99, "Save Creative Powers..."));
            sectionPointers[10] = SaveCreativePowers(world.CreativePowers, bw);
        }

        progress?.Report(new ProgressChangedEventArgs(100, "Save Footers..."));
        SaveFooter(world, bw);
        UpdateSectionPointers(world.Version, sectionPointers, bw);
        progress?.Report(new ProgressChangedEventArgs(100, "Save Complete."));
    }

    public static int SaveTiles(Tile[,] tiles, int version, int maxX, int maxY, BinaryWriter bw, bool[] tileFrameImportant, IProgress<ProgressChangedEventArgs>? progress = null)
    {

        int maxTileId = WorldConfiguration.SaveConfiguration.GetData(version).MaxTileId;
        int maxWallId = WorldConfiguration.SaveConfiguration.GetData(version).MaxWallId;

        for (int x = 0; x < maxX; x++)
        {
            progress?.Report(new ProgressChangedEventArgs((int)(x.ProgressPercentage(maxX) * 0.9), "Saving Tiles..."));


            for (int y = 0; y < maxY; y++)
            {
                Tile tile = tiles[x, y];

                int dataIndex;
                int headerIndex;

                byte[] tileData = SerializeTileData(tile, version, maxTileId, maxWallId, tileFrameImportant, out dataIndex, out headerIndex);

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


                bw.Write(tileData, headerIndex, dataIndex - headerIndex);
            }
        }

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
        out int headerIndex)
    {
        int size = version switch
        {
            int v when v >= 269 => 16, // 1.4.4+
            int v when v > 222 => 15, // 1.4.0+
            _ => 13 // default
        };

        byte[] tileData = new byte[size];
        dataIndex = (version >= 269) ? 4 : 3; // 1.4.4+

        byte header4 = (byte)0;
        byte header3 = (byte)0;
        byte header2 = (byte)0;
        byte header1 = (byte)0;

        // tile data

        if (tile.IsActive && tile.Type <= maxTileId && tile.Type != (int)TileType.IceByRod)
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


            if (tileFrameImportant[tile.Type])
            {
                // pack UV coords
                tileData[dataIndex++] = (byte)(tile.U & 0xFF); // low byte
                tileData[dataIndex++] = (byte)((tile.U & 0xFF00) >> 8); // high byte
                tileData[dataIndex++] = (byte)(tile.V & 0xFF); // low byte
                tileData[dataIndex++] = (byte)((tile.V & 0xFF00) >> 8); // high byte

            }

            if (version < 269)
            {
                if (tile.TileColor != 0 || tile.FullBrightBlock)
                {

                    var color = tile.TileColor;

                    // downgraded illuminate coating to illuminate paint
                    // IF there is no other paint
                    if (color == 0 && tile.FullBrightBlock)
                    {
                        color = 31;
                    }

                    // set header3 bit[3] for tile color active
                    header3 |= 0b_0000_1000;
                    tileData[dataIndex++] = color;
                }
            }
            else
            {
                if (tile.TileColor != 0 && tile.TileColor != 31)
                {
                    var color = tile.TileColor;

                    // set header3 bit[3] for tile color active
                    header3 |= 0b_0000_1000;
                    tileData[dataIndex++] = color;
                }
            }


        }

        // wall data
        if (tile.Wall != 0 && tile.Wall <= maxWallId)
        {
            // set header1 bit[2] for wall active
            header1 |= 0b_0000_0100;
            tileData[dataIndex++] = (byte)tile.Wall;

            // save tile wall color
            if (version < 269)
            {
                if (tile.WallColor != 0 || tile.FullBrightWall)
                {
                    var color = tile.WallColor;

                    // downgraded illuminate coating to illuminate paint
                    // IF there is no other paint
                    if (color == 0 && version < 269 && tile.FullBrightWall)
                    {
                        color = 31;
                    }

                    // set header3 bit[4] for wall color active
                    header3 |= 0b_0001_0000;
                    tileData[dataIndex++] = color;
                }
            }
            else
            {
                // for versions >= 269 upgrade illuminant paint to coating
                if (tile.WallColor != 0 && tile.WallColor != 31)
                {
                    var color = tile.WallColor;
                    // set header3 bit[4] for wall color active
                    header3 |= 0b_0001_0000;
                    tileData[dataIndex++] = color;
                }
            }
        }

        // liquid data
        if (tile.LiquidAmount != 0 && tile.LiquidType != LiquidType.None)
        {
            if (version >= 269 && tile.LiquidType == LiquidType.Shimmer)
            {
                // shimmer (v 1.4.4 +)
                header3 = (byte)(header3 | (byte)0b_1000_0000);
                header1 = (byte)(header1 | (byte)0b_0000_1000);
            }
            else if (tile.LiquidType == LiquidType.Lava)
            {
                // lava
                header1 = (byte)(header1 | (byte)0b_0001_0000);
            }
            else if (tile.LiquidType == LiquidType.Honey)
            {
                // honey
                header1 = (byte)(header1 | (byte)0b_0001_1000);
            }
            else
            {
                // water
                header1 = (byte)(header1 | (byte)0b_0000_1000);
            }

            tileData[dataIndex++] = tile.LiquidAmount;

        }

        // wire data
        if (tile.WireRed)
        {
            // red wire = header2 bit[1]
            header2 |= 0b_0000_0010;

        }
        if (tile.WireBlue)
        {
            // blue wire = header2 bit[2]
            header2 |= 0b_0000_0100;

        }
        if (tile.WireGreen)
        {
            // green wire = header2 bit[3]
            header2 |= 0b_0000_1000;
        }

        // brick style
        byte brickStyle = (byte)((byte)tile.BrickStyle << 4);

        // set bits[4,5,6] of header2
        header2 = (byte)(header2 | brickStyle);


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

        if (version >= 269)
        {
            // custom block lighting (v1.4.4+)
            if (tile.InvisibleBlock)
            {
                header4 |= 0b_0000_0010;
            }
            if (tile.InvisibleWall)
            {
                header4 |= 0b_0000_0100;
            }
            if (tile.FullBrightBlock || tile.TileColor == 31) // convert illuminant paint
            {
                header4 |= 0b_0000_1000;
            }
            if (tile.FullBrightWall || tile.WallColor == 31) // convert illuminant paint
            {
                header4 |= 0b_0001_0000;
            }

            // header 4 only used in 1.4.4+
            headerIndex = 3;
            if (header4 != 0)
            {
                // set header4 active flag bit[0] of header3
                header3 |= 0b_0000_0001;
                tileData[headerIndex--] = header4;
            }
        }
        else
        {
            headerIndex = 2;
        }

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

    public static int SaveChests(IList<Chest> chests, BinaryWriter bw, int version)
    {
        bool useLegacyLimit = version < 216;
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
                if (item != null && item.NetId <= WorldConfiguration.SaveConfiguration.GetData(version).MaxItemId)
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

    public static int SaveSigns(IList<Sign> signs, BinaryWriter bw, int version)
    {
        bool useLegacyLimit = version < 216;
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

    public static int SaveNPCs(World world, BinaryWriter bw, int version)
    {
        var maxNPC = WorldConfiguration.SaveConfiguration.GetData(version).MaxNpcId;

        if (world.Version >= 268)
        {
            bw.Write((int)world.ShimmeredTownNPCs.Count);
            foreach (int npcID in world.ShimmeredTownNPCs)
            {
                bw.Write(npcID);
            }
        }

        foreach (NPC npc in world.NPCs)
        {
            if (npc.SpriteId > maxNPC) { break; }
            bw.Write(true);

            if (version >= 190)
            {
                bw.Write(npc.SpriteId);
            }
            else
            {
                bw.Write(WorldConfiguration.NpcNames[npc.SpriteId]);
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

    public static int SaveTownManager(IList<TownManager> rooms, BinaryWriter bw, int version)
    {
        var maxNPC = WorldConfiguration.SaveConfiguration.GetData(version).MaxNpcId;

        var validRoomsForVersion = rooms.Where(r => r.NpcId <= maxNPC).ToList();

        bw.Write(validRoomsForVersion.Count);
        foreach (TownManager room in validRoomsForVersion)
        {
            bw.Write(room.NpcId);
            bw.Write(room.Home.X);
            bw.Write(room.Home.Y);
        }
        return (int)bw.BaseStream.Position;
    }

    public static int SaveMobs(IEnumerable<NPC> mobs, BinaryWriter bw, int version)
    {
        var maxNPC = WorldConfiguration.SaveConfiguration.GetData(version).MaxNpcId;

        foreach (NPC mob in mobs)
        {
            if (mob.SpriteId > maxNPC) { break; }

            bw.Write(true);
            if (version >= 190)
            {
                bw.Write(mob.SpriteId);
            }
            else
            {
                bw.Write(WorldConfiguration.NpcNames[mob.SpriteId]);
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

    public static int UpdateSectionPointers(uint worldVersion, int[] sectionPointers, BinaryWriter bw)
    {
        bw.BaseStream.Position = 0;
        bw.Write((int)worldVersion);


        bw.BaseStream.Position = (worldVersion >= 140) ? 0x18L : 0x04;
        bw.Write((short)sectionPointers.Length);

        for (int i = 0; i < sectionPointers.Length; i++)
        {
            bw.Write(sectionPointers[i]);
        }

        return (int)bw.BaseStream.Position;
    }

    public static int SaveSectionHeader(World world, BinaryWriter bw, bool[] tileFrameImportant)
    {
        bw.Write(world.Version);

        // World features added in 1.3.0.1
        if (world.Version >= 140)
        {
            if (world.IsChinese)
            {
                bw.Write(WorldConfiguration.ChineseHeader.ToCharArray());
            }
            else
            {
                bw.Write(WorldConfiguration.DesktopHeader.ToCharArray());
            }
            bw.Write((byte)FileType.World);

            bw.Write((int)world.FileRevision);

            UInt64 worldHeaderFlags = 0;
            if (world.IsFavorite) { worldHeaderFlags |= 0x1; }

            bw.Write(worldHeaderFlags);
        }

        short sections = world.GetSectionCount();
        bw.Write(sections);

        // write section pointer placeholders
        for (int i = 0; i < sections; i++)
        {
            bw.Write((Int32)0);
        }

        // write bitpacked tile frame importance           
        WriteBitArray(bw, tileFrameImportant);



        return (int)bw.BaseStream.Position;
    }

    public static int SaveHeaderFlags(World world, BinaryWriter bw, int version)
    {
        bw.Write(world.Title);

        if (world.Version >= 179)
        {
            if (world.Version == 179)
            {
                int.TryParse(world.Seed, out var seed);
                bw.Write(seed);
            }
            else
            {
                bw.Write(world.Seed);
            }

            bw.Write(world.WorldGenVersion);

        }

        if (world.Version >= 181)
        {
            bw.Write(world.WorldGUID.ToByteArray());

        }

        bw.Write(world.WorldId);

        bw.Write((int)world.LeftWorld);

        bw.Write((int)world.RightWorld);

        bw.Write((int)world.TopWorld);

        bw.Write((int)world.BottomWorld);

        bw.Write(world.TilesHigh);

        bw.Write(world.TilesWide);


        if (world.Version >= 209)
        {

            bw.Write(world.GameMode);


            if (world.Version >= 222)
            {
                bw.Write(world.DrunkWorld);
            }
            if (world.Version >= 227)
            {
                bw.Write(world.GoodWorld);
            }
            if (world.Version >= 238)
            {
                bw.Write(world.TenthAnniversaryWorld);
            }
            if (world.Version >= 239)
            {
                bw.Write(world.DontStarveWorld);
            }
            if (world.Version >= 241)
            {
                bw.Write(world.NotTheBeesWorld);
            }
            if (world.Version >= 249)
            {
                bw.Write(world.RemixWorld);
            }
            if (world.Version >= 266)
            {
                bw.Write(world.NoTrapsWorld);
            }
            if (world.Version >= 266)
            {
                bw.Write(world.ZenithWorld);
            }
        }
        else if (world.Version == 208)
        {
            bw.Write((bool)(world.GameMode == 2)); // true = Master: 2, else false

        }
        else if (world.Version >= 112)
        {
            bw.Write((bool)(world.GameMode == 1)); // true = expert: 1, else false

        }

        if (world.Version >= 141)
        {
            bw.Write(world.CreationTime);
        }

        // check if target moonType is over max
        if (world.MoonType > WorldConfiguration.SaveConfiguration.GetData(version).MaxMoonId)
        {
            // target is out of range, reset to zero
            bw.Write((byte)0);

        }
        else
        {
            bw.Write((byte)world.MoonType);
        }
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
        bw.Write(world.DownedBoss1EyeofCthulhu);
        bw.Write(world.DownedBoss2EaterofWorlds);
        bw.Write(world.DownedBoss3Skeletron);
        bw.Write(world.DownedQueenBee);
        bw.Write(world.DownedMechBoss1TheDestroyer);
        bw.Write(world.DownedMechBoss2TheTwins);
        bw.Write(world.DownedMechBoss3SkeletronPrime);
        bw.Write(world.DownedMechBossAny);
        bw.Write(world.DownedPlantBoss);
        bw.Write(world.DownedGolemBoss);

        if (world.Version >= 118)
        {
            bw.Write(world.DownedSlimeKingBoss);
        }

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

        if (world.Version >= 257)
        {
            bw.Write(world.PartyOfDoom);
        }

        bw.Write(world.InvasionDelay);
        bw.Write(world.InvasionSize);
        bw.Write(world.InvasionType);
        bw.Write(world.InvasionX);

        if (world.Version >= 118)
        {
            bw.Write(world.SlimeRainTime);

        }

        if (world.Version >= 113)
        {
            bw.Write((byte)world.SundialCooldown);
        }

        bw.Write(world.IsRaining);
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

        if (world.Version < 95)
        {
            return (int)bw.BaseStream.Position;
        }

        bw.Write(world.Anglers.Count);


        foreach (string angler in world.Anglers)
        {
            bw.Write(angler);
        }



        if (world.Version < 99)
        {
            return (int)bw.BaseStream.Position;
        }

        bw.Write(world.SavedAngler);

        if (world.Version < 101)
        {
            return (int)bw.BaseStream.Position;
        }

        bw.Write(world.AnglerQuest);

        if (world.Version < 104)
        {
            return (int)bw.BaseStream.Position;
        }

        if (world.Version > 104)
        {
            bw.Write(world.SavedStylist);
        }

        if (world.Version >= 129)
        {
            bw.Write(world.SavedTaxCollector);
        }
        if (world.Version >= 201)
        {
            bw.Write(world.SavedGolfer);
        }
        if (world.Version >= 107)
        {
            bw.Write(world.InvasionSizeStart);
        }

        if (world.Version >= 108)
        {
            bw.Write(world.CultistDelay);
        }

        if (world.Version < 109)
        {
            return (int)bw.BaseStream.Position;
        }

        var maxNPCId = WorldConfiguration.SaveConfiguration.GetData(version).MaxNpcId;
        bw.Write((short)(maxNPCId + 1));
        for (int i = 0; i <= maxNPCId; i++)
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

        if (world.Version < 128)
        {
            return (int)bw.BaseStream.Position;
        }

        if (world.Version >= 140)
        {
            bw.Write(world.FastForwardTime);
        }

        if (world.Version < 131)
        {
            return (int)bw.BaseStream.Position;
        }

        bw.Write(world.DownedFishron);

        if (world.Version >= 140)
        {
            bw.Write(world.DownedMartians);
            bw.Write(world.DownedLunaticCultist);
            bw.Write(world.DownedMoonlord);
        }

        bw.Write(world.DownedHalloweenKing);
        bw.Write(world.DownedHalloweenTree);
        bw.Write(world.DownedChristmasQueen);

        if (world.Version < 140)
        {
            return (int)bw.BaseStream.Position;
        }

        bw.Write(world.DownedSanta);
        bw.Write(world.DownedChristmasTree);

        if (world.Version >= 140)
        {
            bw.Write(world.DownedCelestialSolar);
            bw.Write(world.DownedCelestialVortex);
            bw.Write(world.DownedCelestialNebula);
            bw.Write(world.DownedCelestialStardust);
            bw.Write(world.CelestialSolarActive);
            bw.Write(world.CelestialVortexActive);
            bw.Write(world.CelestialNebulaActive);
            bw.Write(world.CelestialStardustActive);
            bw.Write(world.Apocalypse);
        }

        if (world.Version >= 170)
        {
            bw.Write(world.PartyManual);
            bw.Write(world.PartyGenuine);
            bw.Write(world.PartyCooldown);

            bw.Write(world.PartyingNPCs.Count);

            foreach (int partier in world.PartyingNPCs)
            {
                bw.Write(partier);

            }

        }

        if (world.Version >= 174)
        {
            bw.Write(world.SandStormHappening);
            bw.Write(world.SandStormTimeLeft);
            bw.Write(world.SandStormSeverity);
            bw.Write(world.SandStormIntendedSeverity);
        }

        if (world.Version >= 178)
        {
            bw.Write(world.SavedBartender);
            bw.Write(world.DownedDD2InvasionT1);
            bw.Write(world.DownedDD2InvasionT2);
            bw.Write(world.DownedDD2InvasionT3);
        }

        // 1.4 Journey's End
        if (world.Version > 194)
        {
            bw.Write((byte)world.MushroomBg);

        }

        if (world.Version >= 215)
        {
            bw.Write((byte)world.UnderworldBg);

        }

        if (world.Version >= 195)
        {
            bw.Write((byte)world.BgTree2);

            bw.Write((byte)world.BgTree3);

            bw.Write((byte)world.BgTree4);

        }

        if (world.Version >= 204)
        {
            bw.Write(world.CombatBookUsed);
        }

        if (world.Version >= 207)
        {
            bw.Write(world.LanternNightCooldown);
            bw.Write(world.LanternNightGenuine);
            bw.Write(world.LanternNightManual);
            bw.Write(world.LanternNightNextNightIsGenuine);
        }

        if (world.Version >= 211)
        {
            // tree tops
            bw.Write(world.TreeTopVariations.Count);

            for (int i = 0; i < world.TreeTopVariations.Count; i++)
            {
                bw.Write(world.TreeTopVariations[i]);
            }
        }

        if (world.Version >= 212)
        {
            bw.Write(world.ForceHalloweenForToday);
            bw.Write(world.ForceXMasForToday);
        }

        if (world.Version >= 216)
        {
            bw.Write(world.SavedOreTiersCopper);
            bw.Write(world.SavedOreTiersIron);
            bw.Write(world.SavedOreTiersSilver);
            bw.Write(world.SavedOreTiersGold);
        }

        if (world.Version >= 217)
        {
            bw.Write(world.BoughtCat);
            bw.Write(world.BoughtDog);
            bw.Write(world.BoughtBunny);
        }

        if (world.Version >= 223)
        {
            bw.Write(world.DownedEmpressOfLight);
            bw.Write(world.DownedQueenSlime);
        }

        if (world.Version >= 240)
        {
            bw.Write(world.DownedDeerclops);
        }

        if (world.Version >= 250)
        {
            bw.Write(world.UnlockedSlimeBlueSpawn);
        }

        if (world.Version >= 251)
        {
            bw.Write(world.UnlockedMerchantSpawn);
            bw.Write(world.UnlockedDemolitionistSpawn);
            bw.Write(world.UnlockedPartyGirlSpawn);
            bw.Write(world.UnlockedDyeTraderSpawn);
            bw.Write(world.UnlockedTruffleSpawn);
            bw.Write(world.UnlockedArmsDealerSpawn);
            bw.Write(world.UnlockedNurseSpawn);
            bw.Write(world.UnlockedPrincessSpawn);
        }

        if (world.Version >= 259)
        {
            bw.Write(world.CombatBookVolumeTwoWasUsed);
        }

        if (world.Version >= 260)
        {
            bw.Write(world.PeddlersSatchelWasUsed);
        }

        if (world.Version >= 261)
        {
            bw.Write(world.UnlockedSlimeGreenSpawn);
            bw.Write(world.UnlockedSlimeOldSpawn);
            bw.Write(world.UnlockedSlimePurpleSpawn);
            bw.Write(world.UnlockedSlimeRainbowSpawn);
            bw.Write(world.UnlockedSlimeRedSpawn);
            bw.Write(world.UnlockedSlimeYellowSpawn);
            bw.Write(world.UnlockedSlimeCopperSpawn);
        }

        if (world.Version >= 264)
        {
            bw.Write(world.FastForwardTimeToDusk);
            bw.Write((byte)world.MoondialCooldown);
        }

        // unknown flags from data file
        if (world.UnknownData != null && world.UnknownData.Length > 0)
        {
            bw.Write(world.UnknownData);
        }

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

    public static void LoadV2(BinaryReader b, World w, bool headersOnly = false, IProgress<ProgressChangedEventArgs>? progress = null)
    {
        //throw new NotImplementedException("World Version > 87");

        bool[] tileFrameImportant;
        int[] sectionPointers;

        // reset the stream
        b.BaseStream.Position = (long)0;

        progress?.Report(new ProgressChangedEventArgs(0, "Loading File Header..."));
        // read section pointers and tile frame data
        if (!LoadSectionHeader(b, out tileFrameImportant, out sectionPointers, w))
            throw new TEditFileFormatException("Invalid File Format Section");

        w.TileFrameImportant = tileFrameImportant;

        // we should be at the end of the first section
        if (b.BaseStream.Position != sectionPointers[0])
            throw new TEditFileFormatException("Unexpected Position: Invalid File Format Section");

        // Load the flags
        LoadHeaderFlags(b, w, sectionPointers[1]);
        if (b.BaseStream.Position != sectionPointers[1])
            throw new TEditFileFormatException("Unexpected Position: Invalid Header Flags");

        if (headersOnly) { return; }

        progress?.Report(new ProgressChangedEventArgs(0, "Loading Tiles..."));
        w.Tiles = LoadTileData(b, w.TilesWide, w.TilesHigh, (int)w.Version, w.TileFrameImportant);

        if (b.BaseStream.Position != sectionPointers[2])
            b.BaseStream.Position = sectionPointers[2];
        //throw new FileFormatException("Unexpected Position: Invalid Tile Data");

        progress?.Report(new ProgressChangedEventArgs(100, "Loading Chests..."));

        foreach (Chest chest in LoadChestData(b))
        {
            //Tile tile = w.Tiles[chest.X, chest.Y];
            //if (tile.IsActive && (tile.Type == 55 || tile.Type == 85))
            {
                w.Chests.Add(chest);
            }
        }

        if (b.BaseStream.Position != sectionPointers[3])
            throw new TEditFileFormatException("Unexpected Position: Invalid Chest Data");

        progress?.Report(new ProgressChangedEventArgs(100, "Loading Signs..."));

        foreach (Sign sign in LoadSignData(b))
        {
            Tile tile = w.Tiles[sign.X, sign.Y];
            if (tile.IsActive && tile.IsSign())
            {
                w.Signs.Add(sign);
            }
        }

        if (b.BaseStream.Position != sectionPointers[4])
            throw new TEditFileFormatException("Unexpected Position: Invalid Sign Data");

        progress?.Report(new ProgressChangedEventArgs(100, "Loading NPCs..."));
        LoadNPCsData(b, w);
        if (w.Version >= 140)
        {
            progress?.Report(new ProgressChangedEventArgs(100, "Loading Mobs..."));
            LoadMobsData(b, w);
            if (b.BaseStream.Position != sectionPointers[5])
                throw new TEditFileFormatException("Unexpected Position: Invalid Mob and NPC Data");

            progress?.Report(new ProgressChangedEventArgs(100, "Loading Tile Entities Section..."));
            LoadTileEntities(b, w);
            if (b.BaseStream.Position != sectionPointers[6])
                throw new TEditFileFormatException("Unexpected Position: Invalid Tile Entities Section");
        }
        else
        {
            if (b.BaseStream.Position != sectionPointers[5])
                throw new TEditFileFormatException("Unexpected Position: Invalid NPC Data");
        }
        if (w.Version >= 170)
        {
            LoadPressurePlate(b, w);
            if (b.BaseStream.Position != sectionPointers[7])
                throw new TEditFileFormatException("Unexpected Position: Invalid Weighted Pressure Plate Section");
        }
        if (w.Version >= 189)
        {
            LoadTownManager(b, w);
            if (b.BaseStream.Position != sectionPointers[8])
                throw new TEditFileFormatException("Unexpected Position: Invalid Town Manager Section");
        }
        if (w.Version >= 210)
        {
            LoadBestiary(b, w);
            if (b.BaseStream.Position != sectionPointers[9])
                throw new TEditFileFormatException("Unexpected Position: Invalid Bestiary Section");
        }
        if (w.Version >= 220)
        {
            LoadCreativePowers(b, w);
            if (b.BaseStream.Position != sectionPointers[10])
                throw new TEditFileFormatException("Unexpected Position: Invalid Creative Powers Section");
        }

        progress?.Report(new ProgressChangedEventArgs(100, "Verifying File..."));
        LoadFooter(b, w);

        progress?.Report(new ProgressChangedEventArgs(100, "Load Complete."));
    }

    public static Tile[,] LoadTileData(BinaryReader r, int maxX, int maxY, int version, bool[] tileFrameImportant, IProgress<ProgressChangedEventArgs>? progress = null)
    {
        var tiles = new Tile[maxX, maxY];
        int rle;
        for (int x = 0; x < maxX; x++)
        {
            progress?.Report(
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
                            throw new TEditFileFormatException(
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

        int tileType = -1;
        byte header4 = 0;
        byte header3 = 0;
        byte header2 = 0;
        byte header1 = r.ReadByte();

        bool hasHeader2 = false;
        bool hasHeader3 = false;
        bool hasHeader4 = false;

        // check bit[0] to see if header2 has data
        if ((header1 & 0b_0000_0001) == 0b_0000_0001)
        {
            hasHeader2 = true;
            header2 = r.ReadByte();
        }

        // check bit[0] to see if header3 has data
        if (hasHeader2 && (header2 & 0b_0000_0001) == 0b_0000_0001)
        {
            hasHeader3 = true;
            header3 = r.ReadByte();
        }

        if (version >= 269) // 1.4.4+ 
        {
            // check bit[0] to see if header4 has data
            if (hasHeader3 && (header3 & 0b_0000_0001) == 0b_0000_0001)
            {
                hasHeader4 = true;
                header4 = r.ReadByte();
            }
        }

        // check bit[1] for active tile
        bool isActive = (header1 & 0b_0000_0010) == 0b_0000_0010;

        if (isActive)
        {
            tile.IsActive = isActive;
            // read tile type

            if ((header1 & 0b_0010_0000) != 0b_0010_0000) // check bit[5] to see if tile is byte or little endian int16
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
            if ((header3 & 0b_0000_1000) == 0b_0000_1000)
            {
                tile.TileColor = r.ReadByte();
            }
        }

        // Read Walls
        if ((header1 & 0b_0000_0100) == 0b_0000_0100) // check bit[3] bit for active wall
        {
            tile.Wall = r.ReadByte();


            // check bit[4] of header3 to see if there is a wall color
            if ((header3 & 0b_0001_0000) == 0b_0001_0000)
            {
                tile.WallColor = r.ReadByte();
            }
        }

        // check for liquids, grab the bit[3] and bit[4], shift them to the 0 and 1 bits
        byte liquidType = (byte)((header1 & 0b_0001_1000) >> 3);
        if (liquidType != 0)
        {
            tile.LiquidAmount = r.ReadByte();
            tile.LiquidType = (LiquidType)liquidType; // water, lava, honey

            // shimmer (v 1.4.4 +)
            if (version >= 269 && (header3 & 0b_1000_0000) == 0b_1000_0000)
            {
                tile.LiquidType = LiquidType.Shimmer;
            }

        }

        // check if we have data in header2 other than just telling us we have header3
        if (header2 > 1)
        {
            // check bit[1] for red wire
            if ((header2 & 0b_0000_0010) == 0b_0000_0010)
            {
                tile.WireRed = true;
            }
            // check bit[2] for blue wire
            if ((header2 & 0b_0000_0100) == 0b_0000_0100)
            {
                tile.WireBlue = true;
            }
            // check bit[3] for green wire
            if ((header2 & 0b_0000_1000) == 0b_0000_1000)
            {
                tile.WireGreen = true;
            }

            // grab bits[4, 5, 6] and shift 4 places to 0,1,2. This byte is our brick style
            byte brickStyle = (byte)((header2 & 0b_0111_0000) >> 4);
            if (brickStyle != 0 && WorldConfiguration.TileProperties.Count > tile.Type && WorldConfiguration.TileProperties[tile.Type].HasSlopes)
            {
                tile.BrickStyle = (BrickStyle)brickStyle;
            }
        }

        // check if we have data in header3 to process
        if (header3 > 1)
        {
            // check bit[1] for actuator
            if ((header3 & 0b_0000_0010) == 0b_0000_0010)
            {
                tile.Actuator = true;
            }

            // check bit[2] for inactive due to actuator
            if ((header3 & 0b_0000_0100) == 0b_0000_0100)
            {
                tile.InActive = true;
            }

            if ((header3 & 0b_0010_0000) == 0b_0010_0000)
            {
                tile.WireYellow = true;
            }

            if (version >= 222)
            {
                if ((header3 & 0b_0100_0000) == 0b_0100_0000)
                {
                    tile.Wall = (ushort)(r.ReadByte() << 8 | tile.Wall);

                }
            }
        }

        if (version >= 269 && header4 > (byte)1)
        {
            if ((header4 & 0b_0000_0010) == 0b_0000_0010)
            {
                tile.InvisibleBlock = true;
            }
            if ((header4 & 0b_0000_0100) == 0b_0000_0100)
            {
                tile.InvisibleWall = true;
            }
            if ((header4 & 0b_0000_1000) == 0b_0000_1000)
            {
                tile.FullBrightBlock = true;
            }
            if ((header4 & 0b_0001_0000) == 0b_0001_0000)
            {
                tile.FullBrightWall = true;
            }
        }

        // get bit[6,7] shift to 0,1 for RLE encoding type
        // 0 = no RLE compression
        // 1 = byte RLE counter
        // 2 = int16 RLE counter
        // 3 = not implemented, assume int16
        byte rleStorageType = (byte)((header1 & 192) >> 6);

        rle = rleStorageType switch
        {
            0 => (int)0,
            1 => (int)r.ReadByte(),
            _ => (int)r.ReadInt16()
        };

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
        // load shimmerd town
        if (w.Version >= 268)
        {
            int numNpcs = r.ReadInt32();
            w.ShimmeredTownNPCs.Clear();

            for (int i = 0; i < numNpcs; i++)
            {
                w.ShimmeredTownNPCs.Add(r.ReadInt32());
            }
        }

        // load npc
        int totalNpcs = 0;
        for (bool i = r.ReadBoolean(); i; i = r.ReadBoolean())
        {
            NPC npc = new NPC();
            if (w.Version >= 190)
            {
                npc.SpriteId = r.ReadInt32();
                if (WorldConfiguration.NpcNames.ContainsKey(npc.SpriteId))
                    npc.Name = WorldConfiguration.NpcNames[npc.SpriteId];
            }
            else
            {
                npc.Name = r.ReadString();
                if (WorldConfiguration.NpcIds.ContainsKey(npc.Name))
                    npc.SpriteId = WorldConfiguration.NpcIds[npc.Name];
            }
            npc.DisplayName = r.ReadString();
            npc.Position = new Vector2Float(r.ReadSingle(), r.ReadSingle());
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
                if (WorldConfiguration.NpcIds.ContainsKey(npc.Name))
                    npc.SpriteId = WorldConfiguration.NpcIds[npc.Name];
            }
            npc.Position = new Vector2Float(r.ReadSingle(), r.ReadSingle());
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
            throw new TEditFileFormatException("Invalid Footer");

        if (r.ReadString() != w.Title)
            throw new TEditFileFormatException("Invalid Footer");

        if (r.ReadInt32() != w.WorldId)
            throw new TEditFileFormatException("Invalid Footer");
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
            w.WorldGUID = new Guid(r.ReadBytes(16));
        }
        else
        {
            w.WorldGUID = Guid.NewGuid();
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
            if (w.Version >= 249) { w.RemixWorld = r.ReadBoolean(); }
            if (w.Version >= 266) { w.NoTrapsWorld = r.ReadBoolean(); }
            w.ZenithWorld = (w.Version < 267) ? w.RemixWorld && w.DrunkWorld : r.ReadBoolean();
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

        w.DownedBoss1EyeofCthulhu = r.ReadBoolean();
        w.DownedBoss2EaterofWorlds = r.ReadBoolean();
        w.DownedBoss3Skeletron = r.ReadBoolean();
        w.DownedQueenBee = r.ReadBoolean();
        w.DownedMechBoss1TheDestroyer = r.ReadBoolean();
        w.DownedMechBoss2TheTwins = r.ReadBoolean();
        w.DownedMechBoss3SkeletronPrime = r.ReadBoolean();
        w.DownedMechBossAny = r.ReadBoolean();
        w.DownedPlantBoss = r.ReadBoolean();
        w.DownedGolemBoss = r.ReadBoolean();

        if (w.Version >= 118) { w.DownedSlimeKingBoss = r.ReadBoolean(); }

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
        if (w.Version >= 257) { w.PartyOfDoom = r.ReadBoolean(); }
        w.InvasionDelay = r.ReadInt32();
        w.InvasionSize = r.ReadInt32();
        w.InvasionType = r.ReadInt32();
        w.InvasionX = r.ReadDouble();

        if (w.Version >= 118) { w.SlimeRainTime = r.ReadDouble(); }

        if (w.Version >= 113) { w.SundialCooldown = r.ReadByte(); }

        w.IsRaining = r.ReadBoolean();
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

        if (w.Version < 95) { return; }

        for (int i = r.ReadInt32(); i > 0; i--)
        {
            w.Anglers.Add(r.ReadString());
        }

        if (w.Version < 99) { return; }

        w.SavedAngler = r.ReadBoolean();

        if (w.Version < 101) { return; }
        w.AnglerQuest = r.ReadInt32();

        if (w.Version < 104) { return; }

        w.SavedStylist = r.ReadBoolean();

        if (w.Version >= 140)
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

        if (w.Version < 109) { return; }

        var versionMaxNPCId = WorldConfiguration.SaveConfiguration.GetData(w.Version).MaxNpcId;
        var maxNpcId = WorldConfiguration.SaveConfiguration.GetData(WorldConfiguration.SaveConfiguration.GetMaxVersion()).MaxNpcId;
        int numberOfMobs = r.ReadInt16();
        w.KilledMobs.Clear();
        for (int counter = 0; counter <= maxNpcId; counter++)
        {
            if (counter < numberOfMobs)
            {
                w.KilledMobs.Add(r.ReadInt32()); // read all of them
            }
            else
            {
                w.KilledMobs.Add(0); // fill with 0s to max version npc id
            }
        }

        if (w.Version < 128) { return; }

        if (w.Version >= 140)
        {
            w.FastForwardTime = r.ReadBoolean();
        }

        if (w.Version < 131) { return; }

        w.DownedFishron = r.ReadBoolean();

        if (w.Version >= 140)
        {
            w.DownedMartians = r.ReadBoolean();
            w.DownedLunaticCultist = r.ReadBoolean();
            w.DownedMoonlord = r.ReadBoolean();
        }

        w.DownedHalloweenKing = r.ReadBoolean();
        w.DownedHalloweenTree = r.ReadBoolean();
        w.DownedChristmasQueen = r.ReadBoolean();
        w.DownedSanta = r.ReadBoolean();
        w.DownedChristmasTree = r.ReadBoolean();

        if (w.Version < 140) { return; }

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
            w.LanternNightCooldown = r.ReadInt32();
            w.LanternNightGenuine = r.ReadBoolean();
            w.LanternNightManual = r.ReadBoolean();
            w.LanternNightNextNightIsGenuine = r.ReadBoolean();
        }

        // tree tops
        if (w.Version >= 211)
        {
            int numTrees = r.ReadInt32();
            w.TreeTopVariations = new System.Collections.ObjectModel.ObservableCollection<int>(new int[Math.Max(13, numTrees)]);
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

        if (w.Version >= 250)
        {
            w.UnlockedSlimeBlueSpawn = r.ReadBoolean();
        }

        if (w.Version >= 251)
        {
            w.UnlockedMerchantSpawn = r.ReadBoolean();
            w.UnlockedDemolitionistSpawn = r.ReadBoolean();
            w.UnlockedPartyGirlSpawn = r.ReadBoolean();
            w.UnlockedDyeTraderSpawn = r.ReadBoolean();
            w.UnlockedTruffleSpawn = r.ReadBoolean();
            w.UnlockedArmsDealerSpawn = r.ReadBoolean();
            w.UnlockedNurseSpawn = r.ReadBoolean();
            w.UnlockedPrincessSpawn = r.ReadBoolean();
        }

        if (w.Version >= 259)
        {
            w.CombatBookVolumeTwoWasUsed = r.ReadBoolean();
        }

        if (w.Version >= 260)
        {
            w.PeddlersSatchelWasUsed = r.ReadBoolean();
        }

        if (w.Version >= 261)
        {
            w.UnlockedSlimeGreenSpawn = r.ReadBoolean();
            w.UnlockedSlimeOldSpawn = r.ReadBoolean();
            w.UnlockedSlimePurpleSpawn = r.ReadBoolean();
            w.UnlockedSlimeRainbowSpawn = r.ReadBoolean();
            w.UnlockedSlimeRedSpawn = r.ReadBoolean();
            w.UnlockedSlimeYellowSpawn = r.ReadBoolean();
            w.UnlockedSlimeCopperSpawn = r.ReadBoolean();
        }

        if (w.Version >= 264)
        {
            w.FastForwardTimeToDusk = r.ReadBoolean();
            w.MoondialCooldown = r.ReadByte();
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
        uint versionNumber = r.ReadUInt32();

        if (versionNumber >= 140) // 135
        {
            // check for chinese

            w.IsChinese = (char)r.PeekChar() == 'x';

            string headerFormat = new string(r.ReadChars(7));
            FileType fileType = (FileType)r.ReadByte();

            if (fileType != FileType.World)
            {
                throw new TEditFileFormatException($"Is not a supported file type: {fileType.ToString()}");
            }

            if (!w.IsChinese && headerFormat != WorldConfiguration.DesktopHeader)
            {
                throw new TEditFileFormatException("Invalid desktop world header.");
            }

            if (w.IsChinese && headerFormat != WorldConfiguration.ChineseHeader)
            {
                throw new TEditFileFormatException("Invalid chinese world header.");
            }

            w.FileRevision = r.ReadUInt32();

            UInt64 flags = r.ReadUInt64(); // load bitflags (currently only bit 1 isFavorite is used)
            w.IsFavorite = ((flags & 1uL) == 1uL);

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
