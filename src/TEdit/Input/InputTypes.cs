using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace TEdit.Input;

/// <summary>
/// Mouse buttons that can be bound to actions.
/// </summary>
public enum TEditMouseButton
{
    None = 0,
    Left = 1,
    Right = 2,
    Middle = 3,
    XButton1 = 4,
    XButton2 = 5
}

/// <summary>
/// Mouse wheel direction.
/// </summary>
public enum MouseWheelDirection
{
    None = 0,
    Up = 1,
    Down = 2
}

/// <summary>
/// Type of input.
/// </summary>
public enum InputType
{
    None,
    Keyboard,
    Mouse,
    MouseWheel
}

/// <summary>
/// Scope where an action is valid.
/// </summary>
public enum InputScope
{
    /// <summary>Application-level actions (file, clipboard, history) - handled by MainWindow.</summary>
    Application,
    /// <summary>Editor-level actions (tools, zoom, pan) - handled by WorldRenderXna/tools.</summary>
    Editor
}

/// <summary>
/// Unified representation of any input binding (keyboard, mouse button, or mouse wheel).
/// Supports modifier keys (Ctrl, Shift, Alt) with any input type.
/// </summary>
public readonly struct InputBinding : IEquatable<InputBinding>
{
    public Key Key { get; init; }
    public ModifierKeys Modifiers { get; init; }
    public TEditMouseButton MouseButton { get; init; }
    public MouseWheelDirection MouseWheel { get; init; }

    public InputType Type =>
        Key != Key.None ? InputType.Keyboard :
        MouseButton != TEditMouseButton.None ? InputType.Mouse :
        MouseWheel != MouseWheelDirection.None ? InputType.MouseWheel :
        InputType.None;

    public bool IsValid => Type != InputType.None;

    /// <summary>Creates a keyboard binding.</summary>
    public static InputBinding Keyboard(Key key, ModifierKeys modifiers = ModifierKeys.None)
        => new() { Key = key, Modifiers = modifiers };

    /// <summary>Creates a mouse button binding.</summary>
    public static InputBinding Mouse(TEditMouseButton button, ModifierKeys modifiers = ModifierKeys.None)
        => new() { MouseButton = button, Modifiers = modifiers };

    /// <summary>Creates a mouse wheel binding.</summary>
    public static InputBinding Wheel(MouseWheelDirection direction, ModifierKeys modifiers = ModifierKeys.None)
        => new() { MouseWheel = direction, Modifiers = modifiers };

    public override string ToString()
    {
        if (!IsValid) return "None";

        var parts = new List<string>();

        if (Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");

        switch (Type)
        {
            case InputType.Keyboard:
                parts.Add(FormatKey(Key));
                break;
            case InputType.Mouse:
                parts.Add(FormatMouseButton(MouseButton));
                break;
            case InputType.MouseWheel:
                parts.Add(MouseWheel == MouseWheelDirection.Up ? "WheelUp" : "WheelDown");
                break;
        }

        return string.Join("+", parts);
    }

    private static string FormatKey(Key key)
    {
        return key switch
        {
            Key.OemPlus => "Plus",
            Key.OemMinus => "Minus",
            Key.OemComma => "Comma",
            Key.OemPeriod => "Period",
            Key.OemQuestion => "Slash",
            Key.OemOpenBrackets => "LeftBracket",
            Key.OemCloseBrackets => "RightBracket",
            Key.OemSemicolon => "Semicolon",
            Key.OemQuotes => "Quote",
            Key.OemBackslash => "Backslash",
            Key.OemTilde => "Tilde",
            _ => key.ToString()
        };
    }

    private static string FormatMouseButton(TEditMouseButton button)
    {
        return button switch
        {
            TEditMouseButton.Left => "LeftClick",
            TEditMouseButton.Right => "RightClick",
            TEditMouseButton.Middle => "MiddleClick",
            TEditMouseButton.XButton1 => "Mouse4",
            TEditMouseButton.XButton2 => "Mouse5",
            _ => button.ToString()
        };
    }

    /// <summary>Parse a binding string like "Ctrl+C", "Shift+LeftClick", "WheelUp".</summary>
    public static InputBinding Parse(string str)
    {
        if (string.IsNullOrWhiteSpace(str) || str.Equals("None", StringComparison.OrdinalIgnoreCase))
            return default;

        var parts = str.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var modifiers = ModifierKeys.None;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            var mod = parts[i].ToLowerInvariant();
            modifiers |= mod switch
            {
                "ctrl" or "control" => ModifierKeys.Control,
                "shift" => ModifierKeys.Shift,
                "alt" => ModifierKeys.Alt,
                _ => ModifierKeys.None
            };
        }

        var input = parts[^1];

        // Mouse wheel
        if (input.Equals("WheelUp", StringComparison.OrdinalIgnoreCase))
            return Wheel(MouseWheelDirection.Up, modifiers);
        if (input.Equals("WheelDown", StringComparison.OrdinalIgnoreCase))
            return Wheel(MouseWheelDirection.Down, modifiers);

        // Mouse buttons
        if (input.Equals("LeftClick", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("MouseLeft", StringComparison.OrdinalIgnoreCase))
            return Mouse(TEditMouseButton.Left, modifiers);
        if (input.Equals("RightClick", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("MouseRight", StringComparison.OrdinalIgnoreCase))
            return Mouse(TEditMouseButton.Right, modifiers);
        if (input.Equals("MiddleClick", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("MouseMiddle", StringComparison.OrdinalIgnoreCase))
            return Mouse(TEditMouseButton.Middle, modifiers);
        if (input.Equals("Mouse4", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("XButton1", StringComparison.OrdinalIgnoreCase))
            return Mouse(TEditMouseButton.XButton1, modifiers);
        if (input.Equals("Mouse5", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("XButton2", StringComparison.OrdinalIgnoreCase))
            return Mouse(TEditMouseButton.XButton2, modifiers);

        // Keyboard - handle special key names
        var keyName = input switch
        {
            "Plus" => "OemPlus",
            "Minus" => "OemMinus",
            "Comma" => "OemComma",
            "Period" => "OemPeriod",
            "Slash" => "OemQuestion",
            "LeftBracket" => "OemOpenBrackets",
            "RightBracket" => "OemCloseBrackets",
            "Semicolon" => "OemSemicolon",
            "Quote" => "OemQuotes",
            "Backslash" => "OemBackslash",
            "Tilde" => "OemTilde",
            _ => input
        };

        if (Enum.TryParse<Key>(keyName, ignoreCase: true, out var key) && key != Key.None)
            return Keyboard(key, modifiers);

        return default;
    }

    public bool Equals(InputBinding other) =>
        Key == other.Key &&
        Modifiers == other.Modifiers &&
        MouseButton == other.MouseButton &&
        MouseWheel == other.MouseWheel;

    public override bool Equals(object? obj) => obj is InputBinding other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Key, Modifiers, MouseButton, MouseWheel);

    public static bool operator ==(InputBinding left, InputBinding right) => left.Equals(right);
    public static bool operator !=(InputBinding left, InputBinding right) => !left.Equals(right);
}
