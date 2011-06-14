using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Threading.Tasks;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.RenderWorld
{
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

        public WriteableBitmap RenderWorld(TerrariaWorld.World world)
        {
            int width = world.Header.MaxTiles.X;
            int height = world.Header.MaxTiles.Y;
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
                    TerrariaWorld.Tile tile = world.Tiles[x, y];
                    if (tile != null)
                    {
                        Color c;

                        if (y > world.Header.WorldRockLayer)
                            c = tileColors.WallColor[1].Color;
                        else if (y > world.Header.WorldSurface)
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

            wbmap.WritePixels(new Int32Rect(0, 0, wbmap.PixelWidth, wbmap.PixelHeight), pixels, wbmap.PixelWidth*wbmap.Format.BitsPerPixel/8, 0);

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
