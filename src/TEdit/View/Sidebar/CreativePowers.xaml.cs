using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View;

/// <summary>
/// Interaction logic for CreativePowers.xaml
/// </summary>
public partial class CreativePowers : UserControl
{
    private CreativePowersViewModel _vm;

    public CreativePowers()
    {
        InitializeComponent();
        _vm = ViewModelLocator.GetCreativePowersViewModel();
        DataContext = _vm;
    }
}
