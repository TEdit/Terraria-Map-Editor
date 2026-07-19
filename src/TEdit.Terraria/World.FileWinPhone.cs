using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TEdit.Terraria.Objects;
using TEdit.Utility;

namespace TEdit.Terraria;

public partial class World
{
    private const int WinPhoneMaxTitleLength = 128;
    private static readonly ushort[] WinPhoneFrameImportantIds =
    [
        3, 4, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 24, 26, 27, 28, 29,
        31, 33, 34, 35, 36, 42, 50, 55, 61, 71, 72, 73, 74, 77, 78, 79, 81, 82, 83, 84,
        85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102,
        103, 104, 105, 106, 110, 113, 114, 125, 126, 128, 129, 132, 133, 134, 135,
        136, 137, 138, 139, 141, 142, 143, 144, 149, 165, 171, 172, 173, 174, 178, 184,
        185, 186, 187, 201, 207, 209, 210, 212, 215, 216, 217, 218, 219, 220, 227,
        228, 231, 233, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246,
        247, 254, 269, 270, 271, 275, 276, 277, 278, 279, 280, 281, 282, 283, 285,
        286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300,
        301, 302, 303, 304, 305, 306, 307, 308, 309, 310, 314, 316, 317, 318, 319,
        320, 323, 324, 334, 335, 337, 338, 339, 500,
    ];

    /// <summary>
    /// Detects and reads the native Windows Phone world container header.
    /// Versions through 65 have an uncompressed body; version 66 and newer use
    /// the native game's body transform and are not accepted by this reader yet.
    /// </summary>
    internal static bool TryLoadWinPhone(
        BinaryReader reader,
        World world,
        bool headersOnly,
        IProgress<ProgressChangedEventArgs>? progress = null)
    {
        Stream stream = reader.BaseStream;
        long originalPosition = stream.Position;

        try
        {
            stream.Position = 0;
            byte[] source = reader.ReadBytes(checked((int)stream.Length));
            if (!TryReadWinPhoneHeader(source, out WinPhoneHeader header))
            {
                stream.Position = originalPosition;
                return false;
            }

            world.IsWinPhone = true;
            world.WinPhoneSupportsWeather = header.Version >= 58;
            world.WinPhoneSupportsExtendedProgression = header.Version >= 58;
            world.WinPhoneSourceData = source;
            world.WinPhoneStoredCrc = header.Crc;
            world.WinPhoneCloudFlag = header.CloudFlag;
            world.WinPhoneHistory = header.History;
            world.Version = header.Version;
            world.Title = header.Title;
            world.WorldId = header.WorldId;
            world.Rand = new Random(world.WorldId);
            world.LeftWorld = 0;
            world.RightWorld = header.RightWorld;
            world.TopWorld = 0;
            world.BottomWorld = header.BottomWorld;
            world.TilesHigh = header.TilesHigh;
            world.TilesWide = header.TilesWide;
            world.SpawnX = header.SpawnX;
            world.SpawnY = header.SpawnY;
            world.DungeonX = header.DungeonX;
            world.DungeonY = header.DungeonY;
            world.GroundLevel = header.GroundLevel;
            world.RockLevel = header.RockLevel;

            progress?.Report(new ProgressChangedEventArgs(0, "Loaded Windows Phone world header."));

            if (!headersOnly)
            {
                int position = LoadWinPhoneTiles(source, header, world, progress);
                LoadWinPhoneSections(source, header, world, position);
            }

            return true;
        }
        catch
        {
            stream.Position = originalPosition;
            throw;
        }
    }

    private static int LoadWinPhoneTiles(
        ReadOnlySpan<byte> source,
        WinPhoneHeader header,
        World world,
        IProgress<ProgressChangedEventArgs>? progress)
    {
        int markerOffset = FindSectionMarker(source, header.BodyOffset);
        if (markerOffset < 0)
            throw new InvalidDataException("Windows Phone tile section marker was not found.");

        int position = markerOffset + 4;
        world.WinPhoneMetadataPrimitives = ReadWinPhoneMetadataPrimitives(
            source, header.BodyOffset, markerOffset, header.Version);
        ApplyWinPhoneMetadataToWorld(world, header.Version);
        world.WinPhoneTileSectionOffset = position;
        bool[] frameImportant = CreateWinPhoneFrameImportant(header.Version);
        world.TileFrameImportant = (bool[])frameImportant.Clone();
        world.Tiles = new Tile[world.TilesWide, world.TilesHigh];
        int tileCount = checked(world.TilesWide * world.TilesHigh);
        world.WinPhoneStoredTileTypes = new ushort[tileCount];
        world.WinPhoneTileHeader1 = new byte[tileCount];
        world.WinPhoneTileHeader2 = new byte[tileCount];
        world.WinPhoneTileShape = new byte[tileCount];
        world.WinPhoneLegacyTileFlags = new byte[tileCount];
        world.WinPhoneTileRunLengths = new ushort[tileCount];

        // The native loader has two independent tile decoders. Versions below
        // 58 use FUN_00733bac; versions 58 and newer use FUN_00733858.
        if (header.Version < 58)
        {
            LoadWinPhoneLegacyTiles(source, header, world, frameImportant, ref position, progress);
            ValidateWinPhoneTileEndMarker(source, position);
            return position + 4;
        }

        int encodedHeight = world.TilesHigh;

        for (int x = 0; x < world.TilesWide; x++)
        {
            Queue<string> trace = new();
            if ((x & 31) == 0)
                progress?.Report(new ProgressChangedEventArgs(
                    x.ProgressPercentage(world.TilesWide),
                    "Loading Windows Phone tiles..."));

            for (int y = 0; y < encodedHeight;)
            {
                byte header1 = ReadByte(source, ref position);
                int auxiliaryIndex = GetWinPhoneTileIndex(world, x, y);
                world.WinPhoneTileHeader1[auxiliaryIndex] = header1;
                Tile tile = new()
                {
                    IsActive = (header1 & 0x01) != 0,
                    WireRed = (header1 & 0x02) != 0,
                    WireGreen = (header1 & 0x04) != 0,
                    WireBlue = (header1 & 0x08) != 0,
                    Actuator = (header1 & 0x10) != 0,
                    InActive = (header1 & 0x20) != 0,
                };

                if (tile.IsActive)
                {
                    int tileOffset = position;
                    tile.Type = ReadUInt16(source, ref position);
                    world.WinPhoneStoredTileTypes[auxiliaryIndex] = tile.Type;

                    trace.Enqueue($"y={y}:type={tile.Type}@{tileOffset}");
                    while (trace.Count > 20)
                        trace.Dequeue();

                    // The native tile field is nine bits wide. Windows Phone
                    // uses framed type 500 as an internal placeholder tile.
                    if (tile.Type > 0x1FF)
                        throw new InvalidDataException(
                            $"Invalid Windows Phone tile type {tile.Type} at x={x}, y={y}, offset={position}. Trace: {string.Join("; ", trace)}");

                    bool isFramed = tile.Type < frameImportant.Length
                        ? frameImportant[tile.Type]
                        : true;
                    if (isFramed)
                    {
                        tile.U = ReadInt16(source, ref position);
                        tile.V = ReadInt16(source, ref position);
                    }
                    else
                    {
                        tile.U = -1;
                        tile.V = -1;
                    }

                    tile.TileColor = ReadByte(source, ref position);
                }

                tile.Wall = ReadByte(source, ref position);
                if (tile.Wall != 0)
                    tile.WallColor = ReadByte(source, ref position);

                tile.LiquidAmount = ReadByte(source, ref position);
                if (tile.LiquidAmount != 0)
                {
                    byte nativeLiquidType = ReadByte(source, ref position);
                    tile.LiquidType = (LiquidType)(nativeLiquidType + 1);
                }

                byte header2 = ReadByte(source, ref position);
                world.WinPhoneTileHeader2[auxiliaryIndex] = header2;
                tile.WireYellow = (header2 & 0x10) != 0;

                if (header.Version is >= 60 and <= 68)
                {
                    byte shape = ReadByte(source, ref position);
                    world.WinPhoneTileShape[auxiliaryIndex] = shape;
                    tile.BrickStyle = (BrickStyle)(shape >> 4);
                }

                int rle = ReadByte(source, ref position);
                if ((rle & 0x80) != 0)
                    rle = (rle & 0x7F) | ReadByte(source, ref position) << 7;
                world.WinPhoneTileRunLengths[auxiliaryIndex] = (ushort)rle;

                if (y + rle >= encodedHeight)
                    throw new InvalidDataException(
                        $"Windows Phone tile run exceeds the world height at x={x}, y={y}, rle={rle}, offset={position}. Trace: {string.Join("; ", trace)}");

                tile.ResetCache();
                if (y < world.TilesHigh)
                    world.Tiles[x, y] = tile;
                for (int repeat = 1; repeat <= rle && y + repeat < world.TilesHigh; repeat++)
                {
                    world.Tiles[x, y + repeat] = tile;
                    CopyWinPhoneTileAuxiliary(world, x, y, y + repeat);
                }
                y += rle + 1;
            }
        }

        ValidateWinPhoneTileEndMarker(source, position);
        return position + 4;
    }

