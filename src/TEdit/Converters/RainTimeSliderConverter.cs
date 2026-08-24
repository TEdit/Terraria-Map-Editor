using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TEdit.Properties;

namespace TEdit.Converters;

public static class RainTimeValues
{
    public const int MaximumNormal = 221389;
    public const int OneHour = 216000;
    public const int Permanent = 5184000;
    public const int OneYear = 1892160000;

    public const double OneHourSliderValue = 50;
    public const double OneYearSliderValue = 100;
}

/// <summary>
/// Maps rain duration to a piecewise slider: the first half is linear through
/// one hour, while the second half is logarithmic through one year.
/// </summary>
public sealed class RainTimeSliderConverter : IValueConverter
{
    private const double SnapDistance = 0.75;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int rainTime)
            return DependencyProperty.UnsetValue;

        if (rainTime <= 0)
            return 0.0;
        if (rainTime <= RainTimeValues.OneHour)
            return (double)rainTime / RainTimeValues.OneHour * RainTimeValues.OneHourSliderValue;
        if (rainTime <= RainTimeValues.OneYear)
            return LogarithmicMap(
                rainTime,
                RainTimeValues.OneHour,
                RainTimeValues.OneYear,
                RainTimeValues.OneHourSliderValue,
                RainTimeValues.OneYearSliderValue);

        return RainTimeValues.OneYearSliderValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double sliderValue)
            return DependencyProperty.UnsetValue;

        sliderValue = Math.Clamp(sliderValue, 0, RainTimeValues.OneYearSliderValue);
        if (sliderValue >= RainTimeValues.OneYearSliderValue - SnapDistance)
            return RainTimeValues.OneYear;
        if (sliderValue <= RainTimeValues.OneHourSliderValue)
            return (int)Math.Round(sliderValue / RainTimeValues.OneHourSliderValue * RainTimeValues.OneHour);

        return ExponentialMap(
            sliderValue,
            RainTimeValues.OneHourSliderValue,
            RainTimeValues.OneYearSliderValue,
            RainTimeValues.OneHour,
            RainTimeValues.OneYear);
    }

    private static double LogarithmicMap(double value, double sourceMin, double sourceMax, double targetMin, double targetMax)
    {
        double ratio = Math.Log(value / sourceMin) / Math.Log(sourceMax / sourceMin);
        return targetMin + ratio * (targetMax - targetMin);
    }

    private static int ExponentialMap(double value, double sourceMin, double sourceMax, double targetMin, double targetMax)
    {
        double ratio = (value - sourceMin) / (sourceMax - sourceMin);
        double mapped = targetMin * Math.Pow(targetMax / targetMin, ratio);
        return (int)Math.Round(mapped);
    }
}

public sealed class PermanentRainTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is int rainTime && rainTime == RainTimeValues.Permanent;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? RainTimeValues.Permanent : RainTimeValues.OneYear;
}

public sealed class RainTimeDisplayConverter : IValueConverter
{
    private const double TicksPerSecond = 60.0;
    private const double SecondsPerDay = 86400.0;
    private const double DaysPerYear = 365.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int rainTime)
            return DependencyProperty.UnsetValue;

        if (rainTime == RainTimeValues.OneYear)
            return Language.tool_wp_rain_duration_one_year;
        if (rainTime == RainTimeValues.Permanent)
            return Language.tool_wp_rain_duration_permanent;
        if (rainTime <= 0)
            return Format(0, Language.tool_wp_rain_duration_minutes, culture);

        double seconds = rainTime / TicksPerSecond;
        string duration = seconds switch
        {
            < 60 => Format(seconds, Language.tool_wp_rain_duration_seconds, culture),
            < 3600 => Format(seconds / 60, Language.tool_wp_rain_duration_minutes, culture),
            < SecondsPerDay => Format(seconds / 3600, Language.tool_wp_rain_duration_hours, culture),
            < SecondsPerDay * DaysPerYear => Format(seconds / SecondsPerDay, Language.tool_wp_rain_duration_days, culture),
            _ => Format(seconds / SecondsPerDay / DaysPerYear, Language.tool_wp_rain_duration_years, culture),
        };

        return duration;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static string Format(double value, string format, CultureInfo culture) =>
        string.Format(culture, format, value);
}
