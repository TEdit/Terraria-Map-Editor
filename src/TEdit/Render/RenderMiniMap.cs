using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Render;

public class RenderMiniMap
{
    public static int Resolution { get; set; } = 20;

    public static WriteableBitmap Render(World w, bool useFilter = false)
    {
        // scale the minimap appropriatly
        // UI is 300x100 px
        var maxX = (int)Math.Ceiling(w.TilesWide / 300.0);
        var maxY = (int)Math.Ceiling(w.TilesHigh / 100.0);

        Resolution = Math.Max(maxX, maxY);

        WriteableBitmap bmp = new WriteableBitmap(w.TilesWide / Resolution, w.TilesHigh / Resolution, 96, 96, PixelFormats.Bgra32, null);

        // OPTION: This can simply be combined with 'UpdateMinimap'.
        if (useFilter)
            UpdateMinimapUsingFilter(w, ref bmp);
        else
            UpdateMinimap(w, ref bmp);

        return bmp;
    }

    // Normal function.
    public static void UpdateMinimap(World w, ref WriteableBitmap bmp)
    {
        bmp.Lock();
        unsafe
        {
            int pixelCount = bmp.PixelHeight * bmp.PixelWidth;
            var pixels = (int*)bmp.BackBuffer;

            for (int i = 0; i < pixelCount; i++)
            {
                int x = i % bmp.PixelWidth;
                int y = i / bmp.PixelWidth;

                int worldX = x * Resolution;
                int worldY = y * Resolution;

                pixels[i] = XnaColorToWindowsInt(PixelMap.GetTileColor(w.Tiles[worldX, worldY], Microsoft.Xna.Framework.Color.Transparent));
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
        bmp.Unlock();
    }
    public static void UpdateMinimapUsingFilter(World w, ref WriteableBitmap bmp)
    {
        bmp.Lock();
        unsafe
        {
            int pixelCount = bmp.PixelHeight * bmp.PixelWidth;
            var pixels = (int*)bmp.BackBuffer;

            for (int i = 0; i < pixelCount; i++)
            {
                int x = i % bmp.PixelWidth;
                int y = i / bmp.PixelWidth;

                int worldX = x * Resolution;
                int worldY = y * Resolution;

                // Define defualt bools.
                Microsoft.Xna.Framework.Color curBgColor = Microsoft.Xna.Framework.Color.Transparent;

                bool showWalls      = true;
                bool showTiles      = true;
                bool showLiquids    = true;
                bool showRedWire    = true;
                bool showBlueWire   = true;
                bool showGreenWire  = true;
                bool showYellowWire = true;

                bool wallDarken       = false;
                bool tileDarken       = false;
                bool liquidDarken     = false;
                bool redWireDarken    = false;
                bool blueWireDarken   = false;
                bool greenWireDarken  = false;
                bool yellowWireDarken = false;

                // Test the the filter for walls, tiles, liquids, and wires. 
                if (FilterManager.WallIsNotAllowed(w.Tiles[worldX, worldY].Wall))                                                       // Check if this wall is not in the list.
                    if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide) showWalls = false;                            // Hide walls not in list.
                    else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Darken) wallDarken = true;               // Darken walls not in list.

                if (FilterManager.TileIsNotAllowed(w.Tiles[worldX, worldY].Type)                                                        // Since sprites are under the tile denomination, we combine them.
                    && FilterManager.SpriteIsNotAllowed(w.Tiles[worldX, worldY].Type))                                                  // Check if this block / sprite is not in the list.
                    if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide) showTiles = false;                            // Hide blocks not in list.
                    else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Darken) tileDarken = true;               // Darken blocks not in list.

                if (FilterManager.LiquidIsNotAllowed(w.Tiles[worldX, worldY].LiquidType))                                               // Check if this liquid is not in the list.
                    if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide) showLiquids = false;                          // Hide liquids not in list.
                    else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Darken) liquidDarken = true;             // Darken liquids not in list.

                // Use the HasWire bool to save on processing speed.
                if (w.Tiles[worldX, worldY].HasWire)
                {
                    if (w.Tiles[worldX, worldY].WireRed)                                                                                // Check if this tile contains a red wire.
                        if (FilterManager.WireIsNotAllowed(FilterManager.WireType.Red))                                                 // Check if this wire is not in the list.
                            if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide) showRedWire = false;                  // Hide wires not in list.
                            else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Darken) redWireDarken = true;    // Darken wires not in list.

                    if (w.Tiles[worldX, worldY].WireBlue)                                                                               // Check if this tile contains a red wire.
                        if (FilterManager.WireIsNotAllowed(FilterManager.WireType.Blue))                                                // Check if this wire is not in the list.
                            if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide) showBlueWire = false;                 // Hide wires not in list.
                            else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Darken) blueWireDarken = true;   // Darken wires not in list.

                    if (w.Tiles[worldX, worldY].WireGreen)                                                                              // Check if this tile contains a red wire.
                        if (FilterManager.WireIsNotAllowed(FilterManager.WireType.Green))                                               // Check if this wire is not in the list.
                            if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide) showGreenWire = false;                // Hide wires not in list.
                            else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Darken) greenWireDarken = true;  // Darken wires not in list.

                    if (w.Tiles[worldX, worldY].WireYellow)                                                                             // Check if this tile contains a red wire.
                        if (FilterManager.WireIsNotAllowed(FilterManager.WireType.Yellow))                                              // Check if this wire is not in the list.
                            if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide) showYellowWire = false;               // Hide wires not in list.
                            else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Darken) yellowWireDarken = true; // Darken wires not in list.
                }

                // Test the the filter for custom background (solid color) mode.
                if (FilterManager.CurrentBackgroundMode == FilterManager.BackgroundMode.Transparent)
                    curBgColor = Microsoft.Xna.Framework.Color.Transparent;
                else if (FilterManager.CurrentBackgroundMode == FilterManager.BackgroundMode.Custom)
                    curBgColor = FilterManager.BackgroundModeCustomColor;

                // Define the color based on the filter results.
                Microsoft.Xna.Framework.Color color = PixelMap.GetTileColor(w.Tiles[worldX, worldY], curBgColor, showWalls, showTiles, showLiquids, showRedWire, showBlueWire, showGreenWire, showYellowWire,
                        wallDarken: wallDarken, tileDarken: tileDarken, liquidDarken: liquidDarken,
                        redWireDarken: redWireDarken, blueWireDarken: blueWireDarken, greenWireDarken: greenWireDarken, yellowWireDarken: yellowWireDarken);

                // Set the pixel data.
                pixels[i] = XnaColorToWindowsInt(color);
            }
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
        bmp.Unlock();
    }

    private static int XnaColorToWindowsInt(Microsoft.Xna.Framework.Color color)
    {
        byte a = color.A;
        byte b = color.B;
        byte g = color.G;
        byte r = color.R;

        int xnacolor = (a << 24)
      | ((byte)((r * a) >> 8) << 16)
      | ((byte)((g * a) >> 8) << 8)
      | ((byte)((b * a) >> 8));

        return xnacolor;
    }
}
