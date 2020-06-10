using System.ComponentModel;
using TEdit.UI.Xaml.Enum;

namespace TEdit.Editor
{
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
    }
}
