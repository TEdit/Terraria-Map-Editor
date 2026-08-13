using Shouldly;
using TEdit.Editor;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Terraria.Tests.Editor;

public class MorphVineSupportTests
{
    [Theory]
    [InlineData(52)]
    [InlineData(62)]
    [InlineData(115)]
    [InlineData(205)]
    [InlineData(382)]
    [InlineData(528)]
    [InlineData(636)]
    [InlineData(638)]
    public void AirBelow_TreatsVinesAsUnsupportedSpace(ushort vineTileId)
    {
        WorldConfiguration.TileProperties[vineTileId].IsSolid.ShouldBeFalse();

        var world = new World(3, 3, "Morph vine support test", 1);
        world.Tiles = new Tile[3, 3];
        world.Tiles[1, 2].IsActive = true;
        world.Tiles[1, 2].Type = vineTileId;

        MorphBiomeDataApplier.AirBelow(world, 1, 1).ShouldBeTrue();
    }
}
