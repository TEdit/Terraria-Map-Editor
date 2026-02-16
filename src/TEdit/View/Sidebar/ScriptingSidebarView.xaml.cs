using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View.Sidebar;

public partial class ScriptingSidebarView : UserControl
{
    private readonly ScriptingSidebarViewModel _vm;

    public ScriptingSidebarView()
    {
        InitializeComponent();
        _vm = ViewModelLocator.GetScriptingSidebarViewModel();
        DataContext = _vm;
        _vm.EnsureExampleScripts();
    }
}
