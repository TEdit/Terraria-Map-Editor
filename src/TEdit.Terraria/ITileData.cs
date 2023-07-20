using System.Collections.ObjectModel;
using TEdit.Geometry;

namespace TEdit.Terraria
{
    public interface ITileData
    {
        Vector2Int32 Size { get; }
        Tile[,] Tiles { get; }
        ObservableCollection<Sign> Signs { get; }
        ObservableCollection<Chest> Chests { get; }
        ObservableCollection<TileEntity> TileEntities { get; }
        Chest GetChestAtTile(int x, int y, bool findOrigin = false);
        Sign GetSignAtTile(int x, int y, bool findOrigin = false);
        TileEntity GetTileEntityAtTile(int x, int y, bool findOrigin = false);
    }
}
