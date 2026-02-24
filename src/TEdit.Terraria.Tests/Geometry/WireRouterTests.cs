using Shouldly;
using TEdit.Geometry;
using Xunit;

namespace TEdit.Terraria.Tests.Geometry;

public class WireRouterTests
{
    /// <summary>
    /// Verify all consecutive tiles in a path share an edge (4-connected).
    /// Corner-to-corner (diagonal) does NOT count as connected.
    /// </summary>
    private static void AssertFourConnected(List<Vector2Int32> path)
    {
        for (int i = 1; i < path.Count; i++)
        {
            int dx = Math.Abs(path[i].X - path[i - 1].X);
            int dy = Math.Abs(path[i].Y - path[i - 1].Y);
            // Must differ by exactly 1 on one axis and 0 on the other
            (dx + dy).ShouldBe(1, $"Tiles {i - 1}\u2192{i}: ({path[i - 1].X},{path[i - 1].Y})\u2192({path[i].X},{path[i].Y}) are not 4-connected (dx={dx}, dy={dy})");
        }
    }

    /// <summary>Verify no duplicate consecutive tiles.</summary>
    private static void AssertNoDuplicates(List<Vector2Int32> path)
    {
        for (int i = 1; i < path.Count; i++)
        {
            bool same = path[i].X == path[i - 1].X && path[i].Y == path[i - 1].Y;
            same.ShouldBeFalse($"Duplicate at index {i}: ({path[i].X},{path[i].Y})");
        }
    }

    /// <summary>Verify no globally duplicate tiles (not just consecutive).</summary>
    private static void AssertNoGlobalDuplicates(List<Vector2Int32> path)
    {
        var seen = new HashSet<(int, int)>();
        for (int i = 0; i < path.Count; i++)
        {
            var key = (path[i].X, path[i].Y);
            seen.Add(key).ShouldBeTrue($"Global duplicate at index {i}: ({path[i].X},{path[i].Y})");
        }
    }

    /// <summary>Verify path tile count equals |dx| + |dy| + 1 (Manhattan distance + 1).</summary>
    private static void AssertManhattanTileCount(List<Vector2Int32> path, Vector2Int32 start, Vector2Int32 end)
    {
        int expected = Math.Abs(end.X - start.X) + Math.Abs(end.Y - start.Y) + 1;
        path.Count.ShouldBe(expected, $"Expected {expected} tiles for ({start.X},{start.Y})->({end.X},{end.Y}), got {path.Count}");
    }

    /// <summary>Standard assertions for any valid wire path.</summary>
    private static void AssertValidPath(List<Vector2Int32> path, Vector2Int32 start, Vector2Int32 end)
    {
        path.Count.ShouldBeGreaterThan(0);
        path[0].ShouldBe(start);
        path[^1].ShouldBe(end);
        AssertFourConnected(path);
        AssertNoDuplicates(path);
        AssertNoGlobalDuplicates(path);
        AssertManhattanTileCount(path, start, end);
    }

    #region Route90 Tests

