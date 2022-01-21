using System;
using System.Collections.Generic;
using System.Linq;
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

            RandomizerPluginView view = new();
            if (view.ShowDialog() == false)
                return;

            RandomizerSettings settings = new()
            {
                Seed = view.Seed
            };

            var mapping = GetRandomBlockMapping(settings);

            for (int x = 0; x < _wvm.CurrentWorld.Size.X; x++)
            {
                for (int y = 0; y < _wvm.CurrentWorld.Size.Y; y++)
                {
                    Tile t = _wvm.CurrentWorld.Tiles[x, y];
                    
                    if (mapping.ContainsKey(t.Type))
                    {
                        t.Type = (ushort)mapping[t.Type];
                    }
                }
            }
            
            _wvm.UpdateRenderRegion(new(0, 0, _wvm.CurrentWorld.Size.X, _wvm.CurrentWorld.Size.Y)); // Re-render map
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
