using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria.Player;

namespace TEdit.ViewModel;

public enum EquipmentSlotCategory
{
    Head,
    Body,
    Legs,
    Accessory,
    Misc
}

[IReactiveObject]
public partial class EquipmentSlot
{
    public string Label { get; init; } = "";
    public PlayerItem Item { get; init; } = null!;
    public PlayerItem? Dye { get; init; }
    public EquipmentSlotCategory Category { get; init; }

    /// <summary>
    /// Whether this equipment slot is hidden (eye toggle off).
    /// </summary>
    [Reactive] private bool _isHidden;

    [ReactiveCommand]
    private void ToggleVisibility() => IsHidden = !IsHidden;

    /// <summary>
    /// Whether this slot supports a visibility toggle.
    /// Vanity slots do not have hide toggles.
    /// </summary>
    public bool HasVisibilityToggle { get; init; }

    /// <summary>
    /// Index into the hide array for writing back changes.
    /// -1 means no hide array mapping.
    /// </summary>
    public int HideIndex { get; init; } = -1;
}
