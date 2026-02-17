using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TEdit.Terraria.DataModel;

/// <summary>
/// Localization data using string const-name keys (e.g., "IronPickaxe", "BlueSlime").
/// </summary>
public class LocalizationData
{
    [JsonPropertyName("items")]
    public Dictionary<string, string> Items { get; set; } = new();

    [JsonPropertyName("npcs")]
    public Dictionary<string, string> Npcs { get; set; } = new();

    [JsonPropertyName("prefixes")]
    public Dictionary<string, string> Prefixes { get; set; } = new();

    [JsonPropertyName("tiles")]
    public Dictionary<string, string> Tiles { get; set; } = new();

    [JsonPropertyName("walls")]
    public Dictionary<string, string> Walls { get; set; } = new();
}
