using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEdit.Common.IO;
using Xunit;
using Shouldly;

namespace TEdit.Terraria.Tests.TModLoader;

/// <summary>
/// Tests that ModDataSerializer correctly round-trips mod NBT data
/// for chests and tile entities through binary streams.
/// </summary>
public class ModDataSerializerTests
{
    private static TagCompound CreateTestModData()
    {
        var data = new TagCompound();
        data.Set("mod", "CalamityMod");
        data.Set("name", "Murasama");
        data.Set("damage", 9999);

        var nested = new TagCompound();
        nested.Set("enchant", "fire");
        data.Set("extra", nested);

        return data;
    }

    private static List<TagCompound> CreateTestGlobalData()
    {
        var g1 = new TagCompound();
        g1.Set("mod", "CalamityMod");
        g1.Set("rogue", (byte)1);

        var g2 = new TagCompound();
        g2.Set("mod", "ThoriumMod");
        g2.Set("bardLevel", 5);

        return new List<TagCompound> { g1, g2 };
    }

    [Fact]
    public void RoundTrip_ChestModData()
    {
        var chest = new Chest(10, 20);
        chest.MaxItems = Chest.LegacyMaxItems;
        chest.Items[0] = new Item(1, 100)
        {
            ModName = "CalamityMod",
            ModItemName = "Murasama",
            ModItemData = CreateTestModData(),
            ModGlobalData = CreateTestGlobalData(),
        };
        chest.Items[1] = new Item(2, 50)
        {
            ModName = "ThoriumMod",
            ModItemName = "BardWeapon",
            ModPrefixMod = "ThoriumMod",
            ModPrefixName = "Melodic",
        };

        var chests = new List<Chest> { chest };
        var entities = new List<TileEntity>();

        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            ModDataSerializer.SaveModPayload(bw, chests, entities);
        }

        // Clear mod data from chests to simulate vanilla-only load
        chest.Items[0].ModName = null;
        chest.Items[0].ModItemName = null;
        chest.Items[0].ModItemData = null;
        chest.Items[0].ModGlobalData = null;
        chest.Items[1].ModName = null;
        chest.Items[1].ModItemName = null;
        chest.Items[1].ModPrefixMod = null;
        chest.Items[1].ModPrefixName = null;

