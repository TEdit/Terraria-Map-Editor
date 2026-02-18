using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using TEdit.Common;
using TEdit.Terraria;

namespace TEdit.Render;

/// <summary>
/// Loads in-game minimap colors from MapColors.xml and applies them to tile/wall properties.
/// </summary>
public static class MapColorLoader
{
    private static Dictionary<int, TEditColor> _tileColors;
    private static Dictionary<int, TEditColor> _wallColors;
    private static bool _loaded;

    /// <summary>
    /// Loads base (Paint=0) colors from MapColors.xml for tiles and walls.
    /// Returns true if colors were loaded successfully.
    /// </summary>
    public static bool LoadMapColors()
    {
        if (_loaded) return _tileColors != null;

        _loaded = true;
        _tileColors = new Dictionary<int, TEditColor>();
        _wallColors = new Dictionary<int, TEditColor>();

        try
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapColors.xml");
            if (!File.Exists(filePath)) return false;

            var doc = XDocument.Load(filePath);

            // Load tile base colors (Paint=0 only)
            var tiles = doc.Descendants("Tile");
            foreach (var elem in tiles)
            {
                var paintAttr = elem.Attribute("Paint");
                if (paintAttr == null || paintAttr.Value != "0") continue;

                var idAttr = elem.Attribute("Id");
                var colorAttr = elem.Attribute("Color");
                if (idAttr == null || colorAttr == null) continue;

                int id = int.Parse(idAttr.Value);
                if (_tileColors.ContainsKey(id)) continue; // First entry wins (SubID=0)

                var color = ParseArgbColor(colorAttr.Value);
                if (color.HasValue)
                    _tileColors[id] = color.Value;
            }

            // Load wall base colors (Paint=0 only)
            var walls = doc.Descendants("Wall");
            foreach (var elem in walls)
            {
                var paintAttr = elem.Attribute("Paint");
                if (paintAttr == null || paintAttr.Value != "0") continue;

                var idAttr = elem.Attribute("Id");
                var colorAttr = elem.Attribute("Color");
                if (idAttr == null || colorAttr == null) continue;

                int id = int.Parse(idAttr.Value);
                if (_wallColors.ContainsKey(id)) continue;

                var color = ParseArgbColor(colorAttr.Value);
                if (color.HasValue)
                    _wallColors[id] = color.Value;
            }

            return true;
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
            return false;
        }
    }

    /// <summary>
    /// Applies loaded minimap colors to WorldConfiguration tile and wall properties.
    /// Call this after WorldConfiguration is initialized and MapColors are loaded.
    /// </summary>
    public static void ApplyMinimapColors()
    {
        if (_tileColors == null || _wallColors == null) return;

        foreach (var kvp in _tileColors)
        {
            if (kvp.Key >= 0 && kvp.Key < WorldConfiguration.TileProperties.Count)
            {
                var tile = WorldConfiguration.TileProperties[kvp.Key];
                if (tile != null && tile.Id == kvp.Key)
                    tile.Color = kvp.Value;
            }
        }

        foreach (var kvp in _wallColors)
        {
            if (kvp.Key >= 0 && kvp.Key < WorldConfiguration.WallProperties.Count)
            {
                var wall = WorldConfiguration.WallProperties[kvp.Key];
                if (wall != null && wall.Id == kvp.Key)
                    wall.Color = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Parses #AARRGGBB format color string to TEditColor (RGBA).
    /// MapColors.xml uses ARGB format: #FFRRGGBB
    /// </summary>
    private static TEditColor? ParseArgbColor(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length != 9 || hex[0] != '#')
            return null;

        try
        {
            byte a = Convert.ToByte(hex.Substring(1, 2), 16);
            byte r = Convert.ToByte(hex.Substring(3, 2), 16);
            byte g = Convert.ToByte(hex.Substring(5, 2), 16);
            byte b = Convert.ToByte(hex.Substring(7, 2), 16);
            return TEditColor.FromNonPremultiplied(r, g, b, a);
        }
        catch
        {
            return null;
        }
    }
}
