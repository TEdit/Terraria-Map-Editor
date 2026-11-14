using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TEdit.Properties;

namespace TEdit.Helper;

/// <summary>
/// Resolves a background style's English name to a localized display name via the bg_*
/// resource templates. Exact names map directly; numbered/lettered variants are matched
/// against the invariant (English) format templates and re-formatted in the current UI
/// culture. Falls back to the original name when no template matches.
/// </summary>
public static class BackgroundNameLocalizer
{
    private sealed record Template(string Key, string English, Regex? Pattern, int Args);

    private static readonly Template[] Templates = BuildTemplates();

    public static string Get(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        foreach (var template in Templates)
        {
            if (template.Pattern is null)
            {
                if (string.Equals(template.English, name, StringComparison.Ordinal))
                    return Localized(template);
                continue;
            }

            var match = template.Pattern.Match(name);
            if (!match.Success)
                continue;

            var args = new object[match.Groups.Count - 1];
            for (int i = 0; i < args.Length; i++)
                args[i] = match.Groups[i + 1].Value;
            return string.Format(Localized(template), args);
        }

        return name;
    }

    private static string Localized(Template template) =>
        Language.ResourceManager.GetString(template.Key, CultureInfo.CurrentUICulture) ?? template.English;

    private static Template[] BuildTemplates()
    {
        var list = new List<Template>();
        var set = Language.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        if (set != null)
        {
            foreach (DictionaryEntry entry in set)
            {
                if (entry.Key is not string key || !key.StartsWith("bg_", StringComparison.Ordinal))
                    continue;
                if (entry.Value is not string english)
                    continue;

                int args = CountArgs(english);
                list.Add(new Template(key, english, args > 0 ? BuildPattern(english, args) : null, args));
            }
        }

        // Match the most specific templates first; exact names are checked last.
        // Tie-break by key so resolution never depends on resource enumeration order.
        list.Sort((a, b) =>
        {
            int byArgs = b.Args.CompareTo(a.Args);
            return byArgs != 0 ? byArgs : StringComparer.Ordinal.Compare(a.Key, b.Key);
        });
        return list.ToArray();
    }

    private static int CountArgs(string template)
    {
        int highest = -1;
        for (int i = 0; i < 10; i++)
        {
            if (template.Contains($"{{{i}}}", StringComparison.Ordinal))
                highest = i;
        }
        return highest + 1;
    }

    private static Regex BuildPattern(string english, int args)
    {
        // Escape only the literal segments. Regex.Escape handles braces inconsistently,
        // so replacing "{0}" inside an already-escaped string silently fails.
        var literals = new List<string>();
        int position = 0;
        for (int i = 0; i < args; i++)
        {
            string placeholder = $"{{{i}}}";
            int index = english.IndexOf(placeholder, position, StringComparison.Ordinal);
            if (index < 0)
                return new Regex("(?!)", RegexOptions.CultureInvariant);

            literals.Add(Regex.Escape(english[position..index]));
            position = index + placeholder.Length;
        }
        literals.Add(Regex.Escape(english[position..]));

        // Placeholders are bare numbers ("Variant 7") or letters ("Multi-Biome A"),
        // so accept a single alphanumeric token instead of assuming a digit.
        var pattern = new StringBuilder("^");
        for (int i = 0; i < args; i++)
            pattern.Append(literals[i]).Append("([A-Za-z0-9]+)");
        pattern.Append(literals[args]).Append('$');
        return new Regex(pattern.ToString(), RegexOptions.CultureInvariant);
    }
}
