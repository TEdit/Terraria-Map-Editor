using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Shouldly;
using TEdit.Common.IO;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Tests.Undo;

/// <summary>
/// Tests that UndoBuffer round-trip serialization preserves mod tile
/// entity NBT data (ModItemData, ModGlobalData) through the full
/// write → read cycle including ModDataSerializer payload.
/// </summary>
public class ModTileUndoTests : IDisposable
{
    private readonly string _testDir;
    private readonly List<string> _tempFiles = new();

    public ModTileUndoTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "TEditUndoModTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file)) File.Delete(file);
        }
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    private string GetTestFileName()
    {
        var fileName = Path.Combine(_testDir, $"undo_{Guid.NewGuid()}.undo");
        _tempFiles.Add(fileName);
        return fileName;
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

    [Fact]
    public void UndoBuffer_RoundTrip_ModTilePreservesVirtualId()
    {
        var fileName = GetTestFileName();
        ushort modTileId = (ushort)(WorldConfiguration.TileCount + 10);
        var tile = new Tile { IsActive = true, Type = modTileId, U = 18, V = 36 };
        var location = new Vector2Int32(100, 200);

        using (var buffer = new UndoBuffer(fileName, null!))
        {
            buffer.Add(location, tile);
            buffer.Close();
        }

        using var stream = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream);
        var tiles = UndoBuffer.ReadUndoTilesFromStream(reader).ToList();

        tiles.Count.ShouldBe(1);
        tiles[0].Tile.Type.ShouldBe(modTileId);
        tiles[0].Tile.U.ShouldBe((short)18);
        tiles[0].Tile.V.ShouldBe((short)36);
        tiles[0].Location.ShouldBe(location);
    }

    [Fact]
    public void UndoBuffer_RoundTrip_ChestWithModItems_PreservesNbt()
    {
        var fileName = GetTestFileName();
        var version = WorldConfiguration.CompatibleVersion;

        var chest = new Chest(10, 40);
        chest.Items[0] = new Item(1, 10)
        {
            ModName = "CalamityMod",
            ModItemName = "Murasama",
            ModItemData = CreateModItemData(),
            ModGlobalData = CreateGlobalData(),
        };

        var tile = new Tile { IsActive = true, Type = 21 }; // Chest type
        var location = new Vector2Int32(10, 40);

        // Write
        using (var buffer = new UndoBuffer(fileName, null!))
        {
            buffer.Add(location, tile);
            buffer.Chests.Add(chest);
            buffer.Close();
        }

        // Read — same sequence as UndoManager.Undo()
        using var stream = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream);

        var tiles = UndoBuffer.ReadUndoTilesFromStream(reader).ToList();
        tiles.Count.ShouldBe(1);

        var loadedChests = World.LoadChestData(reader, version).ToList();
        var loadedSigns = World.LoadSignData(reader).ToList();
        var loadedEntities = World.LoadTileEntityData(reader, version).ToList();
        ModDataSerializer.LoadModPayload(reader, loadedChests, loadedEntities);

        loadedChests.Count.ShouldBe(1);
        loadedChests[0].X.ShouldBe(10);
        loadedChests[0].Y.ShouldBe(40);
        loadedChests[0].Items[0].ModName.ShouldBe("CalamityMod");
        loadedChests[0].Items[0].ModItemName.ShouldBe("Murasama");
        loadedChests[0].Items[0].ModItemData.ShouldNotBeNull();
        loadedChests[0].Items[0].ModItemData!.GetString("name").ShouldBe("Murasama");
        loadedChests[0].Items[0].ModItemData.GetInt("damage").ShouldBe(9999);
        loadedChests[0].Items[0].ModGlobalData.ShouldNotBeNull();
        loadedChests[0].Items[0].ModGlobalData!.Count.ShouldBe(1);
    }

    [Fact]
    public void UndoBuffer_RoundTrip_TileEntityWithModData_PreservesNbt()
    {
        var fileName = GetTestFileName();
        var version = WorldConfiguration.CompatibleVersion;

        var entity = new TileEntity
        {
            Type = (byte)TileEntityType.ItemFrame,
            Id = 0,
            PosX = 40,
            PosY = 50,
            ModItemData = CreateModItemData("EntityMod", 42),
            ModGlobalData = CreateGlobalData(),
        };

        var tile = new Tile { IsActive = true, Type = (ushort)TileType.ItemFrame };
        var location = new Vector2Int32(40, 50);

        // Write
        using (var buffer = new UndoBuffer(fileName, null!))
        {
            buffer.Add(location, tile);
            buffer.TileEntities.Add(entity);
            buffer.Close();
        }

        // Read
        using var stream = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream);

        var tiles = UndoBuffer.ReadUndoTilesFromStream(reader).ToList();
        var loadedChests = World.LoadChestData(reader, version).ToList();
        var loadedSigns = World.LoadSignData(reader).ToList();
        var loadedEntities = World.LoadTileEntityData(reader, version).ToList();
        ModDataSerializer.LoadModPayload(reader, loadedChests, loadedEntities);

        loadedEntities.Count.ShouldBe(1);
        loadedEntities[0].PosX.ShouldBe((short)40);
        loadedEntities[0].PosY.ShouldBe((short)50);
        loadedEntities[0].ModItemData.ShouldNotBeNull();
        loadedEntities[0].ModItemData!.GetString("name").ShouldBe("EntityMod");
        loadedEntities[0].ModItemData.GetInt("damage").ShouldBe(42);
        loadedEntities[0].ModGlobalData.ShouldNotBeNull();
        loadedEntities[0].ModGlobalData!.Count.ShouldBe(1);
    }

    [Fact]
    public void UndoBuffer_RoundTrip_DisplayDollWithModItems_PreservesAll()
    {
        var fileName = GetTestFileName();
        var version = WorldConfiguration.CompatibleVersion;

        // DisplayDoll uses Items collection (multi-slot entity)
        // Version 308+ requires 9 item/dye slots + 1 misc slot
        var entity = new TileEntity
        {
            Type = (byte)TileEntityType.DisplayDoll,
            Id = 0,
            PosX = 40,
            PosY = 50,
            Items = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(
                Enumerable.Range(0, 9).Select(_ => new TileEntityItem())),
            Dyes = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(
                Enumerable.Range(0, 9).Select(_ => new TileEntityItem())),
            Misc = new System.Collections.ObjectModel.ObservableCollection<TileEntityItem>(
                Enumerable.Range(0, 1).Select(_ => new TileEntityItem())),
        };
        entity.Items[0] = new TileEntityItem
        {
            ModItemData = CreateModItemData("FramedArmor", 500),
            ModGlobalData = CreateGlobalData(),
        };

        var tile = new Tile { IsActive = true, Type = (ushort)TileType.DisplayDoll };
        var location = new Vector2Int32(40, 50);

        // Write
        using (var buffer = new UndoBuffer(fileName, null!))
        {
            buffer.Add(location, tile);
            buffer.TileEntities.Add(entity);
            buffer.Close();
        }

        // Read
        using var stream = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream);

        var tiles = UndoBuffer.ReadUndoTilesFromStream(reader).ToList();
        var loadedChests = World.LoadChestData(reader, version).ToList();
        var loadedSigns = World.LoadSignData(reader).ToList();
        var loadedEntities = World.LoadTileEntityData(reader, version).ToList();
        ModDataSerializer.LoadModPayload(reader, loadedChests, loadedEntities);

        loadedEntities.Count.ShouldBe(1);
        loadedEntities[0].Items.Count.ShouldBeGreaterThanOrEqualTo(8); // 8 or 9 depending on version
        loadedEntities[0].Items[0].ModItemData.ShouldNotBeNull();
        loadedEntities[0].Items[0].ModItemData!.GetString("name").ShouldBe("FramedArmor");
        loadedEntities[0].Items[0].ModItemData.GetInt("damage").ShouldBe(500);
        loadedEntities[0].Items[0].ModGlobalData.ShouldNotBeNull();
        loadedEntities[0].Items[0].ModGlobalData!.Count.ShouldBe(1);
    }

    [Fact]
    public void UndoBuffer_RoundTrip_MultipleEntitiesWithMixedModData()
    {
        var fileName = GetTestFileName();
        var version = WorldConfiguration.CompatibleVersion;

        // Chest with mod items
        var chest = new Chest(10, 40);
        chest.Items[0] = new Item(1, 10)
        {
            ModName = "CalamityMod",
            ModItemName = "Murasama",
            ModItemData = CreateModItemData("Murasama", 9999),
            ModGlobalData = CreateGlobalData(),
        };

        // Vanilla chest (no mod data)
        var vanillaChest = new Chest(20, 40, "Plain Chest");
        vanillaChest.Items[0].NetId = 73;
        vanillaChest.Items[0].StackSize = 50;

        // TileEntity with mod data
        var entity = new TileEntity
        {
            Type = (byte)TileEntityType.ItemFrame,
            Id = 0,
            PosX = 30,
            PosY = 40,
            ModItemData = CreateModItemData("ModFrame", 77),
        };

        // Tiles
        var tile1 = new Tile { IsActive = true, Type = 21 };
        var tile2 = new Tile { IsActive = true, Type = 21 };
        var tile3 = new Tile { IsActive = true, Type = (ushort)TileType.ItemFrame };

        // Write
        using (var buffer = new UndoBuffer(fileName, null!))
        {
            buffer.Add(new Vector2Int32(10, 40), tile1);
            buffer.Add(new Vector2Int32(20, 40), tile2);
            buffer.Add(new Vector2Int32(30, 40), tile3);
            buffer.Chests.Add(chest);
            buffer.Chests.Add(vanillaChest);
            buffer.TileEntities.Add(entity);
            buffer.Close();
        }

        // Read
        using var stream = new FileStream(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream);

        var tiles = UndoBuffer.ReadUndoTilesFromStream(reader).ToList();
        var loadedChests = World.LoadChestData(reader, version).ToList();
        var loadedSigns = World.LoadSignData(reader).ToList();
        var loadedEntities = World.LoadTileEntityData(reader, version).ToList();
        ModDataSerializer.LoadModPayload(reader, loadedChests, loadedEntities);

        // Verify mod chest
        loadedChests.Count.ShouldBe(2);
        var modChest = loadedChests.First(c => c.X == 10);
        modChest.Items[0].ModName.ShouldBe("CalamityMod");
        modChest.Items[0].ModItemData!.GetInt("damage").ShouldBe(9999);

        // Verify vanilla chest is still vanilla
        var plainChest = loadedChests.First(c => c.X == 20);
        plainChest.Items[0].NetId.ShouldBe(73);
        plainChest.Items[0].ModItemData.ShouldBeNull();

        // Verify entity
        loadedEntities.Count.ShouldBe(1);
        loadedEntities[0].ModItemData!.GetString("name").ShouldBe("ModFrame");
    }
}
