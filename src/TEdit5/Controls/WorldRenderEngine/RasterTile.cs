using SkiaSharp;
using System;

namespace TEdit5.Controls.WorldRenderEngine;

public class RasterTile : IDisposable
{
    public SKBitmap? Bitmap { get; set; }
    public bool IsDirty { get; set; }

    public int TileX { get; set; }
    public int TileY { get; set; }
    public int PixelX { get; set; }
    public int PixelY { get; set; }

    #region Dispose
    private bool _disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Bitmap?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~RasterTile()
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
