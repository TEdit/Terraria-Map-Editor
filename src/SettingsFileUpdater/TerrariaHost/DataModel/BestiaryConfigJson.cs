using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SettingsFileUpdater.TerrariaHost.DataModel;

public class BestiaryConfigJson
{
    [JsonPropertyOrder(0)]
    public List<string> Cat { get; set; } = [];

    [JsonPropertyOrder(1)]
    public List<string> Dog { get; set; } = [];

    [JsonPropertyOrder(2)]
    public List<string> Bunny { get; set; } = [];

    [JsonPropertyOrder(3)]
    public List<NpcData> NpcData { get; set; } = [];
}
