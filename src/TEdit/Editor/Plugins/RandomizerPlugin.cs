using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TEdit.Terraria;
using TEdit.ViewModel;


namespace TEdit.Editor.Plugins
{
    public class RandomizerPlugin : BasePlugin
    {
        public RandomizerPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Randomize All Blocks in the World";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null)
                return;

            bool activeSelection = _wvm.Selection.IsActive;

            RandomizerPluginView view = new(activeSelection);
            if (view.ShowDialog() == false)
                return;

            BlockRandomizerSettings blockSettings = new()
            {
                Seed = view.Seed,
            };

            WallRandomizerSetting wallSettings = new()
            {
                Seed = view.Seed,
            };

            var blockMapping = GetRandomBlockMapping(blockSettings);
            var wallMapping = GetRandomWallMapping(wallSettings);

            Rectangle randomizationArea;

            if (view.OnlySelection)
            {
                // Set the randomizationArea to the selected area
                randomizationArea = _wvm.Selection.SelectionArea;
            }
            else
            {
                // Set the randomizationArea to the whole world
                randomizationArea = new(0, 0, _wvm.CurrentWorld.Size.X, _wvm.CurrentWorld.Size.Y);
            }

            // Change every tile in the randomizationArea according to the tilemapping
            for (int x = randomizationArea.Left; x < randomizationArea.Right; x++)
            {
                for (int y = randomizationArea.Top; y < randomizationArea.Bottom; y++)
                {
                    if (view.EnableUndo)
                        _wvm.UndoManager.SaveTile(x, y); // Store tile for undo

                    Tile t = _wvm.CurrentWorld.Tiles[x, y];

                    if (blockMapping.ContainsKey(t.Type))
                        t.Type = (ushort)blockMapping[t.Type];

                    if (wallMapping.ContainsKey(t.Wall))
                        t.Wall = (ushort)wallMapping[t.Wall];
                }
            }

            if (view.EnableUndo)
                _wvm.UndoManager.SaveUndo();

            _wvm.UpdateRenderRegion(randomizationArea); // Re-render map
            _wvm.MinimapImage = Render.RenderMiniMap.Render(_wvm.CurrentWorld); // Update Minimap
        }

        private Dictionary<int, int> GetRandomBlockMapping(BlockRandomizerSettings settings)
        {
            Dictionary<int, int> mapping = new();
            Random rng = new(settings.Seed);

            List<int> tiles = new(Terraria.World.TileBricks.Select(x => x.Id));
            List<int> shuffledTiles = new(tiles);

            int n = shuffledTiles.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                int temp = shuffledTiles[n];
                shuffledTiles[n] = shuffledTiles[k];
                shuffledTiles[k] = temp;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                mapping.Add(tiles[i], shuffledTiles[i]);
            }

            return mapping;
        }

        private Dictionary<int, int> GetRandomWallMapping(WallRandomizerSetting settings)
        {
            Dictionary<int, int> mapping = new();
            Random rng = new(settings.Seed);

            List<int> walls = new(Terraria.World.WallProperties.Select(x => x.Id));
            walls.Remove(0); // Remove Sky from the walls to be shuffled
            List<int> shuffledWalls = new(walls);

            int n = shuffledWalls.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                int temp = shuffledWalls[n];
                shuffledWalls[n] = shuffledWalls[k];
                shuffledWalls[k] = temp;
            }

            for (int i = 0; i < walls.Count; i++)
            {
                mapping.Add(walls[i], shuffledWalls[i]);
            }

            return mapping;
        }

        private class BlockRandomizerSettings
        {
            public int Seed { get; set; }
        }

        private class WallRandomizerSetting
        {
            public int Seed { get; set; }
        }
    }

}
