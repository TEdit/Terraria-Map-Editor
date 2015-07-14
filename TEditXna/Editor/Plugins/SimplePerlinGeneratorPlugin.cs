using System;
using Microsoft.Xna.Framework;
using TEditXna.ViewModel;
using TEditXNA.Terraria;

namespace TEditXna.Editor.Plugins
{
    public sealed class SimplePerlinGeneratorPlugin : BasePlugin
    {
        private PerlinNoise _noiseGenerator;
        public SimplePerlinGeneratorPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            _noiseGenerator = new PerlinNoise(1);
            Name = "Simple Ore Generator";
        }

        public override void Execute()
        {
            if (_wvm.CurrentWorld == null)
                return;

            // refresh generator if needed
            //if (_noiseGenerator.Seed != _wvm.CurrentWorld.WorldId)
            //{
            _noiseGenerator = new PerlinNoise((int)DateTime.Now.Ticks);
            //}

            var area = new Rectangle(0, (int)_wvm.CurrentWorld.GroundLevel, _wvm.CurrentWorld.TilesWide, _wvm.CurrentWorld.TilesHigh - (int)_wvm.CurrentWorld.GroundLevel - 196);

            if (_wvm.Selection.IsActive)
            {
                if (!_wvm.Selection.SelectionArea.Intersect(new Rectangle(0, 0, _wvm.CurrentWorld.TilesWide, _wvm.CurrentWorld.TilesHigh), out area))
                    return;
            }

            if (area.Width <= 0 || area.Height <= 0)
                return;

            var tile = (ushort)_wvm.TilePicker.Tile;
            for (int x = area.Left; x < area.Right; x++)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    var result = TestOctaveGenerator(x, y);
                    if (result > 0.6 && result < 0.75)
                    {
                        var curTile = _wvm.CurrentWorld.Tiles[x, y];

                        // Only replace if the tile is dirt or stone and if the wall is empty, stone or dirt.
                        if (curTile.IsActive && (curTile.Type == (int)TileType.DirtBlock || curTile.Type == (int)TileType.StoneBlock) && (curTile.Wall == 0 || curTile.Wall == 1 || curTile.Wall == 2))
                        {
                            _wvm.UndoManager.SaveTile(x, y);
                            _wvm.CurrentWorld.Tiles[x, y].IsActive = true;
                            _wvm.CurrentWorld.Tiles[x, y].Type = tile;
                            _wvm.UpdateRenderPixel(x, y);
                        }
                    }
                }
            }
            _wvm.UndoManager.SaveUndo();
        }

        private float TestOctaveGenerator(int worldX, int worldY)
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
    }
}