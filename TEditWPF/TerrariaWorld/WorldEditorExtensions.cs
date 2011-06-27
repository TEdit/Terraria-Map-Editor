// -----------------------------------------------------------------------
// <copyright file="WorldExtensions.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel.Composition;
using System.Windows;
using TEditWPF.TerrariaWorld.Structures;
using TEditWPF.Tools;

namespace TEditWPF.TerrariaWorld
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class WorldEditorExtensions
    {

#region Line
        
#endregion
        #region Flood Fill

        public static void FloodFillContig(this World world, PointInt32 start, Int32Rect area, TilePicker tile)
        {
            
        }

        public static void FillRectangle(this World world, Int32Rect area, TilePicker tile)
        {
            // validate area
            if (area.X < 0)
            {
                area.Width += area.X;
                area.X = 0;
            }
            if (area.Y < 0)
            {
                area.Height += area.Y;
                area.Y = 0;
            }
            if ((area.Y + area.Height) >= world.Header.MaxTiles.Y)
            {
                area.Height += world.Header.MaxTiles.Y - (area.Y + area.Height);
            }
            if ((area.X + area.Width) >= world.Header.MaxTiles.X)
            {
                area.Width += world.Header.MaxTiles.X - (area.X + area.Width);
            }

            for (int x = area.X; x < area.X+area.Width; x++)
            {
                for (int y = area.Y; y < area.Y + area.Height; y++)
                {
                    SetTileXY(world, x, y, tile);
                } 
            }
        }

        public static void SetTileXY(this World world, int x, int y, TilePicker tile)
        {
            Tile curTile = world.Tiles[x, y];

            if (tile.Tile.IsActive)
            {
                if (!tile.TileMask.IsActive || (curTile.Type == tile.TileMask.Value && curTile.IsActive))
                {
                    if (tile.IsEraser)
                    {
                        curTile.IsActive = false;
                    }
                    else
                    {
                        //TODO: i don't like redundant conditionals, but its a fix
                        if (!tile.TileMask.IsActive)
                            curTile.IsActive = true;

                        curTile.Type = tile.Tile.Value;

                        // if the tile is solid and there isn't a mask, remove the liquid
                        if (!tile.TileMask.IsActive && TileProperties.TileSolid[curTile.Type] && curTile.Liquid > 0)
                            curTile.Liquid = 0;
                    }
                }
            }

            if (tile.Wall.IsActive)
            {
                if (!tile.WallMask.IsActive || (curTile.Wall == tile.WallMask.Value))
                {
                    if (tile.IsEraser)
                        curTile.Wall = 0;
                    else
                        curTile.Wall = tile.Wall.Value;
                }
            }

            if (tile.Liquid.IsActive && (!curTile.IsActive || !TileProperties.TileSolid[curTile.Type]))
            {
                if (tile.IsEraser)
                {
                    curTile.Liquid = 0;
                    curTile.IsLava = false;
                }
                else
                {
                    curTile.Liquid = 255;
                    curTile.IsLava = tile.Liquid.IsLava;
                }

            }
        }

        #endregion

        #region Ellipse

        public static void FillEllipse(this World world, int x1, int y1, int x2, int y2, TilePicker tile)
        {
            // Calc center and radius
            int xr = (x2 - x1) >> 1;
            int yr = (y2 - y1) >> 1;
            int xc = x1 + xr;
            int yc = y1 + yr;


            world.FillEllipseCentered(xc, yc, xr, yr, tile);
        }

        public static void FillEllipseCentered(this World world, int xc, int yc, int xr, int yr, TilePicker tile)
        {
            int w = world.Header.MaxTiles.X;
            int h = world.Header.MaxTiles.Y;


            // Init vars
            int uh, lh, uy, ly, lx, rx;
            int x = xr;
            int y = 0;
            int xrSqTwo = (xr * xr) << 1;
            int yrSqTwo = (yr * yr) << 1;
            int xChg = yr * yr * (1 - (xr << 1));
            int yChg = xr * xr;
            int err = 0;
            int xStopping = yrSqTwo * xr;
            int yStopping = 0;

            // Draw first set of points counter clockwise where tangent line slope > -1.
            while (xStopping >= yStopping)
            {
                // Draw 4 quadrant points at once
                uy = yc + y;                  // Upper half
                ly = yc - y;                  // Lower half
                if (uy < 0) uy = 0;          // Clip
                if (uy >= h) uy = h - 1;      // ...
                if (ly < 0) ly = 0;
                if (ly >= h) ly = h - 1;
                //uh = uy * w;                  // Upper half
                //lh = ly * w;                  // Lower half

                rx = xc + x;
                lx = xc - x;
                if (rx < 0) rx = 0;          // Clip
                if (rx >= w) rx = w - 1;      // ...
                if (lx < 0) lx = 0;
                if (lx >= w) lx = w - 1;

                // Draw line
                for (int i = lx; i <= rx; i++)
                {
                    SetTileXY(world, i, uy, tile);       // Quadrant II to I (Actually two octants)
                    SetTileXY(world, i, ly, tile);       // Quadrant III to IV    
                }                     

                y++;
                yStopping += xrSqTwo;
                err += yChg;
                yChg += xrSqTwo;
                if ((xChg + (err << 1)) > 0)
                {
                    x--;
                    xStopping -= yrSqTwo;
                    err += xChg;
                    xChg += yrSqTwo;
                }
            }

            // ReInit vars
            x = 0;
            y = yr;
            uy = yc + y;                  // Upper half
            ly = yc - y;                  // Lower half
            if (uy < 0) uy = 0;          // Clip
            if (uy >= h) uy = h - 1;      // ...
            if (ly < 0) ly = 0;
            if (ly >= h) ly = h - 1;
            //uh = uy * w;                  // Upper half
            //lh = ly * w;                  // Lower half
            xChg = yr * yr;
            yChg = xr * xr * (1 - (yr << 1));
            err = 0;
            xStopping = 0;
            yStopping = xrSqTwo * yr;

            // Draw second set of points clockwise where tangent line slope < -1.
            while (xStopping <= yStopping)
            {
                // Draw 4 quadrant points at once
                rx = xc + x;
                lx = xc - x;
                if (rx < 0) rx = 0;          // Clip
                if (rx >= w) rx = w - 1;      // ...
                if (lx < 0) lx = 0;
                if (lx >= w) lx = w - 1;

                // Draw line
                for (int i = lx; i <= rx; i++)
                {
                    SetTileXY(world, i, uy, tile);       // Quadrant II to I (Actually two octants)
                    SetTileXY(world, i, ly, tile);       // Quadrant III to IV    
                }

                x++;
                xStopping += yrSqTwo;
                err += xChg;
                xChg += yrSqTwo;
                if ((yChg + (err << 1)) > 0)
                {
                    y--;
                    uy = yc + y;                  // Upper half
                    ly = yc - y;                  // Lower half
                    if (uy < 0) uy = 0;           // Clip
                    if (uy >= h) uy = h - 1;      // ...
                    if (ly < 0) ly = 0;
                    if (ly >= h) ly = h - 1;
                    uh = uy * w;                  // Upper half
                    lh = ly * w;                  // Lower half
                    yStopping -= xrSqTwo;
                    err += yChg;
                    yChg += xrSqTwo;
                }
            }
        }

        #endregion
    }
}
