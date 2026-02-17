using Shouldly;
using TEdit.Terraria.Player;

namespace TEdit.Terraria.Tests.Player;

public class PlayerItemTests
{
    [Fact]
    public void DefaultConstructor_CreatesEmptyItem()
    {
        var item = new PlayerItem();
        item.NetId.ShouldBe(0);
        item.StackSize.ShouldBe(0);
        item.Prefix.ShouldBe((byte)0);
        item.Favorited.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_WithValues_SetsProperties()
    {
        var item = new PlayerItem(100, 50, 5, true);
        item.NetId.ShouldBe(100);
        item.StackSize.ShouldBe(50);
        item.Prefix.ShouldBe((byte)5);
        item.Favorited.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_ZeroStack_SetsNetIdToZero()
    {
        var item = new PlayerItem(100, 0);
        item.NetId.ShouldBe(0);
        item.StackSize.ShouldBe(0);
    }

    [Fact]
    public void Copy_CreatesIdenticalItem()
    {
        var original = new PlayerItem(100, 50, 5, true);
        var copy = original.Copy();
        copy.NetId.ShouldBe(original.NetId);
        copy.StackSize.ShouldBe(original.StackSize);
        copy.Prefix.ShouldBe(original.Prefix);
        copy.Favorited.ShouldBe(original.Favorited);
    }

    [Fact]
    public void Copy_IsIndependentOfOriginal()
    {
        var original = new PlayerItem(100, 50, 5, true);
        var copy = original.Copy();
        copy.Favorited = false;
        original.Favorited.ShouldBeTrue();
    }

    [Fact]
    public void ToItem_ConvertsCorrectly()
    {
        var playerItem = new PlayerItem(100, 50, 5);
        var item = playerItem.ToItem();
        item.NetId.ShouldBe(100);
        item.StackSize.ShouldBe(50);
        item.Prefix.ShouldBe((byte)5);
    }

    [Fact]
    public void FromItem_ConvertsCorrectly()
    {
        var item = new Item(50, 100, 5);
        var playerItem = PlayerItem.FromItem(item);
        playerItem.NetId.ShouldBe(100);
        playerItem.StackSize.ShouldBe(50);
        playerItem.Prefix.ShouldBe((byte)5);
    }

    [Fact]
    public void StackSize_ClampsNegativeToZero()
    {
        var item = new PlayerItem();
        item.StackSize = -1;
        item.StackSize.ShouldBe(0);
    }
}
