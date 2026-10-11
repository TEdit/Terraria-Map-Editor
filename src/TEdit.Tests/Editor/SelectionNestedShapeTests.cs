using System;
using System.Collections.Generic;
using System.Linq;
using TEdit.Editor;
using TEdit.Editor.Clipboard;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Tests.Scripting;
using Xunit;

namespace TEdit.Tests.Editor;

/// <summary>
/// Complex free-form selections: rings inside rings, islands inside holes, overlapping and
/// comb-like shapes. Each shape is checked against an independent predicate and pushed through
/// copy, paste, flip and rotate.
/// </summary>
public sealed class SelectionNestedShapeTests
{
    private const ushort Stone = 1;
    private const ushort Wood = 30;
    private const ushort Table = 14; // 3x2

    private static Vector2Int32 P(int x, int y) => new(x, y);

    // Square ring (Chebyshev distance) centered at (cx, cy) covering radii [inner, outer].
    private static bool InSquareRing(int x, int y, int cx, int cy, int inner, int outer)
    {
        int d = Math.Max(Math.Abs(x - cx), Math.Abs(y - cy));
        return d >= inner && d <= outer;
    }

    private static IEnumerable<Vector2Int32> Where(RectangleInt32 bounds, Func<int, int, bool> predicate)
    {
        for (int x = bounds.Left; x < bounds.Right; x++)
            for (int y = bounds.Top; y < bounds.Bottom; y++)
                if (predicate(x, y))
                    yield return P(x, y);
    }

    private static Selection Build(RectangleInt32 bounds, Func<int, int, bool> predicate)
    {
        var selection = new Selection();
        selection.SetTiles(Where(bounds, predicate), selected: true);
        return selection;
    }

    private static void AssertMatches(Selection selection, RectangleInt32 bounds, Func<int, int, bool> expected)
    {
        // Check a margin around the shape too, so nothing leaks outside.
        for (int x = bounds.Left - 2; x < bounds.Right + 2; x++)
            for (int y = bounds.Top - 2; y < bounds.Bottom + 2; y++)
                Assert.True(expected(x, y) == selection.IsValid(x, y), $"tile ({x},{y}) expected selected={expected(x, y)}");
    }

    /// <summary>
    /// Copies the selection from a stone-filled source, pastes it (optionally flipped/rotated) onto wood,
    /// and checks every destination tile: stone exactly where the transformed shape is selected.
    /// </summary>
    private static void AssertCopyPasteRespectsShape(Selection selection, Func<ClipboardBuffer, ClipboardBuffer> transform,
        Func<int, int, int, int, (int X, int Y)> mapToSource)
    {
        var world = TestWorldFactory.CreateSmallWorld(200, 200);
        var area = selection.SelectionArea;
        foreach (var p in Where(area, (_, _) => true))
            world.Tiles[p.X, p.Y] = new Tile { IsActive = true, Type = Stone };

        var buffer = transform(ClipboardBuffer.GetSelectionBuffer(world, area, include: selection.IsValid));
        var anchor = P(120, 120);
        foreach (var p in Where(new RectangleInt32(anchor.X, anchor.Y, buffer.Size.X, buffer.Size.Y), (_, _) => true))
            world.Tiles[p.X, p.Y] = new Tile { IsActive = true, Type = Wood };

        buffer.Paste(world, anchor, undo: null, new PasteOptions { PasteEmpty = true });

        for (int bx = 0; bx < buffer.Size.X; bx++)
        {
            for (int by = 0; by < buffer.Size.Y; by++)
            {
                var (sx, sy) = mapToSource(bx, by, area.Width, area.Height);
                bool selected = selection.IsValid(area.X + sx, area.Y + sy);
                var tile = world.Tiles[anchor.X + bx, anchor.Y + by];
                Assert.True(tile.IsActive);
                Assert.True((selected ? Stone : Wood) == tile.Type, $"buffer ({bx},{by}) source ({sx},{sy}) selected={selected}");
            }
        }
    }

    private static void AssertAllTransformsRespectShape(Selection selection)
    {
        AssertCopyPasteRespectsShape(selection, b => b, (x, y, w, h) => (x, y));
        AssertCopyPasteRespectsShape(selection, b => b.FlipX(), (x, y, w, h) => (w - 1 - x, y));
        AssertCopyPasteRespectsShape(selection, b => b.FlipY(), (x, y, w, h) => (x, h - 1 - y));
        // Rotate = vertical flip then swap axes.
        AssertCopyPasteRespectsShape(selection, b => b.Rotate(), (x, y, w, h) => (y, h - 1 - x));
    }

    // ── Rings inside rings ──────────────────────────────────────────

