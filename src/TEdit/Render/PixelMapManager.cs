using System;
using GalaSoft.MvvmLight;
using Microsoft.Xna.Framework;

namespace TEdit.Render
{
    public class PixelMapManager : ObservableObject
    {
        const int MaxTextureSize = 256;

        private int _tileWidth;
        private int _tileHeight;
        private int _tilesX;
        private int _tilesY;

        
        private Color[][] _colorBuffers;
        private bool[] _bufferUpdated;
         

        public bool[] BufferUpdated
        {
            get { return _bufferUpdated; }
            set { Set(nameof(BufferUpdated), ref _bufferUpdated, value); }
        }
        public Color[][] ColorBuffers
        {
            get { return _colorBuffers; }
            private set { Set(nameof(ColorBuffers), ref _colorBuffers, value); }
        }

        public int TilesY
        {
            get { return _tilesY; }
            private set { Set(nameof(TilesY), ref _tilesY, value); }
        }

        public int TilesX
        {
            get { return _tilesX; }
            private set { Set(nameof(TilesX), ref _tilesX, value); }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
            set { Set(nameof(TileHeight), ref _tileHeight, value); }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
            set { Set(nameof(TileWidth), ref _tileWidth, value); }
        }

        public void InitializeBuffers(int worldWidth, int worldHeight)
        {
            // Find maximum buffer size
            for (int h = MaxTextureSize; h >= 100; h--)
            {
                if (worldHeight % h == 0)
                {
                    TileHeight = h;
                    break;
                }
            }

            for (int w = MaxTextureSize; w >= 100; w--)
            {
                if (worldWidth % w == 0)
                {
                    TileWidth = w;
                    break;
                }
            }

            if (TileWidth < 100) TileWidth = 100;
            if (TileHeight < 100) TileHeight = 100;

            TilesX = (int)Math.Ceiling((double)worldWidth / TileWidth);
            TilesY = (int)Math.Ceiling((double)worldHeight / TileHeight);

            int tileCount = TilesX * TilesY;
            int tileSize = TileWidth * TileHeight;

            ColorBuffers = new Color[tileCount][];
            BufferUpdated = new bool[tileCount];

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
                    BufferUpdated[x + y*TilesX] = true;
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

            if (!BufferUpdated[tileIndex])
                BufferUpdated[tileIndex] = true;
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