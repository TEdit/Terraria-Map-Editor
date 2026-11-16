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
/// The official <c>{locale}.json</c> resources must contain only names derived
/// from the game's localization files. Hand-verified translations live in
/// <c>LocalizationOverrides.{locale}.json</c>; supplemental sprite/style names
/// live in <c>supplementalNames.{locale}.json</c>.
///
/// Empty official values are intentional source-name fallbacks. Missing keys,
/// invalid references, duplicate properties, invalid override keys and invalid
/// frame mappings are errors. Missing override tables are normal.
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

        var locales = LocalizationLoader.GetSupportedLocales();
        var resources = locales.ToDictionary(locale => locale, LoadRawOfficial, StringComparer.Ordinal);
        var overrides = locales.ToDictionary(locale => locale, LoadRawOverrides, StringComparer.Ordinal);
        var localized = locales.ToDictionary(
            locale => locale,
            locale => LocalizationLoader.LoadLocalization(locale),
            StringComparer.Ordinal);

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
            var overrideData = overrides[locale];
            var localization = localized[locale];
            var combinedRaw = CreateCombinedRaw(raw, overrideData);
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
            report.AppendLine($"- Empty official localized values: {emptyValues.Count}");
            AppendOverrideSummary(report, locale, raw, overrideData, expectedKeys, knownFrameKeys, errors);

            var invalidReferences = GetLocalizedValues(raw)
                .Concat(overrideData == null ? [] : GetLocalizedValues(overrideData))
                .SelectMany(value => ReferencePattern.Matches(value).Cast<Match>())
                .Where(match => !HasReferenceTarget(combinedRaw, match.Groups[1].Value, match.Groups[2].Value))
                .Select(match => match.Value)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var unresolvedReferences = GetLocalizedValues(localization)
                .SelectMany(value => ReferencePattern.Matches(value).Select(match => match.Value))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var cyclicReferences = FindCircularReferences(combinedRaw);
            errors.AddRange(invalidReferences.Select(token => $"{locale}: invalid reference {token}"));
            errors.AddRange(unresolvedReferences.Select(token => $"{locale}: unresolved reference {token}"));
            errors.AddRange(cyclicReferences.Select(cycle => $"{locale}: cyclic reference {cycle}"));
            report.AppendLine("### References");
            report.AppendLine($"- Invalid targets: {invalidReferences.Count}; unresolved tokens after loading: {unresolvedReferences.Count}; cycles: {cyclicReferences.Count}");
            AppendExamples(report, invalidReferences.Concat(unresolvedReferences).Concat(cyclicReferences));

            var overrideFrameKeys = overrideData?.Frames?
                .SelectMany(group => group.Value.Keys.Select(key => (TileKey: group.Key, FrameKey: key)))
                .ToHashSet() ?? [];
            var invalidFrames = overrideFrameKeys.Except(knownFrameKeys).ToList();
            errors.AddRange(invalidFrames.Select(frame => $"{locale}: invalid override frame {frame.TileKey}.{frame.FrameKey}"));
            var translatedItemNames = items
                .Where(item => item.Key != null
                    && localization.Items.TryGetValue(item.Key, out var value)
                    && !string.IsNullOrWhiteSpace(value))
                .GroupBy(item => item.Name, StringComparer.Ordinal)
                .Where(group => group.Select(item => localization.Items[item.Key!]).Distinct(StringComparer.Ordinal).Count() == 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.Ordinal);
            var explicitTranslations = overrideFrameKeys.Count;
            var localizedFrames = knownFrameKeys.Count(frame =>
                (localization.Frames != null
                    && localization.Frames.TryGetValue(frame.TileKey, out var names)
                    && names.TryGetValue(frame.FrameKey, out var value)
                    && !string.IsNullOrWhiteSpace(value))
                || translatedItemNames.Contains(frame.FrameKey));
            report.AppendLine("### Complete frame labels");
            report.AppendLine($"- Source labels: {knownFrameKeys.Count}; override frame keys: {explicitTranslations}; invalid: {invalidFrames.Count}");
            report.AppendLine($"- Explicit translations: {explicitTranslations}; including exact item-name fallback: {localizedFrames}; whole-label source fallback: {knownFrameKeys.Count - localizedFrames}");
        }

        _output.WriteLine(report.ToString());
        Assert.Empty(errors);
    }

    [Fact]
    public void OfficialLocalizationResources_ContainOnlyGeneratedTranslations()
    {
        var tiles = JsonDataLoader.LoadListFromResource<TileProperty>("tiles.json");
        var walls = JsonDataLoader.LoadListFromResource<WallProperty>("walls.json");
        var items = JsonDataLoader.LoadListFromResource<ItemProperty>("items.json");
        var enUs = LoadRawOfficial("en-US");
        var itemNameToKey = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in enUs.Items)
        {
            if (!string.IsNullOrWhiteSpace(value))
                itemNameToKey[value] = key;
        }
        var itemKeys = items.Where(item => item.Key != null).Select(item => item.Key!).ToHashSet(StringComparer.Ordinal);
        var errors = new List<string>();

        foreach (var locale in LocalizationLoader.GetSupportedLocales())
        {
            var raw = LoadRawOfficial(locale);
            foreach (var tile in tiles)
            {
                if (tile.Key == null)
                    continue;

                var expected = tile.Name != null
                    && itemNameToKey.TryGetValue(tile.Name, out var itemKey)
                    && raw.Items.TryGetValue(itemKey, out var itemTranslation)
                        ? itemTranslation
                        : string.Empty;
                if (!raw.Tiles.TryGetValue(tile.Key, out var actual))
                    errors.Add($"{locale}: official tiles.{tile.Key} is missing");
                else if (!string.Equals(actual, expected, StringComparison.Ordinal))
                    errors.Add($"{locale}: official tiles.{tile.Key} was manually edited");
            }

            foreach (var wall in walls)
            {
                if (wall.Key == null)
                    continue;

                var cleanName = wall.Key.Replace("Unsafe", "", StringComparison.Ordinal);
                var itemKey = new[] { cleanName + "Wall", wall.Key + "Wall", cleanName, wall.Key }
                    .FirstOrDefault(candidate => itemKeys.Contains(candidate)
                        && enUs.Items.ContainsKey(candidate));
                var expected = itemKey != null && raw.Items.TryGetValue(itemKey, out var itemTranslation)
                    ? itemTranslation
                    : string.Empty;
                if (!raw.Walls.TryGetValue(wall.Key, out var actual))
                    errors.Add($"{locale}: official walls.{wall.Key} is missing");
                else if (!string.Equals(actual, expected, StringComparison.Ordinal))
                    errors.Add($"{locale}: official walls.{wall.Key} was manually edited");
            }

            if (raw.Tiles.TryGetValue("Air", out var air) && !string.IsNullOrWhiteSpace(air))
                errors.Add($"{locale}: official tiles.Air was manually edited");

            if (raw.Frames != null && raw.Frames.Count > 0)
                errors.Add($"{locale}: official resource contains frame data");
        }

        // The Wiki-checked ManaCloakStar translation must stay in the override table.
        var zhHansOfficial = LoadRawOfficial("zh-Hans");
        if (!string.IsNullOrWhiteSpace(zhHansOfficial.Items.GetValueOrDefault("ManaCloakStar")))
            errors.Add("zh-Hans: ManaCloakStar must not be written to the official resource");
        var zhHansOverrides = LoadRawOverrides("zh-Hans");
        if (zhHansOverrides?.Items.GetValueOrDefault("ManaCloakStar") != "魔力斗篷星星")
            errors.Add("zh-Hans: missing ManaCloakStar override");

        Assert.Empty(errors);
    }

    [Fact]
    public void OverrideTables_UseValidCategoriesKeysAndNonBlankValues()
    {
        var tiles = JsonDataLoader.LoadListFromResource<TileProperty>("tiles.json");
        var walls = JsonDataLoader.LoadListFromResource<WallProperty>("walls.json");
        var items = JsonDataLoader.LoadListFromResource<ItemProperty>("items.json");
        var npcs = JsonDataLoader.LoadListFromResource<NpcData>("npcs.json");
        var bestiaryNpcs = JsonDataLoader.LoadFromResource<BestiaryNpcConfiguration>("bestiaryNpcs.json");
        var prefixes = JsonDataLoader.LoadListFromResource<PrefixData>("prefixes.json");
        var enUs = LoadRawOfficial("en-US");

        var validKeys = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
        {
            ["items"] = items.Where(x => x.Key != null).Select(x => x.Key!)
                .Concat(enUs.Items.Keys)
                .ToHashSet(StringComparer.Ordinal),
            ["tiles"] = tiles.Where(x => x.Key != null).Select(x => x.Key!)
                .Append("Air")
                .Concat(enUs.Tiles.Keys)
                .ToHashSet(StringComparer.Ordinal),
            ["walls"] = walls.Where(x => x.Key != null).Select(x => x.Key!)
                .Concat(enUs.Walls.Keys)
                .ToHashSet(StringComparer.Ordinal),
            ["npcs"] = npcs.Where(x => x.Key != null).Select(x => x.Key!)
                .Concat(bestiaryNpcs.NpcData.Select(x => x.BestiaryId))
                .Concat(enUs.Npcs.Keys)
                .ToHashSet(StringComparer.Ordinal),
            ["prefixes"] = prefixes.Where(x => x.Key != null).Select(x => x.Key!)
                .Concat(enUs.Prefixes.Keys)
                .ToHashSet(StringComparer.Ordinal),
            ["liquids"] = enUs.Liquids.Keys.ToHashSet(StringComparer.Ordinal),
            ["wires"] = enUs.Wires.Keys.ToHashSet(StringComparer.Ordinal),
        };
        var knownFrameKeys = tiles
            .Where(tile => !string.IsNullOrWhiteSpace(tile.Key) && tile.Frames != null)
            .SelectMany(tile => tile.Frames!.Select(frame => (TileKey: tile.Key!, FrameKey: frame.ToString())))
            .ToHashSet();
        var errors = new List<string>();

        foreach (var locale in LocalizationLoader.GetSupportedLocales())
        {
            var overrides = LoadRawOverrides(locale);
            if (overrides == null)
                continue;

            foreach (var (section, values) in GetNameSections(overrides))
            {
                foreach (var (key, value) in values)
                {
                    if (!validKeys[section].Contains(key))
                        errors.Add($"{locale}: invalid overrides.{section}.{key}");
                    if (string.IsNullOrWhiteSpace(value))
                        errors.Add($"{locale}: blank overrides.{section}.{key}");
                }
            }

            foreach (var (key, buff) in overrides.Buffs)
            {
                if (!enUs.Buffs.ContainsKey(key))
                    errors.Add($"{locale}: invalid overrides.buffs.{key}");
                if (string.IsNullOrWhiteSpace(buff.Name) && string.IsNullOrWhiteSpace(buff.Description))
                    errors.Add($"{locale}: blank overrides.buffs.{key}");
            }

            if (overrides.Frames != null)
            {
                foreach (var (tileKey, frames) in overrides.Frames)
                {
                    foreach (var (frameKey, value) in frames)
                    {
                        if (!knownFrameKeys.Contains((tileKey, frameKey)))
                            errors.Add($"{locale}: invalid overrides.frames.{tileKey}.{frameKey}");
                        if (string.IsNullOrWhiteSpace(value))
                            errors.Add($"{locale}: blank overrides.frames.{tileKey}.{frameKey}");
                    }
                }
            }
        }

        var zhHans = LoadRawOverrides("zh-Hans");
        if (zhHans?.Tiles.GetValueOrDefault("PlanteraBulb") != "世纪之花球茎")
            errors.Add("zh-Hans: Plantera's Bulb override must be 世纪之花球茎");
        if (zhHans?.Tiles.GetValueOrDefault("DemonAltar") != "祭坛")
            errors.Add("zh-Hans: Demon Altar tile override must be 祭坛");
        if (zhHans?.Frames?.GetValueOrDefault("Bottles")?.GetValueOrDefault("Bottle") != "玻璃瓶")
            errors.Add("zh-Hans: missing Bottle frame override");

        var ruRu = LoadRawOverrides("ru-RU");
        if (ruRu?.Tiles.GetValueOrDefault("BorealWood") != "Заснеженная древесина")
            errors.Add("ru-RU: missing BorealWood tile override");
        if (ruRu?.Tiles.GetValueOrDefault("WoodenSpikes") != "Деревянное копье")
            errors.Add("ru-RU: missing WoodenSpikes tile override");

        Assert.Empty(errors);
    }

    [Fact]
    public void SupplementalNames_UseKnownStyleKeysAndCoverAnchors()
    {
        var tiles = JsonDataLoader.LoadListFromResource<TileProperty>("tiles.json");
        var enUsSupplemental = LocalizationLoader.LoadSupplementalNames("en-US");
        var knownKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var tile in tiles)
        {
            if (!string.IsNullOrWhiteSpace(tile.Key))
                knownKeys.Add(tile.Key);
            if (tile.BiomeVariants != null)
            {
                foreach (var variant in tile.BiomeVariants)
                {
                    if (!string.IsNullOrWhiteSpace(variant.Name))
                        knownKeys.Add(variant.Name);
                }
            }
            if (tile.Frames == null)
                continue;
            foreach (var frame in tile.Frames)
            {
                if (!string.IsNullOrWhiteSpace(frame.Name))
                    knownKeys.Add(frame.Name);
                if (!string.IsNullOrWhiteSpace(frame.Variety))
                    knownKeys.Add(frame.Variety);
            }
        }

        var errors = new List<string>();
        if (enUsSupplemental.Count == 0)
            errors.Add("en-US: supplemental names are empty");

        foreach (var (key, value) in enUsSupplemental)
        {
            if (string.IsNullOrWhiteSpace(value))
                errors.Add($"en-US: blank supplemental name {key}");
            if (key.StartsWith("Anchor.", StringComparison.Ordinal))
            {
                if (!Enum.TryParse<FrameAnchor>(key["Anchor.".Length..], out _))
                    errors.Add($"en-US: unknown supplemental anchor {key}");
                continue;
            }
            if (key is "Wall.Natural" or "Wall.Unused")
                continue;
            if (!knownKeys.Contains(key))
                errors.Add($"en-US: supplemental key has no style source {key}");
        }

        foreach (var locale in LocalizationLoader.GetSupportedLocales())
        {
            if (!LocalizationLoader.HasSupplementalNames(locale))
                continue;

            foreach (var (key, value) in LocalizationLoader.LoadSupplementalNames(locale))
            {
                if (!enUsSupplemental.ContainsKey(key))
                    errors.Add($"{locale}: supplemental key not present in en-US source: {key}");
                if (string.IsNullOrWhiteSpace(value))
                    errors.Add($"{locale}: blank supplemental name {key}");
            }
        }

        foreach (var anchor in Enum.GetValues<FrameAnchor>())
        {
            if (!enUsSupplemental.ContainsKey($"Anchor.{anchor}"))
                errors.Add($"en-US: missing supplemental anchor Anchor.{anchor}");
        }

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
        var names = new List<string>
        {
            "items.json", "tiles.json", "walls.json", "npcs.json", "prefixes.json", "bestiaryNpcs.json"
        };
        foreach (var locale in LocalizationLoader.GetSupportedLocales())
        {
            names.Add($"Localization/{locale}.json");
            if (LocalizationLoader.HasOverrides(locale))
                names.Add(LocalizationLoader.GetOverridesResourceName(locale));
            if (LocalizationLoader.HasSupplementalNames(locale))
                names.Add(LocalizationLoader.GetSupplementalResourceName(locale));
        }

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

    private static LocalizationData LoadRawOfficial(string locale)
    {
        using var stream = JsonDataLoader.GetDataStream(LocalizationLoader.GetOfficialResourceName(locale));
        return JsonSerializer.Deserialize<LocalizationData>(stream)!;
    }

    private static LocalizationData? LoadRawOverrides(string locale)
        => LocalizationLoader.LoadLocalizationOverrides(locale);

    private static LocalizationData CreateCombinedRaw(LocalizationData official, LocalizationData? overrides)
    {
        var combined = new LocalizationData
        {
            Items = Clone(official.Items),
            Buffs = official.Buffs.ToDictionary(
                pair => pair.Key,
                pair => new BuffLocalization { Name = pair.Value.Name, Description = pair.Value.Description },
                StringComparer.Ordinal),
            Npcs = Clone(official.Npcs),
            Prefixes = Clone(official.Prefixes),
            Tiles = Clone(official.Tiles),
            Walls = Clone(official.Walls),
            Liquids = Clone(official.Liquids),
            Wires = Clone(official.Wires),
            Frames = official.Frames?.ToDictionary(
                pair => pair.Key,
                pair => Clone(pair.Value),
                StringComparer.Ordinal),
        };
        if (overrides == null)
            return combined;

        MergeStringSection(combined.Items, overrides.Items);
        MergeStringSection(combined.Npcs, overrides.Npcs);
        MergeStringSection(combined.Prefixes, overrides.Prefixes);
        MergeStringSection(combined.Tiles, overrides.Tiles);
        MergeStringSection(combined.Walls, overrides.Walls);
        MergeStringSection(combined.Liquids, overrides.Liquids);
        MergeStringSection(combined.Wires, overrides.Wires);
        foreach (var (key, buff) in overrides.Buffs)
        {
            if (!combined.Buffs.TryGetValue(key, out var target))
                combined.Buffs[key] = target = new BuffLocalization();
            if (!string.IsNullOrWhiteSpace(buff.Name))
                target.Name = buff.Name;
            if (!string.IsNullOrWhiteSpace(buff.Description))
                target.Description = buff.Description;
        }
        if (overrides.Frames != null)
        {
            combined.Frames ??= new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            foreach (var (tileKey, frames) in overrides.Frames)
            {
                if (!combined.Frames.TryGetValue(tileKey, out var target))
                    combined.Frames[tileKey] = target = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var (frameKey, value) in frames)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                        target[frameKey] = value;
                }
            }
        }
        return combined;
    }

    private static Dictionary<string, string> Clone(Dictionary<string, string> source)
        => new(source, StringComparer.Ordinal);

    private static void MergeStringSection(Dictionary<string, string> target, Dictionary<string, string> source)
    {
        foreach (var (key, value) in source)
        {
            if (!string.IsNullOrWhiteSpace(value))
                target[key] = value;
        }
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

    private static void AppendOverrideSummary(
        StringBuilder report,
        string locale,
        LocalizationData raw,
        LocalizationData? overrides,
        IReadOnlyDictionary<string, HashSet<string>> expectedKeys,
        HashSet<(string TileKey, string FrameKey)> knownFrameKeys,
        List<string> errors)
    {
        if (overrides == null)
        {
            report.AppendLine("- Overrides: none");
            return;
        }

        var counts = new List<string>();
        foreach (var (section, values) in GetNameSections(overrides))
        {
            var corrections = 0;
            var supplements = 0;
            var redundant = 0;
            foreach (var (key, value) in values)
            {
                if (!expectedKeys[section].Contains(key))
                    errors.Add($"{locale}: invalid overrides.{section}.{key}");
                if (string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"{locale}: blank overrides.{section}.{key}");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(rawValue(raw, section, key)))
                {
                    if (rawValue(raw, section, key) == value)
                        redundant++;
                    else
                        corrections++;
                }
                else
                {
                    supplements++;
                }
            }
            counts.Add($"{section}: {values.Count} ({corrections} corrections, {supplements} fills, {redundant} redundant)");
        }

        if (overrides.Frames != null)
        {
            var frameCount = 0;
            foreach (var (tileKey, frames) in overrides.Frames)
            {
                foreach (var (frameKey, value) in frames)
                {
                    frameCount++;
                    if (!knownFrameKeys.Contains((tileKey, frameKey)))
                        errors.Add($"{locale}: invalid overrides.frames.{tileKey}.{frameKey}");
                    if (string.IsNullOrWhiteSpace(value))
                        errors.Add($"{locale}: blank overrides.frames.{tileKey}.{frameKey}");
                }
            }
            counts.Add($"frames: {frameCount}");
        }

        report.AppendLine($"- Overrides: {string.Join("; ", counts)}");
    }

    private static string? rawValue(LocalizationData data, string section, string key) => section switch
    {
        "items" => data.Items.GetValueOrDefault(key),
        "npcs" => data.Npcs.GetValueOrDefault(key),
        "prefixes" => data.Prefixes.GetValueOrDefault(key),
        "tiles" => data.Tiles.GetValueOrDefault(key),
        "walls" => data.Walls.GetValueOrDefault(key),
        "liquids" => data.Liquids.GetValueOrDefault(key),
        "wires" => data.Wires.GetValueOrDefault(key),
        _ => null
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
        if (data.Frames != null)
        {
            foreach (var (tileKey, frameNames) in data.Frames)
                foreach (var (frameKey, value) in frameNames)
                    AddNode($"FrameName.{tileKey}.{frameKey}", value);
        }

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

        if (data.Frames != null)
        {
            foreach (var (tileKey, frames) in data.Frames)
                foreach (var (frameKey, value) in frames)
                    if (string.IsNullOrWhiteSpace(value))
                        empty.Add($"frames.{tileKey}.{frameKey}");
        }

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
        if (data.Frames != null)
        {
            foreach (var frameNames in data.Frames.Values)
                foreach (var value in frameNames.Values)
                    yield return value;
        }
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
