using System.Text.Json;
using Shouldly;
using TEdit.Common;
using TEdit.Common.Serialization;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests.DataModel;

public class WallPropertyTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void RoundTrip_PreservesAllProperties()
    {
        var original = new WallProperty
        {
            Id = 1,
            Name = "Stone Wall",
            Color = TEditColor.FromString("#FF787878"),
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<WallProperty>(json, Options)!;

        restored.Id.ShouldBe(1);
        restored.Name.ShouldBe("Stone Wall");
        restored.Color.ShouldBe(original.Color);
    }

    [Fact]
    public void Defaults_AreCorrect()
    {
        var wall = new WallProperty();
        wall.Id.ShouldBe(-1);
        wall.Name.ShouldBe("UNKNOWN");
        wall.Color.ShouldBe(TEditColor.Magenta);
    }
}
