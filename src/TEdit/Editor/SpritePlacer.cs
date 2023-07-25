using System.Collections.Generic;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Render;
using TEdit.Terraria.Objects;
using TEdit.ViewModel;

namespace TEdit.Terraria.Editor;

public static class SpritePlacer
{
    public static void Place(this SpriteItem spriteSub, int destinationX, int destinationY, WorldViewModel wvm)
    {
        ErrorLogging.TelemetryClient?.TrackEvent("PlaceSprite", new Dictionary<string, string> { ["Tile"] = spriteSub.Tile.ToString(), ["UV"] = spriteSub.UV.ToString() });

        if (spriteSub.Tile == (ushort)TileType.ChristmasTree)
        {
            for (int x = 0; x < spriteSub.SizeTiles.X; x++)
            {
                int tilex = x + destinationX;
                for (int y = 0; y < spriteSub.SizeTiles.Y; y++)
                {
                    int tiley = y + destinationY;
                    wvm.UndoManager.SaveTile(tilex, tiley);
                    Tile curtile = wvm.CurrentWorld.Tiles[tilex, tiley];
                    curtile.IsActive = true;
                    curtile.Type = spriteSub.Tile;
                    if (x == 0 && y == 0)
                        curtile.U = 10;
                    else
                        curtile.U = (short)x;
                    curtile.V = (short)y;

                    wvm.UpdateRenderPixel(tilex, tiley);
                    BlendRules.ResetUVCache(wvm, tilex, tiley, spriteSub.SizeTiles.X, spriteSub.SizeTiles.Y);

                }
            }
        }
        else
        {
            for (int x = 0; x < spriteSub.SizeTiles.X; x++)
            {
                Vector2Short[,] tiles = spriteSub.GetTiles();
                int tilex = x + destinationX;
                for (int y = 0; y < spriteSub.SizeTiles.Y; y++)
                {
                    int tiley = y + destinationY;
                    wvm.UndoManager.SaveTile(tilex, tiley);
                    Tile curtile = wvm.CurrentWorld.Tiles[tilex, tiley];
                    curtile.IsActive = true;
                    curtile.Type = spriteSub.Tile;
                    curtile.U = tiles[x, y].X;
                    curtile.V = tiles[x, y].Y;

                    wvm.UpdateRenderPixel(tilex, tiley);
                    BlendRules.ResetUVCache(wvm, tilex, tiley, spriteSub.SizeTiles.X, spriteSub.SizeTiles.Y);

                }
            }
        }
    }

    public static void Place(this SpriteItem spriteSub, int destinationX, int destinationY, ITileData world)
    {
        ErrorLogging.TelemetryClient?.TrackEvent("PlaceSprite", new Dictionary<string, string> { ["Tile"] = spriteSub.Tile.ToString(), ["UV"] = spriteSub.UV.ToString() });

        if (spriteSub.Tile == (ushort)TileType.ChristmasTree)
        {
            for (int x = 0; x < spriteSub.SizeTiles.X; x++)
            {
                int tilex = x + destinationX;
                for (int y = 0; y < spriteSub.SizeTiles.Y; y++)
                {
                    int tiley = y + destinationY;
                    Tile curtile = world.Tiles[tilex, tiley];
                    curtile.IsActive = true;
                    curtile.Type = spriteSub.Tile;
                    if (x == 0 && y == 0)
                        curtile.U = 10;
                    else
                        curtile.U = (short)x;
                    curtile.V = (short)y;

                }
            }
        }
        else
        {
            for (int x = 0; x < spriteSub.SizeTiles.X; x++)
            {
                Vector2Short[,] tiles = spriteSub.GetTiles();
                int tilex = x + destinationX;
                for (int y = 0; y < spriteSub.SizeTiles.Y; y++)
                {
                    int tiley = y + destinationY;
                    Tile curtile = world.Tiles[tilex, tiley];
                    curtile.IsActive = true;
                    curtile.Type = spriteSub.Tile;
                    curtile.U = tiles[x, y].X;
                    curtile.V = tiles[x, y].Y;
                }
            }
        }
    }
}
