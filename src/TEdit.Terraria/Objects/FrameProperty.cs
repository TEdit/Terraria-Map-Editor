using System.Text;
using System.Text.Json.Serialization;
using TEdit.Common;
using TEdit.Geometry;

namespace TEdit.Terraria.Objects;

public class FrameProperty : ITile
{
    public string? Variety { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FrameAnchor Anchor { get; set; } = FrameAnchor.None;
    public TEditColor Color { get; set; } = TEditColor.Transparent;
    public int Id { get; set; }
    public Vector2Short UV { get; set; } = Vector2Short.Zero;
    public Vector2Short Size { get; set; } = Vector2Short.Zero;  // Default to (0,0) so tile's FrameSize is used when not specified
    public string Name { get; set; } = "Default";

    /// <summary>
    /// Custom source rectangle [x, y, width, height] within the texture.
    /// When set, preview reads from this region instead of UV-based grid calculation.
    /// </summary>
    public int[]? SourceRect { get; set; }

    /// <summary>
    /// Pixel offset [x, y] for preview positioning relative to the tile anchor.
    /// </summary>
    public short[]? OffsetPx { get; set; }

    /// <summary>
    /// Half-width and half-height (in tiles) of the buff detection zone for this specific frame.
    /// Overrides tile-level BuffRadius when present.
    /// </summary>
    public Vector2Short? BuffRadius { get; set; }

    /// <summary>
    /// Display name of the buff granted by this frame variant.
    /// </summary>
    public string? BuffName { get; set; }

    /// <summary>
    /// RGBA overlay color used to render the buff radius visualisation.
    /// </summary>
    public TEditColor? BuffColor { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder(Name);
        if (!string.IsNullOrWhiteSpace(Variety))
        {
            sb.Append(": ");
            sb.Append(Variety);
        }
        if (Anchor != FrameAnchor.None)
        {
            sb.Append(" [");
            sb.Append(Anchor.ToString());
            sb.Append(']');

        }
        return sb.ToString();
    }
}
