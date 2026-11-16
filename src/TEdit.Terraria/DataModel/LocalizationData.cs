using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TEdit.Terraria.DataModel;

/// <summary>
/// Localization data using string const-name keys (e.g., "IronPickaxe", "BlueSlime").
/// The official <c>{locale}.json</c> resource only contains names generated from the
/// game's localization files. Manually verified corrections live in the optional
/// <c>LocalizationOverrides.{locale}.json</c> resource and are merged by the loader.
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

    /// <summary>
    /// Exact frame label corrections keyed by parent tile key and original frame
    /// <see cref="Objects.FrameProperty.ToString"/> value. Only valid in the
    /// overrides resource; official generated resources never contain frame data.
    /// Null means the section is absent.
    /// </summary>
    [JsonPropertyName("frames")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Dictionary<string, Dictionary<string, string>>? Frames { get; set; }

    /// <summary>
    /// Supplemental localized names that are not covered by the official mirror sections:
    /// frame style / variety names, frame anchors, wall natural/unused markers and
    /// biome-variant labels. Loaded from <c>supplementalNames.{locale}.json</c>.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, string> SupplementalNames { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Keys that were actually applied from the overrides resource. Useful for
    /// diagnostics and integrity tests, never serialized.
    /// </summary>
    [JsonIgnore]
    public HashSet<string> OverriddenKeys { get; set; } = new(StringComparer.Ordinal);
}

public class BuffLocalization
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
