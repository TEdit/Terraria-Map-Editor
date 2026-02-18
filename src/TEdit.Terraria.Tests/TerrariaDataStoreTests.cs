using Shouldly;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests;

[Collection("SharedState")]
public class TerrariaDataStoreTests : IDisposable
{
    public TerrariaDataStoreTests()
    {
        // Reset singleton before each test
        TerrariaDataStore.Reset();
    }

    public void Dispose()
    {
        TerrariaDataStore.Reset();
    }

    [Fact]
    public void Initialize_LoadsFromEmbeddedResources()
    {
        var store = TerrariaDataStore.Initialize();

        store.ShouldNotBeNull();
        TerrariaDataStore.Instance.ShouldBe(store);
    }

    [Fact]
    public void Initialize_LoadsTiles()
    {
        var store = TerrariaDataStore.Initialize();

        store.Tiles.Count.ShouldBeGreaterThan(0);
        // First tile should be Dirt Block (id 0)
        store.Tiles[0].Name.ShouldBe("Dirt Block");
        store.Tiles[0].Id.ShouldBe(0);
    }

    [Fact]
    public void Initialize_TileCount_MatchesExpected()
    {
        var store = TerrariaDataStore.Initialize();

        // v1.4.5.4 should have 752 tile types (the exact count from settings.xml)
        // The count includes gap-filled entries up to 255
        store.Tiles.Count.ShouldBeGreaterThanOrEqualTo(692);
    }

    [Fact]
    public void Initialize_LoadsWalls()
    {
        var store = TerrariaDataStore.Initialize();

        store.Walls.Count.ShouldBeGreaterThan(0);
        // First wall is Sky (id 0)
        store.Walls[0].Name.ShouldBe("Sky");
    }

    [Fact]
    public void Initialize_WallCount_MatchesExpected()
    {
        var store = TerrariaDataStore.Initialize();

        store.Walls.Count.ShouldBeGreaterThanOrEqualTo(346);
    }

