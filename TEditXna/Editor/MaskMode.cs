using System.ComponentModel;

namespace TEditXna.Editor
{
    public enum MaskMode
    {
        [Description("Disable Mask")]
        Off,
        [Description("Edit Empty Tiles")]
        Empty,
        [Description("Edit Matching Tiles")]
        Tile
    }
}