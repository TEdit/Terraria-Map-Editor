using TEdit.Render;
using TEdit.Terraria;
using Shouldly;
using Xunit;

namespace TEdit.Tests.Render;

public class PixelMapPaintTests
{
    [Fact]
    public void GetTileColor_PaintIdPastTheTable_DoesNotThrow()
    {
        byte outOfRange = (byte)WorldConfiguration.PaintProperties.Count;
        var tile = new Tile { IsActive = true, Type = 1, Wall = 1, TileColor = outOfRange, WallColor = outOfRange };

        Should.NotThrow(() => PixelMap.GetTileColor(tile, default));
        Should.NotThrow(() => PixelMap.GetTileColor(tile, default, showTile: false));
    }
}
