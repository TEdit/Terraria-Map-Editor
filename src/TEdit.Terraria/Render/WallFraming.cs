using System.Collections.Generic;
using TEdit.Geometry;

namespace TEdit.Terraria.Render;

/// <summary>
/// Terraria-accurate wall framing algorithm, ported from Terraria 1.4.5.4 Framing.WallFrame().
/// Replaces the old BlendRules-based wall framing which produced incorrect tiling patterns.
/// </summary>
public static class WallFraming
{
    // Tile IDs that count as wall neighbors (TileID.Sets.TruncatesWalls)
    private static readonly HashSet<int> TruncatesWallsTileIds = [54, 328, 459, 748];

    // phlebasTileFrameNumberLookup[y%4][x%3] (values are 1-based, subtract 1 for 0-based frame number)
    private static readonly int[][] PhlebasLookup =
    [
        [2, 4, 2],
        [1, 3, 1],
        [2, 2, 4],
        [1, 1, 3]
    ];

    // lazureTileFrameNumberLookup[x%2][y%2] (values are 1-based, subtract 1 for 0-based frame number)
    private static readonly int[][] LazureLookup =
    [
        [1, 3],
        [2, 4]
    ];

    // centerWallFrameLookup[x%3][y%3] - added to index 15 when all 4 neighbors present
    private static readonly int[][] CenterWallFrameLookup =
    [
        [2, 0, 0],
        [0, 1, 4],
        [0, 3, 0]
    ];

    // wallFrameLookup[index][frameNumber] - 20 entries, each with 4 variants
    // Grid coordinates (column, row) in the wall texture atlas, pre-multiplied by 36 (wallFrameSize)
    private static readonly Vector2Int32[][] WallFrameLookup = BuildWallFrameLookup();

    private static Vector2Int32[][] BuildWallFrameLookup()
    {
        const int S = 36; // wallFrameSize
        var lookup = new Vector2Int32[20][];

        void Add(int index, int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            lookup[index] =
            [
                new Vector2Int32(x0 * S, y0 * S),
                new Vector2Int32(x1 * S, y1 * S),
                new Vector2Int32(x2 * S, y2 * S),
                new Vector2Int32(x3 * S, y3 * S)
            ];
        }

        // From Framing.AddWallFrameLookup calls (lines 102-121)
        Add(0,   9, 3,  10, 3,  11, 3,   6, 6);  // no neighbors
        Add(1,   6, 3,   7, 3,   8, 3,   4, 6);  // N
        Add(2,  12, 0,  12, 1,  12, 2,  12, 5);  // W
        Add(3,   1, 4,   3, 4,   5, 4,   3, 6);  // N+W
        Add(4,   9, 0,   9, 1,   9, 2,   9, 5);  // E
        Add(5,   0, 4,   2, 4,   4, 4,   2, 6);  // N+E
        Add(6,   6, 4,   7, 4,   8, 4,   5, 6);  // W+E
        Add(7,   1, 2,   2, 2,   3, 2,   3, 5);  // N+W+E
        Add(8,   6, 0,   7, 0,   8, 0,   6, 5);  // S
        Add(9,   5, 0,   5, 1,   5, 2,   5, 5);  // N+S
        Add(10,  1, 3,   3, 3,   5, 3,   1, 6);  // W+S
        Add(11,  4, 0,   4, 1,   4, 2,   4, 5);  // N+W+S
        Add(12,  0, 3,   2, 3,   4, 3,   0, 6);  // E+S
        Add(13,  0, 0,   0, 1,   0, 2,   0, 5);  // N+E+S
        Add(14,  1, 0,   2, 0,   3, 0,   1, 5);  // W+E+S
        Add(15,  1, 1,   2, 1,   3, 1,   2, 5);  // all (center variant 0)
        Add(16,  6, 1,   7, 1,   8, 1,   7, 5);  // all + center 1
        Add(17,  6, 2,   7, 2,   8, 2,   8, 5);  // all + center 2
        Add(18, 10, 0,  10, 1,  10, 2,  10, 5);  // all + center 3
        Add(19, 11, 0,  11, 1,  11, 2,  11, 5);  // all + center 4

        return lookup;
    }

