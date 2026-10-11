using TEdit.Editor;
using TEdit.Editor.Clipboard;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Tests.Scripting;
using Xunit;

namespace TEdit.Tests.Clipboard;

public sealed class MaskedClipboardTests
{
    private const ushort Stone = 1;
    private const ushort Wood = 30;

    private static void FillTiles(World world, RectangleInt32 area, ushort type)
    {
        for (int x = area.Left; x < area.Right; x++)
            for (int y = area.Top; y < area.Bottom; y++)
                world.Tiles[x, y] = new Tile { IsActive = true, Type = type };
    }

    // Selects an L shape inside the 3x3 box at (10, 10): the left column and the bottom row.
    private static Selection LShape()
    {
        var selection = new Selection();
        selection.SetTiles(new[]
        {
            new Vector2Int32(10, 10), new Vector2Int32(10, 11), new Vector2Int32(10, 12),
            new Vector2Int32(11, 12), new Vector2Int32(12, 12),
        }, selected: true);
        return selection;
    }

    private static ClipboardBuffer CopyLShape(World world, Selection selection) =>
        ClipboardBuffer.GetSelectionBuffer(world, selection.SelectionArea, include: selection.IsValid);

    [Fact]
    public void GetSelectionBuffer_WithoutInclude_HasNoMask()
    {
        var world = TestWorldFactory.CreateSmallWorld();

        var buffer = ClipboardBuffer.GetSelectionBuffer(world, new RectangleInt32(10, 10, 3, 3));

        Assert.Null(buffer.Mask);
        Assert.False(buffer.IsMasked(1, 1));
    }

    [Fact]
    public void GetSelectionBuffer_WithInclude_CopiesOnlySelectedTiles()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        FillTiles(world, new RectangleInt32(10, 10, 3, 3), Stone);
        var selection = LShape();

        var buffer = CopyLShape(world, selection);

