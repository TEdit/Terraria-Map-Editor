using System.Text.Json;
using TEdit.Common;
using TEdit.ModScraper;

// Parse arguments
string? modsDirectory = null;
string outputPath = "modColors.json";
bool listFiles = false;
bool verbose = false;

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--output" && i + 1 < args.Length)
    {
        outputPath = args[++i];
    }
    else if (args[i] == "--list")
    {
        listFiles = true;
    }
    else if (args[i] == "--verbose" || args[i] == "-v")
    {
        verbose = true;
    }
    else if (!args[i].StartsWith("--"))
    {
        modsDirectory = args[i];
    }
}

// Search common locations for .tmod files
var searchPaths = new List<string>();

if (!string.IsNullOrEmpty(modsDirectory))
{
    searchPaths.Add(modsDirectory);
}
else
{
    // Default tModLoader mods path
    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    searchPaths.Add(Path.Combine(userProfile, "My Games", "Terraria", "ModLoader", "Mods"));
    searchPaths.Add(Path.Combine(userProfile, "My Games", "Terraria", "tModLoader", "Mods"));

    // Steam Workshop path for tModLoader (app ID 1281930)
    string steamWorkshop = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        "Steam", "steamapps", "workshop", "content", "1281930");
    if (Directory.Exists(steamWorkshop))
        searchPaths.Add(steamWorkshop);
}

// Find all .tmod files
var tmodFiles = new List<string>();
foreach (var searchPath in searchPaths)
{
    if (!Directory.Exists(searchPath))
    {
        Console.Error.WriteLine($"Warning: Directory not found: {searchPath}");
        continue;
    }

    tmodFiles.AddRange(Directory.GetFiles(searchPath, "*.tmod", SearchOption.AllDirectories));
}

if (tmodFiles.Count == 0)
{
    Console.Error.WriteLine("No .tmod files found. Specify a directory containing .tmod files.");
    Console.Error.WriteLine("Usage: TEdit.ModScraper <modsDirectory> [--output modColors.json]");
    return 1;
}

Console.WriteLine($"Found {tmodFiles.Count} .tmod file(s)");

// Process each .tmod file
var result = new Dictionary<string, ModColorData>();

foreach (var tmodPath in tmodFiles)
{
    try
    {
        Console.Write($"Processing: {Path.GetFileName(tmodPath)}...");
        var archive = TmodArchive.Open(tmodPath);
        Console.Write($" [{archive.ModName} v{archive.ModVersion}]");

        if (listFiles)
        {
            Console.WriteLine();
            foreach (var f in archive.ListFiles())
                Console.WriteLine($"  {f}");
            continue;
        }

        var modData = new ModColorData();

        // Extract tile textures
        var tileTextures = FindTextures(archive, "Tiles", verbose);
        foreach (var (name, path, isRawImg) in tileTextures)
        {
            var data = archive.GetFile(path);
            if (data != null)
            {
                var color = TextureColorExtractor.SampleAverageColor(data, isRawImg);
                modData.Tiles[name] = new ColorEntry
                {
                    Color = FormatColor(color),
                    Name = SplitCamelCase(name),
                };
            }
        }

        // Extract wall textures
        var wallTextures = FindTextures(archive, "Walls", verbose);
        foreach (var (name, path, isRawImg) in wallTextures)
        {
            var data = archive.GetFile(path);
            if (data != null)
            {
                var color = TextureColorExtractor.SampleAverageColor(data, isRawImg);
                modData.Walls[name] = new ColorEntry
                {
                    Color = FormatColor(color),
                    Name = SplitCamelCase(name),
                };
            }
        }

        if (modData.Tiles.Count > 0 || modData.Walls.Count > 0)
        {
            // Merge with existing data for same mod (multiple .tmod files)
            if (result.TryGetValue(archive.ModName, out var existing))
            {
                foreach (var kvp in modData.Tiles)
                    existing.Tiles.TryAdd(kvp.Key, kvp.Value);
                foreach (var kvp in modData.Walls)
                    existing.Walls.TryAdd(kvp.Key, kvp.Value);
            }
            else
            {
                result[archive.ModName] = modData;
            }
        }

        Console.WriteLine($" {modData.Tiles.Count} tiles, {modData.Walls.Count} walls");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"\nError processing {Path.GetFileName(tmodPath)}: {ex.Message}");
    }
}

if (result.Count == 0)
{
    Console.Error.WriteLine("No tile/wall textures found in any .tmod files.");
    return 1;
}

// Write output
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(outputPath, json);

int totalTiles = result.Values.Sum(m => m.Tiles.Count);
int totalWalls = result.Values.Sum(m => m.Walls.Count);
Console.WriteLine($"\nWrote {outputPath}: {result.Count} mod(s), {totalTiles} tiles, {totalWalls} walls");
return 0;


// --- Helper methods ---

static List<(string Name, string Path, bool IsRawImg)> FindTextures(TmodArchive archive, string category, bool verbose = false)
{
    var results = new List<(string Name, string Path, bool IsRawImg)>();
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    // Try multiple path patterns used by different tModLoader versions
    string[] prefixes;
    if (category == "Tiles")
        prefixes = ["Content/Tiles/", $"{archive.ModName}/Content/Tiles/", "Tiles/", "Assets/Tiles/"];
    else
        prefixes = ["Content/Walls/", $"{archive.ModName}/Content/Walls/", "Walls/", "Assets/Walls/"];

    foreach (var prefix in prefixes)
    {
        foreach (var filePath in archive.ListFiles(prefix))
        {
            bool isPng = filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
            bool isRawImg = filePath.EndsWith(".rawimg", StringComparison.OrdinalIgnoreCase);

            if (!isPng && !isRawImg)
                continue;

            // Skip glow masks, highlights, flame overlays, etc.
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (fileName.EndsWith("_Glow", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith("_Highlight", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith("_Flame", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith("_Map", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith("_Wings", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith("_Head", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith("_Body", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith("_Legs", StringComparison.OrdinalIgnoreCase))
                continue;

            if (verbose)
                Console.Error.WriteLine($"    Found: {filePath}");

            if (seen.Add(fileName))
            {
                results.Add((fileName, filePath, isRawImg));
            }
        }
    }

    return results;
}

static string FormatColor(TEditColor color)
{
    return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
}

static string SplitCamelCase(string name)
{
    var sb = new System.Text.StringBuilder();
    for (int i = 0; i < name.Length; i++)
    {
        if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
            sb.Append(' ');
        sb.Append(name[i]);
    }
    return sb.ToString();
}


// --- Data models for JSON output ---

class ModColorData
{
    public Dictionary<string, ColorEntry> Tiles { get; set; } = new();
    public Dictionary<string, ColorEntry> Walls { get; set; } = new();
}

class ColorEntry
{
    public string Color { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
