using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TEdit.Converters;

/// <summary>
/// Converts Terraria's internal time (double) + DayTime (bool) to a 12-hour clock string.
/// Uses the exact algorithm from Terraria's Main.cs CLI time command.
///
/// Terraria time system:
///   dayLength = 54000 ticks (4:30 AM to 7:30 PM)
///   nightLength = 32400 ticks (7:30 PM to 4:30 AM)
///   totalCycle = 86400 ticks
/// </summary>
public class TerrariaTimeConverter : IMultiValueConverter
{
    public const double DayLength = 54000.0;
    public const double NightLength = 32400.0;
    public const double TotalCycle = 86400.0;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] is not double time ||
            values[1] is not bool dayTime)
            return string.Empty;

        return FormatTime(time, dayTime);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return [Binding.DoNothing, Binding.DoNothing];

        if (!TryParseTime(text, out double time, out bool dayTime))
            return [Binding.DoNothing, Binding.DoNothing];

        return [time, dayTime];
    }

    /// <summary>
    /// Formats Terraria time exactly as the game does (from Main.cs CLI time command).
    /// </summary>
    public static string FormatTime(double time, bool dayTime)
    {
        if (!dayTime)
            time += DayLength;

        double hours = time / TotalCycle * 24.0 - 7.5 - 12.0;
        if (hours < 0.0)
            hours += 24.0;

        string period = hours >= 12.0 ? "PM" : "AM";

        int displayHour = (int)hours;
        int minutes = (int)((hours - displayHour) * 60.0);

        if (displayHour > 12)
            displayHour -= 12;
        if (displayHour == 0)
            displayHour = 12;

        return $"{displayHour}:{minutes:D2} {period}";
    }

    /// <summary>
    /// Parses a time string like "4:30 AM" back into Terraria time + dayTime.
    /// </summary>
    public static bool TryParseTime(string text, out double time, out bool dayTime)
    {
        time = 0;
        dayTime = true;

        text = text.Trim();

        // Try parsing with AM/PM
        if (DateTime.TryParse(text, CultureInfo.InvariantCulture,
                DateTimeStyles.NoCurrentDateDefault, out var dt))
        {
            return ConvertClockToTerrariaTime(dt.Hour, dt.Minute, out time, out dayTime);
        }

        // Try "H:MM AM/PM" manually for partial input
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 1)
        {
            var timeParts = parts[0].Split(':');
            if (timeParts.Length >= 1 && int.TryParse(timeParts[0], out int hour))
            {
                int minute = 0;
                if (timeParts.Length >= 2)
                    int.TryParse(timeParts[1], out minute);

                // Check for AM/PM suffix
                bool isPM = false;
                bool hasAmPm = false;
                string suffix = parts.Length >= 2 ? parts[1] : "";
                if (suffix.Equals("PM", StringComparison.OrdinalIgnoreCase))
                {
                    isPM = true;
                    hasAmPm = true;
                }
                else if (suffix.Equals("AM", StringComparison.OrdinalIgnoreCase))
                {
                    hasAmPm = true;
                }

                if (hasAmPm)
                {
                    // Convert 12-hour to 24-hour
                    if (hour == 12) hour = isPM ? 12 : 0;
                    else if (isPM) hour += 12;
                }

                return ConvertClockToTerrariaTime(hour, minute, out time, out dayTime);
            }
        }

        return false;
    }

    private static bool ConvertClockToTerrariaTime(int hour24, int minute, out double time, out bool dayTime)
    {
        // Reverse the formula: hours = time / 86400 * 24 - 19.5
        // time = (hours + 19.5) / 24 * 86400
        double hours = hour24 + minute / 60.0;
        double totalTicks = (hours + 19.5) / 24.0 * TotalCycle;

        // Wrap around
        if (totalTicks >= TotalCycle)
            totalTicks -= TotalCycle;

        if (totalTicks < DayLength)
        {
            dayTime = true;
            time = totalTicks;
        }
        else
        {
            dayTime = false;
            time = totalTicks - DayLength;
        }

        return true;
    }

    /// <summary>
    /// Converts continuous slider value (0–86400) to split Time + DayTime.
    /// 0–54000 = daytime, 54000–86400 = nighttime.
    /// </summary>
    public static void ContinuousToSplit(double continuous, out double time, out bool dayTime)
    {
        if (continuous < DayLength)
        {
            dayTime = true;
            time = continuous;
        }
        else
        {
            dayTime = false;
            time = continuous - DayLength;
        }
    }

    /// <summary>
    /// Converts split Time + DayTime to continuous slider value (0–86400).
    /// </summary>
    public static double SplitToContinuous(double time, bool dayTime)
    {
        return dayTime ? time : time + DayLength;
    }
}

/// <summary>
/// MultiValueConverter for a continuous 0–86400 slider bound to Time (double) + DayTime (bool).
/// </summary>
public class ContinuousTimeSliderConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] is not double time ||
            values[1] is not bool dayTime)
            return 0.0;

        return TerrariaTimeConverter.SplitToContinuous(time, dayTime);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        if (value is not double continuous)
            return [Binding.DoNothing, Binding.DoNothing];

        TerrariaTimeConverter.ContinuousToSplit(continuous, out double time, out bool dayTime);
        return [time, dayTime];
    }
}
