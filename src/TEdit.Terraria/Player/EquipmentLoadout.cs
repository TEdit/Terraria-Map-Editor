using System.Collections.ObjectModel;

namespace TEdit.Terraria.Player;

public partial class EquipmentLoadout : ReactiveObject
{
    public ObservableCollection<PlayerItem> Armor { get; } = [];
    public ObservableCollection<PlayerItem> Dye { get; } = [];
    public bool[] Hide { get; set; } = new bool[PlayerConstants.MaxDyeSlots];

    public EquipmentLoadout()
    {
        for (int i = 0; i < PlayerConstants.MaxArmorSlots; i++)
            Armor.Add(new PlayerItem());
        for (int i = 0; i < PlayerConstants.MaxDyeSlots; i++)
            Dye.Add(new PlayerItem());
    }
}
