using System.Linq;
using TEdit.Editor;
using TEdit.Editor.Clipboard;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Tests.Scripting;
using Xunit;

namespace TEdit.Tests.Editor;

/// <summary>
/// Free-form selections must treat multi-tile sprites (chests, tables...) as a unit,
/// otherwise copy/paste and delete would leave broken half-sprites behind.
/// </summary>
public sealed class SelectionSpriteTests
{
    private const ushort Table = 14; // 3x2

    // Places the first configured frame of a sprite, the same way the sprite tool lays out UVs.
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

    private static Selection Brush(params (int X, int Y)[] tiles)
    {
        var selection = new Selection();
        selection.SetTiles(tiles.Select(t => new Vector2Int32(t.X, t.Y)), selected: true);
        return selection;
    }

    private static bool AllSelected(Selection selection, RectangleInt32 area)
    {
        for (int x = area.Left; x < area.Right; x++)
            for (int y = area.Top; y < area.Bottom; y++)
                if (!selection.IsValid(x, y))
                    return false;
        return true;
    }

    private static bool NoneSelected(Selection selection, RectangleInt32 area)
    {
        for (int x = area.Left; x < area.Right; x++)
            for (int y = area.Top; y < area.Bottom; y++)
                if (selection.IsValid(x, y))
                    return false;
        return true;
    }

    // ── GetSpriteBounds ─────────────────────────────────────────────

