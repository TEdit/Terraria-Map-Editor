using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TEdit.UI.Xaml.Dialog;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace TEdit.Services;

/// <summary>
/// WPF-UI based dialog service implementation.
/// </summary>
public class TEditDialogService : IDialogService
{
    private ContentPresenter? _dialogHost;
    private Dispatcher? _cachedDispatcher;

    /// <summary>
    /// Gets the dispatcher, lazily initializing from Application.Current.
    /// </summary>
    private Dispatcher Dispatcher => _cachedDispatcher ??= Application.Current?.Dispatcher ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;

    public TEditDialogService()
    {
        // Dispatcher is lazily initialized to avoid issues during static construction
    }

    /// <summary>
    /// Sets the dialog host (ContentPresenter from MainWindow).
    /// Must be called after MainWindow is loaded.
    /// </summary>
    public void SetDialogHost(ContentPresenter dialogHost)
    {
        _dialogHost = dialogHost;
    }

    #region IDialogService Implementation (Sync - for backwards compatibility)

    public DialogResponse ShowException(string message, DialogImage image = DialogImage.Error)
    {
        return ShowMessage(message, "Error", DialogButton.OK, image);
    }

    public DialogResponse ShowMessage(string message, string caption, DialogButton button, DialogImage image)
    {
        if (_dialogHost == null)
        {
            // Fallback to standard MessageBox if dialog host not set
            var wpfButton = MapButton(button);
            var wpfImage = MapImage(image);
            var result = System.Windows.MessageBox.Show(message, caption, wpfButton, wpfImage);
            return MapResult(result);
        }

        // Run async dialog synchronously on UI thread
        return Dispatcher.Invoke(() =>
        {
            var task = ShowMessageInternalAsync(message, caption, button, image, CancellationToken.None);
            return task.GetAwaiter().GetResult();
        });
    }

    public DialogResponse ShowMessageOverlay(string message, string caption, DialogButton button, DialogImage image)
    {
        // Same as ShowMessage for WPF-UI ContentDialog (it's always an overlay)
        return ShowMessage(message, caption, button, image);
    }

    #endregion

    #region Async Methods (Recommended)

    /// <summary>
    /// Shows a message dialog asynchronously.
    /// </summary>
    public async Task<DialogResponse> ShowMessageAsync(
        string message,
        string caption,
        DialogButton button = DialogButton.OK,
        DialogImage image = DialogImage.Information,
        CancellationToken cancellationToken = default)
    {
        if (_dialogHost == null)
        {
            // Fallback to standard MessageBox
            return await Dispatcher.InvokeAsync(() =>
            {
                var wpfButton = MapButton(button);
                var wpfImage = MapImage(image);
                var result = System.Windows.MessageBox.Show(message, caption, wpfButton, wpfImage);
                return MapResult(result);
            });
        }

        return await Dispatcher.InvokeAsync(async () =>
        {
            return await ShowMessageInternalAsync(message, caption, button, image, cancellationToken);
        }).Task.Unwrap();
    }

    /// <summary>
    /// Shows an error dialog with optional exception details.
    /// </summary>
    public async Task<DialogResponse> ShowErrorAsync(
        string title,
        string message,
        Exception? exception = null,
        CancellationToken cancellationToken = default)
    {
        var fullMessage = exception != null
            ? $"{message}\n\nDetails: {exception.Message}"
            : message;

        return await ShowMessageAsync(fullMessage, title, DialogButton.OK, DialogImage.Error, cancellationToken);
    }

