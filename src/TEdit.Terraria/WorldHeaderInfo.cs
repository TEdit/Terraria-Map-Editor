using System;

namespace TEdit.Terraria;

/// <summary>
/// Lightweight world header data for quick browsing without loading tiles/chests/NPCs.
/// </summary>
public record WorldHeaderInfo(
    string Title,
    uint Version,
    int TilesWide,
    int TilesHigh,
    DateTime LastSave,
    long FileSizeBytes,
    string FilePath,
    bool IsTModLoader,
    bool IsFavorite,
    bool IsCrimson,
    int GameMode,
    bool IsHardMode,
    string Seed)
{
    /// <summary>
    /// Returns a human-readable size category based on dimensions.
    /// </summary>
    public string SizeCategory => (TilesWide, TilesHigh) switch
    {
        (4200, 1200) => "Small",
        (6400, 1800) => "Medium",
        (8400, 2400) => "Large",
        _ => $"{TilesWide}x{TilesHigh}"
    };

    /// <summary>
    /// Returns "Corruption" or "Crimson" based on the evil biome flag.
    /// </summary>
    public string EvilBiome => IsCrimson ? "Crimson" : "Corruption";

    /// <summary>
    /// Returns human-readable game mode.
    /// </summary>
    public string GameModeText => GameMode switch
    {
        0 => "Classic",
        1 => "Expert",
        2 => "Master",
        3 => "Journey",
        _ => $"Mode {GameMode}"
    };
}
