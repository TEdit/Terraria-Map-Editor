using Shouldly;

namespace TEdit.Terraria.Tests;

/// <summary>
/// Tests that verify WorldConfiguration (loaded via bridge from TerrariaDataStore)
/// contains the same data as TerrariaDataStore itself.
/// </summary>
[Collection("SharedState")]
public class BridgeParityTests : IDisposable
{
    private readonly TerrariaDataStore _store;

    public BridgeParityTests()
    {
        // Reset both stores so we start clean
        TerrariaDataStore.Reset();
        WorldConfiguration.Reset();

        // Initialize WorldConfiguration (which also initializes TerrariaDataStore internally)
        WorldConfiguration.Initialize();
        _store = TerrariaDataStore.Initialize();
    }

    public void Dispose()
    {
        // Reset state so other tests are not affected
        TerrariaDataStore.Reset();
        WorldConfiguration.Reset();
    }

    [Fact]
    public void TileProperties_Count_MatchesOrExceedsStore()
    {
        // WorldConfiguration pads up to 255 with placeholders, so it may have more
        WorldConfiguration.TileProperties.Count.ShouldBeGreaterThanOrEqualTo(_store.Tiles.Count);
    }

    [Fact]
    public void TileProperties_DataMatches()
    {
        foreach (var tileData in _store.Tiles)
        {
            var prop = WorldConfiguration.TileProperties[tileData.Id];
            prop.Id.ShouldBe(tileData.Id, $"Tile {tileData.Id}");
            prop.Name.ShouldBe(tileData.Name, $"Tile {tileData.Id} name");
            prop.IsFramed.ShouldBe(tileData.IsFramed, $"Tile {tileData.Id} IsFramed");
            prop.IsSolid.ShouldBe(tileData.IsSolid, $"Tile {tileData.Id} IsSolid");
            prop.IsSolidTop.ShouldBe(tileData.IsSolidTop, $"Tile {tileData.Id} IsSolidTop");
        }
    }

    [Fact]
    public void TileBricks_ContainsOnlyNonFramedTiles()
    {
        // TileBricks should contain Air brick + non-framed tiles
        var nonFramedCount = _store.Tiles.Count(t => !t.IsFramed);
        // +1 for Air brick
        WorldConfiguration.TileBricks.Count.ShouldBe(nonFramedCount + 1);
    }

    [Fact]
    public void WallProperties_CountMatches()
    {
        WorldConfiguration.WallProperties.Count.ShouldBe(_store.Walls.Count);
    }

    [Fact]
    public void WallProperties_DataMatches()
    {
        for (int i = 0; i < _store.Walls.Count; i++)
        {
            var wallData = _store.Walls[i];
            var prop = WorldConfiguration.WallProperties[i];
            prop.Id.ShouldBe(wallData.Id, $"Wall {wallData.Id}");
            prop.Name.ShouldBe(wallData.Name, $"Wall {wallData.Id} name");
        }
    }

    [Fact]
    public void ItemProperties_CountMatches()
    {
        WorldConfiguration.ItemProperties.Count.ShouldBe(_store.Items.Count);
    }

    [Fact]
    public void ItemProperties_DataMatches()
    {
        for (int i = 0; i < _store.Items.Count; i++)
        {
            var itemData = _store.Items[i];
            var prop = WorldConfiguration.ItemProperties[i];
            prop.Id.ShouldBe(itemData.Id, $"Item index {i}");
            prop.Name.ShouldBe(itemData.Name, $"Item {itemData.Id} name");
        }
    }

    [Fact]
    public void PaintProperties_CountMatches()
    {
        WorldConfiguration.PaintProperties.Count.ShouldBe(_store.Paints.Count);
    }

    [Fact]
    public void NpcIds_CountMatches()
    {
        WorldConfiguration.NpcIds.Count.ShouldBe(_store.NpcIdByName.Count);
    }

    [Fact]
    public void NpcIds_DataMatches()
    {
        foreach (var kv in _store.NpcIdByName)
        {
            WorldConfiguration.NpcIds.ShouldContainKey(kv.Key, $"NPC '{kv.Key}'");
            WorldConfiguration.NpcIds[kv.Key].ShouldBe(kv.Value, $"NPC '{kv.Key}' id");
        }
    }

