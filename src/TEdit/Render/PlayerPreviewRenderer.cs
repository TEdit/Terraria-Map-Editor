using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;
using TEdit.Common;
using TEdit.Terraria.Player;
using TEdit.ViewModel;
using WpfPixelFormats = System.Windows.Media.PixelFormats;

namespace TEdit.Render;

/// <summary>
/// Composites a standing player preview from Terraria's body part and hair textures.
///
/// Terraria 1.4+ uses a composite frame system for body rendering. The sprite sheets
/// are organized as a 2D grid (X * 40, Y * 56) rather than a linear vertical strip.
/// For unarmored players (body &lt; 1), composite rendering is ALWAYS used.
///
/// Female characters have a Y+2 row offset for torso and shoulder frames:
///   Male torso:    grid (0,0) → pixel (0, 0)
///   Female torso:  grid (0,2) → pixel (0, 112)
///   Male shoulder: grid (1,1) → pixel (40, 56)
///   Female shoulder: grid (1,3) → pixel (40, 168)
///
/// Reference: PlayerDrawSet.CreateCompositeData() in Terraria source.
///
/// Body part indices (TextureAssets.Players[skinVar, partIndex]):
///   0  = Head skin           → skinColor
///   1  = Eye whites          → white (no tint)
///   2  = Eye irises          → eyeColor
///   3  = Torso skin          → skinColor          (composite frame)
///   4  = Undershirt           → underShirtColor   (composite frame)
///   5  = Hands (skin)        → skinColor          (composite frame)
///   6  = Shirt overlay       → shirtColor         (composite frame)
///   7  = Back arm skin       → skinColor
///   8  = Undershirt sleeves  → underShirtColor
///   9  = Front arm skin      → skinColor
///   10 = Legs skin           → skinColor
///   11 = Pants               → pantsColor
///   12 = Shoes               → shoeColor
///   13 = Shirt sleeves       → shirtColor
///   14 = Long coat (variants 3,7,8) → shirtColor
///   15 = Eyelid              → skinColor
/// </summary>
public static class PlayerPreviewRenderer
{
    private const int FrameWidth = 40;
    private const int FrameHeight = 56;

    private static readonly string[] SkinVariantNames =
    [
        "Male (Default)",     // 0
        "Male (Sticker)",     // 1
        "Male (Gangster)",    // 2
        "Male (Coat)",        // 3
        "Female (Default)",   // 4
        "Female (Sticker)",   // 5
        "Female (Gangster)",  // 6
        "Female (Coat)",      // 7
        "Dress",              // 8
        "Female (Dress)",     // 9
    ];

    /// <summary>
    /// Whether a skin variant uses female body frames.
    /// Variants 4-7 and 9 are female; variant 8 (Dress) uses male body frames.
    /// Maps to !Player.Male in Terraria source.
    /// </summary>
    private static bool IsFemaleVariant(int skinVariant) => skinVariant is >= 4 and <= 7 or 9;

    /// <summary>
    /// Bakes preview images for all skin variants using default colors.
    /// Call once after textures are loaded.
    /// </summary>
    public static List<SkinVariantOption> BakeSkinVariantPreviews(Textures textures)
    {
        var options = new List<SkinVariantOption>();

        for (int i = 0; i < SkinVariantNames.Length; i++)
        {
            // Create fresh appearance per variant with default Terraria colors
            var player = new PlayerCharacter
            {
                Appearance = new PlayerAppearance()
            };
            player.Appearance.SkinVariant = (byte)i;
            player.Appearance.Hair = 0;

            var preview = RenderPreview(player, textures, scale: 2);
            options.Add(new SkinVariantOption
            {
                Index = i,
                Name = SkinVariantNames[i],
                Preview = preview
            });
        }

        return options;
    }

