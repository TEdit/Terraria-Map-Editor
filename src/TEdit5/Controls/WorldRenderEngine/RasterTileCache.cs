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
    public int TileSizeX { get; } = 200;
    public int TileSizeY { get; } = 150;

    private RasterTile[] _tiles;

    public RasterTileCache(int pixelsHigh, int pixelsWide, int tileSizeX = 200, int tileSizeY = 150)
    {
        TileSizeX = tileSizeX;
        TileSizeY = tileSizeY;

        _pixelsHigh = pixelsHigh;
        _pixelsWide = pixelsWide;

        TilesX = (int)Math.Ceiling((float)_pixelsWide / TileSizeX);
        TilesY = (int)Math.Ceiling((float)_pixelsHigh / TileSizeY);

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
        int curTileX = worldPixelX / TileSizeX;
        int curTileY = worldPixelY / TileSizeY;

        int tileIndex = curTileX + curTileY * TilesX;

        return tileIndex;
    }

    public void SetPixelDirty(int x, int y)
    {
        (int tileIndex, int tilePixelX, int tilePixelY) = PixelToTilePixelIndex(x, y);
        if (tileIndex >= 0 && tileIndex < _tiles.Length && _tiles[tileIndex] != null)
        {
            _tiles[tileIndex].IsDirty = true;
        }
    }

    private (int tileIndex, int tilePixelX, int tilePixelY) PixelToTilePixelIndex(int worldPixelX, int worldPixelY)
    {
        int curTileX = worldPixelX / TileSizeX;
        int curTileY = worldPixelY / TileSizeY;

        int tilePixelX = worldPixelX - (curTileX * TileSizeX);
        int tilePixelY = worldPixelY - (curTileY * TileSizeY);

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

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
