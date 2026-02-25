using System.ComponentModel;

namespace TEdit.Editor;

public enum MaskPreset
{
    [Description("Off")]
    Off,
    [Description("Custom")]
    Custom,
    [Description("Exact Match")]
    ExactMatch,
}