    private static void LoadWinPhoneSections(
        ReadOnlySpan<byte> source,
        WinPhoneHeader header,
        World world,
        int position)
    {
        world.WinPhoneDataSectionOffset = position;
        LoadWinPhoneChests(source, header.Version, world, ref position);
        ReadWinPhoneSectionMarker(source, ref position, "chests");
        LoadWinPhoneSigns(source, header.Version, world, ref position);
        ReadWinPhoneSectionMarker(source, ref position, "signs");
        LoadWinPhoneNpcs(source, world, ref position);
        ReadWinPhoneSectionMarker(source, ref position, "NPCs");
        LoadWinPhoneCharacterNames(source, header.Version, world, ref position);
        world.WinPhoneParsedLength = position;

        // The bundled v49 Tutorial.world is a fat resource: the native v49
        // reader stops after ten legacy NPC names at offset 202,363 and ignores
        // the remaining bundled data. Later native saves end at the name table.
        if (header.Version >= 50 && position != source.Length)
            throw new InvalidDataException(
                $"Windows Phone world has {source.Length - position} unexpected trailing bytes.");
    }

    private static void LoadWinPhoneChests(
        ReadOnlySpan<byte> source,
        uint version,
        World world,
        ref int position)
    {
        world.Chests.Clear();
        world.WinPhoneChestSlots = new Chest?[Chest.LegacyLimit];
        int itemCount = version < 58 ? 20 : 40;
        byte[] itemMask = new byte[5];

        for (int chestIndex = 0; chestIndex < Chest.LegacyLimit; chestIndex++)
        {
            if (ReadByte(source, ref position) == 0)
                continue;

            Chest chest = new(ReadInt16(source, ref position), ReadInt16(source, ref position))
            {
                MaxItems = itemCount,
            };

            Array.Clear(itemMask);
            if (version < 58)
            {
                itemMask.AsSpan(0, 3).Fill(byte.MaxValue);
            }
            else
            {
                int maskLength = ReadByte(source, ref position);
                if (maskLength is < 0 or > 5)
                    throw new InvalidDataException($"Invalid Windows Phone chest item mask length {maskLength}.");
                for (int i = 0; i < maskLength; i++)
                    itemMask[i] = ReadByte(source, ref position);
            }

            for (int slot = 0; slot < itemCount; slot++)
            {
                if ((itemMask[slot >> 3] & 1 << (slot & 7)) == 0)
                    continue;

                int stack = version < 55
                    ? ReadByte(source, ref position)
                    : ReadInt16(source, ref position);
                if (stack <= 0)
                    continue;

                int netId = ReadInt16(source, ref position);
                byte prefix = ReadByte(source, ref position);
                chest.Items[slot] = new Item(stack, netId, prefix);
            }

            world.Chests.Add(chest);
            world.WinPhoneChestSlots[chestIndex] = chest;
        }
    }

    private static void LoadWinPhoneSigns(
        ReadOnlySpan<byte> source,
        uint version,
        World world,
        ref int position)
    {
        world.Signs.Clear();
        world.WinPhoneSignSlots = new Sign?[Sign.LegacyLimit];
        world.WinPhoneSignTextFlags = new byte[Sign.LegacyLimit];
        for (int signIndex = 0; signIndex < Sign.LegacyLimit; signIndex++)
        {
            if (ReadByte(source, ref position) == 0)
                continue;

            short x = ReadInt16(source, ref position);
            short y = ReadInt16(source, ref position);
            if (version > 57)
                world.WinPhoneSignTextFlags[signIndex] = ReadByte(source, ref position);
            string text = ReadWinPhoneString(source, version, ref position);
            Sign sign = new(x, y, text);
            world.Signs.Add(sign);
            world.WinPhoneSignSlots[signIndex] = sign;
        }
    }

    private static void LoadWinPhoneNpcs(
        ReadOnlySpan<byte> source,
        World world,
        ref int position)
    {
        world.NPCs.Clear();
        while (ReadByte(source, ref position) != 0)
        {
            int spriteId = ReadByte(source, ref position);
            NPC npc = new()
            {
                SpriteId = spriteId,
                Position = new Vector2FloatObservable(
                    ReadSingle(source, ref position),
                    ReadSingle(source, ref position)),
                IsHomeless = ReadByte(source, ref position) != 0,
                Home = new Vector2Int32Observable(
                    ReadInt16(source, ref position),
                    ReadInt16(source, ref position)),
            };
            npc.Name = WorldConfiguration.NpcNames.TryGetValue(spriteId, out string? name)
                ? name
                : $"NPC {spriteId}";
            world.NPCs.Add(npc);
        }
    }

    private static readonly int[] WinPhoneCharacterNameIds =
    [
        17, 18, 19, 20, 22, 54, 38, 107, 108, 124, 160, 178, 207, 208, 209, 227, 228, 229,
    ];

    private static void LoadWinPhoneCharacterNames(
        ReadOnlySpan<byte> source,
        uint version,
        World world,
        ref int position)
    {
        world.CharacterNames.Clear();
        int nameCount = version < 50 ? 10 : WinPhoneCharacterNameIds.Length;
        for (int index = 0; index < nameCount; index++)
            world.CharacterNames.Add(new NpcName(
                WinPhoneCharacterNameIds[index],
                ReadWinPhoneString(source, version, ref position)));
    }

    private static string ReadWinPhoneString(
        ReadOnlySpan<byte> source,
        uint version,
        ref int position)
    {
        int length = ReadInt32(source, position);
        int lengthOffset = position;
        position += 4;
        if (length < 0 || length > 1_000_000)
            throw new InvalidDataException(
                $"Invalid Windows Phone string length {length} at offset {lengthOffset}.");

        int byteLength = checked(length * (version < 50 ? 1 : 2));
        if (position + byteLength > source.Length)
            throw new EndOfStreamException();
        string value = version < 50
            ? Encoding.UTF8.GetString(source.Slice(position, byteLength))
            : Encoding.Unicode.GetString(source.Slice(position, byteLength));
        position += byteLength;
        return value;
    }

    private static void ReadWinPhoneSectionMarker(
        ReadOnlySpan<byte> source,
        ref int position,
        string precedingSection)
    {
        if (position + 4 > source.Length || ReadInt32(source, position) != 0x162E)
            throw new InvalidDataException(
                $"Windows Phone {precedingSection} section did not end at the expected marker.");
        position += 4;
    }

