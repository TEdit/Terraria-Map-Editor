using System;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public partial class GenerateApi
{
    // ── Tile IDs for biome conversions ───────────────────────────────────

    private const ushort TILE_DIRT = 0;
    private const ushort TILE_STONE = 1;
    private const ushort TILE_GRASS = 2;
    private const ushort TILE_CORRUPT_GRASS = 23;
    private const ushort TILE_EBONSTONE = 25;
    private const ushort TILE_SAND = 53;
    private const ushort TILE_ASH = 57;
    private const ushort TILE_HELLSTONE = 58;
    private const ushort TILE_MUD = 59;
    private const ushort TILE_JUNGLE_GRASS = 60;
    private const ushort TILE_MUSHROOM_GRASS = 70;
    private const ushort TILE_HALLOWED_GRASS = 109;
    private const ushort TILE_EBONSAND = 112;
    private const ushort TILE_PEARLSAND = 116;
    private const ushort TILE_PEARLSTONE = 117;
    private const ushort TILE_SNOW = 147;
    private const ushort TILE_ICE = 161;
    private const ushort TILE_PURPLE_ICE = 163;
    private const ushort TILE_PINK_ICE = 164;
    private const ushort TILE_HALLOWED_DIRT = 477;
    private const ushort TILE_CRIMSON_GRASS = 199;
    private const ushort TILE_RED_ICE = 200;
    private const ushort TILE_CRIMSTONE = 203;
    private const ushort TILE_SLUSH = 224;
    private const ushort TILE_HIVE = 225;
    private const ushort TILE_CRIMSAND = 234;
    private const ushort TILE_MARBLE = 367;
    private const ushort TILE_GRANITE = 368;
    private const ushort TILE_SANDSTONE = 396;
    private const ushort TILE_HARDENED_SAND = 397;
    private const ushort TILE_COBWEB = 51;

    private const ushort WALL_DIRT = 2;
    private const ushort WALL_EBONSTONE = 3;
    private const ushort WALL_MUD = 15;
    private const ushort WALL_SNOW = 40;
    private const ushort WALL_SPIDER = 62;
    private const ushort WALL_MUSHROOM = 80;
    private const ushort WALL_CRIMSTONE = 83;
    private const ushort WALL_HIVE = 108;
    private const ushort WALL_MARBLE = 178;
    private const ushort WALL_GRANITE = 180;
    private const ushort WALL_SANDSTONE = 187;

    // ── Biome Shape Helper ──────────────────────────────────────────────

    /// <summary>
    /// Determine if a tile at (col, row) is inside the given biome shape
    /// defined by bounding box (x, y, w, h).
    /// Shapes: "rectangle", "ellipse", "diagonalLeft", "diagonalRight", "trapezoid", "hemisphere".
    /// Adds per-tile random jitter at boundaries for natural edges.
    /// </summary>
    private bool IsInsideShape(int col, int row, int x, int y, int w, int h, string shape)
    {
        double centerX = x + w * 0.5;
        double halfW = Math.Max(1, w * 0.5);
        int safeH = Math.Max(1, h);

        double nx = (col - centerX) / halfW;  // -1 to 1 horizontally
        double ny = (double)(row - y) / safeH; // 0 to 1 vertically (top to bottom)

        // Per-tile edge jitter for natural boundaries (±~5 tiles at typical biome sizes)
        double jitter = (_rand.NextDouble() - 0.5) * 0.12;

        switch (shape)
        {
            case "ellipse":
            {
                double nyc = ny * 2.0 - 1.0; // remap to -1..1
                return nx * nx + nyc * nyc <= 1.0 + jitter;
            }
            case "diagonalLeft":
            {
                double skew = 0.4;
                double adjustedNx = nx - skew * (ny * 2.0 - 1.0);
                return Math.Abs(adjustedNx) <= 1.0 + jitter && ny >= -0.05 && ny <= 1.05;
            }
            case "diagonalRight":
            {
                double skew = 0.4;
                double adjustedNx = nx + skew * (ny * 2.0 - 1.0);
                return Math.Abs(adjustedNx) <= 1.0 + jitter && ny >= -0.05 && ny <= 1.05;
            }
            case "trapezoid":
            {
                // Width expands toward bottom (positive ny)
                double spread = 0.5;
                double effectiveWidth = 1.0 + spread * ny;
                return Math.Abs(nx) <= effectiveWidth + jitter && ny >= -0.05 && ny <= 1.05;
            }
            case "hemisphere":
            {
                // Dome on top half, rectangle on bottom half
                if (ny > 0.5)
                    return Math.Abs(nx) <= 1.0 + jitter;
                double nyc = ny * 2.0; // 0 to 1 for top half
                return nx * nx + nyc * nyc <= 1.0 + jitter;
            }
            case "rectangle":
            default:
                return Math.Abs(nx) <= 1.0 + jitter && ny >= -0.05 && ny <= 1.05;
        }
    }

    // ── Ice Biome ───────────────────────────────────────────────────────

    /// <summary>
    /// Convert tiles in a region to ice/snow variants using the specified shape.
    /// Default shape is "trapezoid" (wider underground, like Terraria's per-row walk).
    /// stone→ice, dirt→snow, grass→snow, mud→slush, sand→hardened sand.
    /// Also converts dirt walls to snow walls and scatters slush blobs.
    /// </summary>
    public int IceBiome(int x, int y, int w, int h, string shape = "trapezoid")
    {
        int count = 0;
        int margin = 10;
        int startCol = Math.Max(1, x - margin);
        int endCol = Math.Min(_world.TilesWide - 1, x + w + margin);

        for (int col = startCol; col < endCol; col++)
        {
            int surface = FindSurface(col, Math.Max(1, y - 30), y + h);
            if (surface < 0) surface = y;

            int bottom = Math.Min(surface + h, _world.TilesHigh - 1);

            for (int ty = surface; ty < bottom; ty++)
            {
                if (!IsInsideShape(col, ty, x, y, w, h, shape)) continue;

                if (_world.Tiles[col, ty].IsActive)
                {
                    ushort cur = _world.Tiles[col, ty].Type;
                    ushort target = cur switch
                    {
                        TILE_STONE => TILE_ICE,
                        TILE_DIRT => TILE_SNOW,
                        TILE_GRASS => TILE_SNOW,
                        TILE_MUD => TILE_SLUSH,
                        TILE_SAND => TILE_HARDENED_SAND,
                        _ => cur,
                    };

                    if (target != cur)
                    {
                        _undo.SaveTile(_world, col, ty);
                        _world.Tiles[col, ty].Type = target;
                        count++;
                    }
                }

                // Convert dirt walls to snow walls
                ushort wall = _world.Tiles[col, ty].Wall;
                if (wall == WALL_DIRT || wall == 1)
                {
                    _undo.SaveTile(_world, col, ty);
                    _world.Tiles[col, ty].Wall = WALL_SNOW;
                }
            }
        }

        // Ice cave blobs for texture
        int caves = Math.Max(3, (w * h) / 5000);
        for (int i = 0; i < caves; i++)
        {
            int cx = x + _rand.Next(w);
            int cy = y + _rand.Next(h);
            if (_world.ValidTileLocation(cx, cy))
                TileRunnerCore(cx, cy, _rand.Next(3, 8), _rand.Next(5, 20), TILE_ICE, false, 0, 0);
        }

        // Slush blob scattering
        int slushBlobs = Math.Max(2, (w * h) / 6000);
        for (int i = 0; i < slushBlobs; i++)
        {
            int sx = x + _rand.Next(w);
            int sy = y + _rand.Next(h);
            if (_world.ValidTileLocation(sx, sy))
                TileRunnerCore(sx, sy, _rand.Next(2, 6), _rand.Next(3, 12), TILE_SLUSH, false, 0, 0);
        }

        return count;
    }

    // ── Mushroom Biome ─────────────────────────────────────────────────

    /// <summary>
    /// Create a glowing mushroom biome using the specified shape.
    /// Default shape is "ellipse" (oblate ellipse like Terraria's ShroomPatch).
    /// Converts dirt/stone to mud, places mushroom grass on exposed surfaces,
    /// adds mushroom walls, and places mushroom trees.
    /// </summary>
    public int MushroomBiome(int x, int y, int w, int h, string shape = "ellipse")
    {
        int x1 = Math.Max(1, x - 10);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w + 10);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);
        int count = 0;

        // First pass: convert stone/dirt to mud, place mushroom walls
        for (int tx = x1; tx < x2; tx++)
        {
            for (int ty = y1; ty < y2; ty++)
            {
                if (!IsInsideShape(tx, ty, x, y, w, h, shape)) continue;

                if (_world.Tiles[tx, ty].IsActive)
                {
                    ushort cur = _world.Tiles[tx, ty].Type;
                    if (cur == TILE_STONE || cur == TILE_DIRT || cur == TILE_GRASS)
                    {
                        _undo.SaveTile(_world, tx, ty);
                        _world.Tiles[tx, ty].Type = TILE_MUD;
                        count++;
                    }
                }

                // Place mushroom walls — even in air tiles for proper biome detection
                ushort wall = _world.Tiles[tx, ty].Wall;
                if (wall == WALL_DIRT || wall == 1 || wall == 0)
                {
                    _undo.SaveTile(_world, tx, ty);
                    _world.Tiles[tx, ty].Wall = WALL_MUSHROOM;
                }
            }
        }

        // Second pass: convert exposed mud to mushroom grass
        for (int tx = x1; tx < x2; tx++)
        {
            for (int ty = y1; ty < y2; ty++)
            {
                if (!IsInsideShape(tx, ty, x, y, w, h, shape)) continue;
                if (!_world.Tiles[tx, ty].IsActive) continue;
                if (_world.Tiles[tx, ty].Type != TILE_MUD) continue;

                if (IsExposedToAir(tx, ty))
                {
                    _undo.SaveTile(_world, tx, ty);
                    _world.Tiles[tx, ty].Type = TILE_MUSHROOM_GRASS;
                    count++;
                }
            }
        }

        // Scatter some mud blobs for natural edges
        int blobs = Math.Max(2, (w * h) / 4000);
        for (int i = 0; i < blobs; i++)
        {
            int bx = x + _rand.Next(w);
            int by = y + _rand.Next(h);
            if (_world.ValidTileLocation(bx, by))
                TileRunnerCore(bx, by, _rand.Next(3, 7), _rand.Next(5, 15), TILE_MUD, false, 0, 0);
        }

        return count;
    }

    // ── Marble Cave ────────────────────────────────────────────────────

    /// <summary>
    /// Create a marble cave using wandering blobs of marble tile and marble walls.
    /// </summary>
    public void MarbleCave(int x, int y, double strength = 40.0)
    {
        if (!_world.ValidTileLocation(x, y)) return;

        int runs = _rand.Next(3, 6);
        for (int i = 0; i < runs; i++)
        {
            int cx = x + _rand.Next(-20, 21);
            int cy = y + _rand.Next(-10, 11);

            double str = strength * 0.3 + _rand.Next((int)(strength * 0.5));
            int steps = _rand.Next(10, 30);
            TileRunnerCore(cx, cy, str, steps, TILE_MARBLE, false, 0, 0);

            PlaceWallBlob(cx, cy, (int)(str * 1.3), WALL_MARBLE);
        }

        // Carve internal caves
        int caves = _rand.Next(2, 5);
        for (int i = 0; i < caves; i++)
        {
            int cx = x + _rand.Next(-15, 16);
            int cy = y + _rand.Next(-8, 9);
            Tunnel(cx, cy, _rand.Next(3, 7), _rand.Next(5, 20));
        }
    }

    // ── Granite Cave ───────────────────────────────────────────────────

    /// <summary>
    /// Create a granite cave with granite tiles, granite walls, and carved interior.
    /// </summary>
    public void GraniteCave(int x, int y, double strength = 40.0)
    {
        if (!_world.ValidTileLocation(x, y)) return;

        int runs = _rand.Next(3, 6);
        for (int i = 0; i < runs; i++)
        {
            int cx = x + _rand.Next(-20, 21);
            int cy = y + _rand.Next(-10, 11);

            double str = strength * 0.3 + _rand.Next((int)(strength * 0.5));
            int steps = _rand.Next(10, 30);
            TileRunnerCore(cx, cy, str, steps, TILE_GRANITE, false, 0, 0);

            PlaceWallBlob(cx, cy, (int)(str * 1.3), WALL_GRANITE);
        }

        int caves = _rand.Next(3, 7);
        for (int i = 0; i < caves; i++)
        {
            int cx = x + _rand.Next(-15, 16);
            int cy = y + _rand.Next(-8, 9);
            Tunnel(cx, cy, _rand.Next(4, 9), _rand.Next(10, 30));
        }
    }

    // ── Corruption ─────────────────────────────────────────────────────

    /// <summary>
    /// Apply Corruption biome using the specified shape.
    /// Default shape is "diagonalLeft" (leans left like hardmode V-strip).
    /// stone→ebonstone, dirt→ebonstone, grass→corrupt grass, sand→ebonsand, ice→purple ice.
    /// Carves chasms (vertical shafts with side branches).
    /// </summary>
    public int Corruption(int x, int y, int w, int depth, string shape = "diagonalLeft")
    {
        int count = 0;
        int margin = 15;
        int startCol = Math.Max(1, x - margin);
        int endCol = Math.Min(_world.TilesWide - 1, x + w + margin);

        for (int col = startCol; col < endCol; col++)
        {
            int surface = FindSurface(col, Math.Max(1, y - 30), y + depth);
            if (surface < 0) surface = y;

            int bottom = Math.Min(surface + depth, _world.TilesHigh - 1);

            for (int ty = surface; ty < bottom; ty++)
            {
                if (!IsInsideShape(col, ty, x, y, w, depth, shape)) continue;

                if (_world.Tiles[col, ty].IsActive)
                {
                    ushort cur = _world.Tiles[col, ty].Type;
                    ushort target = cur switch
                    {
                        TILE_STONE => TILE_EBONSTONE,
                        TILE_DIRT => TILE_EBONSTONE,
                        TILE_GRASS => TILE_CORRUPT_GRASS,
                        TILE_SAND => TILE_EBONSAND,
                        TILE_ICE => TILE_PURPLE_ICE,
                        _ => cur,
                    };

                    if (target != cur)
                    {
                        _undo.SaveTile(_world, col, ty);
                        _world.Tiles[col, ty].Type = target;
                        count++;
                    }
                }

                // Convert walls
                ushort wall = _world.Tiles[col, ty].Wall;
                if (wall == WALL_DIRT || wall == 1)
                {
                    _undo.SaveTile(_world, col, ty);
                    _world.Tiles[col, ty].Wall = WALL_EBONSTONE;
                }
            }
        }

        // Carve chasms (vertical shafts) — center of the biome
        int chasmCount = Math.Max(1, w / 40);
        for (int i = 0; i < chasmCount; i++)
        {
            int chasmX = x + _rand.Next(10, Math.Max(11, w - 10));
            int surface = FindSurface(chasmX, Math.Max(1, y - 10), y + 50);
            if (surface < 0) surface = y;
            ChasmRunner(chasmX, surface, _rand.Next(30, 60));
        }

        return count;
    }

    /// <summary>
    /// Port of WorldGen.ChasmRunner — carves a vertical shaft with side branches.
    /// </summary>
    private void ChasmRunner(int startX, int startY, int steps)
    {
        double width = _rand.Next(5) + 7;
        double posX = startX;
        double posY = startY;
        double velX = _rand.Next(-10, 11) * 0.1;
        double velY = _rand.Next(11) * 0.2 + 0.5;
        int remaining = steps;
        bool branchedLeft = false;
        bool branchedRight = false;

        while (remaining > 0 && width > 0)
        {
            remaining--;

            int x0 = Math.Max(1, (int)(posX - width * 0.5));
            int x1 = Math.Min(_world.TilesWide - 1, (int)(posX + width * 0.5));
            int y0 = Math.Max(1, (int)(posY - width * 0.5));
            int y1 = Math.Min(_world.TilesHigh - 1, (int)(posY + width * 0.5));

            for (int tx = x0; tx < x1; tx++)
            {
                for (int ty = y0; ty < y1; ty++)
                {
                    double dist = Math.Abs(tx - posX) + Math.Abs(ty - posY);
                    if (dist < width * 0.5)
                    {
                        _undo.SaveTile(_world, tx, ty);
                        _world.Tiles[tx, ty].ClearTile();
                    }
                    else if (dist < width * 0.8)
                    {
                        if (_world.Tiles[tx, ty].IsActive)
                        {
                            var tp = WorldConfiguration.GetTileProperties((int)_world.Tiles[tx, ty].Type);
                            if (tp == null || !tp.IsFramed)
                            {
                                _undo.SaveTile(_world, tx, ty);
                                _world.Tiles[tx, ty].Type = TILE_EBONSTONE;
                            }
                        }
                    }
                }
            }

            if (remaining < steps / 2)
            {
                if (!branchedLeft && _rand.Next(3) == 0)
                {
                    branchedLeft = true;
                    ChasmRunnerSideways((int)posX, (int)posY, -1, _rand.Next(20, 40));
                }
                if (!branchedRight && _rand.Next(3) == 0)
                {
                    branchedRight = true;
                    ChasmRunnerSideways((int)posX, (int)posY, 1, _rand.Next(20, 40));
                }
            }

            posX += velX;
            posY += velY;

            velX += _rand.Next(-10, 11) * 0.05;
            velY += _rand.Next(-10, 11) * 0.02;
            velX = Math.Clamp(velX, -1.0, 1.0);
            velY = Math.Clamp(velY, 0.5, 2.5);
            width -= 0.1;
        }
    }

    private void ChasmRunnerSideways(int startX, int startY, int direction, int steps)
    {
        double width = _rand.Next(7, 12);
        double posX = startX;
        double posY = startY;
        double velX = direction * (_rand.Next(5, 20) * 0.1);
        double velY = _rand.Next(-10, 11) * 0.05;

        while (steps > 0 && width > 2)
        {
            steps--;

            int x0 = Math.Max(1, (int)(posX - width * 0.5));
            int x1 = Math.Min(_world.TilesWide - 1, (int)(posX + width * 0.5));
            int y0 = Math.Max(1, (int)(posY - width * 0.3));
            int y1 = Math.Min(_world.TilesHigh - 1, (int)(posY + width * 0.3));

            for (int tx = x0; tx < x1; tx++)
            {
                for (int ty = y0; ty < y1; ty++)
                {
                    double dist = Math.Abs(tx - posX) + Math.Abs(ty - posY);
                    if (dist < width * 0.4)
                    {
                        _undo.SaveTile(_world, tx, ty);
                        _world.Tiles[tx, ty].ClearTile();
                    }
                }
            }

            posX += velX;
            posY += velY;

            velY += _rand.Next(-10, 11) * 0.05;
            velY = Math.Clamp(velY, -0.5, 0.5);
            width -= 0.15;
        }
    }

    // ── Crimson ─────────────────────────────────────────────────────────

    /// <summary>
    /// Apply Crimson biome using the specified shape.
    /// Default shape is "diagonalRight" (leans right, opposite to corruption).
    /// stone→crimstone, dirt→crimstone, grass→crimson grass, sand→crimsand, ice→red ice.
    /// Carves wider, more organic chasms than corruption.
    /// </summary>
    public int Crimson(int x, int y, int w, int depth, string shape = "diagonalRight")
    {
        int count = 0;
        int margin = 15;
        int startCol = Math.Max(1, x - margin);
        int endCol = Math.Min(_world.TilesWide - 1, x + w + margin);

        for (int col = startCol; col < endCol; col++)
        {
            int surface = FindSurface(col, Math.Max(1, y - 30), y + depth);
            if (surface < 0) surface = y;

            int bottom = Math.Min(surface + depth, _world.TilesHigh - 1);

            for (int ty = surface; ty < bottom; ty++)
            {
                if (!IsInsideShape(col, ty, x, y, w, depth, shape)) continue;

                if (_world.Tiles[col, ty].IsActive)
                {
                    ushort cur = _world.Tiles[col, ty].Type;
                    ushort target = cur switch
                    {
                        TILE_STONE => TILE_CRIMSTONE,
                        TILE_DIRT => TILE_CRIMSTONE,
                        TILE_GRASS => TILE_CRIMSON_GRASS,
                        TILE_SAND => TILE_CRIMSAND,
                        TILE_ICE => TILE_RED_ICE,
                        _ => cur,
                    };

                    if (target != cur)
                    {
                        _undo.SaveTile(_world, col, ty);
                        _world.Tiles[col, ty].Type = target;
                        count++;
                    }
                }

                ushort wall = _world.Tiles[col, ty].Wall;
                if (wall == WALL_DIRT || wall == 1)
                {
                    _undo.SaveTile(_world, col, ty);
                    _world.Tiles[col, ty].Wall = WALL_CRIMSTONE;
                }
            }
        }

        // Crimson chasms — wider, more organic
        int chasmCount = Math.Max(1, w / 50);
        for (int i = 0; i < chasmCount; i++)
        {
            int cx = x + _rand.Next(10, Math.Max(11, w - 10));
            int surface = FindSurface(cx, Math.Max(1, y - 10), y + 50);
            if (surface < 0) surface = y;
            CrimsonChasmRunner(cx, surface, _rand.Next(30, 50));
        }

        return count;
    }

    private void CrimsonChasmRunner(int startX, int startY, int steps)
    {
        double width = _rand.Next(8, 15);
        double posX = startX;
        double posY = startY;
        double velX = _rand.Next(-10, 11) * 0.15;
        double velY = _rand.Next(10, 20) * 0.15;
        int remaining = steps;

        while (remaining > 0 && width > 3)
        {
            remaining--;

            int x0 = Math.Max(1, (int)(posX - width * 0.5));
            int x1 = Math.Min(_world.TilesWide - 1, (int)(posX + width * 0.5));
            int y0 = Math.Max(1, (int)(posY - width * 0.4));
            int y1 = Math.Min(_world.TilesHigh - 1, (int)(posY + width * 0.4));

            for (int tx = x0; tx < x1; tx++)
            {
                for (int ty = y0; ty < y1; ty++)
                {
                    double dx = Math.Abs(tx - posX);
                    double dy = Math.Abs(ty - posY);
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist < width * 0.4)
                    {
                        _undo.SaveTile(_world, tx, ty);
                        _world.Tiles[tx, ty].ClearTile();
                    }
                    else if (dist < width * 0.6)
                    {
                        if (_world.Tiles[tx, ty].IsActive)
                        {
                            var tp = WorldConfiguration.GetTileProperties((int)_world.Tiles[tx, ty].Type);
                            if (tp == null || !tp.IsFramed)
                            {
                                _undo.SaveTile(_world, tx, ty);
                                _world.Tiles[tx, ty].Type = TILE_CRIMSTONE;
                            }
                        }
                    }
                }
            }

            posX += velX;
            posY += velY;

            velX += _rand.Next(-10, 11) * 0.1;
            velY += _rand.Next(-5, 6) * 0.02;
            velX = Math.Clamp(velX, -2.0, 2.0);
            velY = Math.Clamp(velY, 0.3, 2.0);
            width -= 0.15;
        }
    }

    // ── Hallow ──────────────────────────────────────────────────────────

    /// <summary>
    /// Apply Hallow biome using the specified shape.
    /// Default shape is "diagonalLeft" (diagonal like hardmode V-strip).
    /// Converts: stone→pearlstone, grass→hallowed grass, sand→pearlsand,
    /// ice→pink ice, dirt→hallowed dirt. Returns tiles converted.
    /// </summary>
    public int Hallow(int x, int y, int w, int h, string shape = "diagonalLeft")
    {
        int count = 0;
        int margin = 10;
        int startCol = Math.Max(1, x - margin);
        int endCol = Math.Min(_world.TilesWide - 1, x + w + margin);

        for (int col = startCol; col < endCol; col++)
        {
            int surface = FindSurface(col, Math.Max(1, y - 30), y + h);
            if (surface < 0) surface = y;

            int bottom = Math.Min(surface + h, _world.TilesHigh - 1);

            for (int ty = surface; ty < bottom; ty++)
            {
                if (!IsInsideShape(col, ty, x, y, w, h, shape)) continue;
                if (!_world.Tiles[col, ty].IsActive) continue;

                ushort cur = _world.Tiles[col, ty].Type;
                ushort target = cur switch
                {
                    TILE_STONE => TILE_PEARLSTONE,
                    TILE_GRASS => TILE_HALLOWED_GRASS,
                    TILE_SAND => TILE_PEARLSAND,
                    TILE_ICE => TILE_PINK_ICE,
                    TILE_DIRT => TILE_HALLOWED_DIRT,
                    _ => cur,
                };

                if (target != cur)
                {
                    _undo.SaveTile(_world, col, ty);
                    _world.Tiles[col, ty].Type = target;
                    count++;
                }
            }
        }

        return count;
    }

    // ── Spider Cave ────────────────────────────────────────────────────

    /// <summary>
    /// Create a spider cave: a small cave filled with cobwebs and spider walls.
    /// </summary>
    public void SpiderCave(int x, int y, double strength = 10.0)
    {
        if (!_world.ValidTileLocation(x, y)) return;

        int caves = _rand.Next(2, 5);
        for (int i = 0; i < caves; i++)
        {
            int cx = x + _rand.Next(-10, 11);
            int cy = y + _rand.Next(-6, 7);
            Tunnel(cx, cy, _rand.Next(4, (int)strength), _rand.Next(8, 25));
        }

        int radius = (int)(strength * 1.5);
        PlaceWallBlob(x, y, radius, WALL_SPIDER);

        int x0 = Math.Max(1, x - radius);
        int x1 = Math.Min(_world.TilesWide - 1, x + radius);
        int y0 = Math.Max(1, y - radius);
        int y1 = Math.Min(_world.TilesHigh - 1, y + radius);

        for (int tx = x0; tx < x1; tx++)
        {
            for (int ty = y0; ty < y1; ty++)
            {
                if (!_world.Tiles[tx, ty].IsActive &&
                    _world.Tiles[tx, ty].Wall == WALL_SPIDER &&
                    _rand.Next(3) != 0)
                {
                    SaveAndSet(tx, ty, TILE_COBWEB);
                }
            }
        }
    }

    // ── Biome Helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Check if a tile has any cardinal neighbor that is empty (air).
    /// </summary>
    private bool IsExposedToAir(int x, int y)
    {
        if (y > 0 && !_world.Tiles[x, y - 1].IsActive) return true;
        if (y < _world.TilesHigh - 1 && !_world.Tiles[x, y + 1].IsActive) return true;
        if (x > 0 && !_world.Tiles[x - 1, y].IsActive) return true;
        if (x < _world.TilesWide - 1 && !_world.Tiles[x + 1, y].IsActive) return true;
        return false;
    }

    /// <summary>
    /// Place walls in a roughly circular area around (cx, cy).
    /// </summary>
    private void PlaceWallBlob(int cx, int cy, int radius, ushort wallId)
    {
        int x0 = Math.Max(1, cx - radius);
        int x1 = Math.Min(_world.TilesWide - 1, cx + radius);
        int y0 = Math.Max(1, cy - radius);
        int y1 = Math.Min(_world.TilesHigh - 1, cy + radius);

        for (int tx = x0; tx < x1; tx++)
        {
            for (int ty = y0; ty < y1; ty++)
            {
                double dx = tx - cx;
                double dy = ty - cy;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                double threshold = radius * (0.8 + _rand.NextDouble() * 0.4);
                if (dist < threshold)
                {
                    _undo.SaveTile(_world, tx, ty);
                    _world.Tiles[tx, ty].Wall = wallId;
                }
            }
        }
    }
}
