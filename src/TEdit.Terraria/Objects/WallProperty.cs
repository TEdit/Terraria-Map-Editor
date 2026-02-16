using System.Text.Json.Serialization;
using ReactiveUI;
using TEdit.Common;

namespace TEdit.Terraria.Objects;

public class WallProperty : ReactiveObject, ITile
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; } = -1;

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    [JsonPropertyOrder(2)]
    public string? Key { get; set; }

    [JsonPropertyOrder(3)]
    public TEditColor Color { get; set; } = TEditColor.Magenta;

    public override string ToString() => Name;
}