    /// <summary>
    /// Shows a confirmation dialog (Yes/No).
    /// </summary>
    public async Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        var result = await ShowMessageAsync(message, title, DialogButton.YesNo, DialogImage.Question, cancellationToken);
        return result == DialogResponse.Yes;
    }

    /// <summary>
    /// Shows a simple alert dialog (OK only).
    /// </summary>
    public async Task ShowAlertAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        await ShowMessageAsync(message, title, DialogButton.OK, DialogImage.Information, cancellationToken);
    }

    /// <summary>
    /// Shows a warning dialog.
    /// </summary>
    public async Task<DialogResponse> ShowWarningAsync(
        string title,
        string message,
        DialogButton button = DialogButton.OK,
        CancellationToken cancellationToken = default)
    {
        return await ShowMessageAsync(message, title, button, DialogImage.Warning, cancellationToken);
    }

    #endregion

    #region Private Implementation

    private async Task<DialogResponse> ShowMessageInternalAsync(
        string message,
        string caption,
        DialogButton button,
        DialogImage image,
        CancellationToken cancellationToken)
    {
        var dialog = new ContentDialog
        {
            Title = caption,
            Content = message,
            DialogHost = _dialogHost
        };

        // Configure buttons based on DialogButton enum
        switch (button)
        {
            case DialogButton.OK:
                dialog.CloseButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Close;
                break;
            case DialogButton.OKCancel:
                dialog.PrimaryButtonText = "OK";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Primary;
                break;
            case DialogButton.YesNo:
                dialog.PrimaryButtonText = "Yes";
                dialog.SecondaryButtonText = "No";
                dialog.DefaultButton = ContentDialogButton.Primary;
                break;
            case DialogButton.YesNoCancel:
                dialog.PrimaryButtonText = "Yes";
                dialog.SecondaryButtonText = "No";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Primary;
                break;
        }

        // Set icon appearance based on image type
        switch (image)
        {
            case DialogImage.Error:
            case DialogImage.Stop:
            case DialogImage.Hand:
                dialog.PrimaryButtonAppearance = ControlAppearance.Danger;
                break;
            case DialogImage.Warning:
            case DialogImage.Exclamation:
                dialog.PrimaryButtonAppearance = ControlAppearance.Caution;
                break;
            case DialogImage.Information:
            case DialogImage.Asterisk:
                dialog.PrimaryButtonAppearance = ControlAppearance.Info;
                break;
            case DialogImage.Question:
                dialog.PrimaryButtonAppearance = ControlAppearance.Primary;
                break;
        }

        var result = await dialog.ShowAsync(cancellationToken);

        return MapContentDialogResult(result, button);
    }

    private static DialogResponse MapContentDialogResult(ContentDialogResult result, DialogButton button)
    {
        return result switch
        {
            ContentDialogResult.Primary => button == DialogButton.YesNo || button == DialogButton.YesNoCancel
                ? DialogResponse.Yes
                : DialogResponse.OK,
            ContentDialogResult.Secondary => DialogResponse.No,
            ContentDialogResult.None => button == DialogButton.OK
                ? DialogResponse.OK
                : DialogResponse.Cancel,
            _ => DialogResponse.None
        };
    }

    private static System.Windows.MessageBoxButton MapButton(DialogButton button)
    {
        return button switch
        {
            DialogButton.OK => System.Windows.MessageBoxButton.OK,
            DialogButton.OKCancel => System.Windows.MessageBoxButton.OKCancel,
            DialogButton.YesNo => System.Windows.MessageBoxButton.YesNo,
            DialogButton.YesNoCancel => System.Windows.MessageBoxButton.YesNoCancel,
            _ => System.Windows.MessageBoxButton.OK
        };
    }

    private static MessageBoxImage MapImage(DialogImage image)
    {
        return image switch
        {
            DialogImage.Asterisk => MessageBoxImage.Asterisk,
            DialogImage.Error => MessageBoxImage.Error,
            DialogImage.Exclamation => MessageBoxImage.Exclamation,
            DialogImage.Hand => MessageBoxImage.Hand,
            DialogImage.Information => MessageBoxImage.Information,
            DialogImage.None => MessageBoxImage.None,
            DialogImage.Question => MessageBoxImage.Question,
            DialogImage.Stop => MessageBoxImage.Stop,
            DialogImage.Warning => MessageBoxImage.Warning,
            _ => MessageBoxImage.None
        };
    }

    private static DialogResponse MapResult(System.Windows.MessageBoxResult result)
    {
        return result switch
        {
            System.Windows.MessageBoxResult.Cancel => DialogResponse.Cancel,
            System.Windows.MessageBoxResult.No => DialogResponse.No,
            System.Windows.MessageBoxResult.None => DialogResponse.None,
            System.Windows.MessageBoxResult.OK => DialogResponse.OK,
            System.Windows.MessageBoxResult.Yes => DialogResponse.Yes,
            _ => DialogResponse.None
        };
    }

    #endregion
}
