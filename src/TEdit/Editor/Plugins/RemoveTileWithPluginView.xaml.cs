using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Plugins;

/// <summary>
/// Interaction logic for RemoveTileWithPluginView.xaml
/// </summary>
public partial class RemoveTileWithPluginView : FluentWindow
{
    public string BlockToRemove { get; private set; }
    public string WallToRemove { get; private set; }
    public string SpriteToRemove { get; private set; }
    public RemoveTileWithPluginView()
    {
        InitializeComponent();
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void RemoveButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        BlockToRemove = BlockRemove.Text;
        WallToRemove = WallRemove.Text;
        Close();
    }
}
