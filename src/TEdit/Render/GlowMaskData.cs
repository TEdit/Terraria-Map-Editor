using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TEdit.Render;

/// <summary>
/// Maps Terraria tile types to their glow mask texture indices and colors.
/// Based on Terraria Main.cs tile glow mask assignments (lines 8067-8111).
/// </summary>
public static class GlowMaskData
{
    /// <summary>
    /// Tile type → glow mask texture index. Only tiles with glow masks are included.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, int> TileToGlowIndex = new Dictionary<int, int>
    {
        // Meteorite
        [370] = 111,
        // Meteor Brick
        [390] = 130,

        // Lava Moss variants
        [381] = 126,
        [517] = 258,
        [687] = 336,

        // Krypton Moss variants
        [534] = 259,
        [535] = 260,
        [689] = 338,

        // Xenon Moss variants
        [536] = 261,
        [537] = 262,
        [690] = 339,

        // Argon Moss variants
        [539] = 263,
        [540] = 264,
        [688] = 337,

        // Violet Moss variants
        [625] = 311,
        [626] = 312,
        [691] = 340,

        // Disco Moss variants (TODO: animate color cycling)
        [627] = 313,
        [628] = 314,
        [692] = 341,

        // Martian Conduit Plating
        [350] = 94,

        // Ash / various
        [633] = 326,
        [634] = 326, // shares glow index
        [699] = 353,

        // Shimmer
        [659] = 348,
        [667] = 349,
        [708] = 359,

        // Lava-intensity tile
        [717] = 362,

        // Noir Tile
        [725] = 371,

        // Wire Bulb
        [429] = 214,
        [445] = 214,

        // Portal (TODO: animate portal colors)
        [209] = 215,

        // Various other glow tiles
        [410] = 201,
        [509] = 265,
        [658] = 333,
        [720] = 368,
        [721] = 369,
    };

    /// <summary>
    /// Returns the glow color tint for a given tile type.
    /// Colors use alpha = 0 because the additive blend state uses Blend.One,
    /// ignoring alpha. The RGB channels are the additive glow intensity.
    /// </summary>
    public static Color GetGlowColor(int tileType)
    {
        switch (tileType)
        {
            // Meteorite, Meteor Brick
            case 370:
            case 390:
                return new Color(100, 100, 100, 0);

            // Lava Moss + lava-intensity
            case 381:
            case 517:
            case 687:
            case 717:
                return new Color(150, 100, 50, 0);

            // Krypton Moss (green)
            case 534:
            case 535:
            case 689:
                return new Color(0, 200, 0, 0);

            // Xenon Moss (cyan)
            case 536:
            case 537:
            case 690:
                return new Color(0, 180, 250, 0);

            // Argon Moss (magenta)
            case 539:
            case 540:
            case 688:
                return new Color(225, 0, 125, 0);

            // Violet Moss
            case 625:
            case 626:
            case 691:
                return new Color(150, 0, 250, 0);

            // Disco Moss — TODO #2170: animate with color cycling
            case 627:
            case 628:
            case 692:
                return new Color(200, 100, 200, 0);

            // Crystal Ball — TODO #2170: animate HSL rainbow cycle
            case 129:
                return new Color(0, 150, 200, 0);

            // Portal — TODO #2170: animate portal colors
            case 209:
                return new Color(0, 150, 200, 0);

            // Martian Conduit, Shimmer, Ash, Noir, Wire Bulb, and other white glow tiles
            default:
                return Color.White;
        }
    }
}
