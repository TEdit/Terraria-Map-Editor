using System.Text.Json.Serialization;

namespace SettingsFileUpdater.TerrariaHost.DataModel;

public class PrefixDataJson
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "";
}
