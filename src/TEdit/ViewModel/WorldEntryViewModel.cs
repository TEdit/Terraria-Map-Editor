using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
using TEdit.Utility;

namespace TEdit.ViewModel;

/// <summary>
/// Represents a single world file entry in the World Explorer grid.
/// </summary>
[IReactiveObject]
public partial class WorldEntryViewModel : IComparable<WorldEntryViewModel>
{
    public WorldEntryViewModel(WorldHeaderInfo header, bool isPinned = false, bool isRecent = false,
        bool isCloudSave = false, string cloudUserId = null)
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
        IsCorrupt = header.IsCorrupt;
        CorruptReason = header.CorruptReason;
        IsPinned = isPinned;
        IsRecent = isRecent;
        IsMissing = false;
        IsCloudSave = isCloudSave;
        CloudUserId = cloudUserId;
        Backups.CollectionChanged += OnBackupsChanged;
    }

    /// <summary>
    /// Creates a missing/placeholder entry for a world that no longer exists on disk
    /// (recent world or orphan backups).
    /// </summary>
    public WorldEntryViewModel(string filePath)
    {
        Title = Path.GetFileNameWithoutExtension(filePath);
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
        IsMissing = true;
        IsRecent = true;
        Backups.CollectionChanged += OnBackupsChanged;
    }

    /// <summary>
    /// Creates a placeholder entry for orphan backups (world file missing, not a recent world).
    /// </summary>
    public WorldEntryViewModel(string worldName, bool isMissing)
    {
        Title = worldName;
        FilePath = "";
        FileName = "";
        IsMissing = isMissing;
        Backups.CollectionChanged += OnBackupsChanged;
    }

    public string Title { get; }
    public string FilePath { get; }
    public uint Version { get; }
    public int TilesWide { get; }
    public int TilesHigh { get; }
    public DateTime LastModified { get; }
    public long FileSizeBytes { get; }
    [Reactive]
    private bool _isFavorite;
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
    public bool IsCorrupt { get; }
    public string CorruptReason { get; }
    public bool IsCloudSave { get; }
    public string CloudUserId { get; }

    [Reactive]
    private bool _isPinned;

    [Reactive]
    private bool _isExpanded;

    public ObservableCollection<BackupEntryViewModel> Backups { get; } = [];

    public bool HasBackups => Backups.Count > 0;

    public string BackupSummary
    {
        get
        {
            int backups = Backups.Count(e => !e.IsAutosave);
            int autosaves = Backups.Count(e => e.IsAutosave);
            var parts = new System.Collections.Generic.List<string>();
            if (backups > 0) parts.Add($"{backups} backup{(backups != 1 ? "s" : "")}");
            if (autosaves > 0) parts.Add($"{autosaves} autosave{(autosaves != 1 ? "s" : "")}");
            return string.Join(", ", parts);
        }
    }

    private void OnBackupsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(HasBackups));
        this.RaisePropertyChanged(nameof(BackupSummary));
        this.RaisePropertyChanged(nameof(HasAnyBackup));
        this.RaisePropertyChanged(nameof(HasBakFile));
    }

    /// <summary>
    /// Whether the standard Terraria .bak file exists for this world (e.g. WorldName.wld.bak).
    /// </summary>
    public bool HasBakFile => !IsMissing && !string.IsNullOrEmpty(FilePath) && File.Exists(FilePath + ".bak");

    /// <summary>
    /// True if at least the Terraria .bak or a TEdit backup exists.
    /// </summary>
    public bool HasAnyBackup => HasBakFile || HasBackups;

    public void RefreshBackupState()
    {
        this.RaisePropertyChanged(nameof(HasBakFile));
        this.RaisePropertyChanged(nameof(HasAnyBackup));
    }

    public string CloudLabel => IsCloudSave ? $"☁ User {CloudUserId}" : "";

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
        // Favorites first
        if (IsFavorite != other.IsFavorite) return IsFavorite ? -1 : 1;
        // Missing last
        if (IsMissing != other.IsMissing) return IsMissing ? 1 : -1;
        // Then by most recent
        return other.LastModified.CompareTo(LastModified);
    }
}
