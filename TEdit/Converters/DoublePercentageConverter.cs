using System;
using System.Windows.Data;

namespace TEdit.Converters
{
    public class DoublePercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                     System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value.ToString())) return 0;

            if (value.GetType() == typeof(double)) return (double)value;

            if (value.GetType() == typeof(decimal)) return (decimal)value;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value.ToString())) return 0;

            var trimmedValue = value.ToString().TrimEnd(new char[] { '%' });

            if (targetType == typeof(double))
            {
                double result;
                if (double.TryParse(trimmedValue, out result))
                    return result / 100;
                else
                    return value ;
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
    }
}