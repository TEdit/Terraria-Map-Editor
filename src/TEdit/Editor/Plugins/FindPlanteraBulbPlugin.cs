using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TEdit.ViewModel;
using TEdit.Terraria;
using TEdit.Configuration;

namespace TEdit.Editor.Plugins;

class FindPlanteraBulbPlugin : BasePlugin
{
    public FindPlanteraBulbPlugin(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Name = "Find Plantera's Bulb";
    }

    public override void Execute()
    {
        if (_wvm.CurrentWorld == null) return;

        List<Tuple<Vector2, string>> locations = new List<Tuple<Vector2, string>>();

        // Search the whole World
        for (int x = 0; x < _wvm.CurrentWorld.TilesWide; x++)
        {
            for (int y = 0; y < _wvm.CurrentWorld.TilesHigh; y++)
            {
                // Check if a tile is a chest
                if (_wvm.CurrentWorld.Tiles[x, y].Type == (int)TileType.PlanteraBulb)
                {
                    if (!findConnectedTitle(locations, x, y))
                    {
                        locations.Add(new Tuple<Vector2, string>(new Vector2(x, y), ""));
                    }
                }
            }
        }

        // show the result view with the list of locations
        FindLocationResultView resultView = new FindLocationResultView(locations);
        resultView.Show();
    }

    protected bool findConnectedTitle(List<Tuple<Vector2, string>> locations, int x, int y)
    {
        Vector2 position;
        for ( int i = 0; i < locations.Count; i++ )
        {
            position = locations[i].Item1;
            if ( Math.Abs((int)position.X - x) <= 1 && Math.Abs((int)position.Y - y) <= 1 )
            {
                return true;
            }
        }
        return false;
    }
}
