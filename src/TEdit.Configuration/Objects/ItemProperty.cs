using TEdit.Common;
using TEdit.Geometry;

namespace TEdit.Terraria.Objects;

public class ItemProperty : ITile
{
    public Vector2Short UV { get;set; }
    public Vector2Short Size { get; set; }
    public string Name { get; set; } = "UNKNOWN";
    public float Scale { get; set; }
    public bool IsFood { get; set; }
    public TEditColor Color => TEditColor.Transparent;
    public int Id { get; set; }
    public int MaxStackSize { get; set; }
}
