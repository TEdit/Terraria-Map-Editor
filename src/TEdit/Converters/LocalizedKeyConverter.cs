using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TEdit.Properties;

namespace TEdit.Converters;

/// <summary>Looks up "&lt;prefix&gt;&lt;name&gt;" in Language.resx using the UI culture.</summary>
public class LocalizedKeyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string name)
            return value;

        // WPF passes en-US here; use the app's UI culture.
        var key = (parameter as string ?? string.Empty) + name;
        return Language.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? name;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}
