using System.Text.Json.Serialization;
using TEdit.Common;

namespace TEdit.Terraria.DataModel;

public class GlobalColorEntry
{
    [JsonPropertyOrder(0)]
    public string Name { get; set; } = "";

    [JsonPropertyOrder(1)]
    public TEditColor Color { get; set; }
}
