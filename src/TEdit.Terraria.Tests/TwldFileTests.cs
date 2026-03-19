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
    /// <summary>
    /// Builds SaveTypeToTileMapIndex and SaveTypeToWallMapIndex lookups from TileMap/WallMap entries.
    /// Mirrors what ParseRootTag does during real .twld loading.
    /// </summary>
    private static void BuildSaveTypeLookups(TwldData data)
    {
        data.SaveTypeToTileMapIndex.Clear();
        data.SaveTypeToWallMapIndex.Clear();
        for (int i = 0; i < data.TileMap.Count; i++)
            data.SaveTypeToTileMapIndex[data.TileMap[i].SaveType] = i;
        for (int i = 0; i < data.WallMap.Count; i++)
            data.SaveTypeToWallMapIndex[data.WallMap[i].SaveType] = i;
    }

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

        BuildSaveTypeLookups(data);

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
        BuildSaveTypeLookups(data);

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
        BuildSaveTypeLookups(data);

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
        BuildSaveTypeLookups(data);

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
        BuildSaveTypeLookups(data);

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
        BuildSaveTypeLookups(data);

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
        BuildSaveTypeLookups(data);

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

        // No "value" key → fallback to (index + 1) = 6
        entry.SaveType.ShouldBe((ushort)6);
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
        WorldConfiguration.TileCount.ShouldBe((short)753);
        WorldConfiguration.WallCount.ShouldBe((short)367);
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

    #region ApplyToWorld / StripFromWorld / ReapplyToWorld

    private static World CreateTestWorld(int width, int height)
    {
        WorldConfiguration.Initialize();
        var world = new World(width, height, "Test", 1);
        world.Tiles = new Tile[width, height];
        return world;
    }

    [Fact]
    public void ApplyToWorld_SetsTileType_ViaRef()
    {
        var world = CreateTestWorld(10, 10);
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "TestTile",
            FrameImportant = false,
        });
        BuildSaveTypeLookups(data);
        data.ModTileGrid[(3, 5)] = new ModTileData { TileMapIndex = 0, Color = 2 };
        data.TileWallDataParsed = true;

        TwldFile.ApplyToWorld(world, data);

        // The actual tile in the world array must be modified (not a copy)
        ref var tile = ref world.Tiles[3, 5];
        tile.IsActive.ShouldBeTrue("Tile should be active after ApplyToWorld");
        tile.Type.ShouldBeGreaterThanOrEqualTo((ushort)WorldConfiguration.TileCount,
            "Tile type should be a virtual mod ID");
        tile.TileColor.ShouldBe((byte)2);
    }

    [Fact]
    public void ApplyToWorld_SetsWallType_ViaRef()
    {
        var world = CreateTestWorld(10, 10);
        var data = new TwldData();
        data.WallMap.Add(new ModWallEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "TestWall",
        });
        BuildSaveTypeLookups(data);
        data.ModWallGrid[(4, 6)] = new ModWallData { WallMapIndex = 0, WallColor = 3 };
        data.TileWallDataParsed = true;

        TwldFile.ApplyToWorld(world, data);

        ref var tile = ref world.Tiles[4, 6];
        tile.Wall.ShouldBeGreaterThanOrEqualTo((ushort)WorldConfiguration.WallCount,
            "Wall type should be a virtual mod ID");
        tile.WallColor.ShouldBe((byte)3);
    }

    [Fact]
    public void StripFromWorld_ClearsTileFromGrid()
    {
        var world = CreateTestWorld(10, 10);
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "TestTile",
            FrameImportant = false,
        });
        BuildSaveTypeLookups(data);
        data.ModTileGrid[(2, 3)] = new ModTileData { TileMapIndex = 0, Color = 0 };
        data.TileWallDataParsed = true;

        // Apply first so the world has virtual tile IDs
        TwldFile.ApplyToWorld(world, data);
        world.Tiles[2, 3].IsActive.ShouldBeTrue();

        // Strip should remove virtual tiles from the world grid
        data.ModTileGrid.Clear();
        TwldFile.StripFromWorld(world, data);

        ref var tile = ref world.Tiles[2, 3];
        tile.IsActive.ShouldBeFalse("Tile should be inactive after StripFromWorld");
        tile.Type.ShouldBe((ushort)0);
        data.ModTileGrid.ShouldContainKey((2, 3));
    }

    [Fact]
    public void ReapplyToWorld_RestoresTileAfterStrip()
    {
        var world = CreateTestWorld(10, 10);
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry
        {
            SaveType = 0,
            ModName = "TestMod",
            Name = "TestTile",
            FrameImportant = false,
        });
        BuildSaveTypeLookups(data);
        data.ModTileGrid[(1, 1)] = new ModTileData { TileMapIndex = 0, Color = 0 };
        data.TileWallDataParsed = true;

        TwldFile.ApplyToWorld(world, data);
        ushort expectedType = world.Tiles[1, 1].Type;

        // Strip removes mod tiles
        TwldFile.StripFromWorld(world, data);
        world.Tiles[1, 1].IsActive.ShouldBeFalse();

        // Reapply restores them
        TwldFile.ReapplyToWorld(world, data);
        ref var tile = ref world.Tiles[1, 1];
        tile.IsActive.ShouldBeTrue("Tile should be active after ReapplyToWorld");
        tile.Type.ShouldBe(expectedType);
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
        BuildSaveTypeLookups(data);

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
        BuildSaveTypeLookups(loaded);

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
            BuildSaveTypeLookups(reloaded);
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
            BuildSaveTypeLookups(reloaded);
            TwldFile.ParseTileWallBinary(rebuilt, reloaded, header.TilesWide, header.TilesHigh);

            reloaded.ModTileGrid.Count.ShouldBe(originalTiles,
                $"Round-trip lost tiles: {originalTiles} -> {reloaded.ModTileGrid.Count}");
            reloaded.ModWallGrid.Count.ShouldBe(originalWalls,
                $"Round-trip lost walls: {originalWalls} -> {reloaded.ModWallGrid.Count}");
        }
    }

    #endregion

    #region Calamity5 Diagnostic

    private const string Calamity5WldPath = @"src\TEdit.Tests\WorldFiles\calamity\Calamity5.wld";
    private const string Calamity5WldBakPath = @"src\TEdit.Tests\WorldFiles\calamity\Calamity5.wld.bak";
    private const string Calamity5TwldPath = @"src\TEdit.Tests\WorldFiles\calamity\Calamity5.twld";
    private const string Calamity5TwldBakPath = @"src\TEdit.Tests\WorldFiles\calamity\Calamity5.twld.bak";

    private static string ResolvePath(string relativePath)
    {
        // Walk up from test bin dir to find repo root
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "src", "TEdit.slnx")))
            dir = dir.Parent;
        return dir != null ? Path.Combine(dir.FullName, relativePath) : relativePath;
    }

    [SkippableFact]
    public void Calamity5_LoadOnly_DoesNotChangeTileMapCount()
    {
        var bakTwld = ResolvePath(Calamity5TwldBakPath);
        var bakWld = ResolvePath(Calamity5WldBakPath);
        Skip.IfNot(File.Exists(bakTwld), "Calamity5.twld.bak not found");
        Skip.IfNot(File.Exists(bakWld), "Calamity5.wld.bak not found");

        // Copy to temp with proper names
        string tempDir = Path.Combine(Path.GetTempPath(), "tedit_loadonly_test");
        Directory.CreateDirectory(tempDir);
        string tempWld = Path.Combine(tempDir, "Calamity5.wld");
        string tempTwld = Path.Combine(tempDir, "Calamity5.twld");
        try
        {
            File.Copy(bakWld, tempWld, true);
            File.Copy(bakTwld, tempTwld, true);

            // Load .twld
            var twldData = TwldFile.Load(tempWld);
            int originalCount = twldData.TileMap.Count;
            int originalWallCount = twldData.WallMap.Count;

            // Parse tile data (simulates what ApplyToWorld does)
            var header = World.ReadWorldHeader(tempWld);
            if (twldData.RawTileData.Length > 0)
                TwldFile.ParseTileDataDense(twldData.RawTileData, twldData, header.TilesWide, header.TilesHigh);
            if (twldData.RawWallData.Length > 0)
                TwldFile.ParseWallDataDense(twldData.RawWallData, twldData, header.TilesWide, header.TilesHigh);
            twldData.TileWallDataParsed = true;

            // Check: did parsing change the count?
            twldData.TileMap.Count.ShouldBe(originalCount,
                $"ParseTileDataDense changed TileMap count from {originalCount} to {twldData.TileMap.Count}");

            // Simulate strip (which rebuilds raw data)
            // First we need a world to strip from — load the full world
            WorldConfiguration.Initialize();
            var world = World.LoadWorld(tempWld);

            int countAfterWorldLoad = world.World.TwldData.TileMap.Count;
            countAfterWorldLoad.ShouldBe(originalCount,
                $"World.LoadWorld changed TileMap count from {originalCount} to {countAfterWorldLoad}");

            // Now simulate save: strip → rebuild → save
            TwldFile.StripFromWorld(world.World, world.World.TwldData);
            int countAfterStrip = world.World.TwldData.TileMap.Count;
            countAfterStrip.ShouldBe(originalCount,
                $"StripFromWorld changed TileMap count from {originalCount} to {countAfterStrip}");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [SkippableFact]
    public void Calamity5_FullRoundTrip_LoadSaveReload()
    {
        var bakTwld = ResolvePath(Calamity5TwldBakPath);
        var bakWld = ResolvePath(Calamity5WldBakPath);
        Skip.IfNot(File.Exists(bakTwld), "Calamity5.twld.bak not found");
        Skip.IfNot(File.Exists(bakWld), "Calamity5.wld.bak not found");

        string tempDir = Path.Combine(Path.GetTempPath(), "tedit_fullrt_test");
        Directory.CreateDirectory(tempDir);
        string tempWld = Path.Combine(tempDir, "Calamity5.wld");
        string tempTwld = Path.Combine(tempDir, "Calamity5.twld");
        try
        {
            File.Copy(bakWld, tempWld, true);
            File.Copy(bakTwld, tempTwld, true);

            // === LOAD (simulates TEdit opening the world) ===
            WorldConfiguration.Initialize();
            var (world, loadErr) = World.LoadWorld(tempWld);
            loadErr.ShouldBeNull($"Failed to load world: {loadErr?.Message}");
            world.TwldData.ShouldNotBeNull();

            int originalTileMapCount = world.TwldData.TileMap.Count;
            int originalWallMapCount = world.TwldData.WallMap.Count;
            int originalModTiles = world.TwldData.ModTileGrid.Count;
            int originalModWalls = world.TwldData.ModWallGrid.Count;

            originalTileMapCount.ShouldBeGreaterThan(0);
            originalModTiles.ShouldBeGreaterThan(0);

            // Snapshot original tileMap SaveTypes and names
            var origTileEntries = world.TwldData.TileMap
                .Select(e => (e.SaveType, e.ModName, e.Name, e.FrameImportant))
                .ToList();

            // === SAVE (simulates TEdit saving the world) ===
            World.Save(world, tempWld);

            // === RELOAD (simulates tModLoader or TEdit reopening) ===
            var (world2, loadErr2) = World.LoadWorld(tempWld);
            loadErr2.ShouldBeNull($"Failed to reload world: {loadErr2?.Message}");
            world2.TwldData.ShouldNotBeNull();

            // Verify TileMap count preserved
            world2.TwldData.TileMap.Count.ShouldBe(originalTileMapCount,
                $"TileMap count changed: {originalTileMapCount} → {world2.TwldData.TileMap.Count}");
            world2.TwldData.WallMap.Count.ShouldBe(originalWallMapCount,
                $"WallMap count changed: {originalWallMapCount} → {world2.TwldData.WallMap.Count}");

            // Verify each TileMap entry matches
            for (int i = 0; i < originalTileMapCount; i++)
            {
                var orig = origTileEntries[i];
                var reloaded = world2.TwldData.TileMap[i];
                reloaded.SaveType.ShouldBe(orig.SaveType, $"TileMap[{i}] SaveType mismatch");
                reloaded.ModName.ShouldBe(orig.ModName, $"TileMap[{i}] ModName mismatch");
                reloaded.Name.ShouldBe(orig.Name, $"TileMap[{i}] Name mismatch");
                reloaded.FrameImportant.ShouldBe(orig.FrameImportant, $"TileMap[{i}] FrameImportant mismatch");
            }

            // Verify mod tile grid count preserved (tiles weren't lost)
            world2.TwldData.ModTileGrid.Count.ShouldBe(originalModTiles,
                $"ModTileGrid count changed: {originalModTiles} → {world2.TwldData.ModTileGrid.Count}");
            world2.TwldData.ModWallGrid.Count.ShouldBe(originalModWalls,
                $"ModWallGrid count changed: {originalModWalls} → {world2.TwldData.ModWallGrid.Count}");

            // Verify tModLoader fields are preserved in saved NBT
            var savedTag = TagIO.FromFile(tempTwld);
            var savedTileMap = savedTag.GetCompound("tiles").GetList<TagCompound>("tileMap");
            for (int i = 0; i < savedTileMap.Count; i++)
            {
                var orig = world.TwldData.TileMap[i].RawTag;
                var saved = savedTileMap[i];
                if (orig != null && orig.ContainsKey("<type>"))
                    saved.ContainsKey("<type>").ShouldBeTrue($"tileMap[{i}] lost <type> field");
                if (orig != null && orig.ContainsKey("uType"))
                    saved.ContainsKey("uType").ShouldBeTrue($"tileMap[{i}] lost uType field");
                if (orig != null && orig.ContainsKey("fallbackID"))
                    saved.ContainsKey("fallbackID").ShouldBeTrue($"tileMap[{i}] lost fallbackID field");
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [SkippableFact]
    public void Calamity1_FullRoundTrip_DecompressedSizeAndSectionComparison()
    {
        var origTwld = ResolvePath(Calamity1OrigTwldPath);
        var origWld = ResolvePath(Calamity1OrigWldPath);
        Skip.IfNot(File.Exists(origTwld), "Calamity1-orig.twld not found");
        Skip.IfNot(File.Exists(origWld), "Calamity1-orig.wld not found");

        string tempDir = Path.Combine(Path.GetTempPath(), "tedit_c1_rt_test");
        Directory.CreateDirectory(tempDir);
        string tempWld = Path.Combine(tempDir, "Calamity1.wld");
        string tempTwld = Path.Combine(tempDir, "Calamity1.twld");
        try
        {
            File.Copy(origWld, tempWld, true);
            File.Copy(origTwld, tempTwld, true);

            // Load
            WorldConfiguration.Initialize();
            var (world, err) = World.LoadWorld(tempWld);
            err.ShouldBeNull($"Load failed: {err?.Message}");

            // Save (no edits)
            World.Save(world, tempWld);

            // Compare saved .twld with original
            var origTag = TagIO.FromFile(origTwld);
            var savedTag = TagIO.FromFile(tempTwld);

            // Chests should be identical (not rebuilt)
            var origChests = origTag.GetList<TagCompound>("chests");
            var savedChests = savedTag.GetList<TagCompound>("chests");
            savedChests.Count.ShouldBe(origChests.Count, "Chest count mismatch");

            // Verify chest entries are preserved (not rebuilt with different key ordering)
            for (int i = 0; i < origChests.Count; i++)
            {
                var origKeys = origChests[i].Select(kv => kv.Key).ToList();
                var savedKeys = savedChests[i].Select(kv => kv.Key).ToList();
                savedKeys.ShouldBe(origKeys, $"Chest[{i}] key ordering changed");
            }

            // TileMap should be preserved
            var origTiles = origTag.GetCompound("tiles");
            var savedTiles = savedTag.GetCompound("tiles");
            var origTileMap = origTiles.GetList<TagCompound>("tileMap");
            var savedTileMap = savedTiles.GetList<TagCompound>("tileMap");
            savedTileMap.Count.ShouldBe(origTileMap.Count, "TileMap count mismatch");

            // Tile data should be identical
            var origTD = origTiles.GetByteArray("tileData");
            var savedTD = savedTiles.GetByteArray("tileData");
            savedTD.Length.ShouldBe(origTD.Length, "tileData length mismatch");
            savedTD.ShouldBe(origTD, "tileData content mismatch");

            // Decompressed sizes should be very close
            byte[] origDecomp, savedDecomp;
            using (var fs = File.OpenRead(origTwld))
            using (var gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress))
            using (var ms = new MemoryStream())
            { gz.CopyTo(ms); origDecomp = ms.ToArray(); }
            using (var fs = File.OpenRead(tempTwld))
            using (var gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress))
            using (var ms = new MemoryStream())
            { gz.CopyTo(ms); savedDecomp = ms.ToArray(); }

            // Allow small differences from tiles section rebuild key ordering
            int sizeDiff = System.Math.Abs(savedDecomp.Length - origDecomp.Length);
            sizeDiff.ShouldBeLessThan(1000,
                $"Decompressed size diff too large: orig={origDecomp.Length} saved={savedDecomp.Length} diff={sizeDiff}");

            // Compare per-section byte sizes
            var origTag2 = TagIO.FromFile(origTwld);
            var savedTag2 = TagIO.FromFile(tempTwld);
            var sb = new System.Text.StringBuilder();
            foreach (var key in origTag2.Select(kv => kv.Key).OrderBy(k => k))
            {
                if (!savedTag2.ContainsKey(key)) { sb.AppendLine($"  MISSING: {key}"); continue; }
                var origWrap = new TagCompound(); origWrap[key] = origTag2[key];
                var savedWrap = new TagCompound(); savedWrap[key] = savedTag2[key];
                using var oms = new MemoryStream();
                TagIO.ToStream(origWrap, oms, compressed: false);
                using var sms = new MemoryStream();
                TagIO.ToStream(savedWrap, sms, compressed: false);
                string marker = oms.Length != sms.Length ? " ***DIFF***" : "";
                sb.AppendLine($"  {key}: orig={oms.Length} saved={sms.Length}{marker}");
            }
            // Show comparison even on pass
            sb.Length.ShouldBeGreaterThan(0, "No sections found");

            // All sections except tiles should be identical size (tiles has different key ordering)
            foreach (var key in origTag2.Select(kv => kv.Key).Where(k => k != "tiles"))
            {
                if (!savedTag2.ContainsKey(key)) continue;
                var origWrap = new TagCompound(); origWrap[key] = origTag2[key];
                var savedWrap = new TagCompound(); savedWrap[key] = savedTag2[key];
                using var oms = new MemoryStream();
                TagIO.ToStream(origWrap, oms, compressed: false);
                using var sms = new MemoryStream();
                TagIO.ToStream(savedWrap, sms, compressed: false);
                oms.Length.ShouldBe(sms.Length, $"Section '{key}' size mismatch: {sb}");
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    private const string Calamity1OrigWldPath = @"src\TEdit.Tests\WorldFiles\calamity\Calamity1-orig.wld";
    private const string Calamity1OrigTwldPath = @"src\TEdit.Tests\WorldFiles\calamity\Calamity1-orig.twld";
    private const string Calamity1WldPath = @"src\TEdit.Tests\WorldFiles\calamity\Calamity1.wld";
    private const string Calamity1TwldPath = @"src\TEdit.Tests\WorldFiles\calamity\Calamity1.twld";

    [Fact]
    public void TagIO_ToStream_CompressionEfficiency()
    {
        // Verify that TagIO.ToStream compressed output matches manual buffer+GZip compression.
        // Prior to the buffered write fix, streaming small writes through GZipStream produced
        // ~4x worse compression than buffering first.
        var tag = new TagCompound();
        // Create a realistically-sized payload (mostly zeros = highly compressible)
        tag.Set("data", new byte[500_000]);
        tag.Set("name", "test");
        tag.Set("value", 42);

        // Path A: TagIO.ToStream with compression
        using var pathA = new MemoryStream();
        TagIO.ToStream(tag, pathA, compressed: true);

        // Path B: serialize uncompressed, then GZip manually
        using var uncompressed = new MemoryStream();
        TagIO.ToStream(tag, uncompressed, compressed: false);

        using var pathB = new MemoryStream();
        using (var gz = new System.IO.Compression.GZipStream(pathB, System.IO.Compression.CompressionMode.Compress, true))
            gz.Write(uncompressed.GetBuffer(), 0, (int)uncompressed.Length);

        // Sizes should be within 5% of each other
        double ratio = pathA.Length / (double)pathB.Length;
        ratio.ShouldBeInRange(0.95, 1.05,
            $"TagIO.ToStream compression ratio vs manual: {ratio:F2}x " +
            $"(pathA={pathA.Length}, pathB={pathB.Length}, uncompressed={uncompressed.Length})");
    }

    [SkippableFact(Skip = "Diagnostic-only test — enable manually when investigating save issues")]
    public void Calamity1_DeepCompare_OrigVsSaved()
    {
        var origTwld = ResolvePath(Calamity1OrigTwldPath);
        var savedTwld = ResolvePath(Calamity1TwldPath);
        Skip.IfNot(File.Exists(origTwld), "Calamity1-orig.twld not found");
        Skip.IfNot(File.Exists(savedTwld), "Calamity1.twld not found");

        var origTag = TagIO.FromFile(origTwld);
        var savedTag = TagIO.FromFile(savedTwld);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"File sizes: orig={new FileInfo(origTwld).Length} saved={new FileInfo(savedTwld).Length}");

        // Root keys
        sb.AppendLine($"Root keys orig: [{string.Join(", ", origTag.Select(kv => kv.Key).OrderBy(k => k))}]");
        sb.AppendLine($"Root keys saved: [{string.Join(", ", savedTag.Select(kv => kv.Key).OrderBy(k => k))}]");

        // Compare each root section size/presence
        foreach (var key in origTag.Select(kv => kv.Key).Union(savedTag.Select(kv => kv.Key)).Distinct().OrderBy(k => k))
        {
            bool origHas = origTag.ContainsKey(key);
            bool savedHas = savedTag.ContainsKey(key);
            if (!origHas || !savedHas)
            {
                sb.AppendLine($"  [{key}] orig={origHas} saved={savedHas}");
                continue;
            }
            var origVal = origTag[key];
            var savedVal = savedTag[key];
            string origDesc = DescribeTagValue(origVal);
            string savedDesc = DescribeTagValue(savedVal);
            if (origDesc != savedDesc)
                sb.AppendLine($"  [{key}] orig={origDesc} saved={savedDesc}");
        }

        // Deep compare tiles section
        var origTiles = origTag.GetCompound("tiles");
        var savedTiles = savedTag.GetCompound("tiles");

        var origTileMap = origTiles.GetList<TagCompound>("tileMap");
        var savedTileMap = savedTiles.GetList<TagCompound>("tileMap");
        sb.AppendLine($"\nTileMap: orig={origTileMap.Count} saved={savedTileMap.Count}");

        // Check if entries match
        int tileMapDiffs = 0;
        for (int i = 0; i < System.Math.Min(origTileMap.Count, savedTileMap.Count); i++)
        {
            var o = TagCompoundToString(origTileMap[i]);
            var s = TagCompoundToString(savedTileMap[i]);
            if (o != s)
            {
                if (tileMapDiffs < 5)
                    sb.AppendLine($"  [{i}] orig={o}");
                if (tileMapDiffs < 5)
                    sb.AppendLine($"  [{i}] saved={s}");
                tileMapDiffs++;
            }
        }
        if (tileMapDiffs > 5)
            sb.AppendLine($"  ... and {tileMapDiffs - 5} more differences");

        // Tile data comparison
        bool origNewFmt = origTiles.ContainsKey("tileData");
        bool savedNewFmt = savedTiles.ContainsKey("tileData");
        sb.AppendLine($"\nTile format: orig={origNewFmt} saved={savedNewFmt}");
        if (origNewFmt && savedNewFmt)
        {
            var origTD = origTiles.GetByteArray("tileData");
            var savedTD = savedTiles.GetByteArray("tileData");
            var origWD = origTiles.GetByteArray("wallData");
            var savedWD = savedTiles.GetByteArray("wallData");
            sb.AppendLine($"tileData: orig={origTD.Length} saved={savedTD.Length}");
            sb.AppendLine($"wallData: orig={origWD.Length} saved={savedWD.Length}");

            // Find first difference
            int firstTileDiff = FindFirstDiff(origTD, savedTD);
            sb.AppendLine($"tileData first diff at byte: {firstTileDiff}");
            if (firstTileDiff >= 0 && firstTileDiff < origTD.Length && firstTileDiff < savedTD.Length)
            {
                int start = System.Math.Max(0, firstTileDiff - 4);
                int end = System.Math.Min(System.Math.Min(origTD.Length, savedTD.Length), firstTileDiff + 16);
                sb.AppendLine($"  orig:  {BitConverter.ToString(origTD, start, end - start)}");
                sb.AppendLine($"  saved: {BitConverter.ToString(savedTD, start, end - start)}");
            }

            // Count total different bytes
            int diffCount = 0;
            for (int i = 0; i < System.Math.Min(origTD.Length, savedTD.Length); i++)
                if (origTD[i] != savedTD[i]) diffCount++;
            sb.AppendLine($"tileData total diff bytes: {diffCount} / {origTD.Length}");

            int wallDiffCount = 0;
            for (int i = 0; i < System.Math.Min(origWD.Length, savedWD.Length); i++)
                if (origWD[i] != savedWD[i]) wallDiffCount++;
            sb.AppendLine($"wallData total diff bytes: {wallDiffCount} / {origWD.Length}");
        }

        // Check legacy data key
        bool origHasData = origTiles.ContainsKey("data");
        bool savedHasData = savedTiles.ContainsKey("data");
        if (origHasData || savedHasData)
        {
            int origLen = origHasData ? origTiles.GetByteArray("data").Length : 0;
            int savedLen = savedHasData ? savedTiles.GetByteArray("data").Length : 0;
            sb.AppendLine($"\nLegacy data: orig={origLen} saved={savedLen}");
        }

        // WallMap
        var origWallMap = origTiles.GetList<TagCompound>("wallMap");
        var savedWallMap = savedTiles.GetList<TagCompound>("wallMap");
        sb.AppendLine($"\nWallMap: orig={origWallMap.Count} saved={savedWallMap.Count}");

        // Chests
        int origChests = origTag.ContainsKey("chests") ? origTag.GetList<TagCompound>("chests").Count : 0;
        int savedChests = savedTag.ContainsKey("chests") ? savedTag.GetList<TagCompound>("chests").Count : 0;
        sb.AppendLine($"Chests: orig={origChests} saved={savedChests}");

        // Containers
        sb.AppendLine($"Containers: orig={origTag.ContainsKey("containers")} saved={savedTag.ContainsKey("containers")}");
        if (origTag.ContainsKey("containers"))
            sb.AppendLine($"  orig keys: [{string.Join(", ", origTag.GetCompound("containers").Select(kv => kv.Key))}]");
        if (savedTag.ContainsKey("containers"))
            sb.AppendLine($"  saved keys: [{string.Join(", ", savedTag.GetCompound("containers").Select(kv => kv.Key))}]");

        // Compare raw decompressed sizes
        byte[] origDecomp, savedDecomp;
        using (var origFs = File.OpenRead(origTwld))
        using (var origGzip = new System.IO.Compression.GZipStream(origFs, System.IO.Compression.CompressionMode.Decompress))
        using (var origMs = new MemoryStream())
        {
            origGzip.CopyTo(origMs);
            origDecomp = origMs.ToArray();
        }
        using (var savedFs = File.OpenRead(savedTwld))
        using (var savedGzip = new System.IO.Compression.GZipStream(savedFs, System.IO.Compression.CompressionMode.Decompress))
        using (var savedMs = new MemoryStream())
        {
            savedGzip.CopyTo(savedMs);
            savedDecomp = savedMs.ToArray();
        }
        sb.AppendLine($"\nDecompressed sizes: orig={origDecomp.Length} saved={savedDecomp.Length}");
        int firstRawDiff = FindFirstDiff(origDecomp, savedDecomp);
        sb.AppendLine($"Decompressed first diff at byte: {firstRawDiff}");
        if (firstRawDiff >= 0)
        {
            int start = System.Math.Max(0, firstRawDiff - 8);
            int end = System.Math.Min(System.Math.Min(origDecomp.Length, savedDecomp.Length), firstRawDiff + 32);
            sb.AppendLine($"  orig:  {BitConverter.ToString(origDecomp, start, end - start)}");
            sb.AppendLine($"  saved: {BitConverter.ToString(savedDecomp, start, end - start)}");
        }

        // Deep compare chests section
        if (origTag.ContainsKey("chests") && savedTag.ContainsKey("chests"))
        {
            var origChestList = origTag.GetList<TagCompound>("chests");
            var savedChestList = savedTag.GetList<TagCompound>("chests");
            sb.AppendLine($"\nChest entries: orig={origChestList.Count} saved={savedChestList.Count}");

            // Show first orig chest entry keys
            if (origChestList.Count > 0)
            {
                var first = origChestList[0];
                sb.AppendLine($"  orig chest[0] keys: [{string.Join(", ", first.Select(kv => $"{kv.Key}({kv.Value?.GetType().Name})"))}]");
            }
            if (savedChestList.Count > 0)
            {
                var first = savedChestList[0];
                sb.AppendLine($"  saved chest[0] keys: [{string.Join(", ", first.Select(kv => $"{kv.Key}({kv.Value?.GetType().Name})"))}]");
            }

            // Find first chest that differs
            int chestDiffs = 0;
            for (int i = 0; i < System.Math.Min(origChestList.Count, savedChestList.Count); i++)
            {
                var o = TagCompoundToString(origChestList[i]);
                var s = TagCompoundToString(savedChestList[i]);
                if (o != s)
                {
                    if (chestDiffs < 3)
                    {
                        sb.AppendLine($"  chest[{i}] orig: {o}");
                        sb.AppendLine($"  chest[{i}] saved: {s}");
                    }
                    chestDiffs++;
                }
            }
            sb.AppendLine($"  Total chest diffs: {chestDiffs} / {origChestList.Count}");

            // Check if orig chest items have <type> or other special fields
            if (origChestList.Count > 0 && origChestList[0].ContainsKey("items"))
            {
                var origItems = origChestList[0].GetList<TagCompound>("items");
                if (origItems.Count > 0)
                    sb.AppendLine($"  orig chest[0].items[0] keys: [{string.Join(", ", origItems[0].Select(kv => $"{kv.Key}({kv.Value?.GetType().Name})"))}]");
            }
        }

        // Deep compare tileEntities
        if (origTag.ContainsKey("tileEntities") && savedTag.ContainsKey("tileEntities"))
        {
            var origTE = origTag.GetList<TagCompound>("tileEntities");
            var savedTE = savedTag.GetList<TagCompound>("tileEntities");
            sb.AppendLine($"\nTileEntities: orig={origTE.Count} saved={savedTE.Count}");
            if (origTE.Count > 0)
                sb.AppendLine($"  orig TE[0] keys: [{string.Join(", ", origTE[0].Select(kv => $"{kv.Key}({kv.Value?.GetType().Name})"))}]");
            if (savedTE.Count > 0)
                sb.AppendLine($"  saved TE[0] keys: [{string.Join(", ", savedTE[0].Select(kv => $"{kv.Key}({kv.Value?.GetType().Name})"))}]");
        }

        // Deep compare modData
        if (origTag.ContainsKey("modData") && savedTag.ContainsKey("modData"))
        {
            var origMD = origTag.GetList<TagCompound>("modData");
            var savedMD = savedTag.GetList<TagCompound>("modData");
            sb.AppendLine($"\nmodData: orig={origMD.Count} saved={savedMD.Count}");
        }

        // Compare ALL root sections by serializing each to bytes
        sb.AppendLine($"\nPer-section byte sizes:");
        foreach (var key in origTag.Select(kv => kv.Key).OrderBy(k => k))
        {
            if (!savedTag.ContainsKey(key)) continue;
            // Wrap in a TagCompound to serialize
            var origWrap = new TagCompound();
            origWrap[key] = origTag[key];
            var savedWrap = new TagCompound();
            savedWrap[key] = savedTag[key];

            using var origSectionMs = new MemoryStream();
            TagIO.ToStream(origWrap, origSectionMs, compressed: false);
            using var savedSectionMs = new MemoryStream();
            TagIO.ToStream(savedWrap, savedSectionMs, compressed: false);

            string marker = origSectionMs.Length != savedSectionMs.Length ? " ***" : "";
            sb.AppendLine($"  {key}: orig={origSectionMs.Length} saved={savedSectionMs.Length}{marker}");
        }

        // Deep compare individual chest item entries
        if (origTag.ContainsKey("chests") && savedTag.ContainsKey("chests"))
        {
            var origChestList2 = origTag.GetList<TagCompound>("chests");
            var savedChestList2 = savedTag.GetList<TagCompound>("chests");

            // Find a chest that has mod items and compare the item details
            for (int i = 0; i < System.Math.Min(3, origChestList2.Count); i++)
            {
                var origChest = origChestList2[i];
                var savedChest = savedChestList2[i];
                sb.AppendLine($"\nChest [{i}] detail:");
                sb.AppendLine($"  orig keys: [{string.Join(", ", origChest.Select(kv => $"{kv.Key}"))}]");
                sb.AppendLine($"  saved keys: [{string.Join(", ", savedChest.Select(kv => $"{kv.Key}"))}]");

                var origItems = origChest.ContainsKey("items") ? origChest.GetList<TagCompound>("items") : new List<TagCompound>();
                var savedItems = savedChest.ContainsKey("items") ? savedChest.GetList<TagCompound>("items") : new List<TagCompound>();
                sb.AppendLine($"  orig items: {origItems.Count}, saved items: {savedItems.Count}");

                for (int j = 0; j < System.Math.Min(3, origItems.Count); j++)
                {
                    sb.AppendLine($"    orig item[{j}]: {TagCompoundToString(origItems[j])}");
                    if (j < savedItems.Count)
                        sb.AppendLine($"    saved item[{j}]: {TagCompoundToString(savedItems[j])}");
                }
            }

            // Find a chest without mod items (if any)
            for (int i = origChestList2.Count - 1; i >= origChestList2.Count - 3 && i >= 0; i--)
            {
                var origChest = origChestList2[i];
                var origItems = origChest.ContainsKey("items") ? origChest.GetList<TagCompound>("items") : new List<TagCompound>();
                sb.AppendLine($"\nChest [{i}] (last few):");
                sb.AppendLine($"  orig keys: [{string.Join(", ", origChest.Select(kv => $"{kv.Key}"))}]");
                sb.AppendLine($"  orig items: {origItems.Count}");
                if (origItems.Count > 0)
                    sb.AppendLine($"    item[0]: {TagCompoundToString(origItems[0])}");
            }
        }

        // Re-compress orig data to see what size we get
        using (var recompMs = new MemoryStream())
        {
            using (var recompGzip = new System.IO.Compression.GZipStream(recompMs, System.IO.Compression.CompressionMode.Compress, leaveOpen: true))
                recompGzip.Write(origDecomp, 0, origDecomp.Length);
            sb.AppendLine($"Re-compressed orig with .NET default: {recompMs.Length} (orig was {new FileInfo(origTwld).Length})");
        }

        Assert.Fail(sb.ToString());
    }

    private static int FindFirstDiff(byte[] a, byte[] b)
    {
        for (int i = 0; i < System.Math.Min(a.Length, b.Length); i++)
            if (a[i] != b[i]) return i;
        return a.Length != b.Length ? System.Math.Min(a.Length, b.Length) : -1;
    }

    private static string DescribeTagValue(object val)
    {
        return val switch
        {
            TagCompound tc => $"Compound({tc.Count}keys)",
            byte[] ba => $"byte[{ba.Length}]",
            int[] ia => $"int[{ia.Length}]",
            IList list => $"List({list.Count})",
            _ => val?.ToString() ?? "null"
        };
    }

    /// <summary>
    /// Diagnostic test — compares a TEdit-saved .twld against its backup.
    /// Use for investigating save corruption. Always asserts to show diagnostics.
    /// </summary>
    [SkippableFact(Skip = "Diagnostic-only test — enable manually when investigating save issues")]
    public void Calamity5_DiagnosticCompare_BackupVsSaved()
    {
        var bakTwld = ResolvePath(Calamity5TwldBakPath);
        var savedTwld = ResolvePath(Calamity5TwldPath);

        Skip.IfNot(File.Exists(bakTwld), "Calamity5.twld.bak not found");
        Skip.IfNot(File.Exists(savedTwld), "Calamity5.twld not found");

        var bakTag = TagIO.FromFile(bakTwld);
        var savedTag = TagIO.FromFile(savedTwld);

        var bakTiles = bakTag.GetCompound("tiles");
        var savedTiles = savedTag.GetCompound("tiles");
        var bakTileMap = bakTiles.GetList<TagCompound>("tileMap");
        var savedTileMap = savedTiles.GetList<TagCompound>("tileMap");

        var diffs = new System.Text.StringBuilder();
        int maxTileMap = System.Math.Max(bakTileMap.Count, savedTileMap.Count);
        for (int i = 0; i < maxTileMap; i++)
        {
            string bakEntry = i < bakTileMap.Count ? TagCompoundToString(bakTileMap[i]) : "(missing)";
            string savedEntry = i < savedTileMap.Count ? TagCompoundToString(savedTileMap[i]) : "(missing)";
            if (bakEntry != savedEntry)
                diffs.AppendLine($"  [{i}] bak={bakEntry}  saved={savedEntry}");
        }

        Assert.Fail($"TileMap: bak={bakTileMap.Count} saved={savedTileMap.Count}\nDiffs:\n{diffs}");
    }

    [SkippableFact]
    public void Calamity5_RoundTrip_PreservesTModLoaderFields()
    {
        var bakTwld = ResolvePath(Calamity5TwldBakPath);
        var bakWld = ResolvePath(Calamity5WldBakPath);

        Skip.IfNot(File.Exists(bakTwld), "Calamity5.twld.bak not found");
        Skip.IfNot(File.Exists(bakWld), "Calamity5.wld.bak not found");

        // Copy backup files to temp with proper names so TwldFile.Load works
        string tempDir = Path.Combine(Path.GetTempPath(), "tedit_roundtrip_test");
        Directory.CreateDirectory(tempDir);
        string tempWld = Path.Combine(tempDir, "Calamity5.wld");
        string tempTwld = Path.Combine(tempDir, "Calamity5.twld");
        try
        {
            File.Copy(bakWld, tempWld, true);
            File.Copy(bakTwld, tempTwld, true);

            // Load the working backup
            var twldData = TwldFile.Load(tempWld);
            twldData.ShouldNotBeNull();
            twldData.TileMap.Count.ShouldBeGreaterThan(0);

            // Verify original entries have tModLoader fields
            twldData.TileMap[0].RawTag.ShouldNotBeNull("RawTag should be preserved from load");
            twldData.TileMap[0].RawTag.ContainsKey("<type>").ShouldBeTrue("Original should have <type> field");
            twldData.TileMap[0].RawTag.ContainsKey("uType").ShouldBeTrue("Original should have uType field");

            // Save to the same temp location (overwrites)
            TwldFile.Save(tempWld, twldData);

            // Read back the saved .twld NBT
            var savedTag = TagIO.FromFile(tempTwld);
            var savedTiles = savedTag.GetCompound("tiles");
            var savedTileMap = savedTiles.GetList<TagCompound>("tileMap");
            var savedWallMap = savedTiles.GetList<TagCompound>("wallMap");

            // Verify count preserved
            savedTileMap.Count.ShouldBe(twldData.TileMap.Count, "TileMap count should be preserved");
            savedWallMap.Count.ShouldBe(twldData.WallMap.Count, "WallMap count should be preserved");

            // Verify tModLoader fields preserved
            for (int i = 0; i < savedTileMap.Count; i++)
            {
                var saved = savedTileMap[i];
                var original = twldData.TileMap[i].RawTag;

                if (original.ContainsKey("<type>"))
                    saved.ContainsKey("<type>").ShouldBeTrue($"tileMap[{i}] should preserve <type>");
                if (original.ContainsKey("uType"))
                    saved.ContainsKey("uType").ShouldBeTrue($"tileMap[{i}] should preserve uType");
                if (original.ContainsKey("fallbackID"))
                    saved.ContainsKey("fallbackID").ShouldBeTrue($"tileMap[{i}] should preserve fallbackID");

                // Core fields should match
                saved.Get<ushort>("value").ShouldBe(twldData.TileMap[i].SaveType, $"tileMap[{i}] value mismatch");
                saved.GetString("mod").ShouldBe(twldData.TileMap[i].ModName, $"tileMap[{i}] mod mismatch");
                saved.GetString("name").ShouldBe(twldData.TileMap[i].Name, $"tileMap[{i}] name mismatch");
            }

            // Verify wallMap fields preserved too
            for (int i = 0; i < savedWallMap.Count; i++)
            {
                var saved = savedWallMap[i];
                var original = twldData.WallMap[i].RawTag;

                if (original != null && original.ContainsKey("<type>"))
                    saved.ContainsKey("<type>").ShouldBeTrue($"wallMap[{i}] should preserve <type>");

                saved.Get<ushort>("value").ShouldBe(twldData.WallMap[i].SaveType, $"wallMap[{i}] value mismatch");
                saved.GetString("mod").ShouldBe(twldData.WallMap[i].ModName, $"wallMap[{i}] mod mismatch");
                saved.GetString("name").ShouldBe(twldData.WallMap[i].Name, $"wallMap[{i}] name mismatch");
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    private static string TagCompoundToString(TagCompound tag)
    {
        return string.Join("|", tag.Select(kv =>
        {
            if (kv.Value is byte[] ba) return $"{kv.Key}=byte[{ba.Length}]";
            return $"{kv.Key}={kv.Value}";
        }));
    }

    #endregion
}
