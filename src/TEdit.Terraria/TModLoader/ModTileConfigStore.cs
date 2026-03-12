using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using TEdit.Common.Serialization;
using TEdit.Geometry;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.TModLoader;

/// <summary>
/// Per-tile frame override matching TileProperty shape.
/// Stores user-configured frame data for mod tiles.
/// </summary>
public class ModTileOverride
{
    public Vector2Short TextureGrid { get; set; } = new Vector2Short(16, 16);
    public Vector2Short FrameGap { get; set; } = new Vector2Short(2, 2);
    public Vector2Short[]? FrameSize { get; set; }
    public bool IsAnimated { get; set; }
    public List<FrameProperty>? Frames { get; set; }

    /// <summary>
    /// Per-row pixel heights within a single frame. Matches Terraria's TileObjectData.CoordinateHeights.
    /// Example: [16, 18] for chests where the bottom row extends 2px.
    /// If null, all rows use TextureGrid.Y.
    /// </summary>
    public short[]? CoordinateHeights { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TreeMode TreeMode { get; set; } = TreeMode.None;
}

/// <summary>
/// Root document for a per-mod override file.
/// </summary>
public class ModTileOverrideFile
{
    public Dictionary<string, ModTileOverride> Tiles { get; set; } = new();
}

/// <summary>
/// Three-tier tile override resolution:
///   1. User config   — {dataDir}/modTileOverrides/{ModName}.json  (highest priority, read-write)
///   2. Bundled        — embedded resource Data/TileOverrides/{ModName}.json  (shipped with editor, read-only)
///   3. Heuristic      — pixel detection + dimension fallback  (caller handles when null returned)
///
/// Also supports vanilla overrides using mod name "Terraria".
/// </summary>
public class ModTileConfigStore
{
    private readonly string _overrideDir;
    private static readonly JsonSerializerOptions Options = TEditJsonSerializer.DefaultOptions;

    // Cache to avoid repeated disk/resource reads within a session.
    private readonly Dictionary<string, ModTileOverrideFile> _userCache = new();
    private readonly Dictionary<string, ModTileOverrideFile?> _bundledCache = new();

    public ModTileConfigStore(string dataDir)
    {
        _overrideDir = Path.Combine(dataDir, "modTileOverrides");
    }

    /// <summary>
    /// Gets the override for a specific tile using three-tier resolution:
    /// user config → bundled config → null (heuristic).
    /// </summary>
    public ModTileOverride? GetTileOverride(string modName, string tileName)
    {
        // Tier 1: User config (highest priority)
        var userFile = LoadUser(modName);
        if (userFile.Tiles.TryGetValue(tileName, out var userResult))
            return userResult;

        // Tier 2: Bundled config (shipped with editor)
        var bundledFile = LoadBundled(modName);
        if (bundledFile?.Tiles.TryGetValue(tileName, out var bundledResult) == true)
            return bundledResult;

        // Tier 3: null — caller falls back to heuristic
        return null;
    }

    /// <summary>
    /// Loads user tile overrides for a given mod.
    /// Returns empty dictionary if no user override file exists.
    /// </summary>
    public ModTileOverrideFile LoadUser(string modName)
    {
        if (_userCache.TryGetValue(modName, out var cached))
            return cached;

        var path = GetUserFilePath(modName);
        ModTileOverrideFile result;
        if (!File.Exists(path))
        {
            result = new ModTileOverrideFile();
        }
        else
        {
            var json = File.ReadAllText(path);
            result = JsonSerializer.Deserialize<ModTileOverrideFile>(json, Options)
                     ?? new ModTileOverrideFile();
        }

        _userCache[modName] = result;
        return result;
    }

    /// <summary>
    /// Loads bundled tile overrides from embedded resources.
    /// Returns null if no bundled config exists for this mod.
    /// </summary>
    public ModTileOverrideFile? LoadBundled(string modName)
    {
        if (_bundledCache.TryGetValue(modName, out var cached))
            return cached;

        var assembly = typeof(ModTileConfigStore).Assembly;
        var resourceName = $"TEdit.Terraria.Data.TileOverrides.{modName}.json";
        using var stream = assembly.GetManifestResourceStream(resourceName);

        ModTileOverrideFile? result = null;
        if (stream != null)
        {
            result = JsonSerializer.Deserialize<ModTileOverrideFile>(stream, Options);
        }

        _bundledCache[modName] = result;
        return result;
    }

    /// <summary>
    /// Saves tile overrides to the user config directory.
    /// Creates the directory if it doesn't exist.
    /// </summary>
    public void Save(string modName, ModTileOverrideFile overrideFile)
    {
        Directory.CreateDirectory(_overrideDir);
        var path = GetUserFilePath(modName);
        var json = JsonSerializer.Serialize(overrideFile, Options);
        File.WriteAllText(path, json);
        _userCache[modName] = overrideFile; // update cache
    }

    /// <summary>
    /// Saves a single tile override to the user config.
    /// Loads existing user file, updates the entry, and saves back.
    /// </summary>
    public void SaveTile(string modName, string tileName, ModTileOverride tileOverride)
    {
        var file = LoadUser(modName);
        file.Tiles[tileName] = tileOverride;
        Save(modName, file);
    }

    /// <summary>
    /// Removes a single tile override from the user config.
    /// </summary>
    public bool RemoveTile(string modName, string tileName)
    {
        var file = LoadUser(modName);
        if (file.Tiles.Remove(tileName))
        {
            if (file.Tiles.Count == 0)
            {
                // Delete the file if no user overrides remain
                var path = GetUserFilePath(modName);
                if (File.Exists(path))
                    File.Delete(path);
                _userCache.Remove(modName);
            }
            else
            {
                Save(modName, file);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Lists all mod names that have user override files.
    /// </summary>
    public IEnumerable<string> GetModsWithUserOverrides()
    {
        if (!Directory.Exists(_overrideDir))
            yield break;

        foreach (var file in Directory.EnumerateFiles(_overrideDir, "*.json"))
        {
            yield return Path.GetFileNameWithoutExtension(file);
        }
    }

    /// <summary>
    /// Lists all mod names that have bundled override files.
    /// </summary>
    public IEnumerable<string> GetModsWithBundledOverrides()
    {
        var assembly = typeof(ModTileConfigStore).Assembly;
        const string prefix = "TEdit.Terraria.Data.TileOverrides.";
        const string suffix = ".json";

        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (name.StartsWith(prefix) && name.EndsWith(suffix))
            {
                yield return name[prefix.Length..^suffix.Length];
            }
        }
    }

    /// <summary>
    /// Clears the in-memory cache, forcing re-reads from disk/resources.
    /// </summary>
    public void ClearCache()
    {
        _userCache.Clear();
        _bundledCache.Clear();
    }

    public string GetUserFilePath(string modName) =>
        Path.Combine(_overrideDir, $"{modName}.json");

    // Keep backward compat for existing callers
    [Obsolete("Use LoadUser instead")]
    public ModTileOverrideFile Load(string modName) => LoadUser(modName);

    [Obsolete("Use GetUserFilePath instead")]
    public string GetFilePath(string modName) => GetUserFilePath(modName);
}
