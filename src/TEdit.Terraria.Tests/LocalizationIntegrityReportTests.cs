using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TEdit.Terraria.DataModel;
using TEdit.Terraria.Loaders;
using TEdit.Terraria.Objects;
using Xunit.Abstractions;

namespace TEdit.Terraria.Tests;

/// <summary>
/// Audits every supported locale against the game data and shared name keys.
/// Empty translations are intentional source-name fallbacks; missing keys,
/// invalid references, duplicate keys and invalid frame mappings are errors.
/// Language-specific grammar metadata is preserved without requiring other
/// languages to declare the same grammatical forms.
/// </summary>
public sealed class LocalizationIntegrityReportTests
{
    private static readonly Regex ReferencePattern = new(@"\{\$([^.}]+)\.([^}]+)\}", RegexOptions.Compiled);
    private readonly ITestOutputHelper _output;

    public LocalizationIntegrityReportTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SupportedLocales_CoverGameDataAndSharedNames_WithValidReferencesAndFrameMappings()
    {
        var tiles = JsonDataLoader.LoadListFromResource<TileProperty>("tiles.json");
        var walls = JsonDataLoader.LoadListFromResource<WallProperty>("walls.json");
        var items = JsonDataLoader.LoadListFromResource<ItemProperty>("items.json");
        var npcs = JsonDataLoader.LoadListFromResource<NpcData>("npcs.json");
        var bestiaryNpcs = JsonDataLoader.LoadFromResource<BestiaryNpcConfiguration>("bestiaryNpcs.json");
        var prefixes = JsonDataLoader.LoadListFromResource<PrefixData>("prefixes.json");
        var resources = LocalizationLoader.GetSupportedLocales()
            .ToDictionary(locale => locale, LoadRawLocalization, StringComparer.Ordinal);
        var sourceEntries = new Dictionary<string, IEnumerable<(string? Key, string Name)>>
        {
            ["items"] = items.Select(x => (x.Key, x.Name)),
            ["tiles"] = tiles.Select(x => (x.Key, x.Name)).Append(((string?)"Air", "Air")),
            ["walls"] = walls.Select(x => (x.Key, x.Name)),
            ["npcs"] = npcs.Select(x => (x.Key, x.Name))
                .Concat(bestiaryNpcs.NpcData.Select(x => ((string?)x.BestiaryId, x.FullName))),
            ["prefixes"] = prefixes.Select(x => (x.Key, x.Name)),
            ["liquids"] = new[] { ((string?)"Water", "Water"), ("Lava", "Lava"), ("Honey", "Honey"), ("Shimmer", "Shimmer") },
            ["wires"] = new[] { ((string?)"Red Wire", "Red Wire"), ("Blue Wire", "Blue Wire"), ("Green Wire", "Green Wire"), ("Yellow Wire", "Yellow Wire") }
        };
        var expectedKeys = sourceEntries.ToDictionary(
            section => section.Key,
            section => section.Value.Select(entry => entry.Key)
                .Concat(resources.Values.SelectMany(data => GetNameSections(data)[section.Key].Keys))
                .Where(key => !string.IsNullOrWhiteSpace(key) && !key.Contains('$'))
                .Select(key => key!)
                .ToHashSet(StringComparer.Ordinal));
        const int expectedBuffKeyCount = 340;
        var expectedBuffKeys = resources["en-US"].Buffs.Keys.ToHashSet(StringComparer.Ordinal);
        var knownFrameKeys = tiles
            .Where(tile => !string.IsNullOrWhiteSpace(tile.Key) && tile.Frames != null)
            .SelectMany(tile => tile.Frames!.Select(frame => (TileKey: tile.Key!, FrameKey: frame.ToString())))
            .ToHashSet();
        var errors = new List<string>();
        if (expectedBuffKeys.Count != expectedBuffKeyCount)
            errors.Add($"en-US: expected {expectedBuffKeyCount} buff keys, found {expectedBuffKeys.Count}");
        var report = new StringBuilder("# Localization integrity report: all supported locales\n");

        foreach (var (locale, raw) in resources)
        {
            var localization = LocalizationLoader.LoadLocalization(locale);
            var rawSections = GetNameSections(raw);
            var resolvedSections = GetNameSections(localization);
            report.AppendLine();
            report.AppendLine($"## {locale}");
            report.AppendLine("### Name coverage");
            foreach (var (section, translations) in resolvedSections)
            {
                var missing = expectedKeys[section].Except(rawSections[section].Keys, StringComparer.Ordinal).ToList();
                errors.AddRange(missing.Select(key => $"{locale}: missing {section}.{key}"));
                AppendCoverage(report, section, sourceEntries[section], translations);
                var metadataCount = translations.Keys.Count(key => key.Contains('$'));
                var sourceKeys = sourceEntries[section].Select(entry => entry.Key).ToHashSet(StringComparer.Ordinal);
                var extraNames = translations.Keys.Where(key => !key.Contains('$') && !sourceKeys.Contains(key)).ToList();
                report.AppendLine($"  - Shared keys missing: {missing.Count}; grammar metadata: {metadataCount}; names without data records: {extraNames.Count}");
            }
            errors.AddRange(expectedBuffKeys.Except(raw.Buffs.Keys, StringComparer.Ordinal)
                .Select(key => $"{locale}: missing buffs.{key}"));
            errors.AddRange(raw.Buffs.Keys.Except(expectedBuffKeys, StringComparer.Ordinal)
                .Select(key => $"{locale}: unexpected buffs.{key}"));
            report.AppendLine($"- Buff keys: {raw.Buffs.Count}/{expectedBuffKeys.Count}");

            var emptyValues = GetEmptyValues(raw);
            report.AppendLine($"- Empty localized values: {emptyValues.Count}");

            var invalidReferences = GetLocalizedValues(raw)
                .SelectMany(value => ReferencePattern.Matches(value).Cast<Match>())
                .Where(match => !HasReferenceTarget(raw, match.Groups[1].Value, match.Groups[2].Value))
                .Select(match => match.Value)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var unresolvedReferences = GetLocalizedValues(localization)
                .SelectMany(value => ReferencePattern.Matches(value).Select(match => match.Value))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var cyclicReferences = FindCircularReferences(raw);
            errors.AddRange(invalidReferences.Select(token => $"{locale}: invalid reference {token}"));
            errors.AddRange(unresolvedReferences.Select(token => $"{locale}: unresolved reference {token}"));
            errors.AddRange(cyclicReferences.Select(cycle => $"{locale}: cyclic reference {cycle}"));
            report.AppendLine("### References");
            report.AppendLine($"- Invalid targets: {invalidReferences.Count}; unresolved tokens after loading: {unresolvedReferences.Count}; cycles: {cyclicReferences.Count}");
            AppendExamples(report, invalidReferences.Concat(unresolvedReferences).Concat(cyclicReferences));

            var frameKeys = raw.Frames.SelectMany(group => group.Value.Keys
                .Select(key => (TileKey: group.Key, FrameKey: key))).ToHashSet();
            var missingFrames = knownFrameKeys.Except(frameKeys).ToList();
            var invalidFrames = frameKeys.Except(knownFrameKeys).ToList();
            errors.AddRange(missingFrames.Select(frame => $"{locale}: missing frames.{frame.TileKey}.{frame.FrameKey}"));
            errors.AddRange(invalidFrames.Select(frame => $"{locale}: invalid frames.{frame.TileKey}.{frame.FrameKey}"));
            var translatedItemNames = items
                .Where(item => item.Key != null
                    && localization.Items.TryGetValue(item.Key, out var value)
                    && !string.IsNullOrWhiteSpace(value))
                .GroupBy(item => item.Name, StringComparer.Ordinal)
                .Where(group => group.Select(item => localization.Items[item.Key!]).Distinct(StringComparer.Ordinal).Count() == 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.Ordinal);
            var explicitTranslations = localization.Frames.Sum(group => group.Value.Count(pair => !string.IsNullOrWhiteSpace(pair.Value)));
            var localizedFrames = knownFrameKeys.Count(frame =>
                (localization.Frames.TryGetValue(frame.TileKey, out var names)
                    && names.TryGetValue(frame.FrameKey, out var value)
                    && !string.IsNullOrWhiteSpace(value))
                || translatedItemNames.Contains(frame.FrameKey));
            report.AppendLine("### Complete frame labels");
            report.AppendLine($"- Source labels: {knownFrameKeys.Count}; resource keys: {frameKeys.Count}; missing: {missingFrames.Count}; invalid: {invalidFrames.Count}");
            report.AppendLine($"- Explicit translations: {explicitTranslations}; including exact item-name fallback: {localizedFrames}; whole-label source fallback: {knownFrameKeys.Count - localizedFrames}");
            report.AppendLine($"- Empty explicit frame slots: {localization.Frames.Sum(group => group.Value.Count(pair => string.IsNullOrWhiteSpace(pair.Value)))}");
        }

        _output.WriteLine(report.ToString());
        Assert.Empty(errors);
    }

