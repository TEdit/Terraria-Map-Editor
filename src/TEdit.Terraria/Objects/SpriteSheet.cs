using System.Collections.Generic;
using System.Linq;
using TEdit.Common;
using TEdit.Geometry;

namespace TEdit.Terraria.Objects;

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

    public Vector2Short[,] GetTiles()
    {
        var tiles = new Vector2Short[SizeTiles.X, SizeTiles.Y];
        for (int x = 0; x < SizeTiles.X; x++)
        {
            for (int y = 0; y < SizeTiles.Y; y++)
            {
                var curSize = SizePixelsInterval;
                var tileX = ((curSize.X) * x + UV.X);
                var tileY = ((curSize.Y) * y + UV.Y);

                if (Tile == 388 || Tile == 389)
                {
                    switch (y)
                    {
                        case 0:
                            tileY = UV.Y;
                            break;
                        case 1:
                            tileY = 20 + UV.Y;
                            break;
                        case 2:
                            tileY = 20 + 18 + UV.Y;
                            break;
                        case 3:
                            tileY = 20 + 18 + 18 + UV.Y;
                            break;
                        case 4:
                            tileY = 20 + 18 + 18 + 18 + UV.Y;
                            break;
                    }
                }

                var translated = TileProperty.GetWorldUV(Tile, (ushort) tileX, (ushort)tileY);
                tileX = translated.X;
                tileY = translated.Y;

                tiles[x, y] = new Vector2Short((short)tileX, (short)tileY);
            }
        }

        return tiles;
    }
}

public class SpriteTile
{
    public int Tile { get; set; }
    public Vector2Short UV { get; set; }
}
