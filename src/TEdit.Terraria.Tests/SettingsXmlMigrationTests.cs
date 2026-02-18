using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace TEdit.Terraria.Tests;

public class SettingsXmlMigrationTests
{
    private readonly ITestOutputHelper _output;

    public SettingsXmlMigrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static string GetRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null && !File.Exists(Path.Combine(dir, "TEdit.slnx")) && !File.Exists(Path.Combine(dir, "src", "TEdit.slnx")))
        {
            dir = Directory.GetParent(dir)?.FullName;
        }
        if (dir != null && File.Exists(Path.Combine(dir, "src", "TEdit.slnx")))
            return dir;
        if (dir != null && File.Exists(Path.Combine(dir, "TEdit.slnx")))
            return Directory.GetParent(dir)?.FullName ?? dir;
        throw new InvalidOperationException("Could not find repository root");
    }

    private static string ArgbToRgba(string? argb)
    {
        if (string.IsNullOrEmpty(argb) || !argb.StartsWith("#") || argb.Length != 9)
            return argb ?? "";
        // #AARRGGBB -> #RRGGBBAA
        return "#" + argb.Substring(3) + argb.Substring(1, 2);
    }

    private static int[]? ParseVector(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        var parts = value.Split(',');
        if (parts.Length == 2 && int.TryParse(parts[0], out var x) && int.TryParse(parts[1], out var y))
            return new[] { x, y };
        return null;
    }

    /// <summary>
    /// Compacts JSON for readability:
    /// - Numeric arrays become single-line: [16, 16]
    /// - Small objects (2-4 simple properties) become single-line
    /// </summary>
    private static string CompactNumericArrays(string json)
    {
        // Compact arrays containing only numbers (any length)
        json = Regex.Replace(json, @"\[\s*(-?\d+(?:\s*,\s*-?\d+)*)\s*\]",
            m => "[" + Regex.Replace(m.Groups[1].Value, @"\s+", " ").Trim() + "]");

        // Compact arrays of 2-element arrays: [[1, 2], [3, 4]]
        json = Regex.Replace(json, @"\[\s*(\[-?\d+, -?\d+\](?:\s*,\s*\[-?\d+, -?\d+\])*)\s*\]",
            m => "[" + Regex.Replace(m.Groups[1].Value, @"\s+", " ").Trim() + "]");

        // Compact small objects with 2-4 properties containing only simple values
        // Match: { "key": value, "key2": value2 } where values are strings, numbers, or compact arrays
        json = Regex.Replace(json,
            @"\{\s*\n\s*(""[^""]+"":\s*(?:""[^""]*""|-?\d+(?:\.\d+)?|\[[^\[\]]*\]|true|false|null))" +
            @"(?:,\s*\n\s*(""[^""]+"":\s*(?:""[^""]*""|-?\d+(?:\.\d+)?|\[[^\[\]]*\]|true|false|null))){1,3}" +
            @"\s*\n\s*\}",
            m =>
            {
                // Extract all the property matches and join them
                var content = m.Value;
                // Remove newlines and excess whitespace, keeping structure
                var compact = Regex.Replace(content, @"\s*\n\s*", " ");
                compact = Regex.Replace(compact, @"\{\s+", "{ ");
                compact = Regex.Replace(compact, @"\s+\}", " }");
                compact = Regex.Replace(compact, @",\s+", ", ");
                return compact;
            });

        return json;
    }

    [Fact(Skip = "Run manually to sync data from settings.xml")]
    public void MigrateAllDataFromSettingsXml()
    {
        var repoRoot = GetRepoRoot();
        var settingsXmlPath = Path.Combine(repoRoot, "docs", "settings.xml");
        var tilesJsonPath = Path.Combine(repoRoot, "src", "TEdit.Terraria", "Data", "tiles.json");
        var wallsJsonPath = Path.Combine(repoRoot, "src", "TEdit.Terraria", "Data", "walls.json");
        var itemsJsonPath = Path.Combine(repoRoot, "src", "TEdit.Terraria", "Data", "items.json");

        Assert.True(File.Exists(settingsXmlPath), $"settings.xml not found at {settingsXmlPath}");

        _output.WriteLine($"Reading {settingsXmlPath}...");
        var xml = XDocument.Load(settingsXmlPath);
        var root = xml.Root!;

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        // ========== TILES ==========
        _output.WriteLine("Processing tiles...");
        var tilesArray = JsonNode.Parse(File.ReadAllText(tilesJsonPath))!.AsArray();
        var tileDict = new Dictionary<int, JsonObject>();
        foreach (var t in tilesArray)
        {
            var obj = t!.AsObject();
            var id = obj["id"]?.GetValue<int>() ?? 0;
            tileDict[id] = obj;
        }

        foreach (var tileXml in root.Elements("Tiles").Elements("Tile"))
        {
            var id = (int?)tileXml.Attribute("Id") ?? 0;
            if (!tileDict.TryGetValue(id, out var tile))
            {
                tile = new JsonObject();
                if (id > 0) tile["id"] = id;
                tileDict[id] = tile;
            }

            // Update from settings.xml
            var name = (string?)tileXml.Attribute("Name");
            if (!string.IsNullOrEmpty(name)) tile["name"] = name;

            var color = (string?)tileXml.Attribute("Color");
            if (!string.IsNullOrEmpty(color)) tile["color"] = ArgbToRgba(color);

            if ((string?)tileXml.Attribute("Solid") == "true") tile["isSolid"] = true;
            if ((string?)tileXml.Attribute("SolidTop") == "true") tile["isSolidTop"] = true;
            if ((string?)tileXml.Attribute("Blends") == "true") tile["canBlend"] = true;
            if ((string?)tileXml.Attribute("Framed") == "true") tile["isFramed"] = true;
            if ((string?)tileXml.Attribute("Stone") == "true") tile["isStone"] = true;
            if ((string?)tileXml.Attribute("Light") == "true") tile["isLight"] = true;
            if ((string?)tileXml.Attribute("IsAnimated") == "true") tile["isAnimated"] = true;

            var mergeWith = (int?)tileXml.Attribute("MergeWith");
            if (mergeWith.HasValue) tile["mergeWith"] = mergeWith.Value;

            var special = (string?)tileXml.Attribute("Special");
            if (!string.IsNullOrEmpty(special)) tile["special"] = special;

            var textureGrid = ParseVector((string?)tileXml.Attribute("TextureGrid"));
            if (textureGrid != null) tile["textureGrid"] = new JsonArray(textureGrid[0], textureGrid[1]);

            var frameGap = ParseVector((string?)tileXml.Attribute("FrameGap"));
            if (frameGap != null) tile["frameGap"] = new JsonArray(frameGap[0], frameGap[1]);

            var placement = (string?)tileXml.Attribute("Placement");
            if (!string.IsNullOrEmpty(placement)) tile["placement"] = placement;

            // Process frames
            var framesXml = tileXml.Element("Frames")?.Elements("Frame").ToList();
            if (framesXml != null && framesXml.Count > 0)
            {
                var framesArray = new JsonArray();
                foreach (var frameXml in framesXml)
                {
                    var frame = new JsonObject();

                    var frameName = (string?)frameXml.Attribute("Name");
                    if (!string.IsNullOrEmpty(frameName)) frame["name"] = frameName;
                    else frame["name"] = name;

                    var variety = (string?)frameXml.Attribute("Variety");
                    if (!string.IsNullOrEmpty(variety)) frame["variety"] = variety;

                    var uv = ParseVector((string?)frameXml.Attribute("UV"));
                    if (uv != null) frame["uv"] = new JsonArray(uv[0], uv[1]);

                    var size = ParseVector((string?)frameXml.Attribute("Size"));
                    if (size != null) frame["size"] = new JsonArray(size[0], size[1]);

                    var anchor = (string?)frameXml.Attribute("Anchor");
                    if (!string.IsNullOrEmpty(anchor)) frame["anchor"] = anchor;

                    framesArray.Add(frame);
                }
                tile["frames"] = framesArray;
            }
        }

        // Write tiles back in order
        var sortedTiles = tileDict.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
        var tilesOutput = new JsonArray();
        foreach (var t in sortedTiles) tilesOutput.Add(JsonNode.Parse(t.ToJsonString()));
        File.WriteAllText(tilesJsonPath, CompactNumericArrays(tilesOutput.ToJsonString(jsonOptions)));
        _output.WriteLine($"Updated {sortedTiles.Count} tiles");

        // ========== WALLS ==========
        _output.WriteLine("Processing walls...");
        var wallsArray = JsonNode.Parse(File.ReadAllText(wallsJsonPath))!.AsArray();
        var wallDict = new Dictionary<int, JsonObject>();
        foreach (var w in wallsArray)
        {
            var obj = w!.AsObject();
            var id = obj["id"]?.GetValue<int>() ?? 0;
            wallDict[id] = obj;
        }

        foreach (var wallXml in root.Elements("Walls").Elements("Wall"))
        {
            var id = (int?)wallXml.Attribute("Id") ?? 0;
            if (!wallDict.TryGetValue(id, out var wall))
            {
                wall = new JsonObject();
                if (id > 0) wall["id"] = id;
                wallDict[id] = wall;
            }

            var name = (string?)wallXml.Attribute("Name");
            if (!string.IsNullOrEmpty(name)) wall["name"] = name;

            var color = (string?)wallXml.Attribute("Color");
            if (!string.IsNullOrEmpty(color)) wall["color"] = ArgbToRgba(color);

            if ((string?)wallXml.Attribute("IsHouse") == "true") wall["isHouse"] = true;
        }

        var sortedWalls = wallDict.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
        var wallsOutput = new JsonArray();
        foreach (var w in sortedWalls) wallsOutput.Add(JsonNode.Parse(w.ToJsonString()));
        File.WriteAllText(wallsJsonPath, CompactNumericArrays(wallsOutput.ToJsonString(jsonOptions)));
        _output.WriteLine($"Updated {sortedWalls.Count} walls");

        // ========== ITEMS ==========
        _output.WriteLine("Processing items...");
        var itemsArray = JsonNode.Parse(File.ReadAllText(itemsJsonPath))!.AsArray();
        var itemDict = new Dictionary<int, JsonObject>();
        foreach (var i in itemsArray)
        {
            var obj = i!.AsObject();
            var id = obj["id"]?.GetValue<int>() ?? 0;
            itemDict[id] = obj;
        }

        foreach (var itemXml in root.Elements("Items").Elements("Item"))
        {
            var id = (int?)itemXml.Attribute("Id") ?? 0;
            if (!itemDict.TryGetValue(id, out var item))
            {
                item = new JsonObject();
                item["id"] = id;
                itemDict[id] = item;
            }

            var name = (string?)itemXml.Attribute("Name");
            if (!string.IsNullOrEmpty(name)) item["name"] = name;

            // Remove and re-add optional properties
            item.Remove("isMount");
            item.Remove("isKite");
            item.Remove("isFood");
            item.Remove("isCritter");
            item.Remove("isAccessory");
            item.Remove("head");
            item.Remove("body");
            item.Remove("legs");
            item.Remove("tally");
            item.Remove("rack");

            if ((string?)itemXml.Attribute("Mount") == "True") item["isMount"] = true;
            if ((string?)itemXml.Attribute("IsKite") == "True") item["isKite"] = true;
            if ((string?)itemXml.Attribute("IsFood") == "True") item["isFood"] = true;
            if ((string?)itemXml.Attribute("IsCritter") == "True") item["isCritter"] = true;
            if ((string?)itemXml.Attribute("Accessory") == "True") item["isAccessory"] = true;

            var head = (int?)itemXml.Attribute("Head");
            if (head.HasValue && head.Value >= 0) item["head"] = head.Value;

            var body = (int?)itemXml.Attribute("Body");
            if (body.HasValue && body.Value >= 0) item["body"] = body.Value;

            var legs = (int?)itemXml.Attribute("Legs");
            if (legs.HasValue && legs.Value >= 0) item["legs"] = legs.Value;

            var tally = (int?)itemXml.Attribute("Tally");
            if (tally.HasValue) item["tally"] = tally.Value;

            var rack = (string?)itemXml.Attribute("Rack");
            if (!string.IsNullOrEmpty(rack)) item["rack"] = rack;
        }

        var sortedItems = itemDict.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
        var itemsOutput = new JsonArray();
        foreach (var i in sortedItems) itemsOutput.Add(JsonNode.Parse(i.ToJsonString()));
        File.WriteAllText(itemsJsonPath, CompactNumericArrays(itemsOutput.ToJsonString(jsonOptions)));
        _output.WriteLine($"Updated {sortedItems.Count} items");

        _output.WriteLine("Migration complete!");
    }

    /// <summary>
    /// Reformats all JSON data files to use compact numeric array format.
    /// Run this test to normalize formatting across all data files.
    /// </summary>
    [Fact]
    public void ReformatAllJsonFiles()
    {
        var repoRoot = GetRepoRoot();
        var dataDir = Path.Combine(repoRoot, "src", "TEdit.Terraria", "Data");
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        var jsonFiles = Directory.GetFiles(dataDir, "*.json");
        _output.WriteLine($"Found {jsonFiles.Length} JSON files in {dataDir}");

        foreach (var filePath in jsonFiles)
        {
            var fileName = Path.GetFileName(filePath);
            var content = File.ReadAllText(filePath);

            // Parse and re-serialize to normalize formatting
            var node = JsonNode.Parse(content);
            var formatted = node!.ToJsonString(jsonOptions);

            // Apply compact numeric array formatting
            var compacted = CompactNumericArrays(formatted);

            // Only write if changed
            if (content != compacted)
            {
                File.WriteAllText(filePath, compacted);
                _output.WriteLine($"Reformatted: {fileName}");
            }
            else
            {
                _output.WriteLine($"Unchanged: {fileName}");
            }
        }

        _output.WriteLine("Reformat complete!");
    }
}
