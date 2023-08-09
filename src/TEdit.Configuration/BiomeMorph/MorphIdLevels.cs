namespace TEdit.Configuration.BiomeMorph;

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
