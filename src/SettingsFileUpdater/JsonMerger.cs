using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SettingsFileUpdater.TerrariaHost.DataModel;

namespace SettingsFileUpdater;

public class MergeResult
{
    public int ExistingCount { get; set; }
    public int AddedCount { get; set; }
    public List<string> AddedKeys { get; set; } = new List<string>();

    public override string ToString()
    {
        if (AddedCount == 0)
            return $"{ExistingCount} existing, up to date";
        return $"{ExistingCount} existing + {AddedCount} new = {ExistingCount + AddedCount} total";
    }
}

public static class JsonMerger
{
    /// <summary>
    /// Merges new items into an existing JSON array file, keyed by integer "id" property.
    /// Items with IDs already present in the file are skipped. Existing data is preserved byte-for-byte.
    /// </summary>
    public static MergeResult MergeById<T>(
        string path,
        IList<T> newItems,
        Func<T, int> idSelector,
        JsonSerializerOptions options)
    {
        if (!File.Exists(path))
        {
            // No existing file — write all items fresh
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(stream, newItems, options);
            return new MergeResult
            {
                ExistingCount = 0,
                AddedCount = newItems.Count,
                AddedKeys = newItems.Select(i => idSelector(i).ToString()).ToList()
            };
        }

        var existingText = File.ReadAllText(path);
        var existingIds = ExtractIntIds(existingText);

        var toAdd = newItems.Where(i => !existingIds.Contains(idSelector(i))).ToList();

        var result = new MergeResult
        {
            ExistingCount = existingIds.Count,
            AddedCount = toAdd.Count,
            AddedKeys = toAdd.Select(i => idSelector(i).ToString()).ToList()
        };

        if (toAdd.Count == 0)
            return result;

        AppendToJsonArray(path, existingText, toAdd, options);
        return result;
    }

    /// <summary>
    /// Merges new items into an existing JSON array file, keyed by string "name" property.
    /// Items with names already present in the file are skipped. Existing data is preserved byte-for-byte.
    /// </summary>
    public static MergeResult MergeByName<T>(
        string path,
        IList<T> newItems,
        Func<T, string> nameSelector,
        JsonSerializerOptions options)
    {
        if (!File.Exists(path))
        {
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(stream, newItems, options);
            return new MergeResult
            {
                ExistingCount = 0,
                AddedCount = newItems.Count,
                AddedKeys = newItems.Select(nameSelector).ToList()
            };
        }

        var existingText = File.ReadAllText(path);
        var existingNames = ExtractStringNames(existingText);

        var toAdd = newItems.Where(i => !existingNames.Contains(nameSelector(i))).ToList();

        var result = new MergeResult
        {
            ExistingCount = existingNames.Count,
            AddedCount = toAdd.Count,
            AddedKeys = toAdd.Select(nameSelector).ToList()
        };

        if (toAdd.Count == 0)
            return result;

        AppendToJsonArray(path, existingText, toAdd, options);
        return result;
    }

    /// <summary>
    /// Extracts all integer "id" values from a JSON array.
    /// Handles the edge case where id=0 is omitted due to DefaultIgnoreCondition.WhenWritingDefault.
    /// </summary>
    private static HashSet<int> ExtractIntIds(string json)
    {
        var ids = new HashSet<int>();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
            return ids;

        foreach (var element in root.EnumerateArray())
        {
            if (element.TryGetProperty("id", out var idProp))
            {
                ids.Add(idProp.GetInt32());
            }
            else
            {
                // No "id" property means id=0 (due to DefaultIgnoreCondition.WhenWritingDefault)
                ids.Add(0);
            }
        }

        return ids;
    }

    /// <summary>
    /// Extracts all string "name" values from a JSON array.
    /// </summary>
    private static HashSet<string> ExtractStringNames(string json)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
            return names;

        foreach (var element in root.EnumerateArray())
        {
            if (element.TryGetProperty("name", out var nameProp))
            {
                var name = nameProp.GetString();
                if (name != null)
                    names.Add(name);
            }
        }

