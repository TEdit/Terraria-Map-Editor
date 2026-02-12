using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View;

/// <summary>
/// Interaction logic for BestiaryEditor.xaml
/// </summary>
public partial class BestiaryEditor : UserControl
{
    private BestiaryViewModel _vm;

    public BestiaryEditor()
    {
        InitializeComponent();
        _vm = ViewModelLocator.GetBestiaryViewModel();
        DataContext = _vm;
    }
}
