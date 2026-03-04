using System;

namespace TEdit.Render;

public enum ChunkStatus : byte { Mixed, AllClear, AllDarkened }

/// <summary>
/// Single-channel (byte) overlay buffer for the darken filter.
/// Mirrors PixelMapManager's chunk layout but uses byte[][] (1 byte/pixel)
/// instead of Color[][] (4 bytes/pixel), saving 75% memory.
/// Mask values: 0 = clear (no darkening), 255 = darkened.
/// </summary>
public class FilterOverlayBuffer
{
    const int MaxTextureSize = 256;

    public int TileWidth { get; private set; }
    public int TileHeight { get; private set; }
    public int TilesX { get; private set; }
    public int TilesY { get; private set; }

    public byte[][] MaskBuffers { get; private set; }
    public bool[] BufferUpdated { get; private set; }
    public ChunkStatus[] ChunkStates { get; set; }

    public void InitializeBuffers(int worldWidth, int worldHeight)
    {
        // Find maximum buffer size that divides world evenly (same algorithm as PixelMapManager)
        TileHeight = 100;
        for (int h = MaxTextureSize; h >= 100; h--)
        {
            if (worldHeight % h == 0)
            {
                TileHeight = h;
                break;
            }
        }

        TileWidth = 100;
        for (int w = MaxTextureSize; w >= 100; w--)
        {
            if (worldWidth % w == 0)
            {
                TileWidth = w;
                break;
            }
        }

        TilesX = (int)Math.Ceiling((double)worldWidth / TileWidth);
        TilesY = (int)Math.Ceiling((double)worldHeight / TileHeight);

        int tileCount = TilesX * TilesY;
        int tileSize = TileWidth * TileHeight;

        MaskBuffers = new byte[tileCount][];
        BufferUpdated = new bool[tileCount];
        ChunkStates = new ChunkStatus[tileCount];

        for (int i = 0; i < tileCount; i++)
        {
            MaskBuffers[i] = new byte[tileSize];
            BufferUpdated[i] = true;
        }
    }

    public void SetMask(int x, int y, byte value)
    {
        int curTileX = x / TileWidth;
        int curTileY = y / TileHeight;

        int curPixelX = x - (curTileX * TileWidth);
        int curPixelY = y - (curTileY * TileHeight);

        int tileIndex = curTileX + curTileY * TilesX;
        int pixelIndex = curPixelX + curPixelY * TileWidth;

        MaskBuffers[tileIndex][pixelIndex] = value;

        if (!BufferUpdated[tileIndex])
            BufferUpdated[tileIndex] = true;

        // Reclassify chunk as Mixed if it was uniform (bug fix: per-pixel edits
        // could change a chunk from AllClear/AllDarkened without updating status)
        var status = ChunkStates[tileIndex];
        if (status != ChunkStatus.Mixed)
        {
            if ((status == ChunkStatus.AllClear && value != 0) ||
                (status == ChunkStatus.AllDarkened && value != 255))
            {
                ChunkStates[tileIndex] = ChunkStatus.Mixed;
            }
        }
    }

    public byte GetMask(int x, int y)
    {
        int curTileX = x / TileWidth;
        int curTileY = y / TileHeight;

        int curPixelX = x - (curTileX * TileWidth);
        int curPixelY = y - (curTileY * TileHeight);

        int tileIndex = curTileX + curTileY * TilesX;
        int pixelIndex = curPixelX + curPixelY * TileWidth;

        return MaskBuffers[tileIndex][pixelIndex];
    }
}
