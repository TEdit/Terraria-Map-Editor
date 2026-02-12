using System.Linq;
using System.Windows;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

public class RemoveAllChestsPlugin : BasePlugin
{
    public RemoveAllChestsPlugin(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Name = "移除所有箱子";
    }

    public override void Execute()
    {
        if (_wvm.CurrentWorld == null) return;

        if (MessageBox.Show("您确定要删除所有箱子吗?", "删除箱子",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            return;

        var chestLocations = _wvm.CurrentWorld.Chests.Select(chest => new Vector2Int32 { X = chest.X, Y = chest.Y }).ToList();

        foreach (var location in chestLocations)
        {
            for (int x = location.X; x < location.X + 2; x++)
            {
                for (int y = location.Y; y < location.Y + 2; y++)
                {

                    if (_wvm.CurrentWorld.ValidTileLocation(x, y) && _wvm.CurrentWorld.Tiles[x, y].Type == (int)TileType.Chest)
                    {
                        _wvm.UndoManager.SaveTile(x, y);
                        _wvm.CurrentWorld.Tiles[x, y].Type = 0;
                        _wvm.CurrentWorld.Tiles[x, y].IsActive = false;
                        _wvm.UpdateRenderPixel(x, y);
                    }
                }
            }
        }

        _wvm.UndoManager.SaveUndo();

    }
}
