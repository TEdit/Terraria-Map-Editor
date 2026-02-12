using System.Text.Json.Serialization;
using TEdit.Geometry;

namespace TEdit.Terraria.DataModel;

public class NpcData
{
    [JsonPropertyOrder(0)]
    public int Id { get; set; }

    [JsonPropertyOrder(1)]
    public string Name { get; set; } = "UNKNOWN";

    public Vector2Short Size { get; set; }

    public override string ToString() => Name;
}
