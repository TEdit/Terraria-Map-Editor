using System.Text.Json;
using TEdit.Geometry;
using TEdit.Common.Serialization;
using Shouldly;

namespace TEdit.Terraria.Tests;

[Collection("SharedState")]
public class WorldConfigurationTests
{

    [Fact()]
    public void SerializeAsJson()
    {
        var json = JsonSerializer.Serialize(
            WorldConfiguration.TileProperties,
            options: TEditJsonSerializer.DefaultOptions);
    }

    [Fact()]
    public void DeserializeVector2Short()
    {
        var vector = JsonSerializer.Deserialize<Vector2Short>(
            "[1,2]", 
            options: TEditJsonSerializer.DefaultOptions);
    }

    [Fact]
    public void Terraria1458_UsesVersion326ConfigurationPayload()
    {
        WorldConfiguration.Reset();

        try
        {
            WorldConfiguration.Initialize();

            WorldConfiguration.CompatibleVersion.ShouldBe(326u);
            WorldConfiguration.ApplyForWorldVersion(326, out uint configVersion).ShouldBeTrue();
            configVersion.ShouldBe(326u);
            WorldConfiguration.ActiveWorldVersion.ShouldBe(326u);
            WorldConfiguration.ActiveConfigVersion.ShouldBe(326u);
        }
        finally
        {
            WorldConfiguration.Reset();
            WorldConfiguration.Initialize();
        }
    }
}
