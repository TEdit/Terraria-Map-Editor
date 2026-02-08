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
            Head = -1,
            Body = -1,
            Legs = -1,
            Tally = 0,
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<ItemProperty>(json, Options)!;

        restored.Id.ShouldBe(1);
        restored.Name.ShouldBe("Iron Pickaxe");
        restored.IsRackable.ShouldBe(true);
        restored.Head.ShouldBe(-1);
    }

    [Fact]
    public void RoundTrip_ArmorItem_PreservesSlots()
    {
        var original = new ItemProperty
        {
            Id = 100,
            Name = "Iron Helmet",
            Head = 5,
            Body = -1,
            Legs = -1,
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<ItemProperty>(json, Options)!;

        restored.Head.ShouldBe(5);
        restored.Body.ShouldBe(-1);
        restored.Legs.ShouldBe(-1);
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
        item.Head.ShouldBe(-1);
        item.Body.ShouldBe(-1);
        item.Legs.ShouldBe(-1);
    }
}
