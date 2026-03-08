using System.Text;
using System.Text.Json;
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
