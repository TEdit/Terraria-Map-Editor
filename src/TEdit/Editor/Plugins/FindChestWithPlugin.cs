using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

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
        List<Tuple<Vector2, string>> locations = new List<Tuple<Vector2, string>>();
        Vector2 spawn = new Vector2(_wvm.CurrentWorld.SpawnX, _wvm.CurrentWorld.SpawnY);

        foreach (var chest in _wvm.CurrentWorld.Chests)
        {
            if (chest.Items.Count(c => c.GetName().ToLower().Contains(itemName)) > 0)
            {
                if (view.CalculateDistance)
                {
                    // Record Distance
                    locations.Add(new Tuple<Vector2, string>(new Vector2(chest.X, chest.Y), ", Distance: " + Math.Round(Vector2.Distance(spawn, new Vector2(chest.X, chest.Y)))));
                }
                else
                {
                    locations.Add(new Tuple<Vector2, string>(new Vector2(chest.X, chest.Y), ""));
                }
            }
        }

        // Check If CalcDistance Is Enabled
        if (view.CalculateDistance)
        {
            // Sort Values Based On Distance, Rebuilt Array
            List<Tuple<Vector2, string>> locationsSorted = new List<Tuple<Vector2, string>>();
            foreach (var value in locations.OrderBy(c => int.Parse(c.Item2.Replace(", Distance: ", ""))))
            {
                locationsSorted.Add(value);
            }

            // Update Array
            locations = locationsSorted;
        }

        // show the result view with the list of locations
        FindLocationResultView resultView = new FindLocationResultView(locations);
        resultView.Show();
    }
}
