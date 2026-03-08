namespace TEdit.Terraria.TModLoader;

/// <summary>
/// Per-cell mod tile overlay data extracted from the .twld binary tile stream.
/// </summary>
public struct ModTileData
{
    /// <summary>Index into the TwldData.TileMap (the mod tile entry).</summary>
    public ushort TileMapIndex;

    /// <summary>Paint color byte.</summary>
    public byte Color;

    /// <summary>Frame X coordinate (if frame-important).</summary>
    public short FrameX;

    /// <summary>Frame Y coordinate (if frame-important).</summary>
    public short FrameY;
}
