using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View.Sidebar;

public partial class FindSidebarView : UserControl
{
    private readonly FindSidebarViewModel _vm;

    public FindSidebarView()
    {
        InitializeComponent();
        _vm = ViewModelLocator.GetFindSidebarViewModel();
        DataContext = _vm;
    }
}
