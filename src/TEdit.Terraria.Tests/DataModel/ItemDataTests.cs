using System.Text.Json;
using Shouldly;
using TEdit.Common.Serialization;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests.DataModel;

public class ItemPropertyTests
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    [Fact]
    public void RoundTrip_FullItem_PreservesAllProperties()
    {
        var original = new ItemProperty
        {
            Id = 1,
            Name = "Iron Pickaxe",
            Scale = 1.0f,
            MaxStackSize = 1,
            IsFood = false,
            IsKite = false,
            IsCritter = false,
            IsAccessory = false,
            IsRackable = true,
            IsMount = false,
            Head = null,
            Body = null,
            Legs = null,
            Tally = 0,
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<ItemProperty>(json, Options)!;

        restored.Id.ShouldBe(1);
        restored.Name.ShouldBe("Iron Pickaxe");
        restored.IsRackable.ShouldBe(true);
        restored.Head.ShouldBeNull();
    }

    [Fact]
    public void RoundTrip_ArmorItem_PreservesSlots()
    {
        var original = new ItemProperty
        {
            Id = 100,
            Name = "Iron Helmet",
            Head = 5,
            Body = null,
            Legs = null,
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<ItemProperty>(json, Options)!;

        restored.Head.ShouldBe(5);
        restored.Body.ShouldBeNull();
        restored.Legs.ShouldBeNull();
    }

    [Fact]
    public void CategoryFlags_SerializeCorrectly()
    {
        var item = new ItemProperty
        {
            Id = 200,
            Name = "Test Food",
            IsFood = true,
            IsCritter = true,
        };

        var json = JsonSerializer.Serialize(item, Options);

        json.ShouldContain("\"isFood\": true");
        json.ShouldContain("\"isCritter\": true");
    }

    [Fact]
    public void Defaults_AreCorrect()
    {
        var item = new ItemProperty();
        item.Scale.ShouldBe(1f);
        item.Head.ShouldBeNull();
        item.Body.ShouldBeNull();
        item.Legs.ShouldBeNull();
    }
}
