using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common.Reactive;
using TEdit.ViewModel;
using TEdit.Editor;
using System.Collections.Generic;
using TEdit.View;
using TEdit.Geometry;
using TEdit.Render;

namespace TEdit.Terraria.Objects
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
            set { Set(nameof(TileName), ref _tileName, value); }
        }

        public bool IsPreviewTexture
        {
            get { return _isPreviewTexture; }
            set { Set(nameof(IsPreviewTexture), ref _isPreviewTexture, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(nameof(Name), ref _name, value); }
        }

        public FrameAnchor Anchor
        {
            get { return _anchor; }
            set { Set(nameof(Anchor), ref _anchor, value); }
        }

        public Vector2Short Origin
        {
            get { return _origin; }
            set { Set(nameof(Origin), ref _origin, value); }
        }

        public Vector2Short Size
        {
            get { return _size; }
            set { Set(nameof(Size), ref _size, value); }
        }

        public ushort Tile
        {
            get { return _tile; }
            set { Set(nameof(Tile), ref _tile, value); }
        }

        public WriteableBitmap Preview
        {
            get { return _preview; }
            set { Set(nameof(Preview), ref _preview, value); }
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
            ErrorLogging.TelemetryClient?.TrackEvent("PlaceSpriteOld", new Dictionary<string, string> { ["Tile"] = sprite.Tile.ToString() });

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
