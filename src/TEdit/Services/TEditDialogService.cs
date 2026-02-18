using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TEdit.UI.Xaml.Dialog;

namespace TEdit.Services;

/// <summary>
/// WPF-UI based dialog service implementation using MessageBox.
/// Uses MessageBox instead of ContentDialog to avoid WPF airspace issues
/// with the DirectX/XNA rendering control.
/// </summary>
public class TEditDialogService : IDialogService
{
    /// <inheritdoc/>
    public async Task<DialogResponse> ShowMessageAsync(
        string message,
        string caption,
        DialogButton button = DialogButton.OK,
        DialogImage image = DialogImage.Information,
        CancellationToken cancellationToken = default)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = caption,
            Content = message,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow
        };

        ConfigureButtons(messageBox, button);

        var result = await messageBox.ShowDialogAsync(cancellationToken: cancellationToken);
        return MapMessageBoxResult(result, button);
    }

    /// <inheritdoc/>
    public async Task<DialogResponse> ShowExceptionAsync(
        string message,
        DialogImage image = DialogImage.Error,
        CancellationToken cancellationToken = default)
    {
        return await ShowMessageAsync(message, "Error", DialogButton.OK, image, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        var result = await ShowMessageAsync(message, title, DialogButton.YesNo, DialogImage.Question, cancellationToken);
        return result == DialogResponse.Yes;
    }

    /// <inheritdoc/>
    public async Task ShowAlertAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        await ShowMessageAsync(message, title, DialogButton.OK, DialogImage.Information, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DialogResponse> ShowWarningAsync(
        string title,
        string message,
        DialogButton button = DialogButton.OK,
        CancellationToken cancellationToken = default)
    {
        return await ShowMessageAsync(message, title, button, DialogImage.Warning, cancellationToken);
    }

    #region Private Helpers

    private static void ConfigureButtons(Wpf.Ui.Controls.MessageBox messageBox, DialogButton button)
    {
        switch (button)
        {
            case DialogButton.OK:
                messageBox.PrimaryButtonText = "OK";
                messageBox.IsPrimaryButtonEnabled = true;
                messageBox.IsSecondaryButtonEnabled = false;
                break;
            case DialogButton.OKCancel:
                messageBox.PrimaryButtonText = "OK";
                messageBox.CloseButtonText = "Cancel";
                messageBox.IsPrimaryButtonEnabled = true;
                messageBox.IsSecondaryButtonEnabled = false;
                break;
            case DialogButton.YesNo:
                messageBox.PrimaryButtonText = "Yes";
                messageBox.SecondaryButtonText = "No";
                messageBox.IsPrimaryButtonEnabled = true;
                messageBox.IsSecondaryButtonEnabled = true;
                break;
            case DialogButton.YesNoCancel:
                messageBox.PrimaryButtonText = "Yes";
                messageBox.SecondaryButtonText = "No";
                messageBox.CloseButtonText = "Cancel";
                messageBox.IsPrimaryButtonEnabled = true;
                messageBox.IsSecondaryButtonEnabled = true;
                break;
        }
    }

    private static DialogResponse MapMessageBoxResult(Wpf.Ui.Controls.MessageBoxResult result, DialogButton button)
    {
        return result switch
        {
            Wpf.Ui.Controls.MessageBoxResult.Primary => button == DialogButton.YesNo || button == DialogButton.YesNoCancel
                ? DialogResponse.Yes
                : DialogResponse.OK,
            Wpf.Ui.Controls.MessageBoxResult.Secondary => DialogResponse.No,
            Wpf.Ui.Controls.MessageBoxResult.None => button == DialogButton.OK
                ? DialogResponse.OK
                : DialogResponse.Cancel,
            _ => DialogResponse.None
        };
    }

    #endregion
}
