using System.Collections.Generic;

namespace TEdit.Terraria.DataModel;

public class BestiaryNpcConfiguration
{
    public List<string> Cat { get; set; } = [];
    public List<string> Dog { get; set; } = [];
    public List<string> Bunny { get; set; } = [];
    public List<BestiaryNpcData> NpcData { get; set; } = [];
}
