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

    public enum JunctionBoxMode : short
    {
        [Description("None")]
        None = -1,
        [Description("Left Facing")]
        Left = 18,
        [Description("Normal")]
        Normal = 0,
        [Description("Right Facing")]
        Right = 36
    }
    public enum WireReplaceModeOne
    {
        [Description("Red")]
        Red,
        [Description("Blue")]
        Blue,
        [Description("Green")]
        Green,
        [Description("Yellow")]
        Yellow
    }
    public enum WireReplaceModeTwo
    {
        [Description("Red")]
        Red,
        [Description("Blue")]
        Blue,
        [Description("Green")]
        Green,
        [Description("Yellow")]
        Yellow
    }
    public enum WireReplaceModeThree
    {
        [Description("Red")]
        Red,
        [Description("Blue")]
        Blue,
        [Description("Green")]
        Green,
        [Description("Yellow")]
        Yellow
    }
    public enum WireReplaceModeFour
    {
        [Description("Red")]
        Red,
        [Description("Blue")]
        Blue,
        [Description("Green")]
        Green,
        [Description("Yellow")]
        Yellow
    }
}