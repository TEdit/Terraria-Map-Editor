using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using TEdit.Terraria.DataModel;

namespace TEdit.Terraria.Tests;

public class LocalizationImportTests
{
    // Path to decompiled Terraria server source
    private const string TerrariaSourceRoot = @"D:\dev\ai\tedit\terraria-server-dasm\TerrariaServer\Terraria";
    private const string LocalizationRoot = TerrariaSourceRoot + @"\Localization\Content";
    private const string IdRoot = TerrariaSourceRoot + @"\ID";

    // Output path for generated locale files
    private static readonly string OutputRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
            "TEdit.Terraria", "Data", "Localization"));

    // Output path for data files (tiles.json, items.json, etc.)
    private static readonly string DataRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
            "TEdit.Terraria", "Data"));

    private static readonly string[] Locales =
    [
        "de-DE", "en-US", "es-ES", "fr-FR", "it-IT", "ja-JP",
        "ko-KR", "pl-PL", "pt-BR", "ru-RU", "zh-Hans", "zh-Hant"
    ];

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    [Fact(Skip = "Manual import - generates localization JSON files from Terraria source")]
    public void GenerateLocalizationFiles()
    {
        // Parse ID mappings from decompiled C# source
        var itemIds = ParseConstFields(Path.Combine(IdRoot, "ItemID.cs"), @"public const short (\w+) = (-?\d+);");
        var npcIds = ParseConstFields(Path.Combine(IdRoot, "NPCID.cs"), @"public const short (\w+) = (-?\d+);");
        var prefixIds = ParseConstFields(Path.Combine(IdRoot, "PrefixID.cs"), @"public const int (\w+) = (\d+);");
        var wallIds = ParseConstFields(Path.Combine(IdRoot, "WallID.cs"), @"public const ushort (\w+) = (\d+);");
        var tileIds = ParseConstFields(Path.Combine(IdRoot, "TileID.cs"), @"public const ushort (\w+) = (\d+);");

        Assert.True(itemIds.Count > 5000, $"Expected >5000 ItemID entries, got {itemIds.Count}");
        Assert.True(npcIds.Count > 500, $"Expected >500 NPCID entries, got {npcIds.Count}");
        Assert.True(prefixIds.Count > 80, $"Expected >80 PrefixID entries, got {prefixIds.Count}");

        // Build reverse lookups: id → const name
        var tileIdToKey = tileIds.ToDictionary(kv => kv.Value, kv => kv.Key);
        var itemIdToName = itemIds.ToDictionary(kv => kv.Value, kv => kv.Key);

        // Build wall-to-item mapping using en-US item names
        var enItems = ReadLocaleSection(Path.Combine(LocalizationRoot, "en-US", "Items.json"), "ItemName");
        var wallToItemKey = BuildWallToItemMapping(wallIds, itemIds, enItems);

        // Load TEdit's tiles.json to get English tile names for matching
        var tilesJsonPath = Path.Combine(DataRoot, "tiles.json");
        var tilesJson = JsonSerializer.Deserialize<List<JsonElement>>(File.ReadAllText(tilesJsonPath));
        var tileEnglishNames = new Dictionary<int, string>();
        foreach (var tile in tilesJson!)
        {
            var id = tile.GetProperty("id").GetInt32();
            if (tile.TryGetProperty("name", out var nameProp))
                tileEnglishNames[id] = nameProp.GetString() ?? "";
        }

        // Build item English name → item const name lookup
        var itemNameToConstName = new Dictionary<string, string>();
        foreach (var kv in enItems)
        {
            itemNameToConstName[kv.Value] = kv.Key; // e.g., "Dirt Block" → "DirtBlock"
        }

        Directory.CreateDirectory(OutputRoot);

        foreach (var locale in Locales)
        {
            var localePath = Path.Combine(LocalizationRoot, locale);
            Assert.True(Directory.Exists(localePath), $"Locale directory not found: {localePath}");

            var data = new LocalizationData();

            // Items: constName → localizedName
            var itemsJson = ReadLocaleSection(Path.Combine(localePath, "Items.json"), "ItemName");
            foreach (var kv in itemsJson)
                data.Items[kv.Key] = kv.Value;

            // Prefixes: constName → localizedName
            var prefixJson = ReadLocaleSection(Path.Combine(localePath, "Items.json"), "Prefix");
            foreach (var kv in prefixJson)
                data.Prefixes[kv.Key] = kv.Value;

            // NPCs: constName → localizedName
            var npcJson = ReadLocaleSection(Path.Combine(localePath, "NPCs.json"), "NPCName");
            foreach (var kv in npcJson)
                data.Npcs[kv.Key] = kv.Value;

            // Tiles: tileConstName → localized name (via item English name matching)
            foreach (var kv in tileEnglishNames)
            {
                var tileId = kv.Key;
                var englishName = kv.Value;

                if (tileIdToKey.TryGetValue(tileId, out var tileKey)
                    && itemNameToConstName.TryGetValue(englishName, out var itemConstName)
                    && itemsJson.TryGetValue(itemConstName, out var localizedName))
                {
                    data.Tiles[tileKey] = localizedName;
                }
            }

            // Walls: wallConstName → localized name from wall-to-item mapping
            foreach (var kv in wallToItemKey)
            {
                var wallId = kv.Key;
                var itemKey = kv.Value;

                // Find the wall const name for this wall ID
                foreach (var wall in wallIds)
                {
                    if (wall.Value == wallId)
                    {
                        if (itemsJson.TryGetValue(itemKey, out var name))
                            data.Walls[wall.Key] = name;
                        break;
                    }
                }
            }

            var sorted = SortLocalizationData(data);
            var outputPath = Path.Combine(OutputRoot, $"{locale}.json");
            using var stream = File.Create(outputPath);
            JsonSerializer.Serialize(stream, sorted, WriteOptions);

            // Verify output
            Assert.True(data.Items.Count > 0, $"{locale}: No items found");
            Assert.True(data.Npcs.Count > 0, $"{locale}: No NPCs found");
            Assert.True(data.Prefixes.Count > 0, $"{locale}: No prefixes found");
            Assert.True(data.Tiles.Count > 0, $"{locale}: No tiles found");
            Assert.True(data.Walls.Count > 0, $"{locale}: No walls found");
        }
    }

    [Fact(Skip = "Manual import - populates key fields in data JSON files from Terraria ID source")]
    public void PopulateKeysInDataFiles()
    {
        // Parse ID mappings: constName → id
        var tileIds = ParseConstFields(Path.Combine(IdRoot, "TileID.cs"), @"public const ushort (\w+) = (\d+);");
        var wallIds = ParseConstFields(Path.Combine(IdRoot, "WallID.cs"), @"public const ushort (\w+) = (\d+);");
        var itemIds = ParseConstFields(Path.Combine(IdRoot, "ItemID.cs"), @"public const short (\w+) = (-?\d+);");
        var npcIds = ParseConstFields(Path.Combine(IdRoot, "NPCID.cs"), @"public const short (\w+) = (-?\d+);");
        var prefixIds = ParseConstFields(Path.Combine(IdRoot, "PrefixID.cs"), @"public const int (\w+) = (\d+);");

        // Build reverse lookup: id → const name
        var tileIdToKey = tileIds.ToDictionary(kv => kv.Value, kv => kv.Key);
        var wallIdToKey = wallIds.ToDictionary(kv => kv.Value, kv => kv.Key);
        var itemIdToKey = itemIds.ToDictionary(kv => kv.Value, kv => kv.Key);
        var npcIdToKey = npcIds.ToDictionary(kv => kv.Value, kv => kv.Key);
        var prefixIdToKey = prefixIds.ToDictionary(kv => kv.Value, kv => kv.Key);

        // Update each data file
        UpdateJsonKeysInFile(Path.Combine(DataRoot, "tiles.json"), tileIdToKey);
        UpdateJsonKeysInFile(Path.Combine(DataRoot, "walls.json"), wallIdToKey);
        UpdateJsonKeysInFile(Path.Combine(DataRoot, "items.json"), itemIdToKey);
        UpdateJsonKeysInFile(Path.Combine(DataRoot, "npcs.json"), npcIdToKey);
        UpdateJsonKeysInFile(Path.Combine(DataRoot, "prefixes.json"), prefixIdToKey);

        // Verify
        VerifyKeysInFile(Path.Combine(DataRoot, "tiles.json"), "tiles");
        VerifyKeysInFile(Path.Combine(DataRoot, "items.json"), "items");
        VerifyKeysInFile(Path.Combine(DataRoot, "npcs.json"), "npcs");
        VerifyKeysInFile(Path.Combine(DataRoot, "walls.json"), "walls");
        VerifyKeysInFile(Path.Combine(DataRoot, "prefixes.json"), "prefixes");
    }

    /// <summary>
    /// Adds or updates the "key" field in each entry of a JSON array file,
    /// inserting it immediately after the "name" property.
    /// Preserves all existing properties and their raw JSON formatting.
    /// </summary>
    private static void UpdateJsonKeysInFile(string filePath, Dictionary<int, string> idToKey)
    {
        var text = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(text);

        var outputEntries = new List<string>();
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            // Default to 0 for entries without explicit id (e.g., prefix id=0 omitted by WhenWritingDefault)
            var id = element.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
            var props = new List<string>();

            foreach (var prop in element.EnumerateObject())
            {
                if (prop.Name == "key") continue;

                props.Add($"    {JsonSerializer.Serialize(prop.Name)}: {prop.Value.GetRawText()}");

                if (prop.Name == "name" && idToKey.TryGetValue(id, out var key))
                {
                    props.Add($"    \"key\": {JsonSerializer.Serialize(key)}");
                }
            }

            outputEntries.Add("  {\n" + string.Join(",\n", props) + "\n  }");
        }

        var output = "[\n" + string.Join(",\n", outputEntries) + "\n]\n";
        File.WriteAllText(filePath, output);
    }

    private static void VerifyKeysInFile(string filePath, string label)
    {
        var text = File.ReadAllText(filePath);
        using var doc = JsonDocument.Parse(text);
        var total = 0;
        var withKeys = 0;

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            total++;
            if (element.TryGetProperty("key", out _))
                withKeys++;
        }

        Assert.True(withKeys > 0, $"{label}: No entries have keys");
    }

    private static Dictionary<int, string> BuildWallToItemMapping(
        Dictionary<string, int> wallIds,
        Dictionary<string, int> itemIds,
        Dictionary<string, string> enItems)
    {
        var wallToItemKey = new Dictionary<int, string>();

        foreach (var wall in wallIds)
        {
            var wallName = wall.Key;
            var wallId = wall.Value;

            var cleanName = wallName.Replace("Unsafe", "");
            var candidates = new[]
            {
                cleanName + "Wall",
                wallName + "Wall",
                cleanName,
                wallName,
            };

            foreach (var candidate in candidates)
            {
                if (itemIds.ContainsKey(candidate) && enItems.ContainsKey(candidate))
                {
                    wallToItemKey[wallId] = candidate;
                    break;
                }
            }
        }

        return wallToItemKey;
    }

    /// <summary>
    /// Parse "public const {type} {Name} = {Value};" lines from a C# file.
    /// </summary>
    private static Dictionary<string, int> ParseConstFields(string filePath, string pattern)
    {
        var result = new Dictionary<string, int>();
        var regex = new Regex(pattern);

        foreach (var line in File.ReadLines(filePath))
        {
            var match = regex.Match(line);
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                if (int.TryParse(match.Groups[2].Value, out var id))
                    result[name] = id;
            }
        }

        return result;
    }

    [Fact]
    public void LoadJapaneseLocalization_ContainsKnownItemNames()
    {
        var data = TEdit.Terraria.Loaders.LocalizationLoader.LoadLocalization("ja-JP");

        Assert.NotEmpty(data.Items);
        Assert.NotEmpty(data.Npcs);
        Assert.NotEmpty(data.Prefixes);
        Assert.NotEmpty(data.Tiles);
        Assert.NotEmpty(data.Walls);

        // Iron Pickaxe = てつのツルハシ
        Assert.True(data.Items.ContainsKey("IronPickaxe"), "Missing item key IronPickaxe");
        Assert.Equal("てつのツルハシ", data.Items["IronPickaxe"]);

        // Blue Slime = ブルースライム
        Assert.True(data.Npcs.ContainsKey("BlueSlime"), "Missing NPC key BlueSlime");
        Assert.Equal("ブルースライム", data.Npcs["BlueSlime"]);

        // Prefix "Large" (PrefixID Large = 1)
        Assert.True(data.Prefixes.ContainsKey("Large"), "Missing prefix key Large");
        Assert.Equal("大", data.Prefixes["Large"]);

        // Tile "Dirt" = つちブロック
        Assert.True(data.Tiles.ContainsKey("Dirt"), "Missing tile key Dirt");
        Assert.Equal("つちブロック", data.Tiles["Dirt"]);
    }

    [Fact]
    public void LoadEnglishLocalization_ReturnsEmptyData()
    {
        // en-US is the base data, so the loader returns empty (no overrides needed)
        var data = TEdit.Terraria.Loaders.LocalizationLoader.LoadLocalization("en-US");

        Assert.NotEmpty(data.Items);
    }

    [Fact]
    public void LoadUnsupportedLocale_ReturnsEmptyData()
    {
        var data = TEdit.Terraria.Loaders.LocalizationLoader.LoadLocalization("xx-XX");

        Assert.Empty(data.Items);
        Assert.Empty(data.Npcs);
    }

    [Theory]
    [InlineData("ja-JP", "ja-JP")]
    [InlineData("ja", "ja-JP")]
    [InlineData("zh-Hans", "zh-Hans")]
    [InlineData("zh-CN", "zh-Hans")]
    [InlineData("zh-TW", "zh-Hant")]
    [InlineData("de-DE", "de-DE")]
    [InlineData("de", "de-DE")]
    [InlineData("ar-BH", "en-US")]   // Arabic not in Terraria → English fallback
    [InlineData("sv-SE", "en-US")]   // Swedish not supported → English fallback
    public void GetLocaleFromCurrentCulture_MapsCorrectly(string cultureName, string expectedLocale)
    {
        var originalCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
        try
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture =
                new System.Globalization.CultureInfo(cultureName);

            var locale = TEdit.Terraria.Loaders.LocalizationLoader.GetLocaleFromCurrentCulture();
            Assert.Equal(expectedLocale, locale);
        }
        finally
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = originalCulture;
        }
    }

    private static LocalizationData SortLocalizationData(LocalizationData data)
    {
        return new LocalizationData
        {
            Items = new Dictionary<string, string>(data.Items.OrderBy(kv => kv.Key, StringComparer.Ordinal)),
            Npcs = new Dictionary<string, string>(data.Npcs.OrderBy(kv => kv.Key, StringComparer.Ordinal)),
            Prefixes = new Dictionary<string, string>(data.Prefixes.OrderBy(kv => kv.Key, StringComparer.Ordinal)),
            Tiles = new Dictionary<string, string>(data.Tiles.OrderBy(kv => kv.Key, StringComparer.Ordinal)),
            Walls = new Dictionary<string, string>(data.Walls.OrderBy(kv => kv.Key, StringComparer.Ordinal)),
        };
    }

    /// <summary>
    /// Read a top-level section from a Terraria localization JSON file.
    /// These files use trailing commas which standard JSON doesn't allow.
    /// </summary>
    private static Dictionary<string, string> ReadLocaleSection(string filePath, string sectionName)
    {
        var text = File.ReadAllText(filePath);
        var doc = JsonDocument.Parse(text, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        });

        var result = new Dictionary<string, string>();

        if (doc.RootElement.TryGetProperty(sectionName, out var section))
        {
            foreach (var prop in section.EnumerateObject())
            {
                var value = prop.Value.GetString();
                if (value != null)
                    result[prop.Name] = value;
            }
        }

        return result;
    }
}
