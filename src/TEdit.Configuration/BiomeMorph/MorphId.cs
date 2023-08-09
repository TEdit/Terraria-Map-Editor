using System.Collections.Generic;

namespace TEdit.Configuration.BiomeMorph;

public class MorphId
{
    public string Name { get; set; }
    public bool Delete { get; set; }
    public bool UseMoss { get; set; }

    public MorphIdLevels Default { get; set; } = new MorphIdLevels();
    public MorphIdLevels TouchingAir { get; set; }
    public MorphIdLevels Gravity { get; set; }

    public HashSet<ushort> SourceIds { get; set; } = new();
    public List<MorphSpriteUVOffset> SpriteOffsets { get; set; } = new();
}
