namespace TEdit.Terraria.TModLoader;

/// <summary>
/// Per-cell mod wall overlay data extracted from the .twld binary tile stream.
/// </summary>
public struct ModWallData
{
    /// <summary>Index into the TwldData.WallMap (the mod wall entry).</summary>
    public ushort WallMapIndex;

    /// <summary>Paint color byte.</summary>
    public byte WallColor;
}
