using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common.Reactive;
using TEdit.ViewModel;
using TEdit.Editor;
using System.Collections.Generic;
using TEdit.Geometry;
using TEdit.Common;
using TEdit.Render;

namespace TEdit.Terraria.Objects
{
    public class SpriteSub : ObservableObject
    {
        public ushort Tile { get; set; }
        public int Style { get; set; }
        public TEditColor StyleColor { get; set; }
        public Vector2Short UV { get; set; }
        public Vector2Short SizeTiles { get; set; }
        public WriteableBitmap Preview { get; set; }
        public Vector2Short SizeTexture { get; set; }
        public Vector2Short SizePixelsInterval { get; set; }
        public FrameAnchor Anchor { get; set; }
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
                    var curSize = SizePixelsInterval;
                    var tileX = ((curSize.X) * x + UV.X);
                    var tileY = ((curSize.Y) * y + UV.Y);

                    if (Tile == 388 || Tile == 389)
                    {
                        switch (y)
                        {
                            case 0:
                                tileY = UV.Y;
                                break;
                            case 1:
                                tileY = 20 + UV.Y;
                                break;
                            case 2:
                                tileY = 20 + 18 + UV.Y;
                                break;
                            case 3:
                                tileY = 20 + 18 + 18 + UV.Y;
                                break;
                            case 4:
                                tileY = 20 + 18 + 18 + 18 + UV.Y;
                                break;
                        }
                    }

                    tiles[x, y] = new Vector2Short((short)tileX, (short)tileY);
                }
            }

            return tiles;
        }

        public void Place(int destinationX, int destinationY, WorldViewModel wvm)
        {
            ErrorLogging.TelemetryClient?.TrackEvent("PlaceSprite", new Dictionary<string, string> { ["Tile"] = Tile.ToString(), ["UV"] = UV.ToString()});

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
            ErrorLogging.TelemetryClient?.TrackEvent("PlaceSprite", new Dictionary<string, string> { ["Tile"] = Tile.ToString(), ["UV"] = UV.ToString()});

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
}
