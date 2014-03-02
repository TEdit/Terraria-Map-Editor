using System.ComponentModel;

namespace TEditXna.Editor
{
    public enum PaintMode
    {
        [Description("Tile")]
        Tile,
        [Description("Wall")]
        Wall,
        [Description("Tile and Wall")]
        TileAndWall,
        [Description("Red Wire")]
        Wire,
        [Description("Blue Wire")]
        Wire2,
        [Description("Green Wire")]
        Wire3,
        [Description("Liquid")]
        Liquid,
        [Description("BrickStyle")]
        BrickStyle,
    }
}