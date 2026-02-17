using System;
using System.IO;
using System.Linq;
using TEdit.Common;
using TEdit.ViewModel;

namespace TEdit.Utility;

public static class FileMaintenance
{
    /// <summary>
    /// Strips backup extensions (.TEdit, .bak, .autosave, .tmp) iteratively until a .wld file remains.
    /// </summary>
    public static string NormalizeWorldFilePath(string filename) =>
        FilePathUtility.NormalizeWorldFilePath(filename);

    /// <summary>
    /// Returns true if the file has a backup extension (.TEdit, .bak, .autosave, .tmp).
    /// </summary>
    public static bool IsBackupFile(string filename) =>
        FilePathUtility.IsBackupFile(filename);

    public static void CleanupOldAutosaves()
    {
        try
        {
            string autosavePath = WorldViewModel.AutoSavePath;
            if (!Directory.Exists(autosavePath))
                return;

            // Also clean up legacy autosaves from the root TEdit folder
            CleanupLegacyAutosaves();

            var autosaveFiles = Directory.GetFiles(autosavePath, "*.autosave*")
                .Where(f => f.EndsWith(".autosave", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".autosave.tmp", StringComparison.OrdinalIgnoreCase))
                .ToList();

            int totalAutosaveCount = autosaveFiles.Count;
            long totalAutosaveSize = autosaveFiles.Sum(f => new FileInfo(f).Length);

            ErrorLogging.LogInfo($"Autosave cleanup: Found {totalAutosaveCount} autosave files using {FormatFileSize(totalAutosaveSize)}");

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
                            ErrorLogging.LogDebug($"Deleted old autosave: {Path.GetFileName(file)} ({FormatFileSize(fileSize)})");
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
                ErrorLogging.LogInfo($"Autosave cleanup complete: Deleted {deletedCount} old autosave files, freed {FormatFileSize(deletedSize)}");
            }
            else
            {
                ErrorLogging.LogInfo("Autosave cleanup complete: No old autosave files to delete");
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
    }

    /// <summary>
    /// Cleans up old world backups in the backup directory, keeping only the newest N.
    /// Looks for files matching pattern: WorldBaseName.YYYYMMDDHHMMSS.wld
    /// </summary>
    public static void CleanupOldWorldBackups(string worldBaseName, string backupPath, int maxBackups)
    {
        try
        {
            if (!Directory.Exists(backupPath))
                return;

            var backupFiles = Directory.GetFiles(backupPath, worldBaseName + ".*.wld")
                .Where(f =>
                {
                    string name = Path.GetFileNameWithoutExtension(f);
                    string prefix = worldBaseName + ".";
                    if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        return false;
                    string timestamp = name.Substring(prefix.Length);
                    return timestamp.Length == 14 && timestamp.All(char.IsDigit);
                })
                .OrderByDescending(f => Path.GetFileNameWithoutExtension(f))
                .ToList();

            if (backupFiles.Count <= maxBackups)
                return;

            var toDelete = backupFiles.Skip(maxBackups).ToList();

            int deletedCount = 0;
            long deletedSize = 0;

            foreach (var file in toDelete)
            {
                try
                {
                    long fileSize = new FileInfo(file).Length;
                    File.Delete(file);
                    deletedCount++;
                    deletedSize += fileSize;
                    ErrorLogging.LogDebug($"Deleted old backup: {Path.GetFileName(file)} ({FormatFileSize(fileSize)})");
                }
                catch
                {
                    // Ignore errors deleting individual files
                }
            }

            if (deletedCount > 0)
            {
                ErrorLogging.LogInfo($"Backup cleanup complete: Deleted {deletedCount} old backup(s) for {worldBaseName}, freed {FormatFileSize(deletedSize)}");
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
    }

    /// <summary>
    /// Migrates legacy .TEdit backup files from next to world files into the centralized backup directory.
    /// </summary>
    public static void MigrateLegacyTEditBackups(string worldsPath, string backupPath)
    {
        try
        {
            if (!Directory.Exists(worldsPath))
                return;

            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            var legacyFiles = Directory.GetFiles(worldsPath, "*.TEdit", SearchOption.TopDirectoryOnly).ToList();

            if (legacyFiles.Count == 0)
                return;

            ErrorLogging.LogInfo($"Migrating {legacyFiles.Count} legacy .TEdit backup(s) to {backupPath}");

            int migratedCount = 0;
            foreach (var legacyFile in legacyFiles)
            {
                try
                {
                    string fileName = Path.GetFileName(legacyFile);
                    // Strip all backup extensions to get the world base name
                    string worldBaseName = Path.GetFileNameWithoutExtension(NormalizeWorldFilePath(fileName));

                    // Use file modification time for the timestamp
                    var modTime = File.GetLastWriteTimeUtc(legacyFile);
                    string timestamp = modTime.ToString("yyyyMMddHHmmss");

                    string destPath = Path.Combine(backupPath, $"{worldBaseName}.{timestamp}.wld");

                    if (File.Exists(destPath))
                    {
                        // Duplicate timestamp — delete legacy file
                        File.Delete(legacyFile);
                        ErrorLogging.LogDebug($"Deleted duplicate legacy backup: {fileName}");
                    }
                    else
                    {
                        File.Move(legacyFile, destPath);
                        ErrorLogging.LogDebug($"Migrated legacy backup: {fileName} -> {Path.GetFileName(destPath)}");
                    }
                    migratedCount++;
                }
                catch (Exception ex)
                {
                    ErrorLogging.LogWarn($"Failed to migrate {Path.GetFileName(legacyFile)}: {ex.Message}");
                }
            }

            ErrorLogging.LogInfo($"Legacy backup migration complete: {migratedCount} file(s) processed");
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
                ErrorLogging.LogInfo($"Found {totalBackupCount} .TEdit backup files using {FormatFileSize(totalBackupSize)}");
            }
            else
            {
                ErrorLogging.LogInfo("No .TEdit backup files found");
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
    }

    private static void CleanupLegacyAutosaves()
    {
        try
        {
            string tempPath = WorldViewModel.TempPath;
            if (!Directory.Exists(tempPath))
                return;

            var legacyFiles = Directory.GetFiles(tempPath, "*.autosave*")
                .Where(f => f.EndsWith(".autosave", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".autosave.tmp", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var file in legacyFiles)
            {
                try
                {
                    File.Delete(file);
                    ErrorLogging.LogDebug($"Deleted legacy autosave from root folder: {Path.GetFileName(file)}");
                }
                catch
                {
                    // Ignore errors deleting individual files
                }
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
    }

    public static string FormatFileSize(long bytes) =>
        FilePathUtility.FormatFileSize(bytes);
}
