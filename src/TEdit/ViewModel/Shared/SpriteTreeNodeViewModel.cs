using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Common;
using TEdit.Geometry;

namespace TEdit.ViewModel.Shared;

/// <summary>
/// Represents a node in the sprite picker TreeView.
/// Root nodes are base tile types (TileProperty where IsFramed=true).
/// Child nodes are sprite variants with specific UV coordinates.
/// </summary>
[IReactiveObject]
public partial class SpriteTreeNodeViewModel
{
    public int TileId { get; init; }
    public string Name { get; init; } = string.Empty;
    public TEditColor Color { get; init; }

    /// <summary>
    /// UV coordinates for sprite variants. Null for root nodes (search by tile ID only).
    /// </summary>
    public Vector2Short? UV { get; init; }

    /// <summary>
    /// True if this is a root node (base tile type), false if it's a variant.
    /// </summary>
    public bool IsRoot => UV == null;

    [Reactive]
    private bool _isChecked;

    [Reactive]
    private bool _isExpanded;

    public ObservableCollection<SpriteTreeNodeViewModel> Children { get; } = [];

    public SpriteTreeNodeViewModel() { }

    public SpriteTreeNodeViewModel(int tileId, string name, TEditColor color, Vector2Short? uv = null)
    {
        TileId = tileId;
        Name = name;
        Color = color;
        UV = uv;
    }

    /// <summary>
    /// Display text for the node.
    /// </summary>
    public string DisplayText => UV.HasValue ? $"{Name} ({UV.Value.X}, {UV.Value.Y})" : Name;
}
