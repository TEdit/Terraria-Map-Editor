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

    private string LocalizationDirectory => Path.Combine(_tempDirectory, "Localization");

    private void WriteOfficial(string locale, object data)
        => WriteLocalizationFile($"{locale}.json", data);

    private void WriteOverrides(string locale, object data)
        => WriteLocalizationFile($"LocalizationOverrides.{locale}.json", data);

    private void WriteSupplemental(string locale, Dictionary<string, string> data)
        => WriteLocalizationFile($"supplementalNames.{locale}.json", data);

    private void WriteLocalizationFile(string fileName, object data)
    {
        Directory.CreateDirectory(LocalizationDirectory);
        File.WriteAllText(Path.Combine(LocalizationDirectory, fileName), JsonSerializer.Serialize(data));
    }

    private void DeleteOverrides(string locale)
    {
        var path = Path.Combine(LocalizationDirectory, $"LocalizationOverrides.{locale}.json");
        if (File.Exists(path))
            File.Delete(path);
    }

    [Fact]
    public void ApplyLocalization_KeepsSourceNames_AndDisplaysAndSearchesAltarFramesBilingually()
    {
        WriteOfficial("zh-Hans", new
        {
            items = new Dictionary<string, string>
            {
                ["DemonAltar"] = "恶魔祭坛",
                ["CrimsonAltar"] = "猩红祭坛"
            },
            tiles = new Dictionary<string, string>
            {
                ["Air"] = "空气",
                ["DemonAltar"] = "祭坛"
            },
            liquids = new Dictionary<string, string> { ["Water"] = "水" },
            wires = new Dictionary<string, string> { ["Red Wire"] = "红色电线" }
        });
        WriteOverrides("zh-Hans", new
        {
            frames = new Dictionary<string, Dictionary<string, string>>
            {
                ["DemonAltar"] = new()
                {
                    ["Demon Altar"] = "恶魔祭坛",
                    ["Crimson Altar"] = "猩红祭坛"
                }
            }
        });

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
        WriteOfficial("zh-Hans", new
        {
            items = new Dictionary<string, string>
            {
                ["DemonAltar"] = "恶魔祭坛",
                ["CrimsonAltar"] = "猩红祭坛"
            }
        });

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
        var frameSource = "Source: Variant [Top]";
        WriteOfficial("test-locale", new
        {
            items = new Dictionary<string, string> { ["DirtBlock"] = "泥土块" },
            npcs = new Dictionary<string, string> { ["BlueSlime"] = "蓝史莱姆" },
            prefixes = new Dictionary<string, string> { ["Powerful"] = "强力" },
            tiles = new Dictionary<string, string> { ["TestTile"] = "{$ItemName.DirtBlock}" },
            walls = new Dictionary<string, string> { ["TestWall"] = "{$TileName.TestTile}墙" },
            liquids = new Dictionary<string, string> { ["Water"] = "水" },
            wires = new Dictionary<string, string> { ["Red Wire"] = "{$LiquidName.Water}线" }
        });
        WriteOverrides("test-locale", new
        {
            frames = new Dictionary<string, Dictionary<string, string>>
            {
                ["TestTile"] = new()
                {
                    [frameSource] = "{$TileName.TestTile}：{$NPCName.BlueSlime}（{$PrefixName.Powerful}） {$WallName.TestWall} {$WireName.Red Wire}"
                }
            }
        });

        var data = LocalizationLoader.LoadLocalization("test-locale", _tempDirectory);

        data.Tiles["TestTile"].ShouldBe("泥土块");
        data.Walls["TestWall"].ShouldBe("泥土块墙");
        data.Liquids["Water"].ShouldBe("水");
        data.Wires["Red Wire"].ShouldBe("水线");
        data.Frames!["TestTile"][frameSource]
            .ShouldBe("泥土块：蓝史莱姆（强力） 泥土块墙 水线");
    }

    [Fact]
    public void LoadLocalization_ExcessivelyDeepReferenceChainFallsBackWithoutUnboundedRecursion()
    {
        var items = Enumerable.Range(0, 400)
            .ToDictionary(
                index => $"Chain{index}",
                index => index == 399 ? "终点" : $"{{$ItemName.Chain{index + 1}}}",
                StringComparer.Ordinal);
        WriteOfficial("test-locale", new
        {
            items,
            tiles = new Dictionary<string, string> { ["DeepTile"] = "Prefix {$ItemName.Chain0} suffix" }
        });

        var data = LocalizationLoader.LoadLocalization("test-locale", _tempDirectory);

        data.Tiles["DeepTile"].ShouldBe(string.Empty);
    }

    [Fact]
    public void LoadLocalization_AppliesCustomEnglishOverride()
    {
        WriteOfficial("en-US", new
        {
            items = new Dictionary<string, string> { ["Bottle"] = "Custom Bottle Label" }
        });
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
        WriteOfficial("test-locale", new
        {
            items = new Dictionary<string, string> { ["Bottle"] = "ビン" },
            tiles = new Dictionary<string, string> { ["Bottles"] = "" }
        });
        WriteOverrides("test-locale", new
        {
            frames = new Dictionary<string, Dictionary<string, string>>
            {
                ["Bottles"] = new()
                {
                    ["Bottle"] = "",
                    ["Bottle: Special [Top]"] = ""
                }
            }
        });

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
        WriteOfficial("test-locale", new
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
            walls = new Dictionary<string, string> { ["TestWall"] = "{$TileName.Bottles} wall" }
        });
        WriteOverrides("test-locale", new
        {
            frames = new Dictionary<string, Dictionary<string, string>>
            {
                ["Bottles"] = new() { ["Bottle"] = "{$ItemName.Bottle} / {$WallName.TestWall}" }
            }
        });

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
        store.Localization.Frames!["Bottles"]["Bottle"].ShouldBe(string.Empty);
        tile.DisplayName.ShouldBe("Bottles");
        wall.DisplayName.ShouldBe("Source Wall");
        frame.DisplayName.ShouldBe("ビン");
    }

    [Fact]
    public void ApplyLocalization_PlanteraBulb_UsesOverrideForTileAndNumericFrame()
    {
        // The official tile-name derivation has no item match for Plantera's Bulb,
        // so the official value is empty. The override supplies the verified Wiki name.
        LocalizationLoader.LoadOfficialLocalization("zh-Hans").Tiles["PlanteraBulb"].ShouldBeEmpty();
        WriteOfficial("zh-Hans", new
        {
            tiles = new Dictionary<string, string> { ["PlanteraBulb"] = "" }
        });
        WriteOverrides("zh-Hans", new
        {
            tiles = new Dictionary<string, string> { ["PlanteraBulb"] = "世纪之花球茎" }
        });

        var frame = new FrameProperty { Name = "238" };
        var tile = new TileProperty
        {
            Id = 238,
            Key = "PlanteraBulb",
            Name = "Plantera's Bulb",
            Frames = [frame]
        };
        var store = TerrariaDataStore.InitializeFrom(tiles: [tile]);

        store.LoadLocalization("zh-Hans", _tempDirectory);

        tile.Name.ShouldBe("Plantera's Bulb");
        tile.DisplayName.ShouldBe("世纪之花球茎");
        frame.Name.ShouldBe("238");
        frame.ToString().ShouldBe("238");
        frame.DisplayName.ShouldBe("世纪之花球茎");

        LocalizedNameSearch.Matches("世纪之花球茎", tile.Name, tile.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches("Plantera's Bulb", tile.Name, tile.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches("世纪之花球茎", frame.Name, frame.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches("238", frame.Name, frame.DisplayName).ShouldBeTrue();

        store.LoadLocalization("en-US", _tempDirectory);
        tile.DisplayName.ShouldBe("Plantera's Bulb");
        frame.DisplayName.ShouldBe("238");
    }

    [Fact]
    public void GetLocalizedSpriteStyle_UsesWallTranslationsAndSourceNames()
    {
        WriteOfficial("test-locale", new
        {
            walls = new Dictionary<string, string> { ["TestWall"] = "テスト壁" }
        });
        var wall = new WallProperty { Id = 1, Key = "TestWall", Name = "Test Wall" };
        var store = TerrariaDataStore.InitializeFrom(walls: [wall]);

        store.LoadLocalization("test-locale", _tempDirectory);

        store.GetLocalizedSpriteStyle("TestWall").ShouldBe("テスト壁");
        store.GetLocalizedSpriteStyle("Test Wall").ShouldBe("テスト壁");
        store.GetLocalizedSpriteStyle("Unknown Style").ShouldBe("Unknown Style");
    }

    [Fact]
    public void EmbeddedZhHans_DisplaysVerifiedNamesAndSearchesBilingually()
    {
        var store = TerrariaDataStore.Initialize(locale: "zh-Hans");

        var planteraBulb = store.Tiles.Single(tile => tile.Key == "PlanteraBulb");
        planteraBulb.DisplayName.ShouldBe("世纪之花球茎");

        var altars = store.Tiles.Single(tile => tile.Key == "DemonAltar");
        altars.DisplayName.ShouldBe("祭坛");
        var demonAltar = altars.Frames!.Single(frame => frame.ToString() == "Demon Altar");
        var crimsonAltar = altars.Frames!.Single(frame => frame.ToString() == "Crimson Altar");
        demonAltar.DisplayName.ShouldBe("恶魔祭坛");
        crimsonAltar.DisplayName.ShouldBe("猩红祭坛");

        LocalizedNameSearch.Matches("世纪之花球茎", planteraBulb.Name, planteraBulb.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches("Plantera's Bulb", planteraBulb.Name, planteraBulb.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches("猩红祭坛", crimsonAltar.Name, crimsonAltar.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches("Crimson Altar", crimsonAltar.Name, crimsonAltar.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches("恶魔祭坛", demonAltar.Name, demonAltar.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches("Demon Altar", demonAltar.Name, demonAltar.DisplayName).ShouldBeTrue();
    }

    [Fact]
    public void Overrides_BlankOrInvalidValuesNeverReplaceValidOfficialNames()
    {
        WriteOfficial("test-locale", new
        {
            items = new Dictionary<string, string> { ["Bottle"] = "ビン" },
            tiles = new Dictionary<string, string> { ["Bottles"] = "瓶子" },
            walls = new Dictionary<string, string> { ["TestWall"] = "試験壁" }
        });
        WriteOverrides("test-locale", new
        {
            items = new Dictionary<string, string>
            {
                ["Bottle"] = "   ",
                ["DirtBlock"] = "泥土块"
            },
            tiles = new Dictionary<string, string>
            {
                ["Bottles"] = "",
                ["Invalid"] = "{$ItemName.DoesNotExist}"
            },
            walls = new Dictionary<string, string>
            {
                ["TestWall"] = "{$ItemName.DoesNotExist}"
            },
            frames = new Dictionary<string, Dictionary<string, string>>
            {
                ["Bottles"] = new() { ["Bottle"] = "{$ItemName.DoesNotExist}" }
            }
        });

        var bottle = new FrameProperty { Name = "Bottle" };
        var tile = new TileProperty { Id = 13, Key = "Bottles", Name = "Bottles", Frames = [bottle] };
        var item = new ItemProperty { Id = 31, Key = "Bottle", Name = "Bottle" };
        var wall = new WallProperty { Id = 1, Key = "TestWall", Name = "Test Wall" };
        var store = TerrariaDataStore.InitializeFrom(tiles: [tile], items: [item], walls: [wall]);

        store.LoadLocalization("test-locale", _tempDirectory);

        item.DisplayName.ShouldBe("ビン");
        tile.DisplayName.ShouldBe("瓶子");
        wall.DisplayName.ShouldBe("試験壁");
        bottle.DisplayName.ShouldBe("ビン");
        store.Localization!.Items["DirtBlock"].ShouldBe("泥土块");
        store.Localization.OverriddenKeys.ShouldContain("items.DirtBlock");
        store.Localization.OverriddenKeys.ShouldNotContain("items.Bottle");
        store.Localization.OverriddenKeys.ShouldNotContain("tiles.Bottles");
        store.Localization.OverriddenKeys.ShouldNotContain("walls.TestWall");
        store.Localization.OverriddenKeys.ShouldNotContain("frames.Bottles.Bottle");
    }

    [Fact]
    public void Overrides_ResolveReferencesAgainstMergedOfficialData()
    {
        WriteOfficial("test-locale", new
        {
            items = new Dictionary<string, string>
            {
                ["Base"] = "官方",
                ["Referrer"] = "前缀 {$ItemName.Base} 后缀"
            },
            tiles = new Dictionary<string, string>
            {
                ["ReferencingTile"] = "{$ItemName.Base} 方块"
            }
        });
        WriteOverrides("test-locale", new
        {
            items = new Dictionary<string, string>
            {
                ["Base"] = "修正",
                ["OverrideReferrer"] = "{$ItemName.Base} 覆盖"
            }
        });

        var tile = new TileProperty { Id = 1, Key = "ReferencingTile", Name = "Referencing Tile" };
        var item = new ItemProperty { Id = 2, Key = "Base", Name = "Base" };
        var store = TerrariaDataStore.InitializeFrom(tiles: [tile], items: [item]);

        store.LoadLocalization("test-locale", _tempDirectory);

        store.Localization!.Items["Base"].ShouldBe("修正");
        store.Localization.Items["Referrer"].ShouldBe("前缀 修正 后缀");
        store.Localization.Items["OverrideReferrer"].ShouldBe("修正 覆盖");
        store.Localization.Tiles["ReferencingTile"].ShouldBe("修正 方块");
        item.DisplayName.ShouldBe("修正");
        tile.DisplayName.ShouldBe("修正 方块");
    }

    [Fact]
    public void Overrides_RemovingTableRestoresOfficialTranslations()
    {
        // zh-Hant has no bundled override table, so this fixture can verify that
        // removing the overrides resource restores the official translation.
        WriteOfficial("zh-Hant", new
        {
            tiles = new Dictionary<string, string> { ["PlanteraBulb"] = "世紀之花燈泡" }
        });
        WriteOverrides("zh-Hant", new
        {
            tiles = new Dictionary<string, string> { ["PlanteraBulb"] = "世紀之花球莖" }
        });

        var tile = new TileProperty { Id = 238, Key = "PlanteraBulb", Name = "Plantera's Bulb" };
        var store = TerrariaDataStore.InitializeFrom(tiles: [tile]);

        store.LoadLocalization("zh-Hant", _tempDirectory);
        tile.DisplayName.ShouldBe("世紀之花球莖");

        DeleteOverrides("zh-Hant");
        store.LoadLocalization("zh-Hant", _tempDirectory);
        tile.DisplayName.ShouldBe("世紀之花燈泡");

        WriteOverrides("zh-Hant", new
        {
            tiles = new Dictionary<string, string> { ["PlanteraBulb"] = "   " }
        });
        store.LoadLocalization("zh-Hant", _tempDirectory);
        tile.DisplayName.ShouldBe("世紀之花燈泡");
    }

    [Fact]
    public void Overrides_SurviveOfficialResourceRegeneration()
    {
        WriteOfficial("zh-Hans", new
        {
            tiles = new Dictionary<string, string> { ["PlanteraBulb"] = "旧官方译名" }
        });
        WriteOverrides("zh-Hans", new
        {
            tiles = new Dictionary<string, string> { ["PlanteraBulb"] = "世纪之花球茎" }
        });

        var tile = new TileProperty { Id = 238, Key = "PlanteraBulb", Name = "Plantera's Bulb" };
        var store = TerrariaDataStore.InitializeFrom(tiles: [tile]);
        store.LoadLocalization("zh-Hans", _tempDirectory);
        tile.DisplayName.ShouldBe("世纪之花球茎");

        // Simulate GenerateLocalizationFiles rewriting the official resource.
        WriteOfficial("zh-Hans", new
        {
            tiles = new Dictionary<string, string> { ["PlanteraBulb"] = "新官方译名" }
        });
        store.LoadLocalization("zh-Hans", _tempDirectory);

        store.Localization!.Tiles["PlanteraBulb"].ShouldBe("世纪之花球茎");
        tile.DisplayName.ShouldBe("世纪之花球茎");
    }

    [Fact]
    public void ApplyLocalization_ComposesFramesFromItemsTilesAndSupplementalNames()
    {
        WriteOfficial("zh-Hans", new
        {
            items = new Dictionary<string, string>
            {
                ["BlueSlimeBanner"] = "蓝史莱姆旗",
                ["EyeofCthulhuTrophy"] = "克苏鲁之眼纪念章",
                ["Torch"] = "火把",
                ["BlankWall"] = "空白墙"
            },
            tiles = new Dictionary<string, string>
            {
                ["Banners"] = "旗帜",
                ["Painting3X3"] = "墙面挂饰（3x3）",
                ["Torches"] = "火把",
                ["TileA"] = "方块甲",
                ["TileB"] = "方块乙"
            }
        });
        WriteOverrides("zh-Hans", new
        {
            frames = new Dictionary<string, Dictionary<string, string>>
            {
                ["TileA"] = new() { ["Shared Frame"] = "甲专属" },
                ["TileB"] = new() { ["Shared Frame"] = "乙专属" }
            }
        });
        WriteSupplemental("zh-Hans", new Dictionary<string, string>
        {
            ["On"] = "开启",
            ["Off"] = "关闭",
            ["Anchor.Bottom"] = "底部",
            ["Anchor.Top"] = "顶部"
        });

        var bannerFrame = new FrameProperty { Name = "Craftable Banner", Variety = "Blue Slime Banner" };
        var trophyFrame = new FrameProperty { Name = "Boss Trophy", Variety = "Eye of Cthulhu" };
        var torchFrame = new FrameProperty { Name = "Torch", Variety = "On", Anchor = FrameAnchor.Bottom };
        var sharedA = new FrameProperty { Name = "Shared Frame" };
        var sharedB = new FrameProperty { Name = "Shared Frame" };
        var untranslated = new FrameProperty { Name = "Unknown Frame", Variety = "Variant", Anchor = FrameAnchor.Top };

        var bannerTile = new TileProperty { Id = 91, Key = "Banners", Name = "Banners", Frames = [bannerFrame] };
        var paintingTile = new TileProperty { Id = 240, Key = "Painting3X3", Name = "Wall Hangings 3x3", Frames = [trophyFrame] };
        var torchTile = new TileProperty { Id = 4, Key = "Torches", Name = "Torches", Frames = [torchFrame] };
        var tileA = new TileProperty { Id = 700, Key = "TileA", Name = "Tile A", Frames = [sharedA] };
        var tileB = new TileProperty { Id = 701, Key = "TileB", Name = "Tile B", Frames = [sharedB] };
        var unknownTile = new TileProperty { Id = 702, Key = "TileC", Name = "Tile C", Frames = [untranslated] };
        var bannerItem = new ItemProperty { Id = 1, Key = "BlueSlimeBanner", Name = "Blue Slime Banner" };
        var trophyItem = new ItemProperty { Id = 2, Key = "EyeofCthulhuTrophy", Name = "Eye of Cthulhu Trophy" };
        var torchItem = new ItemProperty { Id = 3, Key = "Torch", Name = "Torch" };

        var store = TerrariaDataStore.InitializeFrom(
            tiles: [bannerTile, paintingTile, torchTile, tileA, tileB, unknownTile],
            items: [bannerItem, trophyItem, torchItem]);

        store.LoadLocalization("zh-Hans", _tempDirectory);

        bannerFrame.DisplayName.ShouldBe("蓝史莱姆旗");
        trophyFrame.DisplayName.ShouldBe("克苏鲁之眼纪念章");
        torchFrame.DisplayName.ShouldBe("火把: 开启 [底部]");
        sharedA.DisplayName.ShouldBe("甲专属");
        sharedB.DisplayName.ShouldBe("乙专属");
        untranslated.DisplayName.ShouldBe("Unknown Frame: Variant [Top]");
    }

    [Fact]
    public void EmbeddedZhHans_ResolvesTrophiesAliasesAndCategoryFrames()
    {
        var store = TerrariaDataStore.Initialize(locale: "zh-Hans");

        FrameProperty Frame(string tileKey, string raw)
            => store.Tiles.Single(tile => tile.Key == tileKey)
                .Frames!.Single(frame => frame.ToString() == raw);

        Frame("Painting3X3", "Boss Trophy: King Slime Trophy").DisplayName.ShouldBe("史莱姆王纪念章");
        Frame("Painting3X3", "Boss Trophy: Moon Lord Trophy").DisplayName.ShouldBe("月亮领主纪念章");
        Frame("Painting3X3", "Boss Trophy: Mourning Wood Trophy").DisplayName.ShouldBe("哀木纪念章");
        Frame("Painting3X3", "Boss Trophy: Pumpking Trophy").DisplayName.ShouldBe("南瓜王纪念章");
        Frame("Painting3X3", "Boss Trophy: Ogre Trophy").DisplayName.ShouldBe("食人魔纪念章");
        Frame("Painting3X3", "Boss Trophy: Ancient Cultist Trophy").DisplayName.ShouldBe("拜月教邪教徒纪念章");
        Frame("Painting3X3", "Fishing Trophy: Goldfish Trophy").DisplayName.ShouldBe("金鱼纪念章");
        Frame("Painting3X3", "Fishing Trophy: Sharkteeth Trophy").DisplayName.ShouldBe("鲨牙纪念章");

        Frame("Tombstones", "Tombstones: Tombstone").DisplayName.ShouldBe("墓石");
        Frame("Tombstones", "Tombstones: Golden Tombstone").DisplayName.ShouldBe("金墓石");
        Frame("MetalBars", "Ore Bars: Copper Bar").DisplayName.ShouldBe("铜锭");
        Frame("MetalBars", "Ore Bars: Luminite Bar").DisplayName.ShouldBe("夜明锭");

        Frame("Banners", "Monster Banner: Nypmh Banner").DisplayName.ShouldBe("宁芙旗");
        Frame("Banners", "Monster Banner: Martian Brainscrambler Banner").DisplayName.ShouldBe("火星扰脑怪旗");
        Frame("Painting6X4", "Painting: Pillagin Me Pixels").DisplayName.ShouldBe("海贼像素画");
        Frame("Painting2X3", "Painting: Strange Growth A").DisplayName.ShouldBe("奇异植株 A");
        Frame("AlphabetStatues", "Text Statue: A Statue").DisplayName.ShouldBe("A字雕像");

        var kingSlimeTrophy = Frame("Painting3X3", "Boss Trophy: King Slime Trophy");
        LocalizedNameSearch.Matches("史莱姆王纪念章", kingSlimeTrophy.Name, kingSlimeTrophy.DisplayName).ShouldBeTrue();
        LocalizedNameSearch.Matches("Boss Trophy", kingSlimeTrophy.Name, kingSlimeTrophy.DisplayName).ShouldBeTrue();

        foreach (var tile in store.Tiles.Where(tile => tile.Frames != null))
        {
            foreach (var frame in tile.Frames!)
            {
                frame.DisplayName.ShouldNotContain("Trophy Trophy");
                frame.DisplayName.ShouldNotContain("墓石: 墓石");
                frame.DisplayName.ShouldNotContain("金属锭: ");
            }
        }
    }

    [Fact]
    public void LanguageSwitch_ResetsAndUpdatesAllDisplayNames()
    {
        WriteOfficial("zh-Hans", new
        {
            items = new Dictionary<string, string> { ["Bottle"] = "瓶子" },
            tiles = new Dictionary<string, string> { ["Bottles"] = "放置的瓶子" }
        });
        WriteOfficial("ja-JP", new
        {
            items = new Dictionary<string, string> { ["Bottle"] = "ビン" },
            tiles = new Dictionary<string, string> { ["Bottles"] = "置かれた瓶" }
        });

        var bottleFrame = new FrameProperty { Name = "Bottle" };
        var tile = new TileProperty { Id = 13, Key = "Bottles", Name = "Bottles", Frames = [bottleFrame] };
        var item = new ItemProperty { Id = 31, Key = "Bottle", Name = "Bottle" };
        var store = TerrariaDataStore.InitializeFrom(tiles: [tile], items: [item]);

        store.LoadLocalization("zh-Hans", _tempDirectory);
        tile.DisplayName.ShouldBe("放置的瓶子");
        item.DisplayName.ShouldBe("瓶子");
        // The bundled zh-Hans override table wins over the exact item-name fallback.
        bottleFrame.DisplayName.ShouldBe("玻璃瓶");
        store.NpcDisplayNameById.ShouldBeEmpty();
        store.PrefixDisplayById.ShouldBeEmpty();

        store.LoadLocalization("ja-JP", _tempDirectory);
        tile.DisplayName.ShouldBe("置かれた瓶");
        item.DisplayName.ShouldBe("ビン");
        bottleFrame.DisplayName.ShouldBe("ビン");

        store.LoadLocalization("en-US", _tempDirectory);
        tile.DisplayName.ShouldBe("Bottles");
        item.DisplayName.ShouldBe("Bottle");
        bottleFrame.DisplayName.ShouldBe("Bottle");
        store.Localization.ShouldBeNull();

        store.LoadLocalization("zh-Hans", _tempDirectory);
        tile.DisplayName.ShouldBe("放置的瓶子");
        bottleFrame.DisplayName.ShouldBe("玻璃瓶");
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