    private static void LoadWinPhoneLegacyTiles(
        ReadOnlySpan<byte> source,
        WinPhoneHeader header,
        World world,
        bool[] frameImportant,
        ref int position,
        IProgress<ProgressChangedEventArgs>? progress)
    {
        for (int x = 0; x < world.TilesWide; x++)
        {
            if ((x & 31) == 0)
                progress?.Report(new ProgressChangedEventArgs(
                    x.ProgressPercentage(world.TilesWide),
                    "Loading legacy Windows Phone tiles..."));

            for (int y = 0; y < world.TilesHigh;)
            {
                int auxiliaryIndex = GetWinPhoneTileIndex(world, x, y);
                bool active = ReadByte(source, ref position) != 0;
                world.WinPhoneTileHeader1![auxiliaryIndex] = active ? (byte)1 : (byte)0;
                Tile tile = new() { IsActive = active };

                if (active)
                {
                    byte storedType = ReadByte(source, ref position);
                    world.WinPhoneStoredTileTypes![auxiliaryIndex] = storedType;
                    ushort type = storedType switch
                    {
                        35 or 36 when header.Version < 58 => 34,
                        150 when header.Version < 58 => 500,
                        _ => storedType,
                    };
                    tile.Type = type;

                    bool isFramed = IsWinPhoneLegacyFramed(storedType);
                    if (isFramed)
                    {
                        tile.U = ReadInt16(source, ref position);
                        tile.V = ReadInt16(source, ref position);

                        // The native migration folds old banner variants into
                        // type 34 while retaining their frame row.
                        if (header.Version < 58 && storedType == 35)
                            tile.V += 54;
                        else if (header.Version < 58 && storedType == 36)
                            tile.V += 108;
                    }
                    else
                    {
                        tile.U = -1;
                        tile.V = -1;
                    }

                    if (tile.Type == 127)
                        tile.IsActive = false;
                }

                tile.Wall = ReadByte(source, ref position);
                if (header.Version > 50)
                    tile.WallColor = ReadByte(source, ref position);

                tile.LiquidAmount = ReadByte(source, ref position);
                if (tile.LiquidAmount != 0)
                    tile.LiquidType = ReadByte(source, ref position) != 0
                        ? LiquidType.Lava
                        : LiquidType.Water;

                byte flags = ReadByte(source, ref position);
                world.WinPhoneLegacyTileFlags![auxiliaryIndex] = flags;
                tile.WireRed = (flags & 0x10) != 0;
                tile.WireGreen = (flags & 0x20) != 0;
                tile.WireBlue = (flags & 0x40) != 0;

                int rle = ReadInt16(source, ref position);
                if (rle < 0 || y + rle >= world.TilesHigh)
                    throw new InvalidDataException(
                        $"Legacy Windows Phone tile run exceeds the world height at x={x}, y={y}, rle={rle}, offset={position}.");
                world.WinPhoneTileRunLengths![auxiliaryIndex] = (ushort)rle;

                tile.ResetCache();
                world.Tiles[x, y] = tile;
                for (int repeat = 1; repeat <= rle; repeat++)
                {
                    world.Tiles[x, y + repeat] = tile;
                    CopyWinPhoneTileAuxiliary(world, x, y, y + repeat);
                }
                y += rle + 1;
            }
        }
    }

    private static int GetWinPhoneTileIndex(World world, int x, int y) =>
        checked(x * world.TilesHigh + y);

    private static void CopyWinPhoneTileAuxiliary(World world, int x, int sourceY, int targetY)
    {
        int sourceIndex = GetWinPhoneTileIndex(world, x, sourceY);
        int targetIndex = GetWinPhoneTileIndex(world, x, targetY);
        world.WinPhoneStoredTileTypes![targetIndex] = world.WinPhoneStoredTileTypes[sourceIndex];
        world.WinPhoneTileHeader1![targetIndex] = world.WinPhoneTileHeader1[sourceIndex];
        world.WinPhoneTileHeader2![targetIndex] = world.WinPhoneTileHeader2[sourceIndex];
        world.WinPhoneTileShape![targetIndex] = world.WinPhoneTileShape[sourceIndex];
        world.WinPhoneLegacyTileFlags![targetIndex] = world.WinPhoneLegacyTileFlags[sourceIndex];
    }

    private static void ValidateWinPhoneTileEndMarker(ReadOnlySpan<byte> source, int position)
    {
        // A second 0x162E marker follows the complete tile stream.
        if (position + 4 > source.Length || ReadInt32(source, position) != 0x162E)
            throw new InvalidDataException("Windows Phone tile section length did not match the world dimensions.");
    }

    private static bool[] CreateWinPhoneFrameImportant(uint version)
    {
        bool[] result = new bool[Math.Max(WorldConfiguration.SettingsTileFrameImportant.Length, 512)];
        foreach (ushort id in WinPhoneFrameImportantIds)
            result[id] = true;
        if (version == 49)
        {
            // The pre-v58 decoder uses this predicate instead of the later
            // property table (FUN_0072ad80).
            Array.Clear(result);
            for (int id = 0; id <= byte.MaxValue; id++)
                result[id] = IsWinPhoneLegacyFramed((byte)id);
        }
        return result;
    }

    private static bool IsWinPhoneLegacyFramed(byte type) => type switch
    {
        3 or 4 or 5 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or
        20 or 21 or 24 or 26 or 27 or 28 or 29 or 31 or 33 or 34 or 35 or 36 or
        42 or 50 or 55 or 61 or
        >= 71 and <= 74 or >= 77 and <= 79 or >= 81 and <= 106 or
        110 or 113 or 114 or >= 125 and <= 129 or >= 132 and <= 139 or
        >= 141 and <= 144 or 149 or 150 => true,
        _ => false,
    };

    private static int FindSectionMarker(ReadOnlySpan<byte> source, int start)
    {
        int limit = Math.Min(source.Length - 4, start + 1024);
        for (int position = start; position <= limit; position++)
        {
            if (ReadInt32(source, position) == 0x162E)
                return position;
        }
        return -1;
    }

    private static WinPhonePrimitive[] ReadWinPhoneMetadataPrimitives(
        ReadOnlySpan<byte> source,
        int start,
        int markerOffset,
        uint version)
    {
        List<WinPhonePrimitive> values = [];
        byte[] metadataSource = source.Slice(start, markerOffset - start).ToArray();
        int position = 0;

        void ReadBytes(int count)
        {
            for (int i = 0; i < count; i++)
                values.Add(new WinPhonePrimitive(WinPhonePrimitiveKind.Byte, ReadByte(metadataSource, ref position)));
        }
        void ReadInt16Values(int count)
        {
            for (int i = 0; i < count; i++)
                values.Add(new WinPhonePrimitive(WinPhonePrimitiveKind.Int16, ReadInt16(metadataSource, ref position)));
        }
        void ReadInt32Values(int count)
        {
            for (int i = 0; i < count; i++)
            {
                values.Add(new WinPhonePrimitive(
                    WinPhonePrimitiveKind.Int32,
                    ReadInt32(metadataSource, position)));
                position += 4;
            }
        }

        ReadInt32Values(version > 47 ? 3 : 2);
        ReadInt16Values(7);
        ReadInt32Values(1);
        ReadBytes(3);
        ReadInt16Values(1);
        if (version >= 58)
            ReadBytes(2);
        ReadInt16Values(2);

        if (version >= 58)
        {
            ReadBytes(1);
            ReadBytes(1);
            ReadInt32Values(3);
        }

        ReadBytes(9);
        if (version > 51)
            ReadBytes(1);
        if (version > 56)
            ReadBytes(1);
        if (version > 57)
            ReadBytes(8);

        ReadBytes(3);
        ReadInt32Values(1);
        if (version > 57)
            ReadInt16Values(3);
        ReadBytes(1);
        if (version > 55)
            ReadInt16Values(1);
        ReadBytes(1);
        ReadInt16Values(1);
        ReadBytes(1);
        ReadInt32Values(1);

        if (version > 57)
        {
            for (int block = 0; block < 2; block++)
            {
                ReadBytes(4);
                ReadInt16Values(3);
            }
            ReadBytes(9);
        }

        if (position != metadataSource.Length)
            throw new InvalidDataException(
                $"Windows Phone metadata layout consumed {position} bytes, expected {metadataSource.Length}.");
        return [.. values];
    }