    [Fact]
    public void NpcNames_CountMatches()
    {
        WorldConfiguration.NpcNames.Count.ShouldBe(_store.NpcNameById.Count);
    }

    [Fact]
    public void NpcFrames_CountMatches()
    {
        WorldConfiguration.NpcFrames.Count.ShouldBe(_store.NpcFrames.Count);
    }

    [Fact]
    public void Prefixes_CountMatches()
    {
        WorldConfiguration.ItemPrefix.Count.ShouldBe(_store.PrefixById.Count);
    }

    [Fact]
    public void GlobalColors_CountMatches()
    {
        WorldConfiguration.GlobalColors.Count.ShouldBe(_store.GlobalColors.Count);
    }

    [Fact]
    public void GlobalColors_DataMatches()
    {
        foreach (var kv in _store.GlobalColors)
        {
            WorldConfiguration.GlobalColors.ShouldContainKey(kv.Key, $"Color '{kv.Key}'");
            WorldConfiguration.GlobalColors[kv.Key].ShouldBe(kv.Value, $"Color '{kv.Key}'");
        }
    }

    [Fact]
    public void SaveConfiguration_IsPopulated()
    {
        WorldConfiguration.SaveConfiguration.ShouldNotBeNull();
        WorldConfiguration.SaveConfiguration.SaveVersions.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void SaveConfiguration_VersionCountMatches()
    {
        _store.VersionManager.ShouldNotBeNull();
        WorldConfiguration.SaveConfiguration.SaveVersions.Count
            .ShouldBe(_store.VersionManager!.SaveVersions.Count);
    }

    [Fact]
    public void BestiaryData_IsPopulated()
    {
        WorldConfiguration.BestiaryData.ShouldNotBeNull();
        WorldConfiguration.BestiaryData.NpcData.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void BestiaryData_NpcCountMatches()
    {
        _store.Bestiary.ShouldNotBeNull();
        // NpcData is a dict keyed by BestiaryId, so duplicates collapse.
        // Compare against the store's dict (BestiaryNpcByBestiaryId), not the raw list.
        WorldConfiguration.BestiaryData.NpcData.Count
            .ShouldBe(_store.BestiaryNpcByBestiaryId.Count);
    }

    [Fact]
    public void MorphSettings_IsPopulated()
    {
        WorldConfiguration.MorphSettings.ShouldNotBeNull();
        WorldConfiguration.MorphSettings.Biomes.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void MorphSettings_BiomeCountMatches()
    {
        _store.Morphs.ShouldNotBeNull();
        WorldConfiguration.MorphSettings.Biomes.Count
            .ShouldBe(_store.Morphs!.Biomes.Count);
    }

    [Fact]
    public void MorphSettings_MossTypesCountMatches()
    {
        _store.Morphs.ShouldNotBeNull();
        WorldConfiguration.MorphSettings.MossTypes.Count
            .ShouldBe(_store.Morphs!.MossTypes.Count);
    }

    [Fact]
    public void CategoryDictionaries_ArePopulated()
    {
        // Verify the category dictionaries have been populated from items
        // These should have at least some entries if items have the right flags
        var items = _store.Items;

        var expectedHeadCount = items.Count(i => i.Head >= 0);
        var expectedBodyCount = items.Count(i => i.Body >= 0);
        var expectedLegsCount = items.Count(i => i.Legs >= 0);
        var expectedFoodCount = items.Count(i => i.IsFood);
        var expectedRackableCount = items.Count(i => i.IsRackable);

        WorldConfiguration.ArmorHeadNames.Count.ShouldBe(expectedHeadCount);
        WorldConfiguration.ArmorBodyNames.Count.ShouldBe(expectedBodyCount);
        WorldConfiguration.ArmorLegsNames.Count.ShouldBe(expectedLegsCount);
        WorldConfiguration.FoodNames.Count.ShouldBe(expectedFoodCount);
        WorldConfiguration.Rackable.Count.ShouldBe(expectedRackableCount);
    }

    [Fact]
    public void ChestProperties_CountMatches()
    {
        WorldConfiguration.ChestProperties.Count.ShouldBe(_store.Chests.Count);
    }

    [Fact]
    public void SignProperties_CountMatches()
    {
        WorldConfiguration.SignProperties.Count.ShouldBe(_store.Signs.Count);
    }
}
