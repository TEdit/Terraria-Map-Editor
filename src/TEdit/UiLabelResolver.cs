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
            return ResolveSlug("frame_text_", key[TEdit.Terraria.LocalizationUiLabels.FrameTextPrefix.Length..]);

        return ResolveFixedUiLabel(key);
    }

    /// <summary>Maps a bestiary category ("Lunar Event") to bestiary_category_lunar_event.</summary>
    public static string? ResolveBestiaryCategory(string category) => ResolveSlug("bestiary_category_", category);

    /// <summary>Looks up "{prefix}{slug}"; the slug lowercases and collapses non-alphanumerics into one underscore.</summary>
    private static string? ResolveSlug(string prefix, string english)
    {
        var slug = new System.Text.StringBuilder(prefix, english.Length + prefix.Length);
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
