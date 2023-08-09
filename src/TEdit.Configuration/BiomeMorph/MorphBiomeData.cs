using System.Collections.Generic;

namespace TEdit.Configuration.BiomeMorph;

public class MorphBiomeData
{
    public string Name { get; set; }
    public List<MorphId> MorphTiles { get; set; } = new();
    public List<MorphId> MorphWalls { get; set; } = new();
}
