using System.ComponentModel;

namespace TEdit.Editor;

public enum BrushShape
{
    [Description("Square")]
    Square,
    [Description("Ellipse")]
    Round,
    [Description("Diagonal Right")]
    Right,
    [Description("Diagonal Left")]
    Left,
    [Description("Star")]
    Star,
    [Description("Triangle")]
    Triangle,
    [Description("Crescent")]
    Crescent,
    [Description("Donut")]
    Donut,
    [Description("X")]
    Cross,
}
