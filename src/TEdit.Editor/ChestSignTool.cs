using TEdit.Configuration;
using TEdit.Terraria;
using TEdit.Terraria.Objects;

namespace TEdit.Editor;

public class ChestSignTool
{
    public static int GetSignId(World world, int X, int Y)
    {
        var uvX = world.Tiles[X, Y].U;
        var uvY = world.Tiles[X, Y].V;
        var type = world.Tiles[X, Y].Type;

        foreach (SignProperty prop in WorldConfiguration.SignProperties)
        {
            if (prop.TileType == type && prop.UV.X == uvX && prop.UV.Y == uvY)
            {
                return prop.SignId;
            }
        }

        return -1;
    }

    public static void SetSignId(World world, int X, int Y, int signId)
    {
        foreach (SignProperty prop in WorldConfiguration.SignProperties)
        {
            if (prop.SignId == signId)
            {
                for (int i = 0; i < 2; ++i)
                {
                    for (int j = 0; j < 2; ++j)
                    {
                        world.Tiles[X + i, Y + j].U = (short)(prop.UV.X + 18 * i);
                        world.Tiles[X + i, Y + j].V = (short)(prop.UV.Y + 18 * j);
                        world.Tiles[X + i, Y + j].Type = prop.TileType;
                    }
                }
                break;
            }
        }
    }

    public static int GetChestId(World world, int X, int Y)
    {
        var uvX = world.Tiles[X, Y].U;
        var uvY = world.Tiles[X, Y].V;
        var type = world.Tiles[X, Y].Type;
        foreach (ChestProperty prop in WorldConfiguration.ChestProperties)
        {
            if (prop.TileType == type && prop.UV.X == uvX && prop.UV.Y == uvY)
            {
                return prop.ChestId;
            }
        }
        return -1;
    }

    public static void SetChestId(World world, int X, int Y, int chestId)
    {
        foreach (ChestProperty prop in WorldConfiguration.ChestProperties)
        {
            if (prop.ChestId == chestId)
            {
                int rowNum = 2, colNum = 2;
                // Chests are 2 * 2, dressers are 2 * 3.
                if (prop.TileType == 88)
                {
                    colNum = 3;
                }
                for (int i = 0; i < colNum; ++i)
                {
                    for (int j = 0; j < rowNum; ++j)
                    {
                        world.Tiles[X + i, Y + j].U = (short)(prop.UV.X + 18 * i);
                        world.Tiles[X + i, Y + j].V = (short)(prop.UV.Y + 18 * j);
                        world.Tiles[X + i, Y + j].Type = prop.TileType;
                    }
                }
                return;
            }
        }
    }
}

