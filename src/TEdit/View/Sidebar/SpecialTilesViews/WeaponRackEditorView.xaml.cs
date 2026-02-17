using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TEdit.Configuration;
using TEdit.Terraria;
using TEdit.View.Sidebar.Controls;

namespace TEdit.View.Sidebar.SpecialTilesViews;

public partial class WeaponRackEditorView : UserControl
{
    public WeaponRackEditorView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Check if the current item is in the rackable list
        var netId = ItemEditor.ItemId;
        var itemInRackList = netId == 0 || WorldConfiguration.RackableItems.ContainsKey(netId);

        // Use saved preference, but force Show All if the item isn't in the filtered list
        var showAll = UserSettingsService.Current.ShowAllWeaponRackItems || !itemInRackList;
        ShowAllItemsCheckBox.IsChecked = showAll;
        ApplyItemsSource(showAll);
    }

    private void OnShowAllItemsChanged(object sender, RoutedEventArgs e)
    {
        var showAll = ShowAllItemsCheckBox.IsChecked == true;
        UserSettingsService.Current.ShowAllWeaponRackItems = showAll;
        ApplyItemsSource(showAll);
    }

    private void ApplyItemsSource(bool showAll)
    {
        var key = showAll ? "ItemsCollection" : "RackCollection";
        var cvs = (CollectionViewSource)FindResource(key);
        ItemEditor.ItemsSource = cvs.View;
        ItemEditor.ItemsSourceType = showAll
            ? ItemsSourceType.ItemPropertyList
            : ItemsSourceType.DictionaryKvp;
    }
}
