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
        if (DataContext is not WorldViewModel vm) return;

        vm.SelectedTabIndex = (int)SidebarTab.NbtExplorer;

        // Navigate to the currently selected entity's NBT node
        var nbtVm = ViewModelLocator.GetNbtExplorerViewModel();
        if (vm.SelectedChest != null)
        {
            nbtVm.NavigateToEntity(NbtEntityKind.Chest, vm.SelectedChest.X, vm.SelectedChest.Y);
        }
        else if (vm.SelectedSign != null)
        {
            nbtVm.NavigateToEntity(NbtEntityKind.Sign, vm.SelectedSign.X, vm.SelectedSign.Y);
        }
        else if (vm.SelectedTileEntity != null)
        {
            nbtVm.NavigateToEntity(NbtEntityKind.TileEntity, vm.SelectedTileEntity.PosX, vm.SelectedTileEntity.PosY);
        }
    }
}
