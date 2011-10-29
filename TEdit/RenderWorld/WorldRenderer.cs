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
        string[]                      layers   = WorldImage.LayerList;
        Dictionary<string, SizeInt32> tileSize = WorldImage.TileSize;
        Dictionary<string, int>       Bpp      = WorldImage.Bpp;

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

        public void UpdateWorldImage(PointInt32 loc) { UpdateWorldImage(new RectI(loc, loc), null); }
        public void UpdateWorldImage(RectI area, string renderMsg = "Render Update Complete.", Dictionary<string, WriteableBitmap> img = null)
        {
            // validate area
            area.Rebound(_world.Header.WorldBounds);

            int width  = area.Width;
            int height = area.Height;

            if (img == null) img = _worldImage.Layer;
            var pixels = new Dictionary<string, BytePixels>();
            foreach (string layer in layers) { pixels.Add(layer, new BytePixels(area.Size * tileSize[layer], Bpp[layer])); }

            for (int x = area.X; x <= area.Right; x++) {
                int dx = x - area.X;
                if (renderMsg != null) OnProgressChanged(this, new ProgressChangedEventArgs(
                    (int)(dx / (double)width*100.0),
                    "Rendering World...")
                );
                for (int y = area.Y; y <= area.Bottom; y++) {
                    int dy = y - area.Y;
                    Tile tile = _world.Tiles[x, y];
                    if (tile != null) {
                        var xy = new PointInt32(x, y);
                        foreach (string layer in layers) {
                            GetTexture(layer, y, tile, y >= _world.Header.WorldBounds.Bottom - 192).PutData(pixels[layer], xy * (PointInt32)tileSize[layer]);
                        }
                    }
                }
            }

            foreach (string layer in layers) {
                SizeInt32 ts = tileSize[layer];
                var realArea = area * new RectI((PointInt32)ts, ts);
                var stride   = realArea.Width * Bpp[layer];

                img[layer].Lock();
                img[layer].WritePixels(realArea, pixels[layer].GetData(), stride, 0);
                img[layer].AddDirtyRect(realArea);
                img[layer].Unlock();
            }

            if (renderMsg != null) OnProgressChanged(this, new ProgressChangedEventArgs(0, renderMsg));
        }


        public Dictionary<string, WriteableBitmap> RenderWorld()
        {
            if (_world.Header.WorldId == 0)
                return null;

            IsRenderingFullMap = true;
            int width  = _world.Header.MaxTiles.X;
            int height = _world.Header.MaxTiles.Y;

            var wbmap = new Dictionary<string, WriteableBitmap>();
            foreach (string layer in layers) {
                _worldImage.Layer[layer] = null;
                wbmap[layer] = new WriteableBitmap(
                    width  * tileSize[layer].Width,
                    height * tileSize[layer].Height,
                    96,
                    96,
                    System.Windows.Media.PixelFormats.Bgr32,
                    null);
            }

            UpdateWorldImage(_world.Header.WorldBounds, "Render Complete.", wbmap);
            IsRenderingFullMap = false;
            return wbmap;
        }

        public Dictionary<string, WriteableBitmap> RenderBuffer(ClipboardBuffer buffer)
        {
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
            }

            UpdateWorldImage(new RectI(0, 0, buffer.Size), "Buffer Render Complete.", wbmap);
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
            var size = 4;
            var pixels = new BytePixels(new SizeInt32(size, size), 4);
            var sizeI = new SizeInt32(size, size);

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
                        pixels = WorldSettings.Walls[tile.Wall].Texture.GetData(new RectI(size, size, sizeI));
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
                        pixels = WorldSettings.Tiles[tile.Type].Texture.GetData(new RectI(0, 0, sizeI));
                    break;

                // FIXME: NPC layer would go here... //

                case "TilesFront":
                    /// FIXME: Needs Frame Textures and frames checks ///
                    if (tile.IsActive && WorldSettings.Tiles[tile.Type].IsSolid)
                        pixels = WorldSettings.Tiles[tile.Type].Texture.GetData(new RectI(0, 0, sizeI));
                    break;

                case "Liquid":
                    if (tile.Liquid > 0) {
                        // FIXME: 16x16, not 8x8 //
                        // Should compress to 8x8 and use Liquid levels to determine final height //
                        // Actually, bottom 8x8 should be for 255, and top 8x8 for anything else //
                        if (tile.IsLava)
                            pixels = WorldSettings.GlobalColors["Lava"].Texture.GetData(new RectI(0, 0, sizeI));
                        else
                            pixels = WorldSettings.GlobalColors["Water"].Texture.GetData(new RectI(0, 0, sizeI));
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