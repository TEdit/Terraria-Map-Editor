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
            Name = "Randomize all blocks in a world";
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
            Dictionary<int, int> output = new();

            Random rng = new Random(settings.Seed);

            List<int> tiles = new List<int>(Terraria.World.TileBricks.Select((x) => x.Id));

            int n = tiles.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                int temp = tiles[n];
                tiles[n] = tiles[k];
                tiles[k] = temp;
            }

            return output;
        }

        private class RandomizerSettings
        {
            public int Seed { get; set; }
        }
    }

}
