using System;
using System.Globalization;
using System.Windows.Data;

namespace TEdit.Converters;

/// <summary>
/// Returns true if the value is greater than the threshold specified in ConverterParameter.
/// </summary>
public class GreaterThanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d && parameter is string s && double.TryParse(s, CultureInfo.InvariantCulture, out double threshold))
            return d > threshold;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
