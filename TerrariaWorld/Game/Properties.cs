using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Game
{
    public static class Properties
    {
        public static bool IsInitialized = false;
        public static bool[] tileSolid { get; private set; }
        public static bool[] tileBlockLight { get; private set; }
        public static bool[] tileNoAttach { get; private set; }
        public static bool[] tileNoFail { get; private set; }
        public static bool[] tileSolidTop { get; private set; }
        public static bool[] tileStone { get; private set; }
        public static bool[] tileDungeon { get; private set; }
        public static bool[] tileFrameImportant { get; private set; }
        public static bool[] tileTable { get; private set; }
        public static bool[] tileWaterDeath { get; private set; }
        public static bool[] tileLavaDeath { get; private set; }
        public static bool[] wallHouse { get; private set; }

        public static void InitializeTileProperties()
        {
            
            tileSolid = new bool[255];
            tileBlockLight = new bool[255];
            tileNoAttach = new bool[255];
            tileNoFail= new bool[255];
            tileSolidTop = new bool[255];
            tileStone= new bool[255];
            tileDungeon= new bool[255];
            tileFrameImportant = new bool[255];
            tileTable = new bool[255];
            tileWaterDeath = new bool[255];
            tileLavaDeath = new bool[255];
            wallHouse = new bool[255];

            tileSolid[0] = true;
            tileBlockLight[0] = true;
            tileSolid[1] = true;
            tileBlockLight[1] = true;
            tileSolid[2] = true;
            tileBlockLight[2] = true;
            tileSolid[3] = false;
            tileNoAttach[3] = true;
            tileNoFail[3] = true;
            tileSolid[4] = false;
            tileNoAttach[4] = true;
            tileNoFail[4] = true;
            tileNoFail[0x18] = true;
            tileSolid[5] = false;
            tileSolid[6] = true;
            tileBlockLight[6] = true;
            tileSolid[7] = true;
            tileBlockLight[7] = true;
            tileSolid[8] = true;
            tileBlockLight[8] = true;
            tileSolid[9] = true;
            tileBlockLight[9] = true;
            tileBlockLight[10] = true;
            tileSolid[10] = true;
            tileNoAttach[10] = true;
            tileBlockLight[10] = true;
            tileSolid[11] = false;
            tileSolidTop[0x13] = true;
            tileSolid[0x13] = true;
            tileSolid[0x16] = true;
            tileSolid[0x17] = true;
            tileSolid[0x19] = true;
            tileSolid[30] = true;
            tileNoFail[0x20] = true;
            tileBlockLight[0x20] = true;
            tileSolid[0x25] = true;
            tileBlockLight[0x25] = true;
            tileSolid[0x26] = true;
            tileBlockLight[0x26] = true;
            tileSolid[0x27] = true;
            tileBlockLight[0x27] = true;
            tileSolid[40] = true;
            tileBlockLight[40] = true;
            tileSolid[0x29] = true;
            tileBlockLight[0x29] = true;
            tileSolid[0x2b] = true;
            tileBlockLight[0x2b] = true;
            tileSolid[0x2c] = true;
            tileBlockLight[0x2c] = true;
            tileSolid[0x2d] = true;
            tileBlockLight[0x2d] = true;
            tileSolid[0x2e] = true;
            tileBlockLight[0x2e] = true;
            tileSolid[0x2f] = true;
            tileBlockLight[0x2f] = true;
            tileSolid[0x30] = true;
            tileBlockLight[0x30] = true;
            tileSolid[0x35] = true;
            tileBlockLight[0x35] = true;
            tileSolid[0x36] = true;
            tileBlockLight[0x34] = true;
            tileSolid[0x38] = true;
            tileBlockLight[0x38] = true;
            tileSolid[0x39] = true;
            tileBlockLight[0x39] = true;
            tileSolid[0x3a] = true;
            tileBlockLight[0x3a] = true;
            tileSolid[0x3b] = true;
            tileBlockLight[0x3b] = true;
            tileSolid[60] = true;
            tileBlockLight[60] = true;
            tileSolid[0x3f] = true;
            tileBlockLight[0x3f] = true;
            tileStone[0x3f] = true;
            tileSolid[0x40] = true;
            tileBlockLight[0x40] = true;
            tileStone[0x40] = true;
            tileSolid[0x41] = true;
            tileBlockLight[0x41] = true;
            tileStone[0x41] = true;
            tileSolid[0x42] = true;
            tileBlockLight[0x42] = true;
            tileStone[0x42] = true;
            tileSolid[0x43] = true;
            tileBlockLight[0x43] = true;
            tileStone[0x43] = true;
            tileSolid[0x44] = true;
            tileBlockLight[0x44] = true;
            tileStone[0x44] = true;
            tileSolid[0x4b] = true;
            tileBlockLight[0x4b] = true;
            tileSolid[0x4c] = true;
            tileBlockLight[0x4c] = true;
            tileSolid[70] = true;
            tileBlockLight[70] = true;
            tileBlockLight[0x33] = true;
            tileNoFail[50] = true;
            tileNoAttach[50] = true;
            tileDungeon[0x29] = true;
            tileDungeon[0x2b] = true;
            tileDungeon[0x2c] = true;
            tileBlockLight[30] = true;
            tileBlockLight[0x19] = true;
            tileBlockLight[0x17] = true;
            tileBlockLight[0x16] = true;
            tileBlockLight[0x3e] = true;
            tileSolidTop[0x12] = true;
            tileSolidTop[14] = true;
            tileSolidTop[0x10] = true;
            tileNoAttach[20] = true;
            tileNoAttach[0x13] = true;
            tileNoAttach[13] = true;
            tileNoAttach[14] = true;
            tileNoAttach[15] = true;
            tileNoAttach[0x10] = true;
            tileNoAttach[0x11] = true;
            tileNoAttach[0x12] = true;
            tileNoAttach[0x13] = true;
            tileNoAttach[0x15] = true;
            tileNoAttach[0x1b] = true;
            tileFrameImportant[3] = true;
            tileFrameImportant[5] = true;
            tileFrameImportant[10] = true;
            tileFrameImportant[11] = true;
            tileFrameImportant[12] = true;
            tileFrameImportant[13] = true;
            tileFrameImportant[14] = true;
            tileFrameImportant[15] = true;
            tileFrameImportant[0x10] = true;
            tileFrameImportant[0x11] = true;
            tileFrameImportant[0x12] = true;
            tileFrameImportant[20] = true;
            tileFrameImportant[0x15] = true;
            tileFrameImportant[0x18] = true;
            tileFrameImportant[0x1a] = true;
            tileFrameImportant[0x1b] = true;
            tileFrameImportant[0x1c] = true;
            tileFrameImportant[0x1d] = true;
            tileFrameImportant[0x1f] = true;
            tileFrameImportant[0x21] = true;
            tileFrameImportant[0x22] = true;
            tileFrameImportant[0x23] = true;
            tileFrameImportant[0x24] = true;
            tileFrameImportant[0x2a] = true;
            tileFrameImportant[50] = true;
            tileFrameImportant[0x37] = true;
            tileFrameImportant[0x3d] = true;
            tileFrameImportant[0x47] = true;
            tileFrameImportant[0x48] = true;
            tileFrameImportant[0x49] = true;
            tileFrameImportant[0x4a] = true;
            tileFrameImportant[0x4d] = true;
            tileFrameImportant[0x4e] = true;
            tileFrameImportant[0x4f] = true;
            tileTable[14] = true;
            tileTable[0x12] = true;
            tileTable[0x13] = true;
            tileWaterDeath[4] = true;
            tileWaterDeath[0x33] = true;
            tileLavaDeath[3] = true;
            tileLavaDeath[5] = true;
            tileLavaDeath[10] = true;
            tileLavaDeath[11] = true;
            tileLavaDeath[12] = true;
            tileLavaDeath[13] = true;
            tileLavaDeath[14] = true;
            tileLavaDeath[15] = true;
            tileLavaDeath[0x10] = true;
            tileLavaDeath[0x11] = true;
            tileLavaDeath[0x12] = true;
            tileLavaDeath[0x13] = true;
            tileLavaDeath[20] = true;
            tileLavaDeath[0x1b] = true;
            tileLavaDeath[0x1c] = true;
            tileLavaDeath[0x1d] = true;
            tileLavaDeath[0x20] = true;
            tileLavaDeath[0x21] = true;
            tileLavaDeath[0x22] = true;
            tileLavaDeath[0x23] = true;
            tileLavaDeath[0x24] = true;
            tileLavaDeath[0x2a] = true;
            tileLavaDeath[0x31] = true;
            tileLavaDeath[50] = true;
            tileLavaDeath[0x34] = true;
            tileLavaDeath[0x37] = true;
            tileLavaDeath[0x3d] = true;
            tileLavaDeath[0x3e] = true;
            tileLavaDeath[0x45] = true;
            tileLavaDeath[0x47] = true;
            tileLavaDeath[0x48] = true;
            tileLavaDeath[0x49] = true;
            tileLavaDeath[0x4a] = true;
            tileLavaDeath[0x4e] = true;
            tileLavaDeath[0x4f] = true;
            wallHouse[1] = true;
            wallHouse[4] = true;
            wallHouse[5] = true;
            wallHouse[6] = true;
            wallHouse[10] = true;
            wallHouse[11] = true;
            wallHouse[12] = true;

            IsInitialized = true;
        }

        //public static List<string> Items
        //{
            
        //}
    }
}
