using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View.Sidebar;

public partial class NbtExplorerView : UserControl
{
    private readonly NbtExplorerViewModel _vm;

    public NbtExplorerView()
    {
        InitializeComponent();
        _vm = ViewModelLocator.GetNbtExplorerViewModel();
        DataContext = _vm;
    }

    private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is NbtNodeViewModel node)
        {
            _vm.SelectedNode = node;
        }
    }
}
