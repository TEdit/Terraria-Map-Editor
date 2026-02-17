using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TEdit.Render;

namespace TEdit.Converters;

/// <summary>
/// Converts an item ID (int or short) to a WriteableBitmap preview from the ItemPreviewCache.
/// </summary>
public class ItemIdToPreviewConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int itemId = value switch
        {
            int i => i,
            short s => s,
            _ => 0
        };

        if (itemId > 0)
        {
            var preview = ItemPreviewCache.GetPreview(itemId);
            if (preview != null)
                return preview;
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
