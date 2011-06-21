// -----------------------------------------------------------------------
// <copyright file="ThicknessMultiConverter.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace TEditWPF.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;
    using System.Windows;

    public class ThicknessMultiConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double left = System.Convert.ToDouble(values[0]);
            double top = System.Convert.ToDouble(values[1]);
            double right = System.Convert.ToDouble(values[2]);
            double bottom = System.Convert.ToDouble(values[3]);
            return new Thickness(left, top, right, bottom);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            Thickness thickness = (Thickness)value;
            return new object[]
        {
            thickness.Left,
            thickness.Top,
            thickness.Right,
            thickness.Bottom
        };
        }

        #endregion
    }
}
