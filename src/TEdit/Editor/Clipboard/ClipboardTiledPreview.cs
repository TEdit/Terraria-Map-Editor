using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TEdit.Editor.Clipboard;
using TEdit.Render;

namespace TEdit.Editor.Clipboard;

public class ClipboardTiledPreview : IDisposable
{
    public const int MaxTileSize = 4096;

    private readonly Texture2D[,] _tiles;

    public int TilesX { get; }
    public int TilesY { get; }
    public int BufferWidth { get; }
    public int BufferHeight { get; }

    private ClipboardTiledPreview(Texture2D[,] tiles, int tilesX, int tilesY, int bufferWidth, int bufferHeight)
    {
        _tiles = tiles;
        TilesX = tilesX;
        TilesY = tilesY;
        BufferWidth = bufferWidth;
        BufferHeight = bufferHeight;
    }

    public Texture2D GetTile(int tx, int ty) => _tiles[tx, ty];

    public static ClipboardTiledPreview Create(ClipboardBuffer buffer, GraphicsDevice device)
    {
        int w = buffer.Size.X;
        int h = buffer.Size.Y;
        int tilesX = (w + MaxTileSize - 1) / MaxTileSize;
        int tilesY = (h + MaxTileSize - 1) / MaxTileSize;

        var tiles = new Texture2D[tilesX, tilesY];

        for (int tx = 0; tx < tilesX; tx++)
        {
            int startX = tx * MaxTileSize;
            int tileW = Math.Min(MaxTileSize, w - startX);

            for (int ty = 0; ty < tilesY; ty++)
            {
                int startY = ty * MaxTileSize;
                int tileH = Math.Min(MaxTileSize, h - startY);

                var pixels = new Color[tileW * tileH];

                for (int x = 0; x < tileW; x++)
                {
                    for (int y = 0; y < tileH; y++)
                    {
                        pixels[y * tileW + x] = PixelMap.GetTileColor(
                            buffer.Tiles[startX + x, startY + y],
                            Color.Transparent);
                    }
                }

                var tex = new Texture2D(device, tileW, tileH);
                tex.SetData(pixels);
                tiles[tx, ty] = tex;
            }
        }

        return new ClipboardTiledPreview(tiles, tilesX, tilesY, w, h);
    }

    public void Dispose()
    {
        for (int tx = 0; tx < TilesX; tx++)
            for (int ty = 0; ty < TilesY; ty++)
                _tiles[tx, ty]?.Dispose();
    }
}
