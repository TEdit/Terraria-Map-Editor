using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TerrariaMapEditor.Renderer
{
    public class WorldRenderer
    {
        private Renderer.TileColors tileColors;
        public Renderer.TileColors TileColors
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
            this.tileColors = Renderer.TileColors.Load("colors.txt");
        }

        public Bitmap RenderRegion(TerrariaWorld.Game.World world, Rectangle area)
        {
            Bitmap bmp = new Bitmap(area.Width, area.Height);

            for (int x = area.Left; x < area.Right; x++)
            {
                this.OnProgressChanged(this, new ProgressChangedEventArgs((int)((double)x / (double)area.Right * 100.0), "Rendering World..."));
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    TerrariaWorld.Game.Tile tile = world.Tiles[x, y];
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
                                c = Common.Utility.AlphaBlend(c, tileColors.LiquidColor[1].Color);
                            }
                        }

                        if (tile.IsActive)
                            c = tileColors.TileColor[tile.Type].Color;

                        bmp.SetPixel(x - area.Left, y - area.Top, c);
                    }
                }
            }

            this.OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Complete."));

            return bmp;
        }

        public void UpdateMultipleTile(TerrariaWorld.Game.World world, Bitmap worldImage, List<Point> updatedCoords)
        {
            for (int p = 0; p < updatedCoords.Count; p++)
            {
                this.OnProgressChanged(this, new ProgressChangedEventArgs((int)((float)p / (float)updatedCoords.Count * 100.0), "Updating World..."));

                int x = updatedCoords[p].X;
                int y = updatedCoords[p].Y;

                TerrariaWorld.Game.Tile tile = world.Tiles[x, y];
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
                            c = Common.Utility.AlphaBlend(c, tileColors.LiquidColor[1].Color);
                        }
                    }

                    if (tile.IsActive)
                        c = tileColors.TileColor[tile.Type].Color;

                    worldImage.SetPixel(x, y, c);
                }
            }

            this.OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Complete."));
        }

        public void UpdateSingleTile(TerrariaWorld.Game.World world, Bitmap worldImage, Point updatedCoords)
        {

            int x = updatedCoords.X;
            int y = updatedCoords.Y;

            TerrariaWorld.Game.Tile tile = world.Tiles[x, y];
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
                        c = Common.Utility.AlphaBlend(c, tileColors.LiquidColor[1].Color);
                    }
                }

                if (tile.IsActive)
                    c = tileColors.TileColor[tile.Type].Color;

                worldImage.SetPixel(x, y, c);
            }

        }


        public event ProgressChangedEventHandler ProgressChanged;
        protected virtual void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(sender, e);

        }
    }
}
