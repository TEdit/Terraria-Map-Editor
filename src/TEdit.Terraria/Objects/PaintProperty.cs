using System.Text.Json.Serialization;
using TEdit.Common;

namespace TEdit.Terraria.Objects;

public class PaintProperty
{
    public override string ToString() => Name;

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

    [JsonPropertyOrder(2)]
    public string? Key { get; set; }

    [JsonPropertyOrder(3)]
    public TEditColor Color { get; set; } = TEditColor.Magenta;
}
