using TEdit.Geometry;

namespace TEdit.Terraria;

public partial class TownManager : ReactiveObject
{
    [Reactive]
    private Vector2Int32 _home;

    [Reactive]
    private int _npcId;
}
