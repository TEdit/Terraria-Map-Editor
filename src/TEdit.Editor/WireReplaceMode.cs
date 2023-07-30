using System;
using System.ComponentModel;

namespace TEdit.Editor;

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
