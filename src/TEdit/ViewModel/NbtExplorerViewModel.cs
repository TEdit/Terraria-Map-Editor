using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria.TModLoader;

namespace TEdit.ViewModel;

/// <summary>
/// ViewModel for the NBT Explorer sidebar. Displays a filterable TreeView
/// of the raw TagCompound data from a .twld file.
/// </summary>
[IReactiveObject]
public partial class NbtExplorerViewModel
{
    private readonly WorldViewModel _worldViewModel;

    public ObservableCollection<NbtNodeViewModel> RootNodes { get; } = [];
    public ICollectionView FilteredRootNodesView { get; }

    [Reactive]
    private string _searchText = string.Empty;

    [Reactive]
    private NbtNodeViewModel? _selectedNode;

    public NbtExplorerViewModel(WorldViewModel worldViewModel)
    {
        _worldViewModel = worldViewModel;

        var cvs = new CollectionViewSource { Source = RootNodes };
        FilteredRootNodesView = cvs.View;
        FilteredRootNodesView.Filter = FilterRootNode;

        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(_ => FilteredRootNodesView.Refresh());
    }

    /// <summary>
    /// Rebuilds the tree from the given TwldData's RawTag.
    /// </summary>
    public void LoadFromTwldData(TwldData data)
    {
        RootNodes.Clear();

        if (data?.RawTag == null) return;

        foreach (var kvp in data.RawTag)
        {
            RootNodes.Add(NbtNodeViewModel.FromTag(kvp.Key, kvp.Value));
        }
    }

    /// <summary>
    /// Clears the tree.
    /// </summary>
    public void Clear()
    {
        RootNodes.Clear();
    }

    [ReactiveCommand]
    private void Refresh()
    {
        if (_worldViewModel.CurrentWorld?.TwldData != null)
        {
            LoadFromTwldData(_worldViewModel.CurrentWorld.TwldData);
        }
    }

    [ReactiveCommand]
    private void ApplyEdit()
    {
        if (SelectedNode is not { IsEditable: true }) return;

        // Walk up to find the parent TagCompound — for top-level nodes, it's the RawTag
        var twldData = _worldViewModel.CurrentWorld?.TwldData;
        if (twldData?.RawTag == null) return;

        // For simplicity, apply directly to RawTag if the node's Name exists as a top-level key.
        // For nested edits, we stored the RawValue reference which points into the live TagCompound.
        // The ApplyEdit method needs the immediate parent TagCompound.
        // Since the tree mirrors the live TagCompound structure, we find the parent by searching.
        var parent = FindParentCompound(twldData.RawTag, SelectedNode);
        if (parent != null)
        {
            SelectedNode.ApplyEdit(parent);
        }
    }

    /// <summary>
    /// Finds the TagCompound that directly contains the given node's key.
    /// </summary>
    private static Common.IO.TagCompound? FindParentCompound(Common.IO.TagCompound root, NbtNodeViewModel target)
    {
        // Check if this compound directly contains the target key with matching value
        if (root.ContainsKey(target.Name))
        {
            var val = root[target.Name];
            if (ReferenceEquals(val, target.RawValue) || (target.IsEditable && val?.ToString() == target.RawValue?.ToString()))
                return root;
        }

        // Recurse into child compounds and lists
        foreach (var kvp in root)
        {
            if (kvp.Value is Common.IO.TagCompound childCompound)
            {
                var found = FindParentCompound(childCompound, target);
                if (found != null) return found;
            }
            else if (kvp.Value is System.Collections.IList list)
            {
                foreach (var item in list)
                {
                    if (item is Common.IO.TagCompound listCompound)
                    {
                        var found = FindParentCompound(listCompound, target);
                        if (found != null) return found;
                    }
                }
            }
        }

        return null;
    }

    private bool FilterRootNode(object obj)
    {
        if (obj is not NbtNodeViewModel node) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        return node.MatchesSearch(SearchText);
    }
}
