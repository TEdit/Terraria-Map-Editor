using System.Linq;
using TEdit.Editor;
using TEdit.Editor.Clipboard;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Tests.Scripting;
using Xunit;

namespace TEdit.Tests.Editor;

/// <summary>
/// Selections with a hole in the middle (a ring), including sprites inside the hole or crossing its edge.
/// </summary>
public sealed class SelectionHoleTests
{
    private const ushort Stone = 1;
    private const ushort Wood = 30;
    private const ushort Table = 14; // 3x2

    private static Vector2Int32 P(int x, int y) => new(x, y);

    private static RectangleInt32 Rect(int x, int y, int w, int h) => new(x, y, w, h);

    private static Vector2Int32[] Tiles(RectangleInt32 area)
    {
        var points = new System.Collections.Generic.List<Vector2Int32>();
        for (int x = area.Left; x < area.Right; x++)
            for (int y = area.Top; y < area.Bottom; y++)
                points.Add(P(x, y));
        return points.ToArray();
    }

    // 9x9 ring at (10,10) with a 5x5 hole at (12,12).
    private static readonly RectangleInt32 Outer = Rect(10, 10, 9, 9);
    private static readonly RectangleInt32 Hole = Rect(12, 12, 5, 5);

    private static Selection Ring()
    {
        var selection = new Selection();
        selection.SetRectangle(P(Outer.Left, Outer.Top), P(Outer.Right - 1, Outer.Bottom - 1));
        selection.SetTiles(Tiles(Hole), selected: false);
        return selection;
    }

    private static void Fill(World world, RectangleInt32 area, ushort type)
    {
        foreach (var p in Tiles(area))
            world.Tiles[p.X, p.Y] = new Tile { IsActive = true, Type = type };
    }

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

    private static bool All(Selection s, RectangleInt32 area, bool selected) =>
        Tiles(area).All(p => s.IsValid(p.X, p.Y) == selected);

    // ── Shape ───────────────────────────────────────────────────────

    [Fact]
    public void Ring_KeepsBoundsAndExcludesHole()
    {
        var selection = Ring();

        Assert.Equal(Outer, selection.SelectionArea);
        Assert.True(All(selection, Hole, selected: false));
        Assert.True(selection.IsValid(11, 11));
        Assert.True(selection.IsValid(17, 17));
        Assert.Equal(81 - 25, Tiles(Outer).Count(p => selection.IsValid(p.X, p.Y)));
    }

    [Fact]
    public void Ring_FillingHoleBack_RestoresFullRectangle()
    {
        var selection = Ring();

        selection.SetTiles(Tiles(Hole), selected: true);

        Assert.True(All(selection, Outer, selected: true));
    }

    [Fact]
    public void DonutLasso_OuterAddInnerSubtract_LeavesHole()
    {
        var selection = new Selection();

        selection.SetLasso(new[] { P(0, 0), P(20, 0), P(20, 20), P(0, 20) }, selected: true);
        selection.SetLasso(new[] { P(5, 5), P(15, 5), P(15, 15), P(5, 15) }, selected: false);

        Assert.True(selection.IsValid(2, 2));
        Assert.True(selection.IsValid(18, 18));
        Assert.True(All(selection, Rect(5, 5, 11, 11), selected: false));
    }

    [Fact]
    public void SelfIntersectingLasso_DoesNotThrowAndSelectsItsEdges()
    {
        var selection = new Selection();

        // Figure-eight: the crossing must not break filling.
        selection.SetLasso(new[] { P(0, 0), P(10, 10), P(10, 0), P(0, 10) }, selected: true);

        Assert.True(selection.IsActive);
        Assert.True(selection.IsValid(0, 0));
        Assert.True(selection.IsValid(10, 10));
        Assert.True(selection.IsValid(5, 5));
    }

    // ── Copy / paste ────────────────────────────────────────────────

    [Fact]
    public void CopyRing_BufferMasksHole()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Fill(world, Outer, Stone);
        var selection = Ring();

        var buffer = ClipboardBuffer.GetSelectionBuffer(world, selection.SelectionArea, include: selection.IsValid);

