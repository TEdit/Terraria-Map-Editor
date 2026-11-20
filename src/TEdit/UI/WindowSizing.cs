using System;
using System.Windows;

namespace TEdit.UI;

internal static class WindowSizing
{
    /// <summary>SizeToContent leaves unpainted borders on FluentWindow; size from measured content instead.</summary>
    public static void SizeFromContent(Window window)
    {
        if (window.Content is not FrameworkElement root)
            return;

        root.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var natural = root.DesiredSize;
        window.Width = Math.Clamp(natural.Width, window.MinWidth, SystemParameters.WorkArea.Width);
        window.Height = Math.Clamp(natural.Height, window.MinHeight, SystemParameters.WorkArea.Height);
    }
}
