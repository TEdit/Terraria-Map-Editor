using System.Collections.Generic;
using TEdit.Geometry;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Editor;

public static class SpriteExtensions
{
    /// <summary>
    /// Get a Sprite from one of it's UV's
    /// </summary>
    /// <param name="uv"></param>
    /// <returns></returns>
    public static KeyValuePair<int, SpriteSub> GetStyleFromUV(this SpriteFull sprite, Vector2Short uv)
    {
        var renderUV = TileProperty.GetRenderUV(sprite.Tile, uv.X, uv.Y);

        foreach (var kvp in sprite.Styles)
        {
            if (kvp.Value.UV == uv) return kvp;

            if (renderUV.X >= kvp.Value.UV.X &&
                renderUV.Y >= kvp.Value.UV.Y &&
                renderUV.X < kvp.Value.UV.X + (kvp.Value.SizePixelsInterval.X * kvp.Value.SizeTiles.X) &&
                renderUV.Y < kvp.Value.UV.Y + (kvp.Value.SizePixelsInterval.Y * kvp.Value.SizeTiles.Y))
            {
                return kvp;
            }
        }
        return default;
    }
}
