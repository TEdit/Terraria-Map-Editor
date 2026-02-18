using System;

namespace TEdit5.Controls.WorldRenderEngine;

public interface IRasterTileCache : IDisposable
{
    void SetTile(RasterTile tile, int x, int y);
    RasterTile? GetTile(int x, int y);
    void Clear();

    void SetPixelDirty(int x, int y);

    int TileSizeX { get; }
    int TileSizeY { get; }

    int TilesX { get; }
    int TilesY { get; }
}
