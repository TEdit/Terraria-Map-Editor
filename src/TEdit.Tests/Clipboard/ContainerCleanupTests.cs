using System.Collections.ObjectModel;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using TEdit.Editor.Clipboard;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Tests.Scripting;

namespace TEdit.Tests.Clipboard;

/// <summary>
/// Tests for ghost tile entity / chest / sign cleanup during
/// copy, paste, delete, erase, undo, and redo operations.
/// A "ghost" entry is a chest/sign/tile-entity that remains in the
/// world's collection after its corresponding tile has been removed
/// or overwritten with a non-container tile.
/// </summary>
public class ContainerCleanupTests
{
    private readonly ITestOutputHelper _output;

    public ContainerCleanupTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Paste over container — ghost cleanup

    [Fact]
    public void Paste_NonChestOverChestTile_ShouldRemoveChestEntry()
    {
        // Arrange: world with a chest at (10, 40)
        var world = TestWorldFactory.CreateWorldWithChestsAndSigns();
        var chestBefore = world.GetChestAtTile(10, 40);
        Assert.NotNull(chestBefore);

        // Create a buffer with a plain dirt tile at the chest position
        var buffer = new ClipboardBuffer(
            new Vector2Int32(1, 1),
            tileFrameImportant: world.TileFrameImportant);
        buffer.Tiles[0, 0] = new Tile { IsActive = true, Type = 0 }; // Dirt

        // Act: paste dirt over the chest location
        buffer.Paste(world, new Vector2Int32(10, 40), undo: null, new PasteOptions());

        // Assert: tile should no longer be a chest
        Assert.False(world.Tiles[10, 40].IsChest());

        // The chest entry should be removed — a ghost means it persists
        var ghostChest = world.GetChestAtTile(10, 40);
        Assert.Null(ghostChest); // FAILS if ghost entity bug exists
    }

    [Fact]
    public void Paste_NonSignOverSignTile_ShouldRemoveSignEntry()
    {
        var world = TestWorldFactory.CreateWorldWithChestsAndSigns();
        var signBefore = world.GetSignAtTile(15, 40);
        Assert.NotNull(signBefore);

        var buffer = new ClipboardBuffer(
            new Vector2Int32(1, 1),
            tileFrameImportant: world.TileFrameImportant);
        buffer.Tiles[0, 0] = new Tile { IsActive = true, Type = 0 }; // Dirt

        buffer.Paste(world, new Vector2Int32(15, 40), undo: null, new PasteOptions());

        Assert.False(world.Tiles[15, 40].IsSign());
        var ghostSign = world.GetSignAtTile(15, 40);
        Assert.Null(ghostSign); // FAILS if ghost entity bug exists
    }

    [Fact]
    public void Paste_NonEntityOverTileEntityTile_ShouldRemoveEntityEntry()
    {
        var world = TestWorldFactory.CreateWorldWithTileEntities();
        var entityBefore = world.GetTileEntityAtTile(10, 50);
        Assert.NotNull(entityBefore);

        var buffer = new ClipboardBuffer(
            new Vector2Int32(1, 1),
            tileFrameImportant: world.TileFrameImportant);
        buffer.Tiles[0, 0] = new Tile { IsActive = true, Type = 0 };

        buffer.Paste(world, new Vector2Int32(10, 50), undo: null, new PasteOptions());

        Assert.False(world.Tiles[10, 50].IsTileEntity());
        var ghostEntity = world.GetTileEntityAtTile(10, 50);
        Assert.Null(ghostEntity); // FAILS if ghost entity bug exists
    }

    #endregion

    #region Paste container over existing container — data replacement

    [Fact]
    public void Paste_SignOverExistingSign_ShouldReplaceSignData()
    {
        var world = TestWorldFactory.CreateWorldWithChestsAndSigns();
        var originalSign = world.GetSignAtTile(15, 40);
        Assert.NotNull(originalSign);
        Assert.Equal("Hello from TEdit!", originalSign.Text);

        // Create buffer with a sign at (0,0) that has different text
        var buffer = new ClipboardBuffer(
            new Vector2Int32(1, 1),
            tileFrameImportant: world.TileFrameImportant);
        buffer.Tiles[0, 0] = new Tile
        {
            IsActive = true,
            Type = (ushort)TileType.Sign
        };
        buffer.Signs.Add(new Sign(0, 0, "Replaced text"));

        buffer.Paste(world, new Vector2Int32(15, 40), undo: null, new PasteOptions());

        var updatedSign = world.GetSignAtTile(15, 40);
        Assert.NotNull(updatedSign);
        Assert.Equal("Replaced text", updatedSign.Text); // FAILS — old data kept
    }

