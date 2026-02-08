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
}
