using System.ComponentModel;

namespace TEditXna.Editor
{
    public enum MaskMode
    {
        [Description("Disable Mask")]
        Off,
        [Description("Edit Empty")]
        Empty,
        [Description("Edit Matching")]
        Match
    }
}