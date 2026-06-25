using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using ReactiveUI.Builder;

namespace TEdit.Terraria.Tests;

internal static class ModuleInit
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        ExtractWorldFiles();
        RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp();
    }

    private static readonly object _extractLock = new();

    /// <summary>
    /// Extracts world-files.zip to the WorldFiles directory if the zip exists.
    /// This allows .wld files to be stored as a compressed archive in the repo
    /// and extracted on-demand before tests run.
    /// </summary>
    private static void ExtractWorldFiles()
    {
        var worldFilesDir = Path.Combine(AppContext.BaseDirectory, "WorldFiles");
        var zipPath = Path.Combine(worldFilesDir, "world-files.zip");

        if (!File.Exists(zipPath)) return;

        lock (_extractLock)
        {
            using var archive = ZipFile.OpenRead(zipPath);
            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name)) continue; // skip directory entries

                var destinationPath = Path.GetFullPath(Path.Combine(worldFilesDir, entry.FullName));

                // Safety check: ensure destination is inside WorldFiles directory
                var relativePath = Path.GetRelativePath(worldFilesDir, destinationPath);
                if (relativePath.StartsWith("..")) continue;

                var destDir = Path.GetDirectoryName(destinationPath)!;
                Directory.CreateDirectory(destDir);

                if (!File.Exists(destinationPath))
                {
                    entry.ExtractToFile(destinationPath);
                }
            }
        }
    }
}