    private static void ApplyWinPhoneMetadataToWorld(World world, uint version)
    {
        WinPhonePrimitive[] values = world.WinPhoneMetadataPrimitives!;
        world.WorldId = values[0].Value;
        world.RightWorld = values[2].Value;
        world.BottomWorld = values[3].Value;
        world.TilesHigh = values[4].Value;
        world.TilesWide = values[5].Value;
        world.DungeonX = values[6].Value;
        world.DungeonY = values[7].Value;
        world.GroundLevel = values[8].Value;
        world.RockLevel = values[9].Value;
        world.Time = BitConverter.Int32BitsToSingle(values[10].Value);
        world.DayTime = values[11].Value != 0;
        world.MoonPhase = values[12].Value;
        world.BloodMoon = values[13].Value != 0;

        int spawnIndex = version >= 58 ? 17 : 15;
        world.SpawnX = values[spawnIndex].Value;
        world.SpawnY = values[spawnIndex + 1].Value;
        if (version >= 58)
        {
            world.IsCrimson = values[19].Value != 0;
            world.IsRaining = values[20].Value != 0;
            world.TempRainTime = values[21].Value;
            world.TempMaxRain = BitConverter.Int32BitsToSingle(values[22].Value);
        }

        int progressionIndex = version >= 58 ? 24 : 17;
        world.DownedBoss1EyeofCthulhu = values[progressionIndex].Value != 0;
        world.DownedBoss2EaterofWorlds = values[progressionIndex + 1].Value != 0;
        world.DownedBoss3Skeletron = values[progressionIndex + 2].Value != 0;
        world.SavedGoblin = values[progressionIndex + 3].Value != 0;
        world.SavedWizard = values[progressionIndex + 4].Value != 0;
        world.SavedMech = values[progressionIndex + 5].Value != 0;
        world.DownedGoblins = values[progressionIndex + 6].Value != 0;
        world.DownedClown = values[progressionIndex + 7].Value != 0;
        world.DownedFrost = values[progressionIndex + 8].Value != 0;
        if (version >= 58)
        {
            // Slots +9 and +10 are the mobile-only Lepus and Turkor flags.
            world.DownedQueenBee = values[progressionIndex + 11].Value != 0;
            world.DownedMechBoss1TheDestroyer = values[progressionIndex + 12].Value != 0;
            world.DownedMechBoss2TheTwins = values[progressionIndex + 13].Value != 0;
            world.DownedMechBoss3SkeletronPrime = values[progressionIndex + 14].Value != 0;
            // Slot +15 is the native derived "any mechanical boss" flag.
            world.DownedPlantBoss = values[progressionIndex + 16].Value != 0;
            world.DownedGolemBoss = values[progressionIndex + 17].Value != 0;
            world.DownedPirates = values[progressionIndex + 18].Value != 0;
        }

        int worldStateIndex = version >= 58 ? 43 : 26;
        world.ShadowOrbCount = values[worldStateIndex + 2].Value;
        // ShadowOrbCount updates ShadowOrbSmashed in the editor model, so apply
        // the independently stored native flag after the count.
        world.ShadowOrbSmashed = values[worldStateIndex].Value != 0;
        world.SpawnMeteor = values[worldStateIndex + 1].Value != 0;
        world.AltarCount = values[worldStateIndex + 3].Value;
        if (version >= 58)
        {
            world.SavedOreTiersCobalt = values[worldStateIndex + 4].Value;
            world.SavedOreTiersMythril = values[worldStateIndex + 5].Value;
            world.SavedOreTiersAdamantite = values[worldStateIndex + 6].Value;
        }

        int hardModeIndex = worldStateIndex + (version >= 58 ? 7 : 4);
        world.HardMode = values[hardModeIndex].Value != 0;
        int invasionIndex = hardModeIndex + (version > 55 ? 2 : 1);
        world.InvasionDelay = values[invasionIndex].Value;
        world.InvasionSize = values[invasionIndex + 1].Value;
        world.InvasionType = values[invasionIndex + 2].Value;
        world.InvasionX = BitConverter.Int32BitsToSingle(values[invasionIndex + 3].Value);
    }

    private static byte ReadByte(ReadOnlySpan<byte> source, ref int position)
    {
        if ((uint)position >= (uint)source.Length)
            throw new EndOfStreamException();
        return source[position++];
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> source, ref int position)
    {
        if (position + 2 > source.Length)
            throw new EndOfStreamException();
        ushort value = (ushort)(source[position] | source[position + 1] << 8);
        position += 2;
        return value;
    }

    private static short ReadInt16(ReadOnlySpan<byte> source, ref int position) =>
        unchecked((short)ReadUInt16(source, ref position));

    private static float ReadSingle(ReadOnlySpan<byte> source, ref int position)
    {
        if (position + 4 > source.Length)
            throw new EndOfStreamException();
        float value = BitConverter.Int32BitsToSingle(ReadInt32(source, position));
        position += 4;
        return value;
    }

    private static byte[] SerializeWinPhoneDataSections(World world, uint version)
    {
        if (world.WinPhoneChestSlots is null || world.WinPhoneSignSlots is null ||
            world.WinPhoneSignTextFlags is null)
            throw new InvalidOperationException("Windows Phone sparse section slots are unavailable.");

        SynchronizeWinPhoneSparseSlots(world);

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        WriteWinPhoneChests(writer, world, version);
        writer.Write(0x162E);
        WriteWinPhoneSigns(writer, world, version);
        writer.Write(0x162E);
        WriteWinPhoneNpcs(writer, world);
        writer.Write(0x162E);
        WriteWinPhoneCharacterNames(writer, world, version);
        writer.Flush();
        return stream.ToArray();
    }

    private static void SynchronizeWinPhoneSparseSlots(World world)
    {
        SynchronizeWinPhoneSlots(world.WinPhoneChestSlots!, world.Chests, "chests");

        Sign?[] signSlots = world.WinPhoneSignSlots!;
        HashSet<Sign> previousSigns = new(ReferenceEqualityComparer.Instance);
        foreach (Sign? sign in signSlots)
            if (sign is not null)
                previousSigns.Add(sign);
        SynchronizeWinPhoneSlots(signSlots, world.Signs, "signs");
        for (int index = 0; index < signSlots.Length; index++)
        {
            Sign? sign = signSlots[index];
            if (sign is null)
            {
                world.WinPhoneSignTextFlags![index] = 0;
            }
            else if (!previousSigns.Contains(sign))
            {
                world.WinPhoneSignTextFlags![index] = string.IsNullOrEmpty(sign.Text) ? (byte)0 : (byte)1;
            }
        }
    }

    private static void SynchronizeWinPhoneSlots<T>(T?[] slots, IList<T> items, string sectionName)
        where T : class
    {
        if (items.Count > slots.Length)
            throw new InvalidDataException(
                $"Windows Phone worlds support at most {slots.Length} {sectionName}.");

        HashSet<T> current = new(items, ReferenceEqualityComparer.Instance);
        HashSet<T> slotted = new(ReferenceEqualityComparer.Instance);
        for (int index = 0; index < slots.Length; index++)
        {
            T? item = slots[index];
            if (item is not null && current.Contains(item) && slotted.Add(item))
                continue;
            slots[index] = null;
        }

        foreach (T item in items)
        {
            if (slotted.Contains(item))
                continue;
            int freeIndex = Array.FindIndex(slots, static slot => slot is null);
            if (freeIndex < 0)
                throw new InvalidDataException($"No free Windows Phone {sectionName} slot is available.");
            slots[freeIndex] = item;
            slotted.Add(item);
        }
    }

    private static byte[] SerializeWinPhoneTiles(World world, uint version)
    {
        if (world.WinPhoneStoredTileTypes is null || world.WinPhoneTileHeader1 is null ||
            world.WinPhoneTileHeader2 is null || world.WinPhoneTileShape is null ||
            world.WinPhoneLegacyTileFlags is null || world.WinPhoneTileRunLengths is null)
            throw new InvalidOperationException("Windows Phone tile encoding metadata is unavailable.");

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        bool[] frameImportant = CreateWinPhoneFrameImportant(version);
        for (int x = 0; x < world.TilesWide; x++)
        {
            for (int y = 0; y < world.TilesHigh;)
            {
                int index = GetWinPhoneTileIndex(world, x, y);
                int run = world.WinPhoneTileRunLengths[index];
                if (y + run >= world.TilesHigh)
                    throw new InvalidDataException($"Stored Windows Phone tile run exceeds column {x}.");
                for (int repeat = 1; repeat <= run; repeat++)
                {
                    if (!WinPhoneTilesEncodeEqual(world, x, y, y + repeat))
                        throw new InvalidDataException(
                            $"Stored Windows Phone tile run no longer matches at {x}, {y + repeat}.");
                }

                if (version < 58)
                    WriteWinPhoneLegacyTile(writer, world, frameImportant, x, y, run, version);
                else
                    WriteWinPhoneCompactTile(writer, world, frameImportant, x, y, run, version);
                y += run + 1;
            }
        }
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] SerializeWinPhonePrefix(World world)
    {
        if (world.WinPhoneMetadataPrimitives is null || world.WinPhoneHistory is null)
            throw new InvalidOperationException("Windows Phone metadata is unavailable.");

        byte[] titleBytes = (world.Version < 50 ? Encoding.UTF8 : Encoding.Unicode)
            .GetBytes(world.Title);
        int titleLength = world.Version < 50 ? titleBytes.Length : world.Title.Length;
        if (titleLength > WinPhoneMaxTitleLength)
            throw new InvalidDataException(
                $"Windows Phone world titles cannot exceed {WinPhoneMaxTitleLength} " +
                (world.Version < 50 ? "UTF-8 bytes." : "characters."));

        WinPhonePrimitive[] metadata = CreateWinPhoneMetadataForSave(world);

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(world.Version);
        writer.Write(world.WinPhoneStoredCrc);
        writer.Write(titleLength);
        writer.Write(titleBytes);
        if (world.Version >= 53)
        {
            writer.Write(world.WinPhoneCloudFlag);
            if (world.Version == 53)
            {
                writer.Write(world.WinPhoneHistory[0]);
            }
            else
            {
                writer.Write(world.WinPhoneHistory.Length);
                foreach (uint value in world.WinPhoneHistory)
                    writer.Write(value);
            }
        }

        foreach (WinPhonePrimitive value in metadata)
        {
            switch (value.Kind)
            {
                case WinPhonePrimitiveKind.Byte:
                    writer.Write((byte)value.Value);
                    break;
                case WinPhonePrimitiveKind.Int16:
                    writer.Write((short)value.Value);
                    break;
                case WinPhonePrimitiveKind.Int32:
                    writer.Write(value.Value);
                    break;
                default:
                    throw new InvalidDataException($"Unknown Windows Phone primitive kind {value.Kind}.");
            }
        }
        writer.Write(0x162E);
        writer.Flush();
        return stream.ToArray();
    }

