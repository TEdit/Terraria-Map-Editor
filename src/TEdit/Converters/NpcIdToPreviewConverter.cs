using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TEdit.Render;
using TEdit.Terraria;

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

/// <summary>
/// Converts (NPC ID, variant name) to a variant-specific preview bitmap.
/// Use with MultiBinding: values[0] = SpriteId (int), values[1] = variant name (string).
/// Falls back to the default NPC preview if no variant preview is available.
/// </summary>
public class NpcVariantPreviewConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is int npcId && values[1] is string variantName)
        {
            if (WorldConfiguration.NpcById.TryGetValue(npcId, out var npcData) && npcData.Variants != null)
            {
                int variantIndex = npcData.Variants.IndexOf(variantName);
                if (variantIndex >= 0)
                {
                    return NpcPreviewCache.GetVariantPreview(npcId, variantIndex)
                        ?? NpcPreviewCache.GetPreview(npcId);
                }
            }
            return NpcPreviewCache.GetPreview(npcId);
        }

        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return null;
    }
}
