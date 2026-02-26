
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TEdit.Common;
using TEdit.Terraria;

namespace TEdit.Converters;

public class TEditColorToMediaColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return DependencyProperty.UnsetValue;

        if (value is TEditColor)
        {
            TEditColor tEditColor = (TEditColor)value;
            return System.Windows.Media.Color.FromArgb(tEditColor.A, tEditColor.R, tEditColor.G, tEditColor.B);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return DependencyProperty.UnsetValue;

        if (value is System.Windows.Media.Color)
        {
            System.Windows.Media.Color mediaColor = (System.Windows.Media.Color)value;
            return new TEditColor(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }

        return DependencyProperty.UnsetValue;
    }
}

public class EnumToBoolConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object trueValue, System.Globalization.CultureInfo culture)
    {
        if (value != null && value.GetType().IsEnum)
            return (Enum.Equals(value, trueValue));
        else
            return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object trueValue, System.Globalization.CultureInfo culture)
    {
        if (value is bool && (bool)value)
            return trueValue;
        else
            return DependencyProperty.UnsetValue;
    }

}
public class ByteIntConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value?.ToString())) return DependencyProperty.UnsetValue;

        return (byte)(int)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value?.ToString())) return DependencyProperty.UnsetValue;

        return (int)(byte)value;
    }
}

public class ShortIntConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value?.ToString())) return DependencyProperty.UnsetValue;

        return (int)(short)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value?.ToString())) return DependencyProperty.UnsetValue;

        return (short)(int)value;
    }
}

public class IntByteConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value?.ToString())) return DependencyProperty.UnsetValue;

        return (int)(byte)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value?.ToString())) return DependencyProperty.UnsetValue;

        return (byte)(int)value;
    }
}

public class DoublePercentageConverter : IValueConverter
{
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter,
                          CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value?.ToString())) return DependencyProperty.UnsetValue;

        if (value.GetType() == typeof(double)) return (double)value;

        if (value.GetType() == typeof(decimal)) return (decimal)value;

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
                              CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value?.ToString())) return DependencyProperty.UnsetValue;

        string trimmedValue = value.ToString().TrimEnd(new[] { '%' });

        if (targetType == typeof(double))
        {
            double result;
            if (double.TryParse(trimmedValue, out result))
                return result / 100;
            else
                return value;
        }

        if (targetType == typeof(decimal))
        {
            decimal result;
            if (decimal.TryParse(trimmedValue, out result))
                return result / 100;
            else
                return value;
        }
        return value;
    }

    #endregion
}

public class TeamIndexToNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int index && index >= 0 && index < World.TeamNames.Length)
        {
            string key = $"TeamSpawn_{World.TeamNames[index]}";
            string localized = Properties.Language.ResourceManager.GetString(key, culture);
            return $"{localized ?? World.TeamNames[index]}:";
        }

        return $"Team {value}:";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}

public class WindSpeedToMphConverter : IValueConverter
{
    // Terraria displays wind mph as: (int)(windSpeedCurrent * 50)
    private const double Factor = 50.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is float f) return $"{(int)(f * Factor)} mph";
        if (value is double d) return $"{(int)(d * Factor)} mph";
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            s = s.Replace("mph", "", StringComparison.OrdinalIgnoreCase).Trim();
            if (int.TryParse(s, out int mph))
                return (float)(mph / Factor);
        }
        return DependencyProperty.UnsetValue;
    }
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