    [Fact]
    public void Paste_TileEntityOverExistingEntity_ShouldReplaceEntityData()
    {
        var world = TestWorldFactory.CreateWorldWithTileEntities();

        // WeaponRack at (20, 50) with NetId=0
        var original = world.GetTileEntityAtTile(20, 50);
        Assert.NotNull(original);
        Assert.Equal(0, original.NetId);

        // Buffer with weapon rack that has a different NetId
        var buffer = new ClipboardBuffer(
            new Vector2Int32(1, 1),
            tileFrameImportant: world.TileFrameImportant);
        buffer.Tiles[0, 0] = new Tile
        {
            IsActive = true,
            Type = (ushort)TileType.WeaponRack
        };
        buffer.TileEntities.Add(new TileEntity
        {
            Type = (byte)TileEntityType.WeaponRack,
            PosX = 0,
            PosY = 0,
            NetId = 42,
            StackSize = 1,
        });

        buffer.Paste(world, new Vector2Int32(20, 50), undo: null, new PasteOptions());

        var updated = world.GetTileEntityAtTile(20, 50);
        Assert.NotNull(updated);
        Assert.Equal(42, updated.NetId); // FAILS — old entity data kept
    }

    #endregion

    #region Paste empty tile over container

    [Fact]
    public void Paste_EmptyOverChestTile_ShouldRemoveChestEntry()
    {
        var world = TestWorldFactory.CreateWorldWithChestsAndSigns();
        Assert.NotNull(world.GetChestAtTile(10, 40));

        var buffer = new ClipboardBuffer(
            new Vector2Int32(1, 1),
            tileFrameImportant: world.TileFrameImportant);
        buffer.Tiles[0, 0] = new Tile(); // empty tile

        buffer.Paste(world, new Vector2Int32(10, 40), undo: null,
            new PasteOptions { PasteEmpty = true });

        Assert.False(world.Tiles[10, 40].IsActive);
        var ghost = world.GetChestAtTile(10, 40);
        Assert.Null(ghost); // FAILS if ghost entity bug exists
    }

    #endregion

    #region Region-wide paste creates ghosts

    [Fact]
    public void Paste_RegionOverMultipleContainers_ShouldRemoveAllGhosts()
    {
        var world = TestWorldFactory.CreateWorldWithChestsAndSigns();

        int initialChestCount = world.Chests.Count;
        int initialSignCount = world.Signs.Count;
        Assert.True(initialChestCount > 0);
        Assert.True(initialSignCount > 0);

        // Copy a dirt-only region (no containers) that covers the container area
        int regionWidth = 30;
        int regionHeight = 5;
        var buffer = new ClipboardBuffer(
            new Vector2Int32(regionWidth, regionHeight),
            tileFrameImportant: world.TileFrameImportant);
        for (int x = 0; x < regionWidth; x++)
        {
            for (int y = 0; y < regionHeight; y++)
            {
                buffer.Tiles[x, y] = new Tile { IsActive = true, Type = 0 };
            }
        }

        // Paste at (5, 38) — covers chests at (10,40), (20,40) and sign at (15,40)
        buffer.Paste(world, new Vector2Int32(5, 38), undo: null, new PasteOptions());

        // Count remaining containers at the paste region
        var ghostChests = world.Chests.Where(c =>
            c.X >= 5 && c.X < 5 + regionWidth &&
            c.Y >= 38 && c.Y < 38 + regionHeight).ToList();

        var ghostSigns = world.Signs.Where(s =>
            s.X >= 5 && s.X < 5 + regionWidth &&
            s.Y >= 38 && s.Y < 38 + regionHeight).ToList();

        _output.WriteLine($"Ghost chests: {ghostChests.Count}, Ghost signs: {ghostSigns.Count}");

        Assert.Empty(ghostChests); // FAILS if ghost entities persist
        Assert.Empty(ghostSigns);
    }

    #endregion

    #region Erase container tiles — ghost cleanup

    [Fact]
    public void Erase_ChestTile_ShouldRemoveChestEntry()
    {
        var world = TestWorldFactory.CreateWorldWithChestsAndSigns();
        Assert.NotNull(world.GetChestAtTile(10, 40));

        // Simulate erasing: clear the tile then clean up orphaned containers
        world.Tiles[10, 40].IsActive = false;
        world.Tiles[10, 40].Type = 0;
        world.RemoveOrphanedContainerAt(10, 40);

        var ghost = world.GetChestAtTile(10, 40);
        Assert.Null(ghost);
    }

