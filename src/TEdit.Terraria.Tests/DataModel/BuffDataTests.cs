using System.Linq;
using Shouldly;
using TEdit.Geometry;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests.DataModel;

/// <summary>
/// Tests to verify buff radius data is loaded correctly from tiles.json for passive buff tiles.
/// </summary>
[Collection("SharedState")]
public class BuffDataTests : IDisposable
{
    public BuffDataTests()
    {
        WorldConfiguration.Reset();
        TerrariaDataStore.Reset();
        WorldConfiguration.Initialize();
    }

    public void Dispose()
    {
        WorldConfiguration.Reset();
        TerrariaDataStore.Reset();
    }

    [Theory]
    [InlineData(27, "Happy!")]           // Sunflower
    [InlineData(49, "Water Candle")]      // Water Candle
    [InlineData(215, "Cozy Fire")]        // Campfires
    [InlineData(372, "Peace Candle")]     // Peace Candle
    [InlineData(405, "Cozy Fire")]        // Fireplace
    [InlineData(506, "Bast Defense")]     // Bast Statue
    [InlineData(567, "Luck")]            // Garden Gnome
    [InlineData(646, "Shadow Candle")]    // Shadow Candle
    public void TileLevelBuffTiles_HaveValidBuffRadius(int tileId, string expectedBuffName)
    {
        var tile = WorldConfiguration.TileProperties[tileId];

        tile.BuffRadius.ShouldNotBeNull();
        tile.BuffRadius!.Value.ShouldBe(new Vector2Short(85, 62));
        tile.BuffName.ShouldBe(expectedBuffName);
        tile.BuffColor.ShouldNotBeNull();
    }

    [Fact]
    public void Tile42_HeartLanternOn_HasFrameLevelBuff()
    {
        var tile = WorldConfiguration.TileProperties[42];
        var heartLanternOn = tile.Frames!.First(f => f.Name == "Heart Lantern" && f.Variety == "On");

        heartLanternOn.BuffRadius.ShouldNotBeNull();
        heartLanternOn.BuffRadius!.Value.ShouldBe(new Vector2Short(85, 62));
        heartLanternOn.BuffName.ShouldBe("Heart Lamp");
        heartLanternOn.BuffColor.ShouldNotBeNull();
    }

    [Fact]
    public void Tile42_HeartLanternOff_HasNoFrameLevelBuff()
    {
        var tile = WorldConfiguration.TileProperties[42];
        var heartLanternOff = tile.Frames!.First(f => f.Name == "Heart Lantern" && f.Variety == "Off");

        heartLanternOff.BuffRadius.ShouldBeNull();
    }

    [Fact]
    public void Tile42_StarInABottleOn_HasFrameLevelBuff()
    {
        var tile = WorldConfiguration.TileProperties[42];
        var starOn = tile.Frames!.First(f => f.Name == "Star in a Bottle" && f.Variety == "On");

        starOn.BuffRadius.ShouldNotBeNull();
        starOn.BuffRadius!.Value.ShouldBe(new Vector2Short(85, 62));
        starOn.BuffName.ShouldBe("Star in a Bottle");
        starOn.BuffColor.ShouldNotBeNull();
    }

    [Fact]
    public void Tile42_StarInABottleOff_HasNoFrameLevelBuff()
    {
        var tile = WorldConfiguration.TileProperties[42];
        var starOff = tile.Frames!.First(f => f.Name == "Star in a Bottle" && f.Variety == "Off");

        starOff.BuffRadius.ShouldBeNull();
    }

    [Fact]
    public void Tile42_OtherLanterns_HaveNoTileLevelBuff()
    {
        var tile = WorldConfiguration.TileProperties[42];

        // Tile 42 itself should not have a tile-level buff (only specific frames do)
        tile.BuffRadius.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]   // Dirt
    [InlineData(1)]   // Stone
    [InlineData(2)]   // Grass
    [InlineData(30)]  // Wood
    public void NonBuffTiles_HaveNullBuffRadius(int tileId)
    {
        var tile = WorldConfiguration.TileProperties[tileId];

        tile.BuffRadius.ShouldBeNull();
        tile.BuffName.ShouldBeNull();
        tile.BuffColor.ShouldBeNull();
    }
}
