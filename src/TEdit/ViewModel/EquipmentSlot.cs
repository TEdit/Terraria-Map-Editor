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

public class EquipmentSlot
{
    public string Label { get; init; } = "";
    public PlayerItem Item { get; init; } = null!;
    public PlayerItem? Dye { get; init; }
    public EquipmentSlotCategory Category { get; init; }
}
