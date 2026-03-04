using System.Windows.Controls;
using System.Windows.Data;
using TEdit.Render;

namespace TEdit.View.Sidebar.Controls;

public partial class TileWallPickerControl : UserControl
{
    public TileWallPickerControl()
    {
        InitializeComponent();

        // When item previews finish loading asynchronously, refresh the list
        // so the ItemIdToPreviewConverter re-evaluates and shows images.
        if (!ItemPreviewCache.IsPopulated)
        {
            ItemPreviewCache.Populated += OnItemPreviewsPopulated;
        }
    }

    private void OnItemPreviewsPopulated()
    {
        ItemPreviewCache.Populated -= OnItemPreviewsPopulated;
        Dispatcher.Invoke(() =>
        {
            CollectionViewSource.GetDefaultView(ItemsListBox.ItemsSource)?.Refresh();
        });
    }
}
