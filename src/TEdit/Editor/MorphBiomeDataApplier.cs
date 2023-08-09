using System;
using System.Collections.Generic;
using System.Linq;
using TEdit.Terraria;
using TEdit.ViewModel;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Configuration.BiomeMorph;

namespace TEdit.Editor;

public static class BiomeMorphExtensions
{
    /// <summary>
    /// This modifies the tile UV coordinates
    /// </summary>
    public static void ApplyOffset(this MorphSpriteUVOffset offset, ref Tile tile)
    {
        if (offset.Delete)
        {
            tile.Type = 0;
            tile.IsActive = false;
            return;
        }

        tile.U += offset.OffsetU;

        if (offset.UseFilterV)
        {
            tile.V += offset.OffsetV;
        }
    }

    /// <summary>
    /// Check if a tile matches the filter
    /// </summary>
    public static bool FilterMatches(this MorphSpriteUVOffset offset, Tile tile)
    {
        if (tile.U < offset.MinU) return false;
        if (tile.U > offset.MaxU) return false;

        if (offset.UseFilterV)
        {
            if (tile.V < offset.MinV) return false;
            if (tile.V > offset.MaxV) return false;
        }

        return true;
    }
}
public class MorphBiomeDataApplier
{
    static Dictionary<MorphBiomeData, MorphBiomeDataApplier> _morpherCache = new();

    public static MorphBiomeDataApplier GetMorpher(MorphBiomeData biome)
    {
        MorphBiomeDataApplier morpher;

        if (!_morpherCache.TryGetValue(biome, out morpher))
        {
            morpher = new MorphBiomeDataApplier(biome);
            _morpherCache[biome] = morpher;
        }

        return morpher;
    }

    private MorphBiomeDataApplier(MorphBiomeData data)
    {
        _morph = data;
        InitCache();
    }

    public bool ContainsWall(int id) => _wallCache.ContainsKey(id);
    public bool ContainsTile(int id) => _tileCache.ContainsKey(id);

    public void ApplyMorph(MorphToolOptions options, Tile source, MorphLevel level, Vector2Int32 location)
    {
        ApplyTileMorph(options, source, level, location);
        ApplyWallMorph(options, source, level, location);
    }

    private void ApplyWallMorph(MorphToolOptions options, Tile source, MorphLevel level, Vector2Int32 location)
    {
        if (source.Wall == 0) { return; }
        bool useEvil = options.EnableEvilTiles;

        ushort sourceId = source.Wall;
        if (!_wallCache.TryGetValue(sourceId, out var morphId)) { return; }

        if (morphId.SourceIds.Contains(sourceId))
        {
            if (morphId.Delete)
            {
                source.Wall = 0;
                return;
            }

            // Check if tiles need gravity checks.
            if (morphId.Gravity != null && AirBelow(location.X, location.Y))
            {
                source.Wall = morphId.Gravity.GetId(level, useEvil) ?? 0;
            }
            else if (morphId.TouchingAir != null && TouchingAir(location.X, location.Y))
            {
                var id = morphId.TouchingAir.GetId(level, useEvil);
                if (id != null)
                {
                    source.Wall = id.Value;
                }
            }
            else
            {
                var id = morphId.Default.GetId(level, useEvil);
                if (id != null)
                {
                    source.Wall = id.Value;
                }
            }
        }
    }

    private void ApplyTileMorph(MorphToolOptions options, Tile source, MorphLevel level, Vector2Int32 location)
    {
        if (!source.IsActive) { return; }
        ushort sourceId = source.Type;

        if (!_tileCache.TryGetValue(sourceId, out var morphId)) { return; }
        bool useEvil = options.EnableEvilTiles;

        if (morphId.SourceIds.Contains(sourceId))
        {
            if (morphId.Delete)
            {
                source.Type = 0;
                source.IsActive = false;
                return;
            }

            // Check if tiles need gravity checks.
            if (morphId.Gravity != null && AirBelow(location.X, location.Y))
            {
                source.Type = morphId.Gravity.GetId(level, useEvil) ?? 397;
            }
            else if (morphId.TouchingAir != null && TouchingAir(location.X, location.Y))
            {
                var id = morphId.TouchingAir.GetId(level, useEvil);
                if (id != null)
                {
                    source.Type = id.Value;
                }
            }
            else
            {
                var id = morphId.Default.GetId(level, useEvil) ?? source.Type;

                // apply moss to stone blocks
                if (morphId.UseMoss &&
                    options.EnableMoss &&
                    (WorldConfiguration.MorphSettings.IsMoss(source.Type) ||
                     TouchingAir(location.X, location.Y)))
                {
                    source.Type = (ushort)options.MossType;
                }
                else
                {
                    source.Type = id;
                }
            }

            // filter sprites
            if (options.EnableSprites &&
                WorldConfiguration.TileProperties[sourceId].IsFramed &&
                morphId.SpriteOffsets.Count > 0)
            {
                // filter and apply morph (offset or delete)
                morphId.SpriteOffsets
                    .FirstOrDefault(uv => uv.FilterMatches(source))
                    ?.ApplyOffset(ref source);
            }
        }
    }





