using System.Text.Json;
using Shouldly;
using TEdit.Common.Serialization;
using TEdit.Geometry;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests.DataModel;

public class FramePropertyTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void RoundTrip_PreservesAllProperties()
    {
        var original = new FrameProperty
        {
            Name = "Torch",
            Variety = "Blue",
            UV = new Vector2Short(22, 0),
            Size = new Vector2Short(1, 1),
            Anchor = FrameAnchor.Left,
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<FrameProperty>(json, Options)!;

        restored.Name.ShouldBe("Torch");
        restored.Variety.ShouldBe("Blue");
        restored.UV.X.ShouldBe((short)22);
        restored.UV.Y.ShouldBe((short)0);
        restored.Size.X.ShouldBe((short)1);
        restored.Anchor.ShouldBe(FrameAnchor.Left);
    }

    [Fact]
    public void Anchor_SerializesAsString()
    {
        var frame = new FrameProperty { Anchor = FrameAnchor.Bottom };
        var json = JsonSerializer.Serialize(frame, Options);
        json.ShouldContain("\"Bottom\"");
    }

    [Fact]
    public void ToString_WithVarietyAndAnchor()
    {
        var frame = new FrameProperty { Name = "Torch", Variety = "Blue", Anchor = FrameAnchor.Left };
        frame.ToString().ShouldBe("Torch: Blue [Left]");
    }

    [Fact]
    public void ToString_NameOnly()
    {
        var frame = new FrameProperty { Name = "Default" };
        frame.ToString().ShouldBe("Default");
    }
}
