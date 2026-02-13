#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using TEdit.Common.Serialization;

namespace TEdit.Terraria.DataModel;

/// <summary>
/// Root configuration for background style mappings from backgroundStyles.json.
/// Contains all biome-specific background style definitions.
/// </summary>
public class BackgroundStyleConfiguration
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("note")]
    public string Note { get; set; } = string.Empty;

    [JsonPropertyName("treeStyle")]
    public BackgroundStyleSection TreeStyle { get; set; } = new();

    [JsonPropertyName("forestBg")]
    public BackgroundStyleSection ForestBg { get; set; } = new();

    [JsonPropertyName("corruptionBg")]
    public BackgroundStyleSection CorruptionBg { get; set; } = new();

    [JsonPropertyName("jungleBg")]
    public BackgroundStyleSection JungleBg { get; set; } = new();

    [JsonPropertyName("snowBg")]
    public BackgroundStyleSection SnowBg { get; set; } = new();

    [JsonPropertyName("hallowBg")]
    public BackgroundStyleSection HallowBg { get; set; } = new();

    [JsonPropertyName("crimsonBg")]
    public BackgroundStyleSection CrimsonBg { get; set; } = new();

    [JsonPropertyName("desertBg")]
    public BackgroundStyleSection DesertBg { get; set; } = new();

    [JsonPropertyName("oceanBg")]
    public BackgroundStyleSection OceanBg { get; set; } = new();

    [JsonPropertyName("mushroomBg")]
    public BackgroundStyleSection MushroomBg { get; set; } = new();

    [JsonPropertyName("underworldBg")]
    public BackgroundStyleSection UnderworldBg { get; set; } = new();

    [JsonPropertyName("caveStyle")]
    public BackgroundStyleSection CaveStyle { get; set; } = new();

    [JsonPropertyName("iceBackStyle")]
    public BackgroundStyleSection IceBackStyle { get; set; } = new();

    [JsonPropertyName("jungleBackStyle")]
    public BackgroundStyleSection JungleBackStyle { get; set; } = new();

    [JsonPropertyName("hellBackStyle")]
    public BackgroundStyleSection HellBackStyle { get; set; } = new();

    public static BackgroundStyleConfiguration Load(Stream stream)
    {
        return JsonSerializer.Deserialize<BackgroundStyleConfiguration>(stream, TEditJsonSerializer.DefaultOptions)
            ?? new BackgroundStyleConfiguration();
    }
}

/// <summary>
/// A section containing valid values and texture mappings for a background style type.
/// </summary>
public class BackgroundStyleSection
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("validValues")]
    public List<int> ValidValues { get; set; } = [];

    [JsonPropertyName("textureMapping")]
    public Dictionary<string, BackgroundTextureMapping> TextureMapping { get; set; } = new();

    /// <summary>
    /// Get the texture mapping for a specific value.
    /// </summary>
    public BackgroundTextureMapping? GetMapping(int value)
    {
        return TextureMapping.TryGetValue(value.ToString(), out var mapping) ? mapping : null;
    }

    /// <summary>
    /// Get all mappings as value-mapping pairs.
    /// </summary>
    public IEnumerable<(int Value, BackgroundTextureMapping Mapping)> GetAllMappings()
    {
        foreach (var value in ValidValues)
        {
            if (TextureMapping.TryGetValue(value.ToString(), out var mapping))
            {
                yield return (value, mapping);
            }
        }
    }
}

/// <summary>
/// Texture mapping data for a single background style value.
/// Different background types use different properties.
/// </summary>
public class BackgroundTextureMapping
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    // Single texture index (used by treeStyle, oceanBg, hellBackStyle)
    [JsonPropertyName("textureIndex")]
    public int? TextureIndex { get; set; }

    // Array of textures (used by most biome backgrounds)
    [JsonPropertyName("textures")]
    public List<int>? Textures { get; set; }

    // Forest-specific (tree and mountain sets)
    [JsonPropertyName("treeSet")]
    public List<int>? TreeSet { get; set; }

    [JsonPropertyName("mountainSet")]
    public List<int>? MountainSet { get; set; }

    // Snow-specific
    [JsonPropertyName("snowBG")]
    public List<int>? SnowBG { get; set; }

    [JsonPropertyName("snowMntBG")]
    public List<int>? SnowMntBG { get; set; }

    // Desert multi-biome variants
    [JsonPropertyName("pure")]
    public List<int>? Pure { get; set; }

    [JsonPropertyName("corrupt")]
    public List<int>? Corrupt { get; set; }

    [JsonPropertyName("hallow")]
    public List<int>? Hallow { get; set; }

    [JsonPropertyName("crimson")]
    public List<int>? Crimson { get; set; }

    // Underworld-specific
    [JsonPropertyName("underworldBG")]
    public List<int>? UnderworldBG { get; set; }

    // Cave style specific
    [JsonPropertyName("undergroundIndex")]
    public int? UndergroundIndex { get; set; }

    // Hell bottom texture
    [JsonPropertyName("bottomTexture")]
    public int? BottomTexture { get; set; }

    /// <summary>
    /// Gets the first valid texture index for preview purposes.
    /// Searches through all possible texture properties.
    /// </summary>
    public int GetPreviewTextureIndex()
    {
        // Single texture index
        if (TextureIndex.HasValue)
            return TextureIndex.Value;

        // General textures array
        if (Textures != null && Textures.Count > 0 && Textures[0] >= 0)
            return Textures[0];

        // Forest tree set
        if (TreeSet != null && TreeSet.Count > 0 && TreeSet[0] >= 0)
            return TreeSet[0];

        // Forest mountain set (fallback if tree set is all -1)
        if (MountainSet != null && MountainSet.Count > 0 && MountainSet[0] >= 0)
            return MountainSet[0];

        // Snow backgrounds
        if (SnowBG != null && SnowBG.Count > 0 && SnowBG[0] >= 0)
            return SnowBG[0];
        if (SnowMntBG != null && SnowMntBG.Count > 0 && SnowMntBG[0] >= 0)
            return SnowMntBG[0];

        // Desert pure
        if (Pure != null && Pure.Count > 0 && Pure[0] >= 0)
            return Pure[0];

        // Underworld
        if (UnderworldBG != null && UnderworldBG.Count > 0)
            return UnderworldBG[0];

        return -1;
    }
}
