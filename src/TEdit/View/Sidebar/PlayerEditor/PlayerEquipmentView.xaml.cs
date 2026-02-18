using System.Windows;
using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View.Sidebar.PlayerEditor;

public partial class PlayerEquipmentView : UserControl
{
    private bool _suppressSelectionChange;

    public PlayerEquipmentView()
    {
        InitializeComponent();
    }

    private void EquipmentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionChange) return;
        if (EquipmentList.SelectedItem is EquipmentSlot slot)
        {
            _suppressSelectionChange = true;
            MiscList.SelectedItem = null;
            _suppressSelectionChange = false;
            SetSelectedSlot(slot);
        }
        else if (EquipmentList.SelectedItem == null && MiscList.SelectedItem == null)
        {
            SetSelectedSlot(null);
        }
    }

    private void MiscList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionChange) return;
        if (MiscList.SelectedItem is EquipmentSlot slot)
        {
            _suppressSelectionChange = true;
            EquipmentList.SelectedItem = null;
            _suppressSelectionChange = false;
            SetSelectedSlot(slot);
        }
        else if (MiscList.SelectedItem == null && EquipmentList.SelectedItem == null)
        {
            SetSelectedSlot(null);
        }
    }

    private void SetSelectedSlot(EquipmentSlot? slot)
    {
        if (DataContext is PlayerEditorViewModel vm)
            vm.SelectedEquipmentSlot = slot;

        // Show the correct category editor
        EditorHead.Visibility = Visibility.Collapsed;
        EditorBody.Visibility = Visibility.Collapsed;
        EditorLegs.Visibility = Visibility.Collapsed;
        EditorAccessory.Visibility = Visibility.Collapsed;
        EditorMisc.Visibility = Visibility.Collapsed;

        if (slot == null) return;

        var editor = slot.Category switch
        {
            EquipmentSlotCategory.Head => EditorHead,
            EquipmentSlotCategory.Body => EditorBody,
            EquipmentSlotCategory.Legs => EditorLegs,
            EquipmentSlotCategory.Accessory => EditorAccessory,
            EquipmentSlotCategory.Misc => EditorMisc,
            _ => EditorMisc
        };
        editor.Visibility = Visibility.Visible;
    }
}