    /// <summary>
    /// Calculate the wall frame UV coordinates for a tile position, matching Terraria's Framing.WallFrame().
    /// Returns grid coordinates (X = column index, Y = row index) for use in uvWallCache.
    /// </summary>
    public static Vector2Int32 CalculateWallFrame(World world, int x, int y, ushort wallId)
    {
        // Boundary check - match Terraria's bounds check
        if (x <= 0 || y <= 0 || x >= world.TilesWide - 1 || y >= world.TilesHigh - 1)
            return new Vector2Int32(0, 0);

        // Determine neighbor index (4-bit cardinal mask)
        // Bit order matches Terraria: N=1, W=2, E=4, S=8
        int index = 0;

        if (HasWallNeighbor(world, x, y - 1))  // North
            index = 1;
        if (HasWallNeighbor(world, x - 1, y))  // West
            index |= 2;
        if (HasWallNeighbor(world, x + 1, y))  // East
            index |= 4;
        if (HasWallNeighbor(world, x, y + 1))  // South
            index |= 8;

        // Determine frame number (variation) based on wall's LargeFrameType
        int frameNumber = DetermineFrameNumber(wallId, x, y);

        // If all 4 neighbors present, add center wall sub-pattern
        if (index == 15)
            index += CenterWallFrameLookup[x % 3][y % 3];

        // Look up the UV coordinates
        Vector2Int32 pixelCoords = WallFrameLookup[index][frameNumber];

        // Convert pixel coordinates back to grid indices (divide by wallFrameSize = 36)
        return new Vector2Int32(pixelCoords.X / 36, pixelCoords.Y / 36);
    }

    /// <summary>
    /// Check if a tile position counts as a wall neighbor.
    /// A position is a wall neighbor if it has a wall, or has an active tile that truncates walls.
    /// </summary>
    private static bool HasWallNeighbor(World world, int x, int y)
    {
        if (x < 0 || y < 0 || x >= world.TilesWide || y >= world.TilesHigh)
            return false;

        ref Tile tile = ref world.Tiles[x, y];
        return tile.Wall > 0 || (tile.IsActive && TruncatesWallsTileIds.Contains(tile.Type));
    }

    /// <summary>
    /// Determine the frame number (0-3) for a wall based on its LargeFrameType.
    /// Mode 0 (default): deterministic position-based variation
    /// Mode 1 (phlebas): 4x3 repeating pattern
    /// Mode 2 (lazure): 2x2 repeating pattern
    /// </summary>
    public static int DetermineFrameNumber(ushort wallId, int x, int y)
    {
        byte largeFrameType = GetLargeFrameType(wallId);

        if (largeFrameType == 1)
            return PhlebasLookup[y % 4][x % 3] - 1;

        if (largeFrameType == 2)
            return LazureLookup[x % 2][y % 2] - 1;

        // Default mode: deterministic pseudo-random based on position
        // Terraria uses WorldGen.genRand for initial placement, but we can't know the stored value.
        // Use a deterministic hash that produces values 0-2 (matching Terraria's Next(0, 3) range).
        return ((x * 7) + (y * 11)) % 3;
    }

    private static byte GetLargeFrameType(ushort wallId)
    {
        var walls = WorldConfiguration.WallProperties;
        if (walls != null && wallId < walls.Count)
            return walls[wallId].LargeFrameType;
        return 0;
    }

    // Expose lookup tables for testing
    internal static int[][] GetPhlebasLookup() => PhlebasLookup;
    internal static int[][] GetLazureLookup() => LazureLookup;
    internal static int[][] GetCenterWallFrameLookup() => CenterWallFrameLookup;
    internal static Vector2Int32[][] GetWallFrameLookup() => WallFrameLookup;
}