    private static WinPhonePrimitive[] CreateWinPhoneMetadataForSave(World world)
    {
        WinPhonePrimitive[] values = (WinPhonePrimitive[])world.WinPhoneMetadataPrimitives!.Clone();
        SetWinPhonePrimitive(values, 0, WinPhonePrimitiveKind.Int32, world.WorldId, nameof(world.WorldId));
        SetWinPhonePrimitive(values, 2, WinPhonePrimitiveKind.Int32,
            ToWinPhoneInt32(world.RightWorld, nameof(world.RightWorld)), nameof(world.RightWorld));
        SetWinPhonePrimitive(values, 3, WinPhonePrimitiveKind.Int16,
            ToWinPhoneInt16(world.BottomWorld, nameof(world.BottomWorld)), nameof(world.BottomWorld));
        SetWinPhonePrimitive(values, 4, WinPhonePrimitiveKind.Int16,
            ToWinPhoneInt16(world.TilesHigh, nameof(world.TilesHigh)), nameof(world.TilesHigh));
        SetWinPhonePrimitive(values, 5, WinPhonePrimitiveKind.Int16,
            ToWinPhoneInt16(world.TilesWide, nameof(world.TilesWide)), nameof(world.TilesWide));
        SetWinPhonePrimitive(values, 6, WinPhonePrimitiveKind.Int16,
            ToWinPhoneInt16(world.DungeonX, nameof(world.DungeonX)), nameof(world.DungeonX));
        SetWinPhonePrimitive(values, 7, WinPhonePrimitiveKind.Int16,
            ToWinPhoneInt16(world.DungeonY, nameof(world.DungeonY)), nameof(world.DungeonY));
        SetWinPhonePrimitive(values, 8, WinPhonePrimitiveKind.Int16,
            ToWinPhoneInt16(world.GroundLevel, nameof(world.GroundLevel)), nameof(world.GroundLevel));
        SetWinPhonePrimitive(values, 9, WinPhonePrimitiveKind.Int16,
            ToWinPhoneInt16(world.RockLevel, nameof(world.RockLevel)), nameof(world.RockLevel));
        SetWinPhonePrimitive(values, 10, WinPhonePrimitiveKind.Int32,
            BitConverter.SingleToInt32Bits(checked((float)world.Time)), nameof(world.Time));
        SetWinPhonePrimitive(values, 11, WinPhonePrimitiveKind.Byte,
            world.DayTime ? 1 : 0, nameof(world.DayTime));
        SetWinPhonePrimitive(values, 12, WinPhonePrimitiveKind.Byte,
            ToWinPhoneByte(world.MoonPhase, nameof(world.MoonPhase)), nameof(world.MoonPhase));
        SetWinPhonePrimitive(values, 13, WinPhonePrimitiveKind.Byte,
            world.BloodMoon ? 1 : 0, nameof(world.BloodMoon));

        int spawnIndex = world.Version >= 58 ? 17 : 15;
        SetWinPhonePrimitive(values, spawnIndex, WinPhonePrimitiveKind.Int16,
            ToWinPhoneInt16(world.SpawnX, nameof(world.SpawnX)), nameof(world.SpawnX));
        SetWinPhonePrimitive(values, spawnIndex + 1, WinPhonePrimitiveKind.Int16,
            ToWinPhoneInt16(world.SpawnY, nameof(world.SpawnY)), nameof(world.SpawnY));
        if (world.Version >= 58)
        {
            if (world.TempRainTime < 0)
                throw new InvalidDataException("Windows Phone rain time cannot be negative.");
            if (!float.IsFinite(world.TempMaxRain) || world.TempMaxRain is < 0 or > 1)
                throw new InvalidDataException("Windows Phone maximum rain intensity must be between 0 and 1.");
            SetWinPhonePrimitive(values, 19, WinPhonePrimitiveKind.Byte,
                world.IsCrimson ? 1 : 0, nameof(world.IsCrimson));
            SetWinPhonePrimitive(values, 20, WinPhonePrimitiveKind.Byte,
                world.IsRaining ? 1 : 0, nameof(world.IsRaining));
            SetWinPhonePrimitive(values, 21, WinPhonePrimitiveKind.Int32,
                world.TempRainTime, nameof(world.TempRainTime));
            SetWinPhonePrimitive(values, 22, WinPhonePrimitiveKind.Int32,
                BitConverter.SingleToInt32Bits(world.TempMaxRain), nameof(world.TempMaxRain));
        }

        int progressionIndex = world.Version >= 58 ? 24 : 17;
        SetWinPhoneBoolean(values, progressionIndex, world.DownedBoss1EyeofCthulhu,
            nameof(world.DownedBoss1EyeofCthulhu));
        SetWinPhoneBoolean(values, progressionIndex + 1, world.DownedBoss2EaterofWorlds,
            nameof(world.DownedBoss2EaterofWorlds));
        SetWinPhoneBoolean(values, progressionIndex + 2, world.DownedBoss3Skeletron,
            nameof(world.DownedBoss3Skeletron));
        SetWinPhoneBoolean(values, progressionIndex + 3, world.SavedGoblin, nameof(world.SavedGoblin));
        SetWinPhoneBoolean(values, progressionIndex + 4, world.SavedWizard, nameof(world.SavedWizard));
        SetWinPhoneBoolean(values, progressionIndex + 5, world.SavedMech, nameof(world.SavedMech));
        SetWinPhoneBoolean(values, progressionIndex + 6, world.DownedGoblins, nameof(world.DownedGoblins));
        SetWinPhoneBoolean(values, progressionIndex + 7, world.DownedClown, nameof(world.DownedClown));
        SetWinPhoneBoolean(values, progressionIndex + 8, world.DownedFrost, nameof(world.DownedFrost));
        if (world.Version >= 58)
        {
            SetWinPhoneBoolean(values, progressionIndex + 11, world.DownedQueenBee, nameof(world.DownedQueenBee));
            SetWinPhoneBoolean(values, progressionIndex + 12, world.DownedMechBoss1TheDestroyer,
                nameof(world.DownedMechBoss1TheDestroyer));
            SetWinPhoneBoolean(values, progressionIndex + 13, world.DownedMechBoss2TheTwins,
                nameof(world.DownedMechBoss2TheTwins));
            SetWinPhoneBoolean(values, progressionIndex + 14, world.DownedMechBoss3SkeletronPrime,
                nameof(world.DownedMechBoss3SkeletronPrime));
            SetWinPhoneBoolean(values, progressionIndex + 15, world.DownedMechBossAny,
                nameof(world.DownedMechBossAny));
            SetWinPhoneBoolean(values, progressionIndex + 16, world.DownedPlantBoss, nameof(world.DownedPlantBoss));
            SetWinPhoneBoolean(values, progressionIndex + 17, world.DownedGolemBoss, nameof(world.DownedGolemBoss));
            SetWinPhoneBoolean(values, progressionIndex + 18, world.DownedPirates, nameof(world.DownedPirates));
        }

        int worldStateIndex = world.Version >= 58 ? 43 : 26;
        SetWinPhoneBoolean(values, worldStateIndex, world.ShadowOrbSmashed, nameof(world.ShadowOrbSmashed));
        SetWinPhoneBoolean(values, worldStateIndex + 1, world.SpawnMeteor, nameof(world.SpawnMeteor));
        SetWinPhonePrimitive(values, worldStateIndex + 2, WinPhonePrimitiveKind.Byte,
            ToWinPhoneByte(world.ShadowOrbCount, nameof(world.ShadowOrbCount)), nameof(world.ShadowOrbCount));
        SetWinPhonePrimitive(values, worldStateIndex + 3, WinPhonePrimitiveKind.Int32,
            world.AltarCount, nameof(world.AltarCount));
        if (world.Version >= 58)
        {
            SetWinPhonePrimitive(values, worldStateIndex + 4, WinPhonePrimitiveKind.Int16,
                ToWinPhoneInt16(world.SavedOreTiersCobalt, nameof(world.SavedOreTiersCobalt)),
                nameof(world.SavedOreTiersCobalt));
            SetWinPhonePrimitive(values, worldStateIndex + 5, WinPhonePrimitiveKind.Int16,
                ToWinPhoneInt16(world.SavedOreTiersMythril, nameof(world.SavedOreTiersMythril)),
                nameof(world.SavedOreTiersMythril));
            SetWinPhonePrimitive(values, worldStateIndex + 6, WinPhonePrimitiveKind.Int16,
                ToWinPhoneInt16(world.SavedOreTiersAdamantite, nameof(world.SavedOreTiersAdamantite)),
                nameof(world.SavedOreTiersAdamantite));
        }

        int hardModeIndex = worldStateIndex + (world.Version >= 58 ? 7 : 4);
        SetWinPhoneBoolean(values, hardModeIndex, world.HardMode, nameof(world.HardMode));
        int invasionIndex = hardModeIndex + (world.Version > 55 ? 2 : 1);
        SetWinPhonePrimitive(values, invasionIndex, WinPhonePrimitiveKind.Byte,
            ToWinPhoneByte(world.InvasionDelay, nameof(world.InvasionDelay)), nameof(world.InvasionDelay));
        SetWinPhonePrimitive(values, invasionIndex + 1, WinPhonePrimitiveKind.Int16,
            ToWinPhoneInt16(world.InvasionSize, nameof(world.InvasionSize)), nameof(world.InvasionSize));
        SetWinPhonePrimitive(values, invasionIndex + 2, WinPhonePrimitiveKind.Byte,
            ToWinPhoneByte(world.InvasionType, nameof(world.InvasionType)), nameof(world.InvasionType));
        if (!double.IsFinite(world.InvasionX) || world.InvasionX is < -float.MaxValue or > float.MaxValue)
            throw new InvalidDataException("Windows Phone invasion position must be a finite 32-bit number.");
        SetWinPhonePrimitive(values, invasionIndex + 3, WinPhonePrimitiveKind.Int32,
            BitConverter.SingleToInt32Bits((float)world.InvasionX), nameof(world.InvasionX));
        return values;
    }

