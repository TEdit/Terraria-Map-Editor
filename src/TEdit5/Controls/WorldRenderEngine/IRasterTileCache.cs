using SkiaSharp;
using System;
using System.Collections.Generic;

namespace TEdit5.Controls.WorldRenderEngine;

public interface IRasterTileCache : IDisposable
{
    void SetTile(RasterTile tile, int x, int y);
    RasterTile? GetTile(int x, int y);
    void Clear();

    void SetPixelDirty(int x, int y);

    int TileSize { get; }

    int TilesX { get; }
    int TilesY { get; }
}
