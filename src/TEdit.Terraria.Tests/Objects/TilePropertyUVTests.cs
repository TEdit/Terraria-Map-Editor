using System.Text.Json;
using Shouldly;
using TEdit.Common.Serialization;
using TEdit.Geometry;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Tests.Objects;

/// <summary>
/// Tests for TextureWrap and UV coordinate conversion in TileProperty.
/// </summary>
[Collection("SharedState")]
public class TilePropertyUVTests : IDisposable
{
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    public TilePropertyUVTests()
    {
        WorldConfiguration.Reset();
        TerrariaDataStore.Reset();
        WorldConfiguration.Initialize();
    }

    public void Dispose()
    {
        WorldConfiguration.Reset();
        TerrariaDataStore.Reset();
    }

    #region TextureWrap Serialization Tests

    [Fact]
    public void TextureWrap_Serialize_UAxisWithOffset()
    {
        var wrap = new TextureWrap
        {
            Axis = TextureWrapAxis.U,
            OffsetIncrement = 36
        };

        var json = JsonSerializer.Serialize(wrap, Options);

        json.ShouldContain("\"axis\": \"U\"");
        json.ShouldContain("\"offsetIncrement\": 36");
        json.ShouldNotContain("conditionalV"); // null is omitted
        json.ShouldNotContain("wrapThreshold"); // JsonIgnore
    }

    [Fact]
    public void TextureWrap_Serialize_VAxisWithOffset()
    {
        var wrap = new TextureWrap
        {
            Axis = TextureWrapAxis.V,
            OffsetIncrement = 36
        };

        var json = JsonSerializer.Serialize(wrap, Options);

        json.ShouldContain("\"axis\": \"V\"");
    }

    [Fact]
    public void TextureWrap_Serialize_WithConditionalV()
    {
        var wrap = new TextureWrap
        {
            Axis = TextureWrapAxis.U,
            OffsetIncrement = 18,
            ConditionalV = 18
        };

        var json = JsonSerializer.Serialize(wrap, Options);

        json.ShouldContain("\"conditionalV\": 18");
    }

