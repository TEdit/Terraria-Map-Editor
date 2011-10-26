using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.Common.Structures;
using TEdit.TerrariaWorld;
using TEdit.Tools.Clipboard;

namespace TEdit.RenderWorld
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WorldRenderer : ObservableObject
    {
        [Import("World")] private World _world;

        [Import] private WorldImage _worldImage;

        public event ProgressChangedEventHandler ProgressChanged;

        protected virtual void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(sender, e);
        }

        public string GetTileName(Tile tile, out string wall)
        {
            string tilename = String.Empty;
            wall = WorldSettings.Walls[tile.Wall].Name;

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
                tilename = WorldSettings.Tiles[tile.Type].Name;
            }

            return tilename;
        }

        private bool _isRenderingFullMap;
        public bool IsRenderingFullMap
        {
            get { return _isRenderingFullMap; }
            set { SetProperty(ref _isRenderingFullMap, ref value, "IsRenderingFullMap"); }
        }

<<<<<<< HEAD
        public static Color AlphaBlend(Color background, Color color)
        {
            var r = (byte) ((color.A/255F)*color.R + (1F - color.A/255F)*background.R);
            var g = (byte) ((color.A/255F)*color.G + (1F - color.A/255F)*background.G);
            var b = (byte) ((color.A/255F)*color.B + (1F - color.A/255F)*background.B);
            return Color.FromArgb(255, r, g, b);
        }

        public void UpdateWorldImage(PointInt32 location)
        {
            Tile tile = _world.Tiles[location.X, location.Y];
            Color color = GetTileColor(location.Y, tile);
            _worldImage.Image.SetPixel(location.X, location.Y, color);
            /// FIXME: Account for other layers ///
        }

        public void UpdateWorldImage(Int32Rect area)
=======
        public void UpdateWorldImage(PointInt32 loc) { UpdateWorldImage(new RectI(loc, loc), null); }
        public void UpdateWorldImage(RectI area, string renderMsg = "Render Update Complete.", Dictionary<string, WriteableBitmap> img = null)
