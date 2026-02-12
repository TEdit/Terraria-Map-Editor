using System.Text.Json;
using Shouldly;
using TEdit.Common.Serialization;
using TEdit.Terraria.DataModel;
using TEdit.Terraria.Loaders;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests;

/// <summary>
/// Tests that validate the embedded JSON data files can be loaded
/// and round-tripped without data loss.
/// </summary>
public class DataIntegrityTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void Tiles_RoundTrip_NoDataLoss()
    {
        var tiles = JsonDataLoader.LoadListFromResource<TileProperty>("tiles.json");
        var json = JsonSerializer.Serialize(tiles, Options);
        var restored = JsonSerializer.Deserialize<List<TileProperty>>(json, Options)!;

        restored.Count.ShouldBe(tiles.Count);
        for (int i = 0; i < tiles.Count; i++)
        {
            restored[i].Id.ShouldBe(tiles[i].Id, $"Tile index {i}");
            restored[i].Name.ShouldBe(tiles[i].Name, $"Tile index {i}");
            restored[i].IsFramed.ShouldBe(tiles[i].IsFramed, $"Tile {tiles[i].Id}");

            if (tiles[i].Frames != null)
            {
                restored[i].Frames.ShouldNotBeNull($"Tile {tiles[i].Id} frames");
                restored[i].Frames!.Count.ShouldBe(tiles[i].Frames!.Count, $"Tile {tiles[i].Id} frame count");
            }
        }
    }

    [Fact]
    public void Walls_RoundTrip_NoDataLoss()
    {
        var walls = JsonDataLoader.LoadListFromResource<WallProperty>("walls.json");
        var json = JsonSerializer.Serialize(walls, Options);
        var restored = JsonSerializer.Deserialize<List<WallProperty>>(json, Options)!;

        restored.Count.ShouldBe(walls.Count);
        for (int i = 0; i < walls.Count; i++)
        {
            restored[i].Id.ShouldBe(walls[i].Id);
            restored[i].Name.ShouldBe(walls[i].Name);
        }
    }

    [Fact]
    public void Items_RoundTrip_NoDataLoss()
    {
        var items = JsonDataLoader.LoadListFromResource<ItemProperty>("items.json");
        var json = JsonSerializer.Serialize(items, Options);
        var restored = JsonSerializer.Deserialize<List<ItemProperty>>(json, Options)!;

        restored.Count.ShouldBe(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            restored[i].Id.ShouldBe(items[i].Id);
            restored[i].Name.ShouldBe(items[i].Name);
            restored[i].IsFood.ShouldBe(items[i].IsFood);
            restored[i].IsRackable.ShouldBe(items[i].IsRackable);
            restored[i].Head.ShouldBe(items[i].Head);
        }
    }

    [Fact]
    public void Npcs_RoundTrip_NoDataLoss()
    {
        var npcs = JsonDataLoader.LoadListFromResource<NpcData>("npcs.json");
        var json = JsonSerializer.Serialize(npcs, Options);
        var restored = JsonSerializer.Deserialize<List<NpcData>>(json, Options)!;

        restored.Count.ShouldBe(npcs.Count);
    }

    [Fact]
    public void Paints_RoundTrip_NoDataLoss()
    {
        var paints = JsonDataLoader.LoadListFromResource<PaintProperty>("paints.json");
        var json = JsonSerializer.Serialize(paints, Options);
        var restored = JsonSerializer.Deserialize<List<PaintProperty>>(json, Options)!;

        restored.Count.ShouldBe(paints.Count);
    }

    [Fact]
    public void Prefixes_RoundTrip_NoDataLoss()
    {
        var prefixes = JsonDataLoader.LoadListFromResource<PrefixData>("prefixes.json");
        var json = JsonSerializer.Serialize(prefixes, Options);
        var restored = JsonSerializer.Deserialize<List<PrefixData>>(json, Options)!;

        restored.Count.ShouldBe(prefixes.Count);
    }

    [Fact]
    public void GlobalColors_RoundTrip_NoDataLoss()
    {
        var colors = JsonDataLoader.LoadListFromResource<GlobalColorEntry>("globalColors.json");
        var json = JsonSerializer.Serialize(colors, Options);
        var restored = JsonSerializer.Deserialize<List<GlobalColorEntry>>(json, Options)!;

        restored.Count.ShouldBe(colors.Count);
    }

    [Fact]
    public void Versions_RoundTrip_PreservesEntries()
    {
        using var stream = JsonDataLoader.GetDataStream("versions.json");
        var mgr = SaveVersionManager.Load(stream);

        var json = JsonSerializer.Serialize(mgr, Options);
        var restored = JsonSerializer.Deserialize<SaveVersionManager>(json, Options)!;
        restored.BuildIndexes();

        restored.SaveVersions.Count.ShouldBe(mgr.SaveVersions.Count);
        restored.GetMaxVersion().ShouldBe(mgr.GetMaxVersion());
    }

    [Fact]
    public void Bestiary_RoundTrip_NoDataLoss()
    {
        var bestiary = JsonDataLoader.LoadFromResource<BestiaryNpcConfiguration>("bestiaryNpcs.json");

        var json = JsonSerializer.Serialize(bestiary, Options);
        var restored = JsonSerializer.Deserialize<BestiaryNpcConfiguration>(json, Options)!;

        restored.NpcData.Count.ShouldBe(bestiary.NpcData.Count);
        restored.Cat.Count.ShouldBe(bestiary.Cat.Count);
        restored.Dog.Count.ShouldBe(bestiary.Dog.Count);
        restored.Bunny.Count.ShouldBe(bestiary.Bunny.Count);
    }

    [Fact]
    public void MorphBiomes_RoundTrip_NoDataLoss()
    {
        using var stream = JsonDataLoader.GetDataStream("morphBiomes.json");
        var morph = MorphConfiguration.Load(stream);

        var json = JsonSerializer.Serialize(morph, Options);
        using var stream2 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        var restored = MorphConfiguration.Load(stream2);

        restored.Biomes.Count.ShouldBe(morph.Biomes.Count);
        restored.MossTypes.Count.ShouldBe(morph.MossTypes.Count);
    }

    // Specific count validation tests
    [Fact]
    public void Tiles_HaveExpectedMinimumCount()
    {
        var tiles = JsonDataLoader.LoadListFromResource<TileProperty>("tiles.json");
        tiles.Count.ShouldBeGreaterThanOrEqualTo(692);
    }

    [Fact]
    public void Walls_HaveExpectedMinimumCount()
    {
        var walls = JsonDataLoader.LoadListFromResource<WallProperty>("walls.json");
        walls.Count.ShouldBeGreaterThanOrEqualTo(346);
    }

    [Fact]
    public void Items_HaveExpectedMinimumCount()
    {
        var items = JsonDataLoader.LoadListFromResource<ItemProperty>("items.json");
        items.Count.ShouldBeGreaterThanOrEqualTo(5400);
    }

    [Fact]
    public void Npcs_HaveExpectedMinimumCount()
    {
        var npcs = JsonDataLoader.LoadListFromResource<NpcData>("npcs.json");
        npcs.Count.ShouldBeGreaterThanOrEqualTo(60);
    }

    [Fact]
    public void Prefixes_HaveExpectedCount()
    {
        var prefixes = JsonDataLoader.LoadListFromResource<PrefixData>("prefixes.json");
        // Game has more prefixes now (98 in 1.4.5)
        prefixes.Count.ShouldBeGreaterThanOrEqualTo(85);
    }

    [Fact]
    public void Tiles_ContainKnownEntries()
    {
        var tiles = JsonDataLoader.LoadListFromResource<TileProperty>("tiles.json");
        var tileById = tiles.ToDictionary(t => t.Id);

        // Dirt Block
        tileById[0].Name.ShouldBe("Dirt Block");
        tileById[0].IsSolid.ShouldBe(true);

        // Stone Block
        tileById[1].Name.ShouldBe("Stone Block");
        tileById[1].IsStone.ShouldBe(true);

        // Torches (framed) - settings.xml uses plural "Torches"
        tileById[4].Name.ShouldBe("Torches");
        tileById[4].IsFramed.ShouldBe(true);

        // Platforms - settings.xml uses generic "Platforms"
        tileById[19].Name.ShouldBe("Platforms");
        tileById[19].IsPlatform.ShouldBe(true);
    }

    [Fact]
    public void Versions_ContainKnownVersions()
    {
        using var stream = JsonDataLoader.GetDataStream("versions.json");
        var mgr = SaveVersionManager.Load(stream);

        // v1.4.5.2 should have save version 315
        var data = mgr.GetDataForGameVersion("1.4.5.2");
        data.ShouldNotBeNull();
        data.SaveVersion.ShouldBe(315);
    }
}
