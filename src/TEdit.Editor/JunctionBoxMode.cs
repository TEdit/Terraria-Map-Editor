using System.ComponentModel;

namespace TEdit.Editor;

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
