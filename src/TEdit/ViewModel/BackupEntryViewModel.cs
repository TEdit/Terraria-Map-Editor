using System;
using System.IO;
using TEdit.Properties;
using TEdit.Utility;

namespace TEdit.ViewModel;

/// <summary>
/// Represents a single backup or autosave file entry in the World Explorer (Tab 2).
/// </summary>
public class BackupEntryViewModel
{
    public BackupEntryViewModel(string filePath)
    {
        FilePath = filePath;
        var fi = new FileInfo(filePath);
        FileSizeBytes = fi.Exists ? fi.Length : 0;
        Timestamp = fi.Exists ? fi.LastWriteTimeUtc : DateTime.MinValue;
        IsAutosave = filePath.EndsWith(".autosave", StringComparison.OrdinalIgnoreCase);
        IsTerrariaBackup = filePath.EndsWith(".bak", StringComparison.OrdinalIgnoreCase);
        IsTEditBackup = filePath.Contains(".TEdit", StringComparison.OrdinalIgnoreCase)
            && !IsAutosave && !IsTerrariaBackup;
    }

    public string FilePath { get; }
    public DateTime Timestamp { get; }
    public long FileSizeBytes { get; }
    public bool IsAutosave { get; }
    public bool IsTerrariaBackup { get; }
    public bool IsTEditBackup { get; }

    public string TypeLabel =>
        IsAutosave ? Language.backup_type_autosave :
        IsTerrariaBackup ? Language.backup_type_terraria :
        IsTEditBackup ? Language.backup_type_tedit :
        Language.backup_type_generic;

    public string TimestampText => Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    public string SizeText => FileMaintenance.FormatFileSize(FileSizeBytes);
    public string DisplayText => $"{TimestampText} [{TypeLabel}] {SizeText}";
}
