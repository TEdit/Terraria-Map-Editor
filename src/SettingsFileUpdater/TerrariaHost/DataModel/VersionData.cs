using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SettingsFileUpdater.TerrariaHost.DataModel;

/// <summary>
/// Mirrors TEdit.Terraria.DataModel.SaveVersionData but uses int[] for FramedTileIds
/// so InlineNumericArrayConverter produces inline JSON arrays.
/// Cannot reference TEdit.Terraria directly (net10.0 vs net48 TFM mismatch).
/// </summary>
public class VersionData
{
    [JsonPropertyOrder(0)]
    public int SaveVersion { get; set; }

    [JsonPropertyOrder(1)]
    public string GameVersion { get; set; }

    [JsonPropertyOrder(2)]
    public int MaxTileId { get; set; }

    [JsonPropertyOrder(3)]
    public int MaxWallId { get; set; }

    [JsonPropertyOrder(4)]
    public int MaxItemId { get; set; }

    [JsonPropertyOrder(5)]
    public int MaxNpcId { get; set; }

    [JsonPropertyOrder(6)]
    public int MaxMoonId { get; set; }

    [JsonPropertyOrder(7)]
    public int[] FramedTileIds { get; set; } = [];
}

/// <summary>
/// Root model for versions.json / TerrariaVersionTileData.json.
/// Typed so int[] FramedTileIds round-trips through InlineNumericArrayConverter.
/// </summary>
public class VersionsFileJson
{
    public Dictionary<string, int> GameVersionToSaveVersion { get; set; } = new();
    public List<VersionData> SaveVersions { get; set; } = new();
}
