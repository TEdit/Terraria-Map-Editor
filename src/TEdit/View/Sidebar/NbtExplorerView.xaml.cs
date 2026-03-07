using System.Windows;
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

        _vm.RequestZoomToTile += OnRequestZoomToTile;
    }

    private void OnRequestZoomToTile(int x, int y)
    {
        var mainWindow = Application.Current.MainWindow as MainWindow;
        mainWindow?.PanTo(x, y);
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is NbtNodeViewModel node)
        {
            _vm.SelectedNode = node;
        }
    }
}
