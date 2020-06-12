using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using TEdit.ViewModel;
using TEdit.Editor;
using System.Collections.Generic;
using System.Linq;

namespace TEdit.Terraria.Objects
{
    public class SpriteSub : ObservableObject
    {
        public ushort Tile { get; set; }
        public int Style { get; set; }
        public Vector2Short UV { get; set; }
        public Vector2Short SizeTiles { get; set; }
        public WriteableBitmap Preview { get; set; }
        public Vector2Short SizeTexture { get; set; }
        public Vector2Short SizePixelsInterval { get; set; }
        public string Name { get; set; }
        public bool IsPreviewTexture { get; set; }
        public void GeneratePreview()
        {
            var bmp = new WriteableBitmap(SizeTexture.X, SizeTexture.Y, 96, 96, PixelFormats.Bgra32, null);
            var c = World.TileProperties[Tile].Color;
            bmp.Clear(Color.FromArgb(c.A, c.R, c.G, c.B));
            Preview = bmp;
            IsPreviewTexture = false;
        }

        internal Vector2Short[,] GetTiles()
        {
            var tiles = new Vector2Short[SizeTiles.X, SizeTiles.Y];
            for (int x = 0; x < SizeTiles.X; x++)
            {
                for (int y = 0; y < SizeTiles.Y; y++)
                {
                    tiles[x, y] = new Vector2Short((short)((SizePixelsInterval.X) * x + UV.X), (short)((SizePixelsInterval.Y) * y + UV.Y));
                }
            }

            return tiles;
        }

        public void Place(int destinationX, int destinationY, WorldViewModel wvm)
        {
            if (Tile == (ushort)TileType.ChristmasTree)
            {
                for (int x = 0; x < SizeTiles.X; x++)
                {
                    int tilex = x + destinationX;
                    for (int y = 0; y < SizeTiles.Y; y++)
                    {
                        int tiley = y + destinationY;
                        wvm.UndoManager.SaveTile(tilex, tiley);
                        Tile curtile = wvm.CurrentWorld.Tiles[tilex, tiley];
                        curtile.IsActive = true;
                        curtile.Type = Tile;
                        if (x == 0 && y == 0)
                            curtile.U = 10;
                        else
                            curtile.U = (short)x;
                        curtile.V = (short)y;

                        wvm.UpdateRenderPixel(tilex, tiley);
                        BlendRules.ResetUVCache(wvm, tilex, tiley, SizeTiles.X, SizeTiles.Y);

                    }
                }
            }
            else
            {
                for (int x = 0; x < SizeTiles.X; x++)
                {
                    Vector2Short[,] tiles = GetTiles();
                    int tilex = x + destinationX;
                    for (int y = 0; y < SizeTiles.Y; y++)
                    {
                        int tiley = y + destinationY;
                        wvm.UndoManager.SaveTile(tilex, tiley);
                        Tile curtile = wvm.CurrentWorld.Tiles[tilex, tiley];
                        curtile.IsActive = true;
                        curtile.Type = Tile;
                        curtile.U = tiles[x, y].X;
                        curtile.V = tiles[x, y].Y;

                        wvm.UpdateRenderPixel(tilex, tiley);
                        BlendRules.ResetUVCache(wvm, tilex, tiley, SizeTiles.X, SizeTiles.Y);

                    }
                }
            }
        }

        public void Place(int destinationX, int destinationY, ITileData world)
        {
            if (Tile == (ushort)TileType.ChristmasTree)
            {
                for (int x = 0; x < SizeTiles.X; x++)
                {
                    int tilex = x + destinationX;
                    for (int y = 0; y < SizeTiles.Y; y++)
                    {
                        int tiley = y + destinationY;
                        Tile curtile = world.Tiles[tilex, tiley];
                        curtile.IsActive = true;
                        curtile.Type = Tile;
                        if (x == 0 && y == 0)
                            curtile.U = 10;
                        else
                            curtile.U = (short)x;
                        curtile.V = (short)y;

                    }
                }
            }
            else
            {
                for (int x = 0; x < SizeTiles.X; x++)
                {
                    Vector2Short[,] tiles = GetTiles();
                    int tilex = x + destinationX;
                    for (int y = 0; y < SizeTiles.Y; y++)
                    {
                        int tiley = y + destinationY;
                        Tile curtile = world.Tiles[tilex, tiley];
                        curtile.IsActive = true;
                        curtile.Type = Tile;
                        curtile.U = tiles[x, y].X;
                        curtile.V = tiles[x, y].Y;
                    }
                }
            }
        }
    }

    public class SpriteFull : ObservableObject
    {
        public ushort Tile { get; set; }
        public WriteableBitmap Preview { get; set; }
        public string Name { get; set; }
        public Vector2Short[] SizeTiles { get; set; }
        public Vector2Short SizePixelsRender { get; set; }
        public Vector2Short SizePixelsInterval { get; set; }
        public Vector2Short SizeTexture { get; set; }
        public bool IsPreviewTexture { get; set; }
        public bool IsAnimated { get; set; }

