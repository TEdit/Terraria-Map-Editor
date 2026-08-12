using System.Linq;
using TEdit.Editor.Clipboard;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Tests.Scripting;
using Xunit;

namespace TEdit.Tests.Clipboard;

public sealed class ChestPasteUndoRegressionTests
{
    [Fact]
    public void PasteChestIntoEmptySpace_WhenPasteOverTilesIsDisabled_PreservesDataThroughUndoRedo()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 10, 20, "Source", itemId: 29, stack: 3);

        var buffer = ClipboardBuffer.GetSelectionBuffer(world, new RectangleInt32(10, 20, 2, 2));
        var undo = new UndoManager(world, notifyTileChanged: null, undoApplied: () => { });
        var options = new PasteOptions { PasteOverTiles = false };

        try
        {
            buffer.Paste(world, new Vector2Int32(40, 20), undo, options);

            AssertChest(world, 10, 20, "Source", 29, 3);
            AssertChest(world, 40, 20, "Source", 29, 3);
            Assert.Equal(2, world.Chests.Count);

            undo.Undo();

            AssertChest(world, 10, 20, "Source", 29, 3);
            Assert.Null(world.GetChestAtTile(40, 20));
            Assert.Single(world.Chests);

            undo.Redo();

            AssertChest(world, 10, 20, "Source", 29, 3);
            AssertChest(world, 40, 20, "Source", 29, 3);
            Assert.Equal(2, world.Chests.Count);
        }
        finally
        {
            undo.Dispose();
        }
    }

    [Fact]
    public void PasteChestOverChest_PreservesClipboardDataThroughUndoRedo()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        PlaceChest(world, 10, 20, "Source", itemId: 29, stack: 3);
        PlaceChest(world, 40, 20, "Destination", itemId: 73, stack: 50);

        var buffer = ClipboardBuffer.GetSelectionBuffer(world, new RectangleInt32(10, 20, 2, 2));
        var undo = new UndoManager(world, notifyTileChanged: null, undoApplied: () => { });

        try
        {
            buffer.Paste(world, new Vector2Int32(40, 20), undo, new PasteOptions());

            AssertChest(world, 10, 20, "Source", 29, 3);
            AssertChest(world, 40, 20, "Source", 29, 3);
            Assert.Equal(2, world.Chests.Count);

            undo.Undo();

            AssertChest(world, 10, 20, "Source", 29, 3);
            AssertChest(world, 40, 20, "Destination", 73, 50);
            Assert.Equal(2, world.Chests.Count);

            undo.Redo();

            AssertChest(world, 10, 20, "Source", 29, 3);
            AssertChest(world, 40, 20, "Source", 29, 3);
            Assert.Equal(2, world.Chests.Count);
        }
        finally
        {
            undo.Dispose();
        }
    }

    private static void PlaceChest(World world, int x, int y, string name, int itemId, int stack)
    {
        var chest = new Chest(x, y, name);
        chest.Items[0].NetId = itemId;
        chest.Items[0].StackSize = stack;
        world.Chests.Add(chest);

        for (int dx = 0; dx < 2; dx++)
        {
            for (int dy = 0; dy < 2; dy++)
            {
                world.Tiles[x + dx, y + dy] = new Tile
                {
                    IsActive = true,
                    Type = (ushort)TileType.Chest,
                    U = (short)(dx * 18),
                    V = (short)(dy * 18),
                };
            }
        }
    }

    private static void AssertChest(
        World world,
        int x,
        int y,
        string name,
        int itemId,
        int stack)
    {
        var matches = world.Chests.Where(c => c.X == x && c.Y == y).ToList();
        var chest = Assert.Single(matches);
        Assert.Equal(name, chest.Name);
        Assert.Equal(itemId, chest.Items[0].NetId);
        Assert.Equal(stack, chest.Items[0].StackSize);
    }
}
