using TEdit.ViewModel;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.DataModel;

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

            World world = _wvm.CurrentWorld;

            // Get the Purify biome morpher.
            if (!WorldConfiguration.MorphSettings.Biomes.TryGetValue("Purify", out var purifyBiome))
                return;

            var morpher = MorphBiomeDataApplier.GetMorpher(purifyBiome);

            // Iterate over each tile in the world starting from the bottom.
            for (int y = world.TilesHigh - 1; y > 0; y--)
            {
                var level = MorphBiomeDataApplier.ComputeMorphLevel(world, y);

                for (int x = 0; x < world.TilesWide; x++)
                {
                    // Save the current state of the tile at (x, y) to the undo manager.
                    _wvm.UndoManager.SaveTile(x, y);

                    // Apply the morphing operation to the tile at (x, y).
                    var p = new Vector2Int32(x, y);
                    morpher.ApplyMorph(_wvm.MorphToolOptions, world, world.Tiles[x, y], level, p);
                    _wvm.UpdateRenderPixel(p);
                }
            }

            // Save the entire operation to the undo stack.
            _wvm.UndoManager.SaveUndo();
        }
    }
}
