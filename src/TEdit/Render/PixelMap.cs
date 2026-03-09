using Microsoft.Xna.Framework;
using System;
using TEdit.Common;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Render;

public static class PixelMap
{
    #region Alpla Blend
    public static Color AlphaBlend(this Color background, Color color)
    {
        return AlphaBlend(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color AlphaBlend(this Color background, TEditColor color)
    {
        return AlphaBlend(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color AlphaBlend(this TEditColor background, Color color)
    {
        return AlphaBlend(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color AlphaBlend(this TEditColor background, TEditColor color)
    {
        return AlphaBlend(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color AlphaBlend(byte a1, byte r1, byte b1, byte g1, byte a2, byte r2, byte b2, byte g2)
    {
        var a = (byte)((a2 / 255F) * a2 + (1F - a2 / 255F) * a1);
        var r = (byte)((a2 / 255F) * r2 + (1F - a2 / 255F) * r1);
        var g = (byte)((a2 / 255F) * g2 + (1F - a2 / 255F) * g1);
        var b = (byte)((a2 / 255F) * b2 + (1F - a2 / 255F) * b1);
        return Color.FromNonPremultiplied(r, g, b, a);
    }


    public static Color Overlay(this Color background, Color color)
    {
        return Overlay(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color Overlay(this Color background, System.Windows.Media.Color color)
    {
        return Overlay(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color Overlay(this System.Windows.Media.Color background, Color color)
    {
        return Overlay(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color Overlay(this System.Windows.Media.Color background, System.Windows.Media.Color color)
    {
        return Overlay(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color Overlay(byte a1, byte r1, byte b1, byte g1, byte a2, byte r2, byte b2, byte g2)
    {
        return Color.FromNonPremultiplied(
            Overlay(r1, (byte)(r2)),
            Overlay(g1, (byte)(g2)),
            Overlay(b1, (byte)(b2)),
            a1);
    }


    public static Color VividLight(this Color background, Color color)
    {
        return VividLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color VividLight(this Color background, System.Windows.Media.Color color)
    {
        return VividLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color VividLight(this System.Windows.Media.Color background, Color color)
    {
        return VividLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color VividLight(this System.Windows.Media.Color background, System.Windows.Media.Color color)
    {
        return VividLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color VividLight(byte a1, byte r1, byte b1, byte g1, byte a2, byte r2, byte b2, byte g2)
    {
        return Color.FromNonPremultiplied(
            VividLight(r1, (byte)(r2)),
            VividLight(g1, (byte)(g2)),
            VividLight(b1, (byte)(b2)),
            a1);
    }

    public static Color LinearLight(this Color background, Color color)
    {
        return LinearLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color LinearLight(this Color background, System.Windows.Media.Color color)
    {
        return LinearLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color LinearLight(this System.Windows.Media.Color background, Color color)
    {
        return LinearLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color LinearLight(this System.Windows.Media.Color background, System.Windows.Media.Color color)
    {
        return LinearLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static Color LinearLight(byte a1, byte r1, byte b1, byte g1, byte a2, byte r2, byte b2, byte g2)
    {
        return Color.FromNonPremultiplied(
            LinearLight(r1, (byte)(r2)),
            LinearLight(g1, (byte)(g2)),
            LinearLight(b1, (byte)(b2)),
            a1);
    }

    public static byte Multiply(byte v1, byte v2) => (byte)((v1 * v2) / 255);
    public static byte Screen(byte v1, byte v2) => (byte)(v1 + v2 - v1 + v2 / 255);
    public static byte Overlay(byte v1, byte v2) => (byte)((v2 < 128) ? (2 * v1 * v2 / 255) : (255 - 2 * (255 - v1) * (255 - v2) / 255));
    public static byte SoftLight(byte v1, byte v2)
    {
        if (v1 > 127.5)
        {
            return (byte)(v2 + (255 - v2) * ((v1 - 127.5) / 127.5) * (0.5 - Math.Abs(v2 - 127.5) / 255));
        }
        else
        {
            return (byte)(v2 - v2 * ((127.5 - v1) / 127.5) * (0.5 - Math.Abs(v2 - 127.5) / 255));
        }
    }
    public static byte HardLight(byte v1, byte v2) => (byte)((v1 > 127.5) ? (v2 + (255 - v2) * ((v1 - 127.5) / 127.5)) : (v2 * v1 / 127.5));
    public static byte ColorDodge(int v1, int v2) => (byte)((v1 == 255) ? v1 : Math.Min(255, ((v2 << 8) / (255 - v1))));
    public static byte ColorBurn(int v1, int v2) => (byte)((v1 == 0) ? v1 : Math.Max(0, (255 - ((255 - v2) << 8) / v1)));
    public static byte LinearColorDodge(int v1, int v2) => (byte)Math.Min(v1 + v2, 255);
    public static byte LinearColorBurn(int v1, int v2) => (byte)(((v1 + v2) < 255) ? 0 : (v1 + v2 - 255));
    public static byte Darken(int v1, int v2) => (byte)Math.Min(v1, v2);
    public static byte Lighten(int v1, int v2) => (byte)Math.Max(v1, v2);
    public static byte Difference(int v1, int v2) => (byte)Math.Abs(v1 - v2);
    public static byte Exclusion(int v1, int v2) => (byte)(v1 + v2 - v1 * v2 / 127.5);
    public static byte Reflex(int v1, int v2) => (byte)((v1 == 255) ? v1 : Math.Min(255, (v2 * v2 / (255 - v1))));
    public static byte LinearLight(byte v1, byte v2) => (v1 < 128) ? LinearColorBurn(v2, (2 * v1)) : LinearColorDodge(v2, (2 * (v1 - 128)));
    public static byte PinLight(int v1, int v2) => (v1 < 128) ? Darken(v2, (2 * v1)) : Lighten(v2, (2 * (v1 - 128)));
    public static byte VividLight(int v1, int v2) => (v1 < 128) ? ColorBurn(v2, (2 * v1)) : ColorDodge(v2, (2 * (v1 - 128)));
    public static byte HardMix(int v1, int v2) => (byte)((VividLight(v1, v2) < 128) ? 0 : 255);

    #endregion

    /// <summary>Darken a color by the configured darken amount.</summary>
    public static Color DarkenColor(Color c)
    {
        float factor = 1f - FilterManager.DarkenAmount;
        return new Color((byte)(c.R * factor), (byte)(c.G * factor), (byte)(c.B * factor), c.A);
    }

    // Cached global colors — resolved once on first use, invalidated on world load
    private static Color s_wireRed, s_wireBlue, s_wireGreen, s_wireYellow;
    private static Color s_lava, s_honey, s_shimmer, s_water;
    private static bool s_globalColorsCached;

    /// <summary>Call when world configuration changes (world load) to refresh cached colors.</summary>
    public static void InvalidateGlobalColorCache() => s_globalColorsCached = false;

    private static void EnsureGlobalColors()
    {
        if (s_globalColorsCached) return;
        var gc = WorldConfiguration.GlobalColors;
        s_wireRed = ToXnaColor(gc["Wire"]);
        s_wireBlue = ToXnaColor(gc["Wire1"]);
        s_wireGreen = ToXnaColor(gc["Wire2"]);
        s_wireYellow = ToXnaColor(gc["Wire3"]);
        s_lava = ToXnaColor(gc["Lava"]);
        s_honey = ToXnaColor(gc["Honey"]);
        s_shimmer = ToXnaColor(gc["Shimmer"]);
        s_water = ToXnaColor(gc["Water"]);
        s_globalColorsCached = true;
    }

    private static Color ToXnaColor(TEditColor c) => new Color(c.R, c.G, c.B, c.A);

    /// <summary>Apply paint color to a tile/wall color with brightness.</summary>
    private static void ApplyPaint(ref Color c, byte paintColor, byte brightness, TEditColor paintPropColor)
    {
        float bf = brightness * (1f / 255f);
        switch (paintColor)
        {
            case 29:
                float light = c.B * 0.3f * bf;
                c.R = (byte)(c.R * light);
                c.G = (byte)(c.G * light);
                c.B = (byte)(c.B * light);
                break;
            case 30:
                float half_bf = 0.5f * bf;
                c.R = (byte)((byte.MaxValue - c.R) * half_bf);
                c.G = (byte)((byte.MaxValue - c.G) * half_bf);
                c.B = (byte)((byte.MaxValue - c.B) * half_bf);
                break;
            default:
                var paint = paintPropColor;
                paint.A = (byte)brightness;
                c = c.AlphaBlend(paint);
                break;
        }
    }

    /// <summary>Apply brightness scaling to RGB channels.</summary>
    private static void ApplyBrightness(ref Color c, byte brightness)
    {
        float bf = brightness * (1f / 255f);
        c.R = (byte)(c.R * bf);
        c.G = (byte)(c.G * bf);
        c.B = (byte)(c.B * bf);
    }

    public static Color GetTileColor(Tile tile, Color background, bool showWall = true, bool showTile = true, bool showLiquid = true, bool showRedWire = true, bool showBlueWire = true, bool showGreenWire = true, bool showYellowWire = true, bool showCoatings = true,
        bool wallDarken = false, bool tileDarken = false, bool liquidDarken = false,
        bool redWireDarken = false, bool blueWireDarken = false, bool greenWireDarken = false, bool yellowWireDarken = false)
    {
        EnsureGlobalColors();
        var c = new Color(0, 0, 0, 0);

        if (tile.Wall > 0 && showWall)
        {
            if (WorldConfiguration.WallProperties.Count > tile.Wall)
            {
                if (WorldConfiguration.WallProperties[tile.Wall].Color.A != 0)
                    c = c.AlphaBlend(WorldConfiguration.WallProperties[tile.Wall].Color);
                else
                    c = background;
            }
            else
                c = c.AlphaBlend(Color.Magenta); // Add out-of-range colors

            byte brightness = 255;
            if (showCoatings)
            {
                brightness = 211;
                if (tile.InvisibleWall) { brightness = 169; }
                if (tile.FullBrightWall) { brightness = 255; }
            }

            // blend paint
            if (tile.WallColor > 0 && (!showTile || tile.TileColor == 0))
                ApplyPaint(ref c, tile.WallColor, brightness, WorldConfiguration.PaintProperties[tile.WallColor].Color);
            else
                ApplyBrightness(ref c, brightness);

            if (wallDarken)
                c = DarkenColor(c);
        }
        else
            c = background;

        if (tile.IsActive && showTile)
        {
            if (WorldConfiguration.TileProperties.Count > tile.Type)
                c = c.AlphaBlend(WorldConfiguration.TileProperties[tile.Type].Color);
            else
                c = c.AlphaBlend(Color.Magenta); // Add out-of-range colors

            byte brightness = 255;
            if (showCoatings)
            {
                brightness = 211;
                if (tile.InvisibleBlock) { brightness = 169; }
                if (tile.FullBrightBlock) { brightness = 255; }
            }

            // blend paint
            if (tile.TileColor > 0 && tile.TileColor <= WorldConfiguration.PaintProperties.Count)
                ApplyPaint(ref c, tile.TileColor, brightness, WorldConfiguration.PaintProperties[tile.TileColor].Color);
            else
                ApplyBrightness(ref c, brightness);

            if (tileDarken)
                c = DarkenColor(c);
        }

        if (tile.LiquidAmount > 0 && showLiquid)
        {
            if (tile.LiquidType == LiquidType.Lava) c = c.AlphaBlend(s_lava);
            else if (tile.LiquidType == LiquidType.Honey) c = c.AlphaBlend(s_honey);
            else if (tile.LiquidType == LiquidType.Shimmer) c = c.AlphaBlend(s_shimmer);
            else c = c.AlphaBlend(s_water);

            if (liquidDarken)
                c = DarkenColor(c);
        }

        if (tile.WireRed && showRedWire)
        {
            c = c.AlphaBlend(s_wireRed);
            if (redWireDarken)
                c = DarkenColor(c);
        }
        if (tile.WireBlue && showBlueWire)
        {
            c = c.AlphaBlend(s_wireBlue);
            if (blueWireDarken)
                c = DarkenColor(c);
        }
        if (tile.WireGreen && showGreenWire)
        {
            c = c.AlphaBlend(s_wireGreen);
            if (greenWireDarken)
                c = DarkenColor(c);
        }
        if (tile.WireYellow && showYellowWire)
        {
            c = c.AlphaBlend(s_wireYellow);
            if (yellowWireDarken)
                c = DarkenColor(c);
        }

        return c;
    }
}
