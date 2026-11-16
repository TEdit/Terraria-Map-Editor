using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using TEdit.Terraria.DataModel;

namespace TEdit.Terraria.Loaders;

public static class LocalizationLoader
{
    private static readonly string[] SupportedLocales =
    [
        "de-DE", "en-US", "es-ES", "fr-FR", "it-IT", "ja-JP",
        "ko-KR", "pl-PL", "pt-BR", "ru-RU", "zh-Hans", "zh-Hant"
    ];

    /// <summary>
    /// Maps from a .NET CultureInfo name to the closest supported Terraria locale.
    /// Returns "en-US" for unsupported cultures.
    /// </summary>
    private static readonly Dictionary<string, string> CultureMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        ["de"]      = "de-DE",
        ["de-DE"]   = "de-DE",
        ["en"]      = "en-US",
        ["en-US"]   = "en-US",
        ["es"]      = "es-ES",
        ["es-ES"]   = "es-ES",
        ["fr"]      = "fr-FR",
        ["fr-FR"]   = "fr-FR",
        ["it"]      = "it-IT",
        ["it-IT"]   = "it-IT",
        ["ja"]      = "ja-JP",
        ["ja-JP"]   = "ja-JP",
        ["ko"]      = "ko-KR",
        ["ko-KR"]   = "ko-KR",
        ["pl"]      = "pl-PL",
        ["pl-PL"]   = "pl-PL",
        ["pt"]      = "pt-BR",
        ["pt-BR"]   = "pt-BR",
        ["ru"]      = "ru-RU",
        ["ru-RU"]   = "ru-RU",
        ["zh-Hans"] = "zh-Hans",
        ["zh-Hant"] = "zh-Hant",
        ["zh-CN"]   = "zh-Hans",
        ["zh-TW"]   = "zh-Hant",
        ["zh-HK"]   = "zh-Hant",
    };

    public static IReadOnlyList<string> GetSupportedLocales() => SupportedLocales;

    /// <summary>
    /// Resolves the current thread's UI culture to the best matching Terraria locale.
    /// </summary>
    public static string GetLocaleFromCurrentCulture()
    {
        var culture = CultureInfo.CurrentUICulture;

        // Try exact match first, then parent culture
        if (CultureMapping.TryGetValue(culture.Name, out var locale))
            return locale;

        if (!culture.IsNeutralCulture && CultureMapping.TryGetValue(culture.Parent.Name, out locale))
            return locale;

        return "en-US";
    }

    #region Resource names

    /// <summary>
    /// Official, automatically generated resource. Never edited by hand.
    /// </summary>
    public static string GetOfficialResourceName(string locale)
        => Path.Combine("Localization", $"{locale}.json");

    /// <summary>
    /// Optional hand-verified corrections, merged after the official resource.
    /// </summary>
    public static string GetOverridesResourceName(string locale)
        => Path.Combine("Localization", $"LocalizationOverrides.{locale}.json");

    /// <summary>
    /// Optional supplemental sprite/style names maintained separately from the
    /// official game localization (Crowdin source: supplementalNames.en-US.json).
    /// </summary>
    public static string GetSupplementalResourceName(string locale)
        => Path.Combine("Localization", $"supplementalNames.{locale}.json");

    public static bool HasOverrides(string locale, string? dataPath = null)
        => ResourceExists(GetOverridesResourceName(locale), dataPath);

    public static bool HasOfficialResource(string locale, string? dataPath = null)
        => ResourceExists(GetOfficialResourceName(locale), dataPath);

    public static bool HasSupplementalNames(string locale, string? dataPath = null)
        => ResourceExists(GetSupplementalResourceName(locale), dataPath);

    private static bool ResourceExists(string resourceName, string? dataPath)
    {
        try
        {
            using var stream = JsonDataLoader.GetDataStream(resourceName, dataPath);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
    }

    #endregion

    #region Loading

    /// <summary>
    /// Loads the official resource for <paramref name="locale"/>, applies the optional
    /// overrides resource, merges supplemental names, and only then resolves references.
    /// Blank or unresolvable override values are discarded so they can never replace a
    /// valid official translation.
    /// </summary>
    public static LocalizationData LoadLocalization(string locale, string? dataPath = null)
    {
        var data = LoadOfficialLocalization(locale, dataPath);

        // Keep a resolved copy of the official-only data. It is used as a safety net
        // when an override is blank or contains an invalid reference.
        var baseline = Clone(data);
        ResolveReferences(baseline);

        var overrides = LoadLocalizationOverrides(locale, dataPath);
        var overrideEntries = MergeOverrides(data, overrides);

        MergeSupplementalNames(data, LoadSupplementalNames(locale, dataPath));

        // Reference resolution happens once, after official + overrides have been
        // merged, so official values can point at corrected keys and vice versa.
        ResolveReferences(data);
        RestoreInvalidOverrides(data, baseline, overrideEntries);
        return data;
    }

    /// <summary>
    /// Loads only the generated official resource. Does not apply overrides or
    /// supplemental names and does not resolve references.
    /// </summary>
    public static LocalizationData LoadOfficialLocalization(string locale, string? dataPath = null)
        => LoadOptional<LocalizationData>(GetOfficialResourceName(locale), dataPath)
            ?? new LocalizationData();

    /// <summary>
    /// Loads the optional overrides resource, or null when the locale has none.
    /// </summary>
    public static LocalizationData? LoadLocalizationOverrides(string locale, string? dataPath = null)
        => LoadOptional<LocalizationData>(GetOverridesResourceName(locale), dataPath);

    /// <summary>
    /// Loads the optional supplemental sprite/style name dictionary.
    /// </summary>
    public static Dictionary<string, string> LoadSupplementalNames(string locale, string? dataPath = null)
        => LoadOptional<Dictionary<string, string>>(GetSupplementalResourceName(locale), dataPath)
            ?? new Dictionary<string, string>(StringComparer.Ordinal);

    private static T? LoadOptional<T>(string resourceName, string? dataPath)
    {
        try
        {
            using var stream = JsonDataLoader.GetDataStream(resourceName, dataPath);
            return JsonSerializer.Deserialize<T>(stream);
        }
        catch (FileNotFoundException)
        {
            return default;
        }
    }

    private static LocalizationData Clone(LocalizationData source) => new()
    {
        Items = CloneDictionary(source.Items),
        Buffs = source.Buffs.ToDictionary(
            pair => pair.Key,
            pair => new BuffLocalization { Name = pair.Value.Name, Description = pair.Value.Description },
            StringComparer.Ordinal),
        Npcs = CloneDictionary(source.Npcs),
        Prefixes = CloneDictionary(source.Prefixes),
        Tiles = CloneDictionary(source.Tiles),
        Walls = CloneDictionary(source.Walls),
        Liquids = CloneDictionary(source.Liquids),
        Wires = CloneDictionary(source.Wires),
        Frames = source.Frames?.ToDictionary(
            pair => pair.Key,
            pair => CloneDictionary(pair.Value),
            StringComparer.Ordinal),
        SupplementalNames = CloneDictionary(source.SupplementalNames),
        OverriddenKeys = new HashSet<string>(source.OverriddenKeys, StringComparer.Ordinal),
    };

    private static Dictionary<string, string> CloneDictionary(Dictionary<string, string> source)
        => new(source, StringComparer.Ordinal);

    #endregion

    #region Override merging

    private readonly record struct OverrideEntry(string Section, string Key, string? FrameKey)
    {
        public string DisplayKey => FrameKey == null
            ? $"{Section}.{Key}"
            : $"{Section}.{Key}.{FrameKey}";
    }

    private static List<OverrideEntry> MergeOverrides(LocalizationData data, LocalizationData? overrides)
    {
        var entries = new List<OverrideEntry>();
        if (overrides == null)
            return entries;

        MergeSection(data.Items, overrides.Items, "items", null, entries);
        MergeSection(data.Npcs, overrides.Npcs, "npcs", null, entries);
        MergeSection(data.Prefixes, overrides.Prefixes, "prefixes", null, entries);
        MergeSection(data.Tiles, overrides.Tiles, "tiles", null, entries);
        MergeSection(data.Walls, overrides.Walls, "walls", null, entries);
        MergeSection(data.Liquids, overrides.Liquids, "liquids", null, entries);
        MergeSection(data.Wires, overrides.Wires, "wires", null, entries);

        if (overrides.Buffs != null)
        {
            foreach (var (key, buff) in overrides.Buffs)
            {
                if (string.IsNullOrWhiteSpace(key) || buff == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(buff.Name))
                {
                    if (!data.Buffs.TryGetValue(key, out var target))
                        data.Buffs[key] = target = new BuffLocalization();
                    target.Name = buff.Name;
                    entries.Add(new OverrideEntry("buffs.name", key, null));
                }

                if (!string.IsNullOrWhiteSpace(buff.Description))
                {
                    if (!data.Buffs.TryGetValue(key, out var target))
                        data.Buffs[key] = target = new BuffLocalization();
                    target.Description = buff.Description;
                    entries.Add(new OverrideEntry("buffs.description", key, null));
                }
            }
        }

        if (overrides.Frames != null)
        {
            data.Frames ??= new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            foreach (var (tileKey, frames) in overrides.Frames)
            {
                if (string.IsNullOrWhiteSpace(tileKey) || frames == null)
                    continue;

                if (!data.Frames.TryGetValue(tileKey, out var target))
                    data.Frames[tileKey] = target = new Dictionary<string, string>(StringComparer.Ordinal);

                foreach (var (frameKey, value) in frames)
                {
                    if (string.IsNullOrWhiteSpace(frameKey) || string.IsNullOrWhiteSpace(value))
                        continue;

                    target[frameKey] = value;
                    entries.Add(new OverrideEntry("frames", tileKey, frameKey));
                }
            }
        }

        return entries;
    }

    private static void MergeSection(
        Dictionary<string, string> target,
        Dictionary<string, string>? source,
        string section,
        string? frameKey,
        List<OverrideEntry> entries)
    {
        if (source == null)
            return;

        foreach (var (key, value) in source)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                continue;

            target[key] = value;
            entries.Add(new OverrideEntry(section, key, frameKey));
        }
    }

    private static void MergeSupplementalNames(LocalizationData data, Dictionary<string, string> supplemental)
    {
        foreach (var (key, value) in supplemental)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                continue;

            data.SupplementalNames[key] = value;
        }
    }

    private static void RestoreInvalidOverrides(
        LocalizationData data,
        LocalizationData baseline,
        List<OverrideEntry> entries)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            var mergedValue = GetValue(data, entry);
            if (string.IsNullOrWhiteSpace(mergedValue))
            {
                var fallback = GetValue(baseline, entry);
                if (!string.IsNullOrWhiteSpace(fallback))
                    SetValue(data, entry, fallback);
                continue;
            }

            applied.Add(entry.DisplayKey);
        }

        data.OverriddenKeys = applied;
    }

    private static string? GetValue(LocalizationData data, OverrideEntry entry) => entry.Section switch
    {
        "items" => data.Items.GetValueOrDefault(entry.Key),
        "npcs" => data.Npcs.GetValueOrDefault(entry.Key),
        "prefixes" => data.Prefixes.GetValueOrDefault(entry.Key),
        "tiles" => data.Tiles.GetValueOrDefault(entry.Key),
        "walls" => data.Walls.GetValueOrDefault(entry.Key),
        "liquids" => data.Liquids.GetValueOrDefault(entry.Key),
        "wires" => data.Wires.GetValueOrDefault(entry.Key),
        "buffs.name" => data.Buffs.TryGetValue(entry.Key, out var buff) ? buff.Name : null,
        "buffs.description" => data.Buffs.TryGetValue(entry.Key, out var description) ? description.Description : null,
        "frames" => data.Frames != null
            && data.Frames.TryGetValue(entry.Key, out var frames)
            && entry.FrameKey != null
            && frames.TryGetValue(entry.FrameKey, out var value)
                ? value
                : null,
        _ => null,
    };

    private static void SetValue(LocalizationData data, OverrideEntry entry, string value)
    {
        switch (entry.Section)
        {
            case "items": data.Items[entry.Key] = value; break;
            case "npcs": data.Npcs[entry.Key] = value; break;
            case "prefixes": data.Prefixes[entry.Key] = value; break;
            case "tiles": data.Tiles[entry.Key] = value; break;
            case "walls": data.Walls[entry.Key] = value; break;
            case "liquids": data.Liquids[entry.Key] = value; break;
            case "wires": data.Wires[entry.Key] = value; break;
            case "buffs.name":
                if (!data.Buffs.TryGetValue(entry.Key, out var buff))
                    data.Buffs[entry.Key] = buff = new BuffLocalization();
                buff.Name = value;
                break;
            case "buffs.description":
                if (!data.Buffs.TryGetValue(entry.Key, out var description))
                    data.Buffs[entry.Key] = description = new BuffLocalization();
                description.Description = value;
                break;
            case "frames":
                if (entry.FrameKey != null)
                {
                    data.Frames ??= new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
                    if (!data.Frames.TryGetValue(entry.Key, out var frames))
                        data.Frames[entry.Key] = frames = new Dictionary<string, string>(StringComparer.Ordinal);
                    frames[entry.FrameKey] = value;
                }
                break;
        }
    }

    #endregion

    #region Reference resolution

    private static readonly Regex ReferencePattern = new(@"\{\$([^.}]+)\.([^}]+)\}", RegexOptions.Compiled);
    private const int MaximumReferenceDepth = 256;

    /// <summary>
    /// Resolves Terraria-style {$Category.Key} string references in all sections.
    /// For example, {$BuffName.Minecart} resolves to the Minecart buff's name.
    /// </summary>
    private static void ResolveReferences(LocalizationData data)
    {
        ResolveDict(data.Items, data);
        ResolveDict(data.Npcs, data);
        ResolveDict(data.Prefixes, data);
        ResolveDict(data.Tiles, data);
        ResolveDict(data.Walls, data);
        ResolveDict(data.Liquids, data);
        ResolveDict(data.Wires, data);
        ResolveDict(data.SupplementalNames, data);
        if (data.Frames != null)
        {
            foreach (var frameNames in data.Frames.Values)
                ResolveDict(frameNames, data);
        }

        foreach (var kvp in data.Buffs)
        {
            kvp.Value.Name = ResolveString(kvp.Value.Name, data);
            kvp.Value.Description = ResolveString(kvp.Value.Description, data);
        }
    }

    private static void ResolveDict(Dictionary<string, string> dict, LocalizationData data)
    {
        var keys = new List<string>(dict.Keys);
        foreach (var key in keys)
        {
            dict[key] = ResolveString(dict[key], data);
        }
    }

    private static string ResolveString(string value, LocalizationData data)
        => ResolveString(value, data, new HashSet<string>(StringComparer.Ordinal), 0);

    private static string ResolveString(
        string value,
        LocalizationData data,
        HashSet<string> resolvingReferences,
        int depth)
    {
        if (string.IsNullOrEmpty(value) || !value.Contains("{$"))
            return value;

        // Locale references are trusted data, but a malformed or unexpectedly
        // deep chain must not be able to exhaust the process stack.
        if (depth >= MaximumReferenceDepth)
            return string.Empty;

        var hasEmptyReference = false;
        var result = ReferencePattern.Replace(value, match =>
        {
            var category = match.Groups[1].Value;
            var key = match.Groups[2].Value;
            var reference = $"{category}.{key}";
            if (!resolvingReferences.Add(reference))
            {
                // Cyclic references cannot produce a complete display label.
                hasEmptyReference = true;
                return string.Empty;
            }

            try
            {
                var resolved = LookupReference(category, key, data);
                if (resolved == null)
                {
                    // Invalid references are reported by the raw-resource
                    // integrity audit; at runtime, fall back to the whole label.
                    hasEmptyReference = true;
                    return string.Empty;
                }

                resolved = ResolveString(resolved, data, resolvingReferences, depth + 1);
                if (string.IsNullOrWhiteSpace(resolved))
                    hasEmptyReference = true;
                return resolved;
            }
            finally
            {
                resolvingReferences.Remove(reference);
            }
        });

        // An untranslated dependency makes the entire label untranslated, so
        // display models can fall back to the complete source name.
        return hasEmptyReference ? string.Empty : result;
    }

    private static string? LookupReference(string category, string key, LocalizationData data)
    {
        switch (category)
        {
            case "ItemName":
                return data.Items.TryGetValue(key, out var item) ? item : null;
            case "NPCName":
                return data.Npcs.TryGetValue(key, out var npc) ? npc : null;
            case "BuffName":
                return data.Buffs.TryGetValue(key, out var buffName) ? buffName.Name : null;
            case "BuffDescription":
                return data.Buffs.TryGetValue(key, out var buffDesc) ? buffDesc.Description : null;
            case "Prefix":
            case "PrefixName":
                return data.Prefixes.TryGetValue(key, out var prefix) ? prefix : null;
            case "TileName":
                return data.Tiles.TryGetValue(key, out var tile) ? tile : null;
            case "WallName":
                return data.Walls.TryGetValue(key, out var wall) ? wall : null;
            case "LiquidName":
                return data.Liquids.TryGetValue(key, out var liquid) ? liquid : null;
            case "WireName":
                return data.Wires.TryGetValue(key, out var wire) ? wire : null;
            default:
                return null;
        }
    }

    #endregion
}
