using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Xna.Framework;
using TEdit.Terraria;
using TEdit.Terraria.Objects;
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
                NoDisappearingBlocks = view.NoDisappearingBlocks,
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

                    if (view.EnableWallRandomize && wallMapping.ContainsKey(t.Wall))
                        t.Wall = (ushort)wallMapping[t.Wall];
                }
            }

            if (view.SupportDependentBlocks)
                AddSupportsToDependentBlocks(randomizationArea);

            if (view.SupportGravityBlocks)
                AddSupportsToGravityBlocks(randomizationArea);

            if (view.EnableUndo)
                _wvm.UndoManager.SaveUndo();

            _wvm.UpdateRenderRegion(randomizationArea); // Re-render map
            _wvm.MinimapImage = Render.RenderMiniMap.Render(_wvm.CurrentWorld); // Update Minimap
        }

        /// <summary>
        /// This function adds the conditional blocks to all tiles that require them. This includes vines and cacti.
        /// </summary>
        private void AddSupportsToDependentBlocks(Rectangle randomizationArea)
        {
            Dictionary<int, int> VineHangBlock = new()
            {
                { 52, 2 },// Vine
                { 205, 199 },// Crimson vine
                { 636, 23 },// Corrupt vine
                { 115, 109 },// hallow vine
                { 62, 60 },// Jungle vine
                { 382, 2 },// flower vine
                { 528, 70 },// mushroom vine
                { 638, 633 },// ash vine
            };

            for (int x = randomizationArea.Left; x < randomizationArea.Right; x++)
            {
                for (int y = randomizationArea.Top; y < randomizationArea.Bottom; y++)
                {
                    Tile t = _wvm.CurrentWorld.Tiles[x, y];

                    if (VineHangBlock.ContainsKey(t.Type))
                    {
                        int currentY = y;
                        while (true)
                        {
                            if (currentY - 1 >= 0)
                            {
                                Tile tAbove = _wvm.CurrentWorld.Tiles[x, currentY];
                                if (tAbove.Type == t.Type)
                                {
                                    currentY--;
                                    continue;
                                }
                                else if (tAbove.Type == VineHangBlock[t.Type])
                                {
                                    break;
                                }
                            }

                            t.Type = (ushort)VineHangBlock[t.Type];
                            break;
                        }
                    }
                    else if (t.Type == 80) // A cactus
                    {

                    }
                }
            }
        }

        private void AddSupportsToGravityBlocks(Rectangle randomizationArea)
        {
            throw new NotImplementedException();
        }

        private Dictionary<int, int> GetRandomBlockMapping(BlockRandomizerSettings settings)
        {
            Random rng = new(settings.Seed);

            // Set up lists
            List<int> fromTiles = new(Terraria.World.TileBricks.Select(x => x.Id));

            if (settings.NoDisappearingBlocks)
                fromTiles = fromTiles.Where(x => !DisappearingTiles.Contains(x)).ToList();

            List<int> toTiles = new(fromTiles);

            // Shuffle to list
            for (int n = toTiles.Count; n > 1;)
            {
                int k = rng.Next(n--);
                int temp = toTiles[n];
                toTiles[n] = toTiles[k];
                toTiles[k] = temp;
            }

            // Add lists to a dictionary
            Dictionary<int, int> mapping = new();
            for (int i = 0; i < fromTiles.Count; i++)
            {
                mapping.Add(fromTiles[i], toTiles[i % toTiles.Count]);
            }

            return mapping;
        }

        private List<int> DisappearingTiles = new()
        {
            127, // Ice Rod Ice
            504, // Mystic snake rope
        };

        private Dictionary<int, int> GetRandomWallMapping(WallRandomizerSetting settings)
        {
            Dictionary<int, int> mapping = new();
            Random rng = new(settings.Seed);

            List<int> fromWalls = new(Terraria.World.WallProperties.Select(x => x.Id));
            fromWalls.Remove(0); // Remove Sky from the walls to be shuffled
            List<int> toWalls = new(fromWalls);

            int n = toWalls.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                int temp = toWalls[n];
                toWalls[n] = toWalls[k];
                toWalls[k] = temp;
            }

            for (int i = 0; i < fromWalls.Count; i++)
            {
                mapping.Add(fromWalls[i], toWalls[i]);
            }

            return mapping;
        }

        private struct BlockRandomizerSettings
        {
            public int Seed { get; set; }
            public bool NoDisappearingBlocks { get; set; }
        }

        private struct WallRandomizerSetting
        {
            public int Seed { get; set; }
        }
    }
}
