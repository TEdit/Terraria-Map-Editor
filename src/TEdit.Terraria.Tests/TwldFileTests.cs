using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEdit.Common;
using TEdit.Common.IO;
using TEdit.Terraria.TModLoader;
using Xunit;
using Shouldly;

namespace TEdit.Terraria.Tests;

public class TwldFileTests
{
    [Fact]
    public void ParseTileWallBinary_BasicTileOverlay()
    {
        // Create a simple binary stream with one mod tile at position (0,0)
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "TestTile",
            FrameImportant = false,
        });

        // tModLoader format: [skip] [flags] [data] ...
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        w.Write((byte)0); // skip=0 (tile at position 0)
        byte flags = 0x01; // hasTile only
        w.Write(flags);
        w.Write((ushort)0); // tile type index into tileMap

        byte[] binary = ms.ToArray();
        TwldFile.ParseTileWallBinary(binary, data, 10, 10);

        data.ModTileGrid.ShouldContainKey((0, 0));
        data.ModTileGrid[(0, 0)].TileMapIndex.ShouldBe((ushort)0);
    }

    [Fact]
    public void ParseTileWallBinary_TileWithColorAndFrame()
    {
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "FramedTile",
            FrameImportant = true,
        });

        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        w.Write((byte)0); // skip=0
        // flags: hasTile(0x01) + hasColor(0x08) = 0x09
        byte flags = 0x09;
        w.Write(flags);
        w.Write((ushort)0); // tile type
        w.Write((byte)10); // frameX (small, < 256)
        w.Write((byte)20); // frameY (small, < 256)
        w.Write((byte)5); // color

        byte[] binary = ms.ToArray();
        TwldFile.ParseTileWallBinary(binary, data, 10, 10);

        data.ModTileGrid.ShouldContainKey((0, 0));
        var tile = data.ModTileGrid[(0, 0)];
        tile.TileMapIndex.ShouldBe((ushort)0);
        tile.FrameX.ShouldBe((short)10);
        tile.FrameY.ShouldBe((short)20);
        tile.Color.ShouldBe((byte)5);
    }

    [Fact]
    public void ParseTileWallBinary_WallOnly()
    {
        var data = new TwldData();
        data.WallMap.Add(new ModWallEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "TestWall",
        });

        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        w.Write((byte)0); // skip=0
        // flags: hasWall(0x10) + hasWallColor(0x20) = 0x30
        byte flags = 0x30;
        w.Write(flags);
        w.Write((ushort)0); // wall type
        w.Write((byte)3); // wall color

        byte[] binary = ms.ToArray();
        TwldFile.ParseTileWallBinary(binary, data, 10, 10);

        data.ModWallGrid.ShouldContainKey((0, 0));
        var wall = data.ModWallGrid[(0, 0)];
        wall.WallMapIndex.ShouldBe((ushort)0);
        wall.WallColor.ShouldBe((byte)3);
    }

    [Fact]
    public void ParseTileWallBinary_SameCount()
    {
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "RepeatedTile",
            FrameImportant = false,
        });

        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        w.Write((byte)0); // skip=0
        // flags: hasTile(0x01) + hasSameCount(0x40) = 0x41
        byte flags = 0x41;
        w.Write(flags);
        w.Write((ushort)0); // tile type
        w.Write((byte)4); // sameCount = 4 (means 5 total cells: 1 original + 4 same)

        byte[] binary = ms.ToArray();
        TwldFile.ParseTileWallBinary(binary, data, 10, 10);

        // Should have 5 consecutive cells: (0,0), (0,1), (0,2), (0,3), (0,4)
        data.ModTileGrid.Count.ShouldBe(5);
        for (int y = 0; y < 5; y++)
        {
            data.ModTileGrid.ShouldContainKey((0, y));
            data.ModTileGrid[(0, y)].TileMapIndex.ShouldBe((ushort)0);
        }
    }

    [Fact]
    public void ParseTileWallBinary_SkipBytes()
    {
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "SkippedTile",
            FrameImportant = false,
        });

        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        // First entry at position 0: skip=0, then record
        w.Write((byte)0); // skip=0
        byte flags = 0x01; // hasTile
        w.Write(flags);
        w.Write((ushort)0);

        // Skip 5 positions, then second entry at position 6 (0 + 1 + skip 5)
        w.Write((byte)5);
        flags = 0x01;
        w.Write(flags);
        w.Write((ushort)0);

        byte[] binary = ms.ToArray();
        TwldFile.ParseTileWallBinary(binary, data, 10, 10);

        data.ModTileGrid.ShouldContainKey((0, 0));
        data.ModTileGrid.ShouldContainKey((0, 6)); // position 6 = (0,6) in a 10-high world
        data.ModTileGrid.Count.ShouldBe(2);
    }

    [Fact]
    public void ParseTileWallBinary_NextIsModFlag()
    {
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "Adjacent",
            FrameImportant = false,
        });

        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        // skip=0, first entry at position 0, nextIsMod=true
        w.Write((byte)0); // skip
        byte flags = 0x81; // hasTile(0x01) + nextIsMod(0x80)
        w.Write(flags);
        w.Write((ushort)0);

        // Second entry at position 1, no nextIsMod (no skip bytes between them)
        flags = 0x01;
        w.Write(flags);
        w.Write((ushort)0);

        byte[] binary = ms.ToArray();
        TwldFile.ParseTileWallBinary(binary, data, 10, 10);

        data.ModTileGrid.ShouldContainKey((0, 0));
        data.ModTileGrid.ShouldContainKey((0, 1));
        data.ModTileGrid.Count.ShouldBe(2);
    }

    [Fact]
    public void ParseTileWallBinary_LargeInitialSkip()
    {
        // Test that tiles far from position 0 are correctly placed (large initial skip)
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "FarTile",
            FrameImportant = false,
        });

        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        // Skip 500 positions: 255 + 245
        w.Write((byte)255);
        w.Write((byte)245);
        byte flags = 0x01; // hasTile
        w.Write(flags);
        w.Write((ushort)0);

        byte[] binary = ms.ToArray();
        // World: 100 wide x 100 high
        TwldFile.ParseTileWallBinary(binary, data, 100, 100);

        // linearPos 500 = x=5, y=0 in a 100-high world
        data.ModTileGrid.ShouldContainKey((5, 0));
        data.ModTileGrid.Count.ShouldBe(1);
    }

    [Fact]
    public void ModTileEntry_FromTag_ParsesCorrectly()
    {
        var tag = new TagCompound();
        tag.Set("mod", "CalamityMod");
        tag.Set("name", "AstralMonolith");
        tag.Set("framed", (byte)1);

        var entry = ModTileEntry.FromTag(tag, 5);

        entry.SaveType.ShouldBe((ushort)5);
        entry.ModName.ShouldBe("CalamityMod");
        entry.Name.ShouldBe("AstralMonolith");
        entry.FrameImportant.ShouldBeTrue();
        entry.FullName.ShouldBe("CalamityMod:AstralMonolith");
    }

    [Fact]
    public void ModTileEntry_RoundTrip()
    {
        var original = new ModTileEntry
        {
            SaveType = 3,
            ModName = "TestMod",
            Name = "TestTile",
            FrameImportant = true,
        };

        var tag = original.ToTag();
        var loaded = ModTileEntry.FromTag(tag, 3);

        loaded.ModName.ShouldBe(original.ModName);
        loaded.Name.ShouldBe(original.Name);
        loaded.FrameImportant.ShouldBe(original.FrameImportant);
    }

    [Fact]
    public void ModWallEntry_RoundTrip()
    {
        var original = new ModWallEntry
        {
            SaveType = 2,
            ModName = "TestMod",
            Name = "TestWall",
        };

        var tag = original.ToTag();
        var loaded = ModWallEntry.FromTag(tag, 2);

        loaded.ModName.ShouldBe(original.ModName);
        loaded.Name.ShouldBe(original.Name);
        loaded.FullName.ShouldBe("TestMod:TestWall");
    }

    [Fact]
    public void GenerateModColor_IsDeterministic()
    {
        var color1 = TwldFile.GenerateModColor("CalamityMod:AstralMonolith");
        var color2 = TwldFile.GenerateModColor("CalamityMod:AstralMonolith");
        var color3 = TwldFile.GenerateModColor("CalamityMod:AerialiteBrick");

        color1.ShouldBe(color2);
        color1.ShouldNotBe(color3);
    }

    [Fact]
    public void GetTwldPath_DerivesSidecarPath()
    {
        string wldPath = @"C:\Worlds\MyWorld.wld";
        string twldPath = TwldFile.GetTwldPath(wldPath);

        twldPath.ShouldEndWith("MyWorld.twld");
        twldPath.ShouldContain(@"Worlds");
    }

    [Fact]
    public void TagIO_RoundTrip_TwldStructure()
    {
        // Simulate a minimal .twld-like TagCompound structure
        var root = new TagCompound();

        // Header
        var header = new TagCompound();
        header.Set("usedMods", new List<string> { "CalamityMod", "ThoriumMod" });
        root.Set("0header", header);

        // Tiles section
        var tiles = new TagCompound();
        var tileMap = new List<TagCompound>();
        var tileEntry = new TagCompound();
        tileEntry.Set("mod", "CalamityMod");
        tileEntry.Set("name", "AstralMonolith");
        tileEntry.Set("framed", (byte)0);
        tileMap.Add(tileEntry);
        tiles.Set("tileMap", tileMap);
        tiles.Set("wallMap", new List<TagCompound>());
        tiles.Set("data", new byte[] { 0x01, 0x00, 0x00, 0x00 }); // minimal binary
        root.Set("tiles", tiles);

        // Round-trip through GZip
        using var ms = new MemoryStream();
        TagIO.ToStream(root, ms, compressed: true);

        ms.Position = 0;
        var loaded = TagIO.FromStream(ms, compressed: true);

        loaded.ContainsKey("0header").ShouldBeTrue();
        loaded.ContainsKey("tiles").ShouldBeTrue();

        var loadedHeader = loaded.GetCompound("0header");
        loadedHeader.GetList<string>("usedMods").Count.ShouldBe(2);
        loadedHeader.GetList<string>("usedMods")[0].ShouldBe("CalamityMod");

        var loadedTiles = loaded.GetCompound("tiles");
        loadedTiles.GetList<TagCompound>("tileMap").Count.ShouldBe(1);
        loadedTiles.GetList<TagCompound>("tileMap")[0].GetString("mod").ShouldBe("CalamityMod");
    }

    #region Vanilla World Regression Tests

    [Fact]
    public void VanillaWorld_TwldLoad_ReturnsNull_WhenNoSidecar()
    {
        // A vanilla .wld with no .twld sidecar should return null
        string fakePath = Path.Combine(Path.GetTempPath(), $"VanillaTest_{Guid.NewGuid()}.wld");
        try
        {
            File.WriteAllBytes(fakePath, new byte[] { 0 }); // dummy .wld
            var result = TwldFile.Load(fakePath);
            result.ShouldBeNull();
        }
        finally
        {
            if (File.Exists(fakePath)) File.Delete(fakePath);
        }
    }

    [Fact]
    public void VanillaWorld_TilePropertiesUnchanged()
    {
        // WorldConfiguration.TileCount and WallCount should stay at vanilla values
        // when no mod data is loaded
        WorldConfiguration.TileCount.ShouldBe((short)752);
        WorldConfiguration.WallCount.ShouldBe((short)366);
    }

    [Fact]
    public void VanillaWorld_TwldSave_DoesNothingForNull()
    {
        // TwldFile.Save with null data should not create any file
        string fakePath = Path.Combine(Path.GetTempPath(), $"VanillaSaveTest_{Guid.NewGuid()}.wld");
        string twldPath = TwldFile.GetTwldPath(fakePath);
        try
        {
            TwldFile.Save(fakePath, null);
            File.Exists(twldPath).ShouldBeFalse();
        }
        finally
        {
            if (File.Exists(twldPath)) File.Delete(twldPath);
        }
    }

    [Fact]
    public void ApplyToWorld_DoesNothingForNull()
    {
        // Passing null TwldData should be a safe no-op (no exception)
        TwldFile.ApplyToWorld(null, null);
    }

    [Fact]
    public void StripFromWorld_DoesNothingForNull()
    {
        // Passing null TwldData should be a safe no-op (no exception)
        TwldFile.StripFromWorld(null, null);
    }

    #endregion

    #region ModColors.json Integration Tests

    [Fact]
    public void LoadModColors_ReturnsEmpty_WhenFileDoesNotExist()
    {
        var colors = TwldFile.LoadModColors(Path.Combine(Path.GetTempPath(), "nonexistent_modColors.json"));
        colors.ShouldNotBeNull();
        colors.Count.ShouldBe(0);
    }

    [Fact]
    public void LoadModColors_ParsesValidJson()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"modColors_test_{Guid.NewGuid()}.json");
        try
        {
            string json = @"{
                ""TestMod"": {
                    ""tiles"": {
                        ""TestTile"": { ""color"": ""#FF8040FF"", ""name"": ""Test Tile"" }
                    },
                    ""walls"": {
                        ""TestWall"": { ""color"": ""#4080FFFF"", ""name"": ""Test Wall"" }
                    }
                }
            }";
            File.WriteAllText(tempFile, json);

            var colors = TwldFile.LoadModColors(tempFile);
            colors.ShouldContainKey("TestMod:TestTile");
            colors["TestMod:TestTile"].R.ShouldBe((byte)0xFF);
            colors["TestMod:TestTile"].G.ShouldBe((byte)0x80);
            colors["TestMod:TestTile"].B.ShouldBe((byte)0x40);

            colors.ShouldContainKey("TestMod:TestWall");
            colors["TestMod:TestWall"].R.ShouldBe((byte)0x40);
            colors["TestMod:TestWall"].G.ShouldBe((byte)0x80);
            colors["TestMod:TestWall"].B.ShouldBe((byte)0xFF);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ApplyToWorld_UsesModColorsOverride()
    {
        // Verify that when mod colors are provided, they override GenerateModColor
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "TestTile",
            FrameImportant = false,
        });

        var overrideColor = new TEditColor(255, 0, 0, 255);
        var hashColor = TwldFile.GenerateModColor("TestMod:TestTile");

        // The hash color should NOT be pure red
        hashColor.ShouldNotBe(overrideColor);

        // Verify override colors can be applied
        var modColors = new Dictionary<string, TEditColor>
        {
            ["TestMod:TestTile"] = overrideColor,
        };

        // Test that LookupModColor returns the override
        TEditColor result;
        modColors.TryGetValue("TestMod:TestTile", out result).ShouldBeTrue();
        result.ShouldBe(overrideColor);
    }

    #endregion

    [Fact]
    public void BuildTileWallBinary_RoundTrip()
    {
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "TestTile",
            FrameImportant = false,
        });
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 1,
            ModName = "TestMod",
            Name = "FramedTile",
            FrameImportant = true,
        });
        data.WallMap.Add(new ModWallEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "TestWall",
        });

        // Add some mod tiles at various positions
        data.ModTileGrid[(0, 0)] = new ModTileData { TileMapIndex = 0, Color = 0 };
        data.ModTileGrid[(0, 5)] = new ModTileData { TileMapIndex = 1, Color = 2, FrameX = 18, FrameY = 36 };
        data.ModTileGrid[(3, 7)] = new ModTileData { TileMapIndex = 0, Color = 0 };
        data.ModWallGrid[(1, 2)] = new ModWallData { WallMapIndex = 0, WallColor = 4 };

        int tilesWide = 10;
        int tilesHigh = 10;

        // Build binary
        byte[] binary = TwldFile.BuildTileWallBinary(data, tilesWide, tilesHigh);
        binary.Length.ShouldBeGreaterThan(0);

        // Parse it back
        var loaded = new TwldData();
        loaded.TileMap.AddRange(data.TileMap);
        loaded.WallMap.AddRange(data.WallMap);

        TwldFile.ParseTileWallBinary(binary, loaded, tilesWide, tilesHigh);

        // Verify the parsed data matches
        loaded.ModTileGrid.ShouldContainKey((0, 0));
        loaded.ModTileGrid[(0, 0)].TileMapIndex.ShouldBe((ushort)0);

        loaded.ModTileGrid.ShouldContainKey((0, 5));
        loaded.ModTileGrid[(0, 5)].TileMapIndex.ShouldBe((ushort)1);
        loaded.ModTileGrid[(0, 5)].FrameX.ShouldBe((short)18);
        loaded.ModTileGrid[(0, 5)].FrameY.ShouldBe((short)36);
        loaded.ModTileGrid[(0, 5)].Color.ShouldBe((byte)2);

        loaded.ModTileGrid.ShouldContainKey((3, 7));

        loaded.ModWallGrid.ShouldContainKey((1, 2));
        loaded.ModWallGrid[(1, 2)].WallColor.ShouldBe((byte)4);
    }

    #region Calamity2 Real World Tests

    private const string Calamity2WldPath = @"D:\dev\ai\tedit\tModLoaderData\Worlds\Calamity2.wld";
    private const string Calamity2TwldPath = @"D:\dev\ai\tedit\tModLoaderData\Worlds\Calamity2.twld";

    [SkippableFact]
    public void Calamity2_ParseTileWallBinary_ProducesTilesInAllQuadrants()
    {
        Skip.IfNot(File.Exists(Calamity2TwldPath), "Calamity2.twld test fixture not found");
        Skip.IfNot(File.Exists(Calamity2WldPath), "Calamity2.wld test fixture not found");

        WorldConfiguration.Initialize();

        // Read world dimensions from header
        var header = World.ReadWorldHeader(Calamity2WldPath);
        header.ShouldNotBeNull();
        int tilesWide = header.TilesWide;
        int tilesHigh = header.TilesHigh;

        tilesWide.ShouldBeGreaterThan(0);
        tilesHigh.ShouldBeGreaterThan(0);

        // Load .twld raw tag to inspect structure
        var rootTag = TagIO.FromFile(Calamity2TwldPath);
        rootTag.ShouldNotBeNull();

        // Diagnostic: check root keys
        var rootKeys = string.Join(", ", rootTag.Select(kv => kv.Key));

        // Check tiles section exists
        rootTag.ContainsKey("tiles").ShouldBeTrue($"Root keys: {rootKeys}");
        var tilesTag = rootTag.GetCompound("tiles");
        var tilesKeys = string.Join(", ", tilesTag.Select(kv => $"{kv.Key}({kv.Value?.GetType().Name})"));
        // New format uses "tileData" and "wallData" keys
        bool isNewFormat = tilesTag.ContainsKey("tileData");
        (isNewFormat || tilesTag.ContainsKey("data")).ShouldBeTrue($"Tiles keys: {tilesKeys}");

        // Load .twld data
        var twldData = TwldFile.Load(Calamity2WldPath);
        twldData.ShouldNotBeNull();

        bool hasRawData = twldData.RawTileData.Length > 0 || twldData.RawWallData.Length > 0
                       || twldData.RawTileWallData.Length > 0;
        hasRawData.ShouldBeTrue(
            $"No raw data. TileMap={twldData.TileMap.Count}, WallMap={twldData.WallMap.Count}. " +
            $"Tiles keys: {tilesKeys}. RawTileData={twldData.RawTileData.Length}, " +
            $"RawWallData={twldData.RawWallData.Length}, RawTileWallData={twldData.RawTileWallData.Length}");

        // Parse binary tile/wall data using the appropriate format
        if (twldData.RawTileData.Length > 0)
            TwldFile.ParseTileDataDense(twldData.RawTileData, twldData, tilesWide, tilesHigh);
        if (twldData.RawWallData.Length > 0)
            TwldFile.ParseWallDataDense(twldData.RawWallData, twldData, tilesWide, tilesHigh);
        if (twldData.RawTileWallData.Length > 0)
            TwldFile.ParseTileWallBinary(twldData.RawTileWallData, twldData, tilesWide, tilesHigh);

        int totalTiles = twldData.ModTileGrid.Count;
        int totalWalls = twldData.ModWallGrid.Count;

        totalTiles.ShouldBeGreaterThan(0, "Expected mod tiles to be parsed");

        // Check tiles exist in all four quadrants
        int halfX = tilesWide / 2;
        int halfY = tilesHigh / 2;

        int topLeft = 0, topRight = 0, bottomLeft = 0, bottomRight = 0;
        foreach (var (pos, _) in twldData.ModTileGrid)
        {
            if (pos.X < halfX && pos.Y < halfY) topLeft++;
            else if (pos.X >= halfX && pos.Y < halfY) topRight++;
            else if (pos.X < halfX && pos.Y >= halfY) bottomLeft++;
            else bottomRight++;
        }

        // The bug: bottom-right tiles don't load
        topLeft.ShouldBeGreaterThan(0, $"Expected tiles in top-left quadrant (total={totalTiles})");
        bottomRight.ShouldBeGreaterThan(0,
            $"Expected tiles in bottom-right quadrant (total={totalTiles}, " +
            $"TL={topLeft}, TR={topRight}, BL={bottomLeft}, BR={bottomRight}, " +
            $"world={tilesWide}x{tilesHigh})");

        // Also check X range coverage
        int maxX = 0;
        foreach (var (pos, _) in twldData.ModTileGrid)
            if (pos.X > maxX) maxX = pos.X;

        // Mod tiles should extend into at least 80% of the world width
        maxX.ShouldBeGreaterThan((int)(tilesWide * 0.8),
            $"Max tile X={maxX} should reach >80% of world width ({tilesWide})");
    }

    [SkippableFact]
    public void Calamity2_TileDataStreamSize()
    {
        Skip.IfNot(File.Exists(Calamity2TwldPath), "Calamity2.twld test fixture not found");
        Skip.IfNot(File.Exists(Calamity2WldPath), "Calamity2.wld test fixture not found");

        WorldConfiguration.Initialize();
        var header = World.ReadWorldHeader(Calamity2WldPath);
        var twldData = TwldFile.Load(Calamity2WldPath);

        int tilesWide = header.TilesWide;
        int tilesHigh = header.TilesHigh;
        long totalCells = (long)tilesWide * tilesHigh;
        long minStreamSize = totalCells * 2; // minimum: 2 bytes per cell (just ushort, no extras)

        // Count framed tiles in the entry map
        int framedCount = twldData.TileMap.Count(e => e.FrameImportant);

        twldData.RawTileData.Length.ShouldBeGreaterThan(0);

        // Stream should be at least totalCells * 2 bytes
        twldData.RawTileData.Length.ShouldBeGreaterThanOrEqualTo((int)minStreamSize,
            $"TileData stream too small. world={tilesWide}x{tilesHigh}, cells={totalCells}, " +
            $"minSize={minStreamSize}, actual={twldData.RawTileData.Length}, " +
            $"tileMapCount={twldData.TileMap.Count}, framedEntries={framedCount}");
    }

    [SkippableFact]
    public void Calamity2_ParseTileWallBinary_NoStreamPositionErrors()
    {
        Skip.IfNot(File.Exists(Calamity2TwldPath), "Calamity2.twld test fixture not found");
        Skip.IfNot(File.Exists(Calamity2WldPath), "Calamity2.wld test fixture not found");

        WorldConfiguration.Initialize();
        var header = World.ReadWorldHeader(Calamity2WldPath);
        var twldData = TwldFile.Load(Calamity2WldPath);

        // Parsing should not throw (no stream overrun, no invalid tile types)
        Should.NotThrow(() =>
        {
            if (twldData.RawTileData.Length > 0)
                TwldFile.ParseTileDataDense(twldData.RawTileData, twldData, header.TilesWide, header.TilesHigh);
            if (twldData.RawWallData.Length > 0)
                TwldFile.ParseWallDataDense(twldData.RawWallData, twldData, header.TilesWide, header.TilesHigh);
            if (twldData.RawTileWallData.Length > 0)
                TwldFile.ParseTileWallBinary(twldData.RawTileWallData, twldData, header.TilesWide, header.TilesHigh);
        });

        // Count tiles/walls with invalid map indices (stream desync artifacts)
        int invalidTiles = 0;
        foreach (var (pos, tile) in twldData.ModTileGrid)
        {
            if (tile.TileMapIndex >= twldData.TileMap.Count)
                invalidTiles++;
        }

        int invalidWalls = 0;
        foreach (var (pos, wall) in twldData.ModWallGrid)
        {
            if (wall.WallMapIndex >= twldData.WallMap.Count)
                invalidWalls++;
        }

        // With bounds-checked parsing, no invalid entries should be stored
        invalidTiles.ShouldBe(0,
            $"Found {invalidTiles} tiles with invalid map indices (total={twldData.ModTileGrid.Count}, " +
            $"tileMapCount={twldData.TileMap.Count})");
        invalidWalls.ShouldBe(0,
            $"Found {invalidWalls} walls with invalid map indices (total={twldData.ModWallGrid.Count}, " +
            $"wallMapCount={twldData.WallMap.Count})");
    }

    [SkippableFact]
    public void Calamity2_RoundTrip_PreservesAllTiles()
    {
        Skip.IfNot(File.Exists(Calamity2TwldPath), "Calamity2.twld test fixture not found");
        Skip.IfNot(File.Exists(Calamity2WldPath), "Calamity2.wld test fixture not found");

        WorldConfiguration.Initialize();
        var header = World.ReadWorldHeader(Calamity2WldPath);
        var twldData = TwldFile.Load(Calamity2WldPath);

        // Parse original
        bool isNewFormat = twldData.RawTileData.Length > 0;
        if (isNewFormat)
        {
            TwldFile.ParseTileDataDense(twldData.RawTileData, twldData, header.TilesWide, header.TilesHigh);
            TwldFile.ParseWallDataDense(twldData.RawWallData, twldData, header.TilesWide, header.TilesHigh);
        }
        else
        {
            TwldFile.ParseTileWallBinary(twldData.RawTileWallData, twldData, header.TilesWide, header.TilesHigh);
        }
        int originalTiles = twldData.ModTileGrid.Count;
        int originalWalls = twldData.ModWallGrid.Count;

        originalTiles.ShouldBeGreaterThan(0, "Expected parsed mod tiles");

        // Rebuild binary using the same format
        if (isNewFormat)
        {
            byte[] rebuiltTiles = TwldFile.BuildTileDataDense(twldData, header.TilesWide, header.TilesHigh);
            byte[] rebuiltWalls = TwldFile.BuildWallDataDense(twldData, header.TilesWide, header.TilesHigh);
            rebuiltTiles.Length.ShouldBeGreaterThan(0);

            var reloaded = new TwldData();
            reloaded.TileMap.AddRange(twldData.TileMap);
            reloaded.WallMap.AddRange(twldData.WallMap);
            TwldFile.ParseTileDataDense(rebuiltTiles, reloaded, header.TilesWide, header.TilesHigh);
            TwldFile.ParseWallDataDense(rebuiltWalls, reloaded, header.TilesWide, header.TilesHigh);

            reloaded.ModTileGrid.Count.ShouldBe(originalTiles,
                $"Round-trip lost tiles: {originalTiles} -> {reloaded.ModTileGrid.Count}");
            reloaded.ModWallGrid.Count.ShouldBe(originalWalls,
                $"Round-trip lost walls: {originalWalls} -> {reloaded.ModWallGrid.Count}");
        }
        else
        {
            byte[] rebuilt = TwldFile.BuildTileWallBinary(twldData, header.TilesWide, header.TilesHigh);
            rebuilt.Length.ShouldBeGreaterThan(0);

            var reloaded = new TwldData();
            reloaded.TileMap.AddRange(twldData.TileMap);
            reloaded.WallMap.AddRange(twldData.WallMap);
            TwldFile.ParseTileWallBinary(rebuilt, reloaded, header.TilesWide, header.TilesHigh);

            reloaded.ModTileGrid.Count.ShouldBe(originalTiles,
                $"Round-trip lost tiles: {originalTiles} -> {reloaded.ModTileGrid.Count}");
            reloaded.ModWallGrid.Count.ShouldBe(originalWalls,
                $"Round-trip lost walls: {originalWalls} -> {reloaded.ModWallGrid.Count}");
        }
    }

    #endregion
}