        Assert.Equal(new Vector2Int32(3, 3), buffer.Size);
        Assert.NotNull(buffer.Mask);
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                bool selected = selection.IsValid(x + 10, y + 10);
                Assert.Equal(selected, buffer.Mask[x, y]);
                Assert.Equal(!selected, buffer.IsMasked(x, y));
                Assert.Equal(selected, buffer.Tiles[x, y].IsActive);
            }
        }
    }

    [Fact]
    public void GetSelectionBuffer_WithInclude_SkipsChestsOutsideSelection()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        world.Chests.Add(new Chest(20, 20, "Outside"));
        for (int dx = 0; dx < 2; dx++)
            for (int dy = 0; dy < 2; dy++)
                world.Tiles[20 + dx, 20 + dy] = new Tile
                {
                    IsActive = true,
                    Type = (ushort)TileType.Chest,
                    U = (short)(dx * 18),
                    V = (short)(dy * 18),
                };

        var selection = new Selection();
        selection.SetTiles(new[] { new Vector2Int32(18, 18), new Vector2Int32(23, 23) }, selected: true);

        var buffer = ClipboardBuffer.GetSelectionBuffer(world, selection.SelectionArea, include: selection.IsValid);

        Assert.Empty(buffer.Chests);
    }

    [Fact]
    public void Paste_MaskedBuffer_LeavesUnselectedWorldTilesAlone_EvenWithPasteEmpty()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        FillTiles(world, new RectangleInt32(10, 10, 3, 3), Stone);
        FillTiles(world, new RectangleInt32(40, 40, 3, 3), Wood);
        var buffer = CopyLShape(world, LShape());

        buffer.Paste(world, new Vector2Int32(40, 40), undo: null, new PasteOptions { PasteEmpty = true });

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                var tile = world.Tiles[40 + x, 40 + y];
                Assert.True(tile.IsActive);
                Assert.Equal(buffer.IsMasked(x, y) ? Wood : Stone, tile.Type);
            }
        }
    }

    [Fact]
    public void Paste_MaskedBuffer_StillPastesSelectedEmptyTiles()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        // Selected source tiles are air; pasting them with PasteEmpty must clear the destination.
        FillTiles(world, new RectangleInt32(40, 40, 3, 3), Wood);
        var buffer = CopyLShape(world, LShape());

        buffer.Paste(world, new Vector2Int32(40, 40), undo: null, new PasteOptions { PasteEmpty = true });

        Assert.False(world.Tiles[40, 40].IsActive); // selected
        Assert.True(world.Tiles[42, 40].IsActive);  // not selected
    }

    [Fact]
    public void Paste_MaskedBuffer_KeepsChestInUnselectedArea()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        FillTiles(world, new RectangleInt32(10, 10, 3, 3), Stone);
        // Destination chest sits in the top-right 2x2, which the L shape does not cover.
        world.Chests.Add(new Chest(41, 40, "Keep"));
        for (int dx = 0; dx < 2; dx++)
            for (int dy = 0; dy < 2; dy++)
                world.Tiles[41 + dx, 40 + dy] = new Tile
                {
                    IsActive = true,
                    Type = (ushort)TileType.Chest,
                    U = (short)(dx * 18),
                    V = (short)(dy * 18),
                };
        var buffer = CopyLShape(world, LShape());

        buffer.Paste(world, new Vector2Int32(40, 40), undo: null, new PasteOptions());

        var chest = Assert.Single(world.Chests);
        Assert.Equal("Keep", chest.Name);
        Assert.Equal((ushort)TileType.Chest, world.Tiles[41, 40].Type);
        Assert.Equal(Stone, world.Tiles[40, 40].Type);
    }

    [Fact]
    public void Paste_UnmaskedBuffer_BehavesAsBefore()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        FillTiles(world, new RectangleInt32(10, 10, 3, 3), Stone);
        FillTiles(world, new RectangleInt32(40, 40, 3, 3), Wood);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, new RectangleInt32(10, 10, 3, 3));

        buffer.Paste(world, new Vector2Int32(40, 40), undo: null, new PasteOptions());

        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                Assert.Equal(Stone, world.Tiles[40 + x, 40 + y].Type);
    }

    [Fact]
    public void Clone_DeepCopiesMask()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var buffer = CopyLShape(world, LShape());

        var clone = buffer.Clone();
        clone.Mask[2, 0] = true;

        Assert.NotSame(buffer.Mask, clone.Mask);
        Assert.False(buffer.Mask[2, 0]);
        Assert.True(clone.Mask[0, 0]);
    }

    [Fact]
    public void Clone_UnmaskedBuffer_StaysUnmasked()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, new RectangleInt32(0, 0, 2, 2));

        Assert.Null(buffer.Clone().Mask);
    }

    [Fact]
    public void FlipX_MirrorsMaskHorizontally()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        FillTiles(world, new RectangleInt32(10, 10, 3, 3), Stone);
        var buffer = CopyLShape(world, LShape());

        var flipped = buffer.FlipX();

        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
            {
                Assert.Equal(buffer.Mask[x, y], flipped.Mask[2 - x, y]);
                Assert.Equal(buffer.Tiles[x, y].IsActive, flipped.Tiles[2 - x, y].IsActive);
            }
    }

    [Fact]
    public void FlipY_MirrorsMaskVertically()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var buffer = CopyLShape(world, LShape());

        var flipped = buffer.FlipY();

        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                Assert.Equal(buffer.Mask[x, y], flipped.Mask[x, 2 - y]);
    }

    [Fact]
    public void Rotate_TransformsMaskLikeTiles()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        // Non-square selection so a wrong axis swap shows up.
        var selection = new Selection();
        selection.SetTiles(new[] { new Vector2Int32(10, 10), new Vector2Int32(13, 10), new Vector2Int32(13, 11) }, selected: true);
        FillTiles(world, selection.SelectionArea, Stone);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, selection.SelectionArea, include: selection.IsValid);

        var rotated = buffer.Rotate();

        Assert.Equal(new Vector2Int32(buffer.Size.Y, buffer.Size.X), rotated.Size);
        Assert.Equal(rotated.Size.X, rotated.Mask.GetLength(0));
        Assert.Equal(rotated.Size.Y, rotated.Mask.GetLength(1));
        for (int x = 0; x < rotated.Size.X; x++)
            for (int y = 0; y < rotated.Size.Y; y++)
                Assert.Equal(rotated.Mask[x, y], rotated.Tiles[x, y].IsActive);
    }

    [Fact]
    public void Resize_ScalesMask()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var buffer = CopyLShape(world, LShape());

        var resized = buffer.Resize(6, 6);

        Assert.Equal(6, resized.Mask.GetLength(0));
        Assert.Equal(6, resized.Mask.GetLength(1));
        for (int x = 0; x < 6; x++)
            for (int y = 0; y < 6; y++)
                Assert.Equal(buffer.Mask[x / 2, y / 2], resized.Mask[x, y]);
    }

    [Fact]
    public void Paste_FlippedMaskedBuffer_UsesFlippedMask()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        FillTiles(world, new RectangleInt32(10, 10, 3, 3), Stone);
        FillTiles(world, new RectangleInt32(40, 40, 3, 3), Wood);
        var flipped = CopyLShape(world, LShape()).FlipX();

        flipped.Paste(world, new Vector2Int32(40, 40), undo: null, new PasteOptions());

        // L shape mirrored: right column and bottom row are pasted, top-left stays wood.
        Assert.Equal(Stone, world.Tiles[42, 40].Type);
        Assert.Equal(Stone, world.Tiles[40, 42].Type);
        Assert.Equal(Wood, world.Tiles[40, 40].Type);
        Assert.Equal(Wood, world.Tiles[41, 41].Type);
    }
}
