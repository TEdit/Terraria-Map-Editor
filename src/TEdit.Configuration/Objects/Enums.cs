using System;

namespace TEdit.Terraria.Objects;


public enum FrameAnchor
{
    None,
    Left,
    Right,
    Top,
    Bottom
}

[Flags]
public enum FramePlacement
{
    None = 0x00,

    Floor = 0x01,
    Surface = 0x02,
    Ceiling = 0x04,
    Left = 0x08,
    Right = 0x10,
    Float = 0x20,
    MustHaveAll = 0x40,

    Wall = Left | Right,

    FloorSurface = Floor | Surface,
    FloorCeiling = Floor | Ceiling,          // currently not encountered in Terraria
    FloorSurfaceCeiling = FloorSurface | Ceiling,   // currently not encountered in Terraria
    WallCeiling = Wall | Ceiling,          // currently not encountered in Terraria
    WallFloor = Wall | Floor,
    WallFloorCeiling = Wall | FloorCeiling,
    WallFloorSurface = Wall | FloorSurface,     // currently not encountered in Terraria

    AnySurface = WallFloorCeiling | Surface,        // currently not encountered in Terraria
    Any = AnySurface | Float,
    CFBoth = FloorCeiling | MustHaveAll,

    All = Any | MustHaveAll     // only used as a bitwise tautology/disjunction
}