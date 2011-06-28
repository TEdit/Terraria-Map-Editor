using System.Windows;
using System.Windows.Controls;

namespace TEdit.Controls
{
    public class MeasurableCanvas : Canvas
    {
        protected override Size MeasureOverride(Size constraint)
        {
            var availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            double maxHeight = 0;
            double maxWidth = 0;

            foreach (UIElement element in base.InternalChildren)
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
            return new Size {Height = maxHeight, Width = maxWidth};
        }
    }
}