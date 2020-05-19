/* 
Copyright (c) 2011 BinaryConstruct
 
This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using System.Windows;
using System.Windows.Controls;

namespace TEdit.UI.Xaml
{
    public class MeasurableCanvas : Canvas
    {
        protected override Size MeasureOverride(Size constraint)
        {
            var availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            double maxHeight = 0;
            double maxWidth = 0;

            foreach (UIElement element in InternalChildren)
            {
                if (element != null)
                {
                    element.Measure(availableSize);
                    double left = GetLeft(element);
                    double top = GetTop(element);
                    left += element.DesiredSize.Width;
                    top += element.DesiredSize.Height;

                    maxWidth = maxWidth < left ? left : maxWidth;
                    maxHeight = maxHeight < top ? top : maxHeight;
                }
            }
            return new Size { Height = maxHeight, Width = maxWidth };
        }
    }
}