using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaMapEditor.Renderer
{
    public static class Constants
    {
        public static System.Drawing.Color DIRT = System.Drawing.Color.FromArgb(175, 131, 101);
        public static System.Drawing.Color STONE = System.Drawing.Color.FromArgb(128, 128, 128);
        public static System.Drawing.Color GRASS = System.Drawing.Color.FromArgb(28, 216, 94);
        public static System.Drawing.Color PLANTS = System.Drawing.Color.FromArgb(13, 101, 36);
        public static System.Drawing.Color LIGHT_SOURCE = System.Drawing.Color.FromArgb(253, 62, 3);
        public static System.Drawing.Color IRON = System.Drawing.Color.FromArgb(189, 159, 139);
        public static System.Drawing.Color COPPER = System.Drawing.Color.FromArgb(255, 149, 50);
        public static System.Drawing.Color GOLD = System.Drawing.Color.FromArgb(185, 164, 23);
        public static System.Drawing.Color WOOD = System.Drawing.Color.FromArgb(86, 62, 44);
        public static System.Drawing.Color SILVER = System.Drawing.Color.FromArgb(217, 223, 223);
        public static System.Drawing.Color DECORATIVE = System.Drawing.Color.FromArgb(0, 255, 242);
        public static System.Drawing.Color IMPORTANT = System.Drawing.Color.FromArgb(255, 0, 0);
        public static System.Drawing.Color CORRUPTION_STONE = System.Drawing.Color.FromArgb(98, 95, 167);
        public static System.Drawing.Color CORRUPTION_GRASS = System.Drawing.Color.FromArgb(141, 137, 223);
        public static System.Drawing.Color CORRUPTION_STONE2 = System.Drawing.Color.FromArgb(75, 74, 130);
        public static System.Drawing.Color CORRUPTION_VINES = System.Drawing.Color.FromArgb(122, 97, 143);
        public static System.Drawing.Color BLOCK = System.Drawing.Color.FromArgb(178, 0, 255);
        public static System.Drawing.Color METEORITE = System.Drawing.Color.FromArgb(223, 159, 137);
        public static System.Drawing.Color CLAY = System.Drawing.Color.FromArgb(216, 115, 101);
        public static System.Drawing.Color DUNGEON = System.Drawing.Color.FromArgb(140, 0, 255);
        public static System.Drawing.Color SPIKES = System.Drawing.Color.FromArgb(109, 109, 109);
        public static System.Drawing.Color WEB = System.Drawing.Color.FromArgb(255, 255, 255);
        public static System.Drawing.Color SAND = System.Drawing.Color.FromArgb(255, 218, 56);
        public static System.Drawing.Color OBSIDIAN = System.Drawing.Color.FromArgb(87, 81, 173);
        public static System.Drawing.Color ASH = System.Drawing.Color.FromArgb(68, 68, 76);
        public static System.Drawing.Color HELLSTONE = System.Drawing.Color.FromArgb(102, 34, 34);
        public static System.Drawing.Color MUD = System.Drawing.Color.FromArgb(92, 68, 73);
        public static System.Drawing.Color UNDERGROUNDJUNGLE_GRASS = System.Drawing.Color.FromArgb(143, 215, 29);
        public static System.Drawing.Color UNDERGROUNDJUNGLE_PLANTS = System.Drawing.Color.FromArgb(143, 215, 29);
        public static System.Drawing.Color UNDERGROUNDJUNGLE_VINES = System.Drawing.Color.FromArgb(138, 206, 28);
        public static System.Drawing.Color UNDERGROUNDJUNGLE_THORNS = System.Drawing.Color.FromArgb(94, 48, 55);
        public static System.Drawing.Color GEMS = System.Drawing.Color.FromArgb(42, 130, 250);
        public static System.Drawing.Color UNDERGROUNDMUSHROOM_GRASS = System.Drawing.Color.FromArgb(93, 127, 255);
        public static System.Drawing.Color UNDERGROUNDMUSHROOM_PLANTS = System.Drawing.Color.FromArgb(177, 174, 131);
        public static System.Drawing.Color UNDERGROUNDMUSHROOM_TREES = System.Drawing.Color.FromArgb(150, 143, 110);
        public static System.Drawing.Color LAVA = System.Drawing.Color.FromArgb(255, 72, 0);
        public static System.Drawing.Color WATER = System.Drawing.Color.FromArgb(0, 12, 255);
        public static System.Drawing.Color SKY = System.Drawing.Color.FromArgb(155, 209, 255);
        public static System.Drawing.Color WALL_STONE = System.Drawing.Color.FromArgb(66, 66, 66);
        public static System.Drawing.Color WALL_DIRT = System.Drawing.Color.FromArgb(88, 61, 46);
        public static System.Drawing.Color WALL_STONE2 = System.Drawing.Color.FromArgb(61, 58, 78);
        public static System.Drawing.Color WALL_WOOD = System.Drawing.Color.FromArgb(73, 51, 36);
        public static System.Drawing.Color WALL_BRICK = System.Drawing.Color.FromArgb(60, 60, 60);
        public static System.Drawing.Color WALL_BACKGROUND = System.Drawing.Color.FromArgb(50, 50, 60);
        public static System.Drawing.Color UNKNOWN = System.Drawing.Color.Magenta;

        public static List<TileProperties> GetWallColors()
        {
            return new List<TileProperties>()
                {
                    new TileProperties(0, Constants.SKY                       ,"Sky"),
                    new TileProperties(1, Constants.WALL_STONE                ,"WallStone"),
                    new TileProperties(2, Constants.WALL_DIRT                 ,"WallDirt"),
                    new TileProperties(3, Constants.WALL_STONE2               ,"WallStone2"),
                    new TileProperties(4, Constants.WALL_WOOD                 ,"WallWood"),
                    new TileProperties(5, Constants.WALL_BRICK                ,"WallBrick"),
                    new TileProperties(6, Constants.WALL_BRICK                ,"WallRed"),
                    new TileProperties(7, Constants.WALL_BRICK                ,"WallBlue"),
                    new TileProperties(8, Constants.WALL_BRICK                ,"WallGreen"),
                    new TileProperties(9, Constants.WALL_BRICK                ,"WallPink"),
                    new TileProperties(10, Constants.WALL_BRICK               ,"WallGold"),
                    new TileProperties(11, Constants.WALL_BRICK               ,"WallSilver"),
                    new TileProperties(12, Constants.WALL_BRICK               ,"WallCopper"),
                    new TileProperties(13, Constants.WALL_BRICK               ,"WallHellstone"),
                    new TileProperties(14, Constants.WALL_BACKGROUND          ,"WallHellstone"),
                };
        }

        public static List<TileProperties> GetLiquidColors()
        {
            return new List<TileProperties>
                {
                    new TileProperties(1, Constants.WATER , "Water"),
                    new TileProperties(2, Constants.LAVA , "Lava")
                };
        }

        public static List<TileProperties> GetTileColors()
        {
            return new List<TileProperties>()
                {
                    new TileProperties(0 ,Constants.DIRT, "Dirt"),
                    new TileProperties(1 ,Constants.STONE, "Stone"),
                    new TileProperties(2 ,Constants.GRASS, "Grass"),
                    new TileProperties(3 ,Constants.PLANTS, "Plants"),
                    new TileProperties(4 ,Constants.LIGHT_SOURCE, "Torches"),
                    new TileProperties(5 ,Constants.WOOD, "Trees"),
                    new TileProperties(6 ,Constants.IRON, "Iron"),
                    new TileProperties(7 ,Constants.COPPER, "Copper"),
                    new TileProperties(8 ,Constants.GOLD, "Gold"),
                    new TileProperties(9 ,Constants.SILVER, "Silver"),
                    new TileProperties(10,Constants.DECORATIVE, "Door1"),
                    new TileProperties(11,Constants.DECORATIVE, "Door2"),
                    new TileProperties(12,Constants.IMPORTANT, "HeartStone"),
                    new TileProperties(13,Constants.DECORATIVE, "Bottles"),
                    new TileProperties(14,Constants.DECORATIVE, "Table"),
                    new TileProperties(15,Constants.DECORATIVE, "Chair"),
                    new TileProperties(16,Constants.DECORATIVE, "Anvil"),
                    new TileProperties(17,Constants.DECORATIVE, "Furnance"),
                    new TileProperties(18,Constants.DECORATIVE, "CraftingTable"),
                    new TileProperties(19,Constants.WOOD, "WoodenPlatform"),
                    new TileProperties(20,Constants.PLANTS, "PlantsDecorative"),
                    new TileProperties(21,Constants.IMPORTANT, "Chest"),
                    new TileProperties(22,Constants.CORRUPTION_STONE, "CorruptionStone1"),
                    new TileProperties(23,Constants.CORRUPTION_GRASS, "CorruptionGrass"),
                    new TileProperties(24,Constants.CORRUPTION_GRASS, "CorruptionPlants"),
                    new TileProperties(25,Constants.CORRUPTION_STONE2, "CorruptionStone2"),
                    new TileProperties(26,Constants.IMPORTANT, "Altar"),
                    new TileProperties(27,Constants.PLANTS, "Sunflower"),
                    new TileProperties(28,Constants.IMPORTANT, "Pot"),
                    new TileProperties(29,Constants.DECORATIVE, "PiggyBank"),
                    new TileProperties(30,Constants.BLOCK, "BlockWood"),
                    new TileProperties(31,Constants.IMPORTANT, "ShadowOrb"),
                    new TileProperties(32,Constants.CORRUPTION_VINES, "CorruptionVines"),
                    new TileProperties(33,Constants.LIGHT_SOURCE, "Candle"),
                    new TileProperties(34,Constants.LIGHT_SOURCE, "ChandlerCopper"),
                    new TileProperties(35,Constants.LIGHT_SOURCE, "ChandlerSilver"),
                    new TileProperties(36,Constants.LIGHT_SOURCE, "ChandlerGold"),
                    new TileProperties(37,Constants.METEORITE, "Meterorite"),
                    new TileProperties(38,Constants.BLOCK, "BlockStone"),
                    new TileProperties(39,Constants.BLOCK, "BlockRedStone"),
                    new TileProperties(40,Constants.CLAY, "Clay"),
                    new TileProperties(41,Constants.DUNGEON, "BlockBlueStone"),
                    new TileProperties(42,Constants.LIGHT_SOURCE, "LightGlobe"),
                    new TileProperties(43,Constants.DUNGEON, "BlockGreenStone"),
                    new TileProperties(44,Constants.DUNGEON, "BlockPinkStone"),
                    new TileProperties(45,Constants.BLOCK, "BlockGold"),
                    new TileProperties(46,Constants.BLOCK, "BlockSilver"),
                    new TileProperties(47,Constants.BLOCK, "BlockCopper"),
                    new TileProperties(48,Constants.SPIKES, "Spikes"),
                    new TileProperties(49,Constants.LIGHT_SOURCE, "CandleBlue"),
                    new TileProperties(50,Constants.DECORATIVE, "Books"),
                    new TileProperties(51,Constants.WEB, "Web"),
                    new TileProperties(52,Constants.PLANTS, "Vines"),
                    new TileProperties(53,Constants.SAND, "Sand"),
                    new TileProperties(54,Constants.DECORATIVE, "Glass"),
                    new TileProperties(55,Constants.DECORATIVE, "Signs"),
                    new TileProperties(56,Constants.OBSIDIAN, "Obsidian"),
                    new TileProperties(57,Constants.ASH, "Ash"),
                    new TileProperties(58,Constants.HELLSTONE, "Hellstone"),
                    new TileProperties(59,Constants.MUD, "Mud"),
                    new TileProperties(60,Constants.UNDERGROUNDJUNGLE_GRASS, "UndergroundJungleGrass"),
                    new TileProperties(61,Constants.UNDERGROUNDJUNGLE_PLANTS, "UndergroundJunglePlants"),
                    new TileProperties(62,Constants.UNDERGROUNDJUNGLE_VINES, "UndergroundJungleVines"),
                    new TileProperties(63,Constants.GEMS, "GemSapphire"),
                    new TileProperties(64,Constants.GEMS, "GemRuby"),
                    new TileProperties(65,Constants.GEMS, "GemEmerald"),
                    new TileProperties(66,Constants.GEMS, "GemTopaz"),
                    new TileProperties(67,Constants.GEMS, "GemAmethyst"),
                    new TileProperties(68,Constants.GEMS, "GemDiamond"),
                    new TileProperties(69,Constants.UNDERGROUNDJUNGLE_THORNS, "UndergroundJungleThorns"),
                    new TileProperties(70,Constants.UNDERGROUNDMUSHROOM_GRASS, "UndergroundMushroomGrass"),
                    new TileProperties(71,Constants.UNDERGROUNDMUSHROOM_PLANTS, "UndergroundMushroomPlants"),
                    new TileProperties(72,Constants.UNDERGROUNDMUSHROOM_TREES, "UndergroundMushroomTrees"),
                    new TileProperties(73,Constants.PLANTS, "Plants2"),
                    new TileProperties(74,Constants.PLANTS, "Plants3"),
                    new TileProperties(75,Constants.BLOCK, "BlockObsidian"),
                    new TileProperties(76,Constants.BLOCK, "BlockHellstone"),
                    new TileProperties(77,Constants.IMPORTANT, "UnderworldFurnance"),
                    new TileProperties(78,Constants.DECORATIVE, "DecorativePot"),
                    new TileProperties(79,Constants.DECORATIVE, "Bed"),
                    new TileProperties(80,Constants.UNKNOWN, "Unknown")
                };
        }

    }

}
