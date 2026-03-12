using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Shouldly;
using TEdit.Common.IO;
using TEdit.Editor.Clipboard;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Tests.Scripting;

namespace TEdit.Tests.Clipboard;

/// <summary>
/// Tests that copy/paste of mod tiles preserves virtual IDs and
/// associated entity NBT data (chests with mod items, tile entities
/// with ModItemData/ModGlobalData).
/// </summary>
public class ModTileCopyPasteTests
{
    private readonly ITestOutputHelper _output;

    // Virtual ID beyond vanilla range — treated as mod tile
    private static ushort ModTileId => (ushort)(WorldConfiguration.TileCount + 10);

    public ModTileCopyPasteTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static TagCompound CreateModItemData(string name = "Murasama", int damage = 9999)
    {
        var data = new TagCompound();
        data.Set("mod", "CalamityMod");
        data.Set("name", name);
        data.Set("damage", damage);
        return data;
    }

    private static List<TagCompound> CreateGlobalData()
    {
        var g = new TagCompound();
        g.Set("mod", "CalamityMod");
        g.Set("rogue", (byte)1);
        return new List<TagCompound> { g };
    }

    #region Virtual ID Preservation

    [Fact]
    public void CopyPaste_ModTile_PreservesVirtualId()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        int srcX = 10, srcY = 40;

        // Place a mod tile
        world.Tiles[srcX, srcY].IsActive = true;
        world.Tiles[srcX, srcY].Type = ModTileId;
        world.Tiles[srcX, srcY].U = 18;
        world.Tiles[srcX, srcY].V = 36;

