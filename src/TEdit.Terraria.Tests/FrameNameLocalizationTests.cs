using Shouldly;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests;

/// <summary>
/// Frame display names in a non-English UI: game items resolve from locale data, TEdit-made labels
/// come from the application's UI resources, and identities (Name/Variety) never change.
/// </summary>
[Collection("SharedState")]
public class FrameNameLocalizationTests : IDisposable
{
    public FrameNameLocalizationTests() => TerrariaDataStore.Reset();

    public void Dispose()
    {
        TerrariaDataStore.Reset();
        LocalizationUiLabels.Resolver = null;
    }

    private static TerrariaDataStore InitJapanese(Func<string, string?>? resolver = null)
    {
        LocalizationUiLabels.Resolver = resolver;
        return TerrariaDataStore.Initialize(locale: "ja-JP");
    }

    private static (TileProperty Tile, FrameProperty Frame) FindFrame(TerrariaDataStore store, Func<FrameProperty, bool> match)
    {
        foreach (var tile in store.Tiles)
            foreach (var frame in tile.Frames ?? [])
                if (match(frame))
                    return (tile, frame);
        throw new InvalidOperationException("frame not found");
    }

    [Fact]
    public void AliasedPlatformName_UsesGameItemTranslation()
    {
        var store = InitJapanese();
        var spookyItem = store.Items.First(i => i.Name == "Spooky Wood Platform");
        var (tile, frame) = FindFrame(store, f => f.Name == "Spooky Platform");

        var display = store.GetLocalizedFrameName(tile, frame);

        display.ShouldStartWith(spookyItem.LocalizedName);
        frame.Name.ShouldBe("Spooky Platform");
    }

    [Fact]
    public void NumericFrameName_FallsBackToTileName()
    {
        var store = InitJapanese();
        var trashCan = store.Tiles.First(t => t.Id == 99);
        var frame = trashCan.Frames!.First(f => f.Name == "99");

        store.GetLocalizedFrameName(trashCan, frame).ShouldStartWith(trashCan.LocalizedName);
    }

    [Fact]
    public void NumericFrameName_EnglishShowsTileName()
    {
        var store = TerrariaDataStore.Initialize();
        var trashCan = store.Tiles.First(t => t.Id == 99);
        var frame = trashCan.Frames!.First(f => f.Name == "99");

        store.GetLocalizedFrameName(trashCan, frame).ShouldStartWith("Trash Can");
    }

    [Fact]
    public void FrameText_TranslatesDescriptorsAndLetteredStates()
    {
        var store = InitJapanese(key => key switch
        {
            "FrameText.Stair Top Landing R" => "STL-R",
            "FrameText.On" => "ON!",
            _ => null
        });

        var (platformTile, stairFrame) = FindFrame(store, f => f.Variety == "Stair Top Landing R");
        store.GetLocalizedFrameName(platformTile, stairFrame).ShouldContain("STL-R");

        var (lanternTile, onFrame) = FindFrame(store, f => f.Variety == "B On");
        store.GetLocalizedFrameName(lanternTile, onFrame).ShouldContain("B ON!");
        onFrame.Variety.ShouldBe("B On");
    }

    [Fact]
    public void FrameText_UntranslatedResourceKeepsComposedEnglishName()
    {
        // Neutral resources return the English text for untranslated keys.
        var store = InitJapanese(key => key.StartsWith(LocalizationUiLabels.FrameTextPrefix, StringComparison.Ordinal)
            ? key[LocalizationUiLabels.FrameTextPrefix.Length..]
            : null);

        var (lanternTile, onFrame) = FindFrame(store, f => f.Variety == "B On");
        store.GetLocalizedFrameName(lanternTile, onFrame).ShouldContain(": B On");
    }

    [Fact]
    public void FrameText_MissingTranslationStaysEnglish()
    {
        var store = InitJapanese(_ => null);

        var (platformTile, stairFrame) = FindFrame(store, f => f.Variety == "Stair Top Landing R");
        store.GetLocalizedFrameName(platformTile, stairFrame).ShouldContain("Stair Top Landing R");
    }

    [Fact]
    public void BossTrophies_ShowOnlyTheTrophyItemName()
    {
        var store = InitJapanese(key => key == "FrameText.Boss Trophy" ? "BT-LABEL" : null);
        var trophyTile = store.Tiles.First(t => t.Id == 240);
        var trophies = trophyTile.Frames!.Where(f => f.Name == "Boss Trophy").ToList();
        trophies.Count.ShouldBeGreaterThan(20);

        foreach (var frame in trophies)
        {
            var item = store.Items.FirstOrDefault(i => i.Name == frame.Variety)
                       ?? store.Items.FirstOrDefault(i => i.Name == frame.Variety + " Trophy");
            if (item == null)
                continue;

            var display = store.GetLocalizedFrameName(trophyTile, frame);
            display.ShouldBe(item.LocalizedName, $"variety \"{frame.Variety}\"");
            display.ShouldNotContain("BT-LABEL");
            frame.Name.ShouldBe("Boss Trophy");
        }
    }

    [Fact]
    public void DirectItemNames_KeepTheAnchorSuffix()
    {
        var store = InitJapanese(key => key == "Anchor.Left" ? "L!" : null);
        var trophyTile = store.Tiles.First(t => t.Id == 240);
        var source = trophyTile.Frames!.First(f => f.Name == "Boss Trophy" && store.Items.Any(i => i.Name == f.Variety + " Trophy"));
        var anchoredTrophy = new FrameProperty { Name = source.Name, Variety = source.Variety, UV = source.UV, Anchor = FrameAnchor.Left };

        var (bannerTile, banner) = FindFrame(store, f => f.Name.EndsWith("Banner", StringComparison.Ordinal)
                                                         && f.Variety != null && store.Items.Any(i => i.Name == f.Variety));
        var anchoredBanner = new FrameProperty { Name = banner.Name, Variety = banner.Variety, UV = banner.UV, Anchor = FrameAnchor.Left };

        store.GetLocalizedFrameName(trophyTile, anchoredTrophy).ShouldEndWith(" [L!]");
        store.GetLocalizedFrameName(bannerTile, anchoredBanner).ShouldEndWith(" [L!]");
    }
}
