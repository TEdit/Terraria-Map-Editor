using TEdit.ViewModel;
using TEdit.Editor.Tools;
using TEdit.Geometry;

namespace TEdit.Editor.Plugins
{
    public sealed class CleanseWorldPlugin : BasePlugin
    {
        public CleanseWorldPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Cleanse World";
        }

        public override void Execute()
        {
            // Check if the current world is loaded.
            if (_wvm.CurrentWorld == null)
                return;

            // Instantiate a MorphTool with the current WorldViewModel.
            MorphTool morphTool = new(_wvm);

            // Iterate over each tile in the world starting from the bottom.
            for (int y = _wvm.CurrentWorld.TilesHigh - 1; y > 0; y--)
            {
                for (int x = 0; x < _wvm.CurrentWorld.TilesWide; x++)
                {
                    // Save the current state of the tile at (x, y) to the undo manager.
                    _wvm.UndoManager.SaveTile(x, y);

                    // Apply the morphing operation to the tile at (x, y).
                    morphTool.MorphTileExternal(new Vector2Int32(x, y));
                }
            }

            // Save the entire operation to the undo stack.
            _wvm.UndoManager.SaveUndo();
        }
    }
}