    // Target: three concentric square rings around (30, 30) with gaps between them and an open center.
    private static bool Target(int x, int y) =>
        InSquareRing(x, y, 30, 30, 18, 20) ||
        InSquareRing(x, y, 30, 30, 10, 13) ||
        InSquareRing(x, y, 30, 30, 3, 5);

    private static readonly RectangleInt32 TargetBounds = new(10, 10, 41, 41);

    [Fact]
    public void ConcentricRings_BuiltWithBrush_MatchShape()
    {
        var selection = Build(TargetBounds, Target);

        Assert.Equal(TargetBounds, selection.SelectionArea);
        AssertMatches(selection, TargetBounds, Target);
        Assert.False(selection.IsValid(30, 30)); // center
        Assert.False(selection.IsValid(30, 15)); // gap between outer and middle ring
        Assert.False(selection.IsValid(30, 23)); // gap between middle and inner ring
    }

    [Fact]
    public void ConcentricRings_BuiltWithAlternatingLassos_MatchShape()
    {
        // Add / subtract nested squares from the outside in.
        var selection = new Selection();
        int[] radii = { 20, 17, 13, 9, 5, 2 };
        for (int i = 0; i < radii.Length; i++)
        {
            int r = radii[i];
            selection.SetLasso(new[] { P(30 - r, 30 - r), P(30 + r, 30 - r), P(30 + r, 30 + r), P(30 - r, 30 + r) },
                selected: i % 2 == 0);
        }

        AssertMatches(selection, TargetBounds, Target);
    }

    [Fact]
    public void ConcentricRings_CopyPasteFlipRotate_RespectShape()
    {
        AssertAllTransformsRespectShape(Build(TargetBounds, Target));
    }

    // ── Two rings inside a big ring ─────────────────────────────────

    private static bool BigRingWithTwoRings(int x, int y) =>
        InSquareRing(x, y, 30, 30, 18, 20) ||   // big ring
        InSquareRing(x, y, 22, 30, 3, 5) ||     // left small ring
        InSquareRing(x, y, 38, 30, 3, 5);       // right small ring

    [Fact]
    public void TwoRingsInsideBigRing_MatchShape()
    {
        var selection = Build(TargetBounds, BigRingWithTwoRings);

        AssertMatches(selection, TargetBounds, BigRingWithTwoRings);
        Assert.False(selection.IsValid(22, 30)); // left ring hole
        Assert.False(selection.IsValid(38, 30)); // right ring hole
        Assert.False(selection.IsValid(30, 30)); // between the small rings
        Assert.False(selection.IsValid(30, 15)); // between small rings and big ring
    }

    [Fact]
    public void TwoRingsInsideBigRing_CopyPasteFlipRotate_RespectShape()
    {
        AssertAllTransformsRespectShape(Build(TargetBounds, BigRingWithTwoRings));
    }

    [Fact]
    public void TwoRingsInsideBigRing_RemovingOneSmallRing_KeepsTheOthers()
    {
        var selection = Build(TargetBounds, BigRingWithTwoRings);

        selection.SetTiles(Where(TargetBounds, (x, y) => InSquareRing(x, y, 22, 30, 0, 5)), selected: false);

        AssertMatches(selection, TargetBounds, (x, y) =>
            InSquareRing(x, y, 30, 30, 18, 20) || InSquareRing(x, y, 38, 30, 3, 5));
    }

    [Fact]
    public void TwoRingsInsideBigRing_RemovingBigRing_KeepsSmallRingsAndBoundsStayValid()
    {
        var selection = Build(TargetBounds, BigRingWithTwoRings);

        selection.SetTiles(Where(TargetBounds, (x, y) => InSquareRing(x, y, 30, 30, 18, 20)), selected: false);

        Assert.True(selection.IsActive);
        AssertMatches(selection, TargetBounds, (x, y) =>
            InSquareRing(x, y, 22, 30, 3, 5) || InSquareRing(x, y, 38, 30, 3, 5));
        AssertAllTransformsRespectShape(selection);
    }

    [Fact]
    public void TwoRingsInsideBigRing_SpritesInHolesAndAcrossEdges()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var selection = Build(TargetBounds, BigRingWithTwoRings);
        PlaceChest(world, 22, 30, "LeftHole");          // fully inside the left ring's hole
        PlaceChest(world, 29, 30, "Between");           // in the gap between the small rings
        var tableSize = PlaceSprite(world, Table, 42, 29); // x 42-44 crosses the right ring's outer edge (x = 43)

        selection.SnapToSprites(world, include: true);

        Assert.False(selection.IsValid(22, 30));
        Assert.False(selection.IsValid(29, 30));
        for (int dx = 0; dx < tableSize.X; dx++)
            for (int dy = 0; dy < tableSize.Y; dy++)
                Assert.True(selection.IsValid(42 + dx, 29 + dy));

