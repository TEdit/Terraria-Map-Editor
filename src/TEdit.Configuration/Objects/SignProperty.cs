using TEdit.Common.Reactive;
using TEdit.Geometry;
using TEdit.Common;

namespace TEdit.Terraria.Objects;

public class SignProperty : ObservableObject, ITile
{
    public int SignId { get; set; }
    public Vector2Short UV { get; set; }
    public ushort TileType { get; set; }
    public string Name { get; set; } = "UNKNOWN";
    public TEditColor Color => TEditColor.Transparent;
    public int Id { get; set; }
}
