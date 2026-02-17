using System.Windows.Controls;
using System.Windows.Data;
using TEdit.Render;

namespace TEdit.View.Sidebar.SpecialTilesViews;

public partial class ChestEditorView : UserControl
{
    public ChestEditorView()
    {
        InitializeComponent();

        if (!ItemPreviewCache.IsPopulated)
        {
            ItemPreviewCache.Populated += OnItemPreviewsPopulated;
        }
    }

    private void OnItemPreviewsPopulated()
    {
        ItemPreviewCache.Populated -= OnItemPreviewsPopulated;

        // Refresh the ListBox items so the converter re-evaluates with loaded previews
        CollectionViewSource.GetDefaultView(ChestList.ItemsSource)?.Refresh();
    }
}
