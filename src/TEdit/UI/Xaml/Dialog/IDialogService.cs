using System.Threading;
using System.Threading.Tasks;

namespace TEdit.UI.Xaml.Dialog;

/// <summary>
/// Async-first dialog service interface for WPF-UI ContentDialog.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a message dialog asynchronously.
    /// </summary>
    Task<DialogResponse> ShowMessageAsync(
        string message,
        string caption,
        DialogButton button = DialogButton.OK,
        DialogImage image = DialogImage.Information,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Shows an exception/error dialog asynchronously.
    /// </summary>
    Task<DialogResponse> ShowExceptionAsync(
        string message,
        DialogImage image = DialogImage.Error,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Shows a Yes/No confirmation dialog asynchronously.
    /// </summary>
    Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Shows a simple OK alert dialog asynchronously.
    /// </summary>
    Task ShowAlertAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Shows a warning dialog asynchronously.
    /// </summary>
    Task<DialogResponse> ShowWarningAsync(
        string title,
        string message,
        DialogButton button = DialogButton.OK,
        CancellationToken cancellationToken = default);
}
