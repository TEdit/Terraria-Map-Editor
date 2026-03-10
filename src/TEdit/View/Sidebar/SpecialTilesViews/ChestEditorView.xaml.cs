using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TEdit.Render;
using TEdit.Terraria;
using TEdit.ViewModel;

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

        ChestList.PreviewKeyDown += OnChestListPreviewKeyDown;
    }

    private void OnChestListPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.FocusedElement is TextBox) return;
        if (e.KeyboardDevice.Modifiers != ModifierKeys.Control) return;

        switch (e.Key)
        {
            case Key.C:
                if (ChestList.SelectedItem is Item item)
                    WorldViewModel.ItemClipboard = item.Copy();
                e.Handled = true;
                break;
            case Key.X:
                if (ChestList.SelectedItem is Item cutItem)
                {
                    WorldViewModel.ItemClipboard = cutItem.Copy();
                    cutItem.NetId = 0;
                    cutItem.StackSize = 0;
                    cutItem.Prefix = 0;
                }
                e.Handled = true;
                break;
            case Key.V:
                if (WorldViewModel.ItemClipboard != null)
                {
                    foreach (var obj in ChestList.SelectedItems)
                    {
                        if (obj is Item target)
                        {
                            target.NetId = WorldViewModel.ItemClipboard.NetId;
                            target.StackSize = WorldViewModel.ItemClipboard.StackSize;
                            target.Prefix = WorldViewModel.ItemClipboard.Prefix;
                        }
                    }
                }
                e.Handled = true;
                break;
        }
    }

    private void OnItemPreviewsPopulated()
    {
        ItemPreviewCache.Populated -= OnItemPreviewsPopulated;

        // Refresh the ListBox items so the converter re-evaluates with loaded previews
        CollectionViewSource.GetDefaultView(ChestList.ItemsSource)?.Refresh();
    }
}