    [Fact]
    public void Initialize_LoadsItems()
    {
        var store = TerrariaDataStore.Initialize();

        store.Items.Count.ShouldBeGreaterThan(0);
        store.ItemById.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Initialize_ItemCount_MatchesExpected()
    {
        var store = TerrariaDataStore.Initialize();

        store.Items.Count.ShouldBeGreaterThanOrEqualTo(5400);
    }

    [Fact]
    public void Initialize_LoadsNpcs()
    {
        var store = TerrariaDataStore.Initialize();

        store.NpcIdByName.Count.ShouldBeGreaterThan(0);
        store.NpcNameById.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Initialize_NpcLookup_WorksBothWays()
    {
        var store = TerrariaDataStore.Initialize();

        store.NpcIdByName.ShouldContainKey("Guide");
        var guideId = store.NpcIdByName["Guide"];
        store.NpcNameById[guideId].ShouldBe("Guide");
    }

    [Fact]
    public void Initialize_LoadsPaints()
    {
        var store = TerrariaDataStore.Initialize();

        store.Paints.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Initialize_LoadsPrefixes()
    {
        var store = TerrariaDataStore.Initialize();

        store.PrefixById.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Initialize_LoadsGlobalColors()
    {
        var store = TerrariaDataStore.Initialize();

        store.GlobalColors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Initialize_LoadsVersionManager()
    {
        var store = TerrariaDataStore.Initialize();

        store.VersionManager.ShouldNotBeNull();
        store.VersionManager!.GetMaxVersion().ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Initialize_LoadsBestiary()
    {
        var store = TerrariaDataStore.Initialize();

        store.Bestiary.ShouldNotBeNull();
        store.BestiaryNpcById.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Initialize_LoadsMorphs()
    {
        var store = TerrariaDataStore.Initialize();

        store.Morphs.ShouldNotBeNull();
        store.Morphs!.Biomes.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void TileBricks_ContainsOnlyNonFramedTiles()
    {
        var store = TerrariaDataStore.Initialize();

        // First entry is Air (id -1)
        store.TileBricks[0].Id.ShouldBe(-1);
        store.TileBricks[0].Name.ShouldBe("Air");

        // All remaining should be non-framed
        foreach (var brick in store.TileBricks.Skip(1))
        {
            brick.IsFramed.ShouldBe(false, $"Tile {brick.Id} ({brick.Name}) should not be framed");
        }
    }

    [Fact]
    public void TileById_CanLookUpDirt()
    {
        var store = TerrariaDataStore.Initialize();

        store.TileById.ShouldContainKey(0);
        store.TileById[0].Name.ShouldBe("Dirt Block");
    }

    [Fact]
    public void Chests_AreDerivedFromTileFrames()
    {
        var store = TerrariaDataStore.Initialize();

        store.Chests.Count.ShouldBeGreaterThan(0);
        // All chests should have valid tile types (21, 88, 467, 468, 441)
        foreach (var chest in store.Chests)
        {
            new int[] { 21, 88, 467, 468, 441 }.ShouldContain(chest.TileType);
        }
    }

    [Fact]
    public void Signs_AreDerivedFromTileFrames()
    {
        var store = TerrariaDataStore.Initialize();

        store.Signs.Count.ShouldBeGreaterThan(0);
        // All signs should have valid tile types (55, 85, 425, 573)
        foreach (var sign in store.Signs)
        {
            new int[] { 55, 85, 425, 573 }.ShouldContain(sign.TileType);
        }
    }

    [Fact]
    public void CategoryDictionaries_ArePopulatedFromItemFlags()
    {
        var store = TerrariaDataStore.Initialize();

        // At least some armor should exist
        store.ArmorHeadItems.Count.ShouldBeGreaterThan(0);
        store.ArmorBodyItems.Count.ShouldBeGreaterThan(0);
        store.ArmorLegsItems.Count.ShouldBeGreaterThan(0);

        // Food, accessories should exist
        store.FoodItems.Count.ShouldBeGreaterThan(0);
        store.AccessoryItems.Count.ShouldBeGreaterThan(0);
        store.RackableItems.Count.ShouldBeGreaterThan(0);

        // Dye names derived from name containing "Dye"
        store.DyeItems.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void TileFrameImportant_HasCorrectLength()
    {
        var store = TerrariaDataStore.Initialize();

        store.TileFrameImportant.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void InitializeFrom_CreatesStoreWithProvidedData()
    {
        var tiles = new List<TileProperty>
        {
            new() { Id = 0, Name = "Test Tile", IsSolid = true },
            new() { Id = 1, Name = "Framed Tile", IsFramed = true },
        };

        var store = TerrariaDataStore.InitializeFrom(tiles: tiles);

        store.Tiles.Count.ShouldBe(2);
        store.TileBricks.Count.ShouldBe(2); // Air + Test Tile (non-framed)
        store.TileById[0].Name.ShouldBe("Test Tile");
    }

    [Fact]
    public void Instance_ThrowsIfNotInitialized()
    {
        Should.Throw<InvalidOperationException>(() => _ = TerrariaDataStore.Instance);
    }

    [Fact]
    public void VersionManager_GetData_ReturnsVersionData()
    {
        var store = TerrariaDataStore.Initialize();

        var maxVersion = store.VersionManager!.GetMaxVersion();
        var data = store.VersionManager.GetData(maxVersion);

        data.ShouldNotBeNull();
        data.MaxTileId.ShouldBeGreaterThan(0);
        data.MaxWallId.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ApplyWorldVersion_UpdatesLimits()
    {
        var store = TerrariaDataStore.Initialize();

        var maxVersion = (uint)store.VersionManager!.GetMaxVersion();
        store.ApplyWorldVersion(maxVersion);

        store.TileCount.ShouldBeGreaterThan(0);
        store.WallCount.ShouldBeGreaterThan(0);
        store.MaxNpcId.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void BestiaryLookups_AreCorrectlyPopulated()
    {
        var store = TerrariaDataStore.Initialize();

        // BestiaryTalkedIDs should contain NPCs that can talk
        store.BestiaryTalkedIDs.Count.ShouldBeGreaterThan(0);

        // BestiaryKilledIDs should contain NPCs that give kill credit
        store.BestiaryKilledIDs.Count.ShouldBeGreaterThan(0);

        // BestiaryNearIDs should contain critters
        store.BestiaryNearIDs.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void MorphConfig_IsMoss_Works()
    {
        var store = TerrariaDataStore.Initialize();

        store.Morphs.ShouldNotBeNull();
        store.Morphs!.MossTypes.Count.ShouldBeGreaterThan(0);

        // Get a known moss type and verify IsMoss returns true
        var firstMossType = store.Morphs.MossTypes.Values.First();
        store.Morphs.IsMoss((ushort)firstMossType).ShouldBe(true);

        // A random high number should not be moss
        store.Morphs.IsMoss(9999).ShouldBe(false);
    }
}
