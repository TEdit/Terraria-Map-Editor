using Xunit;
using Xunit.Abstractions;
using TEdit.Terraria;
using TEdit.Geometry;
using TEdit.Editor.Clipboard;
using System.IO;
using System.Linq;

namespace TEdit.Editor.Clipboard.Tests;

public class ClipboardEntityTests
{
    private readonly ITestOutputHelper _output;
    private const string TestWorldFile = ".\\WorldFiles\\test-entity-world.wld";

    // Known test region with entities: (64,56) to (79,87)
    private static readonly RectangleInt32 TestRegion = new(64, 56, 16, 32);
    private const int PasteOffsetX = 75; // Paste 75 tiles to the right to avoid overlap with original

    public ClipboardEntityTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Checks if a .wld file is a valid binary world file (not a Git LFS pointer).
    /// </summary>
    private static bool IsValidWorldFile(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists) return false;
        if (info.Length < 500) return false;

        using var fs = File.OpenRead(path);
        var buf = new byte[8];
        if (fs.Read(buf, 0, 8) < 8) return false;
        var header = System.Text.Encoding.ASCII.GetString(buf);
        return !header.StartsWith("version ");
    }

    [Fact]
    public void CopyPaste_ChestWithItems_PreservesContents()
    {
        if (!IsValidWorldFile(TestWorldFile)) return;

        // Arrange - Load world
        var (world, error) = World.LoadWorld(TestWorldFile);
        Assert.Null(error);
        Assert.NotNull(world);

        // Find chest in test region
        var chestInfo = FindChestInRegion(world, TestRegion);
        Assert.NotNull(chestInfo.chest);

        var (chestX, chestY, originalChest) = chestInfo;
        _output.WriteLine($"Found chest at ({chestX}, {chestY}) with {originalChest!.Items.Count(i => i.NetId != 0)} items");

        // Act - Copy the test region
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, TestRegion);

        // Verify chest was copied to buffer
        int bufferChestX = chestX - TestRegion.X;
        int bufferChestY = chestY - TestRegion.Y;
        var bufferChest = buffer.GetChestAtTile(bufferChestX, bufferChestY);
        Assert.NotNull(bufferChest);
        _output.WriteLine($"Buffer chest at ({bufferChestX}, {bufferChestY})");

        // Act - Paste 100 tiles to the right
        var pasteAnchor = new Vector2Int32(TestRegion.X + PasteOffsetX, TestRegion.Y);
        buffer.Paste(world, pasteAnchor, undo: null, new PasteOptions());

        // Assert - Find pasted chest at new location
        int pastedChestX = chestX + PasteOffsetX;
        var pastedChest = world.GetChestAtTile(pastedChestX, chestY);
        Assert.NotNull(pastedChest);
        _output.WriteLine($"Pasted chest at ({pastedChest.X}, {pastedChest.Y})");

        // Verify position
        Assert.Equal(pastedChestX, pastedChest.X);
        Assert.Equal(chestY, pastedChest.Y);

        // Verify items match
        AssertChestItemsEqual(originalChest, pastedChest);
        _output.WriteLine("All items verified successfully");
    }

    [Fact]
    public void CopyPaste_Sign_PreservesText()
    {
        if (!IsValidWorldFile(TestWorldFile)) return;

        // Arrange
        var (world, error) = World.LoadWorld(TestWorldFile);
        Assert.Null(error);
        Assert.NotNull(world);

        var signInfo = FindSignInRegion(world, TestRegion);
        Assert.NotNull(signInfo.sign);

        var (signX, signY, originalSign) = signInfo;

        // Act - Copy
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, TestRegion);

        // Verify sign was copied to buffer
        int bufferSignX = signX - TestRegion.X;
        int bufferSignY = signY - TestRegion.Y;
        var bufferSign = buffer.GetSignAtTile(bufferSignX, bufferSignY);
        Assert.NotNull(bufferSign);

        // Act - Paste
        var pasteAnchor = new Vector2Int32(TestRegion.X + PasteOffsetX, TestRegion.Y);
        buffer.Paste(world, pasteAnchor, undo: null, new PasteOptions());

        // Assert - Find pasted sign
        int pastedSignX = signX + PasteOffsetX;
        var pastedSign = world.GetSignAtTile(pastedSignX, signY);
        Assert.NotNull(pastedSign);

        // Verify position and text
        Assert.Equal(pastedSignX, pastedSign.X);
        Assert.Equal(signY, pastedSign.Y);
        Assert.Equal(originalSign!.Text, pastedSign.Text);
    }

    [Fact]
    public void CopyPaste_TileEntity_PreservesData()
    {
        if (!IsValidWorldFile(TestWorldFile)) return;

        // Arrange
        var (world, error) = World.LoadWorld(TestWorldFile);
        Assert.Null(error);
        Assert.NotNull(world);

        var entityInfo = FindTileEntityInRegion(world, TestRegion);
        Assert.NotNull(entityInfo.entity);

        var (entityX, entityY, originalEntity) = entityInfo;

        // Act - Copy
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, TestRegion);

        // Verify entity was copied to buffer
        int bufferEntityX = entityX - TestRegion.X;
        int bufferEntityY = entityY - TestRegion.Y;
        var bufferEntity = buffer.GetTileEntityAtTile(bufferEntityX, bufferEntityY);
        Assert.NotNull(bufferEntity);

        // Act - Paste
        var pasteAnchor = new Vector2Int32(TestRegion.X + PasteOffsetX, TestRegion.Y);
        buffer.Paste(world, pasteAnchor, undo: null, new PasteOptions());

        // Assert - Find pasted entity
        int pastedEntityX = entityX + PasteOffsetX;
        var pastedEntity = world.GetTileEntityAtTile(pastedEntityX, entityY);
        Assert.NotNull(pastedEntity);

        // Verify position and type
        Assert.Equal(pastedEntityX, pastedEntity.PosX);
        Assert.Equal(entityY, pastedEntity.PosY);
        Assert.Equal(originalEntity!.Type, pastedEntity.Type);

        // Verify entity-specific data based on type
        AssertTileEntityDataEqual(originalEntity, pastedEntity);
    }

    #region Helper Methods

    private static (int x, int y, Chest? chest) FindChestInRegion(World world, RectangleInt32 region)
    {
        for (int x = region.X; x < region.Right; x++)
        {
            for (int y = region.Y; y < region.Bottom; y++)
            {
                var tile = world.Tiles[x, y];
                if (tile.IsChest())
                {
                    var anchor = world.GetAnchor(x, y);
                    if (anchor.X == x && anchor.Y == y)
                    {
                        var chest = world.GetChestAtTile(x, y);
                        if (chest != null && chest.Items.Any(i => i.NetId != 0))
                        {
                            return (x, y, chest);
                        }
                    }
                }
            }
        }
        return (0, 0, null);
    }

    private static (int x, int y, Sign? sign) FindSignInRegion(World world, RectangleInt32 region)
    {
        for (int x = region.X; x < region.Right; x++)
        {
            for (int y = region.Y; y < region.Bottom; y++)
            {
                var tile = world.Tiles[x, y];
                if (tile.IsSign())
                {
                    var anchor = world.GetAnchor(x, y);
                    if (anchor.X == x && anchor.Y == y)
                    {
                        var sign = world.GetSignAtTile(x, y);
                        if (sign != null && !string.IsNullOrEmpty(sign.Text))
                        {
                            return (x, y, sign);
                        }
                    }
                }
            }
        }
        return (0, 0, null);
    }

    private static (int x, int y, TileEntity? entity) FindTileEntityInRegion(World world, RectangleInt32 region)
    {
        for (int x = region.X; x < region.Right; x++)
        {
            for (int y = region.Y; y < region.Bottom; y++)
            {
                var tile = world.Tiles[x, y];
                if (tile.IsTileEntity())
                {
                    var anchor = world.GetAnchor(x, y);
                    if (anchor.X == x && anchor.Y == y)
                    {
                        var entity = world.GetTileEntityAtTile(x, y);
                        if (entity != null)
                        {
                            return (x, y, entity);
                        }
                    }
                }
            }
        }
        return (0, 0, null);
    }

    private static void AssertChestItemsEqual(Chest expected, Chest actual)
    {
        Assert.Equal(expected.Items.Count, actual.Items.Count);

        for (int i = 0; i < expected.Items.Count; i++)
        {
            Assert.Equal(expected.Items[i].NetId, actual.Items[i].NetId);
            Assert.Equal(expected.Items[i].StackSize, actual.Items[i].StackSize);
            Assert.Equal(expected.Items[i].Prefix, actual.Items[i].Prefix);
        }
    }

    private static void AssertTileEntityDataEqual(TileEntity expected, TileEntity actual)
    {
        // Common properties
        Assert.Equal(expected.Type, actual.Type);

        // Type-specific properties
        Assert.Equal(expected.NetId, actual.NetId);
        Assert.Equal(expected.StackSize, actual.StackSize);
        Assert.Equal(expected.Prefix, actual.Prefix);
        Assert.Equal(expected.Npc, actual.Npc);

        // Items collection (for DisplayDoll, HatRack, etc.)
        if (expected.Items != null && actual.Items != null)
        {
            Assert.Equal(expected.Items.Count, actual.Items.Count);
            for (int i = 0; i < expected.Items.Count; i++)
            {
                Assert.Equal(expected.Items[i].Id, actual.Items[i].Id);
                Assert.Equal(expected.Items[i].StackSize, actual.Items[i].StackSize);
                Assert.Equal(expected.Items[i].Prefix, actual.Items[i].Prefix);
            }
        }

        // Dyes collection
        if (expected.Dyes != null && actual.Dyes != null)
        {
            Assert.Equal(expected.Dyes.Count, actual.Dyes.Count);
            for (int i = 0; i < expected.Dyes.Count; i++)
            {
                Assert.Equal(expected.Dyes[i].Id, actual.Dyes[i].Id);
                Assert.Equal(expected.Dyes[i].StackSize, actual.Dyes[i].StackSize);
                Assert.Equal(expected.Dyes[i].Prefix, actual.Dyes[i].Prefix);
            }
        }
    }

    #endregion
}
