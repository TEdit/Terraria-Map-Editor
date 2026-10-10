using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.Objects;

namespace TEdit.Editor;

public static class SpritePlacer
{
    /// <summary>
    /// World-space footprint of the multi-tile sprite (chest, table, painting...) covering a tile,
    /// clipped to the world. Null for single tiles, plain blocks and empty tiles.
    /// </summary>
    public static RectangleInt32? GetSpriteBounds(World world, int x, int y)
    {
        if (world == null || !world.ValidTileLocation(x, y))
            return null;

        var tile = world.Tiles[x, y];
        if (!tile.IsActive)
            return null;

        var frameImportant = world.TileFrameImportant ?? WorldConfiguration.SettingsTileFrameImportant;
        if (frameImportant == null || tile.Type >= frameImportant.Length || !frameImportant[tile.Type])
            return null;

        if (tile.Type >= WorldConfiguration.TileProperties.Count)
            return null;
        var prop = WorldConfiguration.TileProperties[tile.Type];
        if (prop == null || !prop.IsFramed)
            return null;

        // Read frames from tile properties (always loaded) rather than Sprites2, which the UI fills later on another thread.
        var bounds = FindFrameBounds(prop, tile.GetUV(), x, y);
        if (bounds == null)
        {
            var size = prop.GetFrameSize(tile.V);
            var anchor = world.GetAnchor(x, y);
            bounds = new RectangleInt32(anchor.X, anchor.Y, size.X, size.Y);
        }

        var b = bounds.Value;
        if (b.Width * b.Height <= 1 || !b.Contains(x, y))
            return null;

        return RectangleInt32.Intersect(b, new RectangleInt32(0, 0, world.TilesWide, world.TilesHigh));
    }

    /// <summary>
    /// UV distance between neighbouring tiles of one sprite as Terraria saves them. Tiles are padded by 2px;
    /// a larger FrameGap (chairs, toilets, sinks) only spaces the styles apart in the texture.
    /// </summary>
    public static Vector2Short GetTileStep(TileProperty prop)
    {
        var interval = prop.TextureGrid + prop.FrameGap;
        return new Vector2Short(
            (short)System.Math.Min(interval.X, prop.TextureGrid.X + 2),
            (short)System.Math.Min(interval.Y, prop.TextureGrid.Y + 2));
    }

    private static RectangleInt32? FindFrameBounds(TileProperty prop, Vector2Short uv, int x, int y)
    {
        var interval = GetTileStep(prop);
        if (prop.Frames == null || interval.X <= 0 || interval.Y <= 0 || prop.FrameSize == null || prop.FrameSize.Length == 0)
            return null;

        foreach (var frame in prop.Frames)
        {
            var size = frame.Size.X > 0 && frame.Size.Y > 0 ? frame.Size : prop.FrameSize[0];
            if (uv.X < frame.UV.X || uv.Y < frame.UV.Y ||
                uv.X >= frame.UV.X + interval.X * size.X || uv.Y >= frame.UV.Y + interval.Y * size.Y)
                continue;

            // Offset of this tile inside the sprite, from its UV relative to the frame's origin UV.
            int offsetX = (uv.X - frame.UV.X) / interval.X;
            int offsetY = (uv.Y - frame.UV.Y) / interval.Y;
            return new RectangleInt32(x - offsetX, y - offsetY, size.X, size.Y);
        }

        return null;
    }

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
