using System.Windows;
using System.Windows.Input;
using TEdit.Input;
using Wpf.Ui.Controls;

namespace TEdit.View.Popups;

public partial class InputCaptureWindow : FluentWindow
{
    public TEdit.Input.InputBinding? CapturedInput { get; private set; }

    public InputCaptureWindow()
    {
        InitializeComponent();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Ignore modifier-only presses
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LeftAlt || key == Key.RightAlt ||
            key == Key.LWin || key == Key.RWin)
        {
            return;
        }

        // Escape cancels
        if (key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
        {
            DialogResult = false;
            Close();
            return;
        }

        CapturedInput = TEdit.Input.InputBinding.Keyboard(key, Keyboard.Modifiers);
        CapturedText.Text = CapturedInput.Value.ToString();

        e.Handled = true;
        DialogResult = true;
        Close();
    }

    private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        TEditMouseButton button = e.ChangedButton switch
        {
            MouseButton.Left => TEditMouseButton.Left,
            MouseButton.Right => TEditMouseButton.Right,
            MouseButton.Middle => TEditMouseButton.Middle,
            MouseButton.XButton1 => TEditMouseButton.XButton1,
            MouseButton.XButton2 => TEditMouseButton.XButton2,
            _ => TEditMouseButton.None
        };

        if (button == TEditMouseButton.None) return;

        CapturedInput = TEdit.Input.InputBinding.Mouse(button, Keyboard.Modifiers);
        CapturedText.Text = CapturedInput.Value.ToString();

        e.Handled = true;
        DialogResult = true;
        Close();
    }

    private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var direction = e.Delta > 0 ? MouseWheelDirection.Up : MouseWheelDirection.Down;
        CapturedInput = TEdit.Input.InputBinding.Wheel(direction, Keyboard.Modifiers);
        CapturedText.Text = CapturedInput.Value.ToString();

        e.Handled = true;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
