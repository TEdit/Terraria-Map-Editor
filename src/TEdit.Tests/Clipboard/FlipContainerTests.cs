using TEdit.Editor.Clipboard;
using TEdit.Geometry;
using TEdit.Terraria;
using Shouldly;
using Xunit;

namespace TEdit.Tests.Clipboard;

public class FlipContainerTests
{
    [Fact]
    public void Flip_Dresser_KeepsChestOnTheSpriteAnchor()
    {
        // 5x2 buffer with a 3x2 dresser at x 0..2.
        var buffer = new ClipboardBuffer(new Vector2Int32(5, 2));
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 2; y++)
                buffer.Tiles[x, y] = x < 3
                    ? new Tile { IsActive = true, Type = (ushort)TileType.Dresser, U = (short)(x * 18), V = (short)(y * 18) }
                    : new Tile();
        buffer.Chests.Add(new Chest(0, 0));

        var flipped = ClipboardBuffer.Flip(buffer, flipX: true, rotate: false);

        // Mirrored, the dresser spans x 2..4, so its chest anchors at x 2 (a 2x2 flip put it at 3).
        flipped.Chests[0].X.ShouldBe(2);
        flipped.Chests[0].Y.ShouldBe(0);
    }
}
