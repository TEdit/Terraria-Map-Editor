using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace TEdit;

public partial class TestWindow : FluentWindow
{
    private readonly ContentDialogService _dialogService = new();
    private readonly SnackbarService _snackbarService = new();

    public TestWindow()
    {
        InitializeComponent();
        // Loaded handler is in XAML
    }

    private void TestWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _dialogService.SetDialogHost(RootContentDialogHost);
        _snackbarService.SetSnackbarPresenter(SnackbarPresenter);
    }

    private async void ShowDialogButton_Click(object sender, RoutedEventArgs e)
    {
        var result = await _dialogService.ShowSimpleDialogAsync(
            new SimpleContentDialogCreateOptions
            {
                Title = "Test Dialog",
                Content = "This is a test dialog. Does it appear?",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                CloseButtonText = "Cancel"
            });

        _snackbarService.Show("Dialog Result", $"You selected: {result}", ControlAppearance.Success);
    }

    private void ShowSnackbarButton_Click(object sender, RoutedEventArgs e)
    {
        _snackbarService.Show("Test Snackbar", "This is a test snackbar message!", ControlAppearance.Info);
    }

    private async void ShowMessageBoxButton_Click(object sender, RoutedEventArgs e)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox
        {
            Title = "WPF-UI MessageBox",
            Content = "This is a WPF-UI MessageBox. Does it appear?",
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "No",
            CloseButtonText = "Cancel"
        };

        var result = await messageBox.ShowDialogAsync();
        _snackbarService.Show("MessageBox Result", $"You selected: {result}", ControlAppearance.Success);
    }
}
