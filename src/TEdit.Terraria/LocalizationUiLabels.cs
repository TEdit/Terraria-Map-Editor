using System;

namespace TEdit.Terraria;

/// <summary>
/// TEdit-only UI labels used while composing localized game-data names (frame anchors, wall
/// natural/unused markers, and names the game's language files never define).
/// The data layer cannot reference the application's Language.resx, so the application
/// registers <see cref="Resolver"/> at startup, before game data is loaded. Unresolved keys
/// fall back to English.
/// </summary>
public static class LocalizationUiLabels
{
    public const string AnchorPrefix = "Anchor.";

    /// <summary>
    /// Prefix for TEdit-made frame labels with no game equivalent (e.g. "FrameText.Stair Top Landing R").
    /// The text after the prefix is the English label; the resolver returns null when it has no translation.
    /// </summary>
    public const string FrameTextPrefix = "FrameText.";

    /// <summary>Composite format with one placeholder for the base wall name, e.g. "{0} (natural)".</summary>
    public const string WallNaturalFormat = "Wall.NaturalFormat";

    /// <summary>Composite format with one placeholder for the base wall name, e.g. "{0} (unused)".</summary>
    public const string WallUnusedFormat = "Wall.UnusedFormat";

    /// <summary>Wall 0 (no wall) — shown as "Sky".</summary>
    public const string WallSky = "Wall.Sky";

    /// <summary>The unbreakable block wall, which no item places.</summary>
    public const string WallUnbreakable = "Wall.Unbreakable";

    /// <summary>Paint 0 (no paint).</summary>
    public const string PaintNone = "Paint.None";

    /// <summary>Application-supplied lookup; returns null when the key is unknown.</summary>
    public static Func<string, string?>? Resolver { get; set; }

    public static string? Get(string key)
    {
        try
        {
            var value = Resolver?.Invoke(key);
            return string.IsNullOrEmpty(value) ? null : value;
        }
        catch
        {
            return null;
        }
    }
}
