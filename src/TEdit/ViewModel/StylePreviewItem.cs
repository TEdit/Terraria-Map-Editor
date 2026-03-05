using System.Windows.Media.Imaging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace TEdit.ViewModel;

/// <summary>
/// Represents a style option with a texture preview for display in comboboxes.
/// Used for tree styles, background options, and cave styles in WorldPropertiesView.
/// </summary>
public partial class StylePreviewItem : ReactiveObject
{
    /// <summary>
    /// The value of this style (stored in world file).
    /// Type varies: byte for TreeStyle/CaveStyle, int for BgTree/etc.
    /// </summary>
    [Reactive]
    private object _value;

    /// <summary>
    /// Display name shown in the combobox alongside the preview.
    /// </summary>
    [Reactive]
    private string _displayName;

    /// <summary>
    /// Scaled texture preview (max 128px dimension).
    /// </summary>
    [Reactive]
    private WriteableBitmap _preview;

    /// <summary>
    /// Optional tree top preview shown beside background previews for biome comboboxes.
    /// </summary>
    [Reactive]
    private WriteableBitmap _treeTopPreview;
}
