using System;
using System.Collections.Generic;
using TEdit.ViewModel;
using TEdit.Geometry;

namespace TEdit.Editor.Plugins
{
    public sealed class SimpleOreGeneratorPlugin : BasePlugin
    {
        private PerlinNoise _noiseGenerator;
        private readonly Random _random;
        private ushort _currentOreId;
        private bool IncludeAsh = true;
        private bool EnableUndo = true;

        public SimpleOreGeneratorPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            _noiseGenerator = new PerlinNoise(1);
            _random = new Random();
            Name = "Simple Ore Generator";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null)
                return;

            bool activeSelection = _wvm.Selection.IsActive;

            // Show the view.
            SimpleOreGeneratorPluginView view = new(activeSelection);
            if (view.ShowDialog() == false)
                return;

            // Set local bools.
            IncludeAsh = view.IncludeAsh;
            EnableUndo = view.EnableUndo;

            // Get selected ore IDs from the view
            List<int> selectedOres = GetSelectedOres(view);
            if (selectedOres.Count == 0)
                return;

            // Define random seed from time.
            _noiseGenerator = new PerlinNoise((int)DateTime.Now.Ticks);

            RectangleInt32 randomizationArea;

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

            // Initialize the current ore ID
            _currentOreId = (ushort)selectedOres[_random.Next(selectedOres.Count)];

            // Visited tiles to track veins
            bool[,] visited = new bool[randomizationArea.Width, randomizationArea.Height];

