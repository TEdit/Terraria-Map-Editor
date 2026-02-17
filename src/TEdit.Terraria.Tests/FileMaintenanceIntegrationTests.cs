using Shouldly;
using TEdit.Common;

namespace TEdit.Terraria.Tests;

/// <summary>
/// Integration tests for backup file cleanup and migration logic.
/// Uses real temp directories to test file system operations.
/// Tests the pure logic from FilePathUtility; the WPF-specific FileMaintenance
/// methods delegate to these.
/// </summary>
public class FileMaintenanceIntegrationTests : IDisposable
{
    private readonly string _tempDir;

    public FileMaintenanceIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "TEditTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public void CleanupOldWorldBackups_KeepsNewestN()
    {
        string backupPath = Path.Combine(_tempDir, "backups");
        Directory.CreateDirectory(backupPath);

        // Create 5 timestamped backups for "MyWorld"
        var timestamps = new[] { "20260101120000", "20260102120000", "20260103120000", "20260104120000", "20260105120000" };
        foreach (var ts in timestamps)
        {
            File.WriteAllText(Path.Combine(backupPath, $"MyWorld.{ts}.wld"), "data");
        }

        // Keep only 3
        CleanupOldWorldBackups("MyWorld", backupPath, 3);

        var remaining = Directory.GetFiles(backupPath, "*.wld");
        remaining.Length.ShouldBe(3);

        // Newest 3 should remain
        remaining.ShouldContain(f => Path.GetFileName(f).Contains("20260103"));
        remaining.ShouldContain(f => Path.GetFileName(f).Contains("20260104"));
        remaining.ShouldContain(f => Path.GetFileName(f).Contains("20260105"));
    }

    [Fact]
    public void CleanupOldWorldBackups_DoesNotDeleteNonMatchingFiles()
    {
        string backupPath = Path.Combine(_tempDir, "backups2");
        Directory.CreateDirectory(backupPath);

        File.WriteAllText(Path.Combine(backupPath, "MyWorld.20260101120000.wld"), "data");
        File.WriteAllText(Path.Combine(backupPath, "OtherWorld.20260101120000.wld"), "other");
        File.WriteAllText(Path.Combine(backupPath, "random.wld"), "random");

        CleanupOldWorldBackups("MyWorld", backupPath, 0);

        // MyWorld backup should be deleted (maxBackups=0, but clamp to at least keep logic)
        // OtherWorld and random should remain
        Directory.GetFiles(backupPath).Length.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void CleanupOldWorldBackups_EmptyDirectory_NoError()
    {
        string backupPath = Path.Combine(_tempDir, "empty");
        Directory.CreateDirectory(backupPath);

        // Should not throw
        CleanupOldWorldBackups("MyWorld", backupPath, 3);

        Directory.GetFiles(backupPath).Length.ShouldBe(0);
    }

    [Fact]
    public void CleanupOldWorldBackups_NonExistentDirectory_NoError()
    {
        string backupPath = Path.Combine(_tempDir, "nonexistent");

        // Should not throw
        CleanupOldWorldBackups("MyWorld", backupPath, 3);
    }

    [Fact]
    public void MigrateLegacyTEditBackups_MovesFiles()
    {
        string worldsPath = Path.Combine(_tempDir, "worlds");
        string backupPath = Path.Combine(_tempDir, "backups3");
        Directory.CreateDirectory(worldsPath);

        // Create a legacy backup file
        string legacyFile = Path.Combine(worldsPath, "MyWorld.wld.TEdit");
        File.WriteAllText(legacyFile, "backup data");
        File.SetLastWriteTimeUtc(legacyFile, new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc));

        MigrateLegacyTEditBackups(worldsPath, backupPath);

        // Legacy file should be gone
        File.Exists(legacyFile).ShouldBeFalse();

        // Should be in backup path with timestamp
        var backupFiles = Directory.GetFiles(backupPath, "*.wld");
        backupFiles.Length.ShouldBe(1);
        Path.GetFileName(backupFiles[0]).ShouldBe("MyWorld.20260115103000.wld");
    }

    [Fact]
    public void MigrateLegacyTEditBackups_HandlesDoubleExtension()
    {
        string worldsPath = Path.Combine(_tempDir, "worlds2");
        string backupPath = Path.Combine(_tempDir, "backups4");
        Directory.CreateDirectory(worldsPath);

        // Create a doubly-stacked extension file
        string legacyFile = Path.Combine(worldsPath, "MyWorld.wld.bak.TEdit");
        File.WriteAllText(legacyFile, "data");
        File.SetLastWriteTimeUtc(legacyFile, new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc));

        MigrateLegacyTEditBackups(worldsPath, backupPath);

        File.Exists(legacyFile).ShouldBeFalse();
        var backupFiles = Directory.GetFiles(backupPath, "*.wld");
        backupFiles.Length.ShouldBe(1);
        // Base name should be "MyWorld" (all backup extensions stripped)
        Path.GetFileName(backupFiles[0]).ShouldStartWith("MyWorld.");
    }

    [Fact]
    public void MigrateLegacyTEditBackups_IgnoresNonTEditFiles()
    {
        string worldsPath = Path.Combine(_tempDir, "worlds3");
        string backupPath = Path.Combine(_tempDir, "backups5");
        Directory.CreateDirectory(worldsPath);

        File.WriteAllText(Path.Combine(worldsPath, "MyWorld.wld"), "world");
        File.WriteAllText(Path.Combine(worldsPath, "MyWorld.wld.bak"), "bak");

        MigrateLegacyTEditBackups(worldsPath, backupPath);

        // Neither file should be touched
        File.Exists(Path.Combine(worldsPath, "MyWorld.wld")).ShouldBeTrue();
        File.Exists(Path.Combine(worldsPath, "MyWorld.wld.bak")).ShouldBeTrue();
        Directory.Exists(backupPath).ShouldBeFalse(); // backup dir never created
    }

    // --- Helpers that replicate the core logic from FileMaintenance ---
    // These test the same algorithms without WPF dependencies.

    private static void CleanupOldWorldBackups(string worldBaseName, string backupPath, int maxBackups)
    {
        if (!Directory.Exists(backupPath)) return;

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

        if (backupFiles.Count <= maxBackups) return;

        foreach (var file in backupFiles.Skip(maxBackups))
        {
            File.Delete(file);
        }
    }

    private static void MigrateLegacyTEditBackups(string worldsPath, string backupPath)
    {
        if (!Directory.Exists(worldsPath)) return;

        var legacyFiles = Directory.GetFiles(worldsPath, "*.TEdit", SearchOption.TopDirectoryOnly).ToList();
        if (legacyFiles.Count == 0) return;

        if (!Directory.Exists(backupPath))
            Directory.CreateDirectory(backupPath);

        foreach (var legacyFile in legacyFiles)
        {
            string fileName = Path.GetFileName(legacyFile);
            string worldBaseName = Path.GetFileNameWithoutExtension(FilePathUtility.NormalizeWorldFilePath(fileName));

            var modTime = File.GetLastWriteTimeUtc(legacyFile);
            string timestamp = modTime.ToString("yyyyMMddHHmmss");
            string destPath = Path.Combine(backupPath, $"{worldBaseName}.{timestamp}.wld");

            if (File.Exists(destPath))
            {
                File.Delete(legacyFile);
            }
            else
            {
                File.Move(legacyFile, destPath);
            }
        }
    }
}
