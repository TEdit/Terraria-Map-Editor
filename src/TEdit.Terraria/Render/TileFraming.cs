using System.Collections.Generic;
using TEdit.Geometry;

namespace TEdit.Terraria.Render;

/// <summary>
/// Terraria-accurate tile framing algorithms, ported from Terraria 1.4.5.4 Framing.SelfFrame8Way().
/// Used for gemspark blocks and other tiles that use 8-way neighbor-based framing.
/// </summary>
public static class TileFraming
{
    private const int FrameSize8Way = 18; // 16px tile + 2px gap

    // Gemspark tile IDs that use SelfFrame8Way framing
    private static readonly HashSet<int> GemsparkTileIds =
        [255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 385, 446, 447, 448];

    // phlebasTileFrameNumberLookup[y%4][x%3] (values are 1-based, subtract 1 for 0-based)
    private static readonly int[][] PhlebasLookup =
    [
        [2, 4, 2],
        [1, 3, 1],
        [2, 2, 4],
        [1, 1, 3]
    ];

    // lazureTileFrameNumberLookup[x%2][y%2] (values are 1-based, subtract 1 for 0-based)
    private static readonly int[][] LazureLookup =
    [
        [1, 3],
        [2, 4]
    ];

    // selfFrame8WayLookup[index][variant] — grid coordinates (col, row), stored as pixel coords
    // 47 defined entries from Framing.cs Add8WayLookup calls; remaining 209 are null (safe default used)
    private static readonly Vector2Int32[][] SelfFrame8WayLookup = BuildSelfFrame8WayLookup();

    /// <summary>
    /// BlockStyle lookup matching Terraria's Framing.blockStyleLookup.
    /// Index maps to BrickStyle enum value (0=Full, 1=HalfBrick, 2-5=slopes).
    /// Each entry defines which faces are open for neighbor connections.
    /// </summary>
    private static readonly BlockStyle[] BlockStyleLookup =
    [
        new(Top: true,  Left: true,  Right: true,  Bottom: true),   // 0: Full block
        new(Top: false, Left: true,  Right: true,  Bottom: true),   // 1: HalfBrick
        new(Top: false, Left: true,  Right: false, Bottom: true),   // 2: SlopeTopRight
        new(Top: false, Left: false, Right: true,  Bottom: true),   // 3: SlopeTopLeft
        new(Top: true,  Left: false, Right: true,  Bottom: false),  // 4: SlopeBottomRight
        new(Top: true,  Left: false, Right: false, Bottom: true),   // 5: SlopeBottomLeft
    ];

    // Default frame for undefined lookup entries (center tile, all neighbors = index 255, variant 0)
    private static readonly Vector2Int32 DefaultFrame = new(1 * FrameSize8Way, 1 * FrameSize8Way);

    private static Vector2Int32[][] BuildSelfFrame8WayLookup()
    {
        var lookup = new Vector2Int32[256][];

        void Add3(int index, int x0, int y0, int x1, int y1, int x2, int y2)
        {
            lookup[index] =
            [
                new Vector2Int32(x0 * FrameSize8Way, y0 * FrameSize8Way),
                new Vector2Int32(x1 * FrameSize8Way, y1 * FrameSize8Way),
                new Vector2Int32(x2 * FrameSize8Way, y2 * FrameSize8Way),
            ];
        }

        void Add1(int index, int x, int y)
        {
            var pt = new Vector2Int32(x * FrameSize8Way, y * FrameSize8Way);
            lookup[index] = [pt, pt, pt];
        }

        // 47 entries from Framing.cs lines 28-74
        Add3(0,   9, 3,  10, 3,  11, 3);
        Add3(1,   6, 3,   7, 3,   8, 3);
        Add3(2,  12, 0,  12, 1,  12, 2);
        Add1(3,  15, 2);
        Add3(4,   9, 0,   9, 1,   9, 2);
        Add1(5,  13, 2);
        Add3(6,   6, 4,   7, 4,   8, 4);
        Add1(7,  14, 2);
        Add3(8,   6, 0,   7, 0,   8, 0);
        Add3(9,   5, 0,   5, 1,   5, 2);
        Add1(10, 15, 0);
        Add1(11, 15, 1);
        Add1(12, 13, 0);
        Add1(13, 13, 1);
        Add1(14, 14, 0);
        Add1(15, 14, 1);
        Add3(19,  1, 4,   3, 4,   5, 4);
        Add1(23, 16, 3);
        Add1(27, 17, 0);
        Add1(31, 13, 4);
        Add3(37,  0, 4,   2, 4,   4, 4);
        Add1(39, 17, 3);
        Add1(45, 16, 0);
        Add1(47, 12, 4);
        Add3(55,  1, 2,   2, 2,   3, 2);
        Add3(63,  6, 2,   7, 2,   8, 2);
        Add3(74,  1, 3,   3, 3,   5, 3);
        Add1(75, 17, 1);
        Add1(78, 16, 2);
        Add1(79, 13, 3);
        Add3(91,  4, 0,   4, 1,   4, 2);
        Add3(95, 11, 0,  11, 1,  11, 2);
        Add1(111, 17, 4);
        Add1(127, 14, 3);
        Add3(140,  0, 3,   2, 3,   4, 3);
        Add1(141, 16, 1);
        Add1(142, 17, 2);
        Add1(143, 12, 3);
        Add1(159, 16, 4);
        Add3(173,  0, 0,   0, 1,   0, 2);
        Add3(175, 10, 0,  10, 1,  10, 2);
        Add1(191, 15, 3);
        Add3(206,  1, 0,   2, 0,   3, 0);
        Add3(207,  6, 1,   7, 1,   8, 1);
        Add1(223, 14, 4);
        Add1(239, 15, 4);
        Add3(255,  1, 1,   2, 1,   3, 1);

        return lookup;
    }

