using System.Collections.Generic;

namespace TEdit.Terraria.DataModel;

public enum MorphLevel
{
    Sky,
    Dirt,
    Rock,
    DeepRock,
    Hell
}

public class MorphBiomeData
{
    public string Name { get; set; } = "";
    public List<MorphId> MorphTiles { get; set; } = [];
    public List<MorphId> MorphWalls { get; set; } = [];
}

public class MorphId
{
    public string Name { get; set; } = "";
    public bool Delete { get; set; }
    public bool UseMoss { get; set; }

    public MorphIdLevels Default { get; set; } = new();
    public MorphIdLevels? TouchingAir { get; set; }
    public MorphIdLevels? Gravity { get; set; }

    public HashSet<ushort> SourceIds { get; set; } = [];
    public List<MorphSpriteUVOffset> SpriteOffsets { get; set; } = [];
}

public class MorphIdLevels
{
    public ushort? EvilId { get; set; }
    public ushort? SkyId { get; set; }
    public ushort? DirtId { get; set; }
    public ushort? RockId { get; set; }
    public ushort? DeepRockId { get; set; }
    public ushort? HellId { get; set; }

    public ushort? GetId(MorphLevel level, bool useEvil)
    {
        if (useEvil && EvilId != null)
        {
            return EvilId;
        }

        if (level == MorphLevel.Dirt)
        {
            return DirtId ?? SkyId;
        }

        if (level == MorphLevel.Rock)
        {
            return RockId ?? DirtId ?? SkyId;
        }

        if (level == MorphLevel.DeepRock)
        {
            return DeepRockId ?? RockId ?? DirtId ?? SkyId;
        }

        if (level == MorphLevel.Hell)
        {
            return HellId ?? DeepRockId ?? RockId ?? DirtId ?? SkyId;
        }

        // default to sky
        return SkyId;
    }
}

public class MorphSpriteUVOffset
{
    public short MinU { get; set; }
    public short MaxU { get; set; }
    public short OffsetU { get; set; }
    public short MinV { get; set; }
    public short MaxV { get; set; }
    public short OffsetV { get; set; }
    public bool UseFilterV { get; set; }
    public bool Delete { get; set; }
}