        return names;
    }

    /// <summary>
    /// Appends serialized items to an existing JSON array file by string manipulation.
    /// Finds the last ']' and inserts new entries before it, preserving existing content byte-for-byte.
    /// </summary>
    private static void AppendToJsonArray<T>(
        string path,
        string existingText,
        IList<T> items,
        JsonSerializerOptions options)
    {
        // Find the last ']' in the file
        int closeBracketPos = existingText.LastIndexOf(']');
        if (closeBracketPos < 0)
            throw new InvalidOperationException($"Could not find closing ']' in {path}");

        // Detect indentation style from existing file
        int indent = DetectIndent(existingText);

        // Build the new entries
        var sb = new StringBuilder();

        // Add comma after the last existing entry
        // Find the last non-whitespace character before the ']'
        int lastContentPos = closeBracketPos - 1;
        while (lastContentPos >= 0 && char.IsWhiteSpace(existingText[lastContentPos]))
            lastContentPos--;

        string beforeBracket = existingText.Substring(0, lastContentPos + 1);
        string afterLastContent = existingText.Substring(lastContentPos + 1, closeBracketPos - lastContentPos - 1);
        string afterBracket = existingText.Substring(closeBracketPos + 1);

        sb.Append(beforeBracket);

        // Add comma if the last content char isn't already a comma
        if (lastContentPos >= 0 && existingText[lastContentPos] != ',')
            sb.Append(',');

        // Serialize and append each new item
        foreach (var item in items)
        {
            var itemJson = JsonSerializer.Serialize(item, options);
            sb.AppendLine();
            sb.Append(IndentJson(itemJson, indent));
        }

        // Close the array with original whitespace/newline
        sb.Append(afterLastContent);
        sb.Append(']');
        sb.Append(afterBracket);

        File.WriteAllText(path, sb.ToString());
    }

    /// <summary>
    /// Detects the indentation level (in spaces) used for array elements in a JSON file.
    /// </summary>
    private static int DetectIndent(string json)
    {
        // Look for the first line that starts with whitespace after the opening '['
        var lines = json.Split('\n');
        foreach (var line in lines.Skip(1))
        {
            var trimmed = line.TrimStart();
            if (trimmed.Length > 0 && trimmed[0] == '{' || trimmed[0] == '"')
            {
                return line.Length - line.TrimStart().Length;
            }
        }
        return 2; // default
    }

    /// <summary>
    /// Indents each line of a JSON string by the specified number of spaces.
    /// </summary>
    private static string IndentJson(string json, int spaces)
    {
        var prefix = new string(' ', spaces);
        var lines = json.Split('\n');
        var sb = new StringBuilder();
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0)
                sb.AppendLine();
            sb.Append(prefix);
            sb.Append(lines[i].TrimEnd('\r'));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Merges bestiary data using JsonNode for clean mutation.
    /// The bestiary file is a single JSON object with cat/dog/bunny string arrays and an npcData array.
    /// </summary>
    public static MergeResult MergeBestiary(
        string path,
        object newBestiaryConfig,
        IList<object> newNpcData,
        Func<object, int> npcIdSelector,
        IList<string> newCat,
        IList<string> newDog,
        IList<string> newBunny,
        JsonSerializerOptions options)
    {
        if (!File.Exists(path))
        {
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(stream, newBestiaryConfig, options);
            return new MergeResult
            {
                ExistingCount = 0,
                AddedCount = newNpcData.Count
            };
        }

        var existingText = File.ReadAllText(path);
        var root = JsonNode.Parse(existingText)!.AsObject();

        // Collect existing NPC IDs
        var existingNpcIds = new HashSet<int>();
        var npcArray = root["npcData"]?.AsArray();
        if (npcArray != null)
        {
            foreach (var entry in npcArray)
            {
                var id = entry?["id"];
                existingNpcIds.Add(id != null ? id.GetValue<int>() : 0);
            }
        }

        // Collect existing string sets
        HashSet<string> ExistingStrings(string key) =>
            root[key]?.AsArray()
                .Select(n => n?.GetValue<string>())
                .Where(s => s != null)
                .ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>();

        var existingCat = ExistingStrings("cat");
        var existingDog = ExistingStrings("dog");
        var existingBunny = ExistingStrings("bunny");

        var newNpcEntries = newNpcData.Where(n => !existingNpcIds.Contains(npcIdSelector(n))).ToList();
        var newCatEntries = newCat.Where(c => !existingCat.Contains(c)).ToList();
        var newDogEntries = newDog.Where(d => !existingDog.Contains(d)).ToList();
        var newBunnyEntries = newBunny.Where(b => !existingBunny.Contains(b)).ToList();

        var result = new MergeResult
        {
            ExistingCount = existingNpcIds.Count,
            AddedCount = newNpcEntries.Count
        };

        if (newNpcEntries.Count == 0 && newCatEntries.Count == 0 &&
            newDogEntries.Count == 0 && newBunnyEntries.Count == 0)
            return result;

        // Mutate the JsonNode tree
        foreach (var c in newCatEntries) root["cat"]!.AsArray().Add(c);
        foreach (var d in newDogEntries) root["dog"]!.AsArray().Add(d);
        foreach (var b in newBunnyEntries) root["bunny"]!.AsArray().Add(b);

        foreach (var entry in newNpcEntries)
        {
            var entryNode = JsonSerializer.SerializeToNode(entry, options);
            npcArray!.Add(entryNode);
        }

        File.WriteAllText(path, root.ToJsonString(options));
        return result;
    }

    /// <summary>
    /// Merges a new version entry into a versions.json file.
    /// Uses typed deserialization so int[] FramedTileIds round-trips through InlineNumericArrayConverter.
    /// </summary>
    public static bool MergeVersionData(
        string path,
        VersionData newVersion,
        JsonSerializerOptions options)
    {
        if (!File.Exists(path))
            return false;

        var existingText = File.ReadAllText(path);
        var file = JsonSerializer.Deserialize<VersionsFileJson>(existingText, options);
        if (file == null)
            return false;

        // Check if saveVersion already exists
        if (file.SaveVersions.Any(v => v.SaveVersion == newVersion.SaveVersion))
            return false;

        file.SaveVersions.Add(newVersion);

        if (!file.GameVersionToSaveVersion.ContainsKey(newVersion.GameVersion))
            file.GameVersionToSaveVersion[newVersion.GameVersion] = newVersion.SaveVersion;

        var json = JsonSerializer.Serialize(file, options);
        File.WriteAllText(path, json);
        return true;
    }
}
