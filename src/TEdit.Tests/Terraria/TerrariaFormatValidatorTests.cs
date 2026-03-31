using System;
using System.IO;
using TEdit.Terraria;
using TEdit.Terraria.IO;
using Xunit;
using Xunit.Abstractions;

namespace TEdit.Terraria.Tests;

/// <summary>
/// Validates world files using Terraria's exact validation logic from the
/// decompiled server source. This catches format issues that TEdit's lenient
/// loader would accept but the game would reject.
/// </summary>
public class TerrariaFormatValidatorTests
{
    private readonly ITestOutputHelper _output;

    public TerrariaFormatValidatorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ─── Terraria-exact validation (from decompiled WorldFile.cs) ───

    /// <summary>
    /// Reimplementation of Terraria's ValidateWorld method.
    /// Returns (isValid, failureReason) for diagnostic purposes.
    /// </summary>
    private (bool valid, string reason) TerrariaValidateWorld(byte[] rawData)
    {
        using var ms = new MemoryStream(rawData);
        using var r = new BinaryReader(ms);

        try
        {
            int version = r.ReadInt32();
            if (version == 0 || version > 319)
                return (false, $"Version {version} out of range (0 < v <= 319)");

            ms.Position = 0;

            // LoadFileFormatHeader
            int ver = r.ReadInt32();
            if (ver >= 135)
            {
                // FileMetadata.Read - 7 bytes header + 1 byte type + 4 bytes revision + 8 bytes flags
                r.ReadBytes(7); // "relogic"
                byte fileType = r.ReadByte();
                r.ReadUInt32(); // revision
                r.ReadUInt64(); // flags
            }

            short sectionCount = r.ReadInt16();
            int[] positions = new int[sectionCount];
            for (int i = 0; i < sectionCount; i++)
                positions[i] = r.ReadInt32();

            // Tile importance flags
            ushort tileTypeCount = r.ReadUInt16();
            bool[] importance = new bool[tileTypeCount];
            byte bits = 0;
            byte mask = 128;
            for (int i = 0; i < tileTypeCount; i++)
            {
                if (mask == 128)
                {
                    bits = r.ReadByte();
                    mask = 1;
                }
                else
                {
                    mask <<= 1;
                }
                if ((bits & mask) == mask)
                    importance[i] = true;
            }

            // === Section 0: Header validation ===
            // Read world name (needed for footer check)
            string worldName = r.ReadString();

            if (version >= 179)
            {
                if (version == 179)
                    r.ReadInt32();
                else
                    r.ReadString(); // seed
                r.ReadUInt64(); // WorldGeneratorVersion
            }
            if (version >= 181)
                r.ReadBytes(16); // UniqueId (GUID)

            int worldId = r.ReadInt32();
            r.ReadInt32(); // leftWorld
            r.ReadInt32(); // rightWorld
            r.ReadInt32(); // topWorld
            r.ReadInt32(); // bottomWorld
            int maxTilesX = r.ReadInt32();
            int maxTilesY = r.ReadInt32();

            _output.WriteLine($"World: \"{worldName}\" v{version} {maxTilesX}x{maxTilesY} id={worldId}");
            _output.WriteLine($"Section count: {sectionCount}");
            for (int i = 0; i < sectionCount; i++)
                _output.WriteLine($"  Section[{i}]: {positions[i]:N0}");

            // Skip to section 1 (tiles) using section pointer
            // We DON'T validate header completeness — jump to tiles
            ms.Position = positions[1];

            // === Section 1: Tiles ===
            _output.WriteLine($"\nValidating Tiles (start={positions[1]:N0})...");
            for (int y = 0; y < maxTilesY; y++)
            {
                for (int x = 0; x < maxTilesX;)
                {
                    byte h3 = 0;
                    byte h2 = 0;
                    byte h1 = r.ReadByte();
                    bool hasH2 = (h1 & 1) == 1;
                    if (hasH2)
                        h2 = r.ReadByte();
                    bool hasH3 = hasH2 && (h2 & 1) == 1;
                    if (hasH3)
                        h3 = r.ReadByte();
                    if (hasH3 && (h3 & 1) == 1)
                        r.ReadByte(); // header4

                    // Tile type
                    if ((h1 & 2) == 2)
                    {
                        int tileType;
                        if ((h1 & 32) == 32)
                        {
                            byte lo = r.ReadByte();
                            tileType = (r.ReadByte() << 8) | lo;
                        }
                        else
                        {
                            tileType = r.ReadByte();
                        }
                        if (tileType < importance.Length && importance[tileType])
                        {
                            r.ReadInt16(); // frameX
                            r.ReadInt16(); // frameY
                        }
                        if ((h3 & 8) == 8)
                            r.ReadByte(); // tile color
                    }

                    // Wall
                    if ((h1 & 4) == 4)
                    {
                        r.ReadByte(); // wall type
                        if ((h3 & 16) == 16)
                            r.ReadByte(); // wall color
                    }

                    // Liquid
                    if (((h1 & 24) >> 3) != 0)
                        r.ReadByte(); // liquid amount

                    // Wall high byte
                    if ((h3 & 64) == 64)
                        r.ReadByte();

                    // RLE count
                    int rle;
                    switch ((byte)((h1 & 192) >> 6))
                    {
                        case 0: rle = 0; break;
                        case 1: rle = r.ReadByte(); break;
                        default: rle = r.ReadInt16(); break;
                    }
                    x += rle + 1;
                }
            }

            if (ms.Position != positions[2])
                return (false, $"TILE SECTION END MISMATCH: reader at {ms.Position:N0}, expected {positions[2]:N0} (diff={ms.Position - positions[2]:+#;-#;0})");
            _output.WriteLine($"  Tiles OK (end={ms.Position:N0})");

            // === Section 2: Chests ===
            _output.WriteLine($"Validating Chests (start={positions[2]:N0})...");
            int chestCount = r.ReadInt16();
            int maxItems = 0;
            if (version < 294)
                maxItems = r.ReadInt16();
            for (int i = 0; i < chestCount; i++)
            {
                r.ReadInt32(); // x
                r.ReadInt32(); // y
                r.ReadString(); // name
                if (version >= 294)
                    maxItems = r.ReadInt32();
                for (int j = 0; j < maxItems; j++)
                {
                    short stack = r.ReadInt16();
                    if (stack > 0)
                    {
                        r.ReadInt32(); // netId
                        r.ReadByte(); // prefix
                    }
                }
            }

            if (ms.Position != positions[3])
                return (false, $"CHEST SECTION END MISMATCH: reader at {ms.Position:N0}, expected {positions[3]:N0} (diff={ms.Position - positions[3]:+#;-#;0})");
            _output.WriteLine($"  Chests OK ({chestCount} chests, end={ms.Position:N0})");

            // === Section 3: Signs ===
            _output.WriteLine($"Validating Signs (start={positions[3]:N0})...");
            int signCount = r.ReadInt16();
            for (int i = 0; i < signCount; i++)
            {
                r.ReadString(); // text
                r.ReadInt32(); // x
                r.ReadInt32(); // y
            }

            if (ms.Position != positions[4])
                return (false, $"SIGN SECTION END MISMATCH: reader at {ms.Position:N0}, expected {positions[4]:N0} (diff={ms.Position - positions[4]:+#;-#;0})");
            _output.WriteLine($"  Signs OK ({signCount} signs, end={ms.Position:N0})");

            // === Section 4: NPCs ===
            _output.WriteLine($"Validating NPCs (start={positions[4]:N0})...");
            long npcSectionStart = ms.Position;

            // ShimmeredTownNPCs (v268+)
            if (version >= 268)
            {
                int shimCount = r.ReadInt32();
                for (int i = 0; i < shimCount; i++)
                    r.ReadInt32();
                _output.WriteLine($"  Shimmered NPCs: {shimCount}");
            }

            // Town NPCs
            int npcCount = 0;
            while (r.ReadBoolean()) // hasMore
            {
                npcCount++;
                if (version >= 190)
                    r.ReadInt32(); // spriteId
                else
                    r.ReadString(); // npcName
                r.ReadString(); // displayName
                r.ReadSingle(); // posX
                r.ReadSingle(); // posY
                r.ReadBoolean(); // isHomeless
                r.ReadInt32(); // homeX
                r.ReadInt32(); // homeY

                if (version >= 213)
                {
                    byte flags = r.ReadByte();
                    if ((flags & 1) == 1)
                        r.ReadInt32(); // townNpcVariationIndex
                }

                if (version >= 315)
                    r.ReadBoolean(); // homelessDespawn
            }
            _output.WriteLine($"  Town NPCs: {npcCount}");

            // Mobs (v140+)
            int mobCount = 0;
            if (version >= 140)
            {
                while (r.ReadBoolean()) // hasMore
                {
                    mobCount++;
                    if (version >= 190)
                        r.ReadInt32(); // spriteId
                    else
                        r.ReadString(); // npcName
                    r.ReadSingle(); // posX
                    r.ReadSingle(); // posY
                }
            }
            _output.WriteLine($"  Mobs: {mobCount}");

            if (ms.Position != positions[5])
                return (false, $"NPC SECTION END MISMATCH: reader at {ms.Position:N0}, expected {positions[5]:N0} (diff={ms.Position - positions[5]:+#;-#;0})");
            _output.WriteLine($"  NPCs OK (end={ms.Position:N0})");

            // === Section 5: Tile Entities ===
            _output.WriteLine($"Validating TileEntities (start={positions[5]:N0})...");
            if (version >= 122)
            {
                int teCount = r.ReadInt32();
                for (int i = 0; i < teCount; i++)
                {
                    byte teType = r.ReadByte();
                    r.ReadInt32(); // id
                    short teX = r.ReadInt16();
                    short teY = r.ReadInt16();
                    switch (teType)
                    {
                        case 0: // TETrainingDummy
                            r.ReadInt16(); // npc
                            break;
                        case 1: // TEItemFrame
                            r.ReadInt16(); // itemId
                            r.ReadByte(); // prefix
                            r.ReadInt16(); // stack
                            break;
                        case 2: // TELogicSensor
                            r.ReadByte(); // logicCheck
                            r.ReadBoolean(); // on
                            break;
                        case 3: // TEDisplayDoll
                            ReadDisplayDoll(r);
                            break;
                        case 4: // TEWeaponsRack
                            r.ReadInt16(); // itemId
                            r.ReadByte(); // prefix
                            r.ReadInt16(); // stack
                            break;
                        case 5: // TEHatRack
                            ReadHatRack(r);
                            break;
                        case 6: // TEFoodPlatter
                            r.ReadInt16(); // itemId
                            r.ReadByte(); // prefix
                            r.ReadInt16(); // stack
                            break;
                        case 7: // TETeleportationPylon
                            break; // no extra data
                        default:
                            return (false, $"Unknown tile entity type {teType} at position {ms.Position:N0}");
                    }
                }
                _output.WriteLine($"  TileEntities: {teCount}");
            }

            if (ms.Position != positions[6])
                return (false, $"TILE ENTITY SECTION END MISMATCH: reader at {ms.Position:N0}, expected {positions[6]:N0} (diff={ms.Position - positions[6]:+#;-#;0})");
            _output.WriteLine($"  TileEntities OK (end={ms.Position:N0})");

            // === Section 6: Pressure Plates ===
            if (version >= 170)
            {
                int ppCount = r.ReadInt32();
                for (int i = 0; i < ppCount; i++)
                    r.ReadInt64(); // x,y packed
                _output.WriteLine($"  PressurePlates: {ppCount}");
            }

            if (ms.Position != positions[7])
                return (false, $"PRESSURE PLATE SECTION END MISMATCH: reader at {ms.Position:N0}, expected {positions[7]:N0}");
            _output.WriteLine($"  PressurePlates OK (end={ms.Position:N0})");

            // === Section 7: Town Manager ===
            if (version >= 189)
            {
                int tmCount = r.ReadInt32();
                r.ReadBytes(12 * tmCount);
                _output.WriteLine($"  TownManager rooms: {tmCount}");
            }

            if (ms.Position != positions[8])
                return (false, $"TOWN MANAGER SECTION END MISMATCH: reader at {ms.Position:N0}, expected {positions[8]:N0}");
            _output.WriteLine($"  TownManager OK (end={ms.Position:N0})");

            // === Section 8: Bestiary ===
            if (version >= 210)
            {
                // Kills
                int killCount = r.ReadInt32();
                for (int i = 0; i < killCount; i++)
                {
                    r.ReadString();
                    r.ReadInt32();
                }
                // Sights
                int sightCount = r.ReadInt32();
                for (int i = 0; i < sightCount; i++)
                    r.ReadString();
                // Chats
                int chatCount = r.ReadInt32();
                for (int i = 0; i < chatCount; i++)
                    r.ReadString();
                _output.WriteLine($"  Bestiary: kills={killCount} sights={sightCount} chats={chatCount}");
            }

            if (ms.Position != positions[9])
                return (false, $"BESTIARY SECTION END MISMATCH: reader at {ms.Position:N0}, expected {positions[9]:N0}");
            _output.WriteLine($"  Bestiary OK (end={ms.Position:N0})");

            // === Section 9: Creative Powers ===
            if (version >= 220)
            {
                int powerCount = 0;
                while (r.ReadBoolean())
                {
                    ushort powerId = r.ReadUInt16();
                    // Power-specific data: sliders are float (4 bytes), toggles are bool (1 byte)
                    switch (powerId)
                    {
                        case 8:  // time_setspeed (slider)
                        case 12: // setdifficulty (slider)
                        case 14: // setspawnrate (slider)
                            r.ReadSingle();
                            break;
                        default: // all others are booleans
                            r.ReadBoolean();
                            break;
                    }
                    powerCount++;
                }
                _output.WriteLine($"  CreativePowers: {powerCount} powers at {ms.Position:N0}");
            }

            if (ms.Position != positions[10])
                return (false, $"CREATIVE POWERS SECTION END MISMATCH: reader at {ms.Position:N0}, expected {positions[10]:N0}");
            _output.WriteLine($"  CreativePowers OK (end={ms.Position:N0})");

            // === Footer ===
            bool footerValid = r.ReadBoolean();
            string footerName = r.ReadString();
            int footerId = r.ReadInt32();

            _output.WriteLine($"\nFooter: valid={footerValid} name=\"{footerName}\" id={footerId}");
            _output.WriteLine($"Header: name=\"{worldName}\" id={worldId}");

            if (!footerValid)
                return (false, "Footer valid flag is false");
            if (footerName != worldName && footerId != worldId)
                return (false, $"Footer mismatch: name \"{footerName}\" != \"{worldName}\" AND id {footerId} != {worldId}");

            long remaining = ms.Length - ms.Position;
            _output.WriteLine($"Remaining bytes after footer: {remaining}");

            return (true, "VALID");
        }
        catch (Exception ex)
        {
            return (false, $"EXCEPTION at position {ms.Position:N0}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void ReadDisplayDoll(BinaryReader r)
    {
        byte flags1 = r.ReadByte();
        byte flags2 = r.ReadByte();
        for (int i = 0; i < 8; i++)
        {
            if (((flags1 >> i) & 1) == 1)
            {
                r.ReadInt16(); // itemId
                r.ReadByte(); // prefix
                r.ReadInt16(); // stack
            }
        }
        for (int i = 0; i < 8; i++)
        {
            if (((flags2 >> i) & 1) == 1)
            {
                r.ReadInt16(); // dyeId
                r.ReadByte(); // prefix
                r.ReadInt16(); // stack
            }
        }
    }

    private static void ReadHatRack(BinaryReader r)
    {
        byte flags = r.ReadByte();
        // 2 item slots + 2 dye slots
        for (int i = 0; i < 2; i++)
        {
            if (((flags >> i) & 1) == 1)
            {
                r.ReadInt16(); // itemId
                r.ReadByte(); // prefix
                r.ReadInt16(); // stack
            }
        }
        for (int i = 2; i < 4; i++)
        {
            if (((flags >> i) & 1) == 1)
            {
                r.ReadInt16(); // dyeId
                r.ReadByte(); // prefix
                r.ReadInt16(); // stack
            }
        }
    }

    // ─── Test: full header field-by-field validation ───────────────

    /// <summary>
    /// Reads the FULL header using Terraria's exact field ordering and
    /// checks that the reader position matches positions[1] at the end.
    /// This is the critical check that catches version-gate mismatches.
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\ConsoleWorld6.wld")]
    [InlineData(".\\WorldFiles\\ConsoleCookie.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void Validate_Header_FieldByField(string fileName)
    {
        if (!File.Exists(fileName) || new FileInfo(fileName).Length < 500) return;

        // Test both original and round-tripped
        byte[] originalRaw = GetDecompressedBytes(fileName);
        ValidateHeaderFields(originalRaw, "ORIGINAL");

        var (world, error) = World.LoadWorld(fileName);
        Assert.Null(error);

        var saveTest = fileName + ".hdr-validate.test";
        try
        {
            World.Save(world, saveTest, incrementRevision: false);
            byte[] savedRaw = GetDecompressedBytes(saveTest);
            ValidateHeaderFields(savedRaw, "ROUND-TRIPPED");
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    private byte[] GetDecompressedBytes(string path)
    {
        using var fs = File.OpenRead(path);
        if (ConsoleCompressor.IsCompressed(fs))
        {
            using var ms = ConsoleCompressor.DecompressStream(fs);
            return ms.ToArray();
        }
        fs.Position = 0;
        using var ms2 = new MemoryStream();
        fs.CopyTo(ms2);
        return ms2.ToArray();
    }

    private void ValidateHeaderFields(byte[] rawData, string label)
    {
        using var ms = new MemoryStream(rawData);
        using var r = new BinaryReader(ms);

        int version = r.ReadInt32();
        _output.WriteLine($"\n=== {label} v{version} ({rawData.Length:N0} bytes) ===");

        // Read FileMetadata (v >= 135)
        if (version >= 135)
        {
            r.ReadBytes(7); // "relogic"
            r.ReadByte();   // fileType
            r.ReadUInt32(); // revision
            r.ReadUInt64(); // flags
        }

        // Section pointers
        short sectionCount = r.ReadInt16();
        int[] positions = new int[sectionCount];
        for (int i = 0; i < sectionCount; i++)
            positions[i] = r.ReadInt32();

        // Tile importance flags
        ushort tileTypeCount = r.ReadUInt16();
        byte bits = 0, mask = 128;
        for (int i = 0; i < tileTypeCount; i++)
        {
            if (mask == 128) { bits = r.ReadByte(); mask = 1; }
            else mask <<= 1;
        }

        long headerStart = ms.Position;
        _output.WriteLine($"Header starts at: {headerStart}, expected end: {positions[1]}");

        // === BEGIN: Terraria's exact LoadHeader field order ===
        string worldName = r.ReadString(); Log(r, "worldName");

        if (version >= 179)
        {
            if (version == 179) r.ReadInt32(); else r.ReadString(); Log(r, "seed");
            r.ReadUInt64(); Log(r, "worldGenVersion");
        }
        if (version >= 181) { r.ReadBytes(16); Log(r, "uniqueId"); }

        r.ReadInt32(); Log(r, "worldId");
        r.ReadInt32(); Log(r, "leftWorld");
        r.ReadInt32(); Log(r, "rightWorld");
        r.ReadInt32(); Log(r, "topWorld");
        r.ReadInt32(); Log(r, "bottomWorld");
        int maxY = r.ReadInt32(); Log(r, $"maxTilesY={maxY}");
        int maxX = r.ReadInt32(); Log(r, $"maxTilesX={maxX}");

        if (version >= 209)
        {
            r.ReadInt32(); Log(r, "gameMode");
            if (version >= 222) { r.ReadBoolean(); Log(r, "drunkWorld"); }
            if (version >= 227) { r.ReadBoolean(); Log(r, "getGoodWorld"); }
            if (version >= 238) { r.ReadBoolean(); Log(r, "tenthAnniversary"); }
            if (version >= 239) { r.ReadBoolean(); Log(r, "dontStarveWorld"); }
            if (version >= 241) { r.ReadBoolean(); Log(r, "notTheBees"); }
            if (version >= 249) { r.ReadBoolean(); Log(r, "remixWorld"); }
            if (version >= 266) { r.ReadBoolean(); Log(r, "noTrapsWorld"); }
            if (version >= 267) { r.ReadBoolean(); Log(r, "zenithWorld"); }
            if (version >= 302) { r.ReadBoolean(); Log(r, "skyblockWorld"); }
        }
        else
        {
            if (version >= 112) { r.ReadBoolean(); Log(r, "expertMode"); }
            if (version == 208) { r.ReadBoolean(); Log(r, "masterMode"); }
        }

        if (version >= 141) { r.ReadInt64(); Log(r, "creationTime"); }
        if (version >= 284) { r.ReadInt64(); Log(r, "lastPlayed"); }

        r.ReadByte(); Log(r, "moonType");
        for (int i = 0; i < 3; i++) r.ReadInt32(); Log(r, "treeX[3]");
        for (int i = 0; i < 4; i++) r.ReadInt32(); Log(r, "treeStyle[4]");
        for (int i = 0; i < 3; i++) r.ReadInt32(); Log(r, "caveBackX[3]");
        for (int i = 0; i < 4; i++) r.ReadInt32(); Log(r, "caveBackStyle[4]");
        r.ReadInt32(); Log(r, "iceBackStyle");
        r.ReadInt32(); Log(r, "jungleBackStyle");
        r.ReadInt32(); Log(r, "hellBackStyle");
        r.ReadInt32(); Log(r, "spawnX");
        r.ReadInt32(); Log(r, "spawnY");
        r.ReadDouble(); Log(r, "worldSurface");
        r.ReadDouble(); Log(r, "rockLayer");
        r.ReadDouble(); Log(r, "time");
        r.ReadBoolean(); Log(r, "dayTime");
        r.ReadInt32(); Log(r, "moonPhase");
        r.ReadBoolean(); Log(r, "bloodMoon");
        r.ReadBoolean(); Log(r, "eclipse");
        r.ReadInt32(); Log(r, "dungeonX");
        r.ReadInt32(); Log(r, "dungeonY");
        r.ReadBoolean(); Log(r, "crimson");

        // Boss flags
        for (int i = 0; i < 10; i++) r.ReadBoolean();
        Log(r, "bosses[10]");
        if (version >= 118) { r.ReadBoolean(); Log(r, "downedSlimeKing"); }

        // Saved NPCs
        r.ReadBoolean(); r.ReadBoolean(); r.ReadBoolean(); Log(r, "savedGoblin/Wizard/Mech");

        // More boss/event flags
        for (int i = 0; i < 4; i++) r.ReadBoolean();
        Log(r, "downedGoblins/Clown/Frost/Pirates");

        r.ReadBoolean(); r.ReadBoolean(); Log(r, "shadowOrb/spawnMeteor");
        r.ReadByte(); Log(r, "shadowOrbCount");
        r.ReadInt32(); Log(r, "altarCount");
        r.ReadBoolean(); Log(r, "hardMode");

        if (version >= 257) { r.ReadBoolean(); Log(r, "afterPartyOfDoom"); }

        r.ReadInt32(); r.ReadInt32(); r.ReadInt32(); Log(r, "invasionDelay/Size/Type");
        r.ReadDouble(); Log(r, "invasionX");

        if (version >= 118) { r.ReadDouble(); Log(r, "slimeRainTime"); }
        if (version >= 113) { r.ReadByte(); Log(r, "sundialCooldown"); }

        r.ReadBoolean(); Log(r, "isRaining");
        r.ReadInt32(); Log(r, "tempRainTime");
        r.ReadSingle(); Log(r, "tempMaxRain");
        r.ReadInt32(); r.ReadInt32(); r.ReadInt32(); Log(r, "oreTiers[3]");

        // BG styles (8 bytes)
        for (int i = 0; i < 8; i++) r.ReadByte();
        Log(r, "bgStyles[8]");

        r.ReadInt32(); Log(r, "cloudBgActive");
        r.ReadInt16(); Log(r, "numClouds");
        r.ReadSingle(); Log(r, "windSpeedTarget");

        // Anglers
        if (version >= 95)
        {
            int anglerCount = r.ReadInt32();
            for (int i = 0; i < anglerCount; i++) r.ReadString();
            Log(r, $"anglers[{anglerCount}]");
        }

        if (version >= 99) { r.ReadBoolean(); Log(r, "savedAngler"); }
        if (version >= 101) { r.ReadInt32(); Log(r, "anglerQuest"); }
        if (version >= 104) { r.ReadBoolean(); Log(r, "savedStylist"); }
        if (version >= 129) { r.ReadBoolean(); Log(r, "savedTaxCollector"); }
        if (version >= 201) { r.ReadBoolean(); Log(r, "savedGolfer"); }
        if (version >= 107) { r.ReadInt32(); Log(r, "invasionSizeStart"); }
        if (version >= 108) { r.ReadInt32(); Log(r, "cultistDelay"); }

        // BannerSystem.Load
        if (version >= 109)
        {
            int killCount = r.ReadInt16();
            for (int i = 0; i < killCount; i++) r.ReadInt32();
            Log(r, $"killedMobs[{killCount}]");

            if (version >= 289)
            {
                int bannerCount = r.ReadInt16();
                for (int i = 0; i < bannerCount; i++) r.ReadUInt16();
                Log(r, $"claimableBanners[{bannerCount}]");
            }
        }

        if (version >= 128) { r.ReadBoolean(); Log(r, "fastForwardTime"); }

        if (version >= 131)
        {
            r.ReadBoolean(); Log(r, "downedFishron");
            r.ReadBoolean(); Log(r, "downedMartians");
            r.ReadBoolean(); Log(r, "downedCultist");
            r.ReadBoolean(); Log(r, "downedMoonlord");
            r.ReadBoolean(); Log(r, "downedHalloweenKing");
            r.ReadBoolean(); Log(r, "downedHalloweenTree");
            r.ReadBoolean(); Log(r, "downedChristmasQueen");
            r.ReadBoolean(); Log(r, "downedSanta");
            r.ReadBoolean(); Log(r, "downedChristmasTree");
        }

        if (version >= 140)
        {
            for (int i = 0; i < 4; i++) r.ReadBoolean(); Log(r, "celestialDowned[4]");
            for (int i = 0; i < 4; i++) r.ReadBoolean(); Log(r, "celestialActive[4]");
            r.ReadBoolean(); Log(r, "apocalypse");
        }

        if (version >= 170)
        {
            r.ReadBoolean(); r.ReadBoolean(); Log(r, "partyManual/Genuine");
            r.ReadInt32(); Log(r, "partyCooldown");
            int partyCount = r.ReadInt32();
            for (int i = 0; i < partyCount; i++) r.ReadInt32();
            Log(r, $"partyNPCs[{partyCount}]");
        }

        if (version >= 174)
        {
            r.ReadBoolean(); Log(r, "sandstormHappening");
            r.ReadInt32(); Log(r, "sandstormTimeLeft");
            r.ReadSingle(); Log(r, "sandstormSeverity");
            r.ReadSingle(); Log(r, "sandstormIntended");
        }

        // DD2Event
        if (version >= 178)
        {
            r.ReadBoolean(); r.ReadBoolean(); r.ReadBoolean(); r.ReadBoolean();
            Log(r, "DD2Event[4]");
        }

        if (version > 194) { r.ReadByte(); Log(r, "mushroomBg"); }
        if (version >= 215) { r.ReadByte(); Log(r, "underworldBg"); }
        if (version > 195) { r.ReadByte(); r.ReadByte(); r.ReadByte(); Log(r, "bgTree2/3/4"); }

        if (version >= 204) { r.ReadBoolean(); Log(r, "combatBookUsed"); }

        if (version >= 207)
        {
            r.ReadInt32(); Log(r, "lanternCooldown");
            r.ReadBoolean(); r.ReadBoolean(); r.ReadBoolean();
            Log(r, "lanternGenuine/Manual/NextNight");
        }

        // TreeTops
        if (version >= 211)
        {
            int ttCount = r.ReadInt32();
            for (int i = 0; i < ttCount; i++) r.ReadInt32();
            Log(r, $"treeTopVariations[{ttCount}]");
        }

        if (version >= 212) { r.ReadBoolean(); r.ReadBoolean(); Log(r, "forceHalloween/Xmas"); }

        if (version >= 216)
        {
            r.ReadInt32(); r.ReadInt32(); r.ReadInt32(); r.ReadInt32();
            Log(r, "oreTiersCopper/Iron/Silver/Gold");
        }

        if (version >= 217)
        {
            r.ReadBoolean(); r.ReadBoolean(); r.ReadBoolean();
            Log(r, "boughtCat/Dog/Bunny");
        }

        if (version >= 223) { r.ReadBoolean(); r.ReadBoolean(); Log(r, "downedEmpress/QueenSlime"); }
        if (version >= 240) { r.ReadBoolean(); Log(r, "downedDeerclops"); }
        if (version >= 250) { r.ReadBoolean(); Log(r, "unlockedSlimeBlue"); }

        if (version >= 251)
        {
            for (int i = 0; i < 8; i++) r.ReadBoolean();
            Log(r, "unlockedNPCSpawns[8]");
        }

        if (version >= 259) { r.ReadBoolean(); Log(r, "combatBookV2"); }
        if (version >= 260) { r.ReadBoolean(); Log(r, "peddlersSatchel"); }

        if (version >= 261)
        {
            for (int i = 0; i < 7; i++) r.ReadBoolean();
            Log(r, "unlockedSlimeSpawns[7]");
        }

        if (version >= 264)
        {
            r.ReadBoolean(); Log(r, "fastForwardDusk");
            r.ReadByte(); Log(r, "moondialCooldown");
        }

        if (version >= 287) { r.ReadBoolean(); r.ReadBoolean(); Log(r, "forceHalloween/XmasForever"); }
        if (version >= 288) { r.ReadBoolean(); Log(r, "vampireSeed"); }
        if (version >= 296) { r.ReadBoolean(); Log(r, "infectedSeed"); }

        if (version >= 291)
        {
            r.ReadInt32(); r.ReadInt32(); Log(r, "meteorShowerCount/coinRain");
        }

        if (version >= 297)
        {
            r.ReadBoolean(); Log(r, "teamBasedSpawns");
            // ExtraSpawnPointManager.Read
            byte spawnCount = r.ReadByte();
            for (int i = 0; i < spawnCount; i++) { r.ReadInt16(); r.ReadInt16(); }
            Log(r, $"teamSpawns[{spawnCount}]");
        }

        if (version >= 304) { r.ReadBoolean(); Log(r, "dualDungeons"); }

        if (version >= 299 && version < 313)
        {
            r.ReadUInt32(); Log(r, "deprecated_uint");
        }

        if (version >= 299)
        {
            r.ReadString(); Log(r, "worldManifest");
        }

        // === END of Terraria's LoadHeader ===

        long actualEnd = ms.Position;
        long expectedEnd = positions[1];
        long diff = actualEnd - expectedEnd;

        _output.WriteLine($"\n*** Header end: actual={actualEnd}, expected={expectedEnd}, diff={diff:+#;-#;0} ***");

        if (diff != 0)
        {
            _output.WriteLine($"!!! HEADER SIZE MISMATCH: {diff} bytes {(diff > 0 ? "OVER" : "UNDER")} !!!");
            Assert.Fail($"{label} header ends at {actualEnd}, expected {expectedEnd} (diff={diff})");
        }
    }

    private void Log(BinaryReader r, string field)
    {
        _output.WriteLine($"  @{r.BaseStream.Position,8:N0} after {field}");
    }

    // ─── Test: validate original world files ────────────────────────

    [Theory]
    [InlineData(".\\WorldFiles\\ConsoleCookie.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void Validate_OriginalWorldFile(string fileName)
    {
        if (!File.Exists(fileName) || new FileInfo(fileName).Length < 500) return;

        byte[] raw;
        using (var fs = File.OpenRead(fileName))
        {
            if (ConsoleCompressor.IsCompressed(fs))
            {
                using var ms = ConsoleCompressor.DecompressStream(fs);
                raw = ms.ToArray();
            }
            else
            {
                fs.Position = 0;
                using var ms = new MemoryStream();
                fs.CopyTo(ms);
                raw = ms.ToArray();
            }
        }

        _output.WriteLine($"=== ORIGINAL: {Path.GetFileName(fileName)} ({raw.Length:N0} bytes) ===");
        var (valid, reason) = TerrariaValidateWorld(raw);
        _output.WriteLine($"\nRESULT: {reason}");
        Assert.True(valid, $"Original world failed validation: {reason}");
    }

    // ─── Test: validate TEdit round-tripped files ───────────────────

    [Theory]
    [InlineData(".\\WorldFiles\\ConsoleCookie.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void Validate_RoundTrippedWorldFile(string fileName)
    {
        if (!File.Exists(fileName) || new FileInfo(fileName).Length < 500) return;

        var (world, error) = World.LoadWorld(fileName);
        Assert.Null(error);

        var saveTest = fileName + ".validate.test";
        try
        {
            World.Save(world, saveTest, incrementRevision: false);

            byte[] raw;
            using (var fs = File.OpenRead(saveTest))
            {
                if (ConsoleCompressor.IsCompressed(fs))
                {
                    using var ms = ConsoleCompressor.DecompressStream(fs);
                    raw = ms.ToArray();
                }
                else
                {
                    fs.Position = 0;
                    using var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    raw = ms.ToArray();
                }
            }

            _output.WriteLine($"=== ROUND-TRIPPED: {Path.GetFileName(fileName)} ({raw.Length:N0} bytes) ===");
            var (valid, reason) = TerrariaValidateWorld(raw);
            _output.WriteLine($"\nRESULT: {reason}");
            Assert.True(valid, $"Round-tripped world failed validation: {reason}");
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    // ─── Test: validate EDITED world files ──────────────────────────

    [Theory]
    [InlineData(".\\WorldFiles\\ConsoleCookie.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void Validate_EditedWorldFile(string fileName)
    {
        if (!File.Exists(fileName) || new FileInfo(fileName).Length < 500) return;

        var (world, error) = World.LoadWorld(fileName);
        Assert.Null(error);

        // Make edits
        int editX = world.TilesWide / 2;
        int editY = world.TilesHigh / 2;
        world.Tiles[editX, editY].IsActive = true;
        world.Tiles[editX, editY].Type = 0; // dirt

        var saveTest = fileName + ".edited-validate.test";
        try
        {
            World.Save(world, saveTest, incrementRevision: false);

            byte[] raw;
            using (var fs = File.OpenRead(saveTest))
            {
                if (ConsoleCompressor.IsCompressed(fs))
                {
                    using var ms = ConsoleCompressor.DecompressStream(fs);
                    raw = ms.ToArray();
                }
                else
                {
                    fs.Position = 0;
                    using var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    raw = ms.ToArray();
                }
            }

            _output.WriteLine($"=== EDITED: {Path.GetFileName(fileName)} ({raw.Length:N0} bytes) ===");
            var (valid, reason) = TerrariaValidateWorld(raw);
            _output.WriteLine($"\nRESULT: {reason}");
            Assert.True(valid, $"Edited world failed validation: {reason}");
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }
}