        var buffer = ClipboardBuffer.GetSelectionBuffer(world, selection.SelectionArea, include: selection.IsValid);
        Assert.Empty(buffer.Chests);
    }

    // ── Other shapes ────────────────────────────────────────────────

    [Fact]
    public void IslandInsideHole_MatchShapeAndTransforms()
    {
        // Ring with a solid island in the middle of its hole.
        bool Shape(int x, int y) => InSquareRing(x, y, 30, 30, 8, 10) || InSquareRing(x, y, 30, 30, 0, 3);
        var selection = Build(TargetBounds, Shape);

        AssertMatches(selection, TargetBounds, Shape);
        AssertAllTransformsRespectShape(selection);
    }

    [Fact]
    public void OverlappingRings_UnionHasNoDoubleCountingOrGaps()
    {
        bool Shape(int x, int y) => InSquareRing(x, y, 25, 30, 4, 7) || InSquareRing(x, y, 33, 30, 4, 7);
        var selection = new Selection();
        selection.SetTiles(Where(TargetBounds, (x, y) => InSquareRing(x, y, 25, 30, 4, 7)), selected: true);
        selection.SetTiles(Where(TargetBounds, (x, y) => InSquareRing(x, y, 33, 30, 4, 7)), selected: true);

        AssertMatches(selection, TargetBounds, Shape);
        AssertAllTransformsRespectShape(selection);
    }

    [Fact]
    public void RoundDonutBrush_StampsLeaveRoundHoles()
    {
        var brush = new BrushSettings { Width = 4, Height = 15, Shape = BrushShape.Donut };
        var outer = brush.GetShapePoints(P(30, 30)).ToHashSet();
        var inner = new BrushSettings { Width = 4, Height = 9, Shape = BrushShape.Donut }.GetShapePoints(P(30, 30)).ToHashSet();
        var selection = new Selection();
        selection.SetTiles(outer, selected: true);
        selection.SetTiles(inner, selected: true);

        AssertMatches(selection, TargetBounds, (x, y) => outer.Contains(P(x, y)) || inner.Contains(P(x, y)));
        Assert.False(selection.IsValid(30, 30)); // center stays open
        AssertAllTransformsRespectShape(selection);
    }

    [Fact]
    public void CombShape_ManyRunsPerRow_MatchShapeAndTransforms()
    {
        // Vertical teeth on a spine: every other column, plus a solid bottom row.
        bool Shape(int x, int y) => x >= 10 && x < 40 && y >= 10 && y < 30 && (x % 2 == 0 || y == 29);
        var selection = Build(TargetBounds, Shape);

        AssertMatches(selection, TargetBounds, Shape);
        AssertAllTransformsRespectShape(selection);
    }

    [Fact]
    public void Checkerboard_DisconnectedSingleTiles_MatchShapeAndTransforms()
    {
        bool Shape(int x, int y) => x >= 10 && x < 26 && y >= 10 && y < 22 && (x + y) % 2 == 0;
        var selection = Build(TargetBounds, Shape);

        AssertMatches(selection, TargetBounds, Shape);
        AssertAllTransformsRespectShape(selection);
    }

    [Fact]
    public void SpiralLasso_DoesNotThrowAndSelectsItsPath()
    {
        // A square spiral path; even-odd filling may open or close loops, but the path itself is always selected.
        var path = new List<Vector2Int32>();
        int x = 30, y = 30, step = 2;
        for (int i = 0; i < 12; i++)
        {
            switch (i % 4)
            {
                case 0: x += step; break;
                case 1: y += step; break;
                case 2: x -= step; break;
                case 3: y -= step; break;
            }
            path.Add(P(x, y));
            if (i % 2 == 1) step += 2;
        }
        var selection = new Selection();

        selection.SetLasso(path, selected: true);

        foreach (var p in path)
            Assert.True(selection.IsValid(p.X, p.Y));
        AssertAllTransformsRespectShape(selection);
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private static Vector2Short PlaceSprite(World world, ushort tileType, int x, int y)
    {
        var prop = WorldConfiguration.TileProperties[tileType];
        var frame = prop.Frames!.First();
        var size = frame.Size.X > 0 && frame.Size.Y > 0 ? frame.Size : prop.FrameSize![0];
        var interval = prop.TextureGrid + prop.FrameGap;
        for (int dx = 0; dx < size.X; dx++)
            for (int dy = 0; dy < size.Y; dy++)
                world.Tiles[x + dx, y + dy] = new Tile
                {
                    IsActive = true,
                    Type = tileType,
                    U = (short)(frame.UV.X + dx * interval.X),
                    V = (short)(frame.UV.Y + dy * interval.Y),
                };
        return size;
    }

    private static void PlaceChest(World world, int x, int y, string name)
    {
        PlaceSprite(world, (ushort)TileType.Chest, x, y);
        world.Chests.Add(new Chest(x, y, name));
    }
}