    private static void SetWinPhoneBoolean(
        WinPhonePrimitive[] values,
        int index,
        bool value,
        string fieldName) =>
        SetWinPhonePrimitive(values, index, WinPhonePrimitiveKind.Byte, value ? 1 : 0, fieldName);

    private static void SetWinPhonePrimitive(
        WinPhonePrimitive[] values,
        int index,
        WinPhonePrimitiveKind expectedKind,
        int value,
        string fieldName)
    {
        if ((uint)index >= (uint)values.Length || values[index].Kind != expectedKind)
            throw new InvalidDataException($"Windows Phone metadata layout does not contain {fieldName}.");
        values[index] = new WinPhonePrimitive(expectedKind, value);
    }

    private static byte ToWinPhoneByte(int value, string fieldName)
    {
        if (value is < byte.MinValue or > byte.MaxValue)
            throw new InvalidDataException($"{fieldName} value {value} is outside the Windows Phone byte range.");
        return (byte)value;
    }

    private static short ToWinPhoneInt16(double value, string fieldName)
    {
        if (!double.IsFinite(value) || value != Math.Truncate(value) ||
            value is < short.MinValue or > short.MaxValue)
            throw new InvalidDataException(
                $"{fieldName} value {value} is not a Windows Phone 16-bit integer.");
        return (short)value;
    }

    private static int ToWinPhoneInt32(double value, string fieldName)
    {
        if (!double.IsFinite(value) || value != Math.Truncate(value) ||
            value is < int.MinValue or > int.MaxValue)
            throw new InvalidDataException(
                $"{fieldName} value {value} is not a Windows Phone 32-bit integer.");
        return (int)value;
    }

    private static bool WinPhoneTilesEncodeEqual(World world, int x, int firstY, int secondY)
    {
        if (!world.Tiles[x, firstY].Equals(world.Tiles[x, secondY]))
            return false;
        int first = GetWinPhoneTileIndex(world, x, firstY);
        int second = GetWinPhoneTileIndex(world, x, secondY);
        return world.WinPhoneStoredTileTypes![first] == world.WinPhoneStoredTileTypes[second] &&
               world.WinPhoneTileHeader1![first] == world.WinPhoneTileHeader1[second] &&
               world.WinPhoneTileHeader2![first] == world.WinPhoneTileHeader2[second] &&
               world.WinPhoneTileShape![first] == world.WinPhoneTileShape[second] &&
               world.WinPhoneLegacyTileFlags![first] == world.WinPhoneLegacyTileFlags[second];
    }

    private static bool PrepareWinPhoneTilesForSave(World world, World original, uint version)
    {
        bool anyChanges = false;
        for (int x = 0; x < world.TilesWide; x++)
        {
            bool changedColumn = false;
            for (int y = 0; y < world.TilesHigh && !changedColumn; y++)
                changedColumn = !world.Tiles[x, y].Equals(original.Tiles[x, y]);
            if (!changedColumn)
                continue;
            anyChanges = true;

            for (int y = 0; y < world.TilesHigh; y++)
            {
                int index = GetWinPhoneTileIndex(world, x, y);
                world.WinPhoneTileRunLengths![index] = 0;
                if (!world.Tiles[x, y].Equals(original.Tiles[x, y]))
                    UpdateWinPhoneTileEncodingMetadata(world, version, x, y);
            }
        }
        return anyChanges;
    }

    private static void UpdateWinPhoneTileEncodingMetadata(World world, uint version, int x, int y)
    {
        int index = GetWinPhoneTileIndex(world, x, y);
        Tile tile = world.Tiles[x, y];
        if (tile.Wall > byte.MaxValue)
            throw new InvalidDataException($"Wall type {tile.Wall} at {x}, {y} is outside the Windows Phone range.");

        if (version < 58)
        {
            if (tile.TileColor != 0 || (version <= 50 && tile.WallColor != 0) ||
                tile.Actuator || tile.InActive || tile.WireYellow || tile.BrickStyle != BrickStyle.Full)
                throw new InvalidDataException(
                    $"Tile {x}, {y} uses paint, actuator, yellow-wire, or shape data unavailable in Windows Phone version {version}.");
            if (tile.LiquidAmount != 0 && tile.LiquidType is not (LiquidType.Water or LiquidType.Lava))
                throw new InvalidDataException(
                    $"Liquid {tile.LiquidType} at {x}, {y} is unavailable in Windows Phone version {version}.");

            ushort storedType = tile.Type == 500 ? (ushort)150 : tile.Type;
            if (storedType > byte.MaxValue)
                throw new InvalidDataException(
                    $"Tile type {tile.Type} at {x}, {y} is outside the legacy Windows Phone range.");
            world.WinPhoneStoredTileTypes![index] = storedType;
            world.WinPhoneTileHeader1![index] = tile.IsActive ? (byte)1 : (byte)0;
            world.WinPhoneLegacyTileFlags![index] = (byte)(
                (world.WinPhoneLegacyTileFlags[index] & ~0x70) |
                (tile.WireRed ? 0x10 : 0) |
                (tile.WireGreen ? 0x20 : 0) |
                (tile.WireBlue ? 0x40 : 0));
            return;
        }

        if (tile.Type > 0x1FF)
            throw new InvalidDataException(
                $"Tile type {tile.Type} at {x}, {y} is outside the Windows Phone nine-bit range.");
        if (tile.LiquidAmount != 0 && tile.LiquidType is not (LiquidType.Water or LiquidType.Lava or LiquidType.Honey))
            throw new InvalidDataException(
                $"Liquid {tile.LiquidType} at {x}, {y} is unavailable in Windows Phone version {version}.");

        byte header1 = (byte)(world.WinPhoneTileHeader1![index] & 0xC0);
        if (tile.IsActive) header1 |= 0x01;
        if (tile.WireRed) header1 |= 0x02;
        if (tile.WireGreen) header1 |= 0x04;
        if (tile.WireBlue) header1 |= 0x08;
        if (tile.Actuator) header1 |= 0x10;
        if (tile.InActive) header1 |= 0x20;
        world.WinPhoneTileHeader1[index] = header1;
        world.WinPhoneStoredTileTypes![index] = tile.Type;

        byte header2 = (byte)(world.WinPhoneTileHeader2![index] & ~0x10);
        if (tile.WireYellow) header2 |= 0x10;
        world.WinPhoneTileHeader2[index] = header2;
        world.WinPhoneTileShape![index] = (byte)(
            (world.WinPhoneTileShape[index] & 0x0F) | ((byte)tile.BrickStyle << 4));
    }

