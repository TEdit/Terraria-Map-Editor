using TEdit.Render;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Render;

public class ContributionPaintTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnknownPaint_UsesUnpaintedColorWithoutChangingStoredId(bool wall)
    {
        WorldConfiguration.Initialize();
        int count = WorldConfiguration.PaintProperties.Count;
        Assert.InRange(count, 1, 255);
        foreach (byte paint in new[] { (byte)count, byte.MaxValue })
        {
            var tile = new Tile { IsActive = !wall, Type = 1, Wall = 1 };
            var expected = PixelMap.GetTileColor(tile, default, showTile: !wall);
            if (wall) tile.WallColor = paint;
            else tile.TileColor = paint;
            Assert.Equal(expected, PixelMap.GetTileColor(tile, default, showTile: !wall));
            Assert.Equal(paint, wall ? tile.WallColor : tile.TileColor);
        }
    }
}