    public static bool AirBelow(int x, int y)
    {
        var world = ViewModelLocator.WorldViewModel.CurrentWorld;
        if (world == null) return false;

        if (y >= world.TilesHigh - 1) return false;

        Tile tile = world.Tiles[x, y + 1];
        return !tile.IsActive || MorphConfiguration.NotSolidTiles.Contains(tile.Type);
    }

    // copied from rendering
    const int e = 0, n = 1, w = 2, s = 3, ne = 4, nw = 5, sw = 6, se = 7;
    private readonly MorphBiomeData _morph;
    static Tile[] neighborTile = new Tile[8];

    public static bool TouchingAir(int x, int y)
    {
        var world = ViewModelLocator.WorldViewModel.CurrentWorld;
        if (world == null) return false;

        // copied from render code. this should probably be made a method so it can be reused
        neighborTile[e] = x + 1 < world.TilesWide ? world.Tiles[x + 1, y] : null;
        neighborTile[n] = y - 1 > 0 ? world.Tiles[x, y - 1] : null;
        neighborTile[w] = x - 1 > 0 ? world.Tiles[x - 1, y] : null;
        neighborTile[s] = y + 1 < world.TilesHigh ? world.Tiles[x, y + 1] : null;
        neighborTile[ne] = x + 1 < world.TilesWide && y - 1 > 0 ? world.Tiles[x + 1, y - 1] : null;
        neighborTile[nw] = x - 1 > 0 && y - 1 > 0 ? world.Tiles[x - 1, y - 1] : null;
        neighborTile[sw] = x - 1 > 0 && y + 1 < world.TilesHigh ? world.Tiles[x - 1, y + 1] : null;
        neighborTile[se] = x + 1 < world.TilesWide && y + 1 < world.TilesHigh ? world.Tiles[x + 1, y + 1] : null;

        // these loops are split out because checking isactive is orders of magnitude faster than checking the hashset
        // if a tile is empty, this lets the algorithm shortcut the hashcheck, making it faster
        for (int i = 0; i < neighborTile.Length; i++)
        {
            var t = neighborTile[i];
            if (t == null) { continue; }
            if (!t.IsActive) { return true; } // air
        }

        for (int i = 0; i < neighborTile.Length; i++)
        {
            var t = neighborTile[i];
            if (t == null) { continue; }
            if (MorphConfiguration.NotSolidTiles.Contains(t.Type)) { return true; } // non-solid
        }

        return false;
    }

    public void InitCache()
    {
        _tileCache.Clear();
        foreach (var item in _morph.MorphTiles)
        {
            foreach (var id in item.SourceIds)
            {
                try
                {
                    _tileCache.Add(id, item);
                }
                catch (Exception ex)
                {
                    throw new IndexOutOfRangeException($"morphSetting tile entry is invalid or duplicate: {item.Name} [{id}]", ex);
                }
            }
        }

        _wallCache.Clear();
        foreach (var item in _morph.MorphWalls)
        {
            foreach (var id in item.SourceIds)
            {
                try
                {
                    _wallCache.Add(id, item);
                }
                catch (Exception ex)
                {
                    throw new IndexOutOfRangeException($"morphSetting wall entry is invalid or duplicate: {item.Name} [{id}]", ex);
                }
            }
        }
    }

    private Dictionary<int, MorphId> _wallCache = new Dictionary<int, MorphId>();
    private Dictionary<int, MorphId> _tileCache = new Dictionary<int, MorphId>();
}
