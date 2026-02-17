namespace TEdit.Terraria.Player;

public partial class TemporaryItemSlots : ReactiveObject
{
    [Reactive] private PlayerItem? _mouseItem;
    [Reactive] private PlayerItem? _creativeMenuItem;
    [Reactive] private PlayerItem? _guideItem;
    [Reactive] private PlayerItem? _reforgeItem;
}
