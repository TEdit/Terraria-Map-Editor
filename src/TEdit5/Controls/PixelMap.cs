/*
*                               The MIT License (MIT)
* Permission is hereby granted, free of charge, to any person obtaining a copy of
* this software and associated documentation files (the "Software"), to deal in
* the Software without restriction, including without limitation the rights to
* use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
* the Software, and to permit persons to whom the Software is furnished to do so.
*/
// Port from: https://github.com/cyotek/Cyotek.Windows.Forms.ImageBox to AvaloniaUI
// Port from: https://raw.githubusercontent.com/sn4k3/UVtools/master/UVtools.AvaloniaControls/AdvancedImageBox.cs


using Avalonia.Media;
using System;
using TEdit.Common;
using TEdit.Configuration;
using TEdit5.Controls.WorldRenderEngine.Layers;
using TEdit.Terraria;

namespace TEdit5.Controls;


public static class PixelMap
{
    #region Alpla Blend
    public static TEditColor AlphaBlend(this Color background, Color color)
    {
        return AlphaBlend(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor AlphaBlend(this Color background, TEditColor color)
    {
        return AlphaBlend(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor AlphaBlend(this TEditColor background, Color color)
    {
        return AlphaBlend(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor AlphaBlend(this TEditColor background, TEditColor color)
    {
        return AlphaBlend(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor AlphaBlend(byte a1, byte r1, byte b1, byte g1, byte a2, byte r2, byte b2, byte g2)
    {
        var a = (byte)((a2 / 255F) * a2 + (1F - a2 / 255F) * a1);
        var r = (byte)((a2 / 255F) * r2 + (1F - a2 / 255F) * r1);
        var g = (byte)((a2 / 255F) * g2 + (1F - a2 / 255F) * g1);
        var b = (byte)((a2 / 255F) * b2 + (1F - a2 / 255F) * b1);
        return TEditColor.FromNonPremultiplied(r, g, b, a);
    }


    public static TEditColor Overlay(this Color background, Color color)
    {
        return Overlay(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor Overlay(this Color background, TEditColor color)
    {
        return Overlay(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor Overlay(this TEditColor background, Color color)
    {
        return Overlay(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor Overlay(this TEditColor background, TEditColor color)
    {
        return Overlay(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor Overlay(byte a1, byte r1, byte b1, byte g1, byte a2, byte r2, byte b2, byte g2)
    {
        return TEditColor.FromNonPremultiplied(
            Overlay(r1, (byte)(r2)),
            Overlay(g1, (byte)(g2)),
            Overlay(b1, (byte)(b2)),
            a1);
    }


    public static TEditColor VividLight(this Color background, Color color)
    {
        return VividLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor VividLight(this Color background, TEditColor color)
    {
        return VividLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor VividLight(this TEditColor background, Color color)
    {
        return VividLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor VividLight(this TEditColor background, TEditColor color)
    {
        return VividLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor VividLight(byte a1, byte r1, byte b1, byte g1, byte a2, byte r2, byte b2, byte g2)
    {
        return TEditColor.FromNonPremultiplied(
            VividLight(r1, (byte)(r2)),
            VividLight(g1, (byte)(g2)),
            VividLight(b1, (byte)(b2)),
            a1);
    }

    public static TEditColor LinearLight(this Color background, Color color)
    {
        return LinearLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor LinearLight(this Color background, TEditColor color)
    {
        return LinearLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor LinearLight(this TEditColor background, Color color)
    {
        return LinearLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor LinearLight(this TEditColor background, TEditColor color)
    {
        return LinearLight(background.A, background.R, background.B, background.G,
                          color.A, color.R, color.B, color.G);
    }
    public static TEditColor LinearLight(byte a1, byte r1, byte b1, byte g1, byte a2, byte r2, byte b2, byte g2)
    {
        return TEditColor.FromNonPremultiplied(
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

    public static TEditColor GetTileColor(Tile tile, TEditColor background, RenderLayerVisibility? layers = null)
    {
        var c = new TEditColor(0, 0, 0, 0);

        if (tile.Wall > 0 && (layers?.Wall ?? true))
        {

            if (WorldConfiguration.WallProperties.Count > tile.Wall)
            {
                if (WorldConfiguration.WallProperties[tile.Wall].Color.A != 0)
                    c = c.AlphaBlend(WorldConfiguration.WallProperties[tile.Wall].Color);
                else
                    c = background;
            }
            else
                c = c.AlphaBlend(TEditColor.Magenta); // Add out-of-range colors

            byte brightness = 255;
            if (layers?.Coatings ?? true)
            {
                // echo: 169
                // normal: 211
                // illuminant: 255

                brightness = 211;
                if (tile.InvisibleWall) { brightness = 169; }
                if (tile.FullBrightWall) { brightness = 255; }
            }

            // blend paint
            if (tile.WallColor > 0 && (!(layers?.Tile ?? true) || tile.TileColor == 0))
            {
                var paint = WorldConfiguration.PaintProperties[tile.WallColor].Color;
                switch (tile.WallColor)
                {
                    case 29:
                        float light = c.B * 0.3f * (brightness / 255.0f);
                        c.R = (byte)(c.R * light);
                        c.G = (byte)(c.G * light);
                        c.B = (byte)(c.B * light);
                        break;
                    case 30:
                        c.R = (byte)((byte.MaxValue - c.R) * 0.5 * (brightness / 255.0f));
                        c.G = (byte)((byte.MaxValue - c.G) * 0.5 * (brightness / 255.0f));
                        c.B = (byte)((byte.MaxValue - c.B) * 0.5 * (brightness / 255.0f));
                        break;
                    default:
                        paint.A = (byte)brightness;
                        c = c.AlphaBlend(paint);
                        break;
                }
            }
            else
            {
                c.R = (byte)(c.R * (brightness / 255.0f));
                c.G = (byte)(c.G * (brightness / 255.0f));
                c.B = (byte)(c.B * (brightness / 255.0f));
            }
        }
        else
            c = background;

        if (tile.IsActive && (layers?.Tile ?? true))
        {
            if (WorldConfiguration.TileProperties.Count > tile.Type)
                c = c.AlphaBlend(WorldConfiguration.TileProperties[tile.Type].Color);
            else
                c = c.AlphaBlend(TEditColor.Magenta); // Add out-of-range colors

            byte brightness = 255;
            if ((layers?.Coatings ?? true))
            {
                // echo: 169
                // normal: 211
                // illuminant: 255

                brightness = 211;
                if (tile.InvisibleBlock) { brightness = 169; }
                if (tile.FullBrightBlock) { brightness = 255; }
            }

            // blend paint
            if (tile.TileColor > 0 && tile.TileColor <= WorldConfiguration.PaintProperties.Count)
            {
                var paint = WorldConfiguration.PaintProperties[tile.TileColor].Color;

                switch (tile.TileColor)
                {
                    case 29:
                        float light = c.B * 0.3f * (brightness / 255.0f);
                        c.R = (byte)(c.R * light);
                        c.G = (byte)(c.G * light);
                        c.B = (byte)(c.B * light);
                        break;
                    case 30:
                        c.R = (byte)((byte.MaxValue - c.R) * 0.5 * (brightness / 255.0f));
                        c.G = (byte)((byte.MaxValue - c.G) * 0.5 * (brightness / 255.0f));
                        c.B = (byte)((byte.MaxValue - c.B) * 0.5 * (brightness / 255.0f));
                        break;
                    default:
                        paint.A = (byte)brightness;
                        c = c.AlphaBlend(paint);
                        break;
                }
            }
            else
            {
                c.R = (byte)(c.R * (brightness / 255.0f));
                c.G = (byte)(c.G * (brightness / 255.0f));
                c.B = (byte)(c.B * (brightness / 255.0f));
            }
        }

        if (tile.LiquidAmount > 0 && (layers?.Liquid ?? true))
        {
            if (tile.LiquidType == LiquidType.Lava) c = c.AlphaBlend(WorldConfiguration.GlobalColors["Lava"]);
            else if (tile.LiquidType == LiquidType.Honey) c = c.AlphaBlend(WorldConfiguration.GlobalColors["Honey"]);
            else if (tile.LiquidType == LiquidType.Shimmer) c = c.AlphaBlend(WorldConfiguration.GlobalColors["Shimmer"]);
            else c = c.AlphaBlend(WorldConfiguration.GlobalColors["Water"]);
        }

        if (tile.WireRed && (layers?.WireRed ?? true))
        {
            c = c.AlphaBlend(WorldConfiguration.GlobalColors["Wire"]);
        }
        if (tile.WireGreen && (layers?.WireGreen ?? true))
        {
            c = c.AlphaBlend(WorldConfiguration.GlobalColors["Wire2"]);
        }
        if (tile.WireBlue && (layers?.WireBlue ?? true))
        {
            c = c.AlphaBlend(WorldConfiguration.GlobalColors["Wire1"]);
        }
        if (tile.WireYellow && (layers?.WireYellow ?? true))
        {
            c = c.AlphaBlend(WorldConfiguration.GlobalColors["Wire3"]);
        }

        return c;
    }
}
