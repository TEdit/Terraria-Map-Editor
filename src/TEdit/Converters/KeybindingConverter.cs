using System;
using System.Globalization;
using System.Windows.Data;

namespace TEdit.Converters;

/// <summary>
/// Looks up the keybinding for an action ID and appends it to a label or returns it standalone.
/// Use ConverterParameter to specify the action ID (e.g., "file.save").
/// When the input value is a string label, returns "Label (Ctrl+S)".
/// When the input value is not a string, returns just "Ctrl+S" (for InputGestureText).
/// </summary>
public class KeybindingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string actionId)
            return value;

        var bindings = App.Input?.Registry?.GetBindings(actionId);
        if (bindings == null || bindings.Count == 0)
            return value;

        var shortcut = bindings[0].ToString();

        if (value is string label && !string.IsNullOrEmpty(label))
            return $"{label} ({shortcut})";

        return shortcut;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
