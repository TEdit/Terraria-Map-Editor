using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEdit.Geometry.Primitives;
using TEdit.Terraria;

namespace TEdit.Editor
{
    public interface ITileData
    {
        Vector2Int32 Size { get; }
        Tile[,] Tiles { get; }
        ObservableCollection<Sign> Signs { get; }
        ObservableCollection<Chest> Chests { get; }
        ObservableCollection<TileEntity> TileEntities { get; }
        Chest GetChestAtTile(int x, int y);
        Sign GetSignAtTile(int x, int y);
        TileEntity GetTileEntityAtTile(int x, int y);
    }
}
