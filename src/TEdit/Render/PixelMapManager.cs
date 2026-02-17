using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Microsoft.Xna.Framework;

namespace TEdit.Render;

public enum ChunkStatus : byte { Mixed, AllClear, AllDarkened }

public partial class PixelMapManager : ReactiveObject
{
    const int MaxTextureSize = 256;

    [Reactive]
    private int _tileWidth;

    [Reactive]
    private int _tileHeight;

    [Reactive]
    private int _tilesX;

    [Reactive]
    private int _tilesY;

    [Reactive]
    private Color[][] _colorBuffers;

    [Reactive]
    private bool[] _bufferUpdated;

    public ChunkStatus[] ChunkStates { get; set; }

    public void InitializeBuffers(int worldWidth, int worldHeight)
    {
        // Find maximum buffer size
        for (int h = MaxTextureSize; h >= 100; h--)
        {
            if (worldHeight % h == 0)
            {
                TileHeight = h;
                break;
            }
        }

        for (int w = MaxTextureSize; w >= 100; w--)
        {
            if (worldWidth % w == 0)
            {
                TileWidth = w;
                break;
            }
        }

        if (TileWidth < 100) TileWidth = 100;
        if (TileHeight < 100) TileHeight = 100;

        TilesX = (int)Math.Ceiling((double)worldWidth / TileWidth);
        TilesY = (int)Math.Ceiling((double)worldHeight / TileHeight);

        int tileCount = TilesX * TilesY;
        int tileSize = TileWidth * TileHeight;

        ColorBuffers = new Color[tileCount][];
        BufferUpdated = new bool[tileCount];
        ChunkStates = new ChunkStatus[tileCount];

        for (int x = 0; x < TilesX; x++)
        {
            for (int y = 0; y < TilesY; y++)
            {
                var curBuffer = new Color[tileSize];
                for (int i = 0; i < tileSize; i++)
                {
                    curBuffer[i] = new Color(0, 0, 0, 0);
                }
                ColorBuffers[x + y * TilesX] = curBuffer;
                BufferUpdated[x + y*TilesX] = true;
            }
        }
    }

    public void SetPixelColor(int x, int y, Color color)
    {
        int curTileX = x / TileWidth;
        int curTileY = y / TileHeight;

        int curPixelX = x - (curTileX * TileWidth);
        int curPixelY = y - (curTileY * TileHeight);

        int tileIndex = curTileX + curTileY * TilesX;
        int pixelIndex = curPixelX + curPixelY * TileWidth;

        ColorBuffers[tileIndex][pixelIndex] = color;

        if (!BufferUpdated[tileIndex])
            BufferUpdated[tileIndex] = true;
    }

    public Color GetPixelColor(int x, int y)
    {
        int curTileX = x / TileWidth;
        int curTileY = y / TileHeight;

        int curPixelX = x - (curTileX * TileWidth);
        int curPixelY = y - (curTileY * TileHeight);

        int tileIndex = curTileX + curTileY * TilesX;
        int pixelIndex = curPixelX + curPixelY * TileWidth;

        return ColorBuffers[tileIndex][pixelIndex];
    }
}
