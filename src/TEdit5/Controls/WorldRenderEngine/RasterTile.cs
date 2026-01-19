using SkiaSharp;
using System;

namespace TEdit5.Controls.WorldRenderEngine;

public class RasterTile : IDisposable
{
    public SKBitmap? Bitmap { get; set; }
    public bool IsDirty { get; set; }
    public SKRectI? DirtyRegion { get; set; }

    public int TileX { get; set; }
    public int TileY { get; set; }
    public int PixelX { get; set; }
    public int PixelY { get; set; }

    public void MarkPixelDirty(int tilePixelX, int tilePixelY)
    {
        if (DirtyRegion == null)
        {
            DirtyRegion = new SKRectI(tilePixelX, tilePixelY, tilePixelX + 1, tilePixelY + 1);
        }
        else
        {
            var current = DirtyRegion.Value;
            DirtyRegion = new SKRectI(
                Math.Min(current.Left, tilePixelX),
                Math.Min(current.Top, tilePixelY),
                Math.Max(current.Right, tilePixelX + 1),
                Math.Max(current.Bottom, tilePixelY + 1)
            );
        }
        IsDirty = true;
    }

    public void ClearDirtyRegion()
    {
        DirtyRegion = null;
        IsDirty = false;
    }

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