>>>>>>> 76fe96c... Finished new Color class; Rendered layer now merges everything together (slowly)
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
            if ((area.Y + area.Height) >= _world.Header.MaxTiles.Y)
            {
                area.Height += _world.Header.MaxTiles.Y - (area.Y + area.Height);
            }
            if ((area.X + area.Width) >= _world.Header.MaxTiles.X)
            {
                area.Width += _world.Header.MaxTiles.X - (area.X + area.Width);
            }

            int width = area.Width;
            int height = area.Height;


            int stride = width*_worldImage.Image.Format.BitsPerPixel/8;

            int numpixelbytes = height*width*_worldImage.Image.Format.BitsPerPixel/8;

            var pixels = new byte[numpixelbytes];
            for (int x = 0; x < width; x++)
            {
                OnProgressChanged(this,
                                  new ProgressChangedEventArgs(
                                      (int) (x/(double) width*100.0),
                                      "Rendering World..."));
                for (int y = 0; y < height; y++)
                {
                    bool ishell = y + area.Y >= _world.Header.MaxTiles.Y - 192;
                    Tile tile = _world.Tiles[x + area.X, y + area.Y];
                    if (tile != null)
                    {
                        Color c = GetTileColor(+area.Y + y, tile, ishell);

                        pixels[x*4 + y*stride] = c.B;
                        pixels[x*4 + y*stride + 1] = c.G;
                        pixels[x*4 + y*stride + 2] = c.R;
                        pixels[x*4 + y*stride + 3] = c.A;
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
            if (_world.Header.WorldId == 0)
                return null;

            IsRenderingFullMap = true;
            int width = _world.Header.MaxTiles.X;
            int height = _world.Header.MaxTiles.Y;

<<<<<<< HEAD

            var wbmap = new WriteableBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            int stride = wbmap.BackBufferStride;

            int numpixelbytes = wbmap.PixelHeight*wbmap.PixelWidth*wbmap.Format.BitsPerPixel/8;

            var pixels = new byte[numpixelbytes];
            for (int x = 0; x < width; x++)
            {
                OnProgressChanged(this,
                                  new ProgressChangedEventArgs(
                                      (int) (x/(double) width*100.0),
                                      "Rendering World..."));

                for (int y = 0; y < height; y++)
                {
                    Tile tile = _world.Tiles[x, y];
                    if (tile != null)
                    {
                        bool ishell = y >= height - 192;
                        Color c = GetTileColor(y, tile, ishell);


                        pixels[x*4 + y*stride] = c.B;
                        pixels[x*4 + y*stride + 1] = c.G;
                        pixels[x*4 + y*stride + 2] = c.R;
                        pixels[x*4 + y*stride + 3] = c.A;
                        //bmp.SetPixel(x - area.Left, y - area.Top, c);
                    }
                }
=======
            var wbmap = new Dictionary<string, WriteableBitmap>();
            foreach (string layer in layers) {
                wbmap[layer] = new WriteableBitmap(
                    width  * tileSize[layer].Width,
                    height * tileSize[layer].Height,
                    96,
                    96,
                    System.Windows.Media.PixelFormats.Bgr32,
                    null);
>>>>>>> 76fe96c... Finished new Color class; Rendered layer now merges everything together (slowly)
            }

            wbmap.WritePixels(new Int32Rect(0, 0, wbmap.PixelWidth, wbmap.PixelHeight), pixels,
                              wbmap.PixelWidth*wbmap.Format.BitsPerPixel/8, 0);

            OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Complete."));
            IsRenderingFullMap = false;
            return wbmap;
        }

        public WriteableBitmap RenderBuffer(ClipboardBuffer buffer)
        {
<<<<<<< HEAD
            int width = buffer.Size.X;
            int height = buffer.Size.Y;
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
                                      "Rendering Buffer..."));

                for (int y = 0; y < height; y++)
                {
                    Tile tile = buffer.Tiles[x, y];
                    if (tile != null)
                    {
                        Color c = GetTileColor(y, tile);

                        pixels[x * 4 + y * stride] = c.B;
                        pixels[x * 4 + y * stride + 1] = c.G;
                        pixels[x * 4 + y * stride + 2] = c.R;
                        pixels[x * 4 + y * stride + 3] = c.A;
                        //bmp.SetPixel(x - area.Left, y - area.Top, c);
                    }
                }
=======
            int width  = buffer.Size.W;
            int height = buffer.Size.H;

            var wbmap = new Dictionary<string, WriteableBitmap>();
            foreach (string layer in layers) {
                wbmap[layer] = new WriteableBitmap(
                    width  * tileSize[layer].Width,
                    height * tileSize[layer].Height,
                    96,
                    96,
                    System.Windows.Media.PixelFormats.Bgr32,
                    null);
>>>>>>> 76fe96c... Finished new Color class; Rendered layer now merges everything together (slowly)
            }

            wbmap.WritePixels(new Int32Rect(0, 0, wbmap.PixelWidth, wbmap.PixelHeight), pixels,
                              wbmap.PixelWidth * wbmap.Format.BitsPerPixel / 8, 0);

            OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Complete."));

            return wbmap;
        }

        private Color GetTileColor(int y, Tile tile, bool isHell = false)
        {
            Color c;

            if (tile.Wall > 0)
            {
                c = WorldSettings.Walls[tile.Wall].Color;
            }
            else
            {
                if (isHell)
                    c = WorldSettings.GlobalColors["Hell"].Color;
                else if (y > _world.Header.WorldRockLayer && tile.Wall == 0)
                    c = WorldSettings.GlobalColors["Rock"].Color;
                else if (y > _world.Header.WorldSurface && tile.Wall == 0)
                    c = WorldSettings.GlobalColors["Earth"].Color;
                else
                    c = WorldSettings.GlobalColors["Sky"].Color;
            }

            if (tile.IsActive)
                c = c.AlphaBlend(WorldSettings.Tiles[tile.Type].Color);

            if (tile.Liquid > 0)
            {
                if (tile.IsLava)
                    c = c.AlphaBlend(WorldSettings.GlobalColors["Lava"].Color);
                else
                    c = c.AlphaBlend(WorldSettings.GlobalColors["Water"].Color);
            }
            return c;
        }

        private BytePixels GetTexture(string layerType, int y, Tile tile, bool isHell = false)
        {
            var pixels = new BytePixels(new SizeInt32(8, 8), 4);

            switch (layerType)
            {
                case "TilesPixel":
                    Color c = GetTileColor(y, tile, isHell);

                    var b = new byte[4];
                    b[0] = c.B;
                    b[1] = c.G;
                    b[2] = c.R;
                    b[3] = c.A;
                    pixels = new BytePixels(1, 1, b);
                    break;

                case "Rendered":
                    pixels = pixels.AlphaBlend(GetTileColor(y, tile, isHell));  // A BytePixel version of the TileColor

                    if (tile.Wall > 0)
                        pixels = pixels.AlphaBlend( GetTexture("Walls", y, tile, isHell)      );
                    if (tile.IsActive && !WorldSettings.Tiles[tile.Type].IsSolid)
                        pixels = pixels.AlphaBlend( GetTexture("TilesBack", y, tile, isHell)  );
                    if (tile.IsActive && WorldSettings.Tiles[tile.Type].IsSolid)
                        pixels = pixels.AlphaBlend( GetTexture("TilesFront", y, tile, isHell) );
                    if (tile.Liquid > 0)
                        pixels = pixels.AlphaBlend( GetTexture("Liquid", y, tile, isHell)     );
                    break;

                case "Walls":
                    /// FIXME: A complete wall is actually 32x32 big, with 3x3 variations, depending how many tiles exist ///
                    if (tile.Wall > 0)
                        pixels = WorldSettings.Walls[tile.Wall].Texture.GetData(new RectI(8, 8, 15, 15));
                    else
                    {
                        // FIXME: These are actually pretty large bitmaps //
                        // Might need to go on a WallsBack layer... //
                        /* 
                        if (isHell)
                            pixels = WorldSettings.GlobalColors["Hell"].Texture.GetData();
                        else if (y > _world.Header.WorldRockLayer && tile.Wall == 0)
                            pixels = WorldSettings.GlobalColors["Rock"].Texture.GetData();
                        else if (y > _world.Header.WorldSurface && tile.Wall == 0)
                            pixels = WorldSettings.GlobalColors["Earth"].Texture.GetData();
                        else  
                            pixels = WorldSettings.GlobalColors["Sky"].Texture.GetData();
                         */
                    }
                    break;

                case "TilesBack":
                    /// FIXME: Needs Frame Textures and frames checks ///
                    if (tile.IsActive && !WorldSettings.Tiles[tile.Type].IsSolid)
                        pixels = WorldSettings.Tiles[tile.Type].Texture.GetData(new RectI(0, 0, 7, 7));
                    break;

                // FIXME: NPC layer would go here... //

                case "TilesFront":
                    /// FIXME: Needs Frame Textures and frames checks ///
                    if (tile.IsActive && WorldSettings.Tiles[tile.Type].IsSolid) 
                        pixels = WorldSettings.Tiles[tile.Type].Texture.GetData(new RectI(0, 0, 7, 7));
                    break;

                case "Liquid":
                    if (tile.Liquid > 0) {
                        // FIXME: 16x16, not 8x8 //
                        // Should compress to 8x8 and use Liquid levels to determine final height //
                        // Actually, bottom 8x8 should be for 255, and top 8x8 for anything else //
                        if (tile.IsLava)
                            pixels = WorldSettings.GlobalColors["Lava"].Texture.GetData(new RectI(0, 0, 7, 7));
                        else
                            pixels = WorldSettings.GlobalColors["Water"].Texture.GetData(new RectI(0, 0, 7, 7));
                    }
                    break;
            }
            
            return pixels;
        }

        public static IEnumerable<PointInt32> DrawLine(PointInt32 begin, PointInt32 end)
        {
            int y0 = begin.Y;
            int x0 = begin.X;
            int y1 = end.Y;
            int x1 = end.X;

            // Bresenham's Line Algorithm - Pure int version (very fast and compact)
            // http://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm#Simplification
            /*
            int dx  = Math.Abs(x1 - x0);
            int dy  = Math.Abs(y1 - y0);
            int sx  = x0 < x1 ? 1 : -1;
            int sy  = y0 < y1 ? 1 : -1;
            int err = dx - dy;
 
            yield return new PointInt32(x0, y0);
            while (x0 != x1 || y0 != y1) {
                int e2 = err * 2;
                if (e2 > -dy) {
                    err -= dy;
                    x0  += sx;
                }
                if (e2 <  dx) {
                    err += dx;
                    y0  += sy;
                }
                yield return new PointInt32(x0, y0);
            }
            */

            // EFLA Variation E - (even faster)
            // http://www.simppa.fi/blog/extremely-fast-line-algorithm-as3-optimized/
            // (AS3 version with Abs re-added, thus simplifying the bit shifting from EFLA-E)
            int shortLen = y1 - y0;
            int longLen  = x1 - x0;
            bool yLonger = false;

            if (Math.Abs(shortLen) > Math.Abs(longLen)) {
                shortLen ^= longLen;
                longLen  ^= shortLen;
                shortLen ^= longLen;
                yLonger = true;
            }
 
            int inc = longLen < 0 ? -1 : 1;
            // using double here for accuracy, since some lines from one end of a world to another could get quite long...
            double multDiff = longLen == 0 ? shortLen : shortLen / longLen;  
 
            if (yLonger) {
                for (int i = 0; i != longLen; i += inc) { yield return new PointInt32(x0 + (int)(i * multDiff), y0 + i); }
            }
            else {
                for (int i = 0; i != longLen; i += inc) { yield return new PointInt32(x0 + i, y0 + (int)(i * multDiff)); }
            }

        }
    }
}