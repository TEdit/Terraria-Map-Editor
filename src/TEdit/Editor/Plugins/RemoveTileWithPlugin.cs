using System.Collections.Generic;
using TEdit.Terraria;
using TEdit.ViewModel;
using System;
using System.Windows;
using TEdit.Geometry;
using TEdit.Configuration;

namespace TEdit.Editor.Plugins
{
    public class RemoveTileWithPlugin : BasePlugin
    {
        public RemoveTileWithPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Remove Sprite, Block, or Wall";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null) return;

            RemoveTileWithPluginView view = new RemoveTileWithPluginView();
            if (view.ShowDialog() == false)
            {
                return;
            }

            if (MessageBox.Show(
            "This will completely remove any found tiles from your world. Continue?",
            "RemoveTileWithPlugin",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.Yes) != MessageBoxResult.Yes)
                return;

            string blockName = view.BlockToRemove.ToLower();
            string wallName = view.WallToRemove.ToLower();

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
                            if (frame.Name.ToLower().Contains(blockName) ||  // match the frame
                                 prop.Name.ToLower().Contains(blockName))    // or match block name. This catches cases where the frame name is positional
                            {
                                // Add this frame to a list 
                                Dictionary<Vector2Short, string> frameList;
                                if (!spriteIds.TryGetValue((ushort)prop.Id, out frameList))
                                {
                                    frameList = new Dictionary<Vector2Short, string>();
                                    spriteIds.Add((ushort)prop.Id, frameList);
                                }

                                frameList.Add(frame.UV, $"{prop.Name} ({frame.Name})");
                            }
                        }
                    }
                    else
                    {
                        if (prop.Name.ToLower().Contains(blockName))
                        {
                            tileIds.Add((ushort)prop.Id, prop.Name);
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
                        wallIds.Add((ushort)prop.Id, prop.Name);
                    }

                }
            }

            int ItemsFound = 0;
            for (int x = (_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.X : 0; x < ((_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.X + _wvm.Selection.SelectionArea.Width : _wvm.CurrentWorld.TilesWide); x++)
            {
                for (int y = (_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.Y : 0; y < ((_wvm.Selection.IsActive) ? _wvm.Selection.SelectionArea.Y + _wvm.Selection.SelectionArea.Height : _wvm.CurrentWorld.TilesHigh); y++)
                {
                    Tile curTile = _wvm.CurrentWorld.Tiles[x, y];
                    var uv = curTile.GetUV();

                    // Search for tile match
                    if (tileIds.TryGetValue(curTile.Type, out var foundTileName))
                    {
                        _wvm.UndoManager.SaveTile(x, y); // Add tile to the undo buffer.
                        _wvm.CurrentWorld.Tiles[x, y].IsActive = false; // Remove tile.
                        _wvm.CurrentWorld.Tiles[x, y].TileColor = 0; // Remove paint.
                        _wvm.UpdateRenderPixel(new Vector2Int32(x, y)); // Update pixel.
                        ItemsFound++;
                    }

                    // Search for sprite tile
                    if (spriteIds.TryGetValue(curTile.Type, out var frameList))
                    {
                        _wvm.UndoManager.SaveTile(x, y); // Add tile to the undo buffer.
                        _wvm.CurrentWorld.Tiles[x, y].IsActive = false; // Remove tile.
                        _wvm.CurrentWorld.Tiles[x, y].TileColor = 0; // Remove paint.
                        _wvm.UpdateRenderPixel(new Vector2Int32(x, y)); // Update pixel.

                        // followed by search frames
                        if (frameList.TryGetValue(uv, out string spriteName))
                        {
                            ItemsFound++;
                        }
                    }

                    // Search for wall match
                    if (wallIds.TryGetValue(curTile.Wall, out var foundWallName))
                    {
                        _wvm.UndoManager.SaveTile(x, y); // Add tile to the undo buffer.
                        _wvm.CurrentWorld.Tiles[x, y].Wall = 0; // Remove wall.
                        _wvm.CurrentWorld.Tiles[x, y].TileColor = 0; // Remove paint.
                        _wvm.UpdateRenderPixel(new Vector2Int32(x, y)); // Update pixel.
                        ItemsFound++;
                    }
                }
            }
            _wvm.UndoManager.SaveUndo(); // Add to the undo buffer.

            // Display the total tiles removed.
            MessageBox.Show(
            ItemsFound + " tiles have been found and removed.",
            "RemoveTileWithPlugin",
            MessageBoxButton.OK,
            MessageBoxImage.Information,
            MessageBoxResult.Yes);
        }
    }
}
