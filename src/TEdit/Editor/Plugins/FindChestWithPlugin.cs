using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins
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

            foreach (var chest in _wvm.CurrentWorld.Chests)
            {
                if (chest.Items.Count(c => c.GetName().ToLower().Contains(itemName)) > 0)
                {
                    locations.Add(new Vector2(chest.X, chest.Y));
                }
            }

            // show the result view with the list of locations
            FindChestWithPluginResultView resultView = new FindChestWithPluginResultView(locations);
            resultView.Show();
        }
    }
}