    [Fact]
    public void GetSpriteBounds_FromEveryTileOfChest_ReturnsWholeChest()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 20, 20, "c");

        for (int dx = 0; dx < 2; dx++)
            for (int dy = 0; dy < 2; dy++)
                Assert.Equal(new RectangleInt32(20, 20, 2, 2), SpritePlacer.GetSpriteBounds(world, 20 + dx, 20 + dy));
    }

    [Fact]
    public void GetSpriteBounds_FromEveryTileOfTable_ReturnsWholeTable()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var table = PlaceSprite(world, Table, 30, 30);
        var expected = new RectangleInt32(30, 30, table.X, table.Y);

        for (int dx = 0; dx < table.X; dx++)
            for (int dy = 0; dy < table.Y; dy++)
                Assert.Equal(expected, SpritePlacer.GetSpriteBounds(world, 30 + dx, 30 + dy));
    }

    [Theory]
    // UVs as Terraria saves them (taken from a real world): tiles are 18px apart even where FrameGap is larger.
    [InlineData((ushort)15, 0, 960, 1, 2)]   // Pumpkin Chair, FrameGap (2, 4)
    [InlineData((ushort)497, 18, 760, 1, 2)] // Pumpkin Toilet, FrameGap (2, 4)
    [InlineData((ushort)172, 0, 722, 2, 2)]  // Pumpkin Sink, FrameGap (2, 3)
    [InlineData((ushort)93, 0, 1188, 1, 3)]  // Pumpkin Lamp
    public void GetSpriteBounds_GameSavedUVs_ReturnWholeSprite(ushort type, int u, int v, int width, int height)
    {
        var world = TestWorldFactory.CreateSmallWorld();
        for (int dx = 0; dx < width; dx++)
            for (int dy = 0; dy < height; dy++)
                world.Tiles[30 + dx, 30 + dy] = new Tile { IsActive = true, Type = type, U = (short)(u + dx * 18), V = (short)(v + dy * 18) };

        for (int dx = 0; dx < width; dx++)
            for (int dy = 0; dy < height; dy++)
                Assert.Equal(new RectangleInt32(30, 30, width, height), SpritePlacer.GetSpriteBounds(world, 30 + dx, 30 + dy));
    }

    [Fact]
    public void GetSpriteBounds_PlainBlockOrAirOrOutsideWorld_IsNull()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        world.Tiles[5, 5] = new Tile { IsActive = true, Type = 1 };

        Assert.Null(SpritePlacer.GetSpriteBounds(world, 5, 5));
        Assert.Null(SpritePlacer.GetSpriteBounds(world, 6, 6));
        Assert.Null(SpritePlacer.GetSpriteBounds(world, -1, 0));
        Assert.Null(SpritePlacer.GetSpriteBounds(world, world.TilesWide, 0));
        Assert.Null(SpritePlacer.GetSpriteBounds(null, 0, 0));
    }

    [Fact]
    public void GetSpriteBounds_AdjacentChests_AreSeparate()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 20, 20, "left");
        PlaceChest(world, 22, 20, "right");

        Assert.Equal(new RectangleInt32(20, 20, 2, 2), SpritePlacer.GetSpriteBounds(world, 21, 21));
        Assert.Equal(new RectangleInt32(22, 20, 2, 2), SpritePlacer.GetSpriteBounds(world, 22, 21));
    }

    // ── SnapToSprites ───────────────────────────────────────────────

    [Fact]
    public void Snap_Adding_OneTileOfChest_SelectsWholeChest()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 20, 20, "c");
        var selection = Brush((21, 21));

        selection.SnapToSprites(world, include: true);

        Assert.True(AllSelected(selection, new RectangleInt32(20, 20, 2, 2)));
    }

    [Fact]
    public void Snap_Adding_GrowsSelectionBeyondBrushBounds()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var table = PlaceSprite(world, Table, 30, 30);
        var selection = Brush((30, 30));

        selection.SnapToSprites(world, include: true);

        Assert.Equal(new RectangleInt32(30, 30, table.X, table.Y), selection.SelectionArea);
    }

    [Fact]
    public void Snap_Removing_OneTileOfChest_DeselectsWholeChest()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 20, 20, "c");
        var selection = Brush((19, 19), (20, 20), (21, 20), (20, 21), (21, 21), (22, 22));
        selection.SetTiles(new[] { new Vector2Int32(20, 20) }, selected: false);

        selection.SnapToSprites(world, include: false);

        Assert.True(NoneSelected(selection, new RectangleInt32(20, 20, 2, 2)));
        Assert.True(selection.IsValid(19, 19));
        Assert.True(selection.IsValid(22, 22));
    }

    [Fact]
    public void Snap_Removing_LeavesFullySelectedSpritesAlone()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 20, 20, "whole");
        var selection = Brush((20, 20), (21, 20), (20, 21), (21, 21), (40, 40));

        selection.SnapToSprites(world, include: false);

        Assert.True(AllSelected(selection, new RectangleInt32(20, 20, 2, 2)));
    }

    [Fact]
    public void Snap_IgnoresPlainBlocks()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        world.Tiles[10, 10] = new Tile { IsActive = true, Type = 1 };
        var selection = Brush((10, 10));

        selection.SnapToSprites(world, include: true);

        Assert.Equal(new RectangleInt32(10, 10, 1, 1), selection.SelectionArea);
    }

    [Fact]
    public void Snap_OnlyTouchesSpritesThatAreSelected()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 20, 20, "picked");
        PlaceChest(world, 22, 20, "neighbor");
        var selection = Brush((21, 20));

        selection.SnapToSprites(world, include: true);

        Assert.True(AllSelected(selection, new RectangleInt32(20, 20, 2, 2)));
        Assert.True(NoneSelected(selection, new RectangleInt32(22, 20, 2, 2)));
    }

    [Fact]
    public void Snap_RectangleSelection_IsUnchanged()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 20, 20, "c");
        var selection = new Selection();
        selection.SetRectangle(new Vector2Int32(21, 21), new Vector2Int32(25, 25));

        selection.SnapToSprites(world, include: true);

        Assert.False(selection.HasMask);
        Assert.Equal(new RectangleInt32(21, 21, 5, 5), selection.SelectionArea);
    }

    [Fact]
    public void Snap_NullWorldOrInactive_IsNoOp()
    {
        var selection = new Selection();
        selection.SnapToSprites(null, include: true);
        selection.SnapToSprites(TestWorldFactory.CreateSmallWorld(), include: true);

        Assert.False(selection.IsActive);
    }

    // ── End to end: copy / paste ────────────────────────────────────

    [Fact]
    public void CopyPaste_BrushOverPartOfChest_PastesWholeChestWithItsData()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 20, 20, "Loot");
        world.Chests[0].Items[0].NetId = 29;
        world.Chests[0].Items[0].StackSize = 3;
        var selection = Brush((21, 21), (22, 22));
        selection.SnapToSprites(world, include: true);

        var buffer = ClipboardBuffer.GetSelectionBuffer(world, selection.SelectionArea, include: selection.IsValid);
        buffer.Paste(world, new Vector2Int32(60, 60), undo: null, new PasteOptions());

        var pasted = Assert.Single(world.Chests, c => c.X == 60 && c.Y == 60);
        Assert.Equal("Loot", pasted.Name);
        Assert.Equal(29, pasted.Items[0].NetId);
        Assert.Equal(3, pasted.Items[0].StackSize);
        for (int dx = 0; dx < 2; dx++)
            for (int dy = 0; dy < 2; dy++)
            {
                Assert.Equal(world.Tiles[20 + dx, 20 + dy].Type, world.Tiles[60 + dx, 60 + dy].Type);
                Assert.Equal(world.Tiles[20 + dx, 20 + dy].U, world.Tiles[60 + dx, 60 + dy].U);
                Assert.Equal(world.Tiles[20 + dx, 20 + dy].V, world.Tiles[60 + dx, 60 + dy].V);
            }
    }

    [Fact]
    public void CopyPaste_TableAcrossLassoEdge_PastesCompleteTable()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var table = PlaceSprite(world, Table, 30, 30);
        var selection = new Selection();
        // Lasso that only clips the table's left column.
        selection.SetLasso(new[] { new Vector2Int32(26, 28), new Vector2Int32(30, 28), new Vector2Int32(30, 33), new Vector2Int32(26, 33) }, selected: true);
        selection.SnapToSprites(world, include: true);

        var buffer = ClipboardBuffer.GetSelectionBuffer(world, selection.SelectionArea, include: selection.IsValid);
        int offsetX = 60 - selection.SelectionArea.X;
        int offsetY = 60 - selection.SelectionArea.Y;
        buffer.Paste(world, new Vector2Int32(60, 60), undo: null, new PasteOptions());

        for (int dx = 0; dx < table.X; dx++)
            for (int dy = 0; dy < table.Y; dy++)
            {
                var source = world.Tiles[30 + dx, 30 + dy];
                var copy = world.Tiles[30 + dx + offsetX, 30 + dy + offsetY];
                Assert.True(copy.IsActive);
                Assert.Equal(Table, copy.Type);
                Assert.Equal(source.U, copy.U);
                Assert.Equal(source.V, copy.V);
            }
    }
}
