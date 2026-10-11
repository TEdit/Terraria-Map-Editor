using System;
using System.IO;

namespace TEdit.Configuration;

/// <summary>
/// Centralizes the TEdit application data directory.
/// Portable mode is active when:
///   1. A "Data" folder already exists next to the executable (explicit opt-in), OR
///   2. TEdit is NOT a Velopack install (no sq.version file — i.e. extracted from a zip).
/// In portable mode all settings, backups, undo history, logs, and scripts
/// are stored in a "Data" folder next to the executable.
/// Otherwise, the standard %APPDATA%\TEdit location is used.
/// </summary>
public static class AppDataPaths
{
    public static bool IsPortable { get; }
    public static string DataDir { get; }

    static AppDataPaths()
    {
        var exeDir = AppDomain.CurrentDomain.BaseDirectory;
        var portableDir = Path.Combine(exeDir, "Data");

        // Explicit: user created a Data folder next to the exe.
        // Auto-detect: no sq.version means this isn't a Velopack install (portable zip).
        bool isVelopackInstall = File.Exists(Path.Combine(exeDir, "sq.version"));

        if (Directory.Exists(portableDir) || (!isVelopackInstall && TryCreateDirectory(portableDir)))
        {
            IsPortable = true;
            DataDir = portableDir;
        }
        else
        {
            // Velopack install, or a portable copy in a read-only folder (a throw here would
            // fail this static constructor and crash startup).
            IsPortable = false;
            DataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TEdit");
        }
    }

    private static bool TryCreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
