using System.Collections.Generic;
using System.Linq;
using TEdit.Common;
using TEdit.Geometry;

namespace TEdit.Terraria.Objects
{
    public class SpriteSheet
    {
        public ushort Tile { get; set; }
        public string Name { get; set; }
        public Vector2Short[] SizeTiles { get; set; }
        public Vector2Short SizePixelsRender { get; set; }
        public Vector2Short SizePixelsInterval { get; set; }
        public Vector2Short SizeTexture { get; set; }
        public bool IsAnimated { get; set; }
        public List<SpriteItem> Styles { get; } = new();
        public SpriteItem Default => Styles.FirstOrDefault();

        public SpriteItem GetStyleFromUV(Vector2Short uv)
        {
            var renderUV = TileProperty.GetRenderUV(Tile, uv.X, uv.Y);

            foreach (var item in Styles)
            {
                if (item?.ContainsUV(renderUV) == true)
                {
                    return item;
                }
            }
            return default;
        }
    }

    public class SpriteItem
    {
        public ushort Tile { get; set; }
        public int Style { get; set; }
        public TEditColor StyleColor { get; set; }
        public Vector2Short UV { get; set; }
        public Vector2Short SizeTiles { get; set; }
        public Vector2Short SizeTexture { get; set; }
        public Vector2Short SizePixelsInterval { get; set; }
        public FrameAnchor Anchor { get; set; }
        public string Name { get; set; }

        public bool ContainsUV(Vector2Short uv)
        {
            if (UV == uv) { return true; }

            if (uv.X >= UV.X &&
                uv.Y >= UV.Y &&
                uv.X < UV.X + (SizePixelsInterval.X * SizeTiles.X) &&
                uv.Y < UV.Y + (SizePixelsInterval.Y * SizeTiles.Y))
            {
                return true;
            }

            return false;
        }


    }
}
