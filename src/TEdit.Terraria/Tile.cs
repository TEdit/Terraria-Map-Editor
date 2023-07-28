using System;
using System.ComponentModel;
using TEdit.Configuration;
using TEdit.Geometry;

namespace TEdit.Terraria
{



    [Serializable]
    public class Tile
    {
        public static readonly Tile Empty = new Tile();

        public bool IsEmpty { get => !IsActive && Wall == 0 && !HasLiquid && !HasWire; }
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

        public bool IsTileEntity() => IsTileEntity(this.Type);

        public static bool IsChest(int tileType) => TileTypes.IsChest(tileType);
        public static bool IsSign(int tileType) => TileTypes.IsSign(tileType);
        public static bool StopsWalls(ushort type) => TileTypes.StopsWalls(type);
        public static bool IsTileEntity(int tileType) => TileTypes.IsTileEntity(tileType);
    }
}
