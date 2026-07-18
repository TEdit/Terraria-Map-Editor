using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace TEdit.Terraria.Tests;

internal static class TestWorldArchives
{
    private static readonly object PrepareLock = new();

    public static void Prepare()
    {
        var worldFilesDirectory = Path.Combine(AppContext.BaseDirectory, "WorldFiles");
        if (!Directory.Exists(worldFilesDirectory))
            return;

        lock (PrepareLock)
        {
            foreach (var archivePath in Directory.EnumerateFiles(
                worldFilesDirectory,
                "*.wld.zip",
                SearchOption.AllDirectories))
            {
                ExtractWorld(archivePath);
            }

            var unpackagedWorlds = Directory.EnumerateFiles(
                    worldFilesDirectory,
                    "*.wld",
                    SearchOption.AllDirectories)
                .Where(worldPath => !File.Exists(worldPath + ".zip"))
                .Select(worldPath => Path.GetRelativePath(worldFilesDirectory, worldPath))
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (unpackagedWorlds.Length > 0)
            {
                throw new InvalidOperationException(
                    "Uncompressed test world files are missing immutable archives: " +
                    string.Join(", ", unpackagedWorlds) + Environment.NewLine +
                    "Add each fixture with: ./tools/Add-TestWorld.ps1 <path-to-world.wld>");
            }
        }
    }

    private static void ExtractWorld(string archivePath)
    {
        EnsureLfsContentIsPresent(archivePath);

        using var archive = ZipFile.OpenRead(archivePath);
        var files = archive.Entries.Where(entry => !string.IsNullOrEmpty(entry.Name)).ToArray();
        var expectedWorldName = Path.GetFileNameWithoutExtension(archivePath);

        if (files.Length != 1 ||
            !string.Equals(files[0].FullName, expectedWorldName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"Test world archive '{archivePath}' must contain exactly one root entry named " +
                $"'{expectedWorldName}'.");
        }

        var destinationPath = Path.Combine(Path.GetDirectoryName(archivePath)!, expectedWorldName);
        if (File.Exists(destinationPath) && new FileInfo(destinationPath).Length == files[0].Length)
            return;

        files[0].ExtractToFile(destinationPath, overwrite: true);
    }

    private static void EnsureLfsContentIsPresent(string archivePath)
    {
        using var stream = File.OpenRead(archivePath);
        if (stream.Length > 200)
            return;

        Span<byte> prefix = stackalloc byte[8];
        if (stream.Read(prefix) == prefix.Length &&
            Encoding.ASCII.GetString(prefix) == "version ")
        {
            throw new InvalidDataException(
                $"Test world archive '{archivePath}' is a Git LFS pointer. " +
                "Run 'git lfs pull --include=\"src/TEdit.Tests/WorldFiles/**/*.wld.zip\"' first.");
        }
    }
}
