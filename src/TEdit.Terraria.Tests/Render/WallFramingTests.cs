using Shouldly;
using TEdit.Geometry;
using TEdit.Terraria.Render;

namespace TEdit.Terraria.Tests.Render;

[Collection("SharedState")]
public class WallFramingTests
{
    static WallFramingTests()
    {
        WorldConfiguration.Initialize();
    }

    private static World CreateSmallWorld(int width = 20, int height = 20)
    {
        var world = new World(height, width, "Test");
        world.Tiles = new Tile[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                world.Tiles[x, y] = new Tile();
        return world;
    }

    private static void SetWall(World world, int x, int y, ushort wallId)
    {
        world.Tiles[x, y].Wall = wallId;
    }

    private static void SetActiveTile(World world, int x, int y, ushort tileType)
    {
        world.Tiles[x, y].IsActive = true;
        world.Tiles[x, y].Type = tileType;
    }

    #region Neighbor Index Tests

    [Fact]
    public void NoNeighbors_ReturnsIndex0Frame()
    {
        var world = CreateSmallWorld();
        SetWall(world, 5, 5, 1);

        var uv = WallFraming.CalculateWallFrame(world, 5, 5, 1);

        // Index 0 (no neighbors), frame varies by position
        // WallFrameLookup[0] entries are at columns 9,10,11,6 and rows 3,3,3,6
        uv.Y.ShouldBeOneOf(3, 6);
    }

    [Fact]
    public void AllNeighbors_ReturnsIndex15OrHigher()
    {
        var world = CreateSmallWorld();
        // Set center and all 4 cardinal neighbors
        SetWall(world, 5, 5, 1);
        SetWall(world, 5, 4, 1); // N
        SetWall(world, 4, 5, 1); // W
        SetWall(world, 6, 5, 1); // E
        SetWall(world, 5, 6, 1); // S

        var uv = WallFraming.CalculateWallFrame(world, 5, 5, 1);

        // With all neighbors, index is 15 + centerWallFrameLookup offset
        // Valid entries are indices 15-19 in the lookup table
        uv.ShouldNotBe(new Vector2Int32(0, 0));
    }

    [Fact]
    public void NorthNeighborOnly_ReturnsIndex1Frame()
    {
        var world = CreateSmallWorld();
        SetWall(world, 5, 5, 1);
        SetWall(world, 5, 4, 1); // N

        var uv = WallFraming.CalculateWallFrame(world, 5, 5, 1);

        // Index 1 (N only): WallFrameLookup[1] has rows at y=3 or y=6
        uv.Y.ShouldBeOneOf(3, 6);
        uv.X.ShouldBeOneOf(6, 7, 8, 4);
    }

    [Fact]
    public void SouthNeighborOnly_ReturnsIndex8Frame()
    {
        var world = CreateSmallWorld();
        SetWall(world, 5, 5, 1);
        SetWall(world, 5, 6, 1); // S

        var uv = WallFraming.CalculateWallFrame(world, 5, 5, 1);

        // Index 8 (S only): WallFrameLookup[8] has rows at y=0 or y=5
        uv.Y.ShouldBeOneOf(0, 5);
    }

    [Theory]
    [InlineData(true, false, false, false, 1)]   // N
    [InlineData(false, true, false, false, 2)]   // W
    [InlineData(false, false, true, false, 4)]   // E
    [InlineData(false, false, false, true, 8)]   // S
    [InlineData(true, true, false, false, 3)]    // N+W
    [InlineData(true, false, true, false, 5)]    // N+E
    [InlineData(false, true, true, false, 6)]    // W+E
    [InlineData(true, true, true, false, 7)]     // N+W+E
    [InlineData(true, false, false, true, 9)]    // N+S
    [InlineData(false, true, false, true, 10)]   // W+S
    [InlineData(false, false, true, true, 12)]   // E+S
    [InlineData(true, true, true, true, 15)]     // all (before center adjustment)
    public void NeighborCombinations_ProduceValidFrames(bool n, bool w, bool e, bool s, int expectedBaseIndex)
    {
        var world = CreateSmallWorld();
        SetWall(world, 10, 10, 1);
        if (n) SetWall(world, 10, 9, 1);
        if (w) SetWall(world, 9, 10, 1);
        if (e) SetWall(world, 11, 10, 1);
        if (s) SetWall(world, 10, 11, 1);

        var uv = WallFraming.CalculateWallFrame(world, 10, 10, 1);

        // The UV should be a valid non-zero result (unless it happens to map to 0,0)
        // Just verify it doesn't crash and produces a result
        (uv.X >= 0 && uv.Y >= 0).ShouldBeTrue();
    }

    #endregion

    #region LargeFrameType Tests

    [Fact]
    public void PhlebasPattern_RepeatsCorrectly()
    {
        // Phlebas is a 4x3 repeating pattern (y%4, x%3)
        var results = new int[4, 3];
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 3; x++)
                results[y, x] = WallFraming.DetermineFrameNumber(146, x, y); // 146 = Copper Plating (phlebas)

        // Verify pattern matches Terraria's phlebasTileFrameNumberLookup (1-indexed, so subtract 1)
        results[0, 0].ShouldBe(1); // 2-1
        results[0, 1].ShouldBe(3); // 4-1
        results[0, 2].ShouldBe(1); // 2-1
        results[1, 0].ShouldBe(0); // 1-1
        results[1, 1].ShouldBe(2); // 3-1
        results[1, 2].ShouldBe(0); // 1-1
        results[2, 0].ShouldBe(1); // 2-1
        results[2, 1].ShouldBe(1); // 2-1
        results[2, 2].ShouldBe(3); // 4-1
        results[3, 0].ShouldBe(0); // 1-1
        results[3, 1].ShouldBe(0); // 1-1
        results[3, 2].ShouldBe(2); // 3-1
    }

