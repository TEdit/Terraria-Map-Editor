using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaWorld.Game
{
    public static class TileProperties
    {
        public class TileAttributes
        {
            public bool IsDungeon { get; set; }
            public bool IsFrameImportant { get; set; }
            public bool IsLighted { get; set; }
            public bool IsNoAttach { get; set; }
            public bool IsSolid { get; set; }
            public bool IsStone { get; set; }
            public bool IsTable { get; set; }
            public bool IsWallHouse { get; set; }
        }

        private static bool isInitialized = false;

        public static TileAttributes[] _Tiles;
        public static TileAttributes[] Tiles
        {
            get
            {
                lock (_Tiles)
                {
                    if (!isInitialized)
                        InitializeTileProperties();
                }
                return _Tiles;
            }
        }

        private static void InitializeTileProperties()
        {
            int[] dungeon = new int[] { 41, 43, 44 };
            int[] frameImportant = new int[] { 3, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 20, 21, 24, 26, 27, 28, 29, 31, 33, 34, 35, 36, 42, 50, 55, 61, 71, 72, 73, 74, 77, 78, 79 };
            int[] lighted = new int[] { 0, 1, 2, 6, 7, 8, 9, 10, 22, 23, 25, 30, 32, 37, 38, 39, 40, 41, 43, 44, 45, 46, 47, 48, 51, 52, 53, 56, 57, 58, 59, 60, 62, 63, 64, 65, 66, 67, 68, 70, 75, 76 };
            int[] noAttach = new int[] { 3, 4, 10, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 27, 50 };
            int[] solid = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 19, 22, 23, 25, 30, 37, 38, 39, 40, 41, 43, 44, 45, 46, 47, 48, 53, 54, 56, 57, 58, 59, 60, 63, 64, 65, 66, 67, 68, 70, 75, 76 };
            int[] stone = new int[] { 63, 64, 65, 66, 67, 68 };
            int[] table = new int[] { 14, 18, 19 };
            int[] wallHouse = new int[] { 1, 4, 5, 6, 10, 11, 12 };

            _Tiles = new TileAttributes[byte.MaxValue];

            for (int i = 0; i < byte.MaxValue; i++)
            {
                // Initialize defaults to false
                _Tiles[i] = new TileAttributes()
                {
                    IsDungeon = dungeon.Contains(i),
                    IsFrameImportant = frameImportant.Contains(i),
                    IsLighted = lighted.Contains(i),
                    IsNoAttach = noAttach.Contains(i),
                    IsSolid = solid.Contains(i),
                    IsStone = stone.Contains(i),
                    IsTable = table.Contains(i)
                };
            }

            isInitialized = true;
        }

    }
}
