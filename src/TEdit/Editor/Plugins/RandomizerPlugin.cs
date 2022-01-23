using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            RandomizerSettings settings = new()
            {
                Seed = view.Seed
            };

            var mapping = GetRandomBlockMapping(settings);

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
                    Tile t = _wvm.CurrentWorld.Tiles[x, y];

                    if (mapping.ContainsKey(t.Type))
                    {
                        if (view.EnableUndo)
                        {
                            _wvm.UndoManager.SaveTile(x, y); // Store tile for undo
                        }

                        t.Type = (ushort)mapping[t.Type];
                    }
                }
            }

            if (view.EnableUndo)
            {
                _wvm.UndoManager.SaveUndo();
            }

            _wvm.UpdateRenderRegion(randomizationArea); // Re-render map
            _wvm.MinimapImage = Render.RenderMiniMap.Render(_wvm.CurrentWorld); // Update Minimap
        }

        private Dictionary<int, int> GetRandomBlockMapping(RandomizerSettings settings)
        {
            Dictionary<int, int> mapping = new();
            Random rng = new Random(settings.Seed);

            List<int> tiles = new List<int>(Terraria.World.TileBricks.Select((x) => x.Id));
            List<int> shuffledTiles = new List<int>(tiles);

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

        private class RandomizerSettings
        {
            public int Seed { get; set; }
        }
    }

}