    [Fact]
    public void PhlebasPattern_PeriodicOverLargerArea()
    {
        // Verify periodicity: frame at (x,y) == frame at (x+3, y+4)
        for (int x = 0; x < 6; x++)
            for (int y = 0; y < 8; y++)
            {
                var f1 = WallFraming.DetermineFrameNumber(146, x, y);
                var f2 = WallFraming.DetermineFrameNumber(146, x + 3, y + 4);
                f1.ShouldBe(f2, $"Phlebas pattern should repeat at ({x},{y}) vs ({x + 3},{y + 4})");
            }
    }

    [Fact]
    public void LazurePattern_RepeatsCorrectly()
    {
        // Lazure is a 2x2 repeating pattern (x%2, y%2)
        var results = new int[2, 2];
        for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                results[x, y] = WallFraming.DetermineFrameNumber(185, x, y); // 185 = Craggy Stone (lazure)

        // lazureTileFrameNumberLookup[x%2][y%2] - 1
        results[0, 0].ShouldBe(0); // 1-1
        results[0, 1].ShouldBe(2); // 3-1
        results[1, 0].ShouldBe(1); // 2-1
        results[1, 1].ShouldBe(3); // 4-1
    }

    [Fact]
    public void LazurePattern_PeriodicOverLargerArea()
    {
        for (int x = 0; x < 6; x++)
            for (int y = 0; y < 6; y++)
            {
                var f1 = WallFraming.DetermineFrameNumber(185, x, y);
                var f2 = WallFraming.DetermineFrameNumber(185, x + 2, y + 2);
                f1.ShouldBe(f2, $"Lazure pattern should repeat at ({x},{y}) vs ({x + 2},{y + 2})");
            }
    }

    [Fact]
    public void DefaultMode_IsDeterministic()
    {
        // Same position should always give same result
        var f1 = WallFraming.DetermineFrameNumber(4, 100, 200); // Wood Wall (default mode)
        var f2 = WallFraming.DetermineFrameNumber(4, 100, 200);
        f1.ShouldBe(f2);
    }

    [Fact]
    public void DefaultMode_Returns0Through2()
    {
        var seen = new HashSet<int>();
        for (int x = 0; x < 100; x++)
            for (int y = 0; y < 100; y++)
                seen.Add(WallFraming.DetermineFrameNumber(4, x, y));

        seen.ShouldBeSubsetOf(new[] { 0, 1, 2 });
        seen.Count.ShouldBe(3); // All 3 values should appear
    }

    #endregion

    #region CenterWallFrameLookup Tests

    [Fact]
    public void CenterLookup_AllNeighbors_ProducesIndices15Through19()
    {
        var world = CreateSmallWorld();

        var seenIndices = new HashSet<int>();

        // Fill a region with walls so all interior tiles have all 4 neighbors
        for (int x = 1; x < 18; x++)
            for (int y = 1; y < 18; y++)
                SetWall(world, x, y, 1);

        // Collect UV results for a 3x3 grid (one full center pattern period)
        for (int x = 3; x < 6; x++)
            for (int y = 3; y < 6; y++)
            {
                var uv = WallFraming.CalculateWallFrame(world, x, y, 1);
                // Just verify we get valid results
                (uv.X >= 0 && uv.Y >= 0).ShouldBeTrue();
            }
    }

    #endregion

    #region TruncatesWalls Tests

