using TEdit.Common;
using TEdit.Geometry;

namespace TEdit.Terraria.Objects;

public class ChestProperty : ITile
{
    public int ChestId { get; set; }
    public Vector2Short UV { get; set; }
    public ushort TileType { get; set; }
    public Vector2Short Size { get; set; }
    public string Name { get; set; } = "UNKNOWN";
    public TEditColor Color => TEditColor.Transparent;
    public int Id { get; set; }

}
