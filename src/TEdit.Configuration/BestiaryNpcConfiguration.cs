using System.Collections.Generic;

namespace TEdit.Configuration;

public class BestiaryNpcConfiguration
{
    public List<string> Cat { get; private set; } = new();
    public List<string> Dog { get; private set; } = new();
    public List<string> Bunny { get; private set; } = new();
    public List<BestiaryNpcData> NpcData { get; private set; } = new();
}
