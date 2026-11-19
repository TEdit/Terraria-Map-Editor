using System.Text.Json.Serialization;
using TEdit.Common;

namespace TEdit.Terraria.Objects;

public class PaintProperty
{
    public override string ToString() => LocalizedName;

    public PaintProperty()
    {

    }

    public PaintProperty(int id, string name, TEditColor color, string? key = null)
    {
        Color = color;
        Id = id;
        Name = name;
        Key = key;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyOrder(0)]
    public int Id { get; set; } = -1;

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    private string? _localizedName;

    /// <summary>Localized display name; falls back to <see cref="Name"/>.</summary>
    [JsonIgnore]
    public string LocalizedName
    {
        get => _localizedName ?? Name;
        set => _localizedName = value;
    }

    [JsonPropertyOrder(2)]
    public string? Key { get; set; }

    [JsonPropertyOrder(3)]
    public TEditColor Color { get; set; } = TEditColor.Magenta;
}