    private static void WriteWinPhoneLegacyTile(
        BinaryWriter writer,
        World world,
        bool[] frameImportant,
        int x,
        int y,
        int run,
        uint version)
    {
        int index = GetWinPhoneTileIndex(world, x, y);
        Tile tile = world.Tiles[x, y];
        bool storedActive = world.WinPhoneTileHeader1![index] != 0;
        writer.Write(storedActive);
        if (storedActive)
        {
            byte storedType = (byte)world.WinPhoneStoredTileTypes![index];
            writer.Write(storedType);
            if (IsWinPhoneLegacyFramed(storedType))
            {
                writer.Write(tile.U);
                short storedV = tile.V;
                if (storedType == 35)
                    storedV -= 54;
                else if (storedType == 36)
                    storedV -= 108;
                writer.Write(storedV);
            }
        }
        writer.Write((byte)tile.Wall);
        if (version > 50)
            writer.Write(tile.WallColor);
        writer.Write(tile.LiquidAmount);
        if (tile.LiquidAmount != 0)
            writer.Write(tile.LiquidType == LiquidType.Lava);
        writer.Write(world.WinPhoneLegacyTileFlags![index]);
        writer.Write((short)run);
    }

    private static void WriteWinPhoneCompactTile(
        BinaryWriter writer,
        World world,
        bool[] frameImportant,
        int x,
        int y,
        int run,
        uint version)
    {
        int index = GetWinPhoneTileIndex(world, x, y);
        Tile tile = world.Tiles[x, y];
        byte header1 = world.WinPhoneTileHeader1![index];
        writer.Write(header1);
        if ((header1 & 1) != 0)
        {
            ushort storedType = world.WinPhoneStoredTileTypes![index];
            writer.Write(storedType);
            if (storedType < frameImportant.Length && frameImportant[storedType])
            {
                writer.Write(tile.U);
                writer.Write(tile.V);
            }
            writer.Write(tile.TileColor);
        }
        writer.Write((byte)tile.Wall);
        if (tile.Wall != 0)
            writer.Write(tile.WallColor);
        writer.Write(tile.LiquidAmount);
        if (tile.LiquidAmount != 0)
            writer.Write((byte)((int)tile.LiquidType - 1));
        writer.Write(world.WinPhoneTileHeader2![index]);
        if (version is >= 60 and <= 68)
            writer.Write(world.WinPhoneTileShape![index]);
        if (run < 0x80)
        {
            writer.Write((byte)run);
        }
        else
        {
            writer.Write((byte)((run & 0x7F) | 0x80));
            writer.Write((byte)(run >> 7));
        }
    }

    private static void WriteWinPhoneChests(BinaryWriter writer, World world, uint version)
    {
        Chest?[] slots = world.WinPhoneChestSlots!;
        int itemCount = version < 58 ? 20 : 40;
        byte[] itemMask = new byte[5];
        for (int chestIndex = 0; chestIndex < Chest.LegacyLimit; chestIndex++)
        {
            Chest? chest = slots[chestIndex];
            writer.Write(chest is not null);
            if (chest is null)
                continue;

            writer.Write(ToWinPhoneInt16(chest.X, "Chest X"));
            writer.Write(ToWinPhoneInt16(chest.Y, "Chest Y"));
            if (version < 58)
            {
                for (int slot = itemCount; slot < chest.Items.Count; slot++)
                    if (chest.Items[slot].StackSize > 0 && chest.Items[slot].NetId != 0)
                        throw new InvalidDataException(
                            $"Windows Phone version {version} chests support only {itemCount} item slots.");
                for (int slot = 0; slot < itemCount; slot++)
                    WriteWinPhoneChestItem(writer, chest.Items[slot], version);
                continue;
            }

            Array.Clear(itemMask);
            for (int slot = 0; slot < itemCount; slot++)
            {
                Item item = chest.Items[slot];
                if (item.StackSize > 0 && item.NetId != 0)
                    itemMask[slot >> 3] |= (byte)(1 << (slot & 7));
            }
            writer.Write((byte)itemMask.Length);
            writer.Write(itemMask);
            for (int slot = 0; slot < itemCount; slot++)
            {
                if ((itemMask[slot >> 3] & 1 << (slot & 7)) != 0)
                    WriteWinPhoneChestItem(writer, chest.Items[slot], version);
            }
        }
    }

    private static void WriteWinPhoneChestItem(BinaryWriter writer, Item item, uint version)
    {
        int stack = item.NetId == 0 ? 0 : item.StackSize;
        int maxStack = version < 55 ? byte.MaxValue : short.MaxValue;
        if (stack is < 0 || stack > maxStack)
            throw new InvalidDataException(
                $"Item stack {stack} is outside the Windows Phone version {version} range.");
        if (item.NetId is < short.MinValue or > short.MaxValue)
            throw new InvalidDataException($"Item ID {item.NetId} is outside the Windows Phone range.");
        if (version < 55)
            writer.Write((byte)stack);
        else
            writer.Write((short)stack);
        if (stack <= 0)
            return;
        writer.Write((short)item.NetId);
        writer.Write(item.Prefix);
    }

    private static void WriteWinPhoneSigns(BinaryWriter writer, World world, uint version)
    {
        Sign?[] slots = world.WinPhoneSignSlots!;
        for (int signIndex = 0; signIndex < Sign.LegacyLimit; signIndex++)
        {
            Sign? sign = slots[signIndex];
            writer.Write(sign is not null);
            if (sign is null)
                continue;
            writer.Write(ToWinPhoneInt16(sign.X, "Sign X"));
            writer.Write(ToWinPhoneInt16(sign.Y, "Sign Y"));
            if (version > 57)
                writer.Write(world.WinPhoneSignTextFlags![signIndex]);
            WriteWinPhoneString(writer, sign.Text, version);
        }
    }

    private static void WriteWinPhoneNpcs(BinaryWriter writer, World world)
    {
        foreach (NPC npc in world.NPCs)
        {
            if (npc.SpriteId is < byte.MinValue or > byte.MaxValue)
                throw new InvalidDataException($"NPC ID {npc.SpriteId} is outside the Windows Phone range.");
            if (!float.IsFinite(npc.Position.X) || !float.IsFinite(npc.Position.Y))
                throw new InvalidDataException("NPC positions must be finite Windows Phone float values.");
            writer.Write(true);
            writer.Write((byte)npc.SpriteId);
            writer.Write(npc.Position.X);
            writer.Write(npc.Position.Y);
            writer.Write(npc.IsHomeless);
            writer.Write(ToWinPhoneInt16(npc.Home.X, "NPC home X"));
            writer.Write(ToWinPhoneInt16(npc.Home.Y, "NPC home Y"));
        }
        writer.Write(false);
    }

    private static void WriteWinPhoneCharacterNames(BinaryWriter writer, World world, uint version)
    {
        int nameCount = version < 50 ? 10 : WinPhoneCharacterNameIds.Length;
        for (int index = 0; index < nameCount; index++)
        {
            int id = WinPhoneCharacterNameIds[index];
            string name = string.Empty;
            foreach (NpcName candidate in world.CharacterNames)
            {
                if (candidate.Id != id)
                    continue;
                name = candidate.Name;
                break;
            }
            WriteWinPhoneString(writer, name, version);
        }
    }

    private static void WriteWinPhoneString(BinaryWriter writer, string value, uint version)
    {
        value ??= string.Empty;
        writer.Write(value.Length);
        writer.Write((version < 50 ? Encoding.UTF8 : Encoding.Unicode).GetBytes(value));
    }

    /// <summary>
    /// Writes the original native container without normalization. This is the
    /// lossless path used for an unedited Windows Phone world.
    /// </summary>
    public static void SaveWinPhoneUnchanged(World world, Stream output)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(output);

        if (!world.IsWinPhone || world.WinPhoneSourceData is null)
            throw new InvalidOperationException("The world was not loaded from a Windows Phone world file.");

