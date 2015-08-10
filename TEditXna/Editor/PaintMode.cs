using System.ComponentModel;

namespace TEditXna.Editor
{
    public enum PaintMode
    {
        [Description("Tile/Wall")]
        TileAndWall,
        [Description("Wire")]
        Wire,
        [Description("Liquid")]
        Liquid,
    }
}