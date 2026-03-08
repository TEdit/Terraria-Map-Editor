using TEdit.Terraria;
using TEdit.Terraria.Objects;

namespace TEdit.Editor;

public static class SpritePlacer
{
    /// <summary>
    /// Place a sprite on the tile grid. Sets Type, U, V for each tile in the sprite footprint.
    /// </summary>
    public static void Place(SpriteItem sprite, int destinationX, int destinationY, ITileData world)
    {
        var tiles = sprite.GetTiles();

        for (int x = 0; x < sprite.SizeTiles.X; x++)
        {
            int tilex = x + destinationX;
            for (int y = 0; y < sprite.SizeTiles.Y; y++)
            {
                int tiley = y + destinationY;
                Tile curtile = world.Tiles[tilex, tiley];
                curtile.IsActive = true;
                curtile.Type = sprite.Tile;
                curtile.U = tiles[x, y].X;
                curtile.V = tiles[x, y].Y;
                world.Tiles[tilex, tiley] = curtile;
            }
        }
    }

    /// <summary>
    /// Clear all tiles in a sprite footprint.
    /// </summary>
    public static void ClearSprite(int anchorX, int anchorY, int sizeX, int sizeY, ITileData world)
    {
        for (int x = 0; x < sizeX; x++)
        {
            int tilex = x + anchorX;
            for (int y = 0; y < sizeY; y++)
            {
                int tiley = y + anchorY;
                Tile tile = world.Tiles[tilex, tiley];
                tile.ClearTile();
                world.Tiles[tilex, tiley] = tile;
            }
        }
    }
}
