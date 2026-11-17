using System.Text.Json.Serialization;

namespace TEdit.Terraria.Objects;

/// <summary>
/// One entry of a Christmas tree decoration list. <see cref="Value"/> is the
/// bit-field value packed into the tree tile's V coordinate, not an item ID.
/// </summary>
public class XmasDecoProperty
{
    /// <summary>Decoration slot value stored in <c>Tile.V</c> (0 = none).</summary>
    public int Value { get; set; }

    /// <summary>Item ID of the decoration, or 0 for "none".</summary>
    public int ItemId { get; set; }

    public ItemProperty? Item { get; set; }

    /// <summary>Stable English identity name.</summary>
    public string Name => Item?.Name ?? "None";

    private string? _localizedName;

    /// <summary>Localized display name; falls back to <see cref="Name"/>.</summary>
    [JsonIgnore]
    public string LocalizedName
    {
        get => _localizedName ?? Item?.LocalizedName ?? Name;
        set => _localizedName = value;
    }

    public override string ToString() => LocalizedName;
}
