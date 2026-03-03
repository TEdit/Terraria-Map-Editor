using System;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public partial class GenerateApi
{
    // ── WorldGen Structures ─────────────────────────────────────────────

    /// <summary>
    /// Returns the list of supported ore type names and their tile IDs.
    /// </summary>
    public object[] ListOreTypes()
    {
        var result = new object[OreTypes.Length];
        for (int i = 0; i < OreTypes.Length; i++)
            result[i] = new { name = OreTypes[i].Name, tileId = (int)OreTypes[i].TileId };
        return result;
    }

    /// <summary>
    /// Port of WorldGen.TileRunner — wandering painter that fills diamond-shaped blobs.
    /// Starts at (x,y), takes `steps` iterations. Each step fills a radius that tapers
    /// linearly from `strength` to 0. Skips frame-important tiles.
    /// </summary>
    public void TileRunner(int x, int y, double strength, int steps, int tileType,
        double speedX = 0.0, double speedY = 0.0)
    {
        TileRunnerCore(x, y, strength, steps, tileType, false, speedX, speedY);
    }

    /// <summary>
    /// Same algorithm as TileRunner but clears tiles instead of placing them,
    /// creating natural-looking cave tunnels.
    /// </summary>
    public void Tunnel(int x, int y, double strength, int steps,
        double speedX = 0.0, double speedY = 0.0)
    {
        TileRunnerCore(x, y, strength, steps, -1, true, speedX, speedY);
    }

    /// <summary>
    /// Port of WorldGen.Lakinater — creates an irregular liquid pool.
    /// Two-pass: carves an irregular cavity, then fills liquid from the bottom up
    /// so it settles naturally (TEdit has no liquid physics).
    /// </summary>
    public void Lake(int x, int y, string liquidType = "water", double strength = 1.0)
    {
        LiquidType liqType = liquidType?.ToLowerInvariant() switch
        {
            "lava" => LiquidType.Lava,
            "honey" => LiquidType.Honey,
            "shimmer" => LiquidType.Shimmer,
            _ => LiquidType.Water,
        };

        double num1 = _rand.Next(25, 50) * strength;
        double num2 = num1;
        double num3 = _rand.Next(30, 80);

        // 20% chance of a bigger lake
        if (_rand.Next(5) == 0)
        {
            num1 *= 1.5;
            num2 *= 1.5;
            num3 *= 1.2;
        }

        // Track carved region bounds for the fill pass
        int regionMinX = _world.TilesWide, regionMaxX = 0;
        int regionMinY = _world.TilesHigh, regionMaxY = 0;

        double posX = x;
        double posY = y - num3 * 0.3;
        double velX = (_rand.Next(-10, 11)) * 0.1;
        double velY = (_rand.Next(-20, -10)) * 0.1;

        // Pass 1: Carve the cavity (remove tiles, no liquid yet)
        while (num1 > 0.0 && num3 > 0.0)
        {
            num1 -= _rand.Next(3);
            num3--;

            // Current blob radius with ±20% jitter
            num2 = num1 * (_rand.Next(80, 121)) * 0.01;

            int x0 = Math.Max(1, (int)(posX - num1 * 0.5));
            int x1 = Math.Min(_world.TilesWide - 1, (int)(posX + num1 * 0.5));
            int y0 = Math.Max(1, (int)(posY - num1 * 0.5));
            int y1 = Math.Min(_world.TilesHigh - 1, (int)(posY + num1 * 0.5));

            for (int tx = x0; tx < x1; tx++)
            {
                for (int ty = y0; ty < y1; ty++)
                {
                    double dx = Math.Abs(tx - posX);
                    double dy = Math.Abs(ty - posY);
                    if (Math.Sqrt(dx * dx + dy * dy) < num2 * 0.4)
                    {
                        _undo.SaveTile(_world, tx, ty);
                        _world.Tiles[tx, ty].ClearTile();
                        _world.Tiles[tx, ty].LiquidAmount = 0;
                        _world.Tiles[tx, ty].LiquidType = LiquidType.None;

                        if (tx < regionMinX) regionMinX = tx;
                        if (tx > regionMaxX) regionMaxX = tx;
                        if (ty < regionMinY) regionMinY = ty;
                        if (ty > regionMaxY) regionMaxY = ty;
                    }
                }
            }

            posX += velX;
            posY += velY;

            velX += (_rand.Next(-10, 11)) * 0.05;
            velY += (_rand.Next(-10, 11)) * 0.05;
            velX = Math.Clamp(velX, -0.5, 0.5);
            velY = Math.Clamp(velY, 0.5, 1.5);
        }

        if (regionMinX > regionMaxX) return; // nothing carved

        // Pass 2: Fill liquid from bottom up in each column.
        // Water line is 20% from the top (leaves air pocket at top, fills bottom 80%).
        int cavityHeight = regionMaxY - regionMinY;
        int waterLine = regionMinY + (int)(cavityHeight * 0.2);

        for (int tx = regionMinX; tx <= regionMaxX; tx++)
        {
            for (int ty = regionMaxY; ty >= regionMinY; ty--)
            {
                if (_world.Tiles[tx, ty].IsActive) continue; // hit solid floor

                if (ty > waterLine)
                {
                    // Below water line — full liquid
                    _world.Tiles[tx, ty].LiquidAmount = 255;
                    _world.Tiles[tx, ty].LiquidType = liqType;
                }
                else
                {
                    // Above water line — air pocket
                    _world.Tiles[tx, ty].LiquidAmount = 0;
                    _world.Tiles[tx, ty].LiquidType = LiquidType.None;
                }
            }
        }
    }

    /// <summary>
    /// Convenience wrapper around TileRunner with named ore types and preset parameters.
    /// Size: "small" (0.5x), "medium" (1.0x), "large" (2.0x).
    /// </summary>
    public void OreVein(string oreName, int x, int y, string size = "medium")
    {
        if (!OreTypeMap.TryGetValue(oreName, out var info))
            return;

        double multiplier = size?.ToLowerInvariant() switch
        {
            "small" => 0.5,
            "large" => 2.0,
            _ => 1.0,
        };

        double str = info.DefaultStrength * multiplier;
        int steps = (int)(info.DefaultSteps * multiplier);
        if (steps < 1) steps = 1;

        TileRunnerCore(x, y, str, steps, info.TileId, false, 0.0, 0.0);
    }

    /// <summary>
    /// Core wandering-painter algorithm shared by TileRunner and Tunnel.
    /// Port of Terraria's WorldGen.TileRunner, stripped of world-seed checks.
    /// </summary>
    private void TileRunnerCore(int x, int y, double strength, int steps,
        int tileType, bool clearMode, double speedX, double speedY)
    {
        double remaining = steps;
        double total = steps;
        double posX = x;
        double posY = y;

        double velX = speedX != 0.0 ? speedX : (_rand.Next(-10, 11)) * 0.1;
        double velY = speedY != 0.0 ? speedY : (_rand.Next(-10, 11)) * 0.1;

        while (strength > 0.0 && remaining > 0.0)
        {
            double radius = strength * (remaining / total);
            remaining--;

            int x0 = Math.Max(1, (int)(posX - radius * 0.5));
            int x1 = Math.Min(_world.TilesWide - 1, (int)(posX + radius * 0.5));
            int y0 = Math.Max(1, (int)(posY - radius * 0.5));
            int y1 = Math.Min(_world.TilesHigh - 1, (int)(posY + radius * 0.5));

            for (int tx = x0; tx < x1; tx++)
            {
                for (int ty = y0; ty < y1; ty++)
                {
                    double dist = Math.Abs(tx - posX) + Math.Abs(ty - posY);
                    double threshold = strength * 0.5 * (1.0 + (_rand.Next(-10, 11)) * 0.015);

                    if (dist < threshold)
                    {
                        if (clearMode)
                        {
                            _undo.SaveTile(_world, tx, ty);
                            _world.Tiles[tx, ty].ClearTile();
                        }
                        else
                        {
                            // Skip frame-important tiles (furniture, plants, etc.)
                            var tp = WorldConfiguration.GetTileProperties((int)_world.Tiles[tx, ty].Type);
                            if (_world.Tiles[tx, ty].IsActive && tp != null && tp.IsFramed)
                                continue;

                            SaveAndSet(tx, ty, (ushort)tileType);
                        }
                    }
                }
            }

            posX += velX;
            posY += velY;

            velX += (_rand.Next(-10, 11)) * 0.05;
            velY += (_rand.Next(-10, 11)) * 0.05;
            velX = Math.Clamp(velX, -1.0, 1.0);
            velY = Math.Clamp(velY, -1.0, 1.0);
        }
    }

    /// <summary>
    /// Scans downward from yStart to yEnd at column x, returns the y-coordinate
    /// of the first active solid tile (the surface). Returns -1 if no surface found.
    /// </summary>
    public int FindSurface(int x, int yStart, int yEnd)
    {
        yStart = Math.Max(0, yStart);
        yEnd = Math.Min(_world.TilesHigh - 1, yEnd);

        if (!_world.ValidTileLocation(x, 0))
            return -1;

        for (int y = yStart; y <= yEnd; y++)
        {
            if (_world.Tiles[x, y].IsActive)
                return y;
        }

        return -1;
    }
}
