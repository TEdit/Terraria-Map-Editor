using Shouldly;
using TEdit.Scripting.Api;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class ChestApiTests
{
    private readonly World _world;
    private readonly ChestApi _api;

    public ChestApiTests()
    {
        _world = TestWorldFactory.CreateWorldWithChestsAndSigns();
        _api = new ChestApi(_world);
    }

    [Fact]
    public void Count_ReturnsChestCount()
    {
        _api.Count.ShouldBe(2);
    }

    [Fact]
    public void GetAll_ReturnsAllChests()
    {
        var chests = _api.GetAll();
        chests.Count.ShouldBe(2);
        chests[0]["name"].ShouldBe("Gold Chest");
        chests[1]["name"].ShouldBe("Wooden Chest");
    }

    [Fact]
    public void GetAt_ReturnsChestAtPosition()
    {
        var chest = _api.GetAt(10, 40);
        chest.ShouldNotBeNull();
        chest!["name"].ShouldBe("Gold Chest");
    }

    [Fact]
    public void GetAt_ReturnsNullForNoChest()
    {
        var chest = _api.GetAt(0, 0);
        chest.ShouldBeNull();
    }

    [Fact]
    public void FindByItem_FindsChestsWithItem()
    {
        var results = _api.FindByItem(73); // Gold Coin
        results.Count.ShouldBe(2); // Both chests have gold coins
    }

    [Fact]
    public void FindByItem_NoResults()
    {
        var results = _api.FindByItem(9999);
        results.Count.ShouldBe(0);
    }

    [Fact]
    public void SetItem_ModifiesChestItem()
    {
        _api.SetItem(10, 40, 0, 100, 5, 0);

        var chest = _world.GetChestAtTile(10, 40);
        chest.ShouldNotBeNull();
        chest!.Items[0].NetId.ShouldBe(100);
        chest.Items[0].StackSize.ShouldBe(5);
    }

    [Fact]
    public void ClearItem_RemovesItem()
    {
        _api.ClearItem(10, 40, 0);

        var chest = _world.GetChestAtTile(10, 40);
        chest!.Items[0].NetId.ShouldBe(0);
        chest.Items[0].StackSize.ShouldBe(0);
    }

    [Fact]
    public void AddItem_AddsToFirstEmptySlot()
    {
        var result = _api.AddItem(10, 40, 200, 10, 0);
        result.ShouldBeTrue();

        var chest = _world.GetChestAtTile(10, 40);
        chest!.Items[2].NetId.ShouldBe(200); // Slot 0 and 1 are taken
        chest.Items[2].StackSize.ShouldBe(10);
    }

    [Fact]
    public void GetAt_ItemsArrayIncludesAllSlots()
    {
        var chest = _api.GetAt(10, 40);
        chest.ShouldNotBeNull();

        var items = (List<Dictionary<string, object>>)chest!["items"];
        items.Count.ShouldBe(40); // All 40 slots present
    }

    [Fact]
    public void GetAt_EmptySlotsShowEmptyName()
    {
        var chest = _api.GetAt(10, 40);
        var items = (List<Dictionary<string, object>>)chest!["items"];

        // Slot 0 has Life Crystal
        ((int)items[0]["stack"]).ShouldBeGreaterThan(0);

        // Slot 2 is empty
        items[2]["name"].ShouldBe("[empty]");
        items[2]["stack"].ShouldBe(0);
        items[2]["slot"].ShouldBe(2);
    }
}
