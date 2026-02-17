using System;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace TEdit.Services;

/// <summary>
/// WPF-UI Snackbar service for non-blocking notifications.
/// </summary>
public class TEditSnackbarService
{
    private SnackbarPresenter? _snackbarPresenter;
    private Snackbar? _snackbar;
    private Dispatcher? _cachedDispatcher;

    /// <summary>
    /// Gets the dispatcher, lazily initializing from Application.Current.
    /// </summary>
    private Dispatcher Dispatcher => _cachedDispatcher ??= Application.Current?.Dispatcher ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;

    /// <summary>
    /// Default timeout for info/success messages (3 seconds).
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Timeout for warning/error messages (5 seconds).
    /// </summary>
    public TimeSpan WarningTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public TEditSnackbarService()
    {
        // Dispatcher is lazily initialized to avoid issues during static construction
    }

    /// <summary>
    /// Sets the snackbar presenter from MainWindow.
    /// Must be called after MainWindow is loaded.
    /// </summary>
    public void SetSnackbarPresenter(SnackbarPresenter presenter)
    {
        _snackbarPresenter = presenter;
        _snackbar = new Snackbar(_snackbarPresenter);
    }

    /// <summary>
    /// Shows an info notification (blue).
    /// </summary>
    public void ShowInfo(string message, string? title = null)
    {
        Show(title ?? "Info", message, ControlAppearance.Info, SymbolRegular.Info24, DefaultTimeout);
    }

    /// <summary>
    /// Shows a success notification (green).
    /// </summary>
    public void ShowSuccess(string message, string? title = null)
    {
        Show(title ?? "Success", message, ControlAppearance.Success, SymbolRegular.CheckmarkCircle24, DefaultTimeout);
    }

    /// <summary>
    /// Shows a warning notification (yellow/orange).
    /// </summary>
    public void ShowWarning(string message, string? title = null)
    {
        Show(title ?? "Warning", message, ControlAppearance.Caution, SymbolRegular.Warning24, WarningTimeout);
    }

    /// <summary>
    /// Shows an error notification (red).
    /// </summary>
    public void ShowError(string message, string? title = null)
    {
        Show(title ?? "Error", message, ControlAppearance.Danger, SymbolRegular.ErrorCircle24, WarningTimeout);
    }

    /// <summary>
    /// Shows a custom notification.
    /// </summary>
    public void Show(
        string title,
        string message,
        ControlAppearance appearance = ControlAppearance.Secondary,
        SymbolRegular icon = SymbolRegular.Info24,
        TimeSpan? timeout = null)
    {
        if (_snackbar == null || _snackbarPresenter == null)
        {
            // Fallback: log to debug output if snackbar not initialized
            System.Diagnostics.Debug.WriteLine($"[Snackbar] {title}: {message}");
            return;
        }

        Dispatcher.InvokeAsync(() =>
        {
            _snackbar.SetCurrentValue(Snackbar.TitleProperty, title);
            _snackbar.SetCurrentValue(System.Windows.Controls.ContentControl.ContentProperty, message);
            _snackbar.SetCurrentValue(Snackbar.AppearanceProperty, appearance);
            _snackbar.SetCurrentValue(Snackbar.IconProperty, new SymbolIcon(icon));
            _snackbar.SetCurrentValue(Snackbar.TimeoutProperty, (int)(timeout ?? DefaultTimeout).TotalMilliseconds);
            _snackbar.Show();
        });
    }
}
