using System;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public partial class GenerateApi
{
    // ── Structure Tile IDs ─────────────────────────────────────────────

    private const ushort TILE_WOOD = 30;
    private const ushort TILE_OBSIDIAN = 56;
    private const ushort TILE_CORAL = 81;
    private const ushort TILE_LIVING_WOOD = 191;
    private const ushort TILE_LIHZAHRD_BRICK = 226;
    private const ushort TILE_SANDSTONE_BRICK = 151;
    private const ushort TILE_DUNGEON_BLUE = 41;
    private const ushort TILE_DUNGEON_GREEN = 43;
    private const ushort TILE_DUNGEON_PINK = 44;
    private const ushort TILE_PLATFORM = 19;

    private const ushort WALL_LIVING_WOOD = 78;
    private const ushort WALL_LIHZAHRD = 87;
    private const ushort WALL_DUNGEON_BLUE = 7;
    private const ushort WALL_DUNGEON_GREEN = 8;
    private const ushort WALL_DUNGEON_PINK = 9;
    private const ushort WALL_SANDSTONE_BRICK = 10;

    // ── Ocean ──────────────────────────────────────────────────────────

    /// <summary>
    /// Generate an ocean biome with exponential depth progression like Terraria's
    /// TuneOceanDepth. Sand floor slopes down exponentially from beach to deep water.
    /// direction: -1 (left/west edge) or 1 (right/east edge).
    /// oceanWidth: how many tiles wide the ocean is (default ~15% of world).
    /// maxDepth: maximum ocean depth in tiles (default 80).
    /// </summary>
    public int Ocean(int direction = 1, int oceanWidth = 0, int maxDepth = 80)
    {
        int count = 0;
        int w = _world.TilesWide;

        if (oceanWidth <= 0) oceanWidth = Math.Max(80, (int)(w * 0.15));

        // Determine ocean horizontal extent
        int oceanStart, oceanEnd;
        if (direction < 0)
        {
            oceanStart = 1;
            oceanEnd = Math.Min(w - 1, oceanWidth);
        }
        else
        {
            oceanStart = Math.Max(1, w - oceanWidth);
            oceanEnd = w - 1;
        }

        // Find the baseline surface near the beach edge (inland side)
        int inlandCol = direction < 0 ? oceanEnd : oceanStart;
        int baseSurface = FindSurface(inlandCol, 1, _world.TilesHigh / 2);
        if (baseSurface < 0) baseSurface = (int)(_world.TilesHigh * 0.3);

        int seaLevel = baseSurface;
        double sandJitter = 0;

        for (int col = oceanStart; col < oceanEnd; col++)
        {
            // Distance from the inland edge (0 at beach, 1 at world edge)
            double distFromBeach;
            if (direction < 0)
                distFromBeach = 1.0 - (double)(col - oceanStart) / (oceanEnd - oceanStart);
            else
                distFromBeach = (double)(col - oceanStart) / (oceanEnd - oceanStart);

            // Exponential depth: shallow near beach, deep at edge
            double k = 3.5;
            double depth = maxDepth * (1.0 - Math.Exp(-k * distFromBeach));

            sandJitter += (_rand.NextDouble() - 0.5) * 3;
            sandJitter = Math.Clamp(sandJitter, -8, 8);

            int sandFloorY = seaLevel + (int)(depth + sandJitter);
            sandFloorY = Math.Min(sandFloorY, _world.TilesHigh - 20);

            // Find where existing solid ground is below the sand floor.
            // We need to fill sand all the way down to seal any gaps/caves.
            int solidBelow = sandFloorY;
            for (int ty = sandFloorY; ty < _world.TilesHigh - 6; ty++)
            {
                if (_world.Tiles[col, ty].IsActive)
                {
                    solidBelow = ty;
                    break;
                }
                solidBelow = ty;
            }

            // Sand extends from sandFloorY to at least solidBelow, plus 8 tiles
            // into the existing terrain to seal caves near the surface
            int sandBottom = Math.Min(Math.Max(solidBelow + 8, sandFloorY + 15), _world.TilesHigh - 6);

            // Clear terrain from sea level down to sand floor (the water column)
            // Don't clear above sea level — that's sky and should stay empty
            for (int ty = seaLevel; ty < sandFloorY; ty++)
            {
                if (!_world.ValidTileLocation(col, ty)) continue;
                if (_world.Tiles[col, ty].IsActive)
                {
                    _undo.SaveTile(_world, col, ty);
                    _world.Tiles[col, ty].ClearTile();
                }
            }

            // Place water from sea level to sand floor
            for (int ty = seaLevel; ty < sandFloorY; ty++)
            {
                if (!_world.ValidTileLocation(col, ty)) continue;
                if (!_world.Tiles[col, ty].IsActive)
                {
                    _undo.SaveTile(_world, col, ty);
                    _world.Tiles[col, ty].LiquidAmount = 255;
                    _world.Tiles[col, ty].LiquidType = LiquidType.Water;
                }
            }

            // Place sand/sandstone from sand floor down to seal all gaps
            for (int ty = sandFloorY; ty < sandBottom; ty++)
            {
                if (!_world.ValidTileLocation(col, ty)) continue;
                ushort tileType = ty < sandFloorY + 8 ? TILE_SAND : TILE_SANDSTONE;
                SaveAndSet(col, ty, tileType);
                count++;
            }
        }

        // Scatter coral on the ocean floor
        int coralCount = Math.Max(5, oceanWidth / 6);
        for (int i = 0; i < coralCount; i++)
        {
            int cx = _rand.Next(oceanStart, oceanEnd);
            int surface = FindSurface(cx, seaLevel, seaLevel + maxDepth + 20);
            if (surface > 0 && surface > 1)
            {
                int coralY = surface - 1;
                if (_world.ValidTileLocation(cx, coralY) && !_world.Tiles[cx, coralY].IsActive)
                {
                    SaveAndSet(cx, coralY, TILE_CORAL);
                }
            }
        }

        return count;
    }

    // ── Desert ─────────────────────────────────────────────────────────

    /// <summary>
    /// Generate a desert biome using the specified shape.
    /// Default shape is "ellipse" (tall ellipse spanning the map height like Terraria's desert).
    /// Sand on surface, hardened sand in middle, sandstone underground.
    /// </summary>
    public int Desert(int x, int y, int w, int h, string shape = "ellipse")
    {
        int count = 0;
        int margin = 10;
        int x1 = Math.Max(1, x - margin);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w + margin);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);

        int surfaceDepth = Math.Max(10, h / 4);

        for (int tx = x1; tx < x2; tx++)
        {
            for (int ty = y1; ty < y2; ty++)
            {
                if (!IsInsideShape(tx, ty, x, y, w, h, shape)) continue;
                if (!_world.Tiles[tx, ty].IsActive) continue;

                var tp = WorldConfiguration.GetTileProperties((int)_world.Tiles[tx, ty].Type);
                if (tp != null && tp.IsFramed) continue;

                _undo.SaveTile(_world, tx, ty);
                if (ty < y + surfaceDepth)
                {
                    _world.Tiles[tx, ty].Type = TILE_SAND;
                }
                else if (ty < y + surfaceDepth * 3)
                {
                    _world.Tiles[tx, ty].Type = TILE_HARDENED_SAND;
                }
                else
                {
                    _world.Tiles[tx, ty].Type = TILE_SANDSTONE;
                }
                count++;
            }
        }

        // Place sandstone walls underground (inside shape only)
        for (int tx = Math.Max(1, x + 2); tx < Math.Min(_world.TilesWide - 1, x + w - 2); tx++)
        {
            for (int ty = y + surfaceDepth; ty < y2; ty++)
            {
                if (!_world.ValidTileLocation(tx, ty)) continue;
                if (!IsInsideShape(tx, ty, x, y, w, h, shape)) continue;
                if (_world.Tiles[tx, ty].Wall == 0)
                {
                    _undo.SaveTile(_world, tx, ty);
                    _world.Tiles[tx, ty].Wall = WALL_SANDSTONE;
                }
            }
        }

        // Carve some desert caves
        int caves = Math.Max(2, (w * h) / 10000);
        for (int i = 0; i < caves; i++)
        {
            int cx = x + _rand.Next(w);
            int cy = y + surfaceDepth + _rand.Next(Math.Max(1, h - surfaceDepth));
            Tunnel(cx, cy, _rand.Next(4, 10), _rand.Next(10, 40));
        }

        return count;
    }

    // ── Jungle ─────────────────────────────────────────────────────────

    /// <summary>
    /// Generate a jungle biome using the specified shape.
    /// Default shape is "rectangle" — fills the entire vertical space like Terraria's jungle.
    /// Edges use dithering (probability falloff) instead of hard boundaries for organic shape.
    /// ditherWidth: width of the dithered edge zone in tiles (default 15).
    /// Converts existing terrain to mud/jungle grass with caves.
    /// </summary>
    public int Jungle(int x, int y, int w, int h, string shape = "rectangle", int ditherWidth = 15)
    {
        int count = 0;
        int margin = ditherWidth + 5;
        int startCol = Math.Max(1, x - margin);
        int endCol = Math.Min(_world.TilesWide - 1, x + w + margin);

        // First pass: convert terrain to mud with dithered edges
        for (int col = startCol; col < endCol; col++)
        {
            int surface = FindSurface(col, Math.Max(1, y - 20), y + h);
            if (surface < 0) surface = y;

            int bottom = Math.Min(surface + h, _world.TilesHigh - 1);

            for (int ty = surface; ty < bottom; ty++)
            {
                if (!IsInsideShape(col, ty, x, y, w, h, shape)) continue;

                // Dithered edges: probability decreases near horizontal boundaries
                double ditherProb = 1.0;
                if (ditherWidth > 0)
                {
                    int distFromLeft = col - x;
                    int distFromRight = (x + w) - col;
                    int edgeDist = Math.Min(distFromLeft, distFromRight);
                    if (edgeDist < ditherWidth)
                    {
                        ditherProb = (double)edgeDist / ditherWidth;
                        ditherProb = Math.Clamp(ditherProb, 0.0, 1.0);
                    }
                }

                if (ditherProb < 1.0 && _rand.NextDouble() > ditherProb) continue;

                if (_world.Tiles[col, ty].IsActive)
                {
                    ushort cur = _world.Tiles[col, ty].Type;
                    if (cur == TILE_STONE || cur == TILE_DIRT || cur == TILE_GRASS)
                    {
                        _undo.SaveTile(_world, col, ty);
                        _world.Tiles[col, ty].Type = TILE_MUD;
                        count++;
                    }
                }

                // Place mud walls
                ushort wall = _world.Tiles[col, ty].Wall;
                if (wall == WALL_DIRT || wall == 1)
                {
                    _undo.SaveTile(_world, col, ty);
                    _world.Tiles[col, ty].Wall = WALL_MUD;
                }
            }
        }

        // Second pass: convert exposed mud to jungle grass
        for (int col = startCol; col < endCol; col++)
        {
            int surface = FindSurface(col, Math.Max(1, y - 20), y + h);
            if (surface < 0) surface = y;
            int bottom = Math.Min(surface + h, _world.TilesHigh - 1);

            for (int ty = surface; ty < bottom; ty++)
            {
                if (!IsInsideShape(col, ty, x, y, w, h, shape)) continue;
                if (!_world.Tiles[col, ty].IsActive) continue;
                if (_world.Tiles[col, ty].Type != TILE_MUD) continue;

                if (IsExposedToAir(col, ty))
                {
                    _undo.SaveTile(_world, col, ty);
                    _world.Tiles[col, ty].Type = TILE_JUNGLE_GRASS;
                    count++;
                }
            }
        }

        // Carve jungle caves (more numerous)
        int caves = Math.Max(5, (w * h) / 3000);
        for (int i = 0; i < caves; i++)
        {
            int cx = x + _rand.Next(w);
            int cy = y + _rand.Next(h);
            Tunnel(cx, cy, _rand.Next(5, 12), _rand.Next(15, 60));
        }

        // Add mud blobs for natural edges (extends into dither zone)
        int blobs = Math.Max(3, (w * h) / 4000);
        for (int i = 0; i < blobs; i++)
        {
            int bx = x + _rand.Next(-ditherWidth, w + ditherWidth);
            int by = y + _rand.Next(h);
            bx = Math.Clamp(bx, 1, _world.TilesWide - 2);
            TileRunnerCore(bx, by, _rand.Next(4, 10), _rand.Next(8, 25), TILE_MUD, false, 0, 0);
        }

        return count;
    }

    // ── Underworld ─────────────────────────────────────────────────────

    /// <summary>
    /// Generate the underworld (hell) layer with natural features:
    /// - Random walk ceiling (jagged ash ceiling around h-200, ±3/column)
    /// - Random walk lava floor (around h-50, ±10/column)
    /// - Ash blocks between ceiling and floor
    /// - Cave openings/chimneys (1-in-50 chance per column)
    /// - Hellstone veins scattered in ash
    /// - Obsidian near lava level
    /// - Lava filling open spaces below the lava floor level
    /// yStart: optional override for underworld top (default: auto from world height)
    /// </summary>
    public int Underworld(int yStart = 0)
    {
        int count = 0;
        int worldW = _world.TilesWide;
        int worldH = _world.TilesHigh;

        // Default ceiling: start at lava line (80% of world height) to connect
        // seamlessly with the stone layer above. Terraria: maxTilesY - 150 to -190.
        int lavaLine = (int)(worldH * 0.8);
        if (yStart <= 0) yStart = lavaLine;

        // ── Pass 1: Generate ceiling and floor profiles via random walk ──

        int[] ceilingY = new int[worldW];
        int[] lavaFloorY = new int[worldW];

        double ceilWalk = yStart;
        double floorWalk = worldH - _rand.Next(40, 70);

        for (int col = 0; col < worldW; col++)
        {
            // Ceiling random walk: ±3 per column, clamped
            ceilWalk += (_rand.NextDouble() - 0.5) * 6;
            ceilWalk = Math.Clamp(ceilWalk, yStart - 30, yStart + 30);
            ceilingY[col] = (int)ceilWalk;

            // Lava floor random walk: ±10 per column, more dramatic
            floorWalk += (_rand.NextDouble() - 0.5) * 20;
            floorWalk = Math.Clamp(floorWalk, worldH - 80, worldH - 25);
            lavaFloorY[col] = (int)floorWalk;
        }

        // ── Pass 2: Fill ash between ceiling and world bottom ──

        for (int col = 1; col < worldW - 1; col++)
        {
            int ceil = ceilingY[col];
            int floor = worldH - 6;

            for (int ty = ceil; ty < floor; ty++)
            {
                if (!_world.ValidTileLocation(col, ty)) continue;
                _undo.SaveTile(_world, col, ty);
                _world.Tiles[col, ty].IsActive = true;
                _world.Tiles[col, ty].Type = TILE_ASH;
                count++;
            }
        }

        // ── Pass 3: Carve caves and chimneys ──

        // Main underworld caves — large horizontal tunnels
        int mainCaves = Math.Max(10, worldW / 80);
        for (int i = 0; i < mainCaves; i++)
        {
            int cx = _rand.Next(50, worldW - 50);
            int cy = ceilingY[Math.Clamp(cx, 0, worldW - 1)] + _rand.Next(20, 80);
            cy = Math.Min(cy, worldH - 30);
            Tunnel(cx, cy, _rand.Next(8, 20), _rand.Next(40, 200));
        }

        // Smaller caves for texture
        int smallCaves = Math.Max(20, worldW / 40);
        for (int i = 0; i < smallCaves; i++)
        {
            int cx = _rand.Next(50, worldW - 50);
            int cy = ceilingY[Math.Clamp(cx, 0, worldW - 1)] + _rand.Next(10, 100);
            cy = Math.Min(cy, worldH - 20);
            Tunnel(cx, cy, _rand.Next(4, 12), _rand.Next(10, 60));
        }

        // Chimney openings from underworld ceiling upward (1-in-50 columns)
        for (int col = 50; col < worldW - 50; col++)
        {
            if (_rand.Next(50) != 0) continue;

            int chimneyBottom = ceilingY[col];
            int chimneyTop = chimneyBottom - _rand.Next(30, 80);
            chimneyTop = Math.Max(1, chimneyTop);

            // Carve a vertical shaft upward
            int chimneyWidth = _rand.Next(3, 7);
            for (int ty = chimneyTop; ty < chimneyBottom; ty++)
            {
                for (int dx = -chimneyWidth / 2; dx <= chimneyWidth / 2; dx++)
                {
                    int tx = col + dx;
                    if (_world.ValidTileLocation(tx, ty))
                    {
                        _undo.SaveTile(_world, tx, ty);
                        _world.Tiles[tx, ty].ClearTile();
                    }
                }
            }
        }

        // ── Pass 4: Place hellstone veins ──

        int hellstoneCount = Math.Max(15, worldW / 40);
        for (int i = 0; i < hellstoneCount; i++)
        {
            int ox = _rand.Next(50, worldW - 50);
            int ceil = ceilingY[Math.Clamp(ox, 0, worldW - 1)];
            int oy = ceil + _rand.Next(20, Math.Max(21, worldH - ceil - 30));
            oy = Math.Min(oy, worldH - 20);
            TileRunnerCore(ox, oy, _rand.Next(4, 10), _rand.Next(5, 20), TILE_HELLSTONE, false, 0, 0);
        }

        // ── Pass 5: Place obsidian near lava level ──

        int obsidianCount = Math.Max(8, worldW / 60);
        for (int i = 0; i < obsidianCount; i++)
        {
            int ox = _rand.Next(50, worldW - 50);
            int lavaLevel = lavaFloorY[Math.Clamp(ox, 0, worldW - 1)];
            int oy = lavaLevel + _rand.Next(-15, 15);
            oy = Math.Clamp(oy, ceilingY[Math.Clamp(ox, 0, worldW - 1)] + 10, worldH - 15);
            TileRunnerCore(ox, oy, _rand.Next(3, 8), _rand.Next(5, 15), TILE_OBSIDIAN, false, 0, 0);
        }

        // ── Pass 6: Fill lava in open spaces below lava floor ──

        for (int col = 1; col < worldW - 1; col++)
        {
            int lavaLevel = lavaFloorY[col];

            for (int ty = lavaLevel; ty < worldH - 6; ty++)
            {
                if (!_world.ValidTileLocation(col, ty)) continue;
                if (!_world.Tiles[col, ty].IsActive)
                {
                    _undo.SaveTile(_world, col, ty);
                    _world.Tiles[col, ty].LiquidAmount = 255;
                    _world.Tiles[col, ty].LiquidType = LiquidType.Lava;
                }
            }
        }

        return count;
    }

    // ── Beehive ────────────────────────────────────────────────────────

    /// <summary>
    /// Create a beehive structure: hive block shell filled with honey.
    /// </summary>
    public void Beehive(int x, int y, int size = 0)
    {
        if (size <= 0) size = _rand.Next(15, 30);
        if (!_world.ValidTileLocation(x, y)) return;

        int radiusX = size;
        int radiusY = (int)(size * 0.7);

        // Place hive block shell
        for (int tx = x - radiusX; tx <= x + radiusX; tx++)
        {
            for (int ty = y - radiusY; ty <= y + radiusY; ty++)
            {
                if (!_world.ValidTileLocation(tx, ty)) continue;

                double dx = (double)(tx - x) / radiusX;
                double dy = (double)(ty - y) / radiusY;
                double dist = dx * dx + dy * dy;

                if (dist < 1.0)
                {
                    SaveAndSet(tx, ty, TILE_HIVE);
                    _world.Tiles[tx, ty].Wall = WALL_HIVE;
                }
            }
        }

        // Carve interior and fill with honey
        int innerX = (int)(radiusX * 0.7);
        int innerY = (int)(radiusY * 0.7);

        for (int tx = x - innerX; tx <= x + innerX; tx++)
        {
            for (int ty = y - innerY; ty <= y + innerY; ty++)
            {
                if (!_world.ValidTileLocation(tx, ty)) continue;

                double dx = (double)(tx - x) / innerX;
                double dy = (double)(ty - y) / innerY;
                double dist = dx * dx + dy * dy;

                double threshold = 1.0 - _rand.NextDouble() * 0.2;
                if (dist < threshold)
                {
                    _undo.SaveTile(_world, tx, ty);
                    _world.Tiles[tx, ty].ClearTile();
                    _world.Tiles[tx, ty].Wall = WALL_HIVE;

                    // Fill lower 60% with honey
                    if (ty > y - innerY * 0.4)
                    {
                        _world.Tiles[tx, ty].LiquidAmount = 255;
                        _world.Tiles[tx, ty].LiquidType = LiquidType.Honey;
                    }
                }
            }
        }
    }

    // ── Pyramid ────────────────────────────────────────────────────────

    /// <summary>
    /// Create a sandstone pyramid structure with an internal room.
    /// x,y is the tip (top) of the pyramid.
    /// </summary>
    public void Pyramid(int x, int y, int height = 0)
    {
        if (height <= 0) height = _rand.Next(40, 80);
        if (!_world.ValidTileLocation(x, y)) return;

        for (int row = 0; row < height; row++)
        {
            int halfWidth = row;
            int ty = y + row;

            for (int dx = -halfWidth; dx <= halfWidth; dx++)
            {
                int tx = x + dx;
                if (!_world.ValidTileLocation(tx, ty)) continue;

                SaveAndSet(tx, ty, TILE_SANDSTONE_BRICK);
                _world.Tiles[tx, ty].Wall = WALL_SANDSTONE_BRICK;
            }
        }

        // Carve internal shaft (diagonal corridor from tip)
        int direction = _rand.Next(2) == 0 ? -1 : 1;
        int shaftX = x;
        int shaftY = y + 5;

        for (int step = 0; step < height - 15; step++)
        {
            if (step % 3 < 2)
                shaftY++;
            else
                shaftX += direction;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int tx = shaftX + dx;
                    int ty2 = shaftY + dy;
                    if (_world.ValidTileLocation(tx, ty2))
                    {
                        _undo.SaveTile(_world, tx, ty2);
                        _world.Tiles[tx, ty2].ClearTile();
                    }
                }
            }
        }

        // Create a room at the end of the shaft
        int roomW = _rand.Next(8, 14);
        int roomH = _rand.Next(6, 10);
        for (int dx = -roomW / 2; dx <= roomW / 2; dx++)
        {
            for (int dy = -roomH / 2; dy <= roomH / 2; dy++)
            {
                int tx = shaftX + dx;
                int ty2 = shaftY + dy;
                if (_world.ValidTileLocation(tx, ty2))
                {
                    _undo.SaveTile(_world, tx, ty2);
                    _world.Tiles[tx, ty2].ClearTile();
                    _world.Tiles[tx, ty2].Wall = WALL_SANDSTONE_BRICK;
                }
            }
        }
    }

    // ── Living Tree ────────────────────────────────────────────────────

    /// <summary>
    /// Create a living tree: a large hollow trunk with living wood walls and a root system.
    /// x,y is the base (ground level). The tree grows upward.
    /// </summary>
    public void LivingTree(int x, int y, int height = 0)
    {
        if (height <= 0) height = _rand.Next(40, 80);
        if (!_world.ValidTileLocation(x, y)) return;

        int trunkWidth = _rand.Next(4, 8);
        int halfW = trunkWidth / 2;

        for (int seg = 0; seg < height; seg++)
        {
            int ty = y - seg;
            if (ty < 1) break;

            int segWidth = halfW;
            if (seg > height * 0.6)
            {
                segWidth = Math.Max(1, halfW - (seg - (int)(height * 0.6)) / 4);
            }

            for (int dx = -segWidth; dx <= segWidth; dx++)
            {
                int tx = x + dx;
                if (!_world.ValidTileLocation(tx, ty)) continue;

                SaveAndSet(tx, ty, TILE_LIVING_WOOD);
            }
        }

        // Hollow out interior
        for (int seg = 3; seg < height - 5; seg++)
        {
            int ty = y - seg;
            if (ty < 1) break;

            int innerW = Math.Max(1, halfW - 1);
            if (seg > height * 0.6)
            {
                innerW = Math.Max(0, innerW - (seg - (int)(height * 0.6)) / 4);
            }

            for (int dx = -innerW; dx <= innerW; dx++)
            {
                int tx = x + dx;
                if (!_world.ValidTileLocation(tx, ty)) continue;

                _undo.SaveTile(_world, tx, ty);
                _world.Tiles[tx, ty].ClearTile();
                _world.Tiles[tx, ty].Wall = WALL_LIVING_WOOD;
            }
        }

        // Roots going down
        int rootCount = _rand.Next(2, 5);
        for (int i = 0; i < rootCount; i++)
        {
            int rootX = x + _rand.Next(-halfW - 3, halfW + 4);
            int rootY = y;
            double velX = _rand.Next(-10, 11) * 0.2;
            int rootLen = _rand.Next(15, 40);

            for (int step = 0; step < rootLen; step++)
            {
                rootY++;
                rootX += (int)velX;
                velX += _rand.Next(-10, 11) * 0.05;
                velX = Math.Clamp(velX, -1.5, 1.5);

                for (int dx = -1; dx <= 1; dx++)
                {
                    if (_world.ValidTileLocation(rootX + dx, rootY))
                    {
                        SaveAndSet(rootX + dx, rootY, TILE_LIVING_WOOD);
                    }
                }
            }
        }

        // Canopy
        int canopyY = y - height + 3;
        int canopyR = _rand.Next(8, 15);
        for (int dx = -canopyR; dx <= canopyR; dx++)
        {
            for (int dy = -canopyR; dy <= canopyR / 2; dy++)
            {
                int tx = x + dx;
                int ty = canopyY + dy;
                if (!_world.ValidTileLocation(tx, ty)) continue;

                double dist = Math.Sqrt(dx * dx + dy * dy * 4);
                if (dist < canopyR - _rand.NextDouble() * 3)
                {
                    if (!_world.Tiles[tx, ty].IsActive)
                    {
                        SaveAndSet(tx, ty, 192); // leaf block
                    }
                }
            }
        }
    }

    // ── Dungeon ────────────────────────────────────────────────────────

    /// <summary>
    /// Generate a dungeon with branching room-and-corridor layout.
    /// Starts with an entrance shaft from surface, then generates rooms connected
    /// by corridors that can go left, right, or down with random branching.
    /// direction: -1 (left) or 1 (right). style: 0=blue, 1=green, 2=pink.
    /// </summary>
    public void Dungeon(int x, int y, int direction = 1, int style = 0)
    {
        ushort brickTile = style switch
        {
            1 => TILE_DUNGEON_GREEN,
            2 => TILE_DUNGEON_PINK,
            _ => TILE_DUNGEON_BLUE,
        };

        ushort brickWall = style switch
        {
            1 => WALL_DUNGEON_GREEN,
            2 => WALL_DUNGEON_PINK,
            _ => WALL_DUNGEON_BLUE,
        };

        int maxRooms = _rand.Next(10, 20);
        int roomsPlaced = 0;

        // ── Entrance shaft from surface going down ──
        int shaftDepth = _rand.Next(15, 30);
        int shaftWidth = 5;
        for (int dy = 0; dy < shaftDepth; dy++)
        {
            for (int dx = -shaftWidth / 2; dx <= shaftWidth / 2; dx++)
            {
                int tx = x + dx;
                int ty = y + dy;
                if (!_world.ValidTileLocation(tx, ty)) continue;

                bool isEdge = dx == -shaftWidth / 2 || dx == shaftWidth / 2;
                if (isEdge)
                {
                    SaveAndSet(tx, ty, brickTile);
                    _world.Tiles[tx, ty].Wall = brickWall;
                }
                else
                {
                    _undo.SaveTile(_world, tx, ty);
                    _world.Tiles[tx, ty].ClearTile();
                    _world.Tiles[tx, ty].Wall = brickWall;
                }
            }
        }

        // ── BFS room placement with branching ──
        // Queue stores (roomCenterX, roomTopY)
        var queue = new System.Collections.Generic.Queue<(int cx, int cy)>();
        queue.Enqueue((x, y + shaftDepth));

        while (queue.Count > 0 && roomsPlaced < maxRooms)
        {
            var (curX, curY) = queue.Dequeue();

            int roomW = _rand.Next(10, 20);
            int roomH = _rand.Next(8, 14);

            // Place room
            DungeonRoom(curX, curY, roomW, roomH, brickTile, brickWall);
            roomsPlaced++;

            // Add platform in tall rooms
            if (_rand.Next(3) != 0 && roomH > 8)
            {
                int platY = curY + roomH / 2;
                for (int dx = -roomW / 2 + 1; dx < roomW / 2; dx++)
                {
                    int tx = curX + dx;
                    if (_world.ValidTileLocation(tx, platY))
                    {
                        SaveAndSet(tx, platY, TILE_PLATFORM);
                    }
                }
            }

            // Create 1-3 corridors from this room (branching)
            int corridorCount = roomsPlaced == 1 ? _rand.Next(2, 4) : _rand.Next(1, 3);
            for (int c = 0; c < corridorCount && roomsPlaced + queue.Count < maxRooms; c++)
            {
                int corridorDir = _rand.Next(3); // 0=down, 1=left, 2=right
                int corridorLen = _rand.Next(8, 20);

                int nextX = curX;
                int nextY = curY + roomH;

                if (corridorDir == 0)
                {
                    // Vertical corridor going down
                    for (int step = 0; step < corridorLen; step++)
                    {
                        nextY++;
                        // Slight horizontal drift
                        if (_rand.Next(4) == 0) nextX += _rand.Next(-1, 2);

                        DungeonCorridorSlice(nextX, nextY, brickTile, brickWall, true);
                    }
                }
                else
                {
                    // Horizontal corridor going left or right
                    int hDir = corridorDir == 1 ? -1 : 1;
                    nextY = curY + _rand.Next(2, roomH - 2);

                    for (int step = 0; step < corridorLen; step++)
                    {
                        nextX += hDir;
                        // Slight vertical drift
                        if (_rand.Next(4) == 0) nextY += _rand.Next(-1, 2);

                        DungeonCorridorSlice(nextX, nextY, brickTile, brickWall, false);
                    }

                    // Drop down after horizontal corridor to create depth
                    int dropLen = _rand.Next(5, 15);
                    for (int step = 0; step < dropLen; step++)
                    {
                        nextY++;
                        DungeonCorridorSlice(nextX, nextY, brickTile, brickWall, true);
                    }
                }

                queue.Enqueue((nextX, nextY));
            }
        }
    }

    /// <summary>
    /// Place a single dungeon room centered at (cx, topY).
    /// </summary>
    private void DungeonRoom(int cx, int topY, int roomW, int roomH, ushort brickTile, ushort brickWall)
    {
        for (int dx = -roomW / 2; dx <= roomW / 2; dx++)
        {
            for (int dy = 0; dy < roomH; dy++)
            {
                int tx = cx + dx;
                int ty = topY + dy;
                if (!_world.ValidTileLocation(tx, ty)) continue;

                bool isEdge = dx == -roomW / 2 || dx == roomW / 2 || dy == 0 || dy == roomH - 1;

                if (isEdge)
                {
                    SaveAndSet(tx, ty, brickTile);
                    _world.Tiles[tx, ty].Wall = brickWall;
                }
                else
                {
                    _undo.SaveTile(_world, tx, ty);
                    _world.Tiles[tx, ty].ClearTile();
                    _world.Tiles[tx, ty].Wall = brickWall;
                }
            }
        }
    }

    /// <summary>
    /// Carve a single corridor cross-section at (cx, cy).
    /// vertical: true for vertical corridor (wider horizontally), false for horizontal (taller vertically).
    /// </summary>
    private void DungeonCorridorSlice(int cx, int cy, ushort brickTile, ushort brickWall, bool vertical)
    {
        int halfSpan = 2;
        if (vertical)
        {
            // Vertical corridor: 5 tiles wide
            for (int dx = -halfSpan; dx <= halfSpan; dx++)
            {
                int tx = cx + dx;
                if (!_world.ValidTileLocation(tx, cy)) continue;

                bool isEdge = dx == -halfSpan || dx == halfSpan;
                if (isEdge)
                {
                    SaveAndSet(tx, cy, brickTile);
                    _world.Tiles[tx, cy].Wall = brickWall;
                }
                else
                {
                    _undo.SaveTile(_world, tx, cy);
                    _world.Tiles[tx, cy].ClearTile();
                    _world.Tiles[tx, cy].Wall = brickWall;
                }
            }
        }
        else
        {
            // Horizontal corridor: 5 tiles tall
            for (int dy = -halfSpan; dy <= halfSpan; dy++)
            {
                int ty = cy + dy;
                if (!_world.ValidTileLocation(cx, ty)) continue;

                bool isEdge = dy == -halfSpan || dy == halfSpan;
                if (isEdge)
                {
                    SaveAndSet(cx, ty, brickTile);
                    _world.Tiles[cx, ty].Wall = brickWall;
                }
                else
                {
                    _undo.SaveTile(_world, cx, ty);
                    _world.Tiles[cx, ty].ClearTile();
                    _world.Tiles[cx, ty].Wall = brickWall;
                }
            }
        }
    }

    // ── Jungle Temple (Lihzahrd) ──────────────────────────────────────

    /// <summary>
    /// Create a simplified jungle temple: lihzahrd brick maze with corridors.
    /// </summary>
    public void JungleTemple(int x, int y, int w = 0, int h = 0)
    {
        if (w <= 0) w = _rand.Next(80, 150);
        if (h <= 0) h = _rand.Next(60, 100);

        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);

        for (int tx = x1; tx < x2; tx++)
        {
            for (int ty = y1; ty < y2; ty++)
            {
                SaveAndSet(tx, ty, TILE_LIHZAHRD_BRICK);
                _world.Tiles[tx, ty].Wall = WALL_LIHZAHRD;
            }
        }

        int cellW = _rand.Next(12, 18);
        int cellH = _rand.Next(8, 14);

        for (int cx = x1 + 3; cx < x2 - cellW; cx += cellW + 2)
        {
            for (int cy = y1 + 3; cy < y2 - cellH; cy += cellH + 2)
            {
                for (int dx = 1; dx < cellW - 1; dx++)
                {
                    for (int dy = 1; dy < cellH - 1; dy++)
                    {
                        int tx = cx + dx;
                        int ty = cy + dy;
                        if (_world.ValidTileLocation(tx, ty))
                        {
                            _undo.SaveTile(_world, tx, ty);
                            _world.Tiles[tx, ty].ClearTile();
                            _world.Tiles[tx, ty].Wall = WALL_LIHZAHRD;
                        }
                    }
                }

                int doorSide = _rand.Next(3);
                int doorPos;
                if (doorSide == 0)
                {
                    doorPos = cx + cellW / 2;
                    for (int dy = 0; dy < 3; dy++)
                    {
                        int ty = cy + cellH - 1 + dy;
                        if (_world.ValidTileLocation(doorPos, ty))
                        {
                            _undo.SaveTile(_world, doorPos, ty);
                            _world.Tiles[doorPos, ty].ClearTile();
                            _world.Tiles[doorPos, ty].Wall = WALL_LIHZAHRD;
                        }
                    }
                }
                else if (doorSide == 1)
                {
                    doorPos = cy + cellH / 2;
                    for (int dx2 = -2; dx2 < 1; dx2++)
                    {
                        if (_world.ValidTileLocation(cx + dx2, doorPos))
                        {
                            _undo.SaveTile(_world, cx + dx2, doorPos);
                            _world.Tiles[cx + dx2, doorPos].ClearTile();
                            _world.Tiles[cx + dx2, doorPos].Wall = WALL_LIHZAHRD;
                        }
                    }
                }
                else
                {
                    doorPos = cy + cellH / 2;
                    for (int dx2 = 0; dx2 < 3; dx2++)
                    {
                        if (_world.ValidTileLocation(cx + cellW - 1 + dx2, doorPos))
                        {
                            _undo.SaveTile(_world, cx + cellW - 1 + dx2, doorPos);
                            _world.Tiles[cx + cellW - 1 + dx2, doorPos].ClearTile();
                            _world.Tiles[cx + cellW - 1 + dx2, doorPos].Wall = WALL_LIHZAHRD;
                        }
                    }
                }
            }
        }
    }

    // ── Underground House ──────────────────────────────────────────────

    /// <summary>
    /// Place a simple underground house (room with walls and platforms).
    /// style: 0=wood, 1=stone, 2=dungeon.
    /// </summary>
    public void UndergroundHouse(int x, int y, int style = 0)
    {
        int roomW = _rand.Next(8, 16);
        int roomH = _rand.Next(5, 8);

        ushort wallTile = style switch
        {
            1 => TILE_STONE,
            2 => TILE_DUNGEON_BLUE,
            _ => TILE_WOOD,
        };

        ushort wallType = style switch
        {
            1 => 1,
            2 => WALL_DUNGEON_BLUE,
            _ => 4,
        };

        int x0 = x - roomW / 2;
        int y0 = y - roomH / 2;

        for (int dx = 0; dx < roomW; dx++)
        {
            for (int dy = 0; dy < roomH; dy++)
            {
                int tx = x0 + dx;
                int ty = y0 + dy;
                if (!_world.ValidTileLocation(tx, ty)) continue;

                bool isEdge = dx == 0 || dx == roomW - 1 || dy == 0 || dy == roomH - 1;

                if (isEdge)
                {
                    SaveAndSet(tx, ty, wallTile);
                    _world.Tiles[tx, ty].Wall = wallType;
                }
                else
                {
                    _undo.SaveTile(_world, tx, ty);
                    _world.Tiles[tx, ty].ClearTile();
                    _world.Tiles[tx, ty].Wall = wallType;
                }
            }
        }

        int platY = y0 + roomH - 1;
        for (int dx = 1; dx < roomW - 1; dx++)
        {
            int tx = x0 + dx;
            if (_world.ValidTileLocation(tx, platY))
            {
                SaveAndSet(tx, platY, TILE_PLATFORM);
            }
        }
    }
}
