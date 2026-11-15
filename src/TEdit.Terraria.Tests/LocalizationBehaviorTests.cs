using System.Text.Json;
using Shouldly;
using TEdit.Common;
using TEdit.Terraria.DataModel;
using TEdit.Terraria.Loaders;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests;

[Collection("SharedState")]
public sealed class LocalizationBehaviorTests : IDisposable
{
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), $"tedit-localization-{Guid.NewGuid():N}");

    public LocalizationBehaviorTests()
    {
        TerrariaDataStore.Reset();
    }

    public void Dispose()
    {
        TerrariaDataStore.Reset();
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }

    [Fact]
    public void ApplyLocalization_KeepsSourceNames_AndDisplaysAndSearchesAltarFramesBilingually()
    {
        var localizationDirectory = Path.Combine(_tempDirectory, "Localization");
        Directory.CreateDirectory(localizationDirectory);
        File.WriteAllText(
            Path.Combine(localizationDirectory, "zh-Hans.json"),
            JsonSerializer.Serialize(new
            {
                items = new Dictionary<string, string> { ["CrimsonAltar"] = "猩红祭坛" },
                tiles = new Dictionary<string, string>
                {
                    ["Air"] = "空气",
                    ["DemonAltar"] = "祭坛"
                },
                frames = new Dictionary<string, Dictionary<string, string>>
                {
                    ["DemonAltar"] = new()
                    {
                        ["Demon Altar"] = "恶魔祭坛",
                        ["Crimson Altar"] = "猩红祭坛"
                    }
                },
                liquids = new Dictionary<string, string> { ["Water"] = "水" },
                wires = new Dictionary<string, string> { ["Red Wire"] = "红色电线" }
            }));

        var demonAltar = new FrameProperty { Name = "Demon Altar" };
        var crimsonAltar = new FrameProperty { Name = "Crimson Altar" };
        var untranslated = new FrameProperty
        {
            Name = "Untranslated Frame",
            Variety = "Variant A",
            Anchor = FrameAnchor.Top
        };
        var tile = new TileProperty
        {
            Id = 26,
            Name = "Altars",
            Key = "DemonAltar",
            Frames = [demonAltar, crimsonAltar, untranslated]
        };
        var item = new ItemProperty { Id = 1, Name = "Crimson Altar", Key = "CrimsonAltar" };

        var store = TerrariaDataStore.InitializeFrom(tiles: [tile], items: [item]);
        store.LoadLocalization("zh-Hans", _tempDirectory);
        var air = store.TileBricks.Single(brick => brick.Id == -1);

        air.Name.ShouldBe("Air");
        air.DisplayName.ShouldBe("空气");
        tile.Name.ShouldBe("Altars");
        tile.DisplayName.ShouldBe("祭坛");
        item.Name.ShouldBe("Crimson Altar");
        item.DisplayName.ShouldBe("猩红祭坛");
        demonAltar.Name.ShouldBe("Demon Altar");
        demonAltar.ToString().ShouldBe("Demon Altar");
        demonAltar.DisplayName.ShouldBe("恶魔祭坛");
        crimsonAltar.Name.ShouldBe("Crimson Altar");
        crimsonAltar.ToString().ShouldBe("Crimson Altar");
        crimsonAltar.DisplayName.ShouldBe("猩红祭坛");
        untranslated.DisplayName.ShouldBe("Untranslated Frame: Variant A [Top]");

        var spriteSheet = new SpriteSheet { Tile = 26, Name = tile.Name, LocalizedName = tile.DisplayName };
        var spriteStyle = new SpriteItem
        {
            Tile = 26,
            Name = crimsonAltar.ToString(),
            LocalizedDisplayName = crimsonAltar.DisplayName
        };
        spriteSheet.Name.ShouldBe("Altars");
        spriteSheet.DisplayName.ShouldBe("祭坛");
        spriteStyle.Name.ShouldBe("Crimson Altar");
        spriteStyle.DisplayName.ShouldBe("猩红祭坛");

        LocalizedNameSearch.Matches("猩红祭坛", crimsonAltar.Name, crimsonAltar.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches("Crimson Altar", crimsonAltar.Name, crimsonAltar.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches("猩红祭坛", spriteStyle.Name, spriteStyle.DisplayName).ShouldBeTrue();
        tile.Frames!.Any(frame => LocalizedNameSearch.Matches(
            "猩红祭坛", frame.Name, frame.DisplayName)).ShouldBeTrue();
    }

    [Fact]
    public void ApplyLocalization_UsesExactItemTranslationsForCompleteFrameLabelsOnly()
    {
        Directory.CreateDirectory(_tempDirectory);
        var demonAltar = new FrameProperty { Name = "Demon Altar" };
        var crimsonAltar = new FrameProperty { Name = "Crimson Altar" };
        var partialMatch = new FrameProperty { Name = "Crimson Altar", Variety = "Special" };
        var tile = new TileProperty
        {
            Id = 26,
            Name = "Altars",
            Key = "DemonAltar",
            Frames = [demonAltar, crimsonAltar, partialMatch]
        };
        var items = new[]
        {
            new ItemProperty { Id = 5532, Name = "Demon Altar", Key = "DemonAltar" },
            new ItemProperty { Id = 5533, Name = "Crimson Altar", Key = "CrimsonAltar" }
        };

        TerrariaDataStore.InitializeFrom(tiles: [tile], items: items.ToList())
            .LoadLocalization("zh-Hans", _tempDirectory);

        demonAltar.Name.ShouldBe("Demon Altar");
        demonAltar.DisplayName.ShouldBe("恶魔祭坛");
        crimsonAltar.Name.ShouldBe("Crimson Altar");
        crimsonAltar.DisplayName.ShouldBe("猩红祭坛");
        partialMatch.DisplayName.ShouldBe("Crimson Altar: Special");
    }

    [Fact]
    public void LoadLocalization_ResolvesReferencesAcrossAllLocalizedCategories()
    {
        var localizationDirectory = Path.Combine(_tempDirectory, "Localization");
        Directory.CreateDirectory(localizationDirectory);
        var frameSource = "Source: Variant [Top]";
        File.WriteAllText(
            Path.Combine(localizationDirectory, "test-locale.json"),
            JsonSerializer.Serialize(new
            {
                items = new Dictionary<string, string> { ["DirtBlock"] = "泥土块" },
                npcs = new Dictionary<string, string> { ["BlueSlime"] = "蓝史莱姆" },
                prefixes = new Dictionary<string, string> { ["Powerful"] = "强力" },
                tiles = new Dictionary<string, string> { ["TestTile"] = "{$ItemName.DirtBlock}" },
                walls = new Dictionary<string, string> { ["TestWall"] = "{$TileName.TestTile}墙" },
                liquids = new Dictionary<string, string> { ["Water"] = "水" },
                wires = new Dictionary<string, string> { ["Red Wire"] = "{$LiquidName.Water}线" },
                frames = new Dictionary<string, Dictionary<string, string>>
                {
                    ["TestTile"] = new()
                    {
                        [frameSource] = "{$TileName.TestTile}：{$NPCName.BlueSlime}（{$PrefixName.Powerful}） {$WallName.TestWall} {$WireName.Red Wire}"
                    }
                }
            }));

        var data = LocalizationLoader.LoadLocalization("test-locale", _tempDirectory);

        data.Tiles["TestTile"].ShouldBe("泥土块");
        data.Walls["TestWall"].ShouldBe("泥土块墙");
        data.Liquids["Water"].ShouldBe("水");
        data.Wires["Red Wire"].ShouldBe("水线");
        data.Frames["TestTile"][frameSource]
            .ShouldBe("泥土块：蓝史莱姆（强力） 泥土块墙 水线");
    }

    [Fact]
    public void LoadLocalization_ExcessivelyDeepReferenceChainFallsBackWithoutUnboundedRecursion()
    {
        var localizationDirectory = Path.Combine(_tempDirectory, "Localization");
        Directory.CreateDirectory(localizationDirectory);
        var items = Enumerable.Range(0, 400)
            .ToDictionary(
                index => $"Chain{index}",
                index => index == 399 ? "终点" : $"{{$ItemName.Chain{index + 1}}}",
                StringComparer.Ordinal);
        File.WriteAllText(
            Path.Combine(localizationDirectory, "test-locale.json"),
            JsonSerializer.Serialize(new
            {
                items,
                tiles = new Dictionary<string, string> { ["DeepTile"] = "Prefix {$ItemName.Chain0} suffix" }
            }));

        var data = LocalizationLoader.LoadLocalization("test-locale", _tempDirectory);

        data.Tiles["DeepTile"].ShouldBe(string.Empty);
    }

    [Fact]
    public void LoadLocalization_AppliesCustomEnglishOverride()
    {
        var localizationDirectory = Path.Combine(_tempDirectory, "Localization");
        Directory.CreateDirectory(localizationDirectory);
        File.WriteAllText(
            Path.Combine(localizationDirectory, "en-US.json"),
            JsonSerializer.Serialize(new
            {
                items = new Dictionary<string, string> { ["Bottle"] = "Custom Bottle Label" }
            }));
        var item = new ItemProperty { Id = 31, Key = "Bottle", Name = "Bottle" };
        var store = TerrariaDataStore.InitializeFrom(items: [item]);

        store.LoadLocalization("en-US", _tempDirectory);

        item.Name.ShouldBe("Bottle");
        item.DisplayName.ShouldBe("Custom Bottle Label");
        store.Localization!.Items["Bottle"].ShouldBe("Custom Bottle Label");
    }

    [Fact]
    public void ApplyLocalization_EmptyFramePlaceholderPreservesExactItemTranslation()
    {
        var localizationDirectory = Path.Combine(_tempDirectory, "Localization");
        Directory.CreateDirectory(localizationDirectory);
        File.WriteAllText(
            Path.Combine(localizationDirectory, "test-locale.json"),
            JsonSerializer.Serialize(new
            {
                items = new Dictionary<string, string> { ["Bottle"] = "ビン" },
                tiles = new Dictionary<string, string> { ["Bottles"] = "" },
                frames = new Dictionary<string, Dictionary<string, string>>
                {
                    ["Bottles"] = new()
                    {
                        ["Bottle"] = "",
                        ["Bottle: Special [Top]"] = ""
                    }
                }
            }));

        var bottle = new FrameProperty { Name = "Bottle" };
        var variant = new FrameProperty { Name = "Bottle", Variety = "Special", Anchor = FrameAnchor.Top };
        var tile = new TileProperty { Id = 13, Key = "Bottles", Name = "Bottles", Frames = [bottle, variant] };
        var item = new ItemProperty { Id = 31, Key = "Bottle", Name = "Bottle" };
        var store = TerrariaDataStore.InitializeFrom(tiles: [tile], items: [item]);

        store.LoadLocalization("test-locale", _tempDirectory);

        bottle.DisplayName.ShouldBe("ビン");
        variant.DisplayName.ShouldBe("Bottle: Special [Top]");
        tile.DisplayName.ShouldBe("Bottles");
        item.Name.ShouldBe("Bottle");
        bottle.ToString().ShouldBe("Bottle");

        store.LoadLocalization("en-US", _tempDirectory);
        bottle.DisplayName.ShouldBe("Bottle");
        item.DisplayName.ShouldBe("Bottle");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LoadLocalization_EmptyReferenceFallsBackAsAWholeLabel(string emptyTranslation)
    {
        var localizationDirectory = Path.Combine(_tempDirectory, "Localization");
        Directory.CreateDirectory(localizationDirectory);
        File.WriteAllText(
            Path.Combine(localizationDirectory, "test-locale.json"),
            JsonSerializer.Serialize(new
            {
                items = new Dictionary<string, string>
                {
                    ["MissingTranslation"] = emptyTranslation,
                    ["Bottle"] = "ビン",
                    ["Deep0"] = "{$ItemName.Deep1}",
                    ["Deep1"] = "{$ItemName.Deep2}",
                    ["Deep2"] = "{$ItemName.Deep3}",
                    ["Deep3"] = "{$ItemName.Deep4}",
                    ["Deep4"] = "{$ItemName.Deep5}",
                    ["Deep5"] = "{$ItemName.Deep6}",
                    ["Deep6"] = "{$ItemName.MissingTranslation}",
                    ["DeepValid0"] = "{$ItemName.DeepValid1}",
                    ["DeepValid1"] = "{$ItemName.DeepValid2}",
                    ["DeepValid2"] = "{$ItemName.DeepValid3}",
                    ["DeepValid3"] = "{$ItemName.DeepValid4}",
                    ["DeepValid4"] = "{$ItemName.DeepValid5}",
                    ["DeepValid5"] = "{$ItemName.DeepValid6}",
                    ["DeepValid6"] = "终点",
                    ["CycleA"] = "{$ItemName.CycleB}",
                    ["CycleB"] = "{$ItemName.CycleA}"
                },
                tiles = new Dictionary<string, string>
                {
                    ["Bottles"] = "Prefix {$ItemName.MissingTranslation} suffix",
                    ["InvalidReference"] = "Prefix {$ItemName.DoesNotExist} suffix",
                    ["DeepReference"] = "Prefix {$ItemName.Deep0} suffix",
                    ["DeepValidReference"] = "Prefix {$ItemName.DeepValid0} suffix",
                    ["RepeatedReference"] = "{$ItemName.Bottle} / {$ItemName.Bottle}",
                    ["CyclicReference"] = "Prefix {$ItemName.CycleA} suffix"
                },
                walls = new Dictionary<string, string> { ["TestWall"] = "{$TileName.Bottles} wall" },
                frames = new Dictionary<string, Dictionary<string, string>>
                {
                    ["Bottles"] = new() { ["Bottle"] = "{$ItemName.Bottle} / {$WallName.TestWall}" }
                }
            }));

        var frame = new FrameProperty { Name = "Bottle" };
        var tile = new TileProperty { Id = 13, Key = "Bottles", Name = "Bottles", Frames = [frame] };
        var item = new ItemProperty { Id = 31, Key = "Bottle", Name = "Bottle" };
        var wall = new WallProperty { Id = 1, Key = "TestWall", Name = "Source Wall" };
        var store = TerrariaDataStore.InitializeFrom(tiles: [tile], items: [item], walls: [wall]);

        store.LoadLocalization("test-locale", _tempDirectory);

        store.Localization!.Tiles["Bottles"].ShouldBe(string.Empty);
        store.Localization.Tiles["InvalidReference"].ShouldBe(string.Empty);
        store.Localization.Tiles["DeepReference"].ShouldBe(string.Empty);
        store.Localization.Tiles["DeepValidReference"].ShouldBe("Prefix 终点 suffix");
        store.Localization.Tiles["RepeatedReference"].ShouldBe("ビン / ビン");
        store.Localization.Tiles["CyclicReference"].ShouldBe(string.Empty);
        store.Localization.Walls["TestWall"].ShouldBe(string.Empty);
        store.Localization.Frames["Bottles"]["Bottle"].ShouldBe(string.Empty);
        tile.DisplayName.ShouldBe("Bottles");
        wall.DisplayName.ShouldBe("Source Wall");
        frame.DisplayName.ShouldBe("ビン");
    }

    [Theory]
    [MemberData(nameof(SupportedLocales))]
    public void EmbeddedLocales_ApplyAvailableNamesAndPreserveSourceNames(string locale)
    {
        var store = TerrariaDataStore.Initialize(locale: locale);
        var resources = LocalizationLoader.LoadLocalization(locale);
        var tile = Assert.Single(store.Tiles, candidate => candidate.Key == "RubyGemspark");
        var item = Assert.Single(store.Items, candidate => candidate.Key == "Bottle");
        var bottle = Assert.Single(store.Tiles, candidate => candidate.Key == "Bottles")
            .Frames!.First(frame => frame.ToString() == "Bottle");
        var tileTranslation = resources.Tiles["RubyGemspark"];
        var bottleTranslation = resources.Items["Bottle"];

        tile.Name.ShouldBe("Ruby Gemspark Block");
        item.Name.ShouldBe("Bottle");
        bottle.ToString().ShouldBe("Bottle");
        tile.DisplayName.ShouldBe(locale == "en-US" || string.IsNullOrWhiteSpace(tileTranslation)
            ? tile.Name : tileTranslation);
        item.DisplayName.ShouldBe(locale == "en-US" || string.IsNullOrWhiteSpace(bottleTranslation)
            ? item.Name : bottleTranslation);
        bottle.DisplayName.ShouldBe(locale == "en-US" || string.IsNullOrWhiteSpace(bottleTranslation)
            ? "Bottle" : bottleTranslation);
        LocalizedNameSearch.Matches(tile.Name, tile.Name, tile.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches(tile.DisplayName, tile.Name, tile.DisplayName).ShouldBeTrue();
    }

    [Fact]
    public void WorldConfigurationLocalizedLookups_UseTheLiveStoreDictionaries()
    {
        WorldConfiguration.Initialize();
        var storeField = typeof(WorldConfiguration).GetField(
            "_store",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var store = Assert.IsType<TerrariaDataStore>(storeField!.GetValue(null));
        var originalLocale = store.CurrentLocale;

        try
        {
            store.LoadLocalization("ja-JP");

            Assert.Same(store.NpcDisplayNameById, WorldConfiguration.NpcDisplayNames);
            Assert.Same(store.PrefixDisplayById, WorldConfiguration.ItemPrefixDisplay);
        }
        finally
        {
            store.LoadLocalization(originalLocale);
        }
    }

    public static IEnumerable<object[]> SupportedLocales()
        => LocalizationLoader.GetSupportedLocales().Select(locale => new object[] { locale });

}
