using System.Collections.ObjectModel;
using System.Linq;
using Shouldly;
using TEdit.Scripting.Api;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class TileEntityApiTests
{
    private readonly World _world;
    private readonly TileEntityApi _api;

    public TileEntityApiTests()
    {
        _world = TestWorldFactory.CreateWorldWithTileEntities();
        _api = new TileEntityApi(_world);
    }

    [Fact]
    public void Count_ReturnsTotalEntityCount()
    {
        _api.Count.ShouldBe(5);
    }

    [Fact]
    public void MannequinCount_ReturnsDisplayDollCount()
    {
        _api.MannequinCount.ShouldBe(1);
    }

    [Fact]
    public void WeaponRackCount_ReturnsCorrectCount()
    {
        _api.WeaponRackCount.ShouldBe(1);
    }

    [Fact]
    public void HatRackCount_ReturnsCorrectCount()
    {
        _api.HatRackCount.ShouldBe(1);
    }

    [Fact]
    public void ItemFrameCount_ReturnsCorrectCount()
    {
        _api.ItemFrameCount.ShouldBe(1);
    }

    [Fact]
    public void FoodPlatterCount_ReturnsCorrectCount()
    {
        _api.FoodPlatterCount.ShouldBe(1);
    }

    [Fact]
    public void GetAt_ReturnsMannequin()
    {
        var result = _api.GetAt(10, 50);
        result.ShouldNotBeNull();
        result!["type"].ShouldBe("DisplayDoll");
        result["pose"].ShouldBe(0);
    }

    [Fact]
    public void GetAt_ReturnsNullForEmpty()
    {
        _api.GetAt(0, 0).ShouldBeNull();
    }

    [Fact]
    public void GetAllMannequins_ReturnsOnlyDisplayDolls()
    {
        var mannequins = _api.GetAllMannequins();
        mannequins.Count.ShouldBe(1);
        mannequins[0]["type"].ShouldBe("DisplayDoll");
    }

    [Fact]
    public void GetAllByType_ReturnsFilteredEntities()
    {
        var racks = _api.GetAllByType("WeaponRack");
        racks.Count.ShouldBe(1);
    }

    // --- Mannequin Equipment ---

    [Fact]
    public void SetEquipment_SetsItemOnMannequin()
    {
        _api.SetEquipment(10, 50, 0, 123, 5);

        var entity = _world.GetTileEntityAtTile(10, 50);
        entity.ShouldNotBeNull();
        entity!.Items[0].Id.ShouldBe((short)123);
        entity.Items[0].Prefix.ShouldBe((byte)5);
        entity.Items[0].StackSize.ShouldBe((short)1);
    }

    [Fact]
    public void ClearEquipment_ClearsSlot()
    {
        _api.SetEquipment(10, 50, 0, 123);
        _api.ClearEquipment(10, 50, 0);

        var entity = _world.GetTileEntityAtTile(10, 50);
        entity!.Items[0].Id.ShouldBe((short)0);
    }

    [Fact]
    public void SetEquipment_ThrowsForInvalidSlot()
    {
        Should.Throw<System.ArgumentException>(() => _api.SetEquipment(10, 50, 99, 123));
    }

    [Fact]
    public void SetEquipment_ThrowsForWrongEntityType()
    {
        Should.Throw<System.ArgumentException>(() => _api.SetEquipment(20, 50, 0, 123));
    }

    // --- Mannequin Dyes ---

    [Fact]
    public void SetDye_SetsDyeOnMannequin()
    {
        _api.SetDye(10, 50, 0, 264);

        var entity = _world.GetTileEntityAtTile(10, 50);
        entity!.Dyes[0].Id.ShouldBe((short)264);
    }

    [Fact]
    public void ClearDye_ClearsDyeSlot()
    {
        _api.SetDye(10, 50, 0, 264);
        _api.ClearDye(10, 50, 0);

        var entity = _world.GetTileEntityAtTile(10, 50);
        entity!.Dyes[0].Id.ShouldBe((short)0);
    }

    // --- Mannequin Weapon ---

    [Fact]
    public void SetWeapon_SetsWeaponOnMannequin()
    {
        _api.SetWeapon(10, 50, 456, 2);

        var entity = _world.GetTileEntityAtTile(10, 50);
        entity!.Misc[0].Id.ShouldBe((short)456);
        entity.Misc[0].Prefix.ShouldBe((byte)2);
    }

    [Fact]
    public void ClearWeapon_ClearsWeaponSlot()
    {
        _api.SetWeapon(10, 50, 456);
        _api.ClearWeapon(10, 50);

        var entity = _world.GetTileEntityAtTile(10, 50);
        entity!.Misc[0].Id.ShouldBe((short)0);
    }

    // --- Mannequin Pose ---

    [Fact]
    public void SetPose_ChangesPose()
    {
        _api.SetPose(10, 50, 3);
        _api.GetPose(10, 50).ShouldBe(3);
    }

    [Fact]
    public void SetPose_ThrowsForInvalidPose()
    {
        Should.Throw<System.ArgumentException>(() => _api.SetPose(10, 50, 99));
    }

    // --- Hat Rack ---

    [Fact]
    public void SetHatRackItem_SetsItem()
    {
        _api.SetHatRackItem(30, 50, 0, 100);

        var entity = _world.GetTileEntityAtTile(30, 50);
        entity!.Items[0].Id.ShouldBe((short)100);
    }

    [Fact]
    public void ClearHatRackItem_ClearsSlot()
    {
        _api.SetHatRackItem(30, 50, 0, 100);
        _api.ClearHatRackItem(30, 50, 0);

        var entity = _world.GetTileEntityAtTile(30, 50);
        entity!.Items[0].Id.ShouldBe((short)0);
    }

    [Fact]
    public void SetHatRackDye_SetsDye()
    {
        _api.SetHatRackDye(30, 50, 0, 264);

        var entity = _world.GetTileEntityAtTile(30, 50);
        entity!.Dyes[0].Id.ShouldBe((short)264);
    }

    // --- Single-Item Entities (WeaponRack, ItemFrame, FoodPlatter) ---

    [Fact]
    public void SetItem_SetsWeaponRackItem()
    {
        _api.SetItem(20, 50, 789, 3, 1);

        var entity = _world.GetTileEntityAtTile(20, 50);
        entity!.NetId.ShouldBe(789);
        entity.Prefix.ShouldBe((byte)3);
    }

    [Fact]
    public void ClearItem_ClearsWeaponRackItem()
    {
        _api.SetItem(20, 50, 789);
        _api.ClearItem(20, 50);

        var entity = _world.GetTileEntityAtTile(20, 50);
        entity!.NetId.ShouldBe(0);
    }

    [Fact]
    public void SetItem_WorksForItemFrame()
    {
        _api.SetItem(40, 50, 500, 0, 1);

        var entity = _world.GetTileEntityAtTile(40, 50);
        entity!.NetId.ShouldBe(500);
    }

    [Fact]
    public void SetItem_WorksForFoodPlatter()
    {
        _api.SetItem(50, 50, 357, 0, 1);

        var entity = _world.GetTileEntityAtTile(50, 50);
        entity!.NetId.ShouldBe(357);
    }

    [Fact]
    public void SetItem_ThrowsForMannequin()
    {
        Should.Throw<System.InvalidOperationException>(() => _api.SetItem(10, 50, 123));
    }

    // --- FindByItem ---

    [Fact]
    public void FindByItem_FindsWeaponRackWithItem()
    {
        _api.SetItem(20, 50, 789);

        var results = _api.FindByItem(789);
        results.Count.ShouldBe(1);
        results[0]["type"].ShouldBe("WeaponRack");
    }

    [Fact]
    public void FindByItem_FindsMannequinEquipment()
    {
        _api.SetEquipment(10, 50, 0, 123);

        var results = _api.FindByItem(123);
        results.Count.ShouldBe(1);
        results[0]["type"].ShouldBe("DisplayDoll");
    }

    [Fact]
    public void FindByItem_NoResults()
    {
        _api.FindByItem(9999).Count.ShouldBe(0);
    }

    // --- GetAll ---

    [Fact]
    public void GetAll_ReturnsAllEntities()
    {
        var all = _api.GetAll();
        all.Count.ShouldBe(5);
    }
}