    [Fact]
    public void ReferenceAudit_DetectsCyclesBetweenValidTargets()
    {
        var data = new LocalizationData
        {
            Items = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["CycleA"] = "{$ItemName.CycleB}",
                ["CycleB"] = "{$ItemName.CycleA}"
            }
        };

        var cycles = FindCircularReferences(data);

        Assert.Contains(cycles, cycle =>
            cycle.Contains("ItemName.CycleA", StringComparison.Ordinal)
            && cycle.Contains("ItemName.CycleB", StringComparison.Ordinal));
    }

    [Fact]
    public void EmbeddedJson_HasNoDuplicateProperties()
    {
        var names = new[] { "items.json", "tiles.json", "walls.json", "npcs.json", "prefixes.json", "bestiaryNpcs.json" }
            .Concat(LocalizationLoader.GetSupportedLocales().Select(locale => $"Localization/{locale}.json"));
        var duplicates = new List<string>();
        foreach (var name in names)
        {
            using var stream = JsonDataLoader.GetDataStream(name);
            using var document = JsonDocument.Parse(stream);
            FindDuplicateProperties(document.RootElement, name, duplicates);
        }

        Assert.Empty(duplicates);
    }

    [Fact]
    public void GameData_HasUniqueIdsAndKeys()
    {
        var issues = new List<string>();
        AddDuplicateIssues("Items", JsonDataLoader.LoadListFromResource<ItemProperty>("items.json").Select(x => (x.Id, x.Key)), issues);
        AddDuplicateIssues("Tiles", JsonDataLoader.LoadListFromResource<TileProperty>("tiles.json").Select(x => (x.Id, x.Key)), issues);
        AddDuplicateIssues("Walls", JsonDataLoader.LoadListFromResource<WallProperty>("walls.json").Select(x => (x.Id, x.Key)), issues);
        AddDuplicateIssues("NPCs", JsonDataLoader.LoadListFromResource<NpcData>("npcs.json").Select(x => (x.Id, x.Key)), issues);
        AddDuplicateIssues("Prefixes", JsonDataLoader.LoadListFromResource<PrefixData>("prefixes.json").Select(x => (x.Id, x.Key)), issues);
        Assert.Empty(issues);
    }

    private static LocalizationData LoadRawLocalization(string locale)
    {
        using var stream = JsonDataLoader.GetDataStream($"Localization/{locale}.json");
        return JsonSerializer.Deserialize<LocalizationData>(stream)!;
    }

    private static Dictionary<string, Dictionary<string, string>> GetNameSections(LocalizationData data) => new()
    {
        ["items"] = data.Items, ["npcs"] = data.Npcs, ["prefixes"] = data.Prefixes,
        ["tiles"] = data.Tiles, ["walls"] = data.Walls, ["liquids"] = data.Liquids, ["wires"] = data.Wires
    };

    private static bool HasReferenceTarget(LocalizationData data, string category, string key) => category switch
    {
        "ItemName" => data.Items.ContainsKey(key),
        "NPCName" => data.Npcs.ContainsKey(key),
        "BuffName" or "BuffDescription" => data.Buffs.ContainsKey(key),
        "Prefix" or "PrefixName" => data.Prefixes.ContainsKey(key),
        "TileName" => data.Tiles.ContainsKey(key),
        "WallName" => data.Walls.ContainsKey(key),
        "LiquidName" => data.Liquids.ContainsKey(key),
        "WireName" => data.Wires.ContainsKey(key),
        _ => false
    };

    private static List<string> FindCircularReferences(LocalizationData data)
    {
        var graph = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        void AddNode(string node, string value)
        {
            graph[node] = ReferencePattern.Matches(value)
                .Cast<Match>()
                .Select(match => GetReferenceNode(match.Groups[1].Value, match.Groups[2].Value))
                .Where(target => target != null)
                .Select(target => target!)
                .ToList();
        }

        void AddSection(string category, Dictionary<string, string> values)
        {
            foreach (var (key, value) in values)
                AddNode($"{category}.{key}", value);
        }

        AddSection("ItemName", data.Items);
        AddSection("NPCName", data.Npcs);
        AddSection("PrefixName", data.Prefixes);
        AddSection("TileName", data.Tiles);
        AddSection("WallName", data.Walls);
        AddSection("LiquidName", data.Liquids);
        AddSection("WireName", data.Wires);
        foreach (var (key, buff) in data.Buffs)
        {
            AddNode($"BuffName.{key}", buff.Name);
            AddNode($"BuffDescription.{key}", buff.Description);
        }
        foreach (var (tileKey, frameNames) in data.Frames)
            foreach (var (frameKey, value) in frameNames)
                AddNode($"FrameName.{tileKey}.{frameKey}", value);

        var states = new Dictionary<string, byte>(StringComparer.Ordinal);
        var path = new List<string>();
        var pathIndexes = new Dictionary<string, int>(StringComparer.Ordinal);
        var cycles = new HashSet<string>(StringComparer.Ordinal);

        foreach (var start in graph.Keys)
        {
            if (states.ContainsKey(start))
                continue;

            states[start] = 1;
            pathIndexes[start] = 0;
            path.Add(start);
            var pending = new Stack<(string Node, int NextTarget)>();
            pending.Push((start, 0));

            while (pending.TryPop(out var current))
            {
                var targets = graph[current.Node];
                if (current.NextTarget >= targets.Count)
                {
                    states[current.Node] = 2;
                    pathIndexes.Remove(current.Node);
                    path.RemoveAt(path.Count - 1);
                    continue;
                }

                pending.Push((current.Node, current.NextTarget + 1));
                var target = targets[current.NextTarget];
                if (!graph.ContainsKey(target))
                    continue;

                if (states.TryGetValue(target, out var state))
                {
                    if (state == 1 && pathIndexes.TryGetValue(target, out var cycleStart))
                        cycles.Add(string.Join(" -> ", path.Skip(cycleStart).Append(target)));
                    continue;
                }

                states[target] = 1;
                pathIndexes[target] = path.Count;
                path.Add(target);
                pending.Push((target, 0));
            }
        }

        return cycles.ToList();
    }

    private static string? GetReferenceNode(string category, string key) => category switch
    {
        "ItemName" or "NPCName" or "TileName" or "WallName" or "LiquidName" or "WireName"
            => $"{category}.{key}",
        "BuffName" or "BuffDescription" => $"{category}.{key}",
        "Prefix" or "PrefixName" => $"PrefixName.{key}",
        _ => null
    };

    private static void AppendCoverage(
        StringBuilder report,
        string category,
        IEnumerable<(string? Key, string Name)> entries,
        IReadOnlyDictionary<string, string> translations)
    {
        var keyed = entries.Where(x => !string.IsNullOrWhiteSpace(x.Key)).DistinctBy(x => x.Key).ToList();
        var missing = keyed.Where(x => !translations.ContainsKey(x.Key!)).ToList();
        var empty = keyed.Where(x => translations.TryGetValue(x.Key!, out var value)
            && string.IsNullOrWhiteSpace(value)).ToList();
        var sourceIdentical = keyed.Where(x => translations.TryGetValue(x.Key!, out var value)
            && string.Equals(value.Trim(), x.Name.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        report.AppendLine($"- {category}: {keyed.Count - missing.Count - empty.Count}/{keyed.Count}; "
            + $"missing {missing.Count}, empty {empty.Count}, source-identical {sourceIdentical.Count}");
        AppendExamples(report, missing.Select(x => $"missing {x.Key} ({x.Name})"));
        AppendExamples(report, empty.Select(x => $"empty {x.Key} ({x.Name})"));
        AppendExamples(report, sourceIdentical.Select(x => $"source-identical {x.Key} ({x.Name})"));
    }

    private static void AppendExamples(StringBuilder report, IEnumerable<string> examples)
    {
        var sample = examples.Take(8).ToList();
        if (sample.Count > 0)
            report.AppendLine($"  - Examples: {string.Join(", ", sample)}");
    }

    private static void AddDuplicateIssues(
        string category,
        IEnumerable<(int Id, string? Key)> values,
        List<string> issues)
    {
        var entries = values.ToList();
        foreach (var duplicate in entries.GroupBy(x => x.Id).Where(group => group.Count() > 1))
            issues.Add($"{category} duplicate ID {duplicate.Key}");
        foreach (var duplicate in entries.Where(x => !string.IsNullOrWhiteSpace(x.Key))
                     .GroupBy(x => x.Key!, StringComparer.Ordinal).Where(group => group.Count() > 1))
            issues.Add($"{category} duplicate key {duplicate.Key}");
    }

    private static HashSet<string> GetEmptyValues(LocalizationData data)
    {
        var empty = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (sectionName, section) in GetNameSections(data))
            foreach (var (key, value) in section)
                if (string.IsNullOrWhiteSpace(value))
                    empty.Add($"{sectionName}.{key}");

        foreach (var (key, buff) in data.Buffs)
        {
            if (string.IsNullOrWhiteSpace(buff.Name))
                empty.Add($"buffs.{key}.name");
            if (string.IsNullOrWhiteSpace(buff.Description))
                empty.Add($"buffs.{key}.description");
        }

        foreach (var (tileKey, frames) in data.Frames)
            foreach (var (frameKey, value) in frames)
                if (string.IsNullOrWhiteSpace(value))
                    empty.Add($"frames.{tileKey}.{frameKey}");

        return empty;
    }

    private static IEnumerable<string> GetLocalizedValues(LocalizationData data)
    {
        foreach (var section in GetNameSections(data).Values)
            foreach (var value in section.Values)
                yield return value;
        foreach (var buff in data.Buffs.Values)
        {
            yield return buff.Name;
            yield return buff.Description;
        }
        foreach (var frameNames in data.Frames.Values)
            foreach (var value in frameNames.Values)
                yield return value;
    }

    private static void FindDuplicateProperties(JsonElement element, string path, List<string> duplicates)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                var childPath = $"{path}.{property.Name}";
                if (!seen.Add(property.Name))
                    duplicates.Add(childPath);
                FindDuplicateProperties(property.Value, childPath, duplicates);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
                FindDuplicateProperties(item, $"{path}[{index++}]", duplicates);
        }
    }
}
