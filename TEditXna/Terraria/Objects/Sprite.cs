using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;

namespace TEditXNA.Terraria.Objects
{
    public class Sprite : ObservableObject
    {
        private WriteableBitmap _preview;
        private ushort _tile;
        private Vector2Short _size;
        private Vector2Short _origin;
        private FrameAnchor _anchor;
        private string _name;
        private bool _isPreviewTexture;
        private string _tileName;

        public string TileName
        {
            get { return _tileName; }
            set { Set("TileName", ref _tileName, value); }
        }

        public bool IsPreviewTexture
        {
            get { return _isPreviewTexture; }
            set { Set("IsPreviewTexture", ref _isPreviewTexture, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }

        public FrameAnchor Anchor
        {
            get { return _anchor; }
            set { Set("Anchor", ref _anchor, value); }
        }

        public Vector2Short Origin
        {
            get { return _origin; }
            set { Set("Origin", ref _origin, value); }
        }

        public Vector2Short Size
        {
            get { return _size; }
            set { Set("Size", ref _size, value); }
        }

        public ushort Tile
        {
            get { return _tile; }
            set { Set("Tile", ref _tile, value); }
        }

        public WriteableBitmap Preview
        {
            get { return _preview; }
            set { Set("Preview", ref _preview, value); }
        }

        public void GeneratePreview()
        {
            var bmp = new WriteableBitmap(_size.X, _size.Y, 96, 96, PixelFormats.Bgra32, null);
            var c = World.TileProperties[Tile].Color;
            bmp.Clear(Color.FromArgb(c.A, c.R, c.G, c.B));
            Preview = bmp;
            IsPreviewTexture = false;
        }

        public Vector2Short[,] GetTiles()
        {
            var tiles = new Vector2Short[Size.X, Size.Y];
            var tileprop = World.TileProperties[Tile];
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    tiles[x, y] = new Vector2Short((short)((tileprop.TextureGrid.X+2) * x + Origin.X), (short)((tileprop.TextureGrid.Y+2) * y + Origin.Y));
                }
            }

            return tiles;
        }

    }
}