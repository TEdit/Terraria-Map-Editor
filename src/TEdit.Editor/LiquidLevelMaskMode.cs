using System.ComponentModel;

namespace TEdit.Editor;

public enum LiquidLevelMaskMode
{
    [Description("Ignore")]
    Ignore,
    [Description("Greater Than")]
    GreaterThan,
    [Description("Less Than")]
    LessThan,
    [Description("Equal")]
    Equal,
}
