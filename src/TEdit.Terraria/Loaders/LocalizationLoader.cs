using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

    public static LocalizationData LoadLocalization(string locale, string? dataPath = null)
    {
        var data = LoadOptional<LocalizationData>(Path.Combine("Localization", $"{locale}.json"), dataPath)
            ?? new LocalizationData();

        ResolveReferences(data);
        return data;
    }

    private static T? LoadOptional<T>(string resourceName, string? dataPath)
    {
        try
        {
            using var stream = JsonDataLoader.GetDataStream(resourceName, dataPath);
            return JsonSerializer.Deserialize<T>(stream);
        }
        catch (System.IO.FileNotFoundException)
        {
            return default;
        }
    }

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
        foreach (var frameNames in data.Frames.Values)
            ResolveDict(frameNames, data);

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
}