        ms.Position = 0;
        using var br = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true);
        var result = ModDataSerializer.LoadModPayload(br, chests, entities);

        result.ShouldBeTrue();
        chest.Items[0].ModName.ShouldBe("CalamityMod");
        chest.Items[0].ModItemName.ShouldBe("Murasama");
        chest.Items[0].ModItemData.GetInt("damage").ShouldBe(9999);
        chest.Items[0].ModItemData.GetCompound("extra").GetString("enchant").ShouldBe("fire");
        chest.Items[0].ModGlobalData.Count.ShouldBe(2);
        chest.Items[0].ModGlobalData[0].GetBool("rogue").ShouldBeTrue();

        chest.Items[1].ModName.ShouldBe("ThoriumMod");
        chest.Items[1].ModItemName.ShouldBe("BardWeapon");
        chest.Items[1].ModPrefixMod.ShouldBe("ThoriumMod");
        chest.Items[1].ModPrefixName.ShouldBe("Melodic");
    }

    [Fact]
    public void RoundTrip_TileEntityModData()
    {
        var entity = new TileEntity
        {
            Type = 1,
            PosX = 100,
            PosY = 200,
            ModName = "CalamityMod",
            ModItemName = "CalamityFrame",
            ModItemData = CreateTestModData(),
            ModGlobalData = CreateTestGlobalData(),
        };

        var chests = new List<Chest>();
        var entities = new List<TileEntity> { entity };

        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            ModDataSerializer.SaveModPayload(bw, chests, entities);
        }

        // Clear mod data
        entity.ModName = null;
        entity.ModItemName = null;
        entity.ModItemData = null;
        entity.ModGlobalData = null;

        ms.Position = 0;
        using var br = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true);
        ModDataSerializer.LoadModPayload(br, chests, entities);

        entity.ModName.ShouldBe("CalamityMod");
        entity.ModItemName.ShouldBe("CalamityFrame");
        entity.ModItemData.GetInt("damage").ShouldBe(9999);
        entity.ModGlobalData.Count.ShouldBe(2);
    }

    [Fact]
    public void RoundTrip_TileEntityWithModItems()
    {
        var entity = new TileEntity { Type = 1 };
        entity.Items.Add(new TileEntityItem
        {
            ModItemData = CreateTestModData(),
            ModGlobalData = CreateTestGlobalData(),
        });

        var chests = new List<Chest>();
        var entities = new List<TileEntity> { entity };

        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            ModDataSerializer.SaveModPayload(bw, chests, entities);
        }

        // Clear mod data
        entity.Items[0].ModItemData = null;
        entity.Items[0].ModGlobalData = null;

        ms.Position = 0;
        using var br = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true);
        ModDataSerializer.LoadModPayload(br, chests, entities);

        entity.Items[0].ModItemData.GetInt("damage").ShouldBe(9999);
        entity.Items[0].ModGlobalData.Count.ShouldBe(2);
    }

    [Fact]
    public void NoModData_WritesNothing()
    {
        var chest = new Chest(0, 0);
        chest.MaxItems = Chest.LegacyMaxItems;
        var entity = new TileEntity { Type = 1 };

        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            ModDataSerializer.SaveModPayload(bw, new List<Chest> { chest }, new List<TileEntity> { entity });
        }

        // No mod data means nothing written
        ms.Length.ShouldBe(0);
    }

    [Fact]
    public void NoMagicMarker_ReturnsFalse()
    {
        using var ms = new MemoryStream();
        ms.WriteByte(0x00);
        ms.WriteByte(0x01);
        ms.WriteByte(0x02);
        ms.WriteByte(0x03);
        ms.Position = 0;

        using var br = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true);
        var result = ModDataSerializer.LoadModPayload(br, new List<Chest>(), new List<TileEntity>());

        result.ShouldBeFalse();
        // Stream position should be restored
        br.BaseStream.Position.ShouldBe(0);
    }

    [Fact]
    public void EmptyStream_ReturnsFalse()
    {
        using var ms = new MemoryStream();
        using var br = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true);

        var result = ModDataSerializer.LoadModPayload(br, new List<Chest>(), new List<TileEntity>());
        result.ShouldBeFalse();
    }

    [Fact]
    public void RoundTrip_MixedChestsAndEntities()
    {
        // Chest with one mod item and one vanilla item
        var chest = new Chest(5, 10);
        chest.MaxItems = Chest.LegacyMaxItems;
        chest.Items[0] = new Item(1, 1); // vanilla
        chest.Items[1] = new Item(2, 1)
        {
            ModName = "ExampleMod",
            ModItemName = "ExampleSword",
            ModItemData = CreateTestModData(),
        };

        // Entity with mod data
        var entity1 = new TileEntity
        {
            Type = 1,
            ModItemData = CreateTestModData(),
        };
        // Entity without mod data
        var entity2 = new TileEntity { Type = 2 };

        var chests = new List<Chest> { chest };
        var entities = new List<TileEntity> { entity1, entity2 };

        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            ModDataSerializer.SaveModPayload(bw, chests, entities);
        }

        // Clear
        chest.Items[1].ModName = null;
        chest.Items[1].ModItemName = null;
        chest.Items[1].ModItemData = null;
        entity1.ModItemData = null;

        ms.Position = 0;
        using var br = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true);
        ModDataSerializer.LoadModPayload(br, chests, entities);

        // Vanilla item unchanged
        chest.Items[0].ModName.ShouldBeNull();
        // Mod item restored
        chest.Items[1].ModName.ShouldBe("ExampleMod");
        chest.Items[1].ModItemData.GetInt("damage").ShouldBe(9999);
        // Entity mod data restored
        entity1.ModItemData.GetInt("damage").ShouldBe(9999);
        // Non-mod entity unaffected
        entity2.ModItemData.ShouldBeNull();
    }

    [Fact]
    public void BuildPayload_DeepCopiesData()
    {
        var chest = new Chest(0, 0);
        chest.MaxItems = Chest.LegacyMaxItems;
        chest.Items[0] = new Item(1, 1)
        {
            ModName = "TestMod",
            ModItemName = "TestItem",
            ModItemData = CreateTestModData(),
        };

        var payload = ModDataSerializer.BuildPayload(
            new List<Chest> { chest },
            new List<TileEntity>());

        payload.ShouldNotBeNull();

        // Mutate original — payload should be unaffected
        chest.Items[0].ModItemData.Set("damage", 0);

        var chestMods = payload.GetList<TagCompound>("chestMods");
        var items = chestMods[0].GetList<TagCompound>("items");
        items[0].GetCompound("id").GetInt("damage").ShouldBe(9999);
    }

    [Fact]
    public void BackwardCompat_OldFormatWithTrailingData()
    {
        // Simulate reading an old undo file that has some trailing data
        // but no TNBT magic — LoadModPayload should return false and
        // not advance the stream past the non-matching bytes.
        using var ms = new MemoryStream();
        // Write some random trailing data (not TNBT magic)
        var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true);
        bw.Write(42); // random int
        bw.Write("hello"); // random string
        bw.Flush();

        ms.Position = 0;
        using var br = new BinaryReader(ms, System.Text.Encoding.UTF8, leaveOpen: true);
        var result = ModDataSerializer.LoadModPayload(br, new List<Chest>(), new List<TileEntity>());

        result.ShouldBeFalse();
        br.BaseStream.Position.ShouldBe(0); // position restored
    }
}
