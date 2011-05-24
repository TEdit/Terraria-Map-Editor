using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Game
{
    public static class TileProperties
    {
        public static bool IsInitialized = false;
        public static bool[] IsSolid { get; private set; }
        public static bool[] IsLight { get; private set; }
        public static bool[] IsNoAttach { get; private set; }
        public static bool[] IsNoFail { get; private set; }
        public static bool[] IsSolidTop { get; private set; }
        public static bool[] IsStone { get; private set; }
        public static bool[] IsDungeon { get; private set; }
        public static bool[] IsFrameImportant { get; private set; }
        public static bool[] IsTable { get; private set; }
        public static bool[] IsWaterDeath { get; private set; }
        public static bool[] IsLavaDeath { get; private set; }
        public static bool[] IsWallHouse { get; private set; }

        public static void InitializeTileProperties()
        {

            IsSolid = new bool[byte.MaxValue];
            IsLight = new bool[byte.MaxValue];
            IsNoAttach = new bool[byte.MaxValue];
            IsNoFail = new bool[byte.MaxValue];
            IsSolidTop = new bool[byte.MaxValue];
            IsStone = new bool[byte.MaxValue];
            IsDungeon = new bool[byte.MaxValue];
            IsFrameImportant = new bool[byte.MaxValue];
            IsTable = new bool[byte.MaxValue];
            IsWaterDeath = new bool[byte.MaxValue];
            IsLavaDeath = new bool[byte.MaxValue];
            IsWallHouse = new bool[byte.MaxValue];

            for (int i = 0; i < byte.MaxValue; i++)
            {
                // Initialize defaults to false
                IsSolid[i] = false;
                IsLight[i] = false;
                IsNoAttach[i] = false;
                IsNoFail[i] = false;
                IsSolidTop[i] = false;
                IsStone[i] = false;
                IsDungeon[i] = false;
                IsFrameImportant[i] = false;
                IsTable[i] = false;
                IsWaterDeath[i] = false;
                IsLavaDeath[i] = false;
                IsWallHouse[i] = false;
            }


            IsDungeon[41] = true;
            IsDungeon[43] = true;
            IsDungeon[44] = true;

            IsFrameImportant[3] = true;
            IsFrameImportant[5] = true;
            IsFrameImportant[10] = true;
            IsFrameImportant[11] = true;
            IsFrameImportant[12] = true;
            IsFrameImportant[13] = true;
            IsFrameImportant[14] = true;
            IsFrameImportant[15] = true;
            IsFrameImportant[16] = true;
            IsFrameImportant[17] = true;
            IsFrameImportant[18] = true;
            IsFrameImportant[20] = true;
            IsFrameImportant[21] = true;
            IsFrameImportant[24] = true;
            IsFrameImportant[26] = true;
            IsFrameImportant[27] = true;
            IsFrameImportant[28] = true;
            IsFrameImportant[29] = true;
            IsFrameImportant[31] = true;
            IsFrameImportant[33] = true;
            IsFrameImportant[34] = true;
            IsFrameImportant[35] = true;
            IsFrameImportant[36] = true;
            IsFrameImportant[42] = true;
            IsFrameImportant[50] = true;
            IsFrameImportant[55] = true;
            IsFrameImportant[61] = true;
            IsFrameImportant[71] = true;
            IsFrameImportant[72] = true;
            IsFrameImportant[73] = true;
            IsFrameImportant[74] = true;
            IsFrameImportant[77] = true;
            IsFrameImportant[78] = true;
            IsFrameImportant[79] = true;

            IsLavaDeath[3] = true;
            IsLavaDeath[5] = true;
            IsLavaDeath[10] = true;
            IsLavaDeath[11] = true;
            IsLavaDeath[12] = true;
            IsLavaDeath[13] = true;
            IsLavaDeath[14] = true;
            IsLavaDeath[15] = true;
            IsLavaDeath[16] = true;
            IsLavaDeath[17] = true;
            IsLavaDeath[18] = true;
            IsLavaDeath[19] = true;
            IsLavaDeath[20] = true;
            IsLavaDeath[27] = true;
            IsLavaDeath[28] = true;
            IsLavaDeath[29] = true;
            IsLavaDeath[32] = true;
            IsLavaDeath[33] = true;
            IsLavaDeath[34] = true;
            IsLavaDeath[35] = true;
            IsLavaDeath[36] = true;
            IsLavaDeath[42] = true;
            IsLavaDeath[49] = true;
            IsLavaDeath[50] = true;
            IsLavaDeath[52] = true;
            IsLavaDeath[55] = true;
            IsLavaDeath[61] = true;
            IsLavaDeath[62] = true;
            IsLavaDeath[69] = true;
            IsLavaDeath[71] = true;
            IsLavaDeath[72] = true;
            IsLavaDeath[73] = true;
            IsLavaDeath[74] = true;
            IsLavaDeath[78] = true;
            IsLavaDeath[79] = true;

            IsLight[0] = true;
            IsLight[1] = true;
            IsLight[2] = true;
            IsLight[6] = true;
            IsLight[7] = true;
            IsLight[8] = true;
            IsLight[9] = true;
            IsLight[10] = true;
            IsLight[22] = true;
            IsLight[23] = true;
            IsLight[25] = true;
            IsLight[30] = true;
            IsLight[32] = true;
            IsLight[37] = true;
            IsLight[38] = true;
            IsLight[39] = true;
            IsLight[40] = true;
            IsLight[41] = true;
            IsLight[43] = true;
            IsLight[44] = true;
            IsLight[45] = true;
            IsLight[46] = true;
            IsLight[47] = true;
            IsLight[48] = true;
            IsLight[51] = true;
            IsLight[52] = true;
            IsLight[53] = true;
            IsLight[56] = true;
            IsLight[57] = true;
            IsLight[58] = true;
            IsLight[59] = true;
            IsLight[60] = true;
            IsLight[62] = true;
            IsLight[63] = true;
            IsLight[64] = true;
            IsLight[65] = true;
            IsLight[66] = true;
            IsLight[67] = true;
            IsLight[68] = true;
            IsLight[70] = true;
            IsLight[75] = true;
            IsLight[76] = true;

            IsNoAttach[3] = true;
            IsNoAttach[4] = true;
            IsNoAttach[10] = true;
            IsNoAttach[13] = true;
            IsNoAttach[14] = true;
            IsNoAttach[15] = true;
            IsNoAttach[16] = true;
            IsNoAttach[17] = true;
            IsNoAttach[18] = true;
            IsNoAttach[19] = true;
            IsNoAttach[19] = true;
            IsNoAttach[20] = true;
            IsNoAttach[21] = true;
            IsNoAttach[27] = true;
            IsNoAttach[50] = true;

            IsNoFail[3] = true;
            IsNoFail[4] = true;
            IsNoFail[24] = true;
            IsNoFail[32] = true;
            IsNoFail[50] = true;

            IsSolid[0] = true;
            IsSolid[1] = true;
            IsSolid[2] = true;
            IsSolid[3] = false;
            IsSolid[4] = false;
            IsSolid[5] = false;
            IsSolid[6] = true;
            IsSolid[7] = true;
            IsSolid[8] = true;
            IsSolid[9] = true;
            IsSolid[10] = true;
            IsSolid[11] = false;
            IsSolid[19] = true;
            IsSolid[22] = true;
            IsSolid[23] = true;
            IsSolid[25] = true;
            IsSolid[30] = true;
            IsSolid[37] = true;
            IsSolid[38] = true;
            IsSolid[39] = true;
            IsSolid[40] = true;
            IsSolid[41] = true;
            IsSolid[43] = true;
            IsSolid[44] = true;
            IsSolid[45] = true;
            IsSolid[46] = true;
            IsSolid[47] = true;
            IsSolid[48] = true;
            IsSolid[53] = true;
            IsSolid[54] = true;
            IsSolid[56] = true;
            IsSolid[57] = true;
            IsSolid[58] = true;
            IsSolid[59] = true;
            IsSolid[60] = true;
            IsSolid[63] = true;
            IsSolid[64] = true;
            IsSolid[65] = true;
            IsSolid[66] = true;
            IsSolid[67] = true;
            IsSolid[68] = true;
            IsSolid[70] = true;
            IsSolid[75] = true;
            IsSolid[76] = true;

            IsSolidTop[14] = true;
            IsSolidTop[16] = true;
            IsSolidTop[18] = true;
            IsSolidTop[19] = true;

            IsStone[63] = true;
            IsStone[64] = true;
            IsStone[65] = true;
            IsStone[66] = true;
            IsStone[67] = true;
            IsStone[68] = true;
            IsTable[14] = true;
            IsTable[18] = true;
            IsTable[19] = true;

            IsWallHouse[1] = true;
            IsWallHouse[4] = true;
            IsWallHouse[5] = true;
            IsWallHouse[6] = true;
            IsWallHouse[10] = true;
            IsWallHouse[11] = true;
            IsWallHouse[12] = true;

            IsWaterDeath[4] = true;
            IsWaterDeath[51] = true;

            IsInitialized = true;
        }

    }
}