        // Copy the region
        var region = new RectangleInt32(srcX, srcY, 1, 1);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, region);

        buffer.Tiles[0, 0].Type.ShouldBe(ModTileId);
        buffer.Tiles[0, 0].U.ShouldBe((short)18);
        buffer.Tiles[0, 0].V.ShouldBe((short)36);

        // Paste 20 tiles to the right
        int pasteX = srcX + 20;
        buffer.Paste(world, new Vector2Int32(pasteX, srcY), undo: null, new PasteOptions());

        world.Tiles[pasteX, srcY].IsActive.ShouldBeTrue();
        world.Tiles[pasteX, srcY].Type.ShouldBe(ModTileId);
        world.Tiles[pasteX, srcY].U.ShouldBe((short)18);
        world.Tiles[pasteX, srcY].V.ShouldBe((short)36);
    }

    [Fact]
    public void CopyPaste_MixedVanillaAndModTiles_PreservesBothTypes()
    {
        var world = TestWorldFactory.CreateSmallWorld();

        // Place vanilla tile at (10, 40) and mod tile at (11, 40)
        world.Tiles[10, 40].IsActive = true;
        world.Tiles[10, 40].Type = 1; // Stone
        world.Tiles[11, 40].IsActive = true;
        world.Tiles[11, 40].Type = ModTileId;

        var region = new RectangleInt32(10, 40, 2, 1);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, region);

        buffer.Tiles[0, 0].Type.ShouldBe((ushort)1);
        buffer.Tiles[1, 0].Type.ShouldBe(ModTileId);

        // Paste
        int pasteX = 50;
        buffer.Paste(world, new Vector2Int32(pasteX, 40), undo: null, new PasteOptions());

        world.Tiles[pasteX, 40].Type.ShouldBe((ushort)1);
        world.Tiles[pasteX + 1, 40].Type.ShouldBe(ModTileId);
    }

    [Fact]
    public void CopyPaste_ModTileRegion_PreservesAllTilesInGrid()
    {
        var world = TestWorldFactory.CreateSmallWorld();

        // Place a 3x2 mod tile region
        for (int dx = 0; dx < 3; dx++)
        {
            for (int dy = 0; dy < 2; dy++)
            {
                world.Tiles[10 + dx, 40 + dy].IsActive = true;
                world.Tiles[10 + dx, 40 + dy].Type = ModTileId;
                world.Tiles[10 + dx, 40 + dy].U = (short)(dx * 18);
                world.Tiles[10 + dx, 40 + dy].V = (short)(dy * 18);
            }
        }

        var region = new RectangleInt32(10, 40, 3, 2);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, region);

        int pasteX = 50;
        buffer.Paste(world, new Vector2Int32(pasteX, 40), undo: null, new PasteOptions());

        for (int dx = 0; dx < 3; dx++)
        {
            for (int dy = 0; dy < 2; dy++)
            {
                var tile = world.Tiles[pasteX + dx, 40 + dy];
                tile.Type.ShouldBe(ModTileId, $"Tile at offset ({dx},{dy})");
                tile.U.ShouldBe((short)(dx * 18), $"U at offset ({dx},{dy})");
                tile.V.ShouldBe((short)(dy * 18), $"V at offset ({dx},{dy})");
            }
        }
    }

    #endregion

    #region Mod Chest Item Preservation

    [Fact]
    public void CopyPaste_ModTileWithChest_PreservesModItems()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        int srcX = 10, srcY = 40;

        // Place mod tile with a chest containing mod items
        world.Tiles[srcX, srcY].IsActive = true;
        world.Tiles[srcX, srcY].Type = ModTileId;

        var chest = new Chest(srcX, srcY);
        chest.Items[0] = new Item(1, 10)
        {
            ModName = "CalamityMod",
            ModItemName = "Murasama",
            ModItemData = CreateModItemData(),
            ModGlobalData = CreateGlobalData(),
        };
        world.Chests.Add(chest);

        // Copy
        var region = new RectangleInt32(srcX, srcY, 1, 1);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, region);

        buffer.Chests.Count.ShouldBe(1);
        buffer.Chests[0].Items[0].ModName.ShouldBe("CalamityMod");
        buffer.Chests[0].Items[0].ModItemData.ShouldNotBeNull();

        // Paste
        int pasteX = srcX + 20;
        buffer.Paste(world, new Vector2Int32(pasteX, srcY), undo: null, new PasteOptions());

        var pastedChest = world.GetChestAtTile(pasteX, srcY);
        pastedChest.ShouldNotBeNull();
        pastedChest!.Items[0].ModName.ShouldBe("CalamityMod");
        pastedChest.Items[0].ModItemName.ShouldBe("Murasama");
        pastedChest.Items[0].ModItemData.ShouldNotBeNull();
        pastedChest.Items[0].ModItemData!.GetInt("damage").ShouldBe(9999);
        pastedChest.Items[0].ModGlobalData.ShouldNotBeNull();
        pastedChest.Items[0].ModGlobalData!.Count.ShouldBe(1);
    }

    [Fact]
    public void CopyPaste_ModTileChest_DeepCopiesModData()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        int srcX = 10, srcY = 40;

        world.Tiles[srcX, srcY].IsActive = true;
        world.Tiles[srcX, srcY].Type = ModTileId;

        var chest = new Chest(srcX, srcY);
        chest.Items[0] = new Item(1, 10)
        {
            ModName = "CalamityMod",
            ModItemName = "Murasama",
            ModItemData = CreateModItemData(),
        };
        world.Chests.Add(chest);

        var region = new RectangleInt32(srcX, srcY, 1, 1);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, region);

        int pasteX = srcX + 20;
        buffer.Paste(world, new Vector2Int32(pasteX, srcY), undo: null, new PasteOptions());

        // Mutate pasted chest — original should be unchanged
        var pastedChest = world.GetChestAtTile(pasteX, srcY);
        pastedChest!.Items[0].ModItemData!.Set("damage", 1);

        var originalChest = world.GetChestAtTile(srcX, srcY);
        originalChest!.Items[0].ModItemData!.GetInt("damage").ShouldBe(9999);
    }

    #endregion

    #region Mod TileEntity Preservation

    [Fact]
    public void CopyPaste_ModTileWithTileEntity_PreservesNbt()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        int srcX = 10, srcY = 40;

        world.Tiles[srcX, srcY].IsActive = true;
        world.Tiles[srcX, srcY].Type = ModTileId;

        var entity = new TileEntity
        {
            Type = (byte)TileEntityType.ItemFrame,
            Id = 0,
            PosX = (short)srcX,
            PosY = (short)srcY,
            ModItemData = CreateModItemData("EntityData", 42),
            ModGlobalData = CreateGlobalData(),
        };
        world.TileEntities.Add(entity);

        // Copy
        var region = new RectangleInt32(srcX, srcY, 1, 1);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, region);

        buffer.TileEntities.Count.ShouldBe(1);
        buffer.TileEntities[0].ModItemData.ShouldNotBeNull();
        buffer.TileEntities[0].ModItemData!.GetString("name").ShouldBe("EntityData");

        // Paste
        int pasteX = srcX + 20;
        buffer.Paste(world, new Vector2Int32(pasteX, srcY), undo: null, new PasteOptions());

        var pastedEntity = world.GetTileEntityAtTile(pasteX, srcY);
        pastedEntity.ShouldNotBeNull();
        pastedEntity!.PosX.ShouldBe((short)pasteX);
        pastedEntity.PosY.ShouldBe((short)srcY);
        pastedEntity.ModItemData.ShouldNotBeNull();
        pastedEntity.ModItemData!.GetString("name").ShouldBe("EntityData");
        pastedEntity.ModItemData.GetInt("damage").ShouldBe(42);
        pastedEntity.ModGlobalData.ShouldNotBeNull();
        pastedEntity.ModGlobalData!.Count.ShouldBe(1);
    }

    [Fact]
    public void CopyPaste_ModTileEntity_WithModItems_PreservesAll()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        int srcX = 10, srcY = 40;

        world.Tiles[srcX, srcY].IsActive = true;
        world.Tiles[srcX, srcY].Type = ModTileId;

        var entity = new TileEntity
        {
            Type = (byte)TileEntityType.ItemFrame,
            Id = 0,
            PosX = (short)srcX,
            PosY = (short)srcY,
        };
        entity.Items.Add(new TileEntityItem
        {
            ModItemData = CreateModItemData("FramedSword", 500),
            ModGlobalData = CreateGlobalData(),
        });
        world.TileEntities.Add(entity);

        var region = new RectangleInt32(srcX, srcY, 1, 1);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, region);

        int pasteX = srcX + 20;
        buffer.Paste(world, new Vector2Int32(pasteX, srcY), undo: null, new PasteOptions());

        var pastedEntity = world.GetTileEntityAtTile(pasteX, srcY);
        pastedEntity.ShouldNotBeNull();
        pastedEntity!.Items.Count.ShouldBe(1);
        pastedEntity.Items[0].ModItemData.ShouldNotBeNull();
        pastedEntity.Items[0].ModItemData!.GetString("name").ShouldBe("FramedSword");
        pastedEntity.Items[0].ModItemData.GetInt("damage").ShouldBe(500);
    }

    [Fact]
    public void CopyPaste_ModTileEntity_DeepCopiesNbt()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        int srcX = 10, srcY = 40;

        world.Tiles[srcX, srcY].IsActive = true;
        world.Tiles[srcX, srcY].Type = ModTileId;

        var entity = new TileEntity
        {
            Type = (byte)TileEntityType.ItemFrame,
            Id = 0,
            PosX = (short)srcX,
            PosY = (short)srcY,
            ModItemData = CreateModItemData("Original", 100),
        };
        world.TileEntities.Add(entity);

        var region = new RectangleInt32(srcX, srcY, 1, 1);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, region);

        int pasteX = srcX + 20;
        buffer.Paste(world, new Vector2Int32(pasteX, srcY), undo: null, new PasteOptions());

        // Mutate pasted entity — original should be unchanged
        var pastedEntity = world.GetTileEntityAtTile(pasteX, srcY);
        pastedEntity!.ModItemData!.Set("name", "Mutated");

        var originalEntity = world.GetTileEntityAtTile(srcX, srcY);
        originalEntity!.ModItemData!.GetString("name").ShouldBe("Original");
    }

    #endregion

    #region Undo Integration

    [Fact]
    public void CopyPaste_WithUndo_SavesTilesBeforePaste()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var undo = new NoOpUndoManager();

        // Place existing mod tile at paste target
        int targetX = 30, targetY = 40;
        world.Tiles[targetX, targetY].IsActive = true;
        world.Tiles[targetX, targetY].Type = ModTileId;

        // Create buffer with different mod tile
        var buffer = new ClipboardBuffer(
            new Vector2Int32(1, 1),
            tileFrameImportant: world.TileFrameImportant);
        buffer.Tiles[0, 0] = new Tile
        {
            IsActive = true,
            Type = (ushort)(ModTileId + 1), // Different mod tile
        };

        buffer.Paste(world, new Vector2Int32(targetX, targetY), undo, new PasteOptions());

        // Undo manager should have recorded the tile
        undo.SavedTiles.ShouldContain((targetX, targetY));

        // Tile should be updated
        world.Tiles[targetX, targetY].Type.ShouldBe((ushort)(ModTileId + 1));
    }

    #endregion

    #region Chest Deduplication

    [Fact]
    public void ClipboardFixture_ModloaderChest_OldFile_HasDuplicateChests()
    {
        // Negative test: this fixture was saved with the bug that duplicated chest
        // entries for each tile of a multi-tile mod chest. It documents the known
        // bad state that old schematics may contain.
        var fixturePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "WorldFiles", "modloader-chest.TEditSch");

        var buffer = ClipboardBuffer.Load(fixturePath);
        buffer.ShouldNotBeNull();

        _output.WriteLine($"Buffer size: {buffer!.Size.X}x{buffer.Size.Y}");
        _output.WriteLine($"Chest count: {buffer.Chests.Count}");
        foreach (var c in buffer.Chests)
            _output.WriteLine($"  Chest at ({c.X},{c.Y})");

        // Old file has 4 chests (one per tile of the 2x2 mod chest) — this is the bug
        buffer.Chests.Count.ShouldBeGreaterThan(1,
            "Old fixture should demonstrate the duplicate chest bug");
    }

    [Fact]
    public void ClipboardFixture_ModloaderChestFixed_HasExactlyOneChest()
    {
        // Positive test: fixture exported after the deduplication fix
        var fixturePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "WorldFiles", "modloader-chest-fixed.TEditSch");

        var buffer = ClipboardBuffer.Load(fixturePath);
        buffer.ShouldNotBeNull();

        _output.WriteLine($"Buffer size: {buffer!.Size.X}x{buffer.Size.Y}");
        _output.WriteLine($"Chest count: {buffer.Chests.Count}");
        foreach (var c in buffer.Chests)
            _output.WriteLine($"  Chest at ({c.X},{c.Y})");

        buffer.Chests.Count.ShouldBe(1,
            "Fixed fixture should have exactly 1 chest entry for a single mod chest");
    }

    [Fact]
    public void ClipboardFixture_ModloaderChestFixed_RoundTrip_PreservesSingleChest()
    {
        var fixturePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "WorldFiles", "modloader-chest-fixed.TEditSch");

        var buffer = ClipboardBuffer.Load(fixturePath);
        buffer.ShouldNotBeNull();

        // Re-export and re-load
        var tempFile = Path.Combine(Path.GetTempPath(), $"TEdit_ChestDedup_{Guid.NewGuid():N}.TEditSch");
        try
        {
            buffer!.Save(tempFile, WorldConfiguration.CompatibleVersion);
            var reloaded = ClipboardBuffer.Load(tempFile);

            reloaded.ShouldNotBeNull();
            reloaded!.Chests.Count.ShouldBe(buffer.Chests.Count,
                "Re-exported schematic should preserve exactly the same number of chests");

            // Verify no duplicate positions
            var positions = reloaded.Chests.Select(c => (c.X, c.Y)).ToList();
            positions.Distinct().Count().ShouldBe(positions.Count,
                "Each chest should have a unique position");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetSelectionBuffer_MultiTileModChest_ProducesExactlyOneChestEntry()
    {
        var world = TestWorldFactory.CreateSmallWorld();
        int anchorX = 10, anchorY = 40;

        // Simulate a 2x2 mod chest (anchor at top-left)
        for (int dx = 0; dx < 2; dx++)
        {
            for (int dy = 0; dy < 2; dy++)
            {
                world.Tiles[anchorX + dx, anchorY + dy].IsActive = true;
                world.Tiles[anchorX + dx, anchorY + dy].Type = ModTileId;
                world.Tiles[anchorX + dx, anchorY + dy].U = (short)(dx * 18);
                world.Tiles[anchorX + dx, anchorY + dy].V = (short)(dy * 18);
            }
        }

        // Chest is registered at the anchor position
        var chest = new Chest(anchorX, anchorY);
        chest.Items[0] = new Item(1, 10)
        {
            ModName = "CalamityMod",
            ModItemName = "Murasama",
            ModItemData = CreateModItemData(),
        };
        world.Chests.Add(chest);

        // Copy the 2x2 region
        var region = new RectangleInt32(anchorX, anchorY, 2, 2);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, region);

        buffer.Chests.Count.ShouldBe(1,
            "A 2x2 mod chest should produce exactly 1 chest entry in the clipboard, not one per tile");
        buffer.Chests[0].X.ShouldBe(0);
        buffer.Chests[0].Y.ShouldBe(0);
        buffer.Chests[0].Items[0].ModName.ShouldBe("CalamityMod");
    }

    #endregion

    #region Clipboard File Serialization

    [Fact]
    public void ClipboardFile_RoundTrip_ModTilePreservesVirtualId()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"TEdit_ClipTest_{Guid.NewGuid():N}.TEditSch");
        try
        {
            var buffer = new ClipboardBuffer(
                new Vector2Int32(2, 1),
                tileFrameImportant: WorldConfiguration.SettingsTileFrameImportant);
            buffer.Name = "ModTileTest";
            buffer.Tiles[0, 0] = new Tile { IsActive = true, Type = 1 }; // Vanilla
            buffer.Tiles[1, 0] = new Tile { IsActive = true, Type = ModTileId, U = 36, V = 54 }; // Mod

            buffer.Save(tempFile, WorldConfiguration.CompatibleVersion);
            var loaded = ClipboardBuffer.Load(tempFile);

            loaded.ShouldNotBeNull();
            loaded!.Size.X.ShouldBe(2);
            loaded.Tiles[0, 0].Type.ShouldBe((ushort)1);
            loaded.Tiles[1, 0].Type.ShouldBe(ModTileId);
            loaded.Tiles[1, 0].U.ShouldBe((short)36);
            loaded.Tiles[1, 0].V.ShouldBe((short)54);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ClipboardFile_RoundTrip_ModChestAndEntityPreservesNbt()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"TEdit_ClipTest_{Guid.NewGuid():N}.TEditSch");
        try
        {
            var buffer = new ClipboardBuffer(
                new Vector2Int32(2, 1),
                tileFrameImportant: WorldConfiguration.SettingsTileFrameImportant);
            buffer.Name = "ModEntityTest";
            buffer.Tiles[0, 0] = new Tile { IsActive = true, Type = ModTileId };
            buffer.Tiles[1, 0] = new Tile { IsActive = true, Type = ModTileId };

            var chest = new Chest(0, 0);
            chest.Items[0] = new Item(1, 10)
            {
                ModName = "CalamityMod",
                ModItemName = "Murasama",
                ModItemData = CreateModItemData(),
                ModGlobalData = CreateGlobalData(),
            };
            buffer.Chests.Add(chest);

            var entity = new TileEntity
            {
                Type = (byte)TileEntityType.ItemFrame,
                Id = 0,
                PosX = 1,
                PosY = 0,
                ModItemData = CreateModItemData("EntityMod", 42),
            };
            buffer.TileEntities.Add(entity);

            buffer.Save(tempFile, WorldConfiguration.CompatibleVersion);
            var loaded = ClipboardBuffer.Load(tempFile);

            loaded.ShouldNotBeNull();

            // Verify mod chest
            loaded!.Chests.Count.ShouldBe(1);
            loaded.Chests[0].Items[0].ModName.ShouldBe("CalamityMod");
            loaded.Chests[0].Items[0].ModItemData.ShouldNotBeNull();
            loaded.Chests[0].Items[0].ModItemData!.GetInt("damage").ShouldBe(9999);

            // Verify mod entity
            loaded.TileEntities.Count.ShouldBe(1);
            loaded.TileEntities[0].ModItemData.ShouldNotBeNull();
            loaded.TileEntities[0].ModItemData!.GetString("name").ShouldBe("EntityMod");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    #endregion
}
