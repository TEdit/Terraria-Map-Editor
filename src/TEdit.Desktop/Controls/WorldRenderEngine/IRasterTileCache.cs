using SkiaSharp;
using System;
using System.Collections.Generic;

namespace TEdit.Desktop.Controls.WorldRenderEngine;

public interface IRasterTileCache : IDisposable
{
    Dictionary<SKPointI, RasterTile> Tiles { get; }
    void AddOrUpdate(RasterTile tile);
    void Clear();

    int TileSize { get; }
}
