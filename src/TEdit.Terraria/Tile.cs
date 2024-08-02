using System;
using TEdit.Configuration;
using TEdit.Geometry;

namespace TEdit.Terraria;

public class Tile
{
    public static readonly Tile Empty = new Tile();

    public bool IsEmpty { get => !IsActive && Wall == 0 && !HasLiquid && !HasWire && !Actuator; }
    public bool HasWire { get => WireBlue || WireRed || WireGreen || WireYellow; }
    public bool HasLiquid { get => LiquidAmount > 0 && LiquidType != LiquidType.None; }
    public bool HasMultipleWires
    {
        get
        {
            if (WireRed && (WireGreen || WireBlue || WireYellow)) return true;
            if (WireGreen && (WireBlue || WireYellow)) return true;
            if (WireBlue && WireYellow) return true;

            return false;
        }
    }

    public bool Actuator;
    public BrickStyle BrickStyle;
    public bool InActive;
    public bool IsActive;
    public bool v0_Lit;
    public byte LiquidAmount;
    public LiquidType LiquidType;
    public byte TileColor;
    public ushort Type;
    public Int16 U;
    public Int16 V;
    public ushort Wall;
    public byte WallColor;
    public bool WireBlue;
    public bool WireGreen;
    public bool WireRed;
    public bool WireYellow;
    public bool InvisibleBlock;
    public bool InvisibleWall;
    public bool FullBrightBlock;
    public bool FullBrightWall;

    [NonSerialized] /* Heathtech */
    public ushort uvTileCache = 0xFFFF; //Caches the UV position of a tile, since it is costly to generate each frame
    [NonSerialized] /* Heathtech */
    public ushort uvWallCache = 0xFFFF; //Caches the UV position of a wall tile
    [NonSerialized] /* Heathtech */
    public byte lazyMergeId = 0xFF; //The ID here refers to a number that helps blocks know whether they are actually merged with a nearby tile
    [NonSerialized] /* Heathtech */
    public bool hasLazyChecked = false; //Whether the above check has taken place


    public Vector2Short GetUV() => new Vector2Short(U, V);

    public object Clone()
    {
        return MemberwiseClone();
    }

    // Added legacy enums back
    public enum WallType : int
    {
        Sky = 0,
        StoneWall = 1,
        DirtWall = 2
    }

    public enum TileType : int
    {
        DirtBlock = 0,
        StoneBlock = 1,
        GrassBlock = 2,
        AshBlock = 57
    }

    public void Reset()
    {
        Actuator = false;
        BrickStyle = 0;
        InActive = false;
        IsActive = false;
        LiquidAmount = 0;
        LiquidType = 0;
        TileColor = 0;
        Type = 0;
        U = 0;
        V = 0;
        Wall = 0;
        WallColor = 0;
        WireBlue = false;
        WireGreen = false;
        WireRed = false;
        WireYellow = false;
        FullBrightBlock = false;
        FullBrightWall = false;
        InvisibleBlock = false;
        InvisibleWall = false;
    }

    public bool IsTileEntity() => TileTypes.IsTileEntity(Type);
    public bool IsChest() => TileTypes.IsChest(Type);
    public bool IsSign() => TileTypes.IsSign(Type);
    public bool StopsWallsFloodFill() => TileTypes.StopsWallsFloodFill(Type);
}
