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
    /// Hitbox size (legacy, kept for compatibility).
    /// </summary>
    public Vector2Short Size { get; set; }

    /// <summary>
    /// Source rectangle for the first frame of the NPC sprite sheet.
    /// Format: [x, y, width, height]. If not specified, uses full texture.
    /// </summary>
    public RectangleInt32 SourceRect { get; set; }

    /// <summary>
    /// Target draw size for rendering. If not specified, uses SourceRect dimensions.
    /// </summary>
    public Vector2Short DrawSize { get; set; }

    /// <summary>
    /// Vertical tile offset for positioning. Positive moves down, negative moves up.
    /// </summary>
    public int TileOffsetY { get; set; }

    public override string ToString() => Name;
}
