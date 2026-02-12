using System.Text.Json.Serialization;
using TEdit.Common;

namespace SettingsFileUpdater.TerrariaHost.DataModel;

public class PaintDataJson
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    [JsonPropertyOrder(2)]
    public TEditColor Color { get; set; } = TEditColor.Magenta;
}
