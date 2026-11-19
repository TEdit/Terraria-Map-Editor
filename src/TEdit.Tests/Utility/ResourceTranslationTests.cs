using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;
using TEdit.Properties;
using Xunit;

namespace TEdit.Tests.Utility;

public class ResourceTranslationTests
{
    [Theory]
    [InlineData("en")]
    [InlineData("ar-BH")]
    [InlineData("de-DE")]
    [InlineData("es-ES")]
    [InlineData("fr-FR")]
    [InlineData("it-IT")]
    [InlineData("ja-JP")]
    [InlineData("ko-KR")]
    [InlineData("pl-PL")]
    [InlineData("pt-BR")]
    [InlineData("ru-RU")]
    [InlineData("zh-Hans")]
    [InlineData("zh-Hant")]
    public void Translations_PreserveCompositeFormatArguments(string locale)
    {
        var source = Language.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true)!;
        var translations = Language.ResourceManager.GetResourceSet(CultureInfo.GetCultureInfo(locale), true, false)!;

        foreach (DictionaryEntry entry in translations)
        {
            string key = (string)entry.Key;
            string translated = (string)entry.Value!;
            string? original = source.GetString(key);
            if (original is null)
                continue;

            // Reordering and repeating arguments is valid; losing or adding an argument is not.
            string[] expected = Placeholders(original);
            Placeholders(translated).ShouldBe(expected, $"{locale}: {key}");
            if (expected.Length > 0)
            {
                Should.NotThrow(() => CompositeFormat.Parse(translated), $"{locale}: {key}");
            }
        }
    }

    private static string[] Placeholders(string value) =>
        Regex.Matches(value, @"\{\d+(?:[^{}]*)\}")
            .Select(match => match.Value)
            .Distinct()
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
}