        output.Write(world.WinPhoneSourceData, 0, world.WinPhoneSourceData.Length);
    }

    /// <summary>
    /// Rebuilds every decoded native section. Only the still-unmapped metadata
    /// prefix and the v49 fat-resource suffix are copied as opaque bytes.
    /// </summary>
    internal static void SaveWinPhoneReconstructed(World world, Stream output)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(output);
        if (!world.IsWinPhone || world.WinPhoneSourceData is null)
            throw new InvalidOperationException("The world was not loaded from a Windows Phone world file.");
        if (world.WinPhoneTileSectionOffset <= 0 ||
            world.WinPhoneParsedLength < world.WinPhoneDataSectionOffset)
            throw new InvalidOperationException("Load the complete Windows Phone world before reconstructing it.");

        byte[] prefix = SerializeWinPhonePrefix(world);
        byte[] tiles = SerializeWinPhoneTiles(world, world.Version);
        byte[] dataSections = SerializeWinPhoneDataSections(world, world.Version);
        using MemoryStream native = new();
        native.Write(prefix);
        native.Write(tiles);
        using (BinaryWriter writer = new(native, Encoding.UTF8, leaveOpen: true))
            writer.Write(0x162E);
        native.Write(dataSections);
        native.Write(
            world.WinPhoneSourceData,
            world.WinPhoneParsedLength,
            world.WinPhoneSourceData.Length - world.WinPhoneParsedLength);

        byte[] result = native.ToArray();
        if (world.WinPhoneStoredCrc != 0)
        {
            uint crc = ComputeWinPhoneCrc32(result.AsSpan(8));
            BitConverter.TryWriteBytes(result.AsSpan(4, 4), crc);
        }
        output.Write(result);
    }

    /// <summary>
    /// Standard save-path guard. Until edited native serialization is
    /// available, only a fully loaded, semantically unchanged world may use
    /// the lossless source-container path.
    /// </summary>
    private static void SaveWinPhoneVerifiedUnchanged(World world, Stream output)
    {
        if (world.WinPhoneSourceData is null ||
            !TryReadWinPhoneHeader(world.WinPhoneSourceData, out WinPhoneHeader header))
            throw new InvalidOperationException("The original Windows Phone world data is unavailable.");

        if (world.Version != header.Version || world.RightWorld != header.RightWorld ||
            world.BottomWorld != header.BottomWorld || world.TilesHigh != header.TilesHigh ||
            world.TilesWide != header.TilesWide)
            throw new NotSupportedException(
                "Changing Windows Phone world dimensions or native version is not supported yet.");

        if (world.Tiles is null || world.Tiles.Length != world.TilesWide * world.TilesHigh)
            throw new InvalidOperationException(
                "Load the complete Windows Phone world before saving it through World.Save.");

        World original = new()
        {
            Version = header.Version,
            TilesWide = header.TilesWide,
            TilesHigh = header.TilesHigh,
        };
        int originalDataOffset = LoadWinPhoneTiles(
            world.WinPhoneSourceData, header, original, progress: null);
        LoadWinPhoneSections(world.WinPhoneSourceData, header, original, originalDataOffset);

        bool tileEdits = PrepareWinPhoneTilesForSave(world, original, header.Version);

        byte[] serializedTiles = SerializeWinPhoneTiles(world, header.Version);
        ReadOnlySpan<byte> originalTiles = world.WinPhoneSourceData.AsSpan(
            world.WinPhoneTileSectionOffset,
            world.WinPhoneDataSectionOffset - world.WinPhoneTileSectionOffset - 4);
        if (!tileEdits && !serializedTiles.AsSpan().SequenceEqual(originalTiles))
        {
            int commonLength = Math.Min(serializedTiles.Length, originalTiles.Length);
            int difference = 0;
            while (difference < commonLength && serializedTiles[difference] == originalTiles[difference])
                difference++;
            int diagnosticLength = Math.Min(16, commonLength - difference);
            throw new InvalidDataException(
                $"Windows Phone tile serialization differed at relative offset {difference} " +
                $"(serialized length {serializedTiles.Length}, original length {originalTiles.Length}); " +
                $"serialized={Convert.ToHexString(serializedTiles.AsSpan(difference, diagnosticLength))}, " +
                $"original={Convert.ToHexString(originalTiles.Slice(difference, diagnosticLength))}.");
        }

        SaveWinPhoneReconstructed(world, output);
    }

    internal static bool TryReadWinPhoneHeader(ReadOnlySpan<byte> source, out WinPhoneHeader header)
    {
        header = default;
        if (source.Length < 40)
            return false;

        uint version = ReadUInt32(source, 0);
        if (version is < 39 or > 65)
            return false;

        uint storedCrc = ReadUInt32(source, 4);
        int titleLength = ReadInt32(source, 8);
        if (titleLength is < 0 or > WinPhoneMaxTitleLength)
            return false;

        int titleByteLength = version < 50 ? titleLength : checked(titleLength * 2);
        int position = checked(12 + titleByteLength);
        if (position > source.Length)
            return false;

        string title = version < 50
            ? Encoding.UTF8.GetString(source.Slice(12, titleByteLength))
            : Encoding.Unicode.GetString(source.Slice(12, titleByteLength));

        byte cloudFlag = 0;
        uint[] history = [];

        if (version >= 53)
        {
            if (position >= source.Length)
                return false;
            cloudFlag = source[position++];
            if (version == 53)
            {
                if (position + 4 > source.Length)
                    return false;
                history = [ReadUInt32(source, position)];
                position += 4;
            }
            else
            {
                if (position + 4 > source.Length)
                    return false;
                int historyCount = ReadInt32(source, position);
                if (historyCount is < 0 or > 100)
                    return false;
                position += 4;
                history = new uint[historyCount];
                for (int index = 0; index < historyCount; index++)
                {
                    history[index] = ReadUInt32(source, position);
                    position += 4;
                }
            }
        }

        // Native body prefix recovered from FUN_0078d0d8. The two pixel bounds
        // must exactly match the compact 16-bit tile dimensions; this is the
        // discriminator that prevents ordinary desktop V1 files being claimed.
        if (position + 26 > source.Length)
            return false;

        int worldId = ReadInt32(source, position);
        int rightWorld = ReadInt32(source, position + 8);
        short bottomWorld = ReadInt16(source, position + 12);
        short tilesHigh = ReadInt16(source, position + 14);
        short tilesWide = ReadInt16(source, position + 16);
        short dungeonX = ReadInt16(source, position + 18);
        short dungeonY = ReadInt16(source, position + 20);
        short groundLevel = ReadInt16(source, position + 22);
        short rockLevel = ReadInt16(source, position + 24);
        int spawnOffset = position + 26 + (version >= 58 ? 11 : 9);
        if (spawnOffset + 4 > source.Length)
            return false;
        short spawnX = ReadInt16(source, spawnOffset);
        short spawnY = ReadInt16(source, spawnOffset + 2);

        if (tilesWide <= 0 || tilesHigh <= 0 ||
            rightWorld != tilesWide * 16 || bottomWorld != tilesHigh * 16)
            return false;

        // Tutorial v49 intentionally stores zero. Later native saves use the
        // standard reflected CRC-32 over every byte after the first eight.
        if (storedCrc != 0 && storedCrc != ComputeWinPhoneCrc32(source[8..]))
            return false;

        header = new WinPhoneHeader(
            version,
            storedCrc,
            title,
            position,
            worldId,
            rightWorld,
            bottomWorld,
            tilesHigh,
            tilesWide,
            dungeonX,
            dungeonY,
            spawnX,
            spawnY,
            groundLevel,
            rockLevel);
        header = header with { CloudFlag = cloudFlag, History = history };
        return true;
    }

    private static uint ComputeWinPhoneCrc32(ReadOnlySpan<byte> data)
    {
        uint crc = uint.MaxValue;
        foreach (byte value in data)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
                crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xEDB88320u : 0u);
        }
        return ~crc;
    }

    private static short ReadInt16(ReadOnlySpan<byte> source, int offset) =>
        (short)(source[offset] | source[offset + 1] << 8);

    private static int ReadInt32(ReadOnlySpan<byte> source, int offset) =>
        source[offset] |
        source[offset + 1] << 8 |
        source[offset + 2] << 16 |
        source[offset + 3] << 24;

    private static uint ReadUInt32(ReadOnlySpan<byte> source, int offset) =>
        unchecked((uint)ReadInt32(source, offset));

    internal readonly record struct WinPhoneHeader(
        uint Version,
        uint Crc,
        string Title,
        int BodyOffset,
        int WorldId,
        int RightWorld,
        short BottomWorld,
        short TilesHigh,
        short TilesWide,
        short DungeonX,
        short DungeonY,
        short SpawnX,
        short SpawnY,
        short GroundLevel,
        short RockLevel,
        byte CloudFlag = 0,
        uint[]? History = null);

    internal enum WinPhonePrimitiveKind : byte
    {
        Byte,
        Int16,
        Int32,
    }

    internal readonly record struct WinPhonePrimitive(WinPhonePrimitiveKind Kind, int Value);
}
