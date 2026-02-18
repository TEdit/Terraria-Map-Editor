using System.Text.Json.Serialization;
using TEdit.Common;

namespace TEdit.Terraria.Objects;

public class DyeProperty
{
    [JsonPropertyName("itemId")]
    public int ItemId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("color")]
    public TEditColor Color { get; set; } = TEditColor.White;
}