    [Fact]
    public void Erase_SignTile_ShouldRemoveSignEntry()
    {
        var world = TestWorldFactory.CreateWorldWithChestsAndSigns();
        Assert.NotNull(world.GetSignAtTile(15, 40));

        world.Tiles[15, 40].IsActive = false;
        world.Tiles[15, 40].Type = 0;
        world.RemoveOrphanedContainerAt(15, 40);

        var ghost = world.GetSignAtTile(15, 40);
        Assert.Null(ghost);
    }

    [Fact]
    public void Erase_TileEntityTile_ShouldRemoveEntityEntry()
    {
        var world = TestWorldFactory.CreateWorldWithTileEntities();
        Assert.NotNull(world.GetTileEntityAtTile(20, 50)); // WeaponRack

        world.Tiles[20, 50].IsActive = false;
        world.Tiles[20, 50].Type = 0;
        world.RemoveOrphanedContainerAt(20, 50);

        var ghost = world.GetTileEntityAtTile(20, 50);
        Assert.Null(ghost);
    }

    #endregion

    #region Multiple entity types — comprehensive coverage

    [Fact]
    public void Erase_AllTileEntityTypes_ShouldRemoveAllEntityEntries()
    {
        var world = TestWorldFactory.CreateWorldWithTileEntities();

        // Verify all entities exist
        Assert.NotNull(world.GetTileEntityAtTile(10, 50)); // DisplayDoll
        Assert.NotNull(world.GetTileEntityAtTile(20, 50)); // WeaponRack
        Assert.NotNull(world.GetTileEntityAtTile(30, 50)); // HatRack
        Assert.NotNull(world.GetTileEntityAtTile(40, 50)); // ItemFrame
        Assert.NotNull(world.GetTileEntityAtTile(50, 50)); // FoodPlatter

        int initialCount = world.TileEntities.Count;
        Assert.Equal(5, initialCount);

        // Erase all entity tiles and clean up orphaned containers
        int[] entityXPositions = { 10, 20, 30, 40, 50 };
        foreach (int x in entityXPositions)
        {
            world.Tiles[x, 50].IsActive = false;
            world.Tiles[x, 50].Type = 0;
            world.RemoveOrphanedContainerAt(x, 50);
        }

        // All entity entries should be removed
        foreach (int x in entityXPositions)
        {
            var ghost = world.GetTileEntityAtTile(x, 50);
            Assert.Null(ghost);
        }

        Assert.Empty(world.TileEntities);
    }

    #endregion

    #region Copy preserves containers correctly

    [Fact]
    public void Copy_Region_CapturesContainersAtAnchorOnly()
    {
        var world = TestWorldFactory.CreateWorldWithChestsAndSigns();

        // Copy region that includes the chest at (10, 40)
        var area = new RectangleInt32(8, 38, 5, 5);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, area);

        // Buffer should have 1 chest at relative position (2, 2)
        Assert.Single(buffer.Chests);
        Assert.Equal(2, buffer.Chests[0].X); // 10 - 8
        Assert.Equal(2, buffer.Chests[0].Y); // 40 - 38
    }

    [Fact]
    public void Copy_Region_CapturesTileEntities()
    {
        var world = TestWorldFactory.CreateWorldWithTileEntities();

        // Copy region containing WeaponRack at (20, 50)
        var area = new RectangleInt32(18, 48, 5, 5);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, area);

        Assert.Single(buffer.TileEntities);
        Assert.Equal(2, buffer.TileEntities[0].PosX); // 20 - 18
        Assert.Equal(2, buffer.TileEntities[0].PosY); // 50 - 48
    }

    #endregion

    #region Paste then delete source — no duplicate containers

    [Fact]
    public void Paste_ThenClearSource_NoDuplicateChests()
    {
        var world = TestWorldFactory.CreateWorldWithChestsAndSigns();
        int initialChestCount = world.Chests.Count;

        // Copy region with chest at (10, 40)
        var area = new RectangleInt32(8, 38, 5, 5);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, area);
        Assert.Single(buffer.Chests);

        // Paste to new location
        buffer.Paste(world, new Vector2Int32(60, 38), undo: null, new PasteOptions());

        // Should have one more chest (the pasted copy)
        Assert.Equal(initialChestCount + 1, world.Chests.Count);

        // Now clear the original chest tile and clean up
        world.Tiles[10, 40].IsActive = false;
        world.Tiles[10, 40].Type = 0;
        world.RemoveOrphanedContainerAt(10, 40);

        var ghost = world.GetChestAtTile(10, 40);
        Assert.Null(ghost);
    }

    #endregion
}
