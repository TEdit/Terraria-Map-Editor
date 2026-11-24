using System;
using System.Windows;

namespace TEdit.UI;

internal static class WindowSizing
{
    /// <summary>SizeToContent leaves unpainted borders on FluentWindow; size from measured content instead.</summary>
    /// <param name="heightOnly">Keep the XAML Width and measure the content at that width.</param>
    public static void SizeFromContent(Window window, bool heightOnly = false)
    {
        if (window.Content is not FrameworkElement root)
            return;

        double availableWidth = heightOnly && !double.IsNaN(window.Width) ? window.Width : double.PositiveInfinity;
        root.Measure(new Size(availableWidth, double.PositiveInfinity));
        var natural = root.DesiredSize;
        if (!heightOnly)
            window.Width = Math.Clamp(natural.Width, window.MinWidth, SystemParameters.WorkArea.Width);
        window.Height = Math.Clamp(natural.Height, window.MinHeight, SystemParameters.WorkArea.Height);
    }
}
