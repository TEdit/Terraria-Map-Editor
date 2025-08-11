using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace TEdit.Converters
{
    public class FirstOrNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList list && list.Count > 0)
                return list[0];
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
