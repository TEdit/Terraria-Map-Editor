using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TEdit.Common;
using TEdit.Geometry;

namespace TEdit.Terraria.Objects;

/// <summary>
/// Texture type for custom sprite preview rendering.
/// </summary>
public enum PreviewTextureType
{
    Tile,       // Standard Tiles_X texture
    Tree,       // Tiles_5_{style} texture (tree trunks)
    TreeTops,   // Tree_Tops_X texture
    TreeBranch, // Tree_Branches_X texture
    PalmTree,   // Tiles_323 texture (palm tree trunks)
    PalmTreeTop, // Tree_Tops_15 texture (palm tree tops)
    Item,       // Item texture
}

/// <summary>
/// Configuration for custom sprite preview rendering.
/// Supports alternate textures (tree tops) and source rectangles (layered tiles).
/// </summary>
public class PreviewConfig
{
    /// <summary>
    /// Texture type to use for preview. Default = Tile (uses Tiles_X texture).
    /// </summary>
    public PreviewTextureType TextureType { get; set; } = PreviewTextureType.Tile;

    /// <summary>
    /// Style/index for alternate textures (e.g., tree style for TreeTops).
    /// </summary>
    public int TextureStyle { get; set; }

    /// <summary>
    /// Custom source rectangle within the texture (null = use UV-based calculation).
    /// </summary>
    public Rectangle? SourceRect { get; set; }

    /// <summary>
    /// Pixel offset for preview positioning.
    /// </summary>
    public Vector2Short Offset { get; set; }
}

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

public partial class SpriteItem : ReactiveObject
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

    /// <summary>
    /// Custom preview configuration (null = use standard UV-based preview).
    /// </summary>
    public PreviewConfig PreviewConfig { get; set; }

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
