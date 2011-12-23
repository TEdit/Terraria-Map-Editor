using System;
using BCCL.MvvmLight;
using Microsoft.Xna.Framework;

namespace TEditXna.Render
{
    public class PixelMapManager : ObservableObject
    {
        private int _tileWidth;
        private int _tileHeight;
        private int _tilesX;
        private int _tilesY;

        private Color[][] _colorBuffers;

        public Color[][] ColorBuffers
        {
            get { return _colorBuffers; }
            private set { Set("ColorBuffers", ref _colorBuffers, value); }
        }

        public int TilesY
        {
            get { return _tilesY; }
            private set { Set("TilesY", ref _tilesY, value); }
        }

        public int TilesX
        {
            get { return _tilesX; }
            private set { Set("TilesX", ref _tilesX, value); }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
            set { Set("TileHeight", ref _tileHeight, value); }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
            set { Set("TileWidth", ref _tileWidth, value); }
        }

        public PixelMapManager(int tileWidth, int tileHeight)
        {
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        public void InitializeBuffers(int worldWidth, int worldHeight)
        {
            TilesX = (int)Math.Ceiling((double)worldWidth / TileWidth);
            TilesY = (int)Math.Ceiling((double)worldHeight / TileHeight);

            int tileCount = TilesX * TilesY;
            int tileSize = TileWidth * TileHeight;

            ColorBuffers = new Color[tileCount][];

            for (int x = 0; x < TilesX; x++)
            {
                for (int y = 0; y < TilesY; y++)
                {
                    var curBuffer = new Color[tileSize];
                    for (int i = 0; i < tileSize; i++)
                    {
                        curBuffer[i] = new Color(0, 0, 0, 0);
                    }
                    ColorBuffers[x + y * TilesX] = curBuffer;
                }
            }
        }

        public void SetPixelColor(int x, int y, Color color)
        {
            int curTileX = x / TileWidth;
            int curTileY = y / TileHeight;

            int curPixelX = x - (curTileX * TileWidth);
            int curPixelY = y - (curTileY * TileHeight);

            int tileIndex = curTileX + curTileY * TilesX;
            int pixelIndex = curPixelX + curPixelY * TileWidth;

            ColorBuffers[tileIndex][pixelIndex] = color;
        }

        public Color GetPixelColor(int x, int y)
        {
            int curTileX = x / TileWidth;
            int curTileY = y / TileHeight;

            int curPixelX = x - (curTileX * TileWidth);
            int curPixelY = y - (curTileY * TileHeight);

            int tileIndex = curTileX + curTileY * TilesX;
            int pixelIndex = curPixelX + curPixelY * TileWidth;

            return ColorBuffers[tileIndex][pixelIndex];
        }
    }
}