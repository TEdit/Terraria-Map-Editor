using System;

namespace TEdit.RenderWorld
{
    [Flags]
    public enum FramePlacement
    {
        None = 0x00,
        Floor,
        Surface,
        FloorSurface = Floor | Surface,
        Ceiling,
        Wall,
        WallFloor = Floor | Wall,
        WallFloorCeiling = Floor | Ceiling | Wall,
        Float,
        Any = Floor | Surface | Ceiling | Wall | Float,
        MustHaveAll,
        CFBoth = Ceiling | Floor | MustHaveAll,
    }
}