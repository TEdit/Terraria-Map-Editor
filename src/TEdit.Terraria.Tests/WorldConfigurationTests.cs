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
    public void Terraria1460_UsesVersion329ConfigurationPayload()
    {
        WorldConfiguration.Reset();

        try
        {
            WorldConfiguration.Initialize();

            WorldConfiguration.CompatibleVersion.ShouldBe(329u);
            WorldConfiguration.ApplyForWorldVersion(329, out uint configVersion).ShouldBeTrue();
            configVersion.ShouldBe(329u);
            WorldConfiguration.ActiveWorldVersion.ShouldBe(329u);
            WorldConfiguration.ActiveConfigVersion.ShouldBe(329u);
        }
        finally
        {
            WorldConfiguration.Reset();
            WorldConfiguration.Initialize();
        }
    }
}