            // Change every tile in the randomizationArea according to the tile mapping
            for (int x = randomizationArea.Left; x < randomizationArea.Right; x++)
            {
                for (int y = randomizationArea.Top; y < randomizationArea.Bottom; y++)
                {
                    if (!visited[x - randomizationArea.Left, y - randomizationArea.Top])
                    {
                        if (ShouldGenerateOre(x, y) && IsValidLocation(x, y))
                        {
                            _currentOreId = (ushort)selectedOres[_random.Next(selectedOres.Count)];

                            try
                            {
                                GenerateVein(x, y, visited, randomizationArea);
                            }
                            catch (Exception) { continue; }
                        }
                    }
                }
            }
            if (EnableUndo)
                _wvm.UndoManager.SaveUndo();
        }

        private bool ShouldGenerateOre(int x, int y)
        {
            var result = OctaveGenerator(x, y);
            return result > 0.6 && result < 0.75;
        }

        private bool IsValidLocation(int x, int y)
        {
            var curTile = _wvm.CurrentWorld.Tiles[x, y];
            return curTile.IsActive                    // Is not Air.
                && curTile.Type == 0                   // Dirt.
                || curTile.Type == 1                   // Stone block.
                || curTile.Type == 25                  // Ebonstone block.
                || curTile.Type == 53                  // Sand block.
                || (curTile.Type == 57 && IncludeAsh)  // Ash block.
                || curTile.Type == 59                  // Mud block.
                || curTile.Type == 112                 // Ebonsand block.
                || curTile.Type == 116                 // Pearlsand block.
                || curTile.Type == 117                 // Pearlstone block.
                || curTile.Type == 161                 // Ice block.
                || curTile.Type == 163                 // Purple ice block.
                || curTile.Type == 164                 // Pink ice block.
                || curTile.Type == 200                 // Red ice block.
                || curTile.Type == 203                 // Crimstone block.
                // || curTile.Type == 224              // Slush block (Questionable).
                || curTile.Type == 234                 // Crimsand block.
                || curTile.Type == 396                 // Sandstone block.
                || curTile.Type == 400                 // Ebonsandstone block.
                || curTile.Type == 401                 // Crimsandstone block.
                || curTile.Type == 403;                // Pearlsandstone block.
        }

        private void GenerateVein(int startX, int startY, bool[,] visited, RectangleInt32 randomizationArea)
        {
            Stack<(int x, int y)> stack = new();
            stack.Push((startX, startY));

            while (stack.Count > 0)
            {
                var (x, y) = stack.Pop();

                if (x < randomizationArea.Left || x >= randomizationArea.Right || y < randomizationArea.Top || y >= randomizationArea.Bottom)
                    continue;

                if (visited[x - randomizationArea.Left, y - randomizationArea.Top])
                    continue;

                visited[x - randomizationArea.Left, y - randomizationArea.Top] = true;

                if (ShouldGenerateOre(x, y) && IsValidLocation(x, y))
                {
                    if (EnableUndo)
                        _wvm.UndoManager.SaveTile(x, y);
                    _wvm.CurrentWorld.Tiles[x, y].IsActive = true;
                    _wvm.CurrentWorld.Tiles[x, y].Type = _currentOreId;
                    _wvm.UpdateRenderPixel(x, y);

                    stack.Push((x + 1, y));
                    stack.Push((x - 1, y));
                    stack.Push((x, y + 1));
                    stack.Push((x, y - 1));
                }
            }
        }

        private float OctaveGenerator(int worldX, int worldY)
        {
            float result = 0f;

            int octaves = 5;
            float amplitude = 1f;
            float persistance = 0.35f;
            float frequency = 0.04f;

            for (int i = 0; i < octaves; i++)
            {
                result += _noiseGenerator.Noise(worldX * frequency, worldY * frequency) * amplitude;
                amplitude *= persistance;
                frequency *= 2.0f;
            }

            if (result < 0) result = 0.0f;
            if (result > 1) result = 1.0f;
            return result;
        }

        private List<int> GetSelectedOres(SimpleOreGeneratorPluginView view)
        {
            List<int> selectedOres = new();

            // Add ores based on the selection in the view
            if (view.Tier1BothRadioButton.IsChecked == true || view.CopperRadioButton.IsChecked == true)
                selectedOres.Add(7); // Copper Ore
            if (view.Tier1BothRadioButton.IsChecked == true || view.TinRadioButton.IsChecked == true)
                selectedOres.Add(166); // Tin Ore

            if (view.Tier2BothRadioButton.IsChecked == true || view.IronRadioButton.IsChecked == true)
                selectedOres.Add(6); // Iron Ore
            if (view.Tier2BothRadioButton.IsChecked == true || view.LeadRadioButton.IsChecked == true)
                selectedOres.Add(167); // Lead Ore

            if (view.Tier3BothRadioButton.IsChecked == true || view.SilverRadioButton.IsChecked == true)
                selectedOres.Add(9); // Silver Ore
            if (view.Tier3BothRadioButton.IsChecked == true || view.TungstenRadioButton.IsChecked == true)
                selectedOres.Add(168); // Tungsten Ore

            if (view.Tier4BothRadioButton.IsChecked == true || view.GoldRadioButton.IsChecked == true)
                selectedOres.Add(8); // Gold Ore
            if (view.Tier4BothRadioButton.IsChecked == true || view.PlatinumRadioButton.IsChecked == true)
                selectedOres.Add(169); // Platinum Ore

            if (view.MeteoriteCheckbox.IsChecked == true)
                selectedOres.Add(37); // Meteorite

            if (view.Tier6BothRadioButton.IsChecked == true || view.DemoniteRadioButton.IsChecked == true)
                selectedOres.Add(22); // Demonite Ore
            if (view.Tier6BothRadioButton.IsChecked == true || view.CrimtaneRadioButton.IsChecked == true)
                selectedOres.Add(204); // Crimtane Ore

            if (view.ObsidianCheckbox.IsChecked == true)
                selectedOres.Add(56); // Obsidian

            if (view.HellstoneCheckbox.IsChecked == true)
                selectedOres.Add(58); // Hellstone

            if (view.Tier9BothRadioButton.IsChecked == true || view.CobaltRadioButton.IsChecked == true)
                selectedOres.Add(107); // Cobalt Ore
            if (view.Tier9BothRadioButton.IsChecked == true || view.PalladiumRadioButton.IsChecked == true)
                selectedOres.Add(221); // Palladium Ore

            if (view.Tier10BothRadioButton.IsChecked == true || view.MythrilRadioButton.IsChecked == true)
                selectedOres.Add(108); // Mythril Ore
            if (view.Tier10BothRadioButton.IsChecked == true || view.OrichalcumRadioButton.IsChecked == true)
                selectedOres.Add(222); // Orichalcum Ore

            if (view.Tier11BothRadioButton.IsChecked == true || view.AdamantiteRadioButton.IsChecked == true)
                selectedOres.Add(111); // Adamantite Ore
            if (view.Tier11BothRadioButton.IsChecked == true || view.TitaniumRadioButton.IsChecked == true)
                selectedOres.Add(223); // Titanium Ore

            if (view.ChlorophyteCheckbox.IsChecked == true)
                selectedOres.Add(211); // Chlorophyte Ore

            if (view.LuminiteCheckbox.IsChecked == true)
                selectedOres.Add(408); // Luminite

            return selectedOres;
        }
    }
}
