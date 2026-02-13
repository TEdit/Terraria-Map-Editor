using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SettingsFileUpdater.TerrariaHost.DataModel;

public class BestiaryConfigJson
{
    public List<NpcData> NpcData { get; set; } = [];
}
