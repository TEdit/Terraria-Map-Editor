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
