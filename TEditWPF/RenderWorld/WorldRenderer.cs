using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEditWPF.TerrariaWorld;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.RenderWorld
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WorldRenderer
    {
        [Import("World")]
        private World World;

        [Import]
        private WorldImage _worldImage;
        private TileColors tileColors;

        public WorldRenderer()
        {
            tileColors = TileColors.Load("colors.txt");
        }

        public TileColors TileColors
        {
            get { return tileColors; }
            set { tileColors = value; }
        }

        public string GetTileName(Tile tile, out string wall)
        {
            string tilename = string.Empty;
            wall = string.Empty;

            TileProperties wallColor;
            if (tileColors.WallColor.TryGetValue(tile.Wall, out wallColor))
            {
                wall = wallColor.Name;
            }

            if (!tile.IsActive)
                return "[empty]";

            if (tile.Liquid > 0)
            {
                if (tile.IsLava)
                    tilename = String.Format("Lava({0})", tile.Liquid);
                else
                    tilename = String.Format("Water({0})", tile.Liquid);
            }
            else
            {
                TileProperties tileColor;
                if (tileColors.TileColor.TryGetValue(tile.Type, out tileColor))
                {
                    tilename = tileColor.Name;
                }
            }

            return tilename;
        }

        public static Color AlphaBlend(Color background, Color color)
        {
            var r = (byte)((color.A / 255F) * color.R + (1F - color.A / 255F) * background.R);
            var g = (byte)((color.A / 255F) * color.G + (1F - color.A / 255F) * background.G);
            var b = (byte)((color.A / 255F) * color.B + (1F - color.A / 255F) * background.B);
            return Color.FromArgb(255, r, g, b);
        }

        public void UpdateWorldImage(PointInt32 location)
        {
            UpdateWorldImage(new Int32Rect(location.X, location.Y, 1, 1));
        }

        public static IEnumerable<PointInt32> DrawLine(PointInt32 begin, PointInt32 end)
        {
            int y0 = begin.Y;
            int x0 = begin.X;
            int y1 = end.Y;
            int x1 = end.X;

            int dy = y1 - y0;
            int dx = x1 - x0;
            int stepx, stepy;

            if (dy < 0)
            {
                dy = -dy;
                stepy = -1;
            }
            else
            {
                stepy = 1;
            }
            if (dx < 0)
            {
                dx = -dx;
                stepx = -1;
            }
            else
            {
                stepx = 1;
            }
            dy <<= 1;
            dx <<= 1;

            //y0 *= width;
            //y1 *= width;
            yield return new PointInt32(x0, y0);
            if (dx > dy)
            {
                int fraction = dy - (dx >> 1);
                while (x0 != x1)
                {
                    if (fraction >= 0)
                    {
                        y0 += stepy;
                        fraction -= dx;
                    }
                    x0 += stepx;
                    fraction += dy;
                    yield return new PointInt32(x0, y0);
                }
            }
            else
            {
                int fraction = dx - (dy >> 1);
                while (y0 != y1)
                {
                    if (fraction >= 0)
                    {
                        x0 += stepx;
                        fraction -= dy;
                    }
                    y0 += stepy;
                    fraction += dx;
                    yield return new PointInt32(x0, y0);
                }
            }
        }

        public void UpdateWorldImage(Int32Rect area)
        {
            int width = area.Width;
            int height = area.Height;

            int stride = width * _worldImage.Image.Format.BitsPerPixel / 8;

            int numpixelbytes = height * width * _worldImage.Image.Format.BitsPerPixel / 8;

            var pixels = new byte[numpixelbytes];
            for (int x = 0; x < width; x++)
            {
                OnProgressChanged(this,
                                  new ProgressChangedEventArgs(
                                      (int)(x / (double)width * 100.0),
                                      "Rendering World..."));
                for (int y = 0; y < height; y++)
                {
                    Tile tile = World.Tiles[x + area.X, y + area.Y];
                    if (tile != null)
                    {
                        var c = GetTileColor(y, tile);

                        pixels[x * 4 + y * stride] = c.B;
                        pixels[x * 4 + y * stride + 1] = c.G;
                        pixels[x * 4 + y * stride + 2] = c.R;
                        pixels[x * 4 + y * stride + 3] = c.A;
                        //bmp.SetPixel(x - area.Left, y - area.Top, c);
                    }
                }
            }

            _worldImage.Image.Lock();
            _worldImage.Image.WritePixels(area, pixels, stride, 0);
            _worldImage.Image.AddDirtyRect(area);
            _worldImage.Image.Unlock();

            OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Update Complete."));
        }


        public WriteableBitmap RenderWorld()
        {
            int width = World.Header.MaxTiles.X;
            int height = World.Header.MaxTiles.Y;
            var wbmap = new WriteableBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            int stride = wbmap.BackBufferStride;

            int numpixelbytes = wbmap.PixelHeight * wbmap.PixelWidth * wbmap.Format.BitsPerPixel / 8;

            var pixels = new byte[numpixelbytes];
            for (int x = 0; x < width; x++)
            {
                OnProgressChanged(this,
                                  new ProgressChangedEventArgs(
                                      (int)(x / (double)width * 100.0),
                                      "Rendering World..."));

                for (int y = 0; y < height; y++)
                {
                    Tile tile = World.Tiles[x, y];
                    if (tile != null)
                    {
                        var c = GetTileColor(y, tile);


                        pixels[x * 4 + y * stride] = c.B;
                        pixels[x * 4 + y * stride + 1] = c.G;
                        pixels[x * 4 + y * stride + 2] = c.R;
                        pixels[x * 4 + y * stride + 3] = c.A;
                        //bmp.SetPixel(x - area.Left, y - area.Top, c);
                    }
                }
            }

            wbmap.WritePixels(new Int32Rect(0, 0, wbmap.PixelWidth, wbmap.PixelHeight), pixels,
                              wbmap.PixelWidth * wbmap.Format.BitsPerPixel / 8, 0);

            OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Complete."));

            return wbmap;
        }

        private Color GetTileColor(int y, Tile tile)
        {
            Color c;

            if (y > World.Header.WorldRockLayer)
                c = tileColors.WallColor[1].Color;
            else if (y > World.Header.WorldSurface)
                c = tileColors.WallColor[2].Color;
            else
                c = tileColors.WallColor[tile.Wall].Color;


            if (tile.IsActive)
                c = AlphaBlend(c, tileColors.TileColor[tile.Type].Color);
            

            if (tile.Liquid > 0)
            {
                if (tile.IsLava)
                    c = AlphaBlend(c, tileColors.LiquidColor[2].Color);
                else
                    c = AlphaBlend(c, tileColors.LiquidColor[1].Color);
            }
            return c;
        }

        public event ProgressChangedEventHandler ProgressChanged;

        protected virtual void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(sender, e);
        }
    }
}