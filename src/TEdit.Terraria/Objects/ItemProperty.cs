using System.Text.Json.Serialization;
using TEdit.Common;
using TEdit.Geometry;

namespace TEdit.Terraria.Objects;

public class ItemProperty : ITile
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    public float Scale { get; set; } = 1f;
    public int MaxStackSize { get; set; }

    // Category flags
    public bool IsFood { get; set; }
    public bool IsKite { get; set; }
    public bool IsCritter { get; set; }
    public bool IsAccessory { get; set; }
    public bool IsRackable { get; set; }
    public bool IsMount { get; set; }

    // Armor slot indexes (-1 = not an armor piece)
    public int Head { get; set; } = -1;
    public int Body { get; set; } = -1;
    public int Legs { get; set; } = -1;

    // Kill tally index (0 = none)
    public int Tally { get; set; }

    // Legacy properties (for compatibility)
    [JsonIgnore]
    public Vector2Short UV { get; set; }
    [JsonIgnore]
    public Vector2Short Size { get; set; }
    [JsonIgnore]
    public TEditColor Color => TEditColor.Transparent;

    public override string ToString() => Name;
}
