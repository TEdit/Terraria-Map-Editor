using System;
using System.IO;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
using TEdit.Utility;

namespace TEdit.ViewModel;

/// <summary>
/// Represents a single world file entry in the World Explorer (Tab 1).
/// </summary>
[IReactiveObject]
public partial class WorldEntryViewModel : IComparable<WorldEntryViewModel>
{
    public WorldEntryViewModel(WorldHeaderInfo header, bool isPinned = false, bool isRecent = false)
    {
        Title = header.Title;
        FilePath = header.FilePath;
        Version = header.Version;
        TilesWide = header.TilesWide;
        TilesHigh = header.TilesHigh;
        LastModified = header.LastSave;
        FileSizeBytes = header.FileSizeBytes;
        IsFavorite = header.IsFavorite;
        IsTModLoader = header.IsTModLoader;
        IsCrimson = header.IsCrimson;
        GameMode = header.GameMode;
        IsHardMode = header.IsHardMode;
        Seed = header.Seed;
        FileName = Path.GetFileName(header.FilePath);
        SizeCategory = header.SizeCategory;
        EvilBiome = header.EvilBiome;
        GameModeText = header.GameModeText;
        TerrariaVersionText = header.TerrariaVersionText;
        IsPinned = isPinned;
        IsRecent = isRecent;
        IsMissing = false;
    }

    /// <summary>
    /// Creates a missing/placeholder entry for a recent world that no longer exists on disk.
    /// </summary>
    public WorldEntryViewModel(string filePath)
    {
        Title = Path.GetFileNameWithoutExtension(filePath);
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
        IsMissing = true;
        IsRecent = true;
    }

    public string Title { get; }
    public string FilePath { get; }
    public uint Version { get; }
    public int TilesWide { get; }
    public int TilesHigh { get; }
    public DateTime LastModified { get; }
    public long FileSizeBytes { get; }
    public bool IsFavorite { get; }
    public bool IsTModLoader { get; }
    public bool IsCrimson { get; }
    public int GameMode { get; }
    public bool IsHardMode { get; }
    public string Seed { get; }
    public string FileName { get; }
    public string SizeCategory { get; }
    public string EvilBiome { get; }
    public string GameModeText { get; }
    public string TerrariaVersionText { get; }
    public bool IsRecent { get; }
    public bool IsMissing { get; }

    [Reactive]
    private bool _isPinned;

    public string DimensionText => IsMissing ? "" : $"{TilesWide}x{TilesHigh} - {SizeCategory}";
    public string VersionText => IsMissing ? "" : TerrariaVersionText ?? $"v{Version}";
    public string SizeText => IsMissing ? "" : FileMaintenance.FormatFileSize(FileSizeBytes);

    public string LastModifiedText
    {
        get
        {
            if (IsMissing) return "[Missing]";
            var elapsed = DateTime.UtcNow - LastModified;
            if (elapsed.TotalMinutes < 1) return "just now";
            if (elapsed.TotalHours < 1) return $"{(int)elapsed.TotalMinutes}m ago";
            if (elapsed.TotalDays < 1) return $"{(int)elapsed.TotalHours}h ago";
            if (elapsed.TotalDays < 30) return $"{(int)elapsed.TotalDays}d ago";
            return LastModified.ToLocalTime().ToString("yyyy-MM-dd");
        }
    }

    public int CompareTo(WorldEntryViewModel other)
    {
        if (other == null) return -1;
        // Pinned first
        if (IsPinned != other.IsPinned) return IsPinned ? -1 : 1;
        // Missing last
        if (IsMissing != other.IsMissing) return IsMissing ? 1 : -1;
        // Then by most recent
        return other.LastModified.CompareTo(LastModified);
    }
}
