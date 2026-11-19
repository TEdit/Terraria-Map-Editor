using System;

namespace TEdit;

/// <summary>
/// Supplies <see cref="TEdit.Terraria.LocalizationUiLabels"/> from Language.resx: fixed labels
/// (frame anchors, wall markers) and TEdit-made frame labels via "frame_text_{slug}" keys.
/// </summary>
internal static class UiLabelResolver
{
    public static string? Resolve(string key)
    {
        if (key.StartsWith(TEdit.Terraria.LocalizationUiLabels.FrameTextPrefix, StringComparison.Ordinal))
            return ResolveFrameText(key[TEdit.Terraria.LocalizationUiLabels.FrameTextPrefix.Length..]);

        return ResolveFixedUiLabel(key);
    }

    /// <summary>Maps an English frame label to its "frame_text_{slug}" resource, e.g. "Stair Top Landing R" → frame_text_stair_top_landing_r.</summary>
    private static string? ResolveFrameText(string english)
    {
        var slug = new System.Text.StringBuilder("frame_text_", english.Length + 11);
        bool lastUnderscore = true;
        foreach (char ch in english.ToLowerInvariant())
        {
            if (ch is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                slug.Append(ch);
                lastUnderscore = false;
            }
            else if (!lastUnderscore)
            {
                slug.Append('_');
                lastUnderscore = true;
            }
        }
        if (lastUnderscore && slug[^1] == '_')
            slug.Length--;

        return TEdit.Properties.Language.ResourceManager.GetString(slug.ToString(), TEdit.Properties.Language.Culture);
    }

    private static string? ResolveFixedUiLabel(string key) => key switch
    {
        "Anchor.None" => TEdit.Properties.Language.frame_anchor_none,
        "Anchor.Left" => TEdit.Properties.Language.frame_anchor_left,
        "Anchor.Right" => TEdit.Properties.Language.frame_anchor_right,
        "Anchor.Top" => TEdit.Properties.Language.frame_anchor_top,
        "Anchor.Bottom" => TEdit.Properties.Language.frame_anchor_bottom,
        "Anchor.Center" => TEdit.Properties.Language.frame_anchor_center,
        TEdit.Terraria.LocalizationUiLabels.WallNaturalFormat => TEdit.Properties.Language.wall_natural_format,
        TEdit.Terraria.LocalizationUiLabels.WallUnusedFormat => TEdit.Properties.Language.wall_unused_format,
        TEdit.Terraria.LocalizationUiLabels.WallSky => TEdit.Properties.Language.wall_sky,
        TEdit.Terraria.LocalizationUiLabels.WallUnbreakable => TEdit.Properties.Language.wall_unbreakable,
        TEdit.Terraria.LocalizationUiLabels.PaintNone => TEdit.Properties.Language.common_none,
        _ => null
    };
}
