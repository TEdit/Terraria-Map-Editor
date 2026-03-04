using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TEdit.View;

public partial class SelectionToolbarView : UserControl
{
    public SelectionToolbarView()
    {
        InitializeComponent();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }
    }
}
