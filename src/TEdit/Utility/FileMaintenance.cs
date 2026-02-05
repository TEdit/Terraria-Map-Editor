using System;
using System.IO;
using System.Linq;
using TEdit.ViewModel;

namespace TEdit.Utility;

public static class FileMaintenance
{
    public static void CleanupOldAutosaves()
    {
        try
        {
            string tempPath = WorldViewModel.TempPath;
            if (!Directory.Exists(tempPath))
                return;

            var autosaveFiles = Directory.GetFiles(tempPath, "*.autosave*")
                .Where(f => f.EndsWith(".autosave", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".autosave.tmp", StringComparison.OrdinalIgnoreCase))
                .ToList();

            int totalAutosaveCount = autosaveFiles.Count;
            long totalAutosaveSize = autosaveFiles.Sum(f => new FileInfo(f).Length);

            ErrorLogging.Log($"Autosave cleanup: Found {totalAutosaveCount} autosave files using {FormatFileSize(totalAutosaveSize)}");

            var groupedFiles = autosaveFiles
                .GroupBy(f =>
                {
                    string filename = Path.GetFileName(f);
                    int idx = filename.IndexOf(".autosave", StringComparison.OrdinalIgnoreCase);
                    return idx >= 0 ? filename.Substring(0, idx) : filename;
                });

            int deletedCount = 0;
            long deletedSize = 0;

            foreach (var group in groupedFiles)
            {
                var sortedFiles = group.OrderByDescending(f => File.GetLastWriteTimeUtc(f)).ToList();
                var keepFile = sortedFiles.FirstOrDefault(f => f.EndsWith(".autosave", StringComparison.OrdinalIgnoreCase));

                foreach (var file in sortedFiles)
                {
                    if (file != keepFile)
                    {
                        try
                        {
                            long fileSize = new FileInfo(file).Length;
                            File.Delete(file);
                            deletedCount++;
                            deletedSize += fileSize;
                            ErrorLogging.Log($"Deleted old autosave: {Path.GetFileName(file)} ({FormatFileSize(fileSize)})");
                        }
                        catch
                        {
                            // Ignore errors deleting individual files
                        }
                    }
                }
            }

            if (deletedCount > 0)
            {
                ErrorLogging.Log($"Autosave cleanup complete: Deleted {deletedCount} old autosave files, freed {FormatFileSize(deletedSize)}");
            }
            else
            {
                ErrorLogging.Log("Autosave cleanup complete: No old autosave files to delete");
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
    }

    public static void CleanupOldWorldBackups(string worldFilePath)
    {
        try
        {
            if (!File.Exists(worldFilePath))
                return;

            string directory = Path.GetDirectoryName(worldFilePath);
            string baseFilename = Path.GetFileName(worldFilePath);

            var backupFiles = Directory.GetFiles(directory, baseFilename + ".*.TEdit")
                .Where(f =>
                {
                    string pattern = baseFilename + ".";
                    string remaining = Path.GetFileName(f).Substring(pattern.Length);
                    return remaining.Length >= 14 &&
                           remaining.Substring(0, 14).All(char.IsDigit) &&
                           remaining.EndsWith(".TEdit", StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            if (backupFiles.Count > 0)
            {
                long totalBackupSize = backupFiles.Sum(f => new FileInfo(f).Length);
                ErrorLogging.Log($"Found {backupFiles.Count} old timestamped backup(s) for {baseFilename} using {FormatFileSize(totalBackupSize)}");
            }

            int deletedCount = 0;
            long deletedSize = 0;

            foreach (var file in backupFiles)
            {
                try
                {
                    long fileSize = new FileInfo(file).Length;
                    File.Delete(file);
                    deletedCount++;
                    deletedSize += fileSize;
                    ErrorLogging.Log($"Deleted old backup: {Path.GetFileName(file)} ({FormatFileSize(fileSize)})");
                }
                catch
                {
                    // Ignore errors deleting individual files
                }
            }

            if (deletedCount > 0)
            {
                ErrorLogging.Log($"Backup cleanup complete: Deleted {deletedCount} old backup(s), freed {FormatFileSize(deletedSize)}");
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
    }

    public static void LogWorldBackupFiles()
    {
        try
        {
            string worldsPath = DependencyChecker.PathToWorlds;
            if (!Directory.Exists(worldsPath))
                return;

            var backupFiles = Directory.GetFiles(worldsPath, "*.TEdit", SearchOption.TopDirectoryOnly).ToList();

            int totalBackupCount = backupFiles.Count;
            long totalBackupSize = backupFiles.Sum(f => new FileInfo(f).Length);

            if (totalBackupCount > 0)
            {
                ErrorLogging.Log($"Found {totalBackupCount} .TEdit backup files using {FormatFileSize(totalBackupSize)}");
            }
            else
            {
                ErrorLogging.Log("No .TEdit backup files found");
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
    }

    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
