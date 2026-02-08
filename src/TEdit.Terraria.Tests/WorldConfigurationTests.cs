using Xunit;
using TEdit.Terraria;
using System.Text.Json;
using TEdit.Geometry;
using TEdit.Common.Serialization;

namespace TEdit.Terraria.Tests;

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
}
