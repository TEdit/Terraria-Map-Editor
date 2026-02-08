using System.Text.Json.Serialization;

namespace TEdit.Terraria.DataModel;

public class PrefixData
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "";

    public override string ToString() => Name;
}
