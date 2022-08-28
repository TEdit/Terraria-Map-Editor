using System;
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

    [Flags]
    public enum WireReplaceMode : byte
    {
        [Description("Off")]
        Off = 0b0000,
        [Description("Red")]
        Red = 0b0001,
        [Description("Blue")]
        Blue = 0b0010,
        [Description("Green")]
        Green = 0b0100,
        [Description("Yellow")]
        Yellow= 0b1000
    }
}