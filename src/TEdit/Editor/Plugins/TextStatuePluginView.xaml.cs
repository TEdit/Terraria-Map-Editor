using System.Windows;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Plugins;

/// <summary>
/// Interaction logic for TextStatuePluginView.xaml
/// </summary>
public partial class TextStatuePluginView : FluentWindow
{
    public TextStatusPluginViewModel ViewModel { get; private set; }
    public TextStatuePluginView()
    {
        InitializeComponent();
        ViewModel = new TextStatusPluginViewModel();
        this.DataContext = ViewModel;
    }

    private void Ok(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void SetLeft(object sender, RoutedEventArgs e)
    {
        tbStatue.TextAlignment = TextAlignment.Left;
        ViewModel.Justification = tbStatue.TextAlignment;
    }

    private void SetCenter(object sender, RoutedEventArgs e)
    {
        tbStatue.TextAlignment = TextAlignment.Center;
        ViewModel.Justification = tbStatue.TextAlignment;
    }

    private void SetRight(object sender, RoutedEventArgs e)
    {
        tbStatue.TextAlignment = TextAlignment.Right;
        ViewModel.Justification = tbStatue.TextAlignment;
    }
}
