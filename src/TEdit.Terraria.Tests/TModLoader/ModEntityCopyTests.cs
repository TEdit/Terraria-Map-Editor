using System.Collections.Generic;
using System.Linq;
using TEdit.Common.IO;
using Xunit;
using Shouldly;

namespace TEdit.Terraria.Tests.TModLoader;

/// <summary>
/// Tests that Copy() on Item, TileEntityItem, and TileEntity
/// deep-copies ModItemData and ModGlobalData (no shared references).
/// </summary>
public class ModEntityCopyTests
{
    private static TagCompound CreateTestModItemData()
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
    public void Item_Copy_DeepCopiesModItemData()
    {
        var original = new Item(1, 100)
        {
            ModName = "CalamityMod",
            ModItemName = "Murasama",
            ModItemData = CreateTestModItemData(),
            ModGlobalData = CreateTestGlobalData(),
        };

        var copy = original.Copy();

        // Values match
        copy.ModItemData.GetString("name").ShouldBe("Murasama");
        copy.ModItemData.GetInt("damage").ShouldBe(9999);
        copy.ModGlobalData.Count.ShouldBe(2);

        // Mutate copy — original unchanged
        copy.ModItemData.Set("damage", 1);
        copy.ModItemData.GetCompound("extra").Set("enchant", "ice");
        copy.ModGlobalData[0].Set("rogue", (byte)0);

        original.ModItemData.GetInt("damage").ShouldBe(9999);
        original.ModItemData.GetCompound("extra").GetString("enchant").ShouldBe("fire");
        original.ModGlobalData[0].GetBool("rogue").ShouldBeTrue();
    }

    [Fact]
    public void Item_Copy_NullModData_Works()
    {
        var original = new Item(1, 100);
        original.ModItemData.ShouldBeNull();
        original.ModGlobalData.ShouldBeNull();

        var copy = original.Copy();
        copy.ModItemData.ShouldBeNull();
        copy.ModGlobalData.ShouldBeNull();
    }

    [Fact]
    public void TileEntityItem_Copy_DeepCopiesModItemData()
    {
        var original = new TileEntityItem
        {
            ModItemData = CreateTestModItemData(),
            ModGlobalData = CreateTestGlobalData(),
        };

        var copy = original.Copy();

        copy.ModItemData.GetString("name").ShouldBe("Murasama");
        copy.ModGlobalData.Count.ShouldBe(2);

        // Mutate copy — original unchanged
        copy.ModItemData.Set("name", "Zenith");
        copy.ModGlobalData[1].Set("bardLevel", 99);

        original.ModItemData.GetString("name").ShouldBe("Murasama");
        original.ModGlobalData[1].GetInt("bardLevel").ShouldBe(5);
    }

    [Fact]
    public void TileEntity_Copy_DeepCopiesModFields()
    {
        var original = new TileEntity
        {
            Type = 1, // ItemFrame
            PosX = 100,
            PosY = 200,
            ModItemData = CreateTestModItemData(),
            ModGlobalData = CreateTestGlobalData(),
        };

        var copy = original.Copy();

        // Position and type preserved
        copy.Type.ShouldBe((byte)1);
        copy.PosX.ShouldBe((short)100);
        copy.PosY.ShouldBe((short)200);

        // Mod data values match
        copy.ModItemData.GetString("name").ShouldBe("Murasama");
        copy.ModGlobalData.Count.ShouldBe(2);

        // Mutate copy — original unchanged
        copy.ModItemData.Set("damage", 0);
        copy.ModGlobalData.Clear();

        original.ModItemData.GetInt("damage").ShouldBe(9999);
        original.ModGlobalData.Count.ShouldBe(2);
    }

    [Fact]
    public void TileEntity_Copy_WithModItems_DeepCopies()
    {
        var original = new TileEntity { Type = 1 };
        original.Items.Add(new TileEntityItem
        {
            ModItemData = CreateTestModItemData(),
        });

        var copy = original.Copy();

        copy.Items.Count.ShouldBe(1);
        copy.Items[0].ModItemData.GetString("name").ShouldBe("Murasama");

        // Mutate copy's item — original unchanged
        copy.Items[0].ModItemData.Set("name", "Zenith");
        original.Items[0].ModItemData.GetString("name").ShouldBe("Murasama");
    }

    [Fact]
    public void Chest_Copy_DeepCopiesModItems()
    {
        var original = new Chest(10, 20);
        original.MaxItems = Chest.LegacyMaxItems;
        original.Items[0] = new Item(1, 100)
        {
            ModName = "CalamityMod",
            ModItemName = "Murasama",
            ModItemData = CreateTestModItemData(),
            ModGlobalData = CreateTestGlobalData(),
        };

        var copy = original.Copy();

        copy.Items[0].ModItemData.GetString("name").ShouldBe("Murasama");
        copy.Items[0].ModGlobalData.Count.ShouldBe(2);

        // Mutate copy — original unchanged
        copy.Items[0].ModItemData.Set("damage", 0);
        copy.Items[0].ModGlobalData[0].Set("rogue", (byte)0);

        original.Items[0].ModItemData.GetInt("damage").ShouldBe(9999);
        original.Items[0].ModGlobalData[0].GetBool("rogue").ShouldBeTrue();
    }
}
