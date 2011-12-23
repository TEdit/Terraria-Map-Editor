using Microsoft.Xna.Framework;
using TEditXNA.Terraria;

namespace TEditXna.Render
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
        #endregion

        public static Color GetTileColor(Tile tile, Color background)
        {
            var c = new Color(0, 0, 0, 0);

            if (tile.Wall > 0)
                c = c.AlphaBlend(World.WallProperties[tile.Wall].Color);
            else
                c = background;
            

            if (tile.IsActive)
                c = c.AlphaBlend(World.TileProperties[tile.Type].Color);

            if (tile.Liquid > 0)
                c = c.AlphaBlend(tile.IsLava ? World.GlobalColors["Lava"] : World.GlobalColors["Water"]);

            if (tile.HasWire)
                c = c.AlphaBlend(World.GlobalColors["Wire"]);

            return c;
        }
    }
}