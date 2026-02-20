using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TEdit.Render;

namespace TEdit.Converters;

/// <summary>
/// Multi-value converter that resolves item preview bitmaps for both vanilla and mod items.
/// Bindings: [0] = NetId (int), [1] = IsModItem (bool), [2] = ModName (string), [3] = ModItemName (string)
/// </summary>
public class ItemPreviewMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 4)
            return DependencyProperty.UnsetValue;

        // Check mod item first
        bool isModItem = values[1] is bool b && b;
        if (isModItem)
        {
            string modName = values[2] as string;
            string modItemName = values[3] as string;
            var modPreview = ModItemPreviewCache.GetPreview(modName, modItemName);
            if (modPreview != null)
                return modPreview;
        }

        // Fall back to vanilla preview
        int itemId = values[0] switch
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

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
