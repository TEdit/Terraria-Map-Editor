using System.Collections.ObjectModel;
using TEdit.Common.IO;
using TEdit.Editor.Clipboard;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Tests.Scripting;
using Xunit;

namespace TEdit.Tests.Clipboard;

// UndoManager uses a process-wide cache directory and filenames.
[CollectionDefinition("UndoManager replay", DisableParallelization = true)]
public sealed class UndoManagerReplayCollection;

[Collection("UndoManager replay")]
public sealed class EntityPasteUndoRegressionTests
{
    public enum EntityKind { Chest, Sign, ItemFrame, DisplayDoll, ModChest, ModItemFrame }

    public static IEnumerable<object[]> PasteCases()
    {
        foreach (var kind in Enum.GetValues<EntityKind>())
        foreach (bool shifted in new[] { false, true })
            yield return new object[] { kind, shifted };
    }

    [Theory]
    [MemberData(nameof(PasteCases))]
    public void PasteEntities_PreservesOriginalAndPastedDataAcrossRepeatedUndoRedo(EntityKind kind, bool shifted)
    {
        var world = TestWorldFactory.CreateSmallWorld();
        world.IsTModLoader = IsMod(kind);
        int offset = shifted ? 1 : 0;
        Place(world, kind, 10, 10, 1);
        Place(world, kind, 14 + offset, 14 + offset, 2);
        Place(world, kind, 40, 10, 3);
        Place(world, kind, 44, 14, 4);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, new RectangleInt32(10, 10, 8, 8));
        using var undo = new UndoManager(world, null, () => { });
        buffer.Paste(world, new Vector2Int32(40, 10), undo, new PasteOptions());

        for (int cycle = 0; cycle < 3; cycle++)
        {
            CheckPasted();
            undo.Undo();
            AssertEntity(world, kind, 40, 10, 3);
            AssertEntity(world, kind, 44, 14, 4);
            CheckSources();
            undo.Redo();
        }
        CheckPasted();

