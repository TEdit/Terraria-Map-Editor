using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Common;
using TEdit.Terraria;

namespace TEdit.ViewModel.Shared;

public enum PickerDataSource
{
    Items,      // WorldConfiguration.ItemProperties
    TileBricks, // WorldConfiguration.TileBricks (non-framed tiles)
    Walls,      // WorldConfiguration.WallProperties
    Liquids,    // LiquidType enum + GlobalColors
    Wires       // Wire colors from GlobalColors
}

/// <summary>
/// ViewModel for the TileWallPickerControl - manages a filterable, multi-selectable
/// list of tiles/walls/items with color swatches.
/// </summary>
[IReactiveObject]
public partial class TileWallPickerViewModel
{
    public ObservableCollection<PickerItemViewModel> AllItems { get; } = [];
    public ICollectionView FilteredItemsView { get; }

    [Reactive]
    private string _searchText = string.Empty;

    [Reactive]
    private bool _showCheckboxes = true;

    public int SelectedCount => AllItems.Count(x => x.IsChecked);

    public TileWallPickerViewModel(PickerDataSource dataSource)
    {
        // Set up filtered view
        var cvs = new CollectionViewSource { Source = AllItems };
        FilteredItemsView = cvs.View;
        FilteredItemsView.Filter = FilterItem;

        // React to search text changes
        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(_ =>
            {
                FilteredItemsView.Refresh();
                this.RaisePropertyChanged(nameof(SearchText));
            });

        // Load data based on source
        LoadData(dataSource);
    }

    private void LoadData(PickerDataSource dataSource)
    {
        AllItems.Clear();

        switch (dataSource)
        {
            case PickerDataSource.Items:
                foreach (var item in WorldConfiguration.ItemProperties.OrderBy(x => x.Name))
                {
                    // Items don't have a color property, use a default
                    AllItems.Add(new PickerItemViewModel(item.Id, item.Name, TEditColor.White));
                }
                break;

            case PickerDataSource.TileBricks:
                foreach (var tile in WorldConfiguration.TileBricks.OrderBy(x => x.Name))
                {
                    AllItems.Add(new PickerItemViewModel(tile.Id, tile.Name, tile.Color));
                }
                break;

            case PickerDataSource.Walls:
                foreach (var wall in WorldConfiguration.WallProperties.OrderBy(x => x.Name))
                {
                    AllItems.Add(new PickerItemViewModel(wall.Id, wall.Name, wall.Color));
                }
                break;

            case PickerDataSource.Liquids:
                AddLiquid(LiquidType.Water, "Water");
                AddLiquid(LiquidType.Lava, "Lava");
                AddLiquid(LiquidType.Honey, "Honey");
                AddLiquid(LiquidType.Shimmer, "Shimmer");
                break;

            case PickerDataSource.Wires:
                AddWire(1, "Red Wire", "Wire");
                AddWire(2, "Blue Wire", "Wire1");
                AddWire(3, "Green Wire", "Wire2");
                AddWire(4, "Yellow Wire", "Wire3");
                break;
        }
    }

    private void AddLiquid(LiquidType type, string name)
    {
        var color = WorldConfiguration.GlobalColors.TryGetValue(name, out var c) ? c : TEditColor.Magenta;
        AllItems.Add(new PickerItemViewModel((int)type, name, color));
    }

    private void AddWire(int id, string name, string colorKey)
    {
        var color = WorldConfiguration.GlobalColors.TryGetValue(colorKey, out var c) ? c : TEditColor.Magenta;
        AllItems.Add(new PickerItemViewModel(id, name, color));
    }

    private bool FilterItem(object obj)
    {
        if (obj is not PickerItemViewModel item) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        return item.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || item.Id.ToString().Contains(SearchText, StringComparison.Ordinal);
    }

    [ReactiveCommand]
    private void CheckAll()
    {
        foreach (var item in AllItems.Where(x => FilterItem(x)))
        {
            item.IsChecked = true;
        }
        this.RaisePropertyChanged(nameof(SelectedCount));
    }

    [ReactiveCommand]
    private void UncheckAll()
    {
        foreach (var item in AllItems.Where(x => FilterItem(x)))
        {
            item.IsChecked = false;
        }
        this.RaisePropertyChanged(nameof(SelectedCount));
    }

    [ReactiveCommand]
    private void ClearSelection()
    {
        foreach (var item in AllItems)
        {
            item.IsChecked = false;
        }
        this.RaisePropertyChanged(nameof(SelectedCount));
    }

    /// <summary>
    /// Get list of selected IDs for searching.
    /// </summary>
    public IReadOnlyList<int> GetSelectedIds()
    {
        return AllItems.Where(x => x.IsChecked).Select(x => x.Id).ToList();
    }

    /// <summary>
    /// Notify that selection count changed (call after toggling checkboxes).
    /// </summary>
    public void NotifySelectionChanged()
    {
        this.RaisePropertyChanged(nameof(SelectedCount));
    }
}
