using Microsoft.Xna.Framework;
using System;
using TEdit.Terraria;

namespace TEdit.Render
{
    public static class PixelMap
    {
        #region Alpla Blend
        public static Color AlphaBlend(this Color background, Color color)
        {
            return AlphaBlend(background.A, background.R, background.B, background.G,
                              color.A, color.R, color.B, color.G);
        }
        public static Color AlphaBlend(this Color background, System.Windows.Media.Color color)
        {
            return AlphaBlend(background.A, background.R, background.B, background.G,
                              color.A, color.R, color.B, color.G);
        }
        public static Color AlphaBlend(this System.Windows.Media.Color background, Color color)
        {
            return AlphaBlend(background.A, background.R, background.B, background.G,
                              color.A, color.R, color.B, color.G);
        }
        public static Color AlphaBlend(this System.Windows.Media.Color background, System.Windows.Media.Color color)
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

        public static Color GetTileColor(Tile tile, Color background, bool showWall = true, bool showTile = true, bool showLiquid = true, bool showRedWire = true, bool showBlueWire = true, bool showGreenWire = true, bool showYellowWire = true)
        {
            var c = new Color(0, 0, 0, 0);

            if (tile.Wall > 0 && showWall)
            {

                if (World.WallProperties.Count > tile.Wall)
                {
                    if (World.WallProperties[tile.Wall].Color.A != 0)
                        c = c.AlphaBlend(World.WallProperties[tile.Wall].Color);
                    else
                        c = background;
                }
                else
                    c = c.AlphaBlend(Color.Magenta); // Add out-of-range colors

                // blend paint
                if (tile.WallColor > 0 && (!showTile || tile.TileColor == 0))
                {
                    var paint = World.PaintProperties[tile.WallColor].Color;
                    switch (tile.WallColor)
                    {
                        case 29:
                            float light = c.B * 0.3f;
                            c.R = (byte)(c.R * light);
                            c.G = (byte)(c.G * light);
                            c.B = (byte)(c.B * light);
                            break;
                        case 30:
                            c.R = (byte)((byte.MaxValue - c.R) * 0.5);
                            c.G = (byte)((byte.MaxValue - c.G) * 0.5);
                            c.B = (byte)((byte.MaxValue - c.B) * 0.5);
                            break;
                        default:
                            paint.A = (byte)32;
                            c = c.AlphaBlend(paint);
                            break;
                    }

                }
            }
            else
                c = background;

            if (tile.IsActive && showTile)
            {
                if (World.TileProperties.Count > tile.Type)
                    c = c.AlphaBlend(World.TileProperties[tile.Type].Color);
                else
                    c = c.AlphaBlend(Color.Magenta); // Add out-of-range colors

                // blend paint
                if (tile.TileColor > 0 && tile.TileColor <= World.PaintProperties.Count)
                {
                    var paint = World.PaintProperties[tile.TileColor].Color;

                    switch (tile.TileColor)
                    {
                        case 29:
                            float light = c.B * 0.3f;
                            c.R = (byte)(c.R * light);
                            c.G = (byte)(c.G * light);
                            c.B = (byte)(c.B * light);
                            break;
                        case 30:
                            c.R = (byte)(byte.MaxValue - c.R);
                            c.G = (byte)(byte.MaxValue - c.G);
                            c.B = (byte)(byte.MaxValue - c.B);
                            break;
                        default:
                            paint.A = (byte)32;
                            c = c.AlphaBlend(paint);
                            break;
                    }
                }

            }

            if (tile.LiquidAmount > 0 && showLiquid)
            {
                if (tile.LiquidType == LiquidType.Lava) c = c.AlphaBlend(World.GlobalColors["Lava"]);
                else if (tile.LiquidType == LiquidType.Honey) c = c.AlphaBlend(World.GlobalColors["Honey"]);
                else c = c.AlphaBlend(World.GlobalColors["Water"]);
            }

            if (tile.WireRed && showRedWire)
            {
                c = c.AlphaBlend(World.GlobalColors["Wire"]);
            }
            if (tile.WireGreen && showGreenWire)
            {
                c = c.AlphaBlend(World.GlobalColors["Wire2"]);
            }
            if (tile.WireBlue && showBlueWire)
            {
                c = c.AlphaBlend(World.GlobalColors["Wire1"]);
            }
            if (tile.WireYellow && showYellowWire)
            {
                c = c.AlphaBlend(World.GlobalColors["Wire3"]);
            }

            return c;
        }
    }
}