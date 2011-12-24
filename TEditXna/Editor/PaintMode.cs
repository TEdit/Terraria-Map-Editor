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
        [Description("Wire")]
        Wire,
        [Description("Liquid")]
        Liquid
    }
}