using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View.Sidebar;

public partial class FilterSidebarView : UserControl
{
    private readonly FilterSidebarViewModel _vm;

    public FilterSidebarView()
    {
        InitializeComponent();
        _vm = ViewModelLocator.GetFilterSidebarViewModel();
        DataContext = _vm;
    }
}
