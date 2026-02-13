using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using TEdit.Terraria;
using TEdit.Terraria.Objects;

namespace TEdit.Converters;

/// <summary>
/// Converts a banner TallyIndex to a WriteableBitmap preview from the tile 91 (Banner) sprite sheet.
/// </summary>
public class BannerTallyToPreviewConverter : IValueConverter
{
    private const int BannerTileId = 91;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int tallyIndex && tallyIndex >= 0)
        {
            var bannerSheet = WorldConfiguration.Sprites2.FirstOrDefault(s => s.Tile == BannerTileId);
            if (bannerSheet != null)
            {
                var style = bannerSheet.Styles.FirstOrDefault(s => s.Style == tallyIndex);
                if (style is SpriteItemPreview preview)
                {
                    return preview.Preview;
                }
            }
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