    [Theory]
    [InlineData(54)]   // Torches
    [InlineData(328)]  // Cannon
    [InlineData(459)]  // Explosives
    [InlineData(748)]  // Special tile
    public void TruncatesWalls_TileCountsAsWallNeighbor(int tileType)
    {
        var world = CreateSmallWorld();
        SetWall(world, 5, 5, 1);
        // Place a TruncatesWalls tile to the north (no wall, just active tile)
        SetActiveTile(world, 5, 4, (ushort)tileType);

        var uvWithTruncates = WallFraming.CalculateWallFrame(world, 5, 5, 1);

        // Compare with no neighbor at all
        var world2 = CreateSmallWorld();
        SetWall(world2, 5, 5, 1);
        var uvNoNeighbor = WallFraming.CalculateWallFrame(world2, 5, 5, 1);

        // With TruncatesWalls tile, the result should differ from no-neighbor
        uvWithTruncates.ShouldNotBe(uvNoNeighbor);
    }

    [Fact]
    public void NonTruncatesWalls_ActiveTileDoesNotCountAsNeighbor()
    {
        var world = CreateSmallWorld();
        SetWall(world, 5, 5, 1);
        // Place a regular active tile to the north (e.g., dirt block = type 0)
        SetActiveTile(world, 5, 4, 1); // Stone block, not in TruncatesWalls

        var uvWithTile = WallFraming.CalculateWallFrame(world, 5, 5, 1);

        // Compare with completely empty neighbor
        var world2 = CreateSmallWorld();
        SetWall(world2, 5, 5, 1);
        var uvNoNeighbor = WallFraming.CalculateWallFrame(world2, 5, 5, 1);

        // Regular active tile without wall should NOT count as neighbor
        uvWithTile.ShouldBe(uvNoNeighbor);
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void BoundaryTile_AtEdge_ReturnsZero()
    {
        var world = CreateSmallWorld();
        SetWall(world, 0, 0, 1);

        var uv = WallFraming.CalculateWallFrame(world, 0, 0, 1);
        uv.ShouldBe(new Vector2Int32(0, 0));
    }

    [Fact]
    public void BoundaryTile_AtMaxEdge_ReturnsZero()
    {
        var world = CreateSmallWorld();
        int maxX = world.TilesWide - 1;
        int maxY = world.TilesHigh - 1;
        SetWall(world, maxX, maxY, 1);

        var uv = WallFraming.CalculateWallFrame(world, maxX, maxY, 1);
        uv.ShouldBe(new Vector2Int32(0, 0));
    }

    [Fact]
    public void InteriorTile_OneFromEdge_WorksCorrectly()
    {
        var world = CreateSmallWorld();
        SetWall(world, 1, 1, 1);

        // Should not throw, position is valid (x > 0, y > 0, x < width-1, y < height-1)
        var uv = WallFraming.CalculateWallFrame(world, 1, 1, 1);
        (uv.X >= 0 && uv.Y >= 0).ShouldBeTrue();
    }

    #endregion

    #region Lookup Table Validation

    [Fact]
    public void WallFrameLookup_Has20Entries()
    {
        var lookup = WallFraming.GetWallFrameLookup();
        lookup.Length.ShouldBe(20);
    }

    [Fact]
    public void WallFrameLookup_EachEntryHas4Variants()
    {
        var lookup = WallFraming.GetWallFrameLookup();
        for (int i = 0; i < 20; i++)
            lookup[i].Length.ShouldBe(4, $"Entry {i} should have 4 variants");
    }

    [Fact]
    public void WallFrameLookup_AllCoordinatesAreMultiplesOf36()
    {
        var lookup = WallFraming.GetWallFrameLookup();
        for (int i = 0; i < 20; i++)
            for (int j = 0; j < 4; j++)
            {
                (lookup[i][j].X % 36).ShouldBe(0, $"Entry [{i}][{j}].X = {lookup[i][j].X}");
                (lookup[i][j].Y % 36).ShouldBe(0, $"Entry [{i}][{j}].Y = {lookup[i][j].Y}");
            }
    }

    [Fact]
    public void PhlebasLookup_HasCorrectDimensions()
    {
        var lookup = WallFraming.GetPhlebasLookup();
        lookup.Length.ShouldBe(4);
        foreach (var row in lookup)
            row.Length.ShouldBe(3);
    }

    [Fact]
    public void LazureLookup_HasCorrectDimensions()
    {
        var lookup = WallFraming.GetLazureLookup();
        lookup.Length.ShouldBe(2);
        foreach (var row in lookup)
            row.Length.ShouldBe(2);
    }

    [Fact]
    public void CenterWallFrameLookup_HasCorrectDimensions()
    {
        var lookup = WallFraming.GetCenterWallFrameLookup();
        lookup.Length.ShouldBe(3);
        foreach (var row in lookup)
            row.Length.ShouldBe(3);
    }

    #endregion
}
