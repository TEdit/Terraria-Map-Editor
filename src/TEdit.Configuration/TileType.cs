namespace TEdit.Configuration;

public enum TileType : int
{
    DirtBlock = 0,
    StoneBlock = 1,
    Torch = 4,
    Tree = 5,
    Platform = 19,
    Chest = 21,
    Sunflower = 27,
    Chandelier = 34,
    Sign = 55,
    MushroomTree = 72,
    GraveMarker = 85,
    Dresser = 88,
    EbonsandBlock = 112,
    PearlsandBlock = 116,
    CrimsandBlock = 234,
    PlanteraBulb = 238,
    IceByRod = 127,
    WaterCandle = 49,
    MysticSnakeRope = 504,
    TrappedChest = 441,
    Chest2 = 467,
    TrappedChest2 = 468,
    AnnouncementBox = 425,
    TatteredSign = 573,
    ChristmasTree = 171,
    MinecartTrack = 314,
    // Tile Entities
    MannequinLegacy = 128,
    WomannequinLegacy = 269,
    DisplayDoll = 470, // aka Mannequin
    FoodPlatter = 520, // aka plate
    Timer = 144,
    TrainingDummy = 378,
    ItemFrame = 395,
    LogicSensor = 423,
    WeaponRack = 471,
    WeaponRackLegacy = 334,
    HatRack = 475,
    TeleportationPylon = 597,
    DoorClosed = 10,
    DoorOpen = 11,
    TrapDoor = 386,
    TrapDoorOpen = 387,
    TallGate = 388,
    TallGateClosed = 389,
    JunctionBox = 424
}

public static class TileTypes
{
    public static bool IsChest(int tileType)
    {
        return tileType == (int)TileType.Chest
            || tileType == (int)TileType.Dresser
            || tileType == (int)TileType.Chest2
            || tileType == (int)TileType.TrappedChest2
            || tileType == (int)TileType.TrappedChest;
    }

    public static bool IsSign(int tileType)
    {
        return tileType == (int)TileType.Sign
            || tileType == (int)TileType.GraveMarker
            || tileType == (int)TileType.AnnouncementBox
            || tileType == (int)TileType.TatteredSign;
    }
    public static bool IsTileEntity(int tileType)
    {
        return tileType == (int)TileType.DisplayDoll
            || tileType == (int)TileType.MannequinLegacy
            || tileType == (int)TileType.WomannequinLegacy
            || tileType == (int)TileType.FoodPlatter
            || tileType == (int)TileType.TrainingDummy
            || tileType == (int)TileType.ItemFrame
            || tileType == (int)TileType.LogicSensor
            || tileType == (int)TileType.WeaponRackLegacy
            || tileType == (int)TileType.WeaponRack
            || tileType == (int)TileType.HatRack
            || tileType == (int)TileType.TeleportationPylon;
    }

    public static bool StopsWallsFloodFill(ushort type)
    {
        return type == (ushort)TileType.DoorClosed ||
               type == (ushort)TileType.DoorOpen ||
               type == (ushort)TileType.TrapDoor ||
               type == (ushort)TileType.TrapDoorOpen ||
               type == (ushort)TileType.TallGate ||
               type == (ushort)TileType.DoorClosed ||
               type == (ushort)TileType.TallGateClosed;
    }
}
