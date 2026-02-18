using System.Collections.Concurrent;
using System.Collections.Generic;
using TEdit.Common;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Render;

public readonly struct BuffTileInfo
{
    public readonly int X;
    public readonly int Y;
    public readonly short HalfW;
    public readonly short HalfH;
    public readonly float CenterX;
    public readonly float CenterY;
    public readonly int FrameW;
    public readonly int FrameH;
    public readonly TEditColor Color;

    public BuffTileInfo(int x, int y, short halfW, short halfH, float centerX, float centerY, int frameW, int frameH, TEditColor color)
    {
        X = x; Y = y;
        HalfW = halfW; HalfH = halfH;
        CenterX = centerX; CenterY = centerY;
        FrameW = frameW; FrameH = frameH;
        Color = color;
    }
}

public class BuffTileCache
{
    private readonly ConcurrentDictionary<long, BuffTileInfo> _entries = new();

    private static long Key(int x, int y) => ((long)x << 32) | (uint)y;

    public ICollection<BuffTileInfo> Entries => _entries.Values;

    public void Clear() => _entries.Clear();

    /// <summary>
    /// Check a tile at (x,y) and add/remove from cache as needed.
    /// </summary>
    public void UpdateTile(int x, int y, Tile tile)
    {
        var key = Key(x, y);

        if (!tile.IsActive || tile.Type >= WorldConfiguration.TileProperties.Count)
        {
            _entries.TryRemove(key, out _);
            return;
        }

        var tileProp = WorldConfiguration.TileProperties[tile.Type];

        Vector2Short? buffRadius = null;
        TEditColor? buffColor = null;

        if (tileProp.Frames != null && tileProp.Frames.Count > 0)
        {
            var uv = new Vector2Short(tile.U, tile.V);
            if (tileProp.IsOrigin(uv, out var frame) && frame != null)
            {
                if (frame.BuffRadius.HasValue)
                {
                    buffRadius = frame.BuffRadius;
                    buffColor = frame.BuffColor;
                }
                else if (tileProp.BuffRadius.HasValue)
                {
                    buffRadius = tileProp.BuffRadius;
                    buffColor = tileProp.BuffColor;
                }
            }
            else
            {
                // Not an origin tile — remove if previously cached
                _entries.TryRemove(key, out _);
                return;
            }
        }
        else if (tileProp.BuffRadius.HasValue)
        {
            buffRadius = tileProp.BuffRadius;
            buffColor = tileProp.BuffColor;
        }

        if (!buffRadius.HasValue || buffColor == null)
        {
            _entries.TryRemove(key, out _);
            return;
        }

        var frameSize = tileProp.GetFrameSize(tile.V);
        float centerX = x + frameSize.X / 2f;
        float centerY = y + frameSize.Y / 2f;

        _entries[key] = new BuffTileInfo(
            x, y,
            buffRadius.Value.X, buffRadius.Value.Y,
            centerX, centerY,
            frameSize.X, frameSize.Y,
            buffColor.Value);
    }

    /// <summary>
    /// Full rebuild from world data.
    /// </summary>
    public void RebuildFromWorld(World world)
    {
        _entries.Clear();
        if (world == null) return;

        for (int y = 0; y < world.TilesHigh; y++)
        {
            for (int x = 0; x < world.TilesWide; x++)
            {
                var tile = world.Tiles[x, y];
                if (!tile.IsActive) continue;
                if (tile.Type >= WorldConfiguration.TileProperties.Count) continue;

                var tileProp = WorldConfiguration.TileProperties[tile.Type];

                // Quick skip: no buff on this tile type at all
                if (!tileProp.BuffRadius.HasValue &&
                    (tileProp.Frames == null || tileProp.Frames.Count == 0))
                    continue;

                // Only check tiles that could have buffs
                bool hasBuff = tileProp.BuffRadius.HasValue;
                if (!hasBuff && tileProp.Frames != null)
                {
                    foreach (var frame in tileProp.Frames)
                    {
                        if (frame.BuffRadius.HasValue)
                        {
                            hasBuff = true;
                            break;
                        }
                    }
                }

                if (hasBuff)
                    UpdateTile(x, y, tile);
            }
        }
    }
}
