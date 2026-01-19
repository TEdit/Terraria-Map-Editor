using System;
using System.Collections.Generic;
using TEdit.Configuration;
using TEdit.Geometry;

namespace TEdit.Terraria;

public class Tile : IEquatable<Tile>
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

    public override bool Equals(object obj)
    {
        return Equals(obj as Tile);
    }

    /// <summary>
    /// Compares serialized tile properties. Ignores [NonSerialized] cache fields.
    /// </summary>
    public bool Equals(Tile other)
    {
        return other is not null &&
               IsActive == other.IsActive &&
               Type == other.Type &&
               U == other.U &&
               V == other.V &&
               TileColor == other.TileColor &&
               Wall == other.Wall &&
               WallColor == other.WallColor &&
               LiquidAmount == other.LiquidAmount &&
               LiquidType == other.LiquidType &&
               WireRed == other.WireRed &&
               WireGreen == other.WireGreen &&
               WireBlue == other.WireBlue &&
               WireYellow == other.WireYellow &&
               BrickStyle == other.BrickStyle &&
               Actuator == other.Actuator &&
               InActive == other.InActive &&
               InvisibleBlock == other.InvisibleBlock &&
               InvisibleWall == other.InvisibleWall &&
               FullBrightBlock == other.FullBrightBlock &&
               FullBrightWall == other.FullBrightWall;
    }

    public override int GetHashCode()
    {
        int hashCode = -1661845228;
        hashCode = hashCode * -1521134295 + IsActive.GetHashCode();
        hashCode = hashCode * -1521134295 + Type.GetHashCode();
        hashCode = hashCode * -1521134295 + U.GetHashCode();
        hashCode = hashCode * -1521134295 + V.GetHashCode();
        hashCode = hashCode * -1521134295 + TileColor.GetHashCode();
        hashCode = hashCode * -1521134295 + Wall.GetHashCode();
        hashCode = hashCode * -1521134295 + WallColor.GetHashCode();
        hashCode = hashCode * -1521134295 + LiquidAmount.GetHashCode();
        hashCode = hashCode * -1521134295 + LiquidType.GetHashCode();
        hashCode = hashCode * -1521134295 + WireRed.GetHashCode();
        hashCode = hashCode * -1521134295 + WireGreen.GetHashCode();
        hashCode = hashCode * -1521134295 + WireBlue.GetHashCode();
        hashCode = hashCode * -1521134295 + WireYellow.GetHashCode();
        hashCode = hashCode * -1521134295 + BrickStyle.GetHashCode();
        hashCode = hashCode * -1521134295 + Actuator.GetHashCode();
        hashCode = hashCode * -1521134295 + InActive.GetHashCode();
        hashCode = hashCode * -1521134295 + InvisibleBlock.GetHashCode();
        hashCode = hashCode * -1521134295 + InvisibleWall.GetHashCode();
        hashCode = hashCode * -1521134295 + FullBrightBlock.GetHashCode();
        hashCode = hashCode * -1521134295 + FullBrightWall.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(Tile left, Tile right)
    {
        return EqualityComparer<Tile>.Default.Equals(left, right);
    }

    public static bool operator !=(Tile left, Tile right)
    {
        return !(left == right);
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
