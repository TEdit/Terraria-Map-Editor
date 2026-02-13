using System.Collections.Generic;
using System.Text.Json.Serialization;
using TEdit.Geometry;

namespace TEdit.Terraria.DataModel;

public class NpcData
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    /// <summary>
    /// Source rectangle for the first frame of the NPC sprite sheet.
    /// Format: [x, y, width, height]. If not specified, uses full texture.
    /// </summary>
    public RectangleInt32 SourceRect { get; set; }

    /// <summary>
    /// Vertical tile offset for positioning. Positive moves down, negative moves up.
    /// </summary>
    public int TileOffsetY { get; set; }

    /// <summary>
    /// Ordered list of variant display names (index matches TownNpcVariationIndex).
    /// Null if the NPC has no variants.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Variants { get; set; }

    /// <summary>
    /// Whether this NPC supports shimmer transformation.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool CanShimmer { get; set; }

    public override string ToString() => Name;
}
