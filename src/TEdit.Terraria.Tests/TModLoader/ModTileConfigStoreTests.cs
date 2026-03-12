using System.IO;
using System.Text.Json;
using Shouldly;
using TEdit.Common.Serialization;
using TEdit.Geometry;
using TEdit.Terraria.Objects;
using TEdit.Terraria.TModLoader;

namespace TEdit.Terraria.Tests.TModLoader;

public class ModTileConfigStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ModTileConfigStore _store;

    public ModTileConfigStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "TEdit_Tests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _store = new ModTileConfigStore(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void Load_NonExistentMod_ReturnsEmpty()
    {
        var result = _store.LoadUser("NonExistentMod");
        result.ShouldNotBeNull();
        result.Tiles.ShouldBeEmpty();
    }

    [Fact]
    public void SaveAndLoad_RoundTrip_PreservesData()
    {
        var file = new ModTileOverrideFile();
        file.Tiles["TestTile"] = new ModTileOverride
        {
            TextureGrid = new Vector2Short(32, 32),
            FrameGap = new Vector2Short(2, 2),
            FrameSize = [new Vector2Short(2, 3)],
            IsAnimated = true,
            Frames =
            [
                new FrameProperty
                {
                    Name = "Style A",
                    UV = new Vector2Short(0, 0),
                    Size = new Vector2Short(2, 3),
                    Anchor = FrameAnchor.Bottom,
                },
                new FrameProperty
                {
                    Name = "Style B",
                    Variety = "Alt",
                    UV = new Vector2Short(68, 0),
                    Size = new Vector2Short(2, 3),
                    Anchor = FrameAnchor.Bottom,
                }
            ]
        };

        _store.Save("CalamityMod", file);
        var loaded = _store.LoadUser("CalamityMod");

        loaded.Tiles.ShouldContainKey("TestTile");
        var tile = loaded.Tiles["TestTile"];
        tile.TextureGrid.X.ShouldBe((short)32);
        tile.TextureGrid.Y.ShouldBe((short)32);
        tile.FrameSize.ShouldNotBeNull();
        tile.FrameSize![0].X.ShouldBe((short)2);
        tile.FrameSize[0].Y.ShouldBe((short)3);
        tile.IsAnimated.ShouldBeTrue();
        tile.Frames.ShouldNotBeNull();
        tile.Frames!.Count.ShouldBe(2);
        tile.Frames[0].Name.ShouldBe("Style A");
        tile.Frames[0].Anchor.ShouldBe(FrameAnchor.Bottom);
        tile.Frames[1].Variety.ShouldBe("Alt");
        tile.Frames[1].UV.X.ShouldBe((short)68);
    }

    [Fact]
    public void SaveTile_AddsSingleTile()
    {
        var over = new ModTileOverride
        {
            TextureGrid = new Vector2Short(16, 16),
            Frames = [new FrameProperty { Name = "Default", UV = Vector2Short.Zero }]
        };

        _store.SaveTile("TestMod", "MyTile", over);

        var loaded = _store.LoadUser("TestMod");
        loaded.Tiles.ShouldContainKey("MyTile");
        loaded.Tiles["MyTile"].Frames!.Count.ShouldBe(1);
    }

    [Fact]
    public void SaveTile_UpdatesExistingTile()
    {
        var over1 = new ModTileOverride
        {
            TextureGrid = new Vector2Short(16, 16),
            Frames = [new FrameProperty { Name = "V1" }]
        };
        _store.SaveTile("TestMod", "MyTile", over1);

        var over2 = new ModTileOverride
        {
            TextureGrid = new Vector2Short(32, 32),
            Frames = [new FrameProperty { Name = "V2" }]
        };
        _store.SaveTile("TestMod", "MyTile", over2);

        var loaded = _store.GetTileOverride("TestMod", "MyTile");
        loaded.ShouldNotBeNull();
        loaded!.TextureGrid.X.ShouldBe((short)32);
        loaded.Frames![0].Name.ShouldBe("V2");
    }

    [Fact]
    public void RemoveTile_DeletesEntry()
    {
        _store.SaveTile("TestMod", "TileA", new ModTileOverride());
        _store.SaveTile("TestMod", "TileB", new ModTileOverride());

        _store.RemoveTile("TestMod", "TileA").ShouldBeTrue();

        var loaded = _store.LoadUser("TestMod");
        loaded.Tiles.ShouldNotContainKey("TileA");
        loaded.Tiles.ShouldContainKey("TileB");
    }

    [Fact]
    public void RemoveTile_LastTile_DeletesFile()
    {
        _store.SaveTile("TestMod", "OnlyTile", new ModTileOverride());
        _store.RemoveTile("TestMod", "OnlyTile").ShouldBeTrue();

        File.Exists(_store.GetUserFilePath("TestMod")).ShouldBeFalse();
    }

    [Fact]
    public void GetTileOverride_NonExistent_ReturnsNull()
    {
        _store.GetTileOverride("FakeMod", "FakeTile").ShouldBeNull();
    }

    [Fact]
    public void GetTileOverride_UserOverridesTakesPriorityOverBundled()
    {
        // The bundled CalamityMod.json is an embedded resource (empty placeholder).
        // Save a user override for a tile — it should take priority.
        var userOver = new ModTileOverride { TextureGrid = new Vector2Short(32, 32) };
        _store.SaveTile("CalamityMod", "UserTile", userOver);

        var result = _store.GetTileOverride("CalamityMod", "UserTile");
        result.ShouldNotBeNull();
        result!.TextureGrid.X.ShouldBe((short)32);
    }

    [Fact]
    public void LoadBundled_ReturnsBundledResource()
    {
        // CalamityMod.json is embedded as a resource
        var bundled = _store.LoadBundled("CalamityMod");
        bundled.ShouldNotBeNull();
    }

    [Fact]
    public void LoadBundled_NonExistent_ReturnsNull()
    {
        var bundled = _store.LoadBundled("SomeRandomModThatDoesNotExist");
        bundled.ShouldBeNull();
    }

    [Fact]
    public void GetModsWithBundledOverrides_IncludesCalamityMod()
    {
        var mods = _store.GetModsWithBundledOverrides().ToList();
        mods.ShouldContain("CalamityMod");
    }

    [Fact]
    public void GetModsWithUserOverrides_ListsModFiles()
    {
        _store.SaveTile("ModA", "Tile1", new ModTileOverride());
        _store.SaveTile("ModB", "Tile2", new ModTileOverride());

        var mods = _store.GetModsWithUserOverrides().ToList();
        mods.ShouldContain("ModA");
        mods.ShouldContain("ModB");
    }

    [Fact]
    public void TreeMode_SerializesAsString()
    {
        var over = new ModTileOverride { TreeMode = TreeMode.Palm };
        _store.SaveTile("TestMod", "PalmTree", over);

        var json = File.ReadAllText(_store.GetUserFilePath("TestMod"));
        json.ShouldContain("\"Palm\"");
    }

    [Fact]
    public void FrameProperty_SourceRect_RoundTrips()
    {
        var over = new ModTileOverride
        {
            Frames =
            [
                new FrameProperty
                {
                    Name = "Jar",
                    SourceRect = [0, 0, 36, 44],
                    OffsetPx = [-10, 0],
                }
            ]
        };

        _store.SaveTile("TestMod", "JarTile", over);
        var loaded = _store.GetTileOverride("TestMod", "JarTile");
        loaded!.Frames![0].SourceRect.ShouldBe(new[] { 0, 0, 36, 44 });
        loaded.Frames[0].OffsetPx.ShouldBe(new short[] { -10, 0 });
    }
}

public class ModTileOverrideSerializationTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void ModTileOverride_MatchesTilesJsonFormat()
    {
        var over = new ModTileOverride
        {
            TextureGrid = new Vector2Short(16, 16),
            FrameGap = new Vector2Short(2, 2),
            FrameSize = [new Vector2Short(1, 3)],
            Frames =
            [
                new FrameProperty
                {
                    Name = "Lamp",
                    Variety = "On",
                    UV = new Vector2Short(0, 0),
                    Anchor = FrameAnchor.Bottom,
                }
            ]
        };

        var json = JsonSerializer.Serialize(over, Options);

        // Verify camelCase naming
        json.ShouldContain("\"textureGrid\"");
        json.ShouldContain("\"frameGap\"");
        json.ShouldContain("\"frameSize\"");
        json.ShouldContain("\"frames\"");

        // Verify enum as string
        json.ShouldContain("\"Bottom\"");

        // Round trip
        var restored = JsonSerializer.Deserialize<ModTileOverride>(json, Options)!;
        restored.TextureGrid.ShouldBe(new Vector2Short(16, 16));
        restored.Frames!.Count.ShouldBe(1);
        restored.Frames[0].Anchor.ShouldBe(FrameAnchor.Bottom);
    }

    [Fact]
    public void ModTileOverrideFile_SerializesCorrectly()
    {
        var file = new ModTileOverrideFile();
        file.Tiles["MyTile"] = new ModTileOverride
        {
            TextureGrid = new Vector2Short(32, 32),
        };

        var json = JsonSerializer.Serialize(file, Options);
        json.ShouldContain("\"tiles\"");
        json.ShouldContain("\"MyTile\"");

        var restored = JsonSerializer.Deserialize<ModTileOverrideFile>(json, Options)!;
        restored.Tiles.ShouldContainKey("MyTile");
        restored.Tiles["MyTile"].TextureGrid.X.ShouldBe((short)32);
    }
}
