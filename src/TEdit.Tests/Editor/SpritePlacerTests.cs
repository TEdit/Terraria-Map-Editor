using Shouldly;
using TEdit.Editor.Clipboard;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.Editor;
using TEdit.Terraria.Objects;
using Xunit;

namespace TEdit.Tests.Editor;

/// <summary>
/// Regression tests for SpritePlacer after Tile class-to-struct conversion.
/// When Tile is a struct, reading from the array gives a copy — mutations must
/// be written back or changes are silently lost.
/// </summary>
public class SpritePlacerTests
{
    /// <summary>
    /// Tests the ChristmasTree code path of Place(ITileData).
    /// Verifies tiles are actually written back to the array.
    /// </summary>
    [Fact]
    public void Place_ChristmasTree_WritesTilesToWorld()
    {
        // Arrange — 4x4 buffer with empty tiles
        var size = new Vector2Int32(10, 10);
        var buffer = new ClipboardBuffer(size, initTiles: true);

        var sprite = new SpriteItem
        {
            Tile = (ushort)TileType.ChristmasTree,
            SizeTiles = new Vector2Short(4, 4),
            SizePixelsInterval = new Vector2Short(18, 18),
            UV = new Vector2Short(0, 0),
        };

        int destX = 2, destY = 2;

        // Act
        sprite.Place(destX, destY, (ITileData)buffer);

        // Assert — every tile in the placed area should be active with the correct type
        for (int x = 0; x < sprite.SizeTiles.X; x++)
        {
            for (int y = 0; y < sprite.SizeTiles.Y; y++)
            {
                var tile = buffer.Tiles[x + destX, y + destY];
                tile.IsActive.ShouldBeTrue($"Tile at ({x + destX},{y + destY}) should be active");
                tile.Type.ShouldBe((ushort)TileType.ChristmasTree,
                    $"Tile at ({x + destX},{y + destY}) should have ChristmasTree type");
            }
        }

        // Verify origin tile has special U=10
        buffer.Tiles[destX, destY].U.ShouldBe((short)10, "Origin tile U should be 10 for ChristmasTree");
    }

    /// <summary>
    /// Tests the normal (non-ChristmasTree) code path of Place(ITileData).
    /// Uses a simple tile type to verify UV values are written through.
    /// </summary>
    [Fact]
    public void Place_NormalSprite_WritesTilesToWorld()
    {
        // Arrange
        var size = new Vector2Int32(10, 10);
        var buffer = new ClipboardBuffer(size, initTiles: true);

        ushort tileType = 15; // A simple tile type (Anvil-ish)
        var sprite = new SpriteItem
        {
            Tile = tileType,
            SizeTiles = new Vector2Short(2, 2),
            SizePixelsInterval = new Vector2Short(18, 18),
            UV = new Vector2Short(0, 0),
        };

        int destX = 3, destY = 3;

        // Act
        sprite.Place(destX, destY, (ITileData)buffer);

        // Assert — every tile should be active with correct type
        for (int x = 0; x < sprite.SizeTiles.X; x++)
        {
            for (int y = 0; y < sprite.SizeTiles.Y; y++)
            {
                var tile = buffer.Tiles[x + destX, y + destY];
                tile.IsActive.ShouldBeTrue($"Tile at ({x + destX},{y + destY}) should be active");
                tile.Type.ShouldBe(tileType,
                    $"Tile at ({x + destX},{y + destY}) should have type {tileType}");
            }
        }
    }

    /// <summary>
    /// Verifies that tiles outside the sprite placement area are NOT modified.
    /// </summary>
    [Fact]
    public void Place_DoesNotModifyTilesOutsidePlacement()
    {
        // Arrange
        var size = new Vector2Int32(10, 10);
        var buffer = new ClipboardBuffer(size, initTiles: true);

        var sprite = new SpriteItem
        {
            Tile = (ushort)TileType.ChristmasTree,
            SizeTiles = new Vector2Short(2, 2),
            SizePixelsInterval = new Vector2Short(18, 18),
            UV = new Vector2Short(0, 0),
        };

        int destX = 4, destY = 4;

        // Act
        sprite.Place(destX, destY, (ITileData)buffer);

        // Assert — tile at (0,0) should still be inactive
        buffer.Tiles[0, 0].IsActive.ShouldBeFalse("Tile outside placement should remain inactive");
        buffer.Tiles[3, 3].IsActive.ShouldBeFalse("Adjacent tile should remain inactive");
    }
}
