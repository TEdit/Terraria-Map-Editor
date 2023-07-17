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

    public enum LiquidAmountMode : byte
    {
        [Description("0%")]
        ZeroPercent = 0x0,
        [Description("10%")]
        TenPercent = 0x1A,
        [Description("20%")]
        TwentyPercent = 0x33,
        [Description("30%")]
        ThirtyPercent = 0x4D,
        [Description("40%")]
        FourtyPercent = 0x66,
        [Description("50%")]
        FiftyPercent = 0x80,
        [Description("60%")]
        SixtyPercent = 0x99,
        [Description("70%")]
        SeventyPercent = 0xB3,
        [Description("80%")]
        EightyPercent = 0xCC,
        [Description("90%")]
        NinteyPercent = 0xE6,
        [Description("100%")]
        OneHundredPercent = 0xFF
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
