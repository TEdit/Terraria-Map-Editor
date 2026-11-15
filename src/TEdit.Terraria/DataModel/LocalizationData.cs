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

    [JsonPropertyName("buffs")]
    public Dictionary<string, BuffLocalization> Buffs { get; set; } = new();

    [JsonPropertyName("npcs")]
    public Dictionary<string, string> Npcs { get; set; } = new();

    [JsonPropertyName("prefixes")]
    public Dictionary<string, string> Prefixes { get; set; } = new();

    [JsonPropertyName("tiles")]
    public Dictionary<string, string> Tiles { get; set; } = new();

    [JsonPropertyName("walls")]
    public Dictionary<string, string> Walls { get; set; } = new();

    [JsonPropertyName("liquids")]
    public Dictionary<string, string> Liquids { get; set; } = new();

    [JsonPropertyName("wires")]
    public Dictionary<string, string> Wires { get; set; } = new();

    /// <summary>Optional frame labels keyed by parent tile key and original frame ToString.</summary>
    [JsonPropertyName("frames")]
    public Dictionary<string, Dictionary<string, string>> Frames { get; set; } = new();
}

public class BuffLocalization
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