        void CheckSources()
        {
            AssertEntity(world, kind, 10, 10, 1);
            AssertEntity(world, kind, 14 + offset, 14 + offset, 2);
            Assert.Equal(4, EntityCount(world, kind));
        }
        void CheckPasted()
        {
            AssertEntity(world, kind, 40, 10, 1);
            AssertEntity(world, kind, 44 + offset, 14 + offset, 2);
            CheckSources();
        }
    }

    [Theory]
    [InlineData(EntityKind.Chest)]
    [InlineData(EntityKind.Sign)]
    [InlineData(EntityKind.ItemFrame)]
    [InlineData(EntityKind.DisplayDoll)]
    [InlineData(EntityKind.ModChest)]
    [InlineData(EntityKind.ModItemFrame)]
    public void PasteNextToEntity_DoesNotChangeNeighborOutsideSelection(EntityKind kind)
    {
        var world = TestWorldFactory.CreateSmallWorld();
        world.IsTModLoader = IsMod(kind);
        Place(world, kind, 10, 10, 1);
        Place(world, kind, 38, 10, 2);
        Place(world, kind, 40, 10, 3);
        int height = Height(kind);
        var buffer = ClipboardBuffer.GetSelectionBuffer(world, new RectangleInt32(10, 10, 2, height));
        using var undo = new UndoManager(world, null, () => { });
        buffer.Paste(world, new Vector2Int32(40, 10), undo, new PasteOptions());
        for (int cycle = 0; cycle < 3; cycle++)
        {
            Check(1);
            undo.Undo();
            Check(3);
            undo.Redo();
        }
        Check(1);

        void Check(int expected)
        {
            AssertEntity(world, kind, 40, 10, expected);
            AssertEntity(world, kind, 38, 10, 2);
            AssertEntity(world, kind, 10, 10, 1);
            Assert.Equal(3, EntityCount(world, kind));
        }
    }

    [Theory]
    [InlineData(EntityKind.ModChest, false, 0)]
    [InlineData(EntityKind.ModChest, true, 0)]
    [InlineData(EntityKind.ModChest, true, 1)]
    [InlineData(EntityKind.ModItemFrame, false, 0)]
    [InlineData(EntityKind.ModItemFrame, true, 0)]
    [InlineData(EntityKind.ModItemFrame, true, 1)]
    public void EraseVirtualModEntity_PreservesTileIdentityAndPayloadAcrossUndoRedo(
        EntityKind kind, bool hasNeighbor, int entityOffset)
    {
        var world = TestWorldFactory.CreateSmallWorld();
        world.IsTModLoader = true;
        Place(world, kind, 40, 10, 1);
        // Mod entity coordinates need not be the sprite's top-left corner.
        if (IsChest(kind))
        {
            world.Chests[0].X += entityOffset;
            world.Chests[0].Y += entityOffset;
        }
        else
        {
            world.TileEntities[0].PosX += (short)entityOffset;
            world.TileEntities[0].PosY += (short)entityOffset;
        }
        if (hasNeighbor)
            Place(world, kind, 38, 10, 2);
        ushort virtualType = (ushort)(WorldConfiguration.TileCount + 10);
        for (int x = hasNeighbor ? 38 : 40; x < 42; x++)
        for (int y = 10; y < 12; y++)
            world.Tiles[x, y].Type = virtualType;
        using var undo = new UndoManager(world, null, () => { });
        for (int x = 40; x < 42; x++)
        for (int y = 10; y < 12; y++)
        {
            undo.SaveTile(x, y, removeEntities: true);
            world.Tiles[x, y] = new Tile();
        }
        undo.SaveUndo();
        for (int cycle = 0; cycle < 3; cycle++)
        {
            Assert.Equal(hasNeighbor ? 1 : 0, EntityCount(world, kind));
            if (hasNeighbor)
                AssertEntity(world, kind, 38, 10, 2, virtualType);
            undo.Undo();
            AssertEntity(world, kind, 40, 10, 1, virtualType, entityOffset);
            Assert.Equal(hasNeighbor ? 2 : 1, EntityCount(world, kind));
            if (hasNeighbor)
                AssertEntity(world, kind, 38, 10, 2, virtualType);
            undo.Redo();
            for (int x = 40; x < 42; x++)
            for (int y = 10; y < 12; y++)
                Assert.False(world.Tiles[x, y].IsActive);
        }
        Assert.Equal(hasNeighbor ? 1 : 0, EntityCount(world, kind));
        if (hasNeighbor)
            AssertEntity(world, kind, 38, 10, 2, virtualType);
    }

    private static bool IsMod(EntityKind kind) => kind is EntityKind.ModChest or EntityKind.ModItemFrame;
    private static bool IsChest(EntityKind kind) => kind is EntityKind.Chest or EntityKind.ModChest;
    private static int Height(EntityKind kind) => kind == EntityKind.DisplayDoll ? 3 : 2;
    private static ushort TileId(EntityKind kind) => (ushort)(kind switch
    {
        EntityKind.Chest or EntityKind.ModChest => TileType.Chest,
        EntityKind.Sign => TileType.Sign,
        EntityKind.DisplayDoll => TileType.DisplayDoll,
        _ => TileType.ItemFrame,
    });
    private static int EntityCount(World world, EntityKind kind) => IsChest(kind) ? world.Chests.Count
        : kind == EntityKind.Sign ? world.Signs.Count : world.TileEntities.Count;

    private static TagCompound Payload(int value)
    {
        var data = new TagCompound();
        data.Set("marker", value);
        return data;
    }

    private static void Place(World world, EntityKind kind, int x, int y, int value)
    {
        for (int dx = 0; dx < 2; dx++)
        for (int dy = 0; dy < Height(kind); dy++)
            world.Tiles[x + dx, y + dy] = new Tile
            {
                IsActive = true, Type = TileId(kind), U = (short)(dx * 18), V = (short)(dy * 18),
            };
        if (IsChest(kind))
        {
            var chest = new Chest(x, y, $"Chest {value}");
            foreach (int slot in new[] { 0, 39 })
            {
                chest.Items[slot].NetId = 29;
                chest.Items[slot].StackSize = value + slot;
                chest.Items[slot].Prefix = (byte)value;
            }
            if (IsMod(kind))
            {
                chest.Items[0].ModName = "TestMod";
                chest.Items[0].ModItemName = "TestItem";
                chest.Items[0].ModItemData = Payload(value);
                chest.Items[0].ModGlobalData = new() { Payload(value + 10) };
            }
            world.Chests.Add(chest);
        }
        else if (kind == EntityKind.Sign)
            world.Signs.Add(new Sign(x, y, $"Sign {value}\nSecond line"));
        else
        {
            var entity = new TileEntity
            {
                Type = (byte)(kind == EntityKind.DisplayDoll ? TileEntityType.DisplayDoll : TileEntityType.ItemFrame),
                Id = world.TileEntities.Count, PosX = (short)x, PosY = (short)y,
                NetId = 29, StackSize = (short)value, Prefix = (byte)value,
            };
            if (kind == EntityKind.DisplayDoll)
            {
                entity.Items = new ObservableCollection<TileEntityItem>(Enumerable.Range(0, 8)
                    .Select(i => new TileEntityItem { Id = 29, StackSize = (short)(value + i), Prefix = (byte)value }));
                entity.Dyes = new ObservableCollection<TileEntityItem>(Enumerable.Range(0, 8)
                    .Select(i => new TileEntityItem { Id = 1007, StackSize = (short)(value + i), Prefix = (byte)value }));
            }
            if (kind == EntityKind.DisplayDoll)
            {
                // Version 279 saves eight slots; loading pads the newer ninth slot.
                entity.Items.Add(new TileEntityItem());
                entity.Dyes.Add(new TileEntityItem());
            }
            if (IsMod(kind))
            {
                entity.ModItemData = Payload(value);
                entity.ModGlobalData = new() { Payload(value + 10) };
            }
            world.TileEntities.Add(entity);
        }
    }

    private static void AssertEntity(World world, EntityKind kind, int x, int y, int value,
        ushort? tileType = null, int entityOffset = 0)
    {
        for (int dx = 0; dx < 2; dx++)
        for (int dy = 0; dy < Height(kind); dy++)
        {
            var tile = world.Tiles[x + dx, y + dy];
            Assert.True(tile.IsActive);
            Assert.Equal(tileType ?? TileId(kind), tile.Type);
            Assert.Equal((short)(dx * 18), tile.U);
            Assert.Equal((short)(dy * 18), tile.V);
        }
        x += entityOffset;
        y += entityOffset;
        if (IsChest(kind))
        {
            var chest = Assert.Single(world.Chests, c => c.X == x && c.Y == y);
            Assert.Equal($"Chest {value}", chest.Name);
            foreach (int slot in new[] { 0, 39 })
            {
                Assert.Equal(29, chest.Items[slot].NetId);
                Assert.Equal(value + slot, chest.Items[slot].StackSize);
                Assert.Equal((byte)value, chest.Items[slot].Prefix);
            }
            if (IsMod(kind))
            {
                Assert.Equal("TestMod", chest.Items[0].ModName);
                Assert.Equal("TestItem", chest.Items[0].ModItemName);
                Assert.Equal(value, chest.Items[0].ModItemData.GetInt("marker"));
                Assert.Equal(value + 10, Assert.Single(chest.Items[0].ModGlobalData).GetInt("marker"));
            }
        }
        else if (kind == EntityKind.Sign)
            Assert.Equal($"Sign {value}\nSecond line", Assert.Single(world.Signs, s => s.X == x && s.Y == y).Text);
        else
        {
            var entity = Assert.Single(world.TileEntities, e => e.PosX == x && e.PosY == y);
            Assert.Equal((byte)(kind == EntityKind.DisplayDoll ? TileEntityType.DisplayDoll : TileEntityType.ItemFrame), entity.Type);
            if (kind == EntityKind.DisplayDoll)
            {
                Assert.Equal(9, entity.Items.Count);
                Assert.Equal(9, entity.Dyes.Count);
                for (int i = 0; i < 8; i++)
                {
                    Assert.Equal(29, entity.Items[i].Id);
                    Assert.Equal(1007, entity.Dyes[i].Id);
                    Assert.Equal(value + i, entity.Items[i].StackSize);
                    Assert.Equal(value + i, entity.Dyes[i].StackSize);
                    Assert.Equal((byte)value, entity.Items[i].Prefix);
                    Assert.Equal((byte)value, entity.Dyes[i].Prefix);
                }
            }
            else
            {
                Assert.Equal(29, entity.NetId);
                Assert.Equal(value, entity.StackSize);
                Assert.Equal((byte)value, entity.Prefix);
            }
            if (IsMod(kind))
            {
                Assert.Equal(value, entity.ModItemData.GetInt("marker"));
                Assert.Equal(value + 10, Assert.Single(entity.ModGlobalData).GetInt("marker"));
            }
        }
    }
}
