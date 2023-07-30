using System.ComponentModel;

namespace TEdit.Editor;

public enum TrackMode
{
    [Description("Track")]
    Track,
    [Description("Booster")]
    Booster,
    [Description("PressurePlate")]
    Pressure,
    [Description("Hammer")]
    Hammer
}
