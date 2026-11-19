using System;
using System.Linq;
using TEdit.Editor.Undo;
using TEdit.Terraria;
using TEdit.Terraria.Editor;
using TEdit.Terraria.Objects;

namespace TEdit.Scripting.Api;

public class SpriteApi
{
    private readonly World _world;
    private readonly IUndoManager _undo;

    public SpriteApi(World world, IUndoManager undo)
    {
        _world = world;
        _undo = undo;
    }

    /// <summary>
    /// List all sprite sheets with tile ID, name, and style count.
    /// </summary>
    public object[] ListSprites()
    {
        lock (WorldConfiguration.Sprites2Lock)
        {
            return WorldConfiguration.Sprites2
                .Select(s => new { tileId = (int)s.Tile, name = s.Name ?? "", styleCount = s.Styles.Count })
                .ToArray<object>();
        }
    }

    /// <summary>
    /// Get all styles for a given tile type.
    /// </summary>
    public object[] GetStyles(int tileId)
    {
        lock (WorldConfiguration.Sprites2Lock)
        {
            var sheet = WorldConfiguration.Sprites2.FirstOrDefault(s => s.Tile == (ushort)tileId);
            if (sheet == null) return Array.Empty<object>();

            return sheet.Styles
                .Select(s => new
                {
                    index = s.Style,
                    name = s.Name ?? "",
                    width = (int)s.SizeTiles.X,
                    height = (int)s.SizeTiles.Y
                })
                .ToArray<object>();
        }
    }

    /// <summary>
    /// Place a sprite by tile ID and style index at (x, y).
    /// Returns true if placement succeeded.
    /// </summary>
    public bool Place(int tileId, int styleIndex, int x, int y)
    {
        SpriteItem sprite;
        lock (WorldConfiguration.Sprites2Lock)
        {
            var sheet = WorldConfiguration.Sprites2.FirstOrDefault(s => s.Tile == (ushort)tileId);
            if (sheet == null) return false;

            sprite = sheet.Styles.FirstOrDefault(s => s.Style == styleIndex);
            if (sprite == null) return false;
        }

        return PlaceSprite(sprite, x, y);
    }

    /// <summary>
    /// Place a sprite by name (first matching style) at (x, y).
    /// Returns true if placement succeeded.
    /// </summary>
    public bool PlaceByName(string name, int x, int y)
    {
        SpriteItem sprite = null;
        lock (WorldConfiguration.Sprites2Lock)
        {
            foreach (var sheet in WorldConfiguration.Sprites2)
            {
                // Match sheet name or style name
                if (string.Equals(sheet.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    sprite = sheet.Default;
                    break;
                }

                var match = sheet.Styles.FirstOrDefault(
                    s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    sprite = match;
                    break;
                }
            }
        }

        if (sprite == null) return false;
        return PlaceSprite(sprite, x, y);
    }

    private bool PlaceSprite(SpriteItem sprite, int x, int y)
    {
        // Bounds check entire footprint
        int w = sprite.SizeTiles.X;
        int h = sprite.SizeTiles.Y;
        if (!_world.ValidTileLocation(x, y) ||
            !_world.ValidTileLocation(x + w - 1, y + h - 1))
            return false;

        // Save undo for all tiles in footprint
        for (int tx = 0; tx < w; tx++)
            for (int ty = 0; ty < h; ty++)
                _undo.SaveTile(_world, x + tx, y + ty);

        // Place sprite tiles using the ITileData overload (no WPF dependency)
        sprite.Place(x, y, (ITileData)_world);

        // Create entities (chests, signs, tile entities) at anchor position
        CreateEntities(x, y, sprite);

        return true;
    }

    private void CreateEntities(int x, int y, SpriteItem sprite)
    {
        // Only create entities at the anchor (top-left) position
        int tileType = sprite.Tile;

        if (TileTypes.IsChest(tileType))
        {
            if (_world.IsAnchor(x, y) && _world.GetChestAtTile(x, y, true) == null)
                _world.Chests.Add(new Chest(x, y));
        }
        else if (TileTypes.IsSign(tileType))
        {
            if (_world.IsAnchor(x, y) && _world.GetSignAtTile(x, y, true) == null)
                _world.Signs.Add(new Sign(x, y, string.Empty));
        }
        else if (TileTypes.IsTileEntity(tileType))
        {
            if (_world.IsAnchor(x, y) && _world.GetTileEntityAtTile(x, y, true) == null)
            {
                var tile = _world.Tiles[x, y];
                var te = TileEntity.CreateForTile(tile, x, y, _world.TileEntities.Count);
                _world.TileEntities.Add(te);
            }
        }
    }
}
