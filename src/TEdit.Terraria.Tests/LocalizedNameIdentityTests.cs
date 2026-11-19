using Shouldly;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests;

/// <summary>
/// Guards the identity/display split: under a non-English locale, Name stays the English
/// identity (used by saves, scripts and lookups) and only LocalizedName changes.
/// </summary>
[Collection("SharedState")]
public class LocalizedNameIdentityTests : IDisposable
{
    public LocalizedNameIdentityTests() => TerrariaDataStore.Reset();

    public void Dispose()
    {
        TerrariaDataStore.Reset();
        LocalizationUiLabels.Resolver = null;
    }

    [Fact]
    public void NonEnglishLocale_KeepsEnglishItemIdentity()
    {
        var store = TerrariaDataStore.Initialize(locale: "ja-JP");

        var pickaxe = store.ItemById[1];
        pickaxe.Name.ShouldBe("Iron Pickaxe");
        pickaxe.LocalizedName.ShouldNotBe(pickaxe.Name);
        pickaxe.LocalizedName.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void NonEnglishLocale_KeepsEnglishNpcLookups()
    {
        var store = TerrariaDataStore.Initialize(locale: "ja-JP");

        store.NpcIdByName.ShouldContainKey("Guide");
        store.NpcNameById[store.NpcIdByName["Guide"]].ShouldBe("Guide");
    }

    [Fact]
    public void NonEnglishLocale_KeepsEnglishTileAndWallIdentity()
    {
        var store = TerrariaDataStore.Initialize(locale: "ja-JP");

        store.Tiles.First(t => t.Id == 0).Name.ShouldBe("Dirt Block");
        store.Walls.First(w => w.Id == 0).Name.ShouldBe("Sky");
    }

    [Theory]
    [InlineData(2)]   // Grass
    [InlineData(3)]   // Forest short plants
    [InlineData(19)]  // Platforms
    [InlineData(82)]  // Herbs
    [InlineData(104)] // Clocks
    public void CategoryTiles_DoNotTakeAPlacingItemName(int tileId)
    {
        var store = TerrariaDataStore.Initialize(locale: "ja-JP");
        var tile = store.Tiles.First(t => t.Id == tileId);

        store.TileItemLookup.TryGetValue(tileId, out var placingItemId).ShouldBeTrue();
        tile.LocalizedName.ShouldNotBe(store.ItemById[placingItemId].LocalizedName);
    }

    [Fact]
    public void UiLabels_SupplyWallMarkersAndAnchors()
    {
        LocalizationUiLabels.Resolver = key => key switch
        {
            LocalizationUiLabels.WallNaturalFormat => "{0} [N]",
            LocalizationUiLabels.WallUnusedFormat => "{0} [U]",
            LocalizationUiLabels.WallSky => "SKY",
            "Anchor.Left" => "LEFT",
            _ => null
        };
        var store = TerrariaDataStore.Initialize(locale: "ja-JP");

        store.Walls.First(w => w.Id == 0).LocalizedName.ShouldBe("SKY");
        store.GetLocalizedAnchor(FrameAnchor.Left).ShouldBe("LEFT");
        store.GetLocalizedAnchor(FrameAnchor.Right).ShouldBe("Right");
        store.Walls.Where(w => w.Name.EndsWith(" (natural)", StringComparison.Ordinal))
            .Any(w => w.LocalizedName.EndsWith(" [N]", StringComparison.Ordinal))
            .ShouldBeTrue();
    }

    [Fact]
    public void UiLabels_BadFormatFallsBackToEnglishName()
    {
        LocalizationUiLabels.Resolver = key => key switch
        {
            LocalizationUiLabels.WallNaturalFormat => "{1} broken",
            LocalizationUiLabels.WallUnusedFormat => "{0}",
            _ => null
        };
        var store = TerrariaDataStore.Initialize(locale: "ja-JP");

        store.Walls.Where(w => w.Name.EndsWith(" (natural)", StringComparison.Ordinal))
            .ShouldAllBe(w => !w.LocalizedName.Contains("broken"));
    }

    [Fact]
    public void AnglerQuests_MatchGameQuestItems()
    {
        var store = TerrariaDataStore.Initialize();

        store.AnglerQuests.Count.ShouldBe(41);
        store.AnglerQuests[0].ItemId.ShouldBe(2450);
        store.AnglerQuests[39].ItemId.ShouldBe(4393);
        store.AnglerQuests[40].ItemId.ShouldBe(4394);
        store.AnglerQuests.Select(q => q.QuestId).ShouldBe(Enumerable.Range(0, 41));
    }

    [Fact]
    public void XmasDecorations_MatchGameItemOffsets()
    {
        var store = TerrariaDataStore.Initialize();

        store.XmasStarToppers.Count.ShouldBe(4);
        store.XmasGarlands.Count.ShouldBe(6);
        store.XmasBulbs.Count.ShouldBe(11);
        store.XmasLights.Count.ShouldBe(11);
        store.XmasStarToppers[0].ItemId.ShouldBe(1874);
        store.XmasGarlands[0].ItemId.ShouldBe(1878);
        store.XmasBulbs[0].ItemId.ShouldBe(1884);
        store.XmasLights[0].ItemId.ShouldBe(1895);
    }

    [Fact]
    public void Paints_KeepLegacyIlluminantPaint()
    {
        var store = TerrariaDataStore.Initialize();

        store.Paints.ShouldContain(p => p.Id == 31);
    }

    [Fact]
    public void EmbeddedLocalization_HasNoSeparateTranslationLayer()
    {
        var names = typeof(TerrariaDataStore).Assembly.GetManifestResourceNames();

        names.ShouldContain(n => n.EndsWith("Localization.ja-JP.json", StringComparison.Ordinal));
        names.ShouldNotContain(n => n.Contains("supplementalNames", StringComparison.OrdinalIgnoreCase));
        names.ShouldNotContain(n => n.Contains("LocalizationOverrides", StringComparison.OrdinalIgnoreCase));
    }
}
