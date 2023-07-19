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

        public static bool StopsWalls(ushort type)
        {
            return type == (ushort)TileType.DoorClosed ||
                   type == (ushort)TileType.DoorOpen ||
                   type == (ushort)TileType.TrapDoor ||
                   type == (ushort)TileType.TrapDoorOpen ||
                   type == (ushort)TileType.TallGate ||
                   type == (ushort)TileType.DoorClosed ||
                   type == (ushort)TileType.TallGateClosed;
        }

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

        public bool IsTileEntity()
        {
            return Tile.IsTileEntity(this.Type);
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
    }
}
