using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TEdit.Terraria;
using TEdit.ViewModel;

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

            string tileName = view.TileToFind.ToLower();
            List<Vector2> locations = new List<Vector2>();

            for (int x = (_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.X : 0; x < ((_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.X + _wvm.Selection.SelectionArea.Width : _wvm.CurrentWorld.TilesWide); x++)
            {
                for (int y = (_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.Y : 0; y < ((_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.Y + _wvm.Selection.SelectionArea.Height : _wvm.CurrentWorld.TilesHigh); y++)
                {
                    Tile curTile = _wvm.CurrentWorld.Tiles[x, y];
                    TEdit.Terraria.Objects.TileProperty tileProperty = World.TileProperties[curTile.Type];
                    TEdit.Terraria.Objects.WallProperty wallProperty = World.WallProperties[curTile.Wall];
                    if (tileProperty.Name.ToLower().Contains(tileName) || wallProperty.Name.ToLower().Contains(tileName))
                    {
                        locations.Add(new Vector2(x, y));
                    }
                }
            }

            // show the result view with the list of locations
            FindLocationResultView resultView = new FindLocationResultView(locations);
            resultView.Show();
        }
    }
}
