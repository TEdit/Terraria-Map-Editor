using System.ComponentModel;

namespace TEdit.Editor;

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
