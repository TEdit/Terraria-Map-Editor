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
        [Description("Track")]
        Track,
    }
    public enum TrackMode
    {
        [Description("Track")]
        Track,
        [Description("Booster")]
        Booster,
        [Description("PressurePlate")]
        Pressure,
        [Description("Hammer")]
        Hammer,
    }
}