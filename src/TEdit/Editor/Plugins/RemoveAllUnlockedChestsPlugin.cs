using System.Linq;
using System.Windows;
using TEdit.Terraria;
using TEdit.Geometry;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

public class RemoveAllUnlockedChestsPlugin : BasePlugin
{
    public RemoveAllUnlockedChestsPlugin(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Name = "Remove All Unlocked Chests";
    }

    private short[] _lockedChestUs = new short[] { 72, 144, 828, 864, 900, 936, 972 };

    private bool isLocked(short u)
    {
        foreach (short s in _lockedChestUs)
        {
            if (u == s || u == s + 18) return true;
        }

        return false;
    }

    public override async void Execute()
    {
        if (_wvm.CurrentWorld == null) return;

        if (!await App.DialogService.ShowConfirmationAsync("Delete Chests",
                "Are you sure you wish to delete all unlocked chests?"))
            return;

        var chestLocations = _wvm.CurrentWorld.Chests.Select(chest => new Vector2Int32 { X = chest.X, Y = chest.Y }).ToList();

        foreach (var location in chestLocations)
        {
            for (int x = location.X; x < location.X + 2; x++)
            {
                for (int y = location.Y; y < location.Y + 2; y++)
                {

                    if (_wvm.CurrentWorld.ValidTileLocation(x, y) && _wvm.CurrentWorld.Tiles[x, y].Type == (int)TileType.Chest && !isLocked(_wvm.CurrentWorld.Tiles[x, y].U))
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
