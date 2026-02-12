using System.Text.Json.Serialization;
using TEdit.Geometry;

namespace SettingsFileUpdater.TerrariaHost.DataModel;

/// <summary>
/// NPC data for npcs.json (friendly NPCs for placement).
/// Note: This is different from the existing NpcData class which is for bestiary data.
/// </summary>
public class NpcDataJson
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    public Vector2Short Size { get; set; }
}
