using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Common;

namespace TEdit.ViewModel.Shared;

/// <summary>
/// Represents a selectable item in the Filter/Find sidebar pickers.
/// Extends the pattern from FilterCheckItem with Color support for visual display.
/// </summary>
[IReactiveObject]
public partial class PickerItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TEditColor Color { get; init; }

    /// <summary>
    /// The item ID whose icon to display (e.g. the item that places this tile/wall).
    /// For item pickers this equals Id; for tile/wall pickers it's the corresponding item.
    /// </summary>
    public int ItemId { get; init; }

    [Reactive]
    private bool _isChecked;

    public PickerItemViewModel() { }

    public PickerItemViewModel(int id, string name, TEditColor color, bool isChecked = false, int itemId = 0)
    {
        Id = id;
        Name = name;
        Color = color;
        IsChecked = isChecked;
        ItemId = itemId;
    }
}
