using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
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
        var resourceName = $"Localization.{locale}.json";

        try
        {
            using var stream = JsonDataLoader.GetDataStream(resourceName, dataPath);
            return JsonSerializer.Deserialize<LocalizationData>(stream)
                ?? new LocalizationData();
        }
        catch (System.IO.FileNotFoundException)
        {
            return new LocalizationData();
        }
    }
}
