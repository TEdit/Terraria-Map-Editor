using System.Windows;
using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View.Popups;

public partial class SettingsWindow : Window
{
    public SettingsWindow(WorldViewModel wvm)
    {
        InitializeComponent();
        DataContext = new SettingsViewModel(wvm);
    }

    private void BrowsePath_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: SettingItem setting }) return;

        var currentPath = setting.Value as string;
        if (string.IsNullOrWhiteSpace(currentPath))
            currentPath = DependencyChecker.PathToContent ?? "";

        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = setting.Description,
            SelectedPath = currentPath
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            setting.Value = dialog.SelectedPath;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
