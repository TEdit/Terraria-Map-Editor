using System.Windows;
using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View.Sidebar;

public partial class MaskSidebarView : UserControl
{
    public MaskSidebarView()
    {
        InitializeComponent();
    }

    private void ClearAll_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is WorldViewModel vm)
        {
            vm.MaskSettings.ClearAll();
        }
    }
}