    [Fact]
    public void Route90_SamePoint_ReturnsSingleTile()
    {
        var start = new Vector2Int32(5, 5);
        var result = WireRouter.Route90(start, start, false);
        result.Count.ShouldBe(1);
        result[0].ShouldBe(start);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Route90_HorizontalLine_ProducesCorrectPath(bool verticalFirst)
    {
        var start = new Vector2Int32(0, 5);
        var end = new Vector2Int32(10, 5);
        var result = WireRouter.Route90(start, end, verticalFirst);
        AssertValidPath(result, start, end);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Route90_VerticalLine_ProducesCorrectPath(bool verticalFirst)
    {
        var start = new Vector2Int32(5, 0);
        var end = new Vector2Int32(5, 10);
        var result = WireRouter.Route90(start, end, verticalFirst);
        AssertValidPath(result, start, end);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Route90_LShape_IsFourConnected(bool verticalFirst)
    {
        var start = new Vector2Int32(0, 0);
        var end = new Vector2Int32(5, 8);
        var result = WireRouter.Route90(start, end, verticalFirst);
        AssertValidPath(result, start, end);
    }

    [Fact]
    public void Route90_NegativeDirection_Works()
    {
        var start = new Vector2Int32(10, 10);
        var end = new Vector2Int32(3, 2);
        var result = WireRouter.Route90(start, end, false);
        AssertValidPath(result, start, end);
    }

    [Theory]
    [InlineData(0, 0, 10, 5)]   // Quadrant I: right-down
    [InlineData(10, 0, 0, 5)]   // Quadrant II: left-down
    [InlineData(10, 10, 0, 5)]  // Quadrant III: left-up
    [InlineData(0, 10, 10, 5)]  // Quadrant IV: right-up
    public void Route90_AllQuadrants_HorizontalFirst(int sx, int sy, int ex, int ey)
    {
        var start = new Vector2Int32(sx, sy);
        var end = new Vector2Int32(ex, ey);
        var result = WireRouter.Route90(start, end, false);
        AssertValidPath(result, start, end);
    }

    [Theory]
    [InlineData(0, 0, 10, 5)]
    [InlineData(10, 0, 0, 5)]
    [InlineData(10, 10, 0, 5)]
    [InlineData(0, 10, 10, 5)]
    public void Route90_AllQuadrants_VerticalFirst(int sx, int sy, int ex, int ey)
    {
        var start = new Vector2Int32(sx, sy);
        var end = new Vector2Int32(ex, ey);
        var result = WireRouter.Route90(start, end, true);
        AssertValidPath(result, start, end);
    }

    [Fact]
    public void Route90_HorizontalFirst_HasCorrectCorner()
    {
        var start = new Vector2Int32(0, 0);
        var end = new Vector2Int32(5, 3);
        var result = WireRouter.Route90(start, end, false);

        // Horizontal first: corner should be at (5, 0) — end of horizontal leg
        result.ShouldContain(new Vector2Int32(5, 0));
        // First few tiles should all be on Y=0 (horizontal leg)
        result[0].Y.ShouldBe(0);
        result[1].Y.ShouldBe(0);
    }

    [Fact]
    public void Route90_VerticalFirst_HasCorrectCorner()
    {
        var start = new Vector2Int32(0, 0);
        var end = new Vector2Int32(5, 3);
        var result = WireRouter.Route90(start, end, true);

        // Vertical first: corner should be at (0, 3) — end of vertical leg
        result.ShouldContain(new Vector2Int32(0, 3));
        // First few tiles should all be on X=0 (vertical leg)
        result[0].X.ShouldBe(0);
        result[1].X.ShouldBe(0);
    }

    [Fact]
    public void Route90_AdjacentTile_Horizontal()
    {
        var start = new Vector2Int32(5, 5);
        var end = new Vector2Int32(6, 5);
        var result = WireRouter.Route90(start, end, false);
        result.Count.ShouldBe(2);
        AssertValidPath(result, start, end);
    }

    [Fact]
    public void Route90_AdjacentTile_Diagonal()
    {
        var start = new Vector2Int32(5, 5);
        var end = new Vector2Int32(6, 6);
        var result = WireRouter.Route90(start, end, false);
        // L-shape with 3 tiles: (5,5) → (6,5) → (6,6)
        result.Count.ShouldBe(3);
        AssertValidPath(result, start, end);
    }

    [Fact]
    public void Route90_LargePath_Performance()
    {
        var start = new Vector2Int32(0, 0);
        var end = new Vector2Int32(1000, 500);
        var result = WireRouter.Route90(start, end, false);
        AssertValidPath(result, start, end);
        result.Count.ShouldBe(1501); // 1000 + 500 + 1
    }

    #endregion

    #region RouteMiter Tests

    [Fact]
    public void RouteMiter_SamePoint_ReturnsSingleTile()
    {
        var start = new Vector2Int32(5, 5);
        var result = WireRouter.RouteMiter(start, start, false);
        result.Count.ShouldBe(1);
        result[0].ShouldBe(start);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RouteMiter_StraightLine_ProducesCorrectPath(bool verticalFirst)
    {
        var start = new Vector2Int32(0, 5);
        var end = new Vector2Int32(10, 5);
        var result = WireRouter.RouteMiter(start, end, verticalFirst);
        AssertValidPath(result, start, end);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RouteMiter_VerticalStraightLine(bool verticalFirst)
    {
        var start = new Vector2Int32(5, 0);
        var end = new Vector2Int32(5, 10);
        var result = WireRouter.RouteMiter(start, end, verticalFirst);
        AssertValidPath(result, start, end);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RouteMiter_Diagonal_IsFourConnected(bool verticalFirst)
    {
        var start = new Vector2Int32(0, 0);
        var end = new Vector2Int32(5, 5);
        var result = WireRouter.RouteMiter(start, end, verticalFirst);
        AssertValidPath(result, start, end);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RouteMiter_WithStraightAndDiagonal_IsFourConnected(bool verticalFirst)
    {
        var start = new Vector2Int32(0, 0);
        var end = new Vector2Int32(8, 3);
        var result = WireRouter.RouteMiter(start, end, verticalFirst);
        AssertValidPath(result, start, end);
    }

    [Fact]
    public void RouteMiter_NegativeDirection_Works()
    {
        var start = new Vector2Int32(10, 10);
        var end = new Vector2Int32(3, 2);
        var result = WireRouter.RouteMiter(start, end, false);
        AssertValidPath(result, start, end);
    }

    [Theory]
    [InlineData(0, 0, 10, 5)]   // right-down, wider
    [InlineData(0, 0, 5, 10)]   // right-down, taller
    [InlineData(10, 0, 0, 5)]   // left-down
    [InlineData(10, 10, 0, 5)]  // left-up
    [InlineData(0, 10, 10, 5)]  // right-up
    [InlineData(0, 0, 3, 7)]    // steep
    [InlineData(0, 0, 7, 3)]    // shallow
    public void RouteMiter_AllQuadrants_BothModes(int sx, int sy, int ex, int ey)
    {
        var start = new Vector2Int32(sx, sy);
        var end = new Vector2Int32(ex, ey);

        var resultH = WireRouter.RouteMiter(start, end, false);
        AssertValidPath(resultH, start, end);

        var resultV = WireRouter.RouteMiter(start, end, true);
        AssertValidPath(resultV, start, end);
    }

    [Fact]
    public void RouteMiter_PureDiagonal_StaircasePattern()
    {
        var start = new Vector2Int32(0, 0);
        var end = new Vector2Int32(3, 3);
        var result = WireRouter.RouteMiter(start, end, false);

        AssertValidPath(result, start, end);
        // Pure diagonal: |dx|=|dy|=3, so all tiles are staircase
        // Staircase alternates H and V steps: 3+3+1 = 7 tiles
        result.Count.ShouldBe(7);
    }

    [Fact]
    public void RouteMiter_AdjacentDiagonal_IsFourConnected()
    {
        var start = new Vector2Int32(5, 5);
        var end = new Vector2Int32(6, 6);
        var result = WireRouter.RouteMiter(start, end, false);
        AssertValidPath(result, start, end);
        result.Count.ShouldBe(3); // (5,5) → (6,5) → (6,6)
    }

    [Fact]
    public void RouteMiter_LargePath_Performance()
    {
        var start = new Vector2Int32(0, 0);
        var end = new Vector2Int32(1000, 500);
        var result = WireRouter.RouteMiter(start, end, false);
        AssertValidPath(result, start, end);
        result.Count.ShouldBe(1501);
    }

    [Fact]
    public void RouteMiter_StaircaseNeverDiagonallyAdjacent()
    {
        // Verify that in the staircase portion, tiles never connect diagonally
        var start = new Vector2Int32(0, 0);
        var end = new Vector2Int32(5, 5);
        var result = WireRouter.RouteMiter(start, end, false);

        // Check that no two tiles that are 2 apart in the path share only a corner
        for (int i = 2; i < result.Count; i++)
        {
            int dx = Math.Abs(result[i].X - result[i - 2].X);
            int dy = Math.Abs(result[i].Y - result[i - 2].Y);
            // If two steps away, they should NOT be diagonal (1,1)
            // They can be (2,0), (0,2), or (1,1) — but (1,1) would mean the
            // intermediate tile was a diagonal step, which our 4-connected check already catches.
            // This is an additional sanity check.
            if (dx == 1 && dy == 1)
            {
                // This is OK — the intermediate tile ensures 4-connectivity
                // Just verify the intermediate tile bridges them
                int midDx1 = Math.Abs(result[i - 1].X - result[i - 2].X);
                int midDy1 = Math.Abs(result[i - 1].Y - result[i - 2].Y);
                int midDx2 = Math.Abs(result[i].X - result[i - 1].X);
                int midDy2 = Math.Abs(result[i].Y - result[i - 1].Y);
                (midDx1 + midDy1).ShouldBe(1);
                (midDx2 + midDy2).ShouldBe(1);
            }
        }
    }

    #endregion

    #region DetectVerticalFirst Tests

    [Fact]
    public void DetectVerticalFirst_HorizontalBias()
    {
        WireRouter.DetectVerticalFirst(new Vector2Int32(0, 0), new Vector2Int32(10, 3)).ShouldBeFalse();
    }

    [Fact]
    public void DetectVerticalFirst_VerticalBias()
    {
        WireRouter.DetectVerticalFirst(new Vector2Int32(0, 0), new Vector2Int32(3, 10)).ShouldBeTrue();
    }

    [Fact]
    public void DetectVerticalFirst_Equal_ReturnsHorizontal()
    {
        // |dy| == |dx| → not strictly greater, so false (horizontal first)
        WireRouter.DetectVerticalFirst(new Vector2Int32(0, 0), new Vector2Int32(5, 5)).ShouldBeFalse();
    }

    [Fact]
    public void DetectVerticalFirst_SamePoint_ReturnsFalse()
    {
        WireRouter.DetectVerticalFirst(new Vector2Int32(5, 5), new Vector2Int32(5, 5)).ShouldBeFalse();
    }

    [Fact]
    public void DetectVerticalFirst_NegativeCoordinates()
    {
        WireRouter.DetectVerticalFirst(new Vector2Int32(10, 10), new Vector2Int32(3, 0)).ShouldBeTrue();
        WireRouter.DetectVerticalFirst(new Vector2Int32(10, 10), new Vector2Int32(0, 8)).ShouldBeFalse();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Route90_OnePixelDifference_X()
    {
        var start = new Vector2Int32(5, 5);
        var end = new Vector2Int32(6, 5);
        var result = WireRouter.Route90(start, end, false);
        AssertValidPath(result, start, end);
    }

    [Fact]
    public void Route90_OnePixelDifference_Y()
    {
        var start = new Vector2Int32(5, 5);
        var end = new Vector2Int32(5, 6);
        var result = WireRouter.Route90(start, end, true);
        AssertValidPath(result, start, end);
    }

    [Fact]
    public void RouteMiter_OnePixelDifference_X()
    {
        var start = new Vector2Int32(5, 5);
        var end = new Vector2Int32(6, 5);
        var result = WireRouter.RouteMiter(start, end, false);
        AssertValidPath(result, start, end);
    }

    [Fact]
    public void RouteMiter_OnePixelDifference_Y()
    {
        var start = new Vector2Int32(5, 5);
        var end = new Vector2Int32(5, 6);
        var result = WireRouter.RouteMiter(start, end, true);
        AssertValidPath(result, start, end);
    }

    [Theory]
    [InlineData(0, 0, 100, 1)]   // nearly horizontal
    [InlineData(0, 0, 1, 100)]   // nearly vertical
    [InlineData(0, 0, 50, 50)]   // exact diagonal
    [InlineData(0, 0, 100, 99)]  // near-diagonal
    public void Route90_ExtremeAspectRatios(int sx, int sy, int ex, int ey)
    {
        var start = new Vector2Int32(sx, sy);
        var end = new Vector2Int32(ex, ey);
        var result = WireRouter.Route90(start, end, false);
        AssertValidPath(result, start, end);
    }

    [Theory]
    [InlineData(0, 0, 100, 1)]
    [InlineData(0, 0, 1, 100)]
    [InlineData(0, 0, 50, 50)]
    [InlineData(0, 0, 100, 99)]
    public void RouteMiter_ExtremeAspectRatios(int sx, int sy, int ex, int ey)
    {
        var start = new Vector2Int32(sx, sy);
        var end = new Vector2Int32(ex, ey);
        var result = WireRouter.RouteMiter(start, end, false);
        AssertValidPath(result, start, end);
    }

    #endregion

    #region Bus Routing Tests

    /// <summary>
    /// Helper: route individual wires from a bus call so we can test each wire independently.
    /// </summary>
    private static List<List<Vector2Int32>> RouteIndividualWires(
        Vector2Int32 anchor, Vector2Int32 cursor,
        int brushWidth, int brushHeight, bool verticalFirst,
        int spacing, bool miter)
    {
        int wiresFromW = Math.Max(1, (brushWidth + spacing - 1) / spacing);
        int wiresFromH = Math.Max(1, (brushHeight + spacing - 1) / spacing);
        int numWires = Math.Min(wiresFromW, wiresFromH);
        int totalSpan = (numWires - 1) * spacing;
        int firstOffset = -totalSpan / 2;

        int dx = cursor.X - anchor.X;
        int dy = cursor.Y - anchor.Y;
        int sx = dx > 0 ? 1 : (dx < 0 ? -1 : 0);
        int sy = dy > 0 ? 1 : (dy < 0 ? -1 : 0);
        int edgeX = brushWidth / 2;
        int edgeY = brushHeight / 2;

        var wires = new List<List<Vector2Int32>>();
        for (int i = 0; i < numWires; i++)
        {
            int offset = firstOffset + i * spacing;
            Vector2Int32 start, end;

            if (verticalFirst)
            {
                start = new Vector2Int32(anchor.X + offset, anchor.Y - sy * edgeY);
                end = new Vector2Int32(cursor.X, cursor.Y - sy * sx * offset);
            }
            else
            {
                start = new Vector2Int32(anchor.X - sx * edgeX, anchor.Y + offset);
                end = new Vector2Int32(cursor.X - sx * sy * offset, cursor.Y);
            }

            var path = miter
                ? WireRouter.RouteMiter(start, end, verticalFirst)
                : WireRouter.Route90(start, end, verticalFirst);
            wires.Add(path);
        }
        return wires;
    }

    [Fact]
    public void ComputeWireOffsets_Width1_SingleWire()
    {
        var offsets = WireRouter.ComputeWireOffsets(1, 2);
        offsets.Length.ShouldBe(1);
        offsets[0].ShouldBe(0);
    }

    [Fact]
    public void ComputeWireOffsets_Width3_Spacing2_TwoWires()
    {
        var offsets = WireRouter.ComputeWireOffsets(3, 2);
        offsets.Length.ShouldBe(2);
        offsets[0].ShouldBe(-1);
        offsets[1].ShouldBe(1);
    }

    [Fact]
    public void ComputeWireOffsets_Width5_Spacing2_ThreeWires()
    {
        var offsets = WireRouter.ComputeWireOffsets(5, 2);
        offsets.Length.ShouldBe(3);
        offsets[0].ShouldBe(-2);
        offsets[1].ShouldBe(0);
        offsets[2].ShouldBe(2);
    }

    [Fact]
    public void ComputeWireOffsets_Width5_Spacing3_TwoWires()
    {
        var offsets = WireRouter.ComputeWireOffsets(5, 3);
        offsets.Length.ShouldBe(2);
        // totalSpan = 3, firstOffset = -1
        offsets[0].ShouldBe(-1);
        offsets[1].ShouldBe(2);
    }

    [Fact]
    public void ComputeWireOffsets_Width7_Spacing3_ThreeWires()
    {
        var offsets = WireRouter.ComputeWireOffsets(7, 3);
        offsets.Length.ShouldBe(3);
        offsets[0].ShouldBe(-3);
        offsets[1].ShouldBe(0);
        offsets[2].ShouldBe(3);
    }

    [Fact]
    public void RouteBus90_1x1_MatchesSingleWire()
    {
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(20, 15);
        var bus = WireRouter.RouteBus90(anchor, cursor, 1, 1, false);
        var single = WireRouter.Route90(anchor, cursor, false);
        bus.Count.ShouldBe(single.Count);
        for (int i = 0; i < bus.Count; i++)
        {
            bus[i].X.ShouldBe(single[i].X);
            bus[i].Y.ShouldBe(single[i].Y);
        }
    }

    [Fact]
    public void RouteBusMiter_1x1_MatchesSingleWire()
    {
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(20, 15);
        var bus = WireRouter.RouteBusMiter(anchor, cursor, 1, 1, false);
        var single = WireRouter.RouteMiter(anchor, cursor, false);
        bus.Count.ShouldBe(single.Count);
        for (int i = 0; i < bus.Count; i++)
        {
            bus[i].X.ShouldBe(single[i].X);
            bus[i].Y.ShouldBe(single[i].Y);
        }
    }

    [Theory]
    [InlineData(3, 3, false)]
    [InlineData(5, 5, false)]
    [InlineData(5, 3, false)]
    [InlineData(3, 5, false)]
    [InlineData(3, 3, true)]
    [InlineData(5, 5, true)]
    public void RouteBus90_EachWireIs4Connected(int w, int h, bool verticalFirst)
    {
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(25, 20);
        var wires = RouteIndividualWires(anchor, cursor, w, h, verticalFirst, 2, false);
        wires.Count.ShouldBeGreaterThan(1);

        foreach (var wire in wires)
            AssertFourConnected(wire);
    }

    [Theory]
    [InlineData(5, 5, false)]
    [InlineData(7, 7, false)]
    [InlineData(7, 5, false)]
    [InlineData(5, 7, false)]
    [InlineData(5, 5, true)]
    [InlineData(7, 7, true)]
    public void RouteBusMiter_EachWireIs4Connected(int w, int h, bool verticalFirst)
    {
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(25, 20);
        var wires = RouteIndividualWires(anchor, cursor, w, h, verticalFirst, 3, true);
        wires.Count.ShouldBeGreaterThan(1);

        foreach (var wire in wires)
            AssertFourConnected(wire);
    }

    [Theory]
    [InlineData(10, 10, 20, 15)]  // right+down
    [InlineData(20, 15, 10, 10)]  // left+up
    [InlineData(10, 15, 20, 10)]  // right+up
    [InlineData(20, 10, 10, 15)]  // left+down
    public void RouteBus90_AllQuadrants(int ax, int ay, int cx, int cy)
    {
        var anchor = new Vector2Int32(ax, ay);
        var cursor = new Vector2Int32(cx, cy);
        var bus = WireRouter.RouteBus90(anchor, cursor, 5, 5, false);
        bus.Count.ShouldBeGreaterThan(0);

        var wires = RouteIndividualWires(anchor, cursor, 5, 5, false, 2, false);
        foreach (var wire in wires)
            AssertFourConnected(wire);
    }

    [Theory]
    [InlineData(10, 10, 20, 15)]
    [InlineData(20, 15, 10, 10)]
    [InlineData(10, 15, 20, 10)]
    [InlineData(20, 10, 10, 15)]
    public void RouteBusMiter_AllQuadrants(int ax, int ay, int cx, int cy)
    {
        var anchor = new Vector2Int32(ax, ay);
        var cursor = new Vector2Int32(cx, cy);
        var bus = WireRouter.RouteBusMiter(anchor, cursor, 7, 7, false);
        bus.Count.ShouldBeGreaterThan(0);

        var wires = RouteIndividualWires(anchor, cursor, 7, 7, false, 3, true);
        foreach (var wire in wires)
            AssertFourConnected(wire);
    }

    [Fact]
    public void RouteBus90_OppositeEdgeStart_HorizontalFirst_MovingRight()
    {
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(20, 10); // moving right, pure horizontal
        var wires = RouteIndividualWires(anchor, cursor, 5, 5, false, 2, false);

        // Moving right → start at left edge: anchor.X - edgeX = 10 - 2 = 8
        foreach (var wire in wires)
            wire[0].X.ShouldBe(8);
    }

    [Fact]
    public void RouteBus90_OppositeEdgeStart_VerticalFirst_MovingDown()
    {
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(10, 20); // moving down, pure vertical
        var wires = RouteIndividualWires(anchor, cursor, 5, 5, true, 2, false);

        // Moving down → start at top edge: anchor.Y - edgeY = 10 - 2 = 8
        foreach (var wire in wires)
            wire[0].Y.ShouldBe(8);
    }

    [Fact]
    public void RouteBus90_LimitedBySmallerDimension()
    {
        // Brush 5x3 with spacing=2: wiresFromW=3, wiresFromH=2, so numWires=2
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(20, 15);
        var wires = RouteIndividualWires(anchor, cursor, 5, 3, false, 2, false);
        wires.Count.ShouldBe(2);
    }

    [Fact]
    public void RouteBusMiter_LimitedBySmallerDimension()
    {
        // Brush 7x5 with spacing=3: wiresFromW=3, wiresFromH=2, so numWires=2
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(20, 15);
        var wires = RouteIndividualWires(anchor, cursor, 7, 5, false, 3, true);
        wires.Count.ShouldBe(2);
    }

    [Fact]
    public void RouteBus90_Width2_SingleWire()
    {
        // Brush 2x2 with spacing=2: ceil(2/2)=1 wire
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(20, 15);
        var bus = WireRouter.RouteBus90(anchor, cursor, 2, 2, false);
        bus.Count.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData(3, 3, false)]
    [InlineData(5, 5, false)]
    [InlineData(5, 3, false)]
    [InlineData(3, 5, false)]
    [InlineData(3, 3, true)]
    [InlineData(5, 5, true)]
    public void RouteBus90_NoOverlapBetweenWires(int w, int h, bool verticalFirst)
    {
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(25, 20);
        var wires = RouteIndividualWires(anchor, cursor, w, h, verticalFirst, 2, false);
        wires.Count.ShouldBeGreaterThan(1);

        // Build tile sets per wire, verify no shared tiles
        var allSets = wires.Select(wire => new HashSet<(int, int)>(wire.Select(p => (p.X, p.Y)))).ToList();
        for (int i = 0; i < allSets.Count; i++)
        {
            for (int j = i + 1; j < allSets.Count; j++)
            {
                foreach (var tile in allSets[i])
                {
                    allSets[j].Contains(tile).ShouldBeFalse(
                        $"Wire {i} and wire {j} share tile ({tile.Item1},{tile.Item2})");
                }
            }
        }
    }

    [Theory]
    [InlineData(10, 10, 25, 20)]  // right+down
    [InlineData(25, 20, 10, 10)]  // left+up
    [InlineData(10, 20, 25, 10)]  // right+up
    [InlineData(25, 10, 10, 20)]  // left+down
    public void RouteBus90_NoOverlapAllQuadrants(int ax, int ay, int cx, int cy)
    {
        var anchor = new Vector2Int32(ax, ay);
        var cursor = new Vector2Int32(cx, cy);
        var wires = RouteIndividualWires(anchor, cursor, 5, 5, false, 2, false);

        var allSets = wires.Select(wire => new HashSet<(int, int)>(wire.Select(p => (p.X, p.Y)))).ToList();
        for (int i = 0; i < allSets.Count; i++)
        {
            for (int j = i + 1; j < allSets.Count; j++)
            {
                foreach (var tile in allSets[i])
                {
                    allSets[j].Contains(tile).ShouldBeFalse(
                        $"Wire {i} and wire {j} share tile ({tile.Item1},{tile.Item2})");
                }
            }
        }
    }

    [Fact]
    public void RouteBus90_NoGlobalDuplicates()
    {
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(25, 20);
        var bus = WireRouter.RouteBus90(anchor, cursor, 5, 5, false);
        AssertNoGlobalDuplicates(bus);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RouteBus90_EndSpreadPerpendicularToSecondLeg(bool verticalFirst)
    {
        var anchor = new Vector2Int32(10, 10);
        var cursor = new Vector2Int32(25, 20);
        var wires = RouteIndividualWires(anchor, cursor, 5, 5, verticalFirst, 2, false);
        wires.Count.ShouldBeGreaterThan(1);

        if (verticalFirst)
        {
            // V-first: second leg is horizontal → ends spread along Y (same X)
            var endX = wires[0].Last().X;
            // All wires should NOT end at same X (they end at cursor.X - edgeX which is same for all...
            // but Y differs)
            var endYs = wires.Select(w => w.Last().Y).ToList();
            endYs.Distinct().Count().ShouldBe(wires.Count, "End Y positions should be unique");
        }
        else
        {
            // H-first: second leg is vertical → ends spread along X (same Y)
            var endYs = wires.Select(w => w.Last().Y).Distinct().ToList();
            endYs.Count.ShouldBe(1, "All wires should end at same Y");
            var endXs = wires.Select(w => w.Last().X).ToList();
            endXs.Distinct().Count().ShouldBe(wires.Count, "End X positions should be unique");
        }
    }

    #endregion
}
