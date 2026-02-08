using System.Text.Json;
using Shouldly;
using TEdit.Common;
using TEdit.Common.Serialization;
using TEdit.Geometry;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests.DataModel;

public class TilePropertyTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void Serialize_NonFramedTile_ProducesExpectedJson()
    {
        var tile = new TileProperty
        {
            Id = 0,
            Name = "Dirt Block",
            Color = TEditColor.FromString("#FF976B4B"),
            IsSolid = true,
            CanBlend = true,
        };

        var json = JsonSerializer.Serialize(tile, Options);

        json.ShouldContain("\"name\": \"Dirt Block\"");
        json.ShouldContain("\"isSolid\": true");
        json.ShouldContain("\"canBlend\": true");
        json.ShouldNotContain("\"isFramed\""); // false is default, omitted
    }

    [Fact]
    public void RoundTrip_NonFramedTile_PreservesAllProperties()
    {
        var original = new TileProperty
        {
            Id = 1,
            Name = "Stone Block",
            Color = TEditColor.FromString("#FF808080"),
            IsSolid = true,
            IsStone = true,
            CanBlend = true,
            MergeWith = 0,
            TextureGrid = new Vector2Short(16, 16),
            FrameGap = new Vector2Short(2, 2),
            FrameSize = [new Vector2Short(1, 1)],
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<TileProperty>(json, Options)!;

        restored.Id.ShouldBe(original.Id);
        restored.Name.ShouldBe(original.Name);
        restored.Color.ShouldBe(original.Color);
        restored.IsSolid.ShouldBe(true);
        restored.IsStone.ShouldBe(true);
        restored.CanBlend.ShouldBe(true);
        restored.MergeWith.ShouldBe(0);
        restored.IsFramed.ShouldBe(false);
        restored.Frames.ShouldBeNull();
    }

    [Fact]
    public void RoundTrip_FramedTileWithFrames_PreservesFrameData()
    {
        var original = new TileProperty
        {
            Id = 4,
            Name = "Torch",
            Color = TEditColor.FromString("#FFFF0000"),
            IsFramed = true,
            IsLight = true,
            FrameSize = [new Vector2Short(1, 1)],
            Frames =
            [
                new FrameProperty { Name = "Torch", UV = new Vector2Short(0, 0), Anchor = FrameAnchor.Bottom },
                new FrameProperty { Name = "Torch", Variety = "Blue", UV = new Vector2Short(22, 0), Anchor = FrameAnchor.Left },
            ],
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<TileProperty>(json, Options)!;

        restored.IsFramed.ShouldBe(true);
        restored.IsLight.ShouldBe(true);
        restored.Frames.ShouldNotBeNull();
        restored.Frames!.Count.ShouldBe(2);
        restored.Frames[0].Anchor.ShouldBe(FrameAnchor.Bottom);
        restored.Frames[1].Variety.ShouldBe("Blue");
        restored.Frames[1].Anchor.ShouldBe(FrameAnchor.Left);
    }

    [Fact]
    public void HasSlopes_IsSolidOrSaveSlope()
    {
        new TileProperty { IsSolid = true }.HasSlopes.ShouldBe(true);
        new TileProperty { SaveSlope = true }.HasSlopes.ShouldBe(true);
        new TileProperty().HasSlopes.ShouldBe(false);
    }

    [Fact]
    public void Placement_SerializesAsString()
    {
        var tile = new TileProperty { Placement = FramePlacement.Floor | FramePlacement.Surface };

        var json = JsonSerializer.Serialize(tile, Options);

        json.ShouldContain("\"FloorSurface\"");
    }
}
