#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using TEdit.Common.Serialization;

namespace TEdit.Terraria.DataModel;

/// <summary>
/// Describes a background texture that is a sprite sheet (multiple frames in one image).
/// Terraria stores some backgrounds as 2x2 grids or 1x3 vertical strips.
/// </summary>
public class SpriteSheetTexture
{
    /// <summary>Background texture index.</summary>
    public int Id { get; set; }

    /// <summary>Number of columns in the sprite sheet.</summary>
    public int Columns { get; set; } = 1;

    /// <summary>Number of rows in the sprite sheet.</summary>
    public int Rows { get; set; } = 1;
}

/// <summary>
/// A single background style entry. All biome types use the same class
/// with optional properties for biome-specific data.
/// </summary>
public class BackgroundStyle
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "";

    /// <summary>Primary texture indices.</summary>
    public int[]? Textures { get; set; }

    /// <summary>Secondary textures (e.g., mountain layer for forest/snow).</summary>
    public int[]? SecondaryTextures { get; set; }

    /// <summary>Cave underground background index offset.</summary>
    public int? UndergroundIndex { get; set; }

    /// <summary>Hell bottom layer texture.</summary>
    public int? BottomTexture { get; set; }

    // Desert biome variants
    public int[]? CorruptTextures { get; set; }
    public int[]? HallowTextures { get; set; }
    public int[]? CrimsonTextures { get; set; }

    /// <summary>
    /// Gets the first valid texture index for preview purposes.
    /// </summary>
    public int GetPreviewTextureIndex()
    {
        if (Textures is { Length: > 0 } && Textures[0] >= 0)
            return Textures[0];

        if (SecondaryTextures is { Length: > 0 } && SecondaryTextures[0] >= 0)
            return SecondaryTextures[0];

        return -1;
    }

    public override string ToString() => Name;
}

/// <summary>
/// Root configuration for background style mappings from backgroundStyles.json.
/// </summary>
public class BackgroundStyleConfiguration
{
    public string Version { get; set; } = "";

    // Surface backgrounds
    public List<BackgroundStyle> TreeStyles { get; set; } = [];
    public List<BackgroundStyle> ForestBackgrounds { get; set; } = [];
    public List<BackgroundStyle> CorruptionBackgrounds { get; set; } = [];
    public List<BackgroundStyle> JungleBackgrounds { get; set; } = [];
    public List<BackgroundStyle> SnowBackgrounds { get; set; } = [];
    public List<BackgroundStyle> HallowBackgrounds { get; set; } = [];
    public List<BackgroundStyle> CrimsonBackgrounds { get; set; } = [];
    public List<BackgroundStyle> DesertBackgrounds { get; set; } = [];
    public List<BackgroundStyle> OceanBackgrounds { get; set; } = [];
    public List<BackgroundStyle> MushroomBackgrounds { get; set; } = [];

    // Underground backgrounds
    public List<BackgroundStyle> UnderworldBackgrounds { get; set; } = [];
    public List<BackgroundStyle> CaveBackgrounds { get; set; } = [];
    public List<BackgroundStyle> IceBackgrounds { get; set; } = [];
    public List<BackgroundStyle> JungleUndergroundBackgrounds { get; set; } = [];
    public List<BackgroundStyle> HellBackgrounds { get; set; } = [];

    /// <summary>
    /// Background textures that are sprite sheets (multiple frames in one image).
    /// Used to determine the correct source rectangle for rendering and previews.
    /// </summary>
    public List<SpriteSheetTexture> SpriteSheetTextures { get; set; } = [];

    /// <summary>Lookup: texture index → sprite sheet info. Null if not a sprite sheet.</summary>
    [JsonIgnore] public Dictionary<int, SpriteSheetTexture> SpriteSheetById { get; private set; } = new();

    // Lookup dictionaries (built post-load)
    [JsonIgnore] public Dictionary<int, BackgroundStyle> TreeStyleById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> ForestBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> CorruptionBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> JungleBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> SnowBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> HallowBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> CrimsonBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> DesertBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> OceanBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> MushroomBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> UnderworldBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> CaveBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> IceBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> JungleUndergroundBackgroundById { get; private set; } = new();
    [JsonIgnore] public Dictionary<int, BackgroundStyle> HellBackgroundById { get; private set; } = new();

    public static BackgroundStyleConfiguration Load(Stream stream)
    {
        var config = JsonSerializer.Deserialize<BackgroundStyleConfiguration>(stream, TEditJsonSerializer.DefaultOptions)
            ?? new BackgroundStyleConfiguration();
        config.BuildIndexes();
        return config;
    }

    private void BuildIndexes()
    {
        TreeStyleById = TreeStyles.ToDictionary(x => x.Id);
        ForestBackgroundById = ForestBackgrounds.ToDictionary(x => x.Id);
        CorruptionBackgroundById = CorruptionBackgrounds.ToDictionary(x => x.Id);
        JungleBackgroundById = JungleBackgrounds.ToDictionary(x => x.Id);
        SnowBackgroundById = SnowBackgrounds.ToDictionary(x => x.Id);
        HallowBackgroundById = HallowBackgrounds.ToDictionary(x => x.Id);
        CrimsonBackgroundById = CrimsonBackgrounds.ToDictionary(x => x.Id);
        DesertBackgroundById = DesertBackgrounds.ToDictionary(x => x.Id);
        OceanBackgroundById = OceanBackgrounds.ToDictionary(x => x.Id);
        MushroomBackgroundById = MushroomBackgrounds.ToDictionary(x => x.Id);
        UnderworldBackgroundById = UnderworldBackgrounds.ToDictionary(x => x.Id);
        CaveBackgroundById = CaveBackgrounds.ToDictionary(x => x.Id);
        IceBackgroundById = IceBackgrounds.ToDictionary(x => x.Id);
        JungleUndergroundBackgroundById = JungleUndergroundBackgrounds.ToDictionary(x => x.Id);
        HellBackgroundById = HellBackgrounds.ToDictionary(x => x.Id);
        SpriteSheetById = SpriteSheetTextures.ToDictionary(x => x.Id);
    }
}
