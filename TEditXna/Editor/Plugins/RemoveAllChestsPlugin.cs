using System.Linq;
using System.Windows;
using TEdit.Geometry.Primitives;
using TEditXNA.Terraria;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Plugins
{
    public class RemoveAllChestsPlugin : BasePlugin
    {
        public RemoveAllChestsPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Remove All Chests";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null) return;

            if (MessageBox.Show("Are you sure you wish to delete all chests?", "Delete Chests",
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
}
