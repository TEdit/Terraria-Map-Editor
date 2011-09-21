using System;

namespace TEdit.RenderWorld
{
    [Flags]
    public enum FramePlacement
    {
        Any = 0x00,
        Floor,
        Surface,
        FloorSurface = Floor | Surface,
        Ceiling,
        Wall,
        WallFloor = Floor | Wall,
        WallFloorCeiling = Floor | Ceiling | Wall,
        CFBoth,
    }
}