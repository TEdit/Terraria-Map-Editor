using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Xna.Framework;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.Objects;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

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

        var tileMapping = GetRandomTileMapping(blockSettings);
        var wallMapping = GetRandomWallMapping(wallSettings);

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

        // Change every tile in the randomizationArea according to the tilemapping
        for (int x = randomizationArea.Left; x < randomizationArea.Right; x++)
        {
            for (int y = randomizationArea.Top; y < randomizationArea.Bottom; y++)
            {
                if (view.EnableUndo)
                    _wvm.UndoManager.SaveTile(x, y); // Store tile for undo

                Tile t = _wvm.CurrentWorld.Tiles[x, y];

                if (tileMapping.ContainsKey(t.Type))
                    t.Type = (ushort)tileMapping[t.Type];

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
    private void AddSupportsToDependentBlocks(RectangleInt32 randomizationArea)
    {
        Dictionary<int, int> VineHangTile = new()
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

                if (VineHangTile.ContainsKey(t.Type))
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
                            else if (tAbove.Type == VineHangTile[t.Type])
                            {
                                break;
                            }
                        }

                        t.Type = (ushort)VineHangTile[t.Type];
                        break;
                    }
                }
                else if (t.Type == 80) // A cactus
                {
                    int currentY = y;
                    while (true)
                    {
                        if (currentY + 1 >= 0)
                        {
                            Tile tAbove = _wvm.CurrentWorld.Tiles[x, currentY];
                            if (tAbove.Type == t.Type)
                            {
                                currentY++;
                                continue;
                            }
                            else if (tAbove.Type == 53)
                            {
                                break;
                            }
                        }

                        t.Type = 53;
                        break;
                    }
                }
            }
        }
    }

    private void AddSupportsToGravityBlocks(RectangleInt32 randomizationArea)
    {
        Dictionary<int, int> GravitySupportTile = new()
        {
            { 53, 397 },// Sand, Hardened sand
            { 112, 398 }, // Ebonsand, Corrupt hardened sand
            { 234, 399 }, // Crimsand, Crimson hardened sand
            { 116, 402 }, // Pearlsand, hallow hardened sand
            { 123, 1 }, // Silt, stone
            { 224, 161 }, // Slush, ice
            { 330, 7 }, // Copper coin pile, copper ore
            { 331, 9 }, // Silver coin pile, silver ore
            { 332, 8 }, // Gold coin pile, gold ore
            { 333, 169 }, // Plat coin pile, plat ore
        };

        HashSet<int> Sands = new()
        {
            53,
            112,
            234,
            116,
        };

        for (int x = randomizationArea.Left; x < randomizationArea.Right; x++)
        {
            for (int y = randomizationArea.Top; y < randomizationArea.Bottom; y++)
            {
                Tile t = _wvm.CurrentWorld.Tiles[x, y];
                if (!GravitySupportTile.Keys.Contains(t.Type))
                    continue;

                if (y + 1 > randomizationArea.Bottom)
                    continue;

                Tile tBelow = _wvm.CurrentWorld.Tiles[x, y + 1];

                if (tBelow.IsActive)
                    continue;

                if (Sands.Contains(t.Type))
                {
                    if (y + 1 > randomizationArea.Bottom)
                        continue;

                    Tile tAbove = _wvm.CurrentWorld.Tiles[x, y - 1];
                    if (tAbove.Type == 80)
                        continue;
                }

                t.Type = (ushort)GravitySupportTile[t.Type];
            }
        }
    }

    private Dictionary<int, int> GetRandomTileMapping(BlockRandomizerSettings settings)
    {
        Random rng = new(settings.Seed);

        // Set up lists
        List<int> fromTiles = new(WorldConfiguration.TileBricks.Select(x => x.Id));
        fromTiles.Remove(-1);

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

        List<int> fromWalls = new(WorldConfiguration.WallProperties.Select(x => x.Id));
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