    /// <summary>
    /// Renders a standing player preview as a WriteableBitmap.
    /// Must be called on the UI thread.
    /// </summary>
    public static WriteableBitmap? RenderPreview(PlayerCharacter player, Textures textures, int scale = 4)
    {
        if (textures == null || !textures.Valid) return null;

        var appearance = player.Appearance;
        var skinVariant = appearance.SkinVariant;

        // Composite buffer in WPF BGRA format (0xAARRGGBB)
        var composite = new int[FrameWidth * FrameHeight];

        // Terraria 1.4+ composite frame system: female characters use different
        // grid positions for torso/shoulder parts. See PlayerDrawSet.CreateCompositeData().
        bool isFemale = IsFemaleVariant(skinVariant);
        int femaleRowOffset = isFemale ? 2 : 0;

        // Composite frame positions for the standing pose (targetFrameNumber = 0):
        //   Torso:          grid (0, 0 + femaleOffset) → covers chest/belly area
        //   Back shoulder:  grid (1, 1 + femaleOffset) → covers shoulder/upper-arm area
        int torsoFrameX = 0;
        int torsoFrameY = (0 + femaleRowOffset) * FrameHeight;
        int shoulderFrameX = 1 * FrameWidth;
        int shoulderFrameY = (1 + femaleRowOffset) * FrameHeight;

        // Draw order follows Terraria's composite layer order (back to front):

        // Back arm layers (use traditional frame 0 — arm frames are not gender-offset)
        BlendBodyPart(composite, textures, skinVariant, 7, appearance.SkinColor);
        BlendBodyPart(composite, textures, skinVariant, 8, appearance.UnderShirtColor);
        BlendBodyPart(composite, textures, skinVariant, 13, appearance.ShirtColor);

        // Legs and feet (use traditional legFrame — not affected by composite system)
        BlendBodyPart(composite, textures, skinVariant, 10, appearance.SkinColor);
        BlendBodyPart(composite, textures, skinVariant, 12, appearance.ShoeColor);
        BlendBodyPart(composite, textures, skinVariant, 11, appearance.PantsColor);

        // Long coat / dress overlay (part 14) — only exists for variants 3, 7, 8
        // Drawing unconditionally is safe: returns DefaultTexture (skipped) for variants without it
        BlendBodyPart(composite, textures, skinVariant, 14, appearance.ShirtColor);

        // Torso skin — uses composite torso frame
        // (DrawPlayer_12_Skin_Composite draws part 3 with compTorsoFrame)
        BlendBodyPart(composite, textures, skinVariant, 3, appearance.SkinColor, torsoFrameX, torsoFrameY);

        // Undershirt + Shirt clothing — drawn twice per Terraria's DrawPlayer_17_TorsoComposite:
        // first with compBackShoulderFrame (shoulder area), then compTorsoFrame (chest area)
        BlendBodyPart(composite, textures, skinVariant, 4, appearance.UnderShirtColor, shoulderFrameX, shoulderFrameY);
        BlendBodyPart(composite, textures, skinVariant, 6, appearance.ShirtColor, shoulderFrameX, shoulderFrameY);
        BlendBodyPart(composite, textures, skinVariant, 4, appearance.UnderShirtColor, torsoFrameX, torsoFrameY);
        BlendBodyPart(composite, textures, skinVariant, 6, appearance.ShirtColor, torsoFrameX, torsoFrameY);

        // Hands skin — uses composite torso frame
        BlendBodyPart(composite, textures, skinVariant, 5, appearance.SkinColor, torsoFrameX, torsoFrameY);

        // Head and face (use traditional bodyFrame — not affected by composite system)
        BlendBodyPart(composite, textures, skinVariant, 0, appearance.SkinColor);
        BlendBodyPart(composite, textures, skinVariant, 1, null); // eye whites - no tint
        BlendBodyPart(composite, textures, skinVariant, 2, appearance.EyeColor);
        BlendBodyPart(composite, textures, skinVariant, 15, appearance.SkinColor); // eyelid

        // Hair
        BlendHair(composite, textures, appearance.Hair, appearance.HairColor);

        // Front arm (use traditional frame 0 — arm frames are not gender-offset)
        BlendBodyPart(composite, textures, skinVariant, 9, appearance.SkinColor);

        // Scale up with nearest-neighbor
        int outW = FrameWidth * scale;
        int outH = FrameHeight * scale;
        var scaled = new int[outW * outH];

        for (int y = 0; y < outH; y++)
        {
            int srcY = y / scale;
            for (int x = 0; x < outW; x++)
            {
                scaled[y * outW + x] = composite[srcY * FrameWidth + x / scale];
            }
        }

        var bmp = new WriteableBitmap(outW, outH, 96, 96, WpfPixelFormats.Bgra32, null);
        bmp.Lock();
        unsafe
        {
            var pixels = (int*)bmp.BackBuffer;
            for (int i = 0; i < scaled.Length; i++)
                pixels[i] = scaled[i];
        }
        bmp.AddDirtyRect(new Int32Rect(0, 0, outW, outH));
        bmp.Unlock();
        bmp.Freeze();
        return bmp;
    }

