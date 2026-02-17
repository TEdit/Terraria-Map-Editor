using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.ViewModel.Shared;

/// <summary>
/// ViewModel for the SpriteTreePickerControl - manages a filterable, multi-selectable
/// TreeView of sprites (framed tiles) with their variants.
/// </summary>
[IReactiveObject]
public partial class SpriteTreePickerViewModel
{
    public ObservableCollection<SpriteTreeNodeViewModel> RootNodes { get; } = [];
    public ICollectionView FilteredRootNodesView { get; }

    [Reactive]
    private string _searchText = string.Empty;

    [Reactive]
    private bool _showCheckboxes = true;

    [Reactive]
    private SpriteTreeNodeViewModel? _selectedNode;

    public int SelectedCount => CountSelectedNodes();

    public SpriteTreePickerViewModel()
    {
        // Set up filtered view
        var cvs = new CollectionViewSource { Source = RootNodes };
        FilteredRootNodesView = cvs.View;
        FilteredRootNodesView.Filter = FilterRootNode;

        // React to search text changes
        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(_ =>
            {
                FilteredRootNodesView.Refresh();
                this.RaisePropertyChanged(nameof(SearchText));
            });

        LoadData();
    }

    private void LoadData()
    {
        RootNodes.Clear();

        // Get all framed tiles (sprites)
        var framedTiles = WorldConfiguration.TileProperties
            .Where(t => t.IsFramed && t.Frames?.Count > 0)
            .OrderBy(t => t.Name);

        foreach (var tile in framedTiles)
        {
            var rootNode = new SpriteTreeNodeViewModel(tile.Id, tile.Name, tile.Color);

            // Add variant children from Frames
            if (tile.Frames != null)
            {
                foreach (var frame in tile.Frames.OrderBy(f => f.ToString()))
                {
                    var childNode = new SpriteTreeNodeViewModel(
                        tile.Id,
                        frame.ToString(),
                        tile.Color,
                        frame.UV
                    );
                    rootNode.Children.Add(childNode);
                }
            }

            RootNodes.Add(rootNode);
        }
    }

    private bool FilterRootNode(object obj)
    {
        if (obj is not SpriteTreeNodeViewModel node) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        // Match root name or any child name
        if (node.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            return true;

        return node.Children.Any(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
    }

    private int CountSelectedNodes()
    {
        int count = 0;
        foreach (var root in RootNodes)
        {
            if (root.IsChecked) count++;
            count += root.Children.Count(c => c.IsChecked);
        }
        return count;
    }

    [ReactiveCommand]
    private void CheckAll()
    {
        foreach (var root in RootNodes.Where(FilterRootNode))
        {
            root.IsChecked = true;
            // Don't check children - checking root means "any variant"
        }
        this.RaisePropertyChanged(nameof(SelectedCount));
    }

    [ReactiveCommand]
    private void UncheckAll()
    {
        foreach (var root in RootNodes.Where(FilterRootNode))
        {
            root.IsChecked = false;
            foreach (var child in root.Children)
            {
                child.IsChecked = false;
            }
        }
        this.RaisePropertyChanged(nameof(SelectedCount));
    }

    [ReactiveCommand]
    private void ClearSelection()
    {
        foreach (var root in RootNodes)
        {
            root.IsChecked = false;
            foreach (var child in root.Children)
            {
                child.IsChecked = false;
            }
        }
        this.RaisePropertyChanged(nameof(SelectedCount));
    }

    /// <summary>
    /// Get list of selected sprites for searching.
    /// Returns tuples of (TileId, UV?) where UV is null for root selections (match any variant).
    /// </summary>
    public IReadOnlyList<(int TileId, Vector2Short? UV)> GetSelectedSprites()
    {
        var result = new List<(int, Vector2Short?)>();

        foreach (var root in RootNodes)
        {
            if (root.IsChecked)
            {
                // Root selected = match any variant of this tile
                result.Add((root.TileId, null));
            }
            else
            {
                // Check individual children
                foreach (var child in root.Children.Where(c => c.IsChecked))
                {
                    result.Add((child.TileId, child.UV));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Notify that selection count changed (call after toggling checkboxes).
    /// </summary>
    public void NotifySelectionChanged()
    {
        this.RaisePropertyChanged(nameof(SelectedCount));
    }
}
