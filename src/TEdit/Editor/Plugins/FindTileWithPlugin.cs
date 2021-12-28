using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TEdit.Terraria;
using TEdit.ViewModel;
using System;

namespace TEdit.Editor.Plugins
{
    public class FindTileWithPlugin : BasePlugin
    {
        public FindTileWithPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Find Block or Wall";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null) return;

            FindTileWithPluginView view = new FindTileWithPluginView();
            if (view.ShowDialog() == false)
            {
                return;
            }

            string blockName = view.BlockToFind.ToLower();
            string wallName = view.WallToFind.ToLower();


            Dictionary<int, string> tileIds = new Dictionary<int, string>();
            if (!string.IsNullOrWhiteSpace(blockName))
            {
                foreach (var prop in World.TileProperties)
                {
                    if (prop.Name.ToLower().Contains(blockName))
                    {
                        tileIds.Add(prop.Id, prop.Name);
                    }

                }
            }

            Dictionary<int, string> wallIds = new Dictionary<int, string>();
            if (!string.IsNullOrWhiteSpace(wallName))
            {
                foreach (var prop in World.WallProperties)
                {
                    if (prop.Name.ToLower().Contains(wallName))
                    {
                        wallIds.Add(prop.Id, prop.Name);
                    }

                }
            }


            List<Tuple<string, Vector2>> locations = new List<Tuple<string, Vector2>>();

            for (int x = (_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.X : 0; x < ((_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.X + _wvm.Selection.SelectionArea.Width : _wvm.CurrentWorld.TilesWide); x++)
            {
                for (int y = (_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.Y : 0; y < ((_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.Y + _wvm.Selection.SelectionArea.Height : _wvm.CurrentWorld.TilesHigh); y++)
                {
                    if (locations.Count > view.MaxVolumeLimit - 1)
                    {
                        locations.Add(new Tuple<string, Vector2>("HALTING! Too Many Entrees!: ", new Vector2(0, 0)));
                        FindTileLocationResultView resultView0 = new FindTileLocationResultView(locations);
                        resultView0.Show();
                        return;
                    }

                    Tile curTile = _wvm.CurrentWorld.Tiles[x, y];

                    if (tileIds.TryGetValue(curTile.Type, out var foundTileName))
                    {
                        locations.Add(new Tuple<string, Vector2>(foundTileName + ": ", new Vector2(x, y)));
                    }
                    if (wallIds.TryGetValue(curTile.Wall, out var foundWallName))
                    {
                        locations.Add(new Tuple<string, Vector2>(foundWallName + ": ", new Vector2(x, y)));
                    }
                }
            }

            // show the result view with the list of locations
            FindTileLocationResultView resultView1 = new FindTileLocationResultView(locations);
            resultView1.Show();
        }
    }
}
