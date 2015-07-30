using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Name = "Find Chest With";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null) return;

            FindChestWithPluginView view = new FindChestWithPluginView();
            if (view.ShowDialog() == false)
            {
                return;
            }
            string text = view.ItemToFind;
            List<Vector2> locations = new List<Vector2>();

            for (int x = 0; x < _wvm.CurrentWorld.TilesWide; x++)
            {
                for (int y = 0; y < _wvm.CurrentWorld.TilesHigh; y++)
                {
                    if (_wvm.CurrentWorld.Tiles[x, y].Type == (int)TileType.Chest)
                    {
                        Chest chest = _wvm.CurrentWorld.GetChestAtTile(x, y);
                        if (x == chest.X && y == chest.Y)
                        {
                            if (chest.Items.Count(c => c.GetName().ToLower().Contains(text)) > 0)
                            {
                                locations.Add(new Vector2(x, y));
                            }
                        }
                    }
                }
            }

            FindChestWithPluginResultView resultView = new FindChestWithPluginResultView(locations);
            resultView.Show();
        }
    }
}
