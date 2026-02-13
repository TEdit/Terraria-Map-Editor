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
        var original = new NpcData { Id = 17, Name = "Merchant"};
        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<NpcData>(json, Options)!;

        restored.Id.ShouldBe(17);
        restored.Name.ShouldBe("Merchant");
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

public class BackgroundStyleTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void RoundTrip_SimpleBackground()
    {
        var original = new BackgroundStyle { Id = 0, Name = "Default", Textures = [12, 13, 14] };
        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<BackgroundStyle>(json, Options)!;

        restored.Id.ShouldBe(0);
        restored.Name.ShouldBe("Default");
        restored.Textures.ShouldBe([12, 13, 14]);
    }

    [Fact]
    public void RoundTrip_DualArrayBackground()
    {
        var original = new BackgroundStyle
        {
            Id = 0,
            Name = "Default",
            Textures = [9, 10, 11],
            SecondaryTextures = [7, 8]
        };
        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<BackgroundStyle>(json, Options)!;

        restored.Textures.ShouldBe([9, 10, 11]);
        restored.SecondaryTextures.ShouldBe([7, 8]);
    }

    [Fact]
    public void RoundTrip_DesertWithVariants()
    {
        var original = new BackgroundStyle
        {
            Id = 51,
            Name = "Multi-Biome A",
            Textures = [306, 303, -1],
            CorruptTextures = [310, 307, -1],
            HallowTextures = [314, 311, -1],
            CrimsonTextures = [318, 315, -1]
        };
        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<BackgroundStyle>(json, Options)!;

        restored.CorruptTextures.ShouldBe([310, 307, -1]);
        restored.HallowTextures.ShouldBe([314, 311, -1]);
        restored.CrimsonTextures.ShouldBe([318, 315, -1]);
    }

    [Fact]
    public void GetPreviewTextureIndex_ReturnsFirstValidTexture()
    {
        var bg = new BackgroundStyle { Textures = [12, 13, 14] };
        bg.GetPreviewTextureIndex().ShouldBe(12);
    }

    [Fact]
    public void GetPreviewTextureIndex_SkipsNegativeTextures()
    {
        var bg = new BackgroundStyle { Textures = [-1, -1, -1], SecondaryTextures = [93, 94] };
        bg.GetPreviewTextureIndex().ShouldBe(93);
    }
}

public class BackgroundStyleConfigurationTests
{
    [Fact]
    public void Load_FromEmbeddedResource_LoadsAllBiomes()
    {
        using var stream = typeof(BackgroundStyleConfiguration).Assembly
            .GetManifestResourceStream("TEdit.Terraria.Data.backgroundStyles.json")!;

        var config = BackgroundStyleConfiguration.Load(stream);

        config.Version.ShouldBe("1.4.5.4");
        config.TreeStyles.Count.ShouldBe(6);
        config.ForestBackgrounds.Count.ShouldBe(19);
        config.CorruptionBackgrounds.Count.ShouldBe(7);
        config.JungleBackgrounds.Count.ShouldBe(7);
        config.SnowBackgrounds.Count.ShouldBe(15);
        config.HallowBackgrounds.Count.ShouldBe(6);
        config.CrimsonBackgrounds.Count.ShouldBe(7);
        config.DesertBackgrounds.Count.ShouldBe(8);
        config.OceanBackgrounds.Count.ShouldBe(8);
        config.MushroomBackgrounds.Count.ShouldBe(5);
        config.UnderworldBackgrounds.Count.ShouldBe(3);
        config.CaveBackgrounds.Count.ShouldBe(8);
        config.IceBackgrounds.Count.ShouldBe(4);
        config.JungleUndergroundBackgrounds.Count.ShouldBe(2);
        config.HellBackgrounds.Count.ShouldBe(3);
    }

    [Fact]
    public void Load_BuildsIndexes()
    {
        using var stream = typeof(BackgroundStyleConfiguration).Assembly
            .GetManifestResourceStream("TEdit.Terraria.Data.backgroundStyles.json")!;

        var config = BackgroundStyleConfiguration.Load(stream);

        config.TreeStyleById[0].Name.ShouldBe("Oak");
        config.ForestBackgroundById[0].Name.ShouldBe("Default");
        config.CorruptionBackgroundById[51].Name.ShouldBe("Remix 1");
        config.CaveBackgroundById[7].UndergroundIndex.ShouldBe(10);
        config.HellBackgroundById[0].BottomTexture.ShouldBe(185);
    }
}