        public SpriteSub Default => Styles.Values.FirstOrDefault();

        public Dictionary<int, SpriteSub> Styles { get; } = new Dictionary<int, SpriteSub>();

        public void GeneratePreview()
        {
            var bmp = new WriteableBitmap(SizeTexture.X, SizeTexture.Y, 96, 96, PixelFormats.Bgra32, null);
            var c = World.TileProperties[Tile].Color;
            bmp.Clear(Color.FromArgb(c.A, c.R, c.G, c.B));
            Preview = bmp;
            IsPreviewTexture = false;
        }

        /// <summary>
        /// Get a Sprite from one of it's UV's
        /// </summary>
        /// <param name="uv"></param>
        /// <returns></returns>
        public KeyValuePair<int, SpriteSub> GetStyleFromUV(Vector2Short uv)
            => Styles.AsQueryable().FirstOrDefault(kvp =>
                kvp.Value.UV.X >= uv.X &&
                kvp.Value.UV.Y >= uv.Y &&
                kvp.Value.UV.X + (kvp.Value.SizePixelsInterval.X * kvp.Value.SizeTiles.X) < uv.X &&
                kvp.Value.UV.Y + (kvp.Value.SizePixelsInterval.Y * kvp.Value.SizeTiles.Y) < uv.Y
            );
    }

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

        private Vector2Short[,] GetTiles()
        {
            var tiles = new Vector2Short[Size.X, Size.Y];
            var tileprop = World.TileProperties[Tile];
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    tiles[x, y] = new Vector2Short((short)((tileprop.TextureGrid.X + 2) * x + Origin.X), (short)((tileprop.TextureGrid.Y + 2) * y + Origin.Y));
                }
            }

            return tiles;
        }

        public static void PlaceSprite(int destinationX, int destinationY, Sprite sprite, WorldViewModel wvm)
        {
            if (sprite.Tile == (ushort)TileType.ChristmasTree)
            {
                for (int x = 0; x < sprite.Size.X; x++)
                {
                    int tilex = x + destinationX;
                    for (int y = 0; y < sprite.Size.Y; y++)
                    {
                        int tiley = y + destinationY;
                        wvm.UndoManager.SaveTile(tilex, tiley);
                        Tile curtile = wvm.CurrentWorld.Tiles[tilex, tiley];
                        curtile.IsActive = true;
                        curtile.Type = sprite.Tile;
                        if (x == 0 && y == 0)
                            curtile.U = 10;
                        else
                            curtile.U = (short)x;
                        curtile.V = (short)y;

                        wvm.UpdateRenderPixel(tilex, tiley);
                        BlendRules.ResetUVCache(wvm, tilex, tiley, sprite.Size.X, sprite.Size.Y);

                    }
                }
            }
            else
            {
                for (int x = 0; x < sprite.Size.X; x++)
                {
                    Vector2Short[,] tiles = sprite.GetTiles();
                    int tilex = x + destinationX;
                    for (int y = 0; y < sprite.Size.Y; y++)
                    {
                        int tiley = y + destinationY;
                        wvm.UndoManager.SaveTile(tilex, tiley);
                        Tile curtile = wvm.CurrentWorld.Tiles[tilex, tiley];
                        curtile.IsActive = true;
                        curtile.Type = sprite.Tile;
                        curtile.U = tiles[x, y].X;
                        curtile.V = tiles[x, y].Y;

                        wvm.UpdateRenderPixel(tilex, tiley);
                        BlendRules.ResetUVCache(wvm, tilex, tiley, sprite.Size.X, sprite.Size.Y);

                    }
                }
            }
        }

        public static void PlaceSprite(int destinationX, int destinationY, Sprite sprite, ITileData world)
        {
            if (sprite.Tile == (ushort)TileType.ChristmasTree)
            {
                for (int x = 0; x < sprite.Size.X; x++)
                {
                    int tilex = x + destinationX;
                    for (int y = 0; y < sprite.Size.Y; y++)
                    {
                        int tiley = y + destinationY;

                        Tile curtile = world.Tiles[tilex, tiley];
                        curtile.IsActive = true;
                        curtile.Type = sprite.Tile;
                        if (x == 0 && y == 0)
                            curtile.U = 10;
                        else
                            curtile.U = (short)x;
                        curtile.V = (short)y;

                    }
                }
            }
            else
            {
                for (int x = 0; x < sprite.Size.X; x++)
                {
                    Vector2Short[,] tiles = sprite.GetTiles();
                    int tilex = x + destinationX;
                    for (int y = 0; y < sprite.Size.Y; y++)
                    {
                        int tiley = y + destinationY;
                        Tile curtile = world.Tiles[tilex, tiley];
                        curtile.IsActive = true;
                        curtile.Type = sprite.Tile;
                        curtile.U = tiles[x, y].X;
                        curtile.V = tiles[x, y].Y;
                    }
                }
            }
        }

    }
}