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
                        // Create a dictionary of dictionaries for fast searching.
                        foreach (var frame in prop.Frames)
                        {
                            if (frame.Name.ToLower().Contains(blockName) ||  // Match the frame.
                                 prop.Name.ToLower().Contains(blockName))    // Or match block name. This catches cases where the frame name is positional.
                            {
                                // Add this frame to a list.
                                if (!spriteIds.TryGetValue((ushort)prop.Id, out var frameList))
                                {
                                    frameList = new Dictionary<Vector2Short, string>();
                                    spriteIds.Add((ushort)prop.Id, frameList);
                                }

                                if (!frameList.ContainsKey(frame.UV))
                                {
                                    frameList.Add(frame.UV, $"{prop.Name} ({frame.Name})");
                                }
                                else
                                {
									// Log duplicate sprite frame to the console.
                                    Console.WriteLine($"Duplicate sprite frame detected: {prop.Name} ({frame.Name}) at {frame.UV}");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (prop.Name.ToLower().Contains(blockName))
                        {
                            if (!tileIds.ContainsKey((ushort)prop.Id))
                            {
                                tileIds.Add((ushort)prop.Id, prop.Name);
                            }
                            else
                            {
								// Log duplicate tile ID to the console.
                                Console.WriteLine($"Duplicate tile ID detected: {prop.Name} with ID {prop.Id}");
                            }
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
                        if (!wallIds.ContainsKey((ushort)prop.Id))
                        {
                            wallIds.Add((ushort)prop.Id, prop.Name);
                        }
                        else
                        {
							// Log duplicate wall ID to the console.
                            Console.WriteLine($"Duplicate wall ID detected: {prop.Name} with ID {prop.Id}");
                        }
                    }
                }
            }

            int ItemsFound = 0;
			
			// Retrieve world dimensions.
            int worldTilesWide = _wvm.CurrentWorld.TilesWide;
            int worldTilesHigh = _wvm.CurrentWorld.TilesHigh;
			
			// Calculate the range of x and y based on the selection or the entire world.
            int startX = _wvm.Selection.IsActive ? _wvm.Selection.SelectionArea.X : 0;
            int endX = _wvm.Selection.IsActive ? _wvm.Selection.SelectionArea.X + _wvm.Selection.SelectionArea.Width : worldTilesWide;
            int startY = _wvm.Selection.IsActive ? _wvm.Selection.SelectionArea.Y : 0;
            int endY = _wvm.Selection.IsActive ? _wvm.Selection.SelectionArea.Y + _wvm.Selection.SelectionArea.Height : worldTilesHigh;
			
			// Iterate over the specified range of tiles.
            for (int x = startX; x < endX && x < worldTilesWide; x++)
            {
                for (int y = startY; y < endY && y < worldTilesHigh; y++)
                {
                    Tile curTile = _wvm.CurrentWorld.Tiles[x, y];
                    var uv = curTile.GetUV();

                    // Search for tile match.
                    if (tileIds.TryGetValue(curTile.Type, out var foundTileName))
                    {
                        _wvm.UndoManager.SaveTile(x, y);                // Add tile to the undo buffer.
                        _wvm.CurrentWorld.Tiles[x, y].IsActive = false; // Remove tile.
                        _wvm.CurrentWorld.Tiles[x, y].TileColor = 0;    // Remove paint.
                        _wvm.UpdateRenderPixel(new Vector2Int32(x, y)); // Update pixel.
                        ItemsFound++;
                    }

                    // Search for sprite tile.
                    if (spriteIds.TryGetValue(curTile.Type, out var frameList))
                    {
                        _wvm.UndoManager.SaveTile(x, y);                // Add tile to the undo buffer.
                        _wvm.CurrentWorld.Tiles[x, y].IsActive = false; // Remove tile.
                        _wvm.CurrentWorld.Tiles[x, y].TileColor = 0;    // Remove paint.
                        _wvm.UpdateRenderPixel(new Vector2Int32(x, y)); // Update pixel.

                        // followed by search frames.
                        if (frameList.TryGetValue(uv, out string spriteName))
                        {
                            ItemsFound++;
                        }
                    }

                    // Search for wall match.
                    if (wallIds.TryGetValue(curTile.Wall, out var foundWallName))
                    {
                        _wvm.UndoManager.SaveTile(x, y);                // Add tile to the undo buffer.
                        _wvm.CurrentWorld.Tiles[x, y].Wall = 0;         // Remove wall.
                        _wvm.CurrentWorld.Tiles[x, y].TileColor = 0;    // Remove paint.
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