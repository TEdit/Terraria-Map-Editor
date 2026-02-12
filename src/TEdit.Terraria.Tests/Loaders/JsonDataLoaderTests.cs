using System.Text;
using System.Text.Json;
using Shouldly;
using TEdit.Common.Serialization;
using TEdit.Terraria.DataModel;
using TEdit.Terraria.Loaders;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests.Loaders;

public class JsonDataLoaderTests
{
    [Fact]
    public void LoadList_DeserializesJsonArray()
    {
        var json = """[{"id": 1, "name": "Test"}, {"id": 2, "name": "Test2"}]""";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var result = JsonDataLoader.LoadList<WallProperty>(stream);

        result.Count.ShouldBe(2);
        result[0].Id.ShouldBe(1);
        result[0].Name.ShouldBe("Test");
    }

    [Fact]
    public void Load_DeserializesJsonObject()
    {
        var json = """{"id": 1, "name": "Test"}""";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var result = JsonDataLoader.Load<WallProperty>(stream);

        result.Id.ShouldBe(1);
        result.Name.ShouldBe("Test");
    }

    [Fact]
    public void GetDataStream_ReturnsEmbeddedResource()
    {
        // tiles.json should be embedded in TEdit.Terraria assembly
        using var stream = JsonDataLoader.GetDataStream("tiles.json");

        stream.ShouldNotBeNull();
        stream.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GetDataStream_ThrowsForMissingResource()
    {
        Should.Throw<FileNotFoundException>(() =>
            JsonDataLoader.GetDataStream("nonexistent.json"));
    }

    [Fact]
    public void GetDataStream_PrefersFsOverride()
    {
        // Create a temp directory with a test file
        var tempDir = Path.Combine(Path.GetTempPath(), $"tedit_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var testFile = Path.Combine(tempDir, "test.json");
            File.WriteAllText(testFile, """[{"id":999,"name":"Override"}]""");

            using var stream = JsonDataLoader.GetDataStream("test.json", tempDir);
            var result = JsonDataLoader.LoadList<WallProperty>(stream);

            result[0].Id.ShouldBe(999);
            result[0].Name.ShouldBe("Override");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void LoadListFromResource_LoadsTilesJson()
    {
        var tiles = JsonDataLoader.LoadListFromResource<TileProperty>("tiles.json");

        tiles.Count.ShouldBeGreaterThan(0);
        tiles[0].Name.ShouldBe("Dirt Block");
    }

    [Fact]
    public void LoadListFromResource_LoadsWallsJson()
    {
        var walls = JsonDataLoader.LoadListFromResource<WallProperty>("walls.json");

        walls.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void LoadListFromResource_LoadsItemsJson()
    {
        var items = JsonDataLoader.LoadListFromResource<ItemProperty>("items.json");

        items.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void LoadListFromResource_LoadsNpcsJson()
    {
        var npcs = JsonDataLoader.LoadListFromResource<NpcData>("npcs.json");

        npcs.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void LoadListFromResource_LoadsPaintsJson()
    {
        var paints = JsonDataLoader.LoadListFromResource<PaintProperty>("paints.json");

        paints.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void LoadListFromResource_LoadsPrefixesJson()
    {
        var prefixes = JsonDataLoader.LoadListFromResource<PrefixData>("prefixes.json");

        prefixes.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void LoadListFromResource_LoadsGlobalColorsJson()
    {
        var colors = JsonDataLoader.LoadListFromResource<GlobalColorEntry>("globalColors.json");

        colors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void SaveAndLoad_RoundTrip()
    {
        var original = new List<WallProperty>
        {
            new() { Id = 1, Name = "Wall A" },
            new() { Id = 2, Name = "Wall B" },
        };

        var tempFile = Path.GetTempFileName();
        try
        {
            JsonDataLoader.SaveToFile(original, tempFile);

            using var stream = File.OpenRead(tempFile);
            var restored = JsonDataLoader.LoadList<WallProperty>(stream);

            restored.Count.ShouldBe(2);
            restored[0].Name.ShouldBe("Wall A");
            restored[1].Name.ShouldBe("Wall B");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
