using System.Windows;
using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View;

public partial class SpecialTileView : UserControl
{
    public SpecialTileView()
    {
        InitializeComponent();
    }

    private void ViewNbt_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is WorldViewModel vm)
        {
            vm.SelectedTabIndex = (int)SidebarTab.NbtExplorer;
        }
    }
}
