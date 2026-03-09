using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View;

public partial class BestiaryEditor : UserControl
{
    private BestiaryViewModel _vm;

    public BestiaryEditor()
    {
        InitializeComponent();
        _vm = ViewModelLocator.GetBestiaryViewModel();
        DataContext = _vm;
    }

    private void GridViewColumnHeader_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader header || header.Column == null)
            return;

        // Map header text to property name
        var headerText = header.Column.Header?.ToString();
        var binding = header.Column.DisplayMemberBinding as System.Windows.Data.Binding;
        var propertyName = binding?.Path?.Path;

        if (string.IsNullOrEmpty(propertyName))
        {
            // For columns with CellTemplate instead of DisplayMemberBinding
            propertyName = headerText switch
            {
                "⭐" => "BestiaryStars",
                _ => null
            };
        }

        if (!string.IsNullOrEmpty(propertyName))
        {
            _vm.ApplySort(propertyName);
        }
    }
}
