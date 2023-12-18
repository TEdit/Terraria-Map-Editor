using SkiaSharp;
using System;
using System.Collections.Generic;

namespace TEdit.Desktop.Controls.WorldRenderEngine;

public class RasterTileCache : IRasterTileCache
{
    private bool _disposedValue;

    public Dictionary<SKPointI, RasterTile> Tiles { get; } = new();

    public int TileSize => 100;

    public void AddOrUpdate(RasterTile tile)
    {
        if (Tiles.TryGetValue(tile.TilePosition, out var existingTile))
        {
            existingTile?.Dispose();
        }

        Tiles[tile.TilePosition] = tile;
    }

    public void Clear()
    {
        var tiles = Tiles.Values;
        Tiles.Clear();
        foreach (var tile in tiles)
        {
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
                var tiles = Tiles.Values;
                Tiles.Clear();
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
