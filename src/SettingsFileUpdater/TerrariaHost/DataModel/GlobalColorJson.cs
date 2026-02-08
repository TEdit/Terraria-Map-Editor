using System.Text.Json.Serialization;
using TEdit.Common;

namespace SettingsFileUpdater.TerrariaHost.DataModel;

public class GlobalColorJson
{
    [JsonPropertyOrder(0)]
    public string Name { get; set; } = "";

    [JsonPropertyOrder(1)]
    public TEditColor Color { get; set; }
}
