using System;
using System.Globalization;
using TEdit.Properties;
using TEdit.Terraria.DataModel;

namespace TEdit.Helper;

/// <summary>Resolves a background style's localized name from its key in backgroundStyles.json.</summary>
public static class BackgroundNameLocalizer
{
    public static string Get(BackgroundStyle style)
    {
        if (string.IsNullOrEmpty(style.Key))
            return style.Name;

        var text = Language.ResourceManager.GetString(style.Key, CultureInfo.CurrentUICulture);
        if (string.IsNullOrEmpty(text))
            return style.Name;

        if (style.Args is not { Length: > 0 })
            return text;

        try
        {
            return string.Format(text, (object[])style.Args);
        }
        catch (FormatException)
        {
            return style.Name;
        }
    }
}