    /// <summary>
    /// Returns true if the tile type uses SelfFrame8Way framing (gemspark blocks).
    /// </summary>
    public static bool IsGemSpark(ushort tileType) => GemsparkTileIds.Contains(tileType);

    /// <summary>
    /// Calculate the tile frame for a gemspark (SelfFrame8Way) tile.
    /// Returns grid coordinates (col, row) suitable for uvTileCache encoding.
    /// </summary>
    public static Vector2Int32 CalculateSelfFrame8Way(World world, int x, int y)
    {
        if (x < 0 || y < 0 || x >= world.TilesWide || y >= world.TilesHigh)
            return new Vector2Int32(0, 0);

        Tile centerTile = world.Tiles[x, y];
        if (centerTile == null || !centerTile.IsActive)
            return new Vector2Int32(0, 0);

        ushort centerType = centerTile.Type;
        BlockStyle centerStyle = GetBlockStyle(centerTile);
        int index = 0;

        // Check Up neighbor (bit 1)
        BlockStyle upStyle = default;
        if (centerStyle.Top)
        {
            Tile neighbor = GetTileSafely(world, x, y - 1);
            if (neighbor != null && neighbor.IsActive && WillItBlendGemspark(centerType, neighbor.Type))
            {
                upStyle = GetBlockStyle(neighbor);
                if (upStyle.Bottom)
                    index |= 1;
                else
                    upStyle = default;
            }
        }

        // Check Left neighbor (bit 2)
        BlockStyle leftStyle = default;
        if (centerStyle.Left)
        {
            Tile neighbor = GetTileSafely(world, x - 1, y);
            if (neighbor != null && neighbor.IsActive && WillItBlendGemspark(centerType, neighbor.Type))
            {
                leftStyle = GetBlockStyle(neighbor);
                if (leftStyle.Right)
                    index |= 2;
                else
                    leftStyle = default;
            }
        }

        // Check Right neighbor (bit 4)
        BlockStyle rightStyle = default;
        if (centerStyle.Right)
        {
            Tile neighbor = GetTileSafely(world, x + 1, y);
            if (neighbor != null && neighbor.IsActive && WillItBlendGemspark(centerType, neighbor.Type))
            {
                rightStyle = GetBlockStyle(neighbor);
                if (rightStyle.Left)
                    index |= 4;
                else
                    rightStyle = default;
            }
        }

        // Check Down neighbor (bit 8)
        BlockStyle downStyle = default;
        if (centerStyle.Bottom)
        {
            Tile neighbor = GetTileSafely(world, x, y + 1);
            if (neighbor != null && neighbor.IsActive && WillItBlendGemspark(centerType, neighbor.Type))
            {
                downStyle = GetBlockStyle(neighbor);
                if (downStyle.Top)
                    index |= 8;
                else
                    downStyle = default;
            }
        }

        // Corner checks — only when BOTH adjacent cardinals connected AND corner tile's faces align
        // UpLeft (bit 16): requires Up.left && Left.top
        if (upStyle.Left && leftStyle.Top)
        {
            Tile corner = GetTileSafely(world, x - 1, y - 1);
            if (corner != null && corner.IsActive && WillItBlendGemspark(centerType, corner.Type))
            {
                BlockStyle cs = GetBlockStyle(corner);
                if (cs.Right && cs.Bottom)
                    index |= 16;
            }
        }

        // UpRight (bit 32): requires Up.right && Right.top
        if (upStyle.Right && rightStyle.Top)
        {
            Tile corner = GetTileSafely(world, x + 1, y - 1);
            if (corner != null && corner.IsActive && WillItBlendGemspark(centerType, corner.Type))
            {
                BlockStyle cs = GetBlockStyle(corner);
                if (cs.Left && cs.Bottom)
                    index |= 32;
            }
        }

        // DownLeft (bit 64): requires Down.left && Left.bottom
        if (downStyle.Left && leftStyle.Bottom)
        {
            Tile corner = GetTileSafely(world, x - 1, y + 1);
            if (corner != null && corner.IsActive && WillItBlendGemspark(centerType, corner.Type))
            {
                BlockStyle cs = GetBlockStyle(corner);
                if (cs.Right && cs.Top)
                    index |= 64;
            }
        }

        // DownRight (bit 128): requires Down.right && Right.bottom
        if (downStyle.Right && rightStyle.Bottom)
        {
            Tile corner = GetTileSafely(world, x + 1, y + 1);
            if (corner != null && corner.IsActive && WillItBlendGemspark(centerType, corner.Type))
            {
                BlockStyle cs = GetBlockStyle(corner);
                if (cs.Left && cs.Top)
                    index |= 128;
            }
        }

        int frameNumber = DetermineFrameNumber(centerType, x, y);
        Vector2Int32[] entry = SelfFrame8WayLookup[index];
        if (entry == null)
        {
            // Undefined lookup entry — use default (safe fallback)
            return new Vector2Int32(DefaultFrame.X / FrameSize8Way, DefaultFrame.Y / FrameSize8Way);
        }

        Vector2Int32 pixelCoords = entry[frameNumber];
        return new Vector2Int32(pixelCoords.X / FrameSize8Way, pixelCoords.Y / FrameSize8Way);
    }

