using System.Collections.Generic;
using TEdit.Terraria;
using TEdit.ViewModel;
using System;
using TEdit.Geometry;
using TEdit.Configuration;

namespace TEdit.Editor.Plugins;

public class FindTileWithPlugin : BasePlugin
{
    public FindTileWithPlugin(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Name = "Find Sprite, Block, or Wall";
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

        Dictionary<ushort, string> tileIds = new Dictionary<ushort, string>();
        Dictionary<ushort, Dictionary<Vector2Short, string>> spriteIds = new Dictionary<ushort, Dictionary<Vector2Short, string>>();
        if (!string.IsNullOrWhiteSpace(blockName))
        {
            foreach (var prop in WorldConfiguration.TileProperties)
            {
                if (prop.IsFramed)
                {
                    // create a dictionary of dictionaries for fast searching
                    foreach (var frame in prop.Frames)
                    {
                        if (frame.ToString().ToLower().Contains(blockName) ||  // match the frame
                             prop.Name.ToLower().Contains(blockName))    // or match block name. This catches cases where the frame name is positional
                        {
                            // Add this frame to a list 
                            Dictionary<Vector2Short, string> frameList;
                            if (!spriteIds.TryGetValue((ushort)prop.Id, out frameList))
                            {
                                frameList = new Dictionary<Vector2Short, string>();
                                spriteIds[(ushort)prop.Id] = frameList;
                            }

                            frameList[frame.UV] = $"{prop.Name} ({frame})";
                        }
                    }
                }
                else
                {
                    if (prop.Name.ToLower().Contains(blockName))
                    {
                        tileIds[(ushort)prop.Id] = prop.Name;
                    }
                }

            }
        }

        Dictionary<ushort, string> wallIds = new Dictionary<ushort, string>();
        if (!string.IsNullOrWhiteSpace(wallName))
        {
            foreach (var prop in WorldConfiguration.WallProperties)
            {
                if (prop.Name.ToLower().Contains(wallName))
                {
                    wallIds[(ushort)prop.Id] = prop.Name;
                }

            }
        }

        List<Tuple<string, Vector2Float>> locations = new List<Tuple<string, Vector2Float>>();
        int ItemsFound = 0;

        for (int x = (_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.X : 0; x < ((_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.X + _wvm.Selection.SelectionArea.Width : _wvm.CurrentWorld.TilesWide); x++)
        {
            for (int y = (_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.Y : 0; y < ((_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.Y + _wvm.Selection.SelectionArea.Height : _wvm.CurrentWorld.TilesHigh); y++)
            {
                if (locations.Count > view.MaxVolumeLimit - 1)
                {
                    locations.Add(new Tuple<string, Vector2Float>("HALTING! Too Many Entrees!: ", new Vector2Float(0, 0)));
                    FindTileLocationResultView resultView0 = new FindTileLocationResultView(locations, ItemsFound + "+");
                    resultView0.Show();
                    return;
                }

                Tile curTile = _wvm.CurrentWorld.Tiles[x, y];
                var uv = curTile.GetUV();

                // Search for tile match
                if (tileIds.TryGetValue(curTile.Type, out var foundTileName))
                {
                    locations.Add(new Tuple<string, Vector2Float>(foundTileName + ": ", new Vector2Float(x, y)));
                    ItemsFound++;
                }

                // Search for sprite tile
                if (spriteIds.TryGetValue(curTile.Type, out var frameList))
                {
                    // followed by search frames
                    if (frameList.TryGetValue(uv, out string spriteName))
                    {
                        locations.Add(new Tuple<string, Vector2Float>(spriteName + ": ", new Vector2Float(x, y)));
                        ItemsFound++;
                    }
                }

                // Search for wall match
                if (wallIds.TryGetValue(curTile.Wall, out var foundWallName))
                {
                    locations.Add(new Tuple<string, Vector2Float>(foundWallName + ": ", new Vector2Float(x, y)));
                    ItemsFound++;
                }
            }
        }

        // show the result view with the list of locations
        FindTileLocationResultView resultView1 = new FindTileLocationResultView(locations, ItemsFound.ToString());
        resultView1.Show();
    }
}
