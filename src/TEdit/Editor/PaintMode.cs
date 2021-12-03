using System.ComponentModel;

namespace TEdit.Editor
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
        [Description("Sprites")]
        Sprites
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
        Hammer
    }
    public enum JunctionBoxMode
    {
        [Description("None")]
        None,
        [Description("Left Facing")]
        LeftFacingBox,
        [Description("Normal")]
        NormalFacingBox,
        [Description("Right Facing")]
        RightFacingBox
    }
}