    [Fact]
    public void TextureWrap_RoundTrip_PreservesAllProperties()
    {
        var original = new TextureWrap
        {
            Axis = TextureWrapAxis.U,
            OffsetIncrement = 72,
            ConditionalV = 18
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<TextureWrap>(json, Options)!;

        restored.Axis.ShouldBe(TextureWrapAxis.U);
        restored.OffsetIncrement.ShouldBe((short)72);
        restored.ConditionalV.ShouldBe((short)18);
        restored.WrapThreshold.ShouldBe(0); // Not serialized, default value
    }

    [Fact]
    public void TileProperty_WithTextureWrap_RoundTrip()
    {
        var original = new TileProperty
        {
            Id = 87,
            Name = "Pianos",
            IsFramed = true,
            TextureWrap = new TextureWrap
            {
                Axis = TextureWrapAxis.U,
                OffsetIncrement = 36
            }
        };

        var json = JsonSerializer.Serialize(original, Options);
        var restored = JsonSerializer.Deserialize<TileProperty>(json, Options)!;

        restored.TextureWrap.ShouldNotBeNull();
        restored.TextureWrap!.Axis.ShouldBe(TextureWrapAxis.U);
        restored.TextureWrap.OffsetIncrement.ShouldBe((short)36);
    }

    #endregion

    #region Tile Configuration Tests

    [Theory]
    [InlineData(87, TextureWrapAxis.U, 36, null)]   // Pianos
    [InlineData(88, TextureWrapAxis.U, 36, null)]   // Dressers
    [InlineData(89, TextureWrapAxis.U, 36, null)]   // Benches/Sofas
    [InlineData(93, TextureWrapAxis.V, 36, null)]   // Lamps (V-axis)
    [InlineData(101, TextureWrapAxis.U, 72, null)]  // Bookcases
    [InlineData(185, TextureWrapAxis.U, 18, (short)18)]  // Small decorations (conditional)
    [InlineData(187, TextureWrapAxis.U, 36, null)]  // Large decorations
    public void TileProperties_HasCorrectTextureWrap(int tileId, TextureWrapAxis expectedAxis, short expectedOffset, short? expectedConditionalV)
    {
        var tile = WorldConfiguration.TileProperties[tileId];

        tile.TextureWrap.ShouldNotBeNull();
        tile.TextureWrap!.Axis.ShouldBe(expectedAxis);
        tile.TextureWrap.OffsetIncrement.ShouldBe(expectedOffset);

        if (expectedConditionalV.HasValue)
            tile.TextureWrap.ConditionalV.ShouldBe(expectedConditionalV.Value);
        else
            tile.TextureWrap.ConditionalV.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]   // Dirt Block
    [InlineData(1)]   // Stone Block
    [InlineData(4)]   // Torch
    [InlineData(10)]  // Wood
    public void TileProperties_NonWrappedTiles_HaveNoTextureWrap(int tileId)
    {
        var tile = WorldConfiguration.TileProperties[tileId];

        tile.TextureWrap.ShouldBeNull();
    }

    #endregion

    #region GetRenderUV/GetWorldUV Roundtrip Tests

    [Fact]
    public void GetRenderUV_NonWrappedTile_ReturnsUnchanged()
    {
        // Tile 4 (Torch) has no texture wrap
        var result = TileProperty.GetRenderUV(4, 100, 50);

        result.X.ShouldBe((short)100);
        result.Y.ShouldBe((short)50);
    }

    [Fact]
    public void GetWorldUV_NonWrappedTile_ReturnsUnchanged()
    {
        // Tile 4 (Torch) has no texture wrap
        var result = TileProperty.GetWorldUV(4, 100, 50);

        result.X.ShouldBe((short)100);
        result.Y.ShouldBe((short)50);
    }

    [Fact]
    public void GetRenderUV_WrapThresholdZero_ReturnsUnchanged()
    {
        // Before textures are loaded, WrapThreshold is 0
        // This test verifies graceful handling
        var tile = WorldConfiguration.TileProperties[87];
        tile.TextureWrap.ShouldNotBeNull();

        // If WrapThreshold is 0, UV should be unchanged
        if (tile.TextureWrap!.WrapThreshold == 0)
        {
            var result = TileProperty.GetRenderUV(87, 2000, 0);
            result.X.ShouldBe((short)2000);
        }
    }

    [Theory]
    [InlineData(87, 0, 0)]       // Piano at origin
    [InlineData(87, 54, 0)]      // Piano 2nd variant
    [InlineData(93, 0, 0)]       // Lamp at origin
    [InlineData(93, 18, 0)]      // Lamp 2nd variant
    [InlineData(101, 0, 0)]      // Bookcase at origin
    [InlineData(187, 54, 0)]     // Large deco 2nd variant
    public void GetRenderUV_SmallUV_ReturnsUnchanged(ushort tileId, short u, short v)
    {
        // Small UV values (within first texture row) should be unchanged
        var result = TileProperty.GetRenderUV(tileId, u, v);

        result.X.ShouldBe(u);
        result.Y.ShouldBe(v);
    }

    #endregion

    #region JSON Deserialization Tests

    [Fact]
    public void TileProperty_DeserializesTextureWrap()
    {
        var json = """
        {
            "id": 999,
            "name": "Test Tile",
            "textureWrap": {
                "axis": "U",
                "offsetIncrement": 36,
                "conditionalV": 18
            }
        }
        """;

        var tileProperty = JsonSerializer.Deserialize<TileProperty>(json, Options)!;

        tileProperty.TextureWrap.ShouldNotBeNull();
        tileProperty.TextureWrap!.Axis.ShouldBe(TextureWrapAxis.U);
        tileProperty.TextureWrap.OffsetIncrement.ShouldBe((short)36);
        tileProperty.TextureWrap.ConditionalV.ShouldBe((short)18);
    }

    [Fact]
    public void TileProperty_DeserializesWithoutTextureWrap()
    {
        var json = """
        {
            "id": 999,
            "name": "Test Tile"
        }
        """;

        var tileProperty = JsonSerializer.Deserialize<TileProperty>(json, Options)!;

        tileProperty.TextureWrap.ShouldBeNull();
    }

    #endregion
}
