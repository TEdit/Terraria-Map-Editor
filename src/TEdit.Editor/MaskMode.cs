using System.ComponentModel;

namespace TEdit.Editor;

public enum MaskMode
{
    [Description("Disable Mask")]
    Off,
    [Description("Edit Empty")]
    Empty,
    [Description("Edit Matching")]
    Match,
    [Description("Edit Not Matching")]
    NotMatching,
}
