using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TEdit.Render;

namespace TEdit.Converters;

/// <summary>
/// Converts an NPC ID (int) to a WriteableBitmap preview from the NpcPreviewCache.
/// </summary>
public class NpcIdToPreviewConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int npcId)
        {
            return NpcPreviewCache.GetPreview(npcId);
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
