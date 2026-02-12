using System.Text.Json;
using Shouldly;
using TEdit.Common;
using TEdit.Common.Serialization;
using TEdit.Geometry;
using TEdit.Terraria.DataModel;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests.DataModel;

public class NpcDataTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void RoundTrip_PreservesAllProperties()
    {
        var original = new NpcData { Id = 17, Name = "Merchant", Size = new Vector2Short(18, 40) };
        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<NpcData>(json, Options)!;

        restored.Id.ShouldBe(17);
        restored.Name.ShouldBe("Merchant");
        restored.Size.X.ShouldBe((short)18);
        restored.Size.Y.ShouldBe((short)40);
    }
}

public class PaintPropertyTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void RoundTrip_PreservesAllProperties()
    {
        var original = new PaintProperty { Id = 1, Name = "Red Paint", Color = TEditColor.FromString("#FFFF0000") };
        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<PaintProperty>(json, Options)!;

        restored.Id.ShouldBe(1);
        restored.Name.ShouldBe("Red Paint");
        restored.Color.R.ShouldBe((byte)255);
    }

    [Fact]
    public void Defaults_AreCorrect()
    {
        var paint = new PaintProperty();
        paint.Id.ShouldBe(-1);
        paint.Name.ShouldBe("UNKNOWN");
    }
}

public class PrefixDataTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void RoundTrip_PreservesAllProperties()
    {
        var original = new PrefixData { Id = 1, Name = "Large" };
        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<PrefixData>(json, Options)!;

        restored.Id.ShouldBe(1);
        restored.Name.ShouldBe("Large");
    }
}

public class GlobalColorEntryTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void RoundTrip_PreservesAllProperties()
    {
        var original = new GlobalColorEntry { Name = "Sky", Color = TEditColor.FromString("#FF87CEEB") };
        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<GlobalColorEntry>(json, Options)!;

        restored.Name.ShouldBe("Sky");
        restored.Color.ShouldBe(original.Color);
    }
}

public class ChestPropertyTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void RoundTrip_PreservesAllProperties()
    {
        var original = new ChestProperty
        {
            ChestId = 0,
            Name = "Chest",
            UV = new Vector2Short(0, 0),
            Size = new Vector2Short(2, 2),
            TileType = 21,
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<ChestProperty>(json, Options)!;

        restored.ChestId.ShouldBe(0);
        restored.Name.ShouldBe("Chest");
        restored.TileType.ShouldBe((ushort)21);
    }
}

public class SignPropertyTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void RoundTrip_PreservesAllProperties()
    {
        var original = new SignProperty
        {
            SignId = 0,
            Name = "Sign Left Bottom",
            UV = new Vector2Short(0, 0),
            TileType = 55,
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<SignProperty>(json, Options)!;

        restored.SignId.ShouldBe(0);
        restored.Name.ShouldBe("Sign Left Bottom");
        restored.TileType.ShouldBe((ushort)55);
    }
}

public class BestiaryNpcDataTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void RoundTrip_PreservesAllProperties()
    {
        var original = new BestiaryNpcData
        {
            Id = 1,
            BannerId = 1,
            FullName = "Blue Slime",
            Name = "Blue Slime",
            BestiaryId = "Slime",
            CanTalk = false,
            IsCritter = false,
            IsTownNpc = false,
            IsKillCredit = true,
            BestiaryDisplayIndex = 1,
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<BestiaryNpcData>(json, Options)!;

        restored.Id.ShouldBe(1);
        restored.BestiaryId.ShouldBe("Slime");
        restored.IsKillCredit.ShouldBe(true);
    }
}

public class SaveVersionDataTests
{
    [Fact]
    public void GetFrames_ReturnsCorrectBoolArray()
    {
        var data = new SaveVersionData
        {
            MaxTileId = 5,
            FramedTileIds = [1, 3, 5],
        };

        var frames = data.GetFrames();

        frames.Length.ShouldBe(6);
        frames[0].ShouldBe(false);
        frames[1].ShouldBe(true);
        frames[2].ShouldBe(false);
        frames[3].ShouldBe(true);
        frames[4].ShouldBe(false);
        frames[5].ShouldBe(true);
    }
}
