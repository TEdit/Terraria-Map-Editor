using Xunit;
using TEdit.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using TEdit.Geometry;
using TEdit.Common;
using TEdit.Common.Serialization;

namespace TEdit.Configuration.Tests;

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
