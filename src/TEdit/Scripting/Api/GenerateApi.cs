using System;
using System.Collections.Generic;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public partial class GenerateApi
{
    private readonly World _world;
    private readonly IUndoManager _undo;
    private readonly ISelection _selection;
    private readonly Random _rand = new();

    private record TreeTypeInfo(string Name, ushort TileId, int MinHeight, int MaxHeight);
    private record OreTypeInfo(string Name, ushort TileId, double DefaultStrength, int DefaultSteps);

    private static readonly TreeTypeInfo[] TreeTypes =
    [
        new("oak",      5,   5, 16),
        new("palm",     323, 10, 20),
        new("mushroom", 72,  5, 16),
        new("topaz",    583, 7, 12),
        new("amethyst", 584, 7, 12),
        new("sapphire", 585, 7, 12),
        new("emerald",  586, 7, 12),
        new("ruby",     587, 7, 12),
        new("diamond",  588, 7, 12),
        new("amber",    589, 7, 12),
        new("jungle",   596, 5, 16),
        new("sakura",   596, 7, 12),
        new("willow",   616, 7, 12),
        new("ash",      634, 7, 12),
    ];

    private static readonly Dictionary<string, TreeTypeInfo> TreeTypeMap;

    private static readonly OreTypeInfo[] OreTypes =
    [
        // Pre-hardmode ores
        new("copper",      7,   4.0,  10),
        new("tin",         166,  4.0,  10),
        new("iron",        6,   4.0,  12),
        new("lead",        167,  4.0,  12),
        new("silver",      9,   4.5,  12),
        new("tungsten",    168,  4.5,  12),
        new("gold",        8,   5.0,  14),
        new("platinum",    169,  5.0,  14),
        // Special pre-hardmode
        new("meteorite",   37,   6.0,  15),
        new("hellstone",   58,   6.0,  15),
        // Hardmode ores
        new("cobalt",      107,  5.0,  14),
        new("palladium",   221,  5.0,  14),
        new("mythril",     108,  6.0,  16),
        new("orichalcum",  222,  6.0,  16),
        new("adamantite",  111,  7.0,  18),
        new("titanium",    223,  7.0,  18),
        // Late-game
        new("chlorophyte",  211, 6.0,  16),
        new("luminite",     408, 8.0,  20),
    ];

    private static readonly Dictionary<string, OreTypeInfo> OreTypeMap;

    static GenerateApi()
    {
        TreeTypeMap = new Dictionary<string, TreeTypeInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in TreeTypes)
            TreeTypeMap[t.Name] = t;

        OreTypeMap = new Dictionary<string, OreTypeInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var o in OreTypes)
            OreTypeMap[o.Name] = o;
    }

    public GenerateApi(World world, IUndoManager undo, ISelection selection)
    {
        _world = world;
        _undo = undo;
        _selection = selection;
    }

    /// <summary>
    /// Returns the list of supported tree type names and their tile IDs.
    /// </summary>
    public object[] ListTreeTypes()
    {
        var result = new object[TreeTypes.Length];
        for (int i = 0; i < TreeTypes.Length; i++)
            result[i] = new { name = TreeTypes[i].Name, tileId = (int)TreeTypes[i].TileId };
        return result;
    }

    /// <summary>
    /// Generate a single tree of the named type at base coordinate (x, y).
    /// The y coordinate is the ground level — the tree grows upward from there.
    /// </summary>
    public bool Tree(string type, int x, int y)
    {
        if (!TreeTypeMap.TryGetValue(type, out var info))
            return false;

        if (!_world.ValidTileLocation(x, y))
            return false;

        if (info.Name == "palm")
            return GrowPalmTree(info, x, y);
        if (info.Name == "mushroom")
            return GrowMushroomTree(info, x, y);

        // Standard trees: oak, gem, vanity, ash
        return GrowStandardTree(info, x, y);
    }

    /// <summary>
    /// Generate multiple trees within the rectangle (x, y, w, h).
    /// Picks randomly from the types array. density is 0.0-1.0 (default 0.15).
    /// Returns count of trees placed.
    /// </summary>
    public int Forest(string[] types, int x, int y, int w, int h, double density = 0.15)
    {
        if (types == null || types.Length == 0)
            return 0;

        density = Math.Clamp(density, 0.0, 1.0);
        int count = 0;

        // Minimum 4 tiles between trunk centers (Terraria requires 3-tile gap
        // so branches at x±1 don't overlap adjacent trunks)
        const int minSpacing = 4;

        // Average spacing derived from density, clamped to minimum
        int avgSpacing = Math.Max(minSpacing, (int)(1.0 / density));

        int cx = x;
        while (cx < x + w)
        {
            int surface = FindSurface(cx, y, y + h);
            if (surface >= 0)
            {
                var type = types[_rand.Next(types.Length)];
                if (Tree(type, cx, surface))
                    count++;
            }

            // Advance by at least minSpacing, with randomness around avgSpacing
            int step = _rand.Next(minSpacing, avgSpacing + minSpacing / 2 + 1);
            cx += step;
        }

        return count;
    }

    /// <summary>
    /// Same as Forest but uses the current selection rectangle.
    /// </summary>
    public int ForestInSelection(string[] types, double density = 0.15)
    {
        if (!_selection.IsActive)
            return 0;

        var area = _selection.SelectionArea;
        return Forest(types, area.X, area.Y, area.Width, area.Height, density);
    }

    // ── Standard Tree (oak, gem, vanity, ash) ────────────────────────────

    private bool GrowStandardTree(TreeTypeInfo info, int x, int groundY)
    {
        int height = _rand.Next(info.MinHeight, info.MaxHeight + 1);

        // Verify space: trunk column must be clear above ground
        int topY = groundY - height;
        if (topY < 1) return false;

        for (int cy = topY; cy < groundY; cy++)
        {
            if (!_world.ValidTileLocation(x, cy)) return false;
        }

        // Place trunk segments from ground upward
        for (int seg = 0; seg < height; seg++)
        {
            int ty = groundY - 1 - seg;
            if (!_world.ValidTileLocation(x, ty)) continue;

            SaveAndSet(x, ty, info.TileId);

            // Randomize trunk frame
            int num3 = _rand.Next(3); // variant 0-2
            int num4;

            if (seg == 0 || seg == height - 1)
            {
                // Bottom and top segments — always plain trunk
                // (top gets overwritten by treetop frame, bottom by base frame)
                num4 = 0;
            }
            else
            {
                num4 = _rand.Next(10); // 0-9 for middle segments
            }

            SetTrunkFrame(x, ty, num3, num4);

            // Place branches for middle segments
            if (seg > 0 && seg < height - 1)
            {
                PlaceBranches(info.TileId, x, ty, num4, num3);
            }
        }

        // Place treetop
        int topTileY = groundY - height;
        if (_world.ValidTileLocation(x, topTileY))
        {
            SaveAndSet(x, topTileY, info.TileId);
            PlaceTreeTop(x, topTileY);
        }

        // Place base/root extensions at bottom of trunk
        PlaceBase(info.TileId, x, groundY);

        return true;
    }

    private void SetTrunkFrame(int x, int y, int variant, int frameType)
    {
        // Port of Terraria's trunk frame logic
        short frameX, frameY;
        int v = variant; // 0, 1, 2 → frameY = 0/22/44 or 66/88/110

        switch (frameType)
        {
            case 0: // Plain trunk
                frameX = 0;
                frameY = (short)(v * 22);
                break;
            case 1:
                frameX = 0;
                frameY = (short)(66 + v * 22);
                break;
            case 2:
                frameX = 22;
                frameY = (short)(v * 22);
                break;
            case 3:
                frameX = 44;
                frameY = (short)(66 + v * 22);
                break;
            case 4:
                frameX = 22;
                frameY = (short)(66 + v * 22);
                break;
            case 5: // Left branch
                frameX = 88;
                frameY = (short)(v * 22);
                break;
            case 6: // Right branch
                frameX = 66;
                frameY = (short)(66 + v * 22);
                break;
            case 7: // Both branches
                frameX = 110;
                frameY = (short)(66 + v * 22);
                break;
            default: // 8, 9 — plain
                frameX = 0;
                frameY = (short)(v * 22);
                break;
        }

        _world.Tiles[x, y].U = frameX;
        _world.Tiles[x, y].V = frameY;
    }

    private void PlaceBranches(ushort tileId, int x, int y, int frameType, int variant)
    {
        // Left branch: frameType 5 or 7
        if (frameType == 5 || frameType == 7)
        {
            int bx = x - 1;
            if (_world.ValidTileLocation(bx, y) && !_world.Tiles[bx, y].IsActive)
            {
                SaveAndSet(bx, y, tileId);
                if (_rand.Next(3) < 2)
                {
                    _world.Tiles[bx, y].U = 44;
                    _world.Tiles[bx, y].V = (short)(198 + _rand.Next(3) * 22);
                }
                else
                {
                    _world.Tiles[bx, y].U = 66;
                    _world.Tiles[bx, y].V = (short)(_rand.Next(3) * 22);
                }
            }
        }

        // Right branch: frameType 6 or 7
        if (frameType == 6 || frameType == 7)
        {
            int bx = x + 1;
            if (_world.ValidTileLocation(bx, y) && !_world.Tiles[bx, y].IsActive)
            {
                SaveAndSet(bx, y, tileId);
                if (_rand.Next(3) < 2)
                {
                    _world.Tiles[bx, y].U = 66;
                    _world.Tiles[bx, y].V = (short)(198 + _rand.Next(3) * 22);
                }
                else
                {
                    _world.Tiles[bx, y].U = 88;
                    _world.Tiles[bx, y].V = (short)(66 + _rand.Next(3) * 22);
                }
            }
        }
    }

    private void PlaceTreeTop(int x, int y)
    {
        // Treetop tile at the very top of the trunk — references the canopy sprite.
        // Terraria uses frameX ∈ {22, 0}, frameY ∈ {198, 220, 242}.
        // 12/13 chance: frameX=22 (normal top), 1/13: frameX=0 (bare top)
        short frameX = _rand.Next(13) != 0 ? (short)22 : (short)0;
        short frameY = (short)(198 + _rand.Next(3) * 22);

        _world.Tiles[x, y].U = frameX;
        _world.Tiles[x, y].V = frameY;
    }

    private void PlaceBase(ushort tileId, int x, int groundY)
    {
        int baseY = groundY - 1; // bottommost trunk tile

        // Check which sides have valid ground for base extensions
        bool leftGround = _world.ValidTileLocation(x - 1, groundY)
            && _world.Tiles[x - 1, groundY].IsActive;
        bool rightGround = _world.ValidTileLocation(x + 1, groundY)
            && _world.Tiles[x + 1, groundY].IsActive;

        // Determine base style based on ground neighbors (matches Terraria's num7 logic)
        int baseStyle;
        if (leftGround && rightGround)
            baseStyle = _rand.Next(3); // 0=both, 1=right-only, 2=left-only
        else if (rightGround)
            baseStyle = 1;
        else if (leftGround)
            baseStyle = 2;
        else
            baseStyle = 3; // no base extensions

        // Right base extension
        if (baseStyle == 0 || baseStyle == 1)
        {
            if (_world.ValidTileLocation(x + 1, baseY) && !_world.Tiles[x + 1, baseY].IsActive)
            {
                SaveAndSet(x + 1, baseY, tileId);
                _world.Tiles[x + 1, baseY].U = 22;
                _world.Tiles[x + 1, baseY].V = (short)(132 + _rand.Next(3) * 22);
            }
        }

        // Left base extension
        if (baseStyle == 0 || baseStyle == 2)
        {
            if (_world.ValidTileLocation(x - 1, baseY) && !_world.Tiles[x - 1, baseY].IsActive)
            {
                SaveAndSet(x - 1, baseY, tileId);
                _world.Tiles[x - 1, baseY].U = 44;
                _world.Tiles[x - 1, baseY].V = (short)(132 + _rand.Next(3) * 22);
            }
        }

        // Overwrite center bottom trunk tile with base frame
        if (baseStyle != 3 && _world.ValidTileLocation(x, baseY))
        {
            short frameX = baseStyle switch
            {
                0 => 88,  // both sides
                1 => 0,   // right only
                2 => 66,  // left only
                _ => 0,
            };
            _world.Tiles[x, baseY].U = frameX;
            _world.Tiles[x, baseY].V = (short)(132 + _rand.Next(3) * 22);
        }
    }

    // ── Palm Tree (tile 323) ─────────────────────────────────────────────

    private bool GrowPalmTree(TreeTypeInfo info, int x, int groundY)
    {
        int height = _rand.Next(info.MinHeight, info.MaxHeight + 1);

        int topY = groundY - height;
        if (topY < 1) return false;

        for (int cy = topY; cy < groundY; cy++)
        {
            if (!_world.ValidTileLocation(x, cy)) return false;
        }

        // Palm trees lean with an initial offset
        int lean = _rand.Next(-8, 9); // -8 to +8
        short frameYAccum = 0;

        for (int seg = 0; seg < height; seg++)
        {
            int ty = groundY - 1 - seg;
            if (!_world.ValidTileLocation(x, ty)) continue;

            SaveAndSet(x, ty, info.TileId);

            short frameX;
            if (seg == 0)
            {
                // Base segment
                frameX = 66;
            }
            else if (seg == height - 1)
            {
                // Top segment
                frameX = (short)(22 * _rand.Next(4, 8));
            }
            else
            {
                // Middle segments — random trunk variant
                frameX = (short)(22 * _rand.Next(3));
            }

            _world.Tiles[x, ty].U = frameX;
            _world.Tiles[x, ty].V = frameYAccum;

            frameYAccum += (short)(lean > 0 ? 2 : lean < 0 ? -2 : 0);
        }

        return true;
    }

    // ── Mushroom Tree (tile 72) ──────────────────────────────────────────

    private bool GrowMushroomTree(TreeTypeInfo info, int x, int groundY)
    {
        int height = _rand.Next(info.MinHeight, info.MaxHeight + 1);

        int topY = groundY - height;
        if (topY < 1) return false;

        for (int cy = topY; cy < groundY; cy++)
        {
            if (!_world.ValidTileLocation(x, cy)) return false;
        }

        // Place trunk
        for (int seg = 0; seg < height; seg++)
        {
            int ty = groundY - 1 - seg;
            if (!_world.ValidTileLocation(x, ty)) continue;

            SaveAndSet(x, ty, info.TileId);
            _world.Tiles[x, ty].U = 0;
            _world.Tiles[x, ty].V = (short)(_rand.Next(3) * 18);
        }

        // Place mushroom cap at top
        int capY = groundY - height;
        if (_world.ValidTileLocation(x, capY))
        {
            SaveAndSet(x, capY, info.TileId);
            _world.Tiles[x, capY].U = 36;
            _world.Tiles[x, capY].V = (short)(_rand.Next(3) * 18);
        }

        return true;
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private void SaveAndSet(int x, int y, ushort tileId)
    {
        _undo.SaveTile(_world, x, y);
        _world.Tiles[x, y].IsActive = true;
        _world.Tiles[x, y].Type = tileId;
    }

    private void SaveAndSetWall(int x, int y, ushort wallId)
    {
        _undo.SaveTile(_world, x, y);
        _world.Tiles[x, y].Wall = wallId;
    }

    private void SaveAndConvert(int x, int y, ushort fromType, ushort toType)
    {
        if (_world.Tiles[x, y].IsActive && _world.Tiles[x, y].Type == fromType)
        {
            _undo.SaveTile(_world, x, y);
            _world.Tiles[x, y].Type = toType;
        }
    }

    private void SaveAndConvertWall(int x, int y, ushort fromWall, ushort toWall)
    {
        if (_world.Tiles[x, y].Wall == fromWall)
        {
            _undo.SaveTile(_world, x, y);
            _world.Tiles[x, y].Wall = toWall;
        }
    }

    /// <summary>
    /// Apply a set of tile conversion rules within a rectangular region.
    /// Each entry maps a source tile type to a destination tile type.
    /// </summary>
    private int ConvertTilesInRegion(int x, int y, int w, int h, (ushort from, ushort to)[] conversions)
    {
        int count = 0;
        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);

        for (int tx = x1; tx < x2; tx++)
        {
            for (int ty = y1; ty < y2; ty++)
            {
                if (!_world.Tiles[tx, ty].IsActive) continue;

                ushort cur = _world.Tiles[tx, ty].Type;
                foreach (var (from, to) in conversions)
                {
                    if (cur == from)
                    {
                        _undo.SaveTile(_world, tx, ty);
                        _world.Tiles[tx, ty].Type = to;
                        count++;
                        break;
                    }
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Apply a set of wall conversion rules within a rectangular region.
    /// </summary>
    private int ConvertWallsInRegion(int x, int y, int w, int h, (ushort from, ushort to)[] conversions)
    {
        int count = 0;
        int x1 = Math.Max(1, x);
        int y1 = Math.Max(1, y);
        int x2 = Math.Min(_world.TilesWide - 1, x + w);
        int y2 = Math.Min(_world.TilesHigh - 1, y + h);

        for (int tx = x1; tx < x2; tx++)
        {
            for (int ty = y1; ty < y2; ty++)
            {
                ushort cur = _world.Tiles[tx, ty].Wall;
                if (cur == 0) continue;

                foreach (var (from, to) in conversions)
                {
                    if (cur == from)
                    {
                        _undo.SaveTile(_world, tx, ty);
                        _world.Tiles[tx, ty].Wall = to;
                        count++;
                        break;
                    }
                }
            }
        }

        return count;
    }
}
