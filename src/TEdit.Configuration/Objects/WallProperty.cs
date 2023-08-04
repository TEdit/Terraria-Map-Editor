using TEdit.Common;
using TEdit.Common.Reactive;

namespace TEdit.Terraria.Objects;

public class WallProperty : ObservableObject, ITile
{
    public int Id { get; set; } = -1;
    public string Name { get; set; } = "UNKNOWN";
    public TEditColor Color { get; set; } = TEditColor.Magenta;
    public override string ToString() => Name;
}
