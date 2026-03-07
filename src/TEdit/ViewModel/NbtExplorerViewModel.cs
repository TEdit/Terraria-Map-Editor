using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
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

    /// <summary>Raised when the view should pan to a tile coordinate.</summary>
    public event Action<int, int>? RequestZoomToTile;

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

        var twldData = _worldViewModel.CurrentWorld?.TwldData;
        if (twldData?.RawTag == null) return;

        var parent = FindParentCompound(twldData.RawTag, SelectedNode);
        if (parent != null)
        {
            SelectedNode.ApplyEdit(parent);
        }
    }

    [ReactiveCommand]
    private void ZoomToTile(NbtNodeViewModel? node)
    {
        node ??= SelectedNode;
        if (node is not { HasCoordinates: true }) return;
        RequestZoomToTile?.Invoke(node.CoordX, node.CoordY);
    }

    [ReactiveCommand]
    private void EditEntity(NbtNodeViewModel? node)
    {
        node ??= SelectedNode;
        if (node == null || node.EntityKind == NbtEntityKind.None) return;

        var world = _worldViewModel.CurrentWorld;
        if (world == null) return;

        int x = node.CoordX;
        int y = node.CoordY;

        switch (node.EntityKind)
        {
            case NbtEntityKind.Chest:
                var chest = world.Chests.FirstOrDefault(c => c.X == x && c.Y == y);
                if (chest != null)
                    _worldViewModel.SelectedChest = chest.Copy();
                break;

            case NbtEntityKind.Sign:
                var sign = world.Signs.FirstOrDefault(s => s.X == x && s.Y == y);
                if (sign != null)
                    _worldViewModel.SelectedSign = sign.Copy();
                break;

            case NbtEntityKind.TileEntity:
                var te = world.TileEntities.FirstOrDefault(e => e.PosX == x && e.PosY == y);
                if (te != null)
                    _worldViewModel.SelectedTileEntity = te.Copy();
                break;
        }

        // Also zoom to the entity
        RequestZoomToTile?.Invoke(x, y);
    }

    /// <summary>
    /// Finds the TagCompound that directly contains the given node's key.
    /// </summary>
    private static Common.IO.TagCompound? FindParentCompound(Common.IO.TagCompound root, NbtNodeViewModel target)
    {
        if (root.ContainsKey(target.Name))
        {
            var val = root[target.Name];
            if (ReferenceEquals(val, target.RawValue) || (target.IsEditable && val?.ToString() == target.RawValue?.ToString()))
                return root;
        }

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
