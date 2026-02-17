using Shouldly;
using TEdit.Terraria.Player;

namespace TEdit.Terraria.Tests.Player;

public class PlayerCharacterTests
{
    [Fact]
    public void DefaultConstructor_InitializesCollections()
    {
        var player = new PlayerCharacter();
        player.Armor.Count.ShouldBe(PlayerConstants.MaxArmorSlots);
        player.Dye.Count.ShouldBe(PlayerConstants.MaxDyeSlots);
        player.MiscEquips.Count.ShouldBe(PlayerConstants.MaxMiscEquipSlots);
        player.MiscDyes.Count.ShouldBe(PlayerConstants.MaxMiscEquipSlots);
        player.Inventory.Count.ShouldBe(PlayerConstants.MaxInventorySlots);
        player.Bank1.Count.ShouldBe(PlayerConstants.MaxBankSlots);
        player.Bank2.Count.ShouldBe(PlayerConstants.MaxBankSlots);
        player.Bank3.Count.ShouldBe(PlayerConstants.MaxBankSlots);
        player.Bank4.Count.ShouldBe(PlayerConstants.MaxBankSlots);
        player.Buffs.Count.ShouldBe(PlayerConstants.MaxBuffSlots);
    }

    [Fact]
    public void DefaultConstructor_InitializesArrays()
    {
        var player = new PlayerCharacter();
        player.HideVisibleAccessory.Length.ShouldBe(PlayerConstants.MaxHideVisibleAccessory);
        player.HideInfo.Length.ShouldBe(PlayerConstants.MaxHideInfoSlots);
        player.DpadRadialBindings.Length.ShouldBe(PlayerConstants.MaxDpadBindings);
        player.BuilderAccStatus.Length.ShouldBe(PlayerConstants.MaxBuilderAccStatus);
        player.Loadouts.Length.ShouldBe(PlayerConstants.MaxLoadouts);
    }

    [Fact]
    public void PlayTime_ComputesFromTicks()
    {
        var player = new PlayerCharacter();
        player.PlayTimeTicks = TimeSpan.FromHours(2.5).Ticks;
        player.PlayTime.TotalHours.ShouldBe(2.5, 0.001);
    }

    [Fact]
    public void DefaultValues_AreReasonable()
    {
        var player = new PlayerCharacter();
        player.Name.ShouldBe(string.Empty);
        player.Difficulty.ShouldBe((byte)0);
        player.StatLife.ShouldBe(100);
        player.StatLifeMax.ShouldBe(100);
        player.StatMana.ShouldBe(20);
        player.StatManaMax.ShouldBe(20);
    }
}
