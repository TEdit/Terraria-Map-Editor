using System.Text.Json.Serialization;
using TEdit.Common;

namespace TEdit.Terraria.Objects;

public class WallProperty : ReactiveObject, ITile
{
    [JsonPropertyOrder(0)]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int Id { get; set; } = -1;

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    [JsonPropertyOrder(2)]
    public string? Key { get; set; }

    [JsonPropertyOrder(3)]
    public TEditColor Color { get; set; } = TEditColor.Magenta;

    [JsonPropertyOrder(4)]
    public byte LargeFrameType { get; set; } = 0;

    [JsonPropertyOrder(5)]
    public int BlendType { get; set; } = -1;

    public override string ToString() => Name;
}
