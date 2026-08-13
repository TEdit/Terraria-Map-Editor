using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Shouldly;
using TEdit.Common.Serialization;
using TEdit.Terraria.DataModel;

namespace TEdit.Terraria.Tests.DataModel;

public class MorphConfigurationTests
{
    [Fact]
    public void Load_FromEmbeddedResource_Works()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        config.ShouldNotBeNull();
        config.Biomes.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GenerateProfile_SemanticallyMatchesPinnedLegacyDataset()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.generate.json");

        var actualHash = ComputeCanonicalJsonHash(stream);

        // Canonical JSON fingerprint of morphBiomes.json from
        // fb043ee864ae8f77867982a332bddb7034ae74b9 (Git blob 99483b8bd48373a0dd6313a1d919e76dcfd0603b).
        actualHash.ShouldBe("E364C741F6BEDFA10FAB5750B76D459A27C258003E4D20511B734EB30E4006F0",
            "the explicit Generate biome mode must preserve every reviewed legacy mapping");
    }

    [Fact]
    public void GenerateProfile_IsOneCanonicalEmbeddedResource()
    {
        var resources = typeof(MorphConfiguration).Assembly.GetManifestResourceNames()
            .Where(name => name.Contains("morphBiomes.generate", StringComparison.Ordinal))
            .OrderBy(name => name)
            .ToArray();

        resources.Length.ShouldBe(1);
        resources[0].ShouldEndWith(".morphBiomes.generate.json");
    }

    private static string ComputeCanonicalJsonHash(Stream stream)
    {
        using var document = JsonDocument.Parse(stream);
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            WriteCanonicalJson(writer, document.RootElement);
        }

        return Convert.ToHexString(SHA256.HashData(buffer.ToArray()));
    }

    private static void WriteCanonicalJson(Utf8JsonWriter writer, JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();
            foreach (var property in element.EnumerateObject().OrderBy(property => property.Name, StringComparer.Ordinal))
            {
                writer.WritePropertyName(property.Name);
                WriteCanonicalJson(writer, property.Value);
            }
            writer.WriteEndObject();
            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            writer.WriteStartArray();
            foreach (var item in element.EnumerateArray())
                WriteCanonicalJson(writer, item);
            writer.WriteEndArray();
            return;
        }

        element.WriteTo(writer);
    }

    [Fact]
    public void Load_FromEmbeddedResource_NormalMorphsAreNonDestructive()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        var destructiveRules = config.Biomes
            .SelectMany(biome => biome.Value.MorphTiles
                .Where(rule => rule.Delete || rule.SpriteOffsets.Any(offset => offset.Delete))
                .Select(rule => $"{biome.Key}/{rule.Name}"))
            .Concat(config.Biomes
                .SelectMany(biome => biome.Value.MorphWalls
                    .Where(rule => rule.Delete || rule.SpriteOffsets.Any(offset => offset.Delete))
                    .Select(rule => $"{biome.Key}/{rule.Name}")))
            .Concat(config.MorphGroups
                .SelectMany(group => group.Variants
                    .Where(variant => variant.Value.Delete)
                    .Select(variant => $"group:{group.Name}/{variant.Key}")))
            .OrderBy(name => name)
            .ToArray();

        destructiveRules.ShouldBeEmpty(
            "normal biome morphing must preserve tiles and sprites when no exact target exists");
    }

    [Fact]
    public void Load_FromEmbeddedResource_SubstrateConversionsStayWithinReversibleFamilies()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        ushort[][] reversibleFamilies =
        [
            [2, 23, 199, 109],             // grass
            [477, 492],                     // golf grass
            [60, 70, 661, 662],             // jungle/mushroom grass
            [1, 25, 117, 203],              // stone
            [161, 163, 164, 200],           // ice
            [53, 112, 116, 234],            // sand
            [397, 398, 399, 402],            // hardened sand
            [396, 400, 401, 403],            // sandstone
            [0, 668],                        // dirt
            [59],                            // mud
            [123],                           // silt
            [147],                           // snow
            [224],                           // slush
            [179, 180, 181, 182, 183, 381, 534, 536, 539, 625, 627], // moss
            [512, 513, 514, 515, 516, 517, 535, 537, 540, 626, 628], // moss brick
        ];

        var familyById = reversibleFamilies
            .SelectMany((family, index) => family.Select(id => (id, index)))
            .ToDictionary(item => item.id, item => item.index);

        var crossFamilyRules = config.Biomes
            .SelectMany(biome => biome.Value.MorphTiles.Select(rule => (biome: biome.Key, rule)))
            .SelectMany(item => item.rule.SourceIds
                .Where(familyById.ContainsKey)
                .SelectMany(sourceId => GetConfiguredTargetIds(item.rule)
                    .Where(targetId => !familyById.TryGetValue(targetId, out var targetFamily)
                        || targetFamily != familyById[sourceId])
                    .Select(targetId => $"{item.biome}/{item.rule.Name}:{sourceId}->{targetId}")))
            .OrderBy(description => description)
            .ToArray();

        crossFamilyRules.ShouldBeEmpty(
            "MORPH must not collapse substrate identity; unsupported biome counterparts are preserved");
    }

    [Fact]
    public void Load_FromEmbeddedResource_MorphGroupsContainOnlyBiomeCounterparts()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        var expectedBiomesByGroup = new Dictionary<string, string[]>
        {
            ["Altars"] = ["Corruption", "Crimson"],
            ["Orbs"] = ["Corruption", "Crimson"],
            ["Thorns"] = ["Corruption", "Crimson", "Jungle"],
            ["Vines"] = ["Corruption", "Crimson", "Forest", "GlowingMushroom", "Hallow", "Jungle", "Purify"],
            ["CaveSpikes"] = ["Corruption", "Crimson", "Hallow", "Purify"],
            ["Torches"] = ["Corruption", "Crimson", "Desert", "Forest", "GlowingMushroom", "Hallow", "Jungle", "Purify", "Snow"],
        };

        var actualBiomesByGroup = config.MorphGroups.ToDictionary(
            group => group.Name,
            group => group.Variants.Keys.OrderBy(name => name).ToArray());

        actualBiomesByGroup.Keys.ShouldBe(expectedBiomesByGroup.Keys, ignoreOrder: true);
        foreach (var (group, expectedBiomes) in expectedBiomesByGroup)
            actualBiomesByGroup[group].ShouldBe(expectedBiomes.OrderBy(name => name), ignoreOrder: false);
    }

    [Fact]
    public void Load_FromEmbeddedResource_VinesAndThornsConvertEveryCounterpart()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        var expectedVariants = new Dictionary<string, Dictionary<string, ushort>>
        {
            ["Vines"] = new()
            {
                ["Corruption"] = 636, ["Crimson"] = 205, ["Forest"] = 52,
                ["GlowingMushroom"] = 528, ["Hallow"] = 115, ["Jungle"] = 62,
                ["Purify"] = 52,
            },
            ["Thorns"] = new()
            {
                ["Corruption"] = 32, ["Crimson"] = 352, ["Jungle"] = 69,
            },
        };

        var gaps = expectedVariants.SelectMany(group => group.Value.SelectMany(target =>
                group.Value
                    .Where(source => source.Key != target.Key && source.Value != target.Value)
                    .Where(source => ResolveTileTarget(config, target.Key, source.Value) != target.Value)
                    .Select(source => $"{group.Key}/{target.Key}:{source.Value}->{target.Value}"
                        + $" (actual {ResolveTileTarget(config, target.Key, source.Value)?.ToString() ?? "preserve"})")))
            .OrderBy(description => description)
            .ToArray();

        gaps.ShouldBeEmpty("every sprite counterpart in a morph group must have a conversion path");

        var unexpected = expectedVariants.SelectMany(group => group.Value.SelectMany(target =>
                config.Biomes[target.Key].MorphTiles
                    .Where(rule => GetConfiguredTargetIds(rule).Contains(target.Value))
                    .SelectMany(rule => rule.SourceIds
                        .Where(source => !group.Value.Values.Contains(source))
                        .Select(source => $"{group.Key}/{target.Key}:{source}->{target.Value}"))))
            .OrderBy(description => description)
            .ToArray();

        unexpected.ShouldBeEmpty("sprite groups must not absorb visually unrelated tiles");
    }

    [Fact]
    public void Load_FromEmbeddedResource_EverySpriteGroupVariantHasExactReplacement()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        var gaps = config.MorphGroups.SelectMany(group => group.Variants.SelectMany(target =>
                group.Variants
                    .Where(source => source.Key != target.Key && source.Value.TileId.HasValue)
                    .SelectMany(source => FindSpriteReplacementGap(
                        config,
                        group.Name,
                        target.Key,
                        target.Value,
                        source.Key,
                        source.Value))))
            .OrderBy(description => description)
            .ToArray();

        gaps.ShouldBeEmpty(
            "every sprite counterpart must use an exact tile replacement or frame/UV transform");
    }

    [Fact]
    public void Load_FromEmbeddedResource_UnsupportedDirectSpriteTransformsArePreserved()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        var unsupported = config.Biomes.SelectMany(biome => biome.Value.MorphTiles
                .Where(rule => rule.Name.StartsWith("sprite", StringComparison.Ordinal))
                .Where(rule => rule.Name != "spriteCaveSpikes" && !rule.UseMoss)
                .Where(rule => rule.SourceIds.Count > 0)
                .Where(rule => rule.SpriteOffsets.Any(offset =>
                        offset.Delete || offset.OffsetU != 0 || offset.OffsetV != 0)
                    || rule.SourceIds.Any(source => GetConfiguredTargetIds(rule)
                        .Any(target => target != source)))
                .Select(rule => $"{biome.Key}/{rule.Name}"))
            .OrderBy(description => description)
            .ToArray();

        unsupported.ShouldBeEmpty(
            "a hand-authored sprite transform without a complete counterpart family must leave the sprite unchanged");
    }

    [Fact]
    public void Load_FromEmbeddedResource_CaveSpikeFamiliesAreCompleteAndReversible()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        var expected = new Dictionary<string, (short sourceU, short targetU)[]>
        {
            ["Purify"] = [(216, 54), (270, 54), (324, 54), (540, 0), (594, 0), (648, 0)],
            ["Forest"] = [(216, 54), (270, 54), (324, 54), (540, 0), (594, 0), (648, 0)],
            ["Snow"] = [(540, 0), (594, 0), (648, 0)],
            ["Corruption"] = [(0, 594), (540, 594), (648, 594), (54, 270), (216, 270), (324, 270)],
            ["Crimson"] = [(0, 648), (540, 648), (594, 648), (54, 324), (216, 324), (270, 324)],
            ["Hallow"] = [(0, 540), (594, 540), (648, 540), (54, 216), (270, 216), (324, 216)],
        };

        var gaps = expected.SelectMany(biome =>
        {
            var rule = config.Biomes[biome.Key].MorphTiles.Single(item => item.Name == "spriteCaveSpikes");
            return biome.Value
                .Where(path => !rule.SpriteOffsets.Any(offset =>
                    offset.MinU == path.sourceU
                    && offset.MaxU == path.sourceU + 36
                    && offset.OffsetU == path.targetU - path.sourceU))
                .Select(path => $"{biome.Key}:{path.sourceU}->{path.targetU}");
        }).ToArray();

        gaps.ShouldBeEmpty("every supported stone and ice spike counterpart must have an exact 37-pixel UV mapping");

        config.Biomes.Values.SelectMany(biome => biome.MorphTiles)
            .Where(rule => rule.Name == "spriteCaveSpikes")
            .SelectMany(rule => rule.SpriteOffsets)
            .ShouldNotContain(offset => offset.MinU == 378 && (offset.OffsetU != 0 || offset.Delete),
                "sandstone spikes have no evil/snow counterpart and must remain unchanged");

        config.Biomes["Desert"].MorphTiles.Single(rule => rule.Name == "spriteCaveSpikes")
            .SourceIds.ShouldBeEmpty("Desert must not collapse unrelated cave-spike families into sandstone");
    }

    [Fact]
    public void Load_FromEmbeddedResource_SourceIdsResolveToOneRulePerBiome()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        var duplicateSources = config.Biomes.SelectMany(biome =>
                FindDuplicateSources(biome.Key, "tile", biome.Value.MorphTiles)
                    .Concat(FindDuplicateSources(biome.Key, "wall", biome.Value.MorphWalls)))
            .OrderBy(description => description)
            .ToArray();

        duplicateSources.ShouldBeEmpty(
            "rule order must not determine which biome conversion wins");
    }

    [Fact]
    public void Load_FromEmbeddedResource_WallConversionsStayWithinReversibleFamilies()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        ushort[][] reversibleFamilies =
        [
            [63, 64, 69, 70, 81],           // natural grass
            [66, 67, 264, 265, 268],        // safe grass
            [349, 3, 28, 83],                // natural biome stone
            [1, 246, 248, 269],              // safe biome stone
            [212, 188, 192, 200, 204],       // natural rock 1
            [213, 189, 193, 201, 205],       // natural rock 2
            [214, 190, 194, 202, 206],       // natural rock 3
            [215, 191, 195, 203, 207],       // natural rock 4
            [300, 276, 280, 288, 292],       // safe rock 1
            [301, 277, 281, 289, 293],       // safe rock 2
            [302, 278, 282, 290, 294],       // safe rock 3
            [303, 279, 283, 291, 295],       // safe rock 4
            [216, 217, 218, 219],            // natural hardened sand
            [304, 305, 306, 307],            // safe hardened sand
            [187, 220, 221, 222],            // natural sandstone
            [275, 308, 309, 310],            // safe sandstone
            [2], [16],                        // natural/safe dirt
            [15], [247],                      // natural/safe mud
            [40], [249],                      // natural/safe snow
            [71], [266],                      // natural/safe ice
            [61], [262],                      // natural/safe old stone
            [185], [274],                     // natural/safe craggy stone
            [65], [68],                       // natural/safe flower
            [80], [74],                       // ambiguous mushroom walls: preserve
        ];

        var familyById = reversibleFamilies
            .SelectMany((family, index) => family.Select(id => (id, index)))
            .ToDictionary(item => item.id, item => item.index);

        var crossFamilyRules = config.Biomes
            .SelectMany(biome => biome.Value.MorphWalls.Select(rule => (biome: biome.Key, rule)))
            .SelectMany(item => item.rule.SourceIds
                .Where(familyById.ContainsKey)
                .SelectMany(sourceId => GetConfiguredTargetIds(item.rule)
                    .Where(targetId => !familyById.TryGetValue(targetId, out var targetFamily)
                        || targetFamily != familyById[sourceId])
                    .Select(targetId => $"{item.biome}/{item.rule.Name}:{sourceId}->{targetId}")))
            .OrderBy(description => description)
            .ToArray();

        crossFamilyRules.ShouldBeEmpty(
            "walls without an exact biome counterpart must be preserved");
    }

    [Fact]
    public void Load_FromEmbeddedResource_ConvertsEveryCanonicalTileCounterpart()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        var expected = new Dictionary<string, (ushort target, ushort[] sources)[]>
        {
            ["Purify"] =
            [
                (2, [23, 199, 109]), (477, [492]), (60, [661, 662]),
                (1, [25, 203, 117]), (161, [163, 200, 164]),
                (53, [112, 234, 116]), (397, [398, 399, 402]),
                (396, [400, 401, 403]),
            ],
            ["Corruption"] =
            [
                (23, [2, 199, 109]), (661, [60, 70, 662]),
                (25, [1, 203, 117]), (163, [161, 200, 164]),
                (112, [53, 234, 116]), (398, [397, 399, 402]),
                (400, [396, 401, 403]),
            ],
            ["Crimson"] =
            [
                (199, [2, 23, 109]), (662, [60, 70, 661]),
                (203, [1, 25, 117]), (200, [161, 163, 164]),
                (234, [53, 112, 116]), (399, [397, 398, 402]),
                (401, [396, 400, 403]),
            ],
            ["Hallow"] =
            [
                (109, [2, 23, 199]), (492, [477]),
                (117, [1, 25, 203]), (164, [161, 163, 200]),
                (116, [53, 112, 234]), (402, [397, 398, 399]),
                (403, [396, 400, 401]),
            ],
            ["Forest"] =
            [
                (2, [23, 199, 109]), (477, [492]), (60, [661, 662]),
                (1, [25, 203, 117]), (161, [163, 200, 164]),
                (53, [112, 234, 116]), (397, [398, 399, 402]),
                (396, [400, 401, 403]),
            ],
            ["Jungle"] = [(60, [70, 661, 662])],
            ["GlowingMushroom"] = [(70, [60, 661, 662])],
            ["Snow"] = [(161, [163, 200, 164])],
            ["Desert"] =
            [
                (53, [112, 234, 116]), (397, [398, 399, 402]),
                (396, [400, 401, 403]),
            ],
        };

        var gaps = expected.SelectMany(biome => biome.Value.SelectMany(family =>
                family.sources
                    .Where(source => ResolveTileTarget(config, biome.Key, source) != family.target)
                    .Select(source => $"{biome.Key}:{source}->{family.target}"
                        + $" (actual {ResolveTileTarget(config, biome.Key, source)?.ToString() ?? "preserve"})")))
            .OrderBy(description => description)
            .ToArray();

        gaps.ShouldBeEmpty("canonical biome-specific tile counterparts must convert");
    }

    [Fact]
    public void Load_FromEmbeddedResource_ConvertsEveryCanonicalWallCounterpart()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        var pureWalls = new (ushort target, ushort[] sources)[]
        {
            (63, [69, 70, 81]), (66, [264, 265, 268]),
            (349, [3, 28, 83]), (1, [246, 248, 269]),
            (212, [188, 192, 200]), (213, [189, 193, 201]),
            (214, [190, 194, 202]), (215, [191, 195, 203]),
            (300, [276, 280, 288]), (301, [277, 281, 289]),
            (302, [278, 282, 290]), (303, [279, 283, 291]),
            (216, [217, 218, 219]), (304, [305, 306, 307]),
            (187, [220, 221, 222]), (275, [308, 309, 310]),
        };

        var expected = new Dictionary<string, (ushort target, ushort[] sources)[]>
        {
            ["Purify"] = pureWalls,
            ["Forest"] = pureWalls,
            ["Corruption"] =
            [
                (69, [63, 64, 70, 81]), (264, [66, 67, 265, 268]),
                (3, [349, 28, 83]), (246, [1, 248, 269]),
                (188, [212, 192, 200, 204]), (189, [213, 193, 201, 205]),
                (190, [214, 194, 202, 206]), (191, [215, 195, 203, 207]),
                (276, [300, 280, 288, 292]), (277, [301, 281, 289, 293]),
                (278, [302, 282, 290, 294]), (279, [303, 283, 291, 295]),
                (217, [216, 218, 219]), (305, [304, 306, 307]),
                (220, [187, 221, 222]), (308, [275, 309, 310]),
            ],
            ["Crimson"] =
            [
                (81, [63, 64, 69, 70]), (268, [66, 67, 264, 265]),
                (83, [349, 3, 28]), (269, [1, 246, 248]),
                (192, [212, 188, 200, 204]), (193, [213, 189, 201, 205]),
                (194, [214, 190, 202, 206]), (195, [215, 191, 203, 207]),
                (280, [300, 276, 288, 292]), (281, [301, 277, 289, 293]),
                (282, [302, 278, 290, 294]), (283, [303, 279, 291, 295]),
                (218, [216, 217, 219]), (306, [304, 305, 307]),
                (221, [187, 220, 222]), (309, [275, 308, 310]),
            ],
            ["Hallow"] =
            [
                (70, [63, 64, 69, 81]), (265, [66, 67, 264, 268]),
                (28, [349, 3, 83]), (248, [1, 246, 269]),
                (200, [212, 188, 192]), (201, [213, 189, 193]),
                (202, [214, 190, 194]), (203, [215, 191, 195]),
                (288, [300, 276, 280]), (289, [301, 277, 281]),
                (290, [302, 278, 282]), (291, [303, 279, 283]),
                (219, [216, 217, 218]), (307, [304, 305, 306]),
                (222, [187, 220, 221]), (310, [275, 308, 309]),
            ],
            ["Jungle"] =
            [
                (64, [63, 69, 70, 81]), (67, [66, 264, 265, 268]),
                (204, [212, 188, 192, 200]), (205, [213, 189, 193, 201]),
                (206, [214, 190, 194, 202]), (207, [215, 191, 195, 203]),
                (292, [300, 276, 280, 288]), (293, [301, 277, 281, 289]),
                (294, [302, 278, 282, 290]), (295, [303, 279, 283, 291]),
            ],
            ["Desert"] =
            [
                (216, [217, 218, 219]), (304, [305, 306, 307]),
                (187, [220, 221, 222]), (275, [308, 309, 310]),
            ],
        };

        var gaps = expected.SelectMany(biome => biome.Value.SelectMany(family =>
                family.sources
                    .Where(source => ResolveWallTarget(config, biome.Key, source) != family.target)
                    .Select(source => $"{biome.Key}:{source}->{family.target}"
                        + $" (actual {ResolveWallTarget(config, biome.Key, source)?.ToString() ?? "preserve"})")))
            .OrderBy(description => description)
            .ToArray();

        gaps.ShouldBeEmpty("canonical biome-specific wall counterparts must convert");
    }

    [Fact]
    public void Load_FromEmbeddedResource_CanonicalConversionsHaveDeterministicReturnPaths()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        (string targetBiome, ushort source, ushort target, string originBiome)[] tilePaths =
        [
            ("Corruption", 60, 661, "Jungle"),
            ("Crimson", 70, 662, "GlowingMushroom"),
            ("Hallow", 25, 117, "Corruption"),
            ("Desert", 112, 53, "Corruption"),
            ("Snow", 163, 161, "Corruption"),
            ("Forest", 199, 2, "Crimson"),
        ];

        foreach (var path in tilePaths)
        {
            ResolveTileTarget(config, path.targetBiome, path.source).ShouldBe(path.target);
            ResolveTileTarget(config, path.originBiome, path.target).ShouldBe(path.source);
        }

        (string targetBiome, ushort source, ushort target, string originBiome)[] wallPaths =
        [
            ("Corruption", 64, 69, "Jungle"),
            ("Hallow", 3, 28, "Corruption"),
            ("Crimson", 204, 192, "Jungle"),
            ("Desert", 217, 216, "Corruption"),
            ("Forest", 69, 63, "Corruption"),
            ("Purify", 83, 349, "Crimson"),
        ];

        foreach (var path in wallPaths)
        {
            ResolveWallTarget(config, path.targetBiome, path.source).ShouldBe(path.target);
            ResolveWallTarget(config, path.originBiome, path.target).ShouldBe(path.source);
        }
    }

    [Fact]
    public void Load_FromEmbeddedResource_EveryConfiguredCanonicalMappingHasReturnRule()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        var tileOriginById = new Dictionary<ushort, string>
        {
            [2] = "Forest", [23] = "Corruption", [199] = "Crimson", [109] = "Hallow",
            [477] = "Forest", [492] = "Hallow",
            [60] = "Jungle", [70] = "GlowingMushroom", [661] = "Corruption", [662] = "Crimson",
            [1] = "Forest", [25] = "Corruption", [117] = "Hallow", [203] = "Crimson",
            [161] = "Snow", [163] = "Corruption", [164] = "Hallow", [200] = "Crimson",
            [53] = "Desert", [112] = "Corruption", [116] = "Hallow", [234] = "Crimson",
            [397] = "Desert", [398] = "Corruption", [399] = "Crimson", [402] = "Hallow",
            [396] = "Desert", [400] = "Corruption", [401] = "Crimson", [403] = "Hallow",
        };

        var wallOriginById = new Dictionary<ushort, string>
        {
            [63] = "Forest", [64] = "Jungle", [69] = "Corruption", [70] = "Hallow", [81] = "Crimson",
            [66] = "Forest", [67] = "Jungle", [264] = "Corruption", [265] = "Hallow", [268] = "Crimson",
            [349] = "Forest", [3] = "Corruption", [28] = "Hallow", [83] = "Crimson",
            [1] = "Forest", [246] = "Corruption", [248] = "Hallow", [269] = "Crimson",
            [212] = "Forest", [188] = "Corruption", [192] = "Crimson", [200] = "Hallow", [204] = "Jungle",
            [213] = "Forest", [189] = "Corruption", [193] = "Crimson", [201] = "Hallow", [205] = "Jungle",
            [214] = "Forest", [190] = "Corruption", [194] = "Crimson", [202] = "Hallow", [206] = "Jungle",
            [215] = "Forest", [191] = "Corruption", [195] = "Crimson", [203] = "Hallow", [207] = "Jungle",
            [300] = "Forest", [276] = "Corruption", [280] = "Crimson", [288] = "Hallow", [292] = "Jungle",
            [301] = "Forest", [277] = "Corruption", [281] = "Crimson", [289] = "Hallow", [293] = "Jungle",
            [302] = "Forest", [278] = "Corruption", [282] = "Crimson", [290] = "Hallow", [294] = "Jungle",
            [303] = "Forest", [279] = "Corruption", [283] = "Crimson", [291] = "Hallow", [295] = "Jungle",
            [216] = "Desert", [217] = "Corruption", [218] = "Crimson", [219] = "Hallow",
            [304] = "Desert", [305] = "Corruption", [306] = "Crimson", [307] = "Hallow",
            [187] = "Desert", [220] = "Corruption", [221] = "Crimson", [222] = "Hallow",
            [275] = "Desert", [308] = "Corruption", [309] = "Crimson", [310] = "Hallow",
        };

        var missingTileReturns = FindMissingReturnRules(
            config,
            tileOriginById,
            biome => biome.MorphTiles,
            "tile");
        var missingWallReturns = FindMissingReturnRules(
            config,
            wallOriginById,
            biome => biome.MorphWalls,
            "wall");

        missingTileReturns.Concat(missingWallReturns).OrderBy(item => item).ShouldBeEmpty(
            "every configured canonical conversion must have an explicit path back through the source biome");
    }

    private static ushort? ResolveTileTarget(MorphConfiguration config, string biome, ushort sourceId)
    {
        var rule = config.Biomes[biome].MorphTiles.SingleOrDefault(item => item.SourceIds.Contains(sourceId));
        return rule?.Default.GetId(MorphLevel.Sky, useEvil: false);
    }

    private static ushort? ResolveWallTarget(MorphConfiguration config, string biome, ushort sourceId)
    {
        var rule = config.Biomes[biome].MorphWalls.SingleOrDefault(item => item.SourceIds.Contains(sourceId));
        return rule?.Default.GetId(MorphLevel.Sky, useEvil: false);
    }

    private static IEnumerable<string> FindDuplicateSources(
        string biome,
        string category,
        IEnumerable<MorphId> rules)
    {
        return rules
            .SelectMany(rule => rule.SourceIds.Select(sourceId => (sourceId, rule.Name)))
            .GroupBy(item => item.sourceId)
            .Where(group => group.Count() > 1)
            .Select(group => $"{biome}/{category}/{group.Key}:"
                + string.Join(",", group.Select(item => item.Name).OrderBy(name => name)));
    }

    private static IEnumerable<string> FindMissingReturnRules(
        MorphConfiguration config,
        IReadOnlyDictionary<ushort, string> originById,
        Func<MorphBiomeData, IEnumerable<MorphId>> selectRules,
        string category)
    {
        return config.Biomes.SelectMany(targetBiome => selectRules(targetBiome.Value)
            .SelectMany(rule => rule.SourceIds
                .Where(originById.ContainsKey)
                .SelectMany(sourceId => GetConfiguredTargetIds(rule)
                    .Where(originById.ContainsKey)
                    .Where(targetId => targetId != sourceId)
                    .Where(targetId => !selectRules(config.Biomes[originById[sourceId]])
                        .Any(returnRule => returnRule.SourceIds.Contains(targetId)
                            && GetConfiguredTargetIds(returnRule).Contains(sourceId)))
                    .Select(targetId => $"{category}/{targetBiome.Key}:{sourceId}->{targetId}"
                        + $" missing {originById[sourceId]}:{targetId}->{sourceId}"))));
    }

    private static IEnumerable<string> FindSpriteReplacementGap(
        MorphConfiguration config,
        string group,
        string targetBiome,
        MorphGroupVariant target,
        string sourceBiome,
        MorphGroupVariant source)
    {
        if (target.Delete)
            yield break;

        var rules = config.Biomes[targetBiome].MorphTiles;
        if (target.TileId != source.TileId)
        {
            if (!rules.Any(rule => rule.SourceIds.Contains(source.TileId!.Value)
                && rule.SpriteReplacement?.TargetTileId == target.TileId))
            {
                yield return $"{group}/{targetBiome} from {sourceBiome}:"
                    + $" tile {source.TileId}->{target.TileId} lacks sprite replacement";
            }

            yield break;
        }

        short sourceU = source.FrameU ?? 0;
        short sourceV = source.FrameV ?? 0;
        short targetU = target.FrameU ?? 0;
        short targetV = target.FrameV ?? 0;
        if (sourceU == targetU && sourceV == targetV)
            yield break;

        short sourceWidth = source.FrameWidth ?? 1;
        short sourceHeight = source.FrameHeight ?? 1;
        bool filtersV = source.FrameHeight.HasValue || target.FrameHeight.HasValue;

        bool hasExactOffset = rules
            .Where(rule => rule.SourceIds.Contains(source.TileId!.Value))
            .SelectMany(rule => rule.SpriteOffsets)
            .Any(offset => offset.MinU == sourceU
                && offset.MaxU == sourceU + sourceWidth - 1
                && offset.OffsetU == targetU - sourceU
                && offset.UseFilterV == filtersV
                && (!filtersV || (offset.MinV == sourceV
                    && offset.MaxV == sourceV + sourceHeight - 1
                    && offset.OffsetV == targetV - sourceV)));

        if (!hasExactOffset)
        {
            yield return $"{group}/{targetBiome} from {sourceBiome}:"
                + $" frame ({sourceU},{sourceV})->({targetU},{targetV}) lacks exact UV transform";
        }
    }

    private static IEnumerable<ushort> GetConfiguredTargetIds(MorphId rule)
    {
        return GetTargetIds(rule.Default)
            .Concat(GetTargetIds(rule.TouchingAir))
            .Concat(GetTargetIds(rule.Gravity))
            .Distinct();
    }

    private static IEnumerable<ushort> GetTargetIds(MorphIdLevels? levels)
    {
        if (levels == null)
            yield break;

        if (levels.EvilId.HasValue) yield return levels.EvilId.Value;
        if (levels.SkyId.HasValue) yield return levels.SkyId.Value;
        if (levels.DirtId.HasValue) yield return levels.DirtId.Value;
        if (levels.RockId.HasValue) yield return levels.RockId.Value;
        if (levels.DeepRockId.HasValue) yield return levels.DeepRockId.Value;
        if (levels.HellId.HasValue) yield return levels.HellId.Value;
    }

    [Fact]
    public void Load_FromEmbeddedResource_ExpandsMorphGroups()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        config.MorphGroups.Count.ShouldBeGreaterThan(0);

        // Altars group should have generated entries for Corruption and Crimson biomes
        var corruptionTiles = config.Biomes["Corruption"].MorphTiles;
        corruptionTiles.ShouldContain(m => m.Name.StartsWith("group:Altars"));

        var crimsonTiles = config.Biomes["Crimson"].MorphTiles;
        crimsonTiles.ShouldContain(m => m.Name.StartsWith("group:Altars"));

        // Orbs group should have generated entries
        corruptionTiles.ShouldContain(m => m.Name.StartsWith("group:Orbs"));
    }

    [Fact]
    public void Load_FromEmbeddedResource_UsesCurrentEvilJungleConversions()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        var purifyJungle = config.Biomes["Purify"].MorphTiles
            .Single(m => m.Name == "tileCorruptJungleGrass");
        purifyJungle.SourceIds.ShouldBe(new ushort[] { 661, 662 }, ignoreOrder: true);
        purifyJungle.Default.SkyId.ShouldBe((ushort)60);

        var corruptionJungle = config.Biomes["Corruption"].MorphTiles
            .Single(m => m.Name == "tileJungleGrass");
        corruptionJungle.Default.SkyId.ShouldBe((ushort)661);
        corruptionJungle.UseMoss.ShouldBeFalse();

        var crimsonJungle = config.Biomes["Crimson"].MorphTiles
            .Single(m => m.Name == "tileJungleGrass");
        crimsonJungle.Default.SkyId.ShouldBe((ushort)662);
        crimsonJungle.UseMoss.ShouldBeFalse();

        config.Biomes["Corruption"].MorphTiles
            .ShouldNotContain(m => m.SourceIds.Contains((ushort)59));
        config.Biomes["Crimson"].MorphTiles
            .ShouldNotContain(m => m.SourceIds.Contains((ushort)59));
    }

    [Fact]
    public void IsMoss_ReturnsTrueForMossTypes()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("morphBiomes.json");
        var config = MorphConfiguration.Load(stream);

        config.MossTypes.Count.ShouldBeGreaterThan(0);

        foreach (var mossType in config.MossTypes.Values)
        {
            config.IsMoss((ushort)mossType).ShouldBe(true);
        }
    }

    [Fact]
    public void RoundTrip_JsonSerialization()
    {
        var original = new MorphConfiguration
        {
            Biomes = new Dictionary<string, MorphBiomeData>
            {
                ["Forest"] = new MorphBiomeData
                {
                    Name = "Forest",
                    MorphTiles = [new MorphId { Name = "Grass", SourceIds = [2, 23] }],
                    MorphWalls = [],
                },
            },
            MossTypes = new Dictionary<string, int> { ["Krypton"] = 381 },
        };

        var json = JsonSerializer.Serialize(original, TEditJsonSerializer.DefaultOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var restored = MorphConfiguration.Load(stream);

        restored.Biomes.Count.ShouldBe(1);
        restored.Biomes["Forest"].MorphTiles[0].Name.ShouldBe("Grass");
        restored.MossTypes["Krypton"].ShouldBe(381);
        restored.IsMoss(381).ShouldBe(true);
    }

    [Fact]
    public void RoundTrip_MorphGroups_Serialization()
    {
        var original = new MorphConfiguration
        {
            Biomes = new Dictionary<string, MorphBiomeData>
            {
                ["Corruption"] = new() { Name = "Corruption" },
                ["Crimson"] = new() { Name = "Crimson" },
                ["Purify"] = new() { Name = "Purify" },
            },
            MorphGroups =
            [
                new MorphGroup
                {
                    Name = "Vines",
                    Category = "tile",
                    Variants = new()
                    {
                        ["Corruption"] = new() { TileId = 636 },
                        ["Crimson"] = new() { TileId = 205 },
                        ["Purify"] = new() { TileId = 52 },
                    },
                },
            ],
        };

        var json = JsonSerializer.Serialize(original, TEditJsonSerializer.DefaultOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var restored = MorphConfiguration.Load(stream);

        restored.MorphGroups.Count.ShouldBe(1);
        restored.MorphGroups[0].Name.ShouldBe("Vines");
        restored.MorphGroups[0].Variants.Count.ShouldBe(3);
        restored.MorphGroups[0].Variants["Corruption"].TileId.ShouldBe((ushort)636);
    }

    [Fact]
    public void ExpandGroups_DifferentTileIds_GeneratesMorphIdEntries()
    {
        var config = new MorphConfiguration
        {
            Biomes = new Dictionary<string, MorphBiomeData>
            {
                ["Corruption"] = new() { Name = "Corruption" },
                ["Crimson"] = new() { Name = "Crimson" },
                ["Purify"] = new() { Name = "Purify" },
            },
            MorphGroups =
            [
                new MorphGroup
                {
                    Name = "Vines",
                    Category = "tile",
                    Variants = new()
                    {
                        ["Corruption"] = new() { TileId = 636 },
                        ["Crimson"] = new() { TileId = 205 },
                        ["Purify"] = new() { TileId = 52 },
                    },
                },
            ],
        };

        // Serialize and reload to trigger ExpandGroups via Load()
        var json = JsonSerializer.Serialize(config, TEditJsonSerializer.DefaultOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var loaded = MorphConfiguration.Load(stream);

        // Corruption biome should now convert from tiles 205, 52 to 636
        var corruptionTiles = loaded.Biomes["Corruption"].MorphTiles;
        corruptionTiles.Count.ShouldBe(1);
        corruptionTiles[0].Name.ShouldBe("group:Vines");
        corruptionTiles[0].SourceIds.ShouldContain((ushort)205);
        corruptionTiles[0].SourceIds.ShouldContain((ushort)52);
        corruptionTiles[0].Default.SkyId.ShouldBe((ushort)636);

        // Purify biome should convert from 636, 205 to 52
        var purifyTiles = loaded.Biomes["Purify"].MorphTiles;
        purifyTiles.Count.ShouldBe(1);
        purifyTiles[0].SourceIds.ShouldContain((ushort)636);
        purifyTiles[0].SourceIds.ShouldContain((ushort)205);
        purifyTiles[0].Default.SkyId.ShouldBe((ushort)52);
    }

    [Fact]
    public void ExpandGroups_SameTileIdDifferentFrames_GeneratesSpriteOffsets()
    {
        var config = new MorphConfiguration
        {
            Biomes = new Dictionary<string, MorphBiomeData>
            {
                ["Corruption"] = new() { Name = "Corruption" },
                ["Crimson"] = new() { Name = "Crimson" },
            },
            MorphGroups =
            [
                new MorphGroup
                {
                    Name = "Altars",
                    Category = "tile",
                    Variants = new()
                    {
                        ["Corruption"] = new() { TileId = 26, FrameU = 0, FrameV = 0, FrameWidth = 54, FrameHeight = 36 },
                        ["Crimson"] = new() { TileId = 26, FrameU = 54, FrameV = 0, FrameWidth = 54, FrameHeight = 36 },
                    },
                },
            ],
        };

        var json = JsonSerializer.Serialize(config, TEditJsonSerializer.DefaultOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var loaded = MorphConfiguration.Load(stream);

        // Corruption biome should have a sprite offset entry to shift from Crimson frames to Corruption frames
        var corruptionTiles = loaded.Biomes["Corruption"].MorphTiles;
        corruptionTiles.Count.ShouldBe(1);
        corruptionTiles[0].SourceIds.ShouldContain((ushort)26);
        corruptionTiles[0].SpriteOffsets.Count.ShouldBe(1);

        var offset = corruptionTiles[0].SpriteOffsets[0];
        offset.MinU.ShouldBe((short)54);   // Crimson frame starts at U=54
        offset.MaxU.ShouldBe((short)107);  // 54 + 54 - 1
        offset.OffsetU.ShouldBe((short)-54); // shift left to U=0 (Corruption)
        offset.UseFilterV.ShouldBe(true);
        offset.MinV.ShouldBe((short)0);
        offset.MaxV.ShouldBe((short)35);

        // Crimson biome: offset from Corruption (U=0..53) to Crimson (U=54)
        var crimsonTiles = loaded.Biomes["Crimson"].MorphTiles;
        crimsonTiles[0].SpriteOffsets[0].MinU.ShouldBe((short)0);
        crimsonTiles[0].SpriteOffsets[0].MaxU.ShouldBe((short)53);
        crimsonTiles[0].SpriteOffsets[0].OffsetU.ShouldBe((short)54);
    }

    [Fact]
    public void ExpandGroups_MixedGroup_GeneratesBothSwapAndOffset()
    {
        var config = new MorphConfiguration
        {
            Biomes = new Dictionary<string, MorphBiomeData>
            {
                ["Corruption"] = new() { Name = "Corruption" },
                ["Crimson"] = new() { Name = "Crimson" },
                ["Hallow"] = new() { Name = "Hallow" },
            },
            MorphGroups =
            [
                new MorphGroup
                {
                    Name = "Altars",
                    Category = "tile",
                    Variants = new()
                    {
                        ["Corruption"] = new() { TileId = 26, FrameU = 0, FrameV = 0, FrameWidth = 54, FrameHeight = 36 },
                        ["Crimson"] = new() { TileId = 26, FrameU = 54, FrameV = 0, FrameWidth = 54, FrameHeight = 36 },
                        ["Hallow"] = new() { TileId = 133 },
                    },
                },
            ],
        };

        var json = JsonSerializer.Serialize(config, TEditJsonSerializer.DefaultOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var loaded = MorphConfiguration.Load(stream);

        // Hallow should get a tile-ID swap rule from tiles 26 to 133
        var hallowTiles = loaded.Biomes["Hallow"].MorphTiles;
        hallowTiles.ShouldContain(m => m.SourceIds.Contains((ushort)26) && m.Default.SkyId == 133);

        // Corruption should get both:
        // 1. tile-ID swap from 133 to 26
        // 2. sprite offset from tile 26 crimson frames to corruption frames
        var corruptionTiles = loaded.Biomes["Corruption"].MorphTiles;
        corruptionTiles.ShouldContain(m => m.SourceIds.Contains((ushort)133) && m.Default.SkyId == 26);
        corruptionTiles.ShouldContain(m => m.SourceIds.Contains((ushort)26) && m.SpriteOffsets.Count > 0);
    }

    [Fact]
    public void ExpandGroups_DeleteVariant_SetsDeleteFlag()
    {
        var config = new MorphConfiguration
        {
            Biomes = new Dictionary<string, MorphBiomeData>
            {
                ["Corruption"] = new() { Name = "Corruption" },
                ["Purify"] = new() { Name = "Purify" },
            },
            MorphGroups =
            [
                new MorphGroup
                {
                    Name = "Thorns",
                    Category = "tile",
                    Variants = new()
                    {
                        ["Corruption"] = new() { TileId = 32 },
                        ["Purify"] = new() { Delete = true },
                    },
                },
            ],
        };

        var json = JsonSerializer.Serialize(config, TEditJsonSerializer.DefaultOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var loaded = MorphConfiguration.Load(stream);

        // Purify should delete tile 32
        var purifyTiles = loaded.Biomes["Purify"].MorphTiles;
        purifyTiles.Count.ShouldBe(1);
        purifyTiles[0].Delete.ShouldBe(true);
        purifyTiles[0].SourceIds.ShouldContain((ushort)32);
    }

    [Fact]
    public void ExpandGroups_HandAuthoredRulesTakePrecedence()
    {
        var config = new MorphConfiguration
        {
            Biomes = new Dictionary<string, MorphBiomeData>
            {
                ["Corruption"] = new()
                {
                    Name = "Corruption",
                    MorphTiles =
                    [
                        new MorphId
                        {
                            Name = "existingVineRule",
                            SourceIds = [52, 205], // already claims tile 52 and 205
                            Default = new MorphIdLevels { SkyId = 636 },
                        },
                    ],
                },
                ["Purify"] = new() { Name = "Purify" },
            },
            MorphGroups =
            [
                new MorphGroup
                {
                    Name = "Vines",
                    Category = "tile",
                    Variants = new()
                    {
                        ["Corruption"] = new() { TileId = 636 },
                        ["Crimson"] = new() { TileId = 205 },
                        ["Purify"] = new() { TileId = 52 },
                    },
                },
            ],
        };

        var json = JsonSerializer.Serialize(config, TEditJsonSerializer.DefaultOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var loaded = MorphConfiguration.Load(stream);

        // Corruption: hand-authored rule claims 52 and 205, so group should NOT add duplicates
        var corruptionTiles = loaded.Biomes["Corruption"].MorphTiles;
        // Should still be just 1 rule (the hand-authored one)
        corruptionTiles.Count.ShouldBe(1);
        corruptionTiles[0].Name.ShouldBe("existingVineRule");

        // Purify: no hand-authored rules, so group should generate one
        var purifyTiles = loaded.Biomes["Purify"].MorphTiles;
        purifyTiles.Count.ShouldBe(1);
        purifyTiles[0].SourceIds.ShouldContain((ushort)636);
    }

    [Fact]
    public void ExpandGroups_MissingBiome_SkipsGracefully()
    {
        var config = new MorphConfiguration
        {
            Biomes = new Dictionary<string, MorphBiomeData>
            {
                ["Corruption"] = new() { Name = "Corruption" },
            },
            MorphGroups =
            [
                new MorphGroup
                {
                    Name = "Vines",
                    Category = "tile",
                    Variants = new()
                    {
                        ["Corruption"] = new() { TileId = 636 },
                        ["Jungle"] = new() { TileId = 62 },
                    },
                },
            ],
        };

        var json = JsonSerializer.Serialize(config, TEditJsonSerializer.DefaultOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var loaded = MorphConfiguration.Load(stream);

        // Corruption should get a rule for tile 62 -> 636 (Jungle exists in group but not in biomes)
        var corruptionTiles = loaded.Biomes["Corruption"].MorphTiles;
        corruptionTiles.Count.ShouldBe(1);
        corruptionTiles[0].SourceIds.ShouldContain((ushort)62);
    }

    [Fact]
    public void ExpandGroups_DifferentTileIds_SetsSpriteReplacement()
    {
        var config = new MorphConfiguration
        {
            Biomes = new Dictionary<string, MorphBiomeData>
            {
                ["Corruption"] = new() { Name = "Corruption" },
                ["Hallow"] = new() { Name = "Hallow" },
            },
            MorphGroups =
            [
                new MorphGroup
                {
                    Name = "Altars",
                    Category = "tile",
                    Variants = new()
                    {
                        ["Corruption"] = new() { TileId = 26 },
                        ["Hallow"] = new() { TileId = 133 },
                    },
                },
            ],
        };

        var json = JsonSerializer.Serialize(config, TEditJsonSerializer.DefaultOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var loaded = MorphConfiguration.Load(stream);

        // Corruption biome: should convert tile 133 -> 26, with SpriteReplacement set
        var corruptionTiles = loaded.Biomes["Corruption"].MorphTiles;
        corruptionTiles.Count.ShouldBe(1);
        corruptionTiles[0].SpriteReplacement.ShouldNotBeNull();
        corruptionTiles[0].SpriteReplacement!.TargetTileId.ShouldBe((ushort)26);

        // Hallow biome: should convert tile 26 -> 133, with SpriteReplacement set
        var hallowTiles = loaded.Biomes["Hallow"].MorphTiles;
        hallowTiles.Count.ShouldBe(1);
        hallowTiles[0].SpriteReplacement.ShouldNotBeNull();
        hallowTiles[0].SpriteReplacement!.TargetTileId.ShouldBe((ushort)133);
    }

    [Fact]
    public void ExpandGroups_DeleteVariant_DoesNotSetSpriteReplacement()
    {
        var config = new MorphConfiguration
        {
            Biomes = new Dictionary<string, MorphBiomeData>
            {
                ["Corruption"] = new() { Name = "Corruption" },
                ["Purify"] = new() { Name = "Purify" },
            },
            MorphGroups =
            [
                new MorphGroup
                {
                    Name = "Thorns",
                    Category = "tile",
                    Variants = new()
                    {
                        ["Corruption"] = new() { TileId = 32 },
                        ["Purify"] = new() { Delete = true },
                    },
                },
            ],
        };

        var json = JsonSerializer.Serialize(config, TEditJsonSerializer.DefaultOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var loaded = MorphConfiguration.Load(stream);

        // Delete variant should NOT have SpriteReplacement
        var purifyTiles = loaded.Biomes["Purify"].MorphTiles;
        purifyTiles[0].SpriteReplacement.ShouldBeNull();
        purifyTiles[0].Delete.ShouldBe(true);
    }
}
