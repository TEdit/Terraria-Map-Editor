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
    public static SpriteItem GetStyleFromUV(this SpriteSheet sprite, Vector2Short uv)
    {
        var renderUV = TileProperty.GetRenderUV(sprite.Tile, uv.X, uv.Y);

        foreach (var item in sprite.Styles)
        {
            if (item?.ContainsUV(renderUV) == true)
            {
                return item;
            }
        }
        return default;
    }
}
