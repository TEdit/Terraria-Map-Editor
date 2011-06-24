using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using TEditWPF.Common;
using System.Threading.Tasks;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.RenderWorld
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WorldRenderer
    {
        private TileColors tileColors;
        public TileColors TileColors
        {
            get
            {
                return tileColors;
            }
            set
            {
                tileColors = value;
            }
        }

        [Import("World")]
        private TerrariaWorld.World World;

        [Import]
        private WorldImage _worldImage;

        public WorldRenderer()
        {
            this.tileColors = TileColors.Load("colors.txt");
        }

        public string GetTileName(TerrariaWorld.Tile tile, out string wall)
        {
            string tilename = string.Empty;
            wall = string.Empty;

            TileProperties wallColor;
            if (this.tileColors.WallColor.TryGetValue(tile.Wall, out wallColor))
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
                if (this.tileColors.TileColor.TryGetValue(tile.Type, out tileColor))
                {
                    tilename = tileColor.Name;
                }
            }

            return tilename;
        }

        public static Color AlphaBlend(Color background, Color color)
        {
            byte r = (byte)((color.A / 255F) * (float)color.R + (1F - color.A / 255F) * (float)background.R);
            byte g = (byte)((color.A / 255F) * (float)color.G + (1F - color.A / 255F) * (float)background.G);
            byte b = (byte)((color.A / 255F) * (float)color.B + (1F - color.A / 255F) * (float)background.B);
            return Color.FromArgb(255, r, g, b);
        }

        public void UpdateWorldImage(PointInt32 location)
        {
            UpdateWorldImage(new Int32Rect(location.X, location.Y, 1, 1));
        }

        public static IEnumerable<PointInt32> DrawLine1(PointInt32 begin, PointInt32 end)
        {
            yield return begin;

            PointInt32 nextPoint = begin;
            int deltax = end.X - begin.X;
            int deltay = end.Y - begin.Y;
            int error = deltax / 2;
            int ystep = 1;

            if (end.Y < begin.Y)
            {
                ystep = -1;
            }

            while (nextPoint.X < end.X)
            {
                if (nextPoint != begin) yield return nextPoint;
                nextPoint.X++;

                error -= deltay;
                if (error < 0)
                {
                    nextPoint.Y += ystep;
                    error += deltax;
                }
                yield return nextPoint;
            }

            yield return end;
        }


        public static IEnumerable<PointInt32> DrawLine(PointInt32 begin, PointInt32 end)
        {
            var y0 = begin.Y;
            var x0 = begin.X;
            var y1 = end.Y;
            var x1 = end.X;

            int dy = y1 - y0;
            int dx = x1 - x0;
            int stepx, stepy;

            if (dy < 0) { dy = -dy; stepy = -1; } else { stepy = 1; }
            if (dx < 0) { dx = -dx; stepx = -1; } else { stepx = 1; }
            dy <<= 1;
            dx <<= 1;

            //y0 *= width;
            //y1 *= width;
            yield return new PointInt32(x0,y0);
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

            byte[] pixels = new byte[numpixelbytes];
            for (int x = 0; x < width; x++)
            {
                this.OnProgressChanged(this,
                                    new ProgressChangedEventArgs(
                                        (int)((double)x / (double)width * 100.0),
                                        "Rendering World..."));
                for (int y = 0; y < height; y++)
                {
                    TerrariaWorld.Tile tile = World.Tiles[x, y];
                    if (tile != null)
                    {
                        Color c;

                        if (y > World.Header.WorldRockLayer)
                            c = tileColors.WallColor[1].Color;
                        else if (y > World.Header.WorldSurface)
                            c = tileColors.WallColor[2].Color;
                        else
                            c = tileColors.WallColor[0].Color;

                        if (tile.Wall > 0)
                            c = tileColors.WallColor[tile.Wall].Color;

                        if (tile.Liquid > 0)
                        {
                            if (tile.IsLava)
                                c = tileColors.LiquidColor[2].Color;
                            else
                            {
                                // Alphablend water
                                c = AlphaBlend(c, tileColors.LiquidColor[1].Color);
                            }
                        }

                        if (tile.IsActive)
                            c = tileColors.TileColor[tile.Type].Color;
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

            this.OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Update Complete."));
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

            byte[] pixels = new byte[numpixelbytes];
            for (int x = 0; x < width; x++)
            {
                this.OnProgressChanged(this,
                                    new ProgressChangedEventArgs(
                                        (int)((double)x / (double)width * 100.0),
                                        "Rendering World..."));
                for (int y = 0; y < height; y++)
                {
                    TerrariaWorld.Tile tile = World.Tiles[x, y];
                    if (tile != null)
                    {
                        Color c;

                        if (y > World.Header.WorldRockLayer)
                            c = tileColors.WallColor[1].Color;
                        else if (y > World.Header.WorldSurface)
                            c = tileColors.WallColor[2].Color;
                        else
                            c = tileColors.WallColor[0].Color;

                        if (tile.Wall > 0)
                            c = tileColors.WallColor[tile.Wall].Color;

                        if (tile.Liquid > 0)
                        {
                            if (tile.IsLava)
                                c = tileColors.LiquidColor[2].Color;
                            else
                            {
                                // Alphablend water
                                c = AlphaBlend(c, tileColors.LiquidColor[1].Color);
                            }
                        }

                        if (tile.IsActive)
                            c = tileColors.TileColor[tile.Type].Color;
                        pixels[x * 4 + y * stride] = c.B;
                        pixels[x * 4 + y * stride + 1] = c.G;
                        pixels[x * 4 + y * stride + 2] = c.R;
                        pixels[x * 4 + y * stride + 3] = c.A;
                        //bmp.SetPixel(x - area.Left, y - area.Top, c);
                    }
                }
            }

            wbmap.WritePixels(new Int32Rect(0, 0, wbmap.PixelWidth, wbmap.PixelHeight), pixels, wbmap.PixelWidth * wbmap.Format.BitsPerPixel / 8, 0);

            this.OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Complete."));

            return wbmap;
        }

        //public void RenderRegion(TerrariaWorld.World world, Int32Rect area)
        //{
        //    var writeableBitmap = new WriteableBitmap(
        //        (int)w.ActualWidth,
        //        (int)w.ActualHeight,
        //        96,
        //        96,
        //        PixelFormats.Bgr32,
        //        null);

        //    Bitmap bmp = new Bitmap(area.Width, area.Height);

        //    for (int x = area.Left; x < area.Right; x++)
        //    {
        //        this.OnProgressChanged(this, new ProgressChangedEventArgs((int)((double)x / (double)area.Right * 100.0), "Rendering World..."));
        //        for (int y = area.Top; y < area.Bottom; y++)
        //        {
        //            TerrariaWorld.Tile tile = world.Tiles[x, y];
        //            if (tile != null)
        //            {
        //                Color c;

        //                if (y > world.Header.WorldRockLayer)
        //                    c = tileColors.WallColor[1].Color;
        //                else if (y > world.Header.WorldSurface)
        //                    c = tileColors.WallColor[2].Color;
        //                else
        //                    c = tileColors.WallColor[0].Color;

        //                if (tile.Wall > 0)
        //                    c = tileColors.WallColor[tile.Wall].Color;

        //                if (tile.Liquid > 0)
        //                {
        //                    if (tile.IsLava)
        //                        c = tileColors.LiquidColor[2].Color;
        //                    else
        //                    {
        //                        // Alphablend water
        //                        c = Common.Utility.AlphaBlend(c, tileColors.LiquidColor[1].Color);
        //                    }
        //                }

        //                if (tile.IsActive)
        //                    c = tileColors.TileColor[tile.Type].Color;

        //                bmp.SetPixel(x - area.Left, y - area.Top, c);
        //            }
        //        }
        //    }

        //    this.OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Complete."));

        //    return bmp;
        //}

        //public void UpdateMultipleTile(TerrariaWorld.World world, Bitmap worldImage, List<Point> updatedCoords)
        //{
        //    for (int p = 0; p < updatedCoords.Count; p++)
        //    {
        //        this.OnProgressChanged(this, new ProgressChangedEventArgs((int)((float)p / (float)updatedCoords.Count * 100.0), "Updating World..."));

        //        int x = updatedCoords[p].X;
        //        int y = updatedCoords[p].Y;

        //        TerrariaWorld.Game.Tile tile = world.Tiles[x, y];
        //        if (tile != null)
        //        {
        //            Color c;

        //            if (y > world.Header.WorldRockLayer)
        //                c = tileColors.WallColor[1].Color;
        //            else if (y > world.Header.WorldSurface)
        //                c = tileColors.WallColor[2].Color;
        //            else
        //                c = tileColors.WallColor[0].Color;

        //            if (tile.Wall > 0)
        //                c = tileColors.WallColor[tile.Wall].Color;

        //            if (tile.Liquid > 0)
        //            {
        //                if (tile.IsLava)
        //                    c = tileColors.LiquidColor[2].Color;
        //                else
        //                {
        //                    // Alphablend water
        //                    c = Common.Utility.AlphaBlend(c, tileColors.LiquidColor[1].Color);
        //                }
        //            }

        //            if (tile.IsActive)
        //                c = tileColors.TileColor[tile.Type].Color;

        //            worldImage.SetPixel(x, y, c);
        //        }
        //    }

        //    this.OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Complete."));
        //}

        //public void UpdateSingleTile(TerrariaWorld.Game.World world, Bitmap worldImage, Point updatedCoords)
        //{

        //    int x = updatedCoords.X;
        //    int y = updatedCoords.Y;

        //    TerrariaWorld.Game.Tile tile = world.Tiles[x, y];
        //    if (tile != null)
        //    {
        //        Color c;

        //        if (y > world.Header.WorldRockLayer)
        //            c = tileColors.WallColor[1].Color;
        //        else if (y > world.Header.WorldSurface)
        //            c = tileColors.WallColor[2].Color;
        //        else
        //            c = tileColors.WallColor[0].Color;

        //        if (tile.Wall > 0)
        //            c = tileColors.WallColor[tile.Wall].Color;

        //        if (tile.Liquid > 0)
        //        {
        //            if (tile.IsLava)
        //                c = tileColors.LiquidColor[2].Color;
        //            else
        //            {
        //                // Alphablend water
        //                c = Common.Utility.AlphaBlend(c, tileColors.LiquidColor[1].Color);
        //            }
        //        }

        //        if (tile.IsActive)
        //            c = tileColors.TileColor[tile.Type].Color;

        //        worldImage.SetPixel(x, y, c);
        //    }

        //}


        public event ProgressChangedEventHandler ProgressChanged;
        protected virtual void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(sender, e);

        }
    }
}