    private static void BlendBodyPart(int[] composite, Textures textures, int skinVariant, int partIndex, TEditColor? tint, int frameX = 0, int frameY = 0)
    {
        var texture = textures.GetPlayerBody(skinVariant, partIndex);
        if (texture == null || texture == textures.DefaultTexture)
        {
            ErrorLogging.LogDebug($"PlayerPreview: Missing texture for variant {skinVariant} part {partIndex}");
            return;
        }
        BlendFrame(composite, texture, tint, frameX, frameY);
    }

    private static void BlendHair(int[] composite, Textures textures, int hairIndex, TEditColor tint)
    {
        var texture = textures.GetPlayerHair(hairIndex);
        if (texture == null || texture == textures.DefaultTexture) return;
        BlendFrame(composite, texture, tint);
    }

    /// <summary>
    /// Blends a single frame from a sprite sheet onto the composite buffer.
    /// </summary>
    /// <param name="composite">Target buffer (FrameWidth x FrameHeight, WPF BGRA format)</param>
    /// <param name="texture">Source sprite sheet texture</param>
    /// <param name="tint">Color tint to apply (null = white/no tint)</param>
    /// <param name="frameX">X pixel offset of the frame origin in the sprite sheet</param>
    /// <param name="frameY">Y pixel offset of the frame origin in the sprite sheet</param>
    private static void BlendFrame(int[] composite, Texture2D texture, TEditColor? tint, int frameX = 0, int frameY = 0)
    {
        int texW = texture.Width;
        int texH = texture.Height;
        if (texW <= 0 || texH <= 0) return;

        // Calculate usable frame size, clamped to texture bounds
        int frameW = Math.Min(texW - frameX, FrameWidth);
        int frameH = Math.Min(texH - frameY, FrameHeight);
        if (frameW <= 0 || frameH <= 0) return;

        var pixels = new int[texW * texH];
        try
        {
            texture.GetData(pixels);
        }
        catch (Exception ex)
        {
            ErrorLogging.LogWarn($"PlayerPreview: GetData failed for {texture.Width}x{texture.Height} texture: {ex.Message}");
            return;
        }

        byte tR = tint?.R ?? 255;
        byte tG = tint?.G ?? 255;
        byte tB = tint?.B ?? 255;

        for (int y = 0; y < frameH; y++)
        {
            for (int x = 0; x < frameW; x++)
            {
                // XNA pixel format: packed as 0xAABBGGRR (SurfaceFormat.Color, little-endian)
                int abgr = pixels[(frameY + y) * texW + (frameX + x)];
                int srcA = (abgr >> 24) & 0xFF;
                if (srcA == 0) continue;

                int srcB = (abgr >> 16) & 0xFF;
                int srcG = (abgr >> 8) & 0xFF;
                int srcR = abgr & 0xFF;

                // XNA textures are premultiplied alpha - undo premultiplication before tinting
                if (srcA < 255 && srcA > 0)
                {
                    srcR = Math.Min(255, srcR * 255 / srcA);
                    srcG = Math.Min(255, srcG * 255 / srcA);
                    srcB = Math.Min(255, srcB * 255 / srcA);
                }

                // Apply tint
                srcR = srcR * tR / 255;
                srcG = srcG * tG / 255;
                srcB = srcB * tB / 255;

                // Alpha-blend onto composite (WPF BGRA: 0xAARRGGBB)
                int dstPixel = composite[y * FrameWidth + x];
                int dstA = (dstPixel >> 24) & 0xFF;
                int dstR = (dstPixel >> 16) & 0xFF;
                int dstG = (dstPixel >> 8) & 0xFF;
                int dstB = dstPixel & 0xFF;

                int outA = srcA + dstA * (255 - srcA) / 255;
                int outR, outG, outB;
                if (outA == 0)
                {
                    outR = outG = outB = 0;
                }
                else
                {
                    outR = (srcR * srcA + dstR * dstA * (255 - srcA) / 255) / outA;
                    outG = (srcG * srcA + dstG * dstA * (255 - srcA) / 255) / outA;
                    outB = (srcB * srcA + dstB * dstA * (255 - srcA) / 255) / outA;
                }

                composite[y * FrameWidth + x] = (outA << 24) | (Math.Min(255, outR) << 16) | (Math.Min(255, outG) << 8) | Math.Min(255, outB);
            }
        }
    }
}
