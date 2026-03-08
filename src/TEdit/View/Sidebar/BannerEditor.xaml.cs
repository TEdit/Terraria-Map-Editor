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

    private void GridViewColumnHeader_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader header || header.Column == null)
            return;

        var binding = header.Column.DisplayMemberBinding as System.Windows.Data.Binding;
        var propertyName = binding?.Path?.Path;

        if (!string.IsNullOrEmpty(propertyName))
        {
            _vm.ApplySort(propertyName);
        }
    }
}
