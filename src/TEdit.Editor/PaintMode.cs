using System.ComponentModel;

namespace TEdit.Editor;

public enum PaintMode
{
    [Description("Tile/Wall")]
    TileAndWall,
    [Description("Wire")]
    Wire,
    [Description("Liquid")]
    Liquid,
    [Description("Track")]
    Track,
    [Description("Sprites")]
    Sprites
}
