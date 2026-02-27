using System.Windows;
using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View.Sidebar;

public partial class PaletteSidebarView : UserControl
{
    public PaletteSidebarView()
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

    private async void ReplaceAll_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is WorldViewModel vm)
        {
            if (!vm.ReplaceAll())
                await ShowMaskWarningAsync();
        }
    }

    private async void ReplaceSelection_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is WorldViewModel vm)
        {
            if (!vm.ReplaceSelection())
                await ShowMaskWarningAsync();
        }
    }

    private static async System.Threading.Tasks.Task ShowMaskWarningAsync()
    {
        await App.DialogService.ShowWarningAsync(
            "Replace",
            "Enable at least one mask to use replace. Masks define which tiles to match.");
    }
}
