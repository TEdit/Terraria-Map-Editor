using System;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public partial class GenerateApi
{
    // ── Decoration Tile IDs ────────────────────────────────────────────

    private const ushort TILE_VINE = 52;
    private const ushort TILE_JUNGLE_VINE = 62;
    private const ushort TILE_HALLOWED_VINE = 205;
    private const ushort TILE_CRIMSON_VINE = 636;
    private const ushort TILE_SMALL_PLANT = 3;   // grass plants
    private const ushort TILE_JUNGLE_PLANT = 61;
    private const ushort TILE_MUSHROOM_PLANT = 71;
    private const ushort TILE_CORRUPT_PLANT = 24;
    private const ushort TILE_CRIMSON_PLANT = 201;
    private const ushort TILE_HALLOWED_PLANT = 110;
    private const ushort TILE_POT = 28;
    private const ushort TILE_STALACTITE = 165;
    private const ushort TILE_DART_TRAP = 137;
    private const ushort TILE_BOULDER = 138;
    private const ushort TILE_LIFE_CRYSTAL = 12;
    private const ushort TILE_STATUE = 105;
    private const ushort TILE_SUNFLOWER = 27;
    private const ushort TILE_CACTUS = 80;
    private const ushort TILE_MUSHROOM_TALL = 185; // Large mushroom
    private const ushort TILE_THORNS = 32;
    private const ushort TILE_CRIMSON_THORNS = 352;

    // ── Vines ──────────────────────────────────────────────────────────

    /// <summary>
    /// Place vines hanging from grass/jungle grass/hallowed grass in a region.
    /// biome: "forest" (default), "jungle", "hallow", "crimson".
    /// Returns count of vines placed.
    /// </summary>
    public int PlaceVines(int x, int y, int w, int h, string biome = "forest")
    {
        ushort vineType = biome?.ToLowerInvariant() switch
        {
            "jungle" => TILE_JUNGLE_VINE,
            "hallow" => TILE_HALLOWED_VINE,
            "crimson" => TILE_CRIMSON_VINE,
            _ => TILE_VINE,
        };

        ushort grassType = biome?.ToLowerInvariant() switch
        {
            "jungle" => TILE_JUNGLE_GRASS,
            "hallow" => TILE_HALLOWED_GRASS,
            "crimson" => TILE_CRIMSON_GRASS,
            _ => TILE_GRASS,
        };

        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);
        int count = 0;

        for (int tx = x1; tx < x2; tx++)
        {
            for (int ty = y1; ty < y2 - 1; ty++)
            {
                // Look for grass with empty space below
                if (!_world.Tiles[tx, ty].IsActive) continue;
                if (_world.Tiles[tx, ty].Type != grassType) continue;
                if (_world.Tiles[tx, ty + 1].IsActive) continue;

                // ~40% chance to grow vine
                if (_rand.Next(5) < 2) continue;

                // Grow vine downward
                int vineLen = _rand.Next(2, 10);
                for (int vy = 1; vy <= vineLen; vy++)
                {
                    int vy2 = ty + vy;
                    if (vy2 >= y2 || vy2 >= _world.TilesHigh - 1) break;
                    if (_world.Tiles[tx, vy2].IsActive) break;

                    SaveAndSet(tx, vy2, vineType);
                    count++;
                }
            }
        }

        return count;
    }

    // ── Plants ─────────────────────────────────────────────────────────

    /// <summary>
    /// Place random small plants on grass surfaces in a region.
    /// biome: "forest", "jungle", "hallow", "corruption", "crimson", "mushroom".
    /// Returns count of plants placed.
    /// </summary>
    public int PlacePlants(int x, int y, int w, int h, string biome = "forest")
    {
        ushort plantType = biome?.ToLowerInvariant() switch
        {
            "jungle" => TILE_JUNGLE_PLANT,
            "hallow" => TILE_HALLOWED_PLANT,
            "corruption" => TILE_CORRUPT_PLANT,
            "crimson" => TILE_CRIMSON_PLANT,
            "mushroom" => TILE_MUSHROOM_PLANT,
            _ => TILE_SMALL_PLANT,
        };

        ushort grassType = biome?.ToLowerInvariant() switch
        {
            "jungle" => TILE_JUNGLE_GRASS,
            "hallow" => TILE_HALLOWED_GRASS,
            "corruption" => TILE_CORRUPT_GRASS,
            "crimson" => TILE_CRIMSON_GRASS,
            "mushroom" => TILE_MUSHROOM_GRASS,
            _ => TILE_GRASS,
        };

        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);
        int count = 0;

        for (int tx = x1; tx < x2; tx++)
        {
            for (int ty = y1 + 1; ty < y2; ty++)
            {
                // Look for grass with empty space above
                if (!_world.Tiles[tx, ty].IsActive) continue;
                if (_world.Tiles[tx, ty].Type != grassType) continue;
                if (ty <= 0) continue;
                if (_world.Tiles[tx, ty - 1].IsActive) continue;

                // ~30% chance
                if (_rand.Next(10) < 7) continue;

                SaveAndSet(tx, ty - 1, plantType);
                // Random frame variant
                _world.Tiles[tx, ty - 1].U = (short)(_rand.Next(12) * 18);
                _world.Tiles[tx, ty - 1].V = 0;
                count++;
            }
        }

        return count;
    }

    // ── Pots ───────────────────────────────────────────────────────────

    /// <summary>
    /// Place clay pots on solid surfaces in a region. Returns count placed.
    /// </summary>
    public int PlacePots(int x, int y, int w, int h, int count = 0)
    {
        if (count <= 0) count = Math.Max(5, (w * h) / 2000);

        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);
        int placed = 0;

        for (int i = 0; i < count * 10 && placed < count; i++)
        {
            int tx = _rand.Next(x1, x2);
            int ty = _rand.Next(y1, y2);

            // Need solid below and empty at this position
            if (ty >= _world.TilesHigh - 1) continue;
            if (_world.Tiles[tx, ty].IsActive) continue;
            if (!_world.Tiles[tx, ty + 1].IsActive) continue;

            SaveAndSet(tx, ty, TILE_POT);
            _world.Tiles[tx, ty].U = (short)(_rand.Next(4) * 36);
            _world.Tiles[tx, ty].V = 0;
            placed++;
        }

        return placed;
    }

    // ── Stalactites / Stalagmites ──────────────────────────────────────

    /// <summary>
    /// Place stalactites (hanging from ceilings) and stalagmites (growing from floors)
    /// in a region. Returns count placed.
    /// </summary>
    public int PlaceStalactites(int x, int y, int w, int h, int count = 0)
    {
        if (count <= 0) count = Math.Max(10, (w * h) / 1000);

        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);
        int placed = 0;

        for (int i = 0; i < count * 10 && placed < count; i++)
        {
            int tx = _rand.Next(x1, x2);
            int ty = _rand.Next(y1, y2);

            if (_world.Tiles[tx, ty].IsActive) continue;

            bool hangFromCeiling = ty > 1 && _world.Tiles[tx, ty - 1].IsActive &&
                (_world.Tiles[tx, ty - 1].Type == TILE_STONE || _world.Tiles[tx, ty - 1].Type == TILE_ICE);
            bool growFromFloor = ty < _world.TilesHigh - 1 && _world.Tiles[tx, ty + 1].IsActive &&
                (_world.Tiles[tx, ty + 1].Type == TILE_STONE || _world.Tiles[tx, ty + 1].Type == TILE_ICE);

            if (!hangFromCeiling && !growFromFloor) continue;

            SaveAndSet(tx, ty, TILE_STALACTITE);
            // Frame: stalactites use different U/V for hanging vs standing
            if (hangFromCeiling)
            {
                _world.Tiles[tx, ty].U = (short)(_rand.Next(6) * 18);
                _world.Tiles[tx, ty].V = 0;
            }
            else
            {
                _world.Tiles[tx, ty].U = (short)(_rand.Next(6) * 18);
                _world.Tiles[tx, ty].V = 18;
            }
            placed++;
        }

        return placed;
    }

    // ── Traps ──────────────────────────────────────────────────────────

    /// <summary>
    /// Place dart traps on walls facing open space in a region. Returns count placed.
    /// </summary>
    public int PlaceTraps(int x, int y, int w, int h, int count = 0)
    {
        if (count <= 0) count = Math.Max(3, (w * h) / 5000);

        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);
        int placed = 0;

        for (int i = 0; i < count * 20 && placed < count; i++)
        {
            int tx = _rand.Next(x1, x2);
            int ty = _rand.Next(y1, y2);

            // Must be an empty tile next to a solid wall
            if (_world.Tiles[tx, ty].IsActive) continue;
            if (tx <= 1 || tx >= _world.TilesWide - 2) continue;

            bool leftWall = _world.Tiles[tx - 1, ty].IsActive;
            bool rightWall = _world.Tiles[tx + 1, ty].IsActive;
            if (!leftWall && !rightWall) continue;

            // Place dart trap on the wall side
            int trapX = leftWall ? tx - 1 : tx + 1;
            SaveAndSet(trapX, ty, TILE_DART_TRAP);
            // Frame direction: 0=facing right, 18=facing left
            _world.Tiles[trapX, ty].U = leftWall ? (short)0 : (short)18;
            _world.Tiles[trapX, ty].V = 0;
            placed++;
        }

        return placed;
    }

    // ── Life Crystals ──────────────────────────────────────────────────

    /// <summary>
    /// Place life crystals on solid surfaces in a region. Returns count placed.
    /// </summary>
    public int PlaceLifeCrystals(int x, int y, int w, int h, int count = 0)
    {
        if (count <= 0) count = Math.Max(3, (w * h) / 10000);

        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);
        int placed = 0;

        for (int i = 0; i < count * 30 && placed < count; i++)
        {
            int tx = _rand.Next(x1, x2);
            int ty = _rand.Next(y1, y2);

            // Need 2x2 empty space with solid floor below
            if (tx >= x2 - 1 || ty >= y2 - 1 || ty < 1) continue;
            if (_world.Tiles[tx, ty].IsActive) continue;
            if (_world.Tiles[tx + 1, ty].IsActive) continue;
            if (_world.Tiles[tx, ty + 1].IsActive) continue;
            if (_world.Tiles[tx + 1, ty + 1].IsActive) continue;
            if (ty + 2 >= _world.TilesHigh) continue;
            if (!_world.Tiles[tx, ty + 2].IsActive) continue;
            if (!_world.Tiles[tx + 1, ty + 2].IsActive) continue;

            // Place 2x2 life crystal
            SaveAndSet(tx, ty, TILE_LIFE_CRYSTAL);
            _world.Tiles[tx, ty].U = 0;
            _world.Tiles[tx, ty].V = 0;
            SaveAndSet(tx + 1, ty, TILE_LIFE_CRYSTAL);
            _world.Tiles[tx + 1, ty].U = 18;
            _world.Tiles[tx + 1, ty].V = 0;
            SaveAndSet(tx, ty + 1, TILE_LIFE_CRYSTAL);
            _world.Tiles[tx, ty + 1].U = 0;
            _world.Tiles[tx, ty + 1].V = 18;
            SaveAndSet(tx + 1, ty + 1, TILE_LIFE_CRYSTAL);
            _world.Tiles[tx + 1, ty + 1].U = 18;
            _world.Tiles[tx + 1, ty + 1].V = 18;
            placed++;
        }

        return placed;
    }

    // ── Smooth World ───────────────────────────────────────────────────

    /// <summary>
    /// Auto-slope exposed tile edges in a region for natural-looking terrain.
    /// Applies half-bricks and slopes based on neighboring tile layout.
    /// Returns count of tiles modified.
    /// </summary>
    public int SmoothWorld(int x, int y, int w, int h)
    {
        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);
        int count = 0;

        for (int tx = x1; tx < x2; tx++)
        {
            for (int ty = y1; ty < y2; ty++)
            {
                if (!_world.Tiles[tx, ty].IsActive) continue;

                // Skip frame-important tiles
                var tp = WorldConfiguration.GetTileProperties((int)_world.Tiles[tx, ty].Type);
                if (tp != null && tp.IsFramed) continue;

                // Check cardinal neighbors
                bool up = ty > 0 && _world.Tiles[tx, ty - 1].IsActive;
                bool down = ty < _world.TilesHigh - 1 && _world.Tiles[tx, ty + 1].IsActive;
                bool left = tx > 0 && _world.Tiles[tx - 1, ty].IsActive;
                bool right = tx < _world.TilesWide - 1 && _world.Tiles[tx + 1, ty].IsActive;

                BrickStyle newStyle = BrickStyle.Full;

                if (!up && down && left && !right)
                    newStyle = BrickStyle.SlopeTopRight;
                else if (!up && down && !left && right)
                    newStyle = BrickStyle.SlopeTopLeft;
                else if (up && !down && left && !right)
                    newStyle = BrickStyle.SlopeBottomRight;
                else if (up && !down && !left && right)
                    newStyle = BrickStyle.SlopeBottomLeft;
                else if (!up && down && !left && !right)
                    newStyle = BrickStyle.HalfBrick;

                if (newStyle != BrickStyle.Full)
                {
                    _undo.SaveTile(_world, tx, ty);
                    _world.Tiles[tx, ty].BrickStyle = newStyle;
                    count++;
                }
            }
        }

        return count;
    }

    // ── Sunflowers ─────────────────────────────────────────────────────

    /// <summary>
    /// Place sunflowers on grass surfaces. Returns count placed.
    /// </summary>
    public int PlaceSunflowers(int x, int y, int w, int h, int count = 0)
    {
        if (count <= 0) count = Math.Max(3, w / 30);

        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);
        int placed = 0;

        for (int i = 0; i < count * 10 && placed < count; i++)
        {
            int tx = _rand.Next(x1, x2);

            // Find surface grass
            for (int ty = y1; ty < y2 - 1; ty++)
            {
                if (!_world.Tiles[tx, ty].IsActive) continue;
                if (_world.Tiles[tx, ty].Type != TILE_GRASS) continue;
                if (ty <= 0 || _world.Tiles[tx, ty - 1].IsActive) continue;

                // Need 2-wide space above
                if (tx + 1 >= x2) break;
                if (_world.Tiles[tx + 1, ty - 1].IsActive) break;
                if (!_world.Tiles[tx + 1, ty].IsActive) break;

                // Place 2x2 sunflower (simplified)
                if (ty >= 2)
                {
                    SaveAndSet(tx, ty - 1, TILE_SUNFLOWER);
                    _world.Tiles[tx, ty - 1].U = 0;
                    _world.Tiles[tx, ty - 1].V = 0;
                    SaveAndSet(tx, ty - 2, TILE_SUNFLOWER);
                    _world.Tiles[tx, ty - 2].U = 0;
                    _world.Tiles[tx, ty - 2].V = 18;
                    placed++;
                }
                break;
            }
        }

        return placed;
    }

    // ── Thorns ─────────────────────────────────────────────────────────

    /// <summary>
    /// Place thorns growing from corruption/crimson grass. Returns count placed.
    /// biome: "corruption" or "crimson".
    /// </summary>
    public int PlaceThorns(int x, int y, int w, int h, string biome = "corruption")
    {
        ushort thornType = biome?.ToLowerInvariant() == "crimson" ? TILE_CRIMSON_THORNS : TILE_THORNS;
        ushort grassType = biome?.ToLowerInvariant() == "crimson" ? TILE_CRIMSON_GRASS : TILE_CORRUPT_GRASS;

        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);
        int count = 0;

        for (int tx = x1; tx < x2; tx++)
        {
            for (int ty = y1 + 1; ty < y2; ty++)
            {
                if (!_world.Tiles[tx, ty].IsActive) continue;
                if (_world.Tiles[tx, ty].Type != grassType) continue;
                if (ty <= 0 || _world.Tiles[tx, ty - 1].IsActive) continue;

                if (_rand.Next(4) != 0) continue;

                // Grow thorns upward
                int thornLen = _rand.Next(1, 6);
                for (int vy = 1; vy <= thornLen; vy++)
                {
                    int vy2 = ty - vy;
                    if (vy2 < 1 || _world.Tiles[tx, vy2].IsActive) break;

                    SaveAndSet(tx, vy2, thornType);
                    count++;
                }
            }
        }

        return count;
    }
}
