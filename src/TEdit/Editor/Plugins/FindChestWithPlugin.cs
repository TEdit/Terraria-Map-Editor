using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TEditXNA.Terraria;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Plugins
{
    public class FindChestWithPlugin : BasePlugin
    {
        public FindChestWithPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Find Chests With";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null) return;

            FindChestWithPluginView view = new FindChestWithPluginView();
            if (view.ShowDialog() == false)
            {
                return;
            }

            string itemName = view.ItemToFind.ToLower();
            List<Vector2> locations = new List<Vector2>();

            // Search the whole World
            for (int x = 0; x < _wvm.CurrentWorld.TilesWide; x++)
            {
                for (int y = 0; y < _wvm.CurrentWorld.TilesHigh; y++)
                {
                    // Check if a tile is a chest
                    if (_wvm.CurrentWorld.Tiles[x, y].Type == (int)TileType.Chest)
                    {
                        // Convert the tile to its respective chest
                        Chest chest = _wvm.CurrentWorld.GetChestAtTile(x, y);
                        // Only use the chest once (chest = 2x2 so it would add 4 entries)
                        if (x == chest.X && y == chest.Y)
                        {
                            // check if the item exists in the chest
                            if (chest.Items.Count(c => c.GetName().ToLower().Contains(itemName)) > 0)
                            {
                                locations.Add(new Vector2(x, y));
                            }
                        }
                    }
                }
            }

            // show the result view with the list of locations
            FindChestWithPluginResultView resultView = new FindChestWithPluginResultView(locations);
            resultView.Show();
        }
    }
}