    /// <summary>
    /// Determine the frame number (variant 0-2) for a tile based on its LargeFrameType.
    /// Mode 0 (default): deterministic position-based (x*7 + y*11) % 3
    /// Mode 1 (phlebas): 4x3 repeating pattern
    /// Mode 2 (lazure): 2x2 repeating pattern
    /// </summary>
    public static int DetermineFrameNumber(ushort tileType, int x, int y)
    {
        byte largeFrameType = GetLargeFrameType(tileType);

        if (largeFrameType == 1)
            return PhlebasLookup[y % 4][x % 3] - 1;

        if (largeFrameType == 2)
            return LazureLookup[x % 2][y % 2] - 1;

        // Default: deterministic pseudo-random based on position (0-2)
        return ((x * 7) + (y * 11)) % 3;
    }

    /// <summary>
    /// For gemsparks, WillItBlend reduces to same-type check.
    /// Each gemspark's GemsparkFramingTypes maps to itself; non-gemsparks map to 0.
    /// </summary>
    private static bool WillItBlendGemspark(ushort myType, ushort otherType) => myType == otherType;

    private static BlockStyle GetBlockStyle(Tile tile)
    {
        int styleIndex = (int)tile.BrickStyle;
        if (styleIndex >= 0 && styleIndex < BlockStyleLookup.Length)
            return BlockStyleLookup[styleIndex];
        return BlockStyleLookup[0]; // Full block default
    }

    private static Tile GetTileSafely(World world, int x, int y)
    {
        if (x < 0 || y < 0 || x >= world.TilesWide || y >= world.TilesHigh)
            return null;
        return world.Tiles[x, y];
    }

    private static byte GetLargeFrameType(ushort tileType)
    {
        var tiles = WorldConfiguration.TileProperties;
        if (tiles != null && tileType < tiles.Count)
            return tiles[tileType].LargeFrameType;
        return 0;
    }

    // Expose lookup tables for testing
    internal static Vector2Int32[][] GetSelfFrame8WayLookup() => SelfFrame8WayLookup;
    internal static int[][] GetPhlebasLookup() => PhlebasLookup;
    internal static int[][] GetLazureLookup() => LazureLookup;
    internal static HashSet<int> GetGemsparkTileIds() => GemsparkTileIds;

    private readonly record struct BlockStyle(bool Top, bool Left, bool Right, bool Bottom);
}
