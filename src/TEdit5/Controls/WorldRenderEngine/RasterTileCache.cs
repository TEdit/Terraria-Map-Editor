using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using TEdit.Common;

namespace TEdit5.Controls.WorldRenderEngine;

public class RasterTileCache : IRasterTileCache
{
    private bool _disposedValue;

    private readonly int _pixelsHigh;
    private readonly int _pixelsWide;

    public int TilesX { get; }
    public int TilesY { get; }
    public int TileSize { get; } = 100;

    private RasterTile[] _tiles;

    public RasterTileCache(int pixelsHigh, int pixelsWide, int tileSize = 100)
    {
        TileSize = tileSize;

        _pixelsHigh = pixelsHigh;
        _pixelsWide = pixelsWide;

        TilesX = (int)Math.Ceiling((float)_pixelsWide / TileSize);
        TilesY = (int)Math.Ceiling((float)_pixelsHigh / TileSize);

        _tiles = new RasterTile[TilesX * TilesY];
    }

    public RasterTile? GetTile(int x, int y)
    {
        int tileIndex = TileXYToTileIndex(x, y);

        if (tileIndex >= _tiles.Length) { return null; }

        return _tiles[tileIndex];
    }

    public void SetTile(RasterTile tile, int x, int y)
    {
        int tileIndex = TileXYToTileIndex(x, y);
        if (_tiles == null || _tiles.Length <= tileIndex) { return; }

        var existing = _tiles[tileIndex];
        if (existing?.Bitmap != null)
        {
            existing.Dispose();
        }

        _tiles[tileIndex] = tile;
    }

    private int TileXYToTileIndex(int x, int y) => x + y * TilesX;

    public int PixelToTileIndex(int worldPixelX, int worldPixelY)
    {
        int curTileX = worldPixelX / TileSize;
        int curTileY = worldPixelY / TileSize;

        int tileIndex = curTileX + curTileY * TilesX;

        return tileIndex;
    }

    public void SetPixelDirty(int x, int y)
    {
        (int tileIndex, int tilePixelX, int tilePixelY) = PixelToTilePixelIndex(x, y);
        _tiles[tileIndex].IsDirty = true;
    }

    private (int tileIndex, int tilePixelX, int tilePixelY) PixelToTilePixelIndex(int worldPixelX, int worldPixelY)
    {
        int curTileX = worldPixelX / TileSize;
        int curTileY = worldPixelY / TileSize;

        int tilePixelX = worldPixelX - (curTileX * TileSize);
        int tilePixelY = worldPixelY - (curTileY * TileSize);

        int tileIndex = curTileX + curTileY * TilesX;

        return (tileIndex, tilePixelX, tilePixelY);
    }

    public void SetPixelColor(int x, int y, SKColor color)
    {
        (int tileIndex, int tilePixelX, int tilePixelY) = PixelToTilePixelIndex(x, y);

        _tiles[tileIndex]?.Bitmap?.SetPixel(tilePixelX, tilePixelY, color);
    }

    public SKColor? GetPixelColor(int x, int y)
    {
        (int tileIndex, int tilePixelX, int tilePixelY) = PixelToTilePixelIndex(x, y);

        return _tiles[tileIndex]?.Bitmap?.GetPixel(tilePixelX, tilePixelY);
    }

    public void Clear()
    {
        var tiles = _tiles.ToList();
        _tiles = new RasterTile[TilesX * TilesY];
        foreach (var tile in tiles)
        {
            tile.IsDirty = true;
            tile?.Dispose();
        }
    }


    #region Dispose
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                var tiles = _tiles.ToList();
                _tiles = new RasterTile[0];
                foreach (var tile in tiles)
                {
                    tile?.Dispose();
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~RasterTileCache()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
