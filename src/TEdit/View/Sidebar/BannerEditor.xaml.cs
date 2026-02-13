using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View;

public partial class BannerEditor : UserControl
{
    private BannerViewModel _vm;

    public BannerEditor()
    {
        InitializeComponent();
        _vm = ViewModelLocator.GetBannerViewModel();
        DataContext = _vm;
    }
}
