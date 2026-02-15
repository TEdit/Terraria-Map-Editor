using System.Windows;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Plugins;

/// <summary>
/// Interaction logic for ReplaceAllPluginView.xaml
/// </summary>
public partial class ReplaceAllPluginView : FluentWindow
{
    public ReplaceAllPluginView()
    {
        InitializeComponent();
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ReplaceButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