        Assert.True(buffer.IsMasked(4, 4));
        Assert.False(buffer.Tiles[4, 4].IsActive);
        Assert.False(buffer.IsMasked(0, 0));
        Assert.True(buffer.Tiles[0, 0].IsActive);
    }

    [Fact]
    public void PasteRing_LeavesDestinationHoleUntouched_EvenWithPasteEmpty()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Fill(world, Outer, Stone);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, Outer, include: Ring().IsValid);
        var destination = Rect(50, 50, 9, 9);
        Fill(world, destination, Wood);
        PlaceChest(world, 53, 53, "InHole");

        buffer.Paste(world, P(50, 50), undo: null, new PasteOptions { PasteEmpty = true });

        Assert.Equal(Stone, world.Tiles[50, 50].Type);
        Assert.Equal(Stone, world.Tiles[58, 58].Type);
        Assert.Equal(Wood, world.Tiles[52, 52].Type);
        Assert.Equal((ushort)TileType.Chest, world.Tiles[53, 53].Type);
        Assert.Equal((ushort)TileType.Chest, world.Tiles[54, 54].Type);
        Assert.Equal("InHole", Assert.Single(world.Chests, c => c.X == 53 && c.Y == 53).Name);
    }

    [Fact]
    public void CopyRing_ChestInsideHole_IsNotCopied()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Fill(world, Outer, Stone);
        PlaceChest(world, 13, 13, "Hidden");
        var selection = Ring();
        selection.SnapToSprites(world, include: true);

        var buffer = ClipboardBuffer.GetSelectionBuffer(world, selection.SelectionArea, include: selection.IsValid);

        Assert.True(All(selection, Hole, selected: false));
        Assert.Empty(buffer.Chests);
        Assert.False(buffer.Tiles[3, 3].IsActive);
    }

    [Fact]
    public void FlipRing_HoleStaysInPlace_AsymmetricHoleMoves()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Fill(world, Outer, Stone);
        var selection = Ring();
        selection.SetTiles(new[] { P(11, 11) }, selected: false); // extra notch near top-left
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, selection.SelectionArea, include: selection.IsValid);

        var flipped = buffer.FlipX();

        Assert.True(flipped.IsMasked(4, 4));   // centered hole stays
        Assert.True(flipped.IsMasked(7, 1));   // notch mirrored to the right
        Assert.False(flipped.IsMasked(1, 1));
    }

    // ── Sprites crossing the hole edge ──────────────────────────────

    [Fact]
    public void CuttingHole_ThroughTable_DeselectsWholeTable()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        // Table straddles the hole's left edge: column 11 is in the ring, 12-13 are in the hole.
        var size = PlaceSprite(world, Table, 11, 14);
        var table = Rect(11, 14, size.X, size.Y);
        var selection = Ring();

        selection.SnapToSprites(world, include: false);

        Assert.True(All(selection, table, selected: false));
        Assert.True(selection.IsValid(10, 14));
        Assert.True(selection.IsValid(11, 13));
        Assert.True(selection.HasMask);
    }

    [Fact]
    public void AddingStroke_TouchingSpriteInHole_PullsWholeSpriteIn()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 14, 14, "Center");
        var selection = Ring();

        selection.SetTiles(new[] { P(14, 14) }, selected: true);
        selection.SnapToSprites(world, include: true);

        Assert.True(All(selection, Rect(14, 14, 2, 2), selected: true));
        Assert.False(selection.IsValid(13, 13)); // rest of the hole stays open
    }

    [Fact]
    public void SpriteFullyInsideHole_IsLeftAlone()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 13, 13, "Inside");
        var selection = Ring();

        selection.SnapToSprites(world, include: true);
        selection.SnapToSprites(world, include: false);

        Assert.True(All(selection, Hole, selected: false));
        Assert.Equal(81 - 25, Tiles(Outer).Count(p => selection.IsValid(p.X, p.Y)));
    }

    [Fact]
    public void SpriteFullyInsideRing_SurvivesHoleCut()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 10, 10, "Corner");
        var selection = Ring();

        selection.SnapToSprites(world, include: false);

        Assert.True(All(selection, Rect(10, 10, 2, 2), selected: true));
    }

    [Fact]
    public void CopyPaste_RingWithTableAcrossHoleEdge_NoHalfTableInBuffer()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        Fill(world, Outer, Stone);
        PlaceSprite(world, Table, 11, 14);
        var selection = Ring();
        selection.SnapToSprites(world, include: false);

        var buffer = ClipboardBuffer.GetSelectionBuffer(world, selection.SelectionArea, include: selection.IsValid);

        for (int x = 0; x < buffer.Size.X; x++)
            for (int y = 0; y < buffer.Size.Y; y++)
                Assert.NotEqual(Table, buffer.Tiles[x, y].IsActive ? buffer.Tiles[x, y].Type : (ushort)0);
    }
}
