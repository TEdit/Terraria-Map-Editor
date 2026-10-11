using System.Globalization;
using System.Text.Json;
using TEdit.Common.Serialization;
using TEdit.Geometry;

namespace TEdit.Terraria.Tests;

[Collection("SharedState")]
public class ContributionLookupTests
{
    [Fact]
    public void JsonVectors_UseJsonMinusEvenWithNonAsciiCultureSign()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NegativeSign = "\u2212";
            CultureInfo.CurrentCulture = culture;
            var options = new JsonSerializerOptions();
            options.Converters.Add(new Vector2Int32JsonConverter());
            var json = JsonSerializer.Serialize(new Vector2Int32(-5, 3), options);
            Assert.Equal("[-5,3]", json);
            Assert.Equal(-5, JsonSerializer.Deserialize<Vector2Int32>(json, options).X);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    public void DyeChoices_ContainExactlyDyeItemIds(string locale)
    {
        try
        {
            var store = TerrariaDataStore.Initialize(locale: locale);
            var expected = store.DyeColorById.Keys.Where(store.ItemById.ContainsKey).Order().ToArray();
            Assert.NotEmpty(expected);
            Assert.Equal(expected, store.DyeItems.Keys.Order().ToArray());
        }
        finally { WorldConfiguration.Initialize(); }
    }

    [Fact]
    public void ItemLookup_CoversAllLoadedItemsAndPreservesUnknownIds()
    {
        WorldConfiguration.Initialize();
        var items = WorldConfiguration.ItemProperties;
        Assert.NotEmpty(items);
        Assert.Equal(items.Count, items.Select(item => item.Id).Distinct().Count());
        foreach (var property in items)
        {
            Assert.Same(property, WorldConfiguration.ItemLookupTable[property.Id]);
            var item = new Item { NetId = property.Id };
            Assert.Equal(property.Id, item.NetId);
        }
        Assert.Equal(int.MaxValue, new Item { NetId = int.MaxValue }.NetId);
    }
}
