using System.Collections.Generic;
using TEdit.Geometry;

namespace TEdit.Terraria;

public interface ITileData
{
    Vector2Int32 Size { get; }
    Tile[,] Tiles { get; }
    List<Sign> Signs { get; }
    List<Chest> Chests { get; }
    List<TileEntity> TileEntities { get; }
    Chest GetChestAtTile(int x, int y, bool findOrigin = false);
    Sign GetSignAtTile(int x, int y, bool findOrigin = false);
    TileEntity GetTileEntityAtTile(int x, int y, bool findOrigin = false);
}
