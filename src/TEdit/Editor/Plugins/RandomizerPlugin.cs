using System;
using System.Collections.Generic;
using System.Linq;
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
                Seed = 12345678
            };

            var mapping = GetRandomBlockMapping(settings);

            
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
