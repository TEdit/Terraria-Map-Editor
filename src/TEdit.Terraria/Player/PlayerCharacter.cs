using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TEdit.Terraria.Player;

public partial class PlayerCharacter : ReactiveObject
{
    // Header
    [Reactive] private string _name = string.Empty;
    [Reactive] private byte _difficulty;
    [Reactive] private long _playTimeTicks;
    [Reactive] private byte _team;

    public TimeSpan PlayTime => TimeSpan.FromTicks(PlayTimeTicks);

    // Appearance
    public PlayerAppearance Appearance { get; set; } = new();

    // Stats
    [Reactive] private int _statLife = 100;
    [Reactive] private int _statLifeMax = 100;
    [Reactive] private int _statMana = 20;
    [Reactive] private int _statManaMax = 20;
    [Reactive] private bool _extraAccessory;

    // Flags
    [Reactive] private bool _unlockedBiomeTorches;
    [Reactive] private bool _usingBiomeTorches;
    [Reactive] private bool _ateArtisanBread;
    [Reactive] private bool _usedAegisCrystal;
    [Reactive] private bool _usedAegisFruit;
    [Reactive] private bool _usedArcaneCrystal;
    [Reactive] private bool _usedGalaxyPearl;
    [Reactive] private bool _usedGummyWorm;
    [Reactive] private bool _usedAmbrosia;
    [Reactive] private bool _downedDD2EventAnyDifficulty;
    [Reactive] private int _taxMoney;
    [Reactive] private int _numberOfDeathsPVE;
    [Reactive] private int _numberOfDeathsPVP;

    // Hide arrays
    public bool[] HideVisibleAccessory { get; set; } = new bool[PlayerConstants.MaxHideVisibleAccessory];
    public byte HideMisc { get; set; }

    // Equipment (active)
    public ObservableCollection<PlayerItem> Armor { get; } = [];
    public ObservableCollection<PlayerItem> Dye { get; } = [];
    public ObservableCollection<PlayerItem> MiscEquips { get; } = [];
    public ObservableCollection<PlayerItem> MiscDyes { get; } = [];

    // Inventory
    public ObservableCollection<PlayerItem> Inventory { get; } = [];
    public PlayerItem TrashItem { get; set; } = new();

    // Banks
    public ObservableCollection<PlayerItem> Bank1 { get; } = [];
    public ObservableCollection<PlayerItem> Bank2 { get; } = [];
    public ObservableCollection<PlayerItem> Bank3 { get; } = [];
    public ObservableCollection<PlayerItem> Bank4 { get; } = [];
    [Reactive] private byte _voidVaultInfo;

    // Buffs
    public ObservableCollection<PlayerBuff> Buffs { get; } = [];

    // Spawn points
    public ObservableCollection<SpawnPoint> SpawnPoints { get; } = [];

    // Misc state
    [Reactive] private bool _hbLocked;
    public bool[] HideInfo { get; set; } = new bool[PlayerConstants.MaxHideInfoSlots];
    [Reactive] private int _anglerQuestsFinished;
    public int[] DpadRadialBindings { get; set; } = new int[PlayerConstants.MaxDpadBindings];
    public int[] BuilderAccStatus { get; set; } = new int[PlayerConstants.MaxBuilderAccStatus];
    [Reactive] private int _bartenderQuestLog;
    [Reactive] private bool _dead;
    [Reactive] private int _respawnTimer;
    [Reactive] private long _lastSaveTime;
    [Reactive] private int _golferScoreAccumulated;

    // Loadouts (v262+)
    [Reactive] private int _currentLoadoutIndex;
    public EquipmentLoadout[] Loadouts { get; set; } = [new(), new(), new()];

    // Voice (v280+)
    [Reactive] private int _voiceVariant;
    [Reactive] private float _voicePitchOffset;

    // Journey Mode (v218+)
    public Dictionary<string, int> CreativeSacrifices { get; set; } = [];
    public TemporaryItemSlots TemporaryItems { get; set; } = new();
    public CreativePowers JourneyPowers { get; set; } = new();

    // Crafting (v300+)
    public ObservableCollection<PlayerItem> CraftingRefundItems { get; } = [];

    // SuperCart (v253+)
    [Reactive] private bool _unlockedSuperCart;
    [Reactive] private bool _enabledSuperCart;

    // Dialogues (v310+)
    public List<string> OneTimeDialoguesSeen { get; set; } = [];

    // File metadata
    public int Version { get; set; } = PlayerConstants.CurrentVersion;
    public uint FileRevision { get; set; }
    public bool IsFavorite { get; set; }

    public PlayerCharacter()
    {
        InitializeCollections();
    }

    private void InitializeCollections()
    {
        for (int i = 0; i < PlayerConstants.MaxArmorSlots; i++)
            Armor.Add(new PlayerItem());
        for (int i = 0; i < PlayerConstants.MaxDyeSlots; i++)
            Dye.Add(new PlayerItem());
        for (int i = 0; i < PlayerConstants.MaxMiscEquipSlots; i++)
        {
            MiscEquips.Add(new PlayerItem());
            MiscDyes.Add(new PlayerItem());
        }
        for (int i = 0; i < PlayerConstants.MaxInventorySlots; i++)
            Inventory.Add(new PlayerItem());
        for (int i = 0; i < PlayerConstants.MaxBankSlots; i++)
        {
            Bank1.Add(new PlayerItem());
            Bank2.Add(new PlayerItem());
            Bank3.Add(new PlayerItem());
            Bank4.Add(new PlayerItem());
        }
        for (int i = 0; i < PlayerConstants.MaxBuffSlots; i++)
            Buffs.Add(new PlayerBuff());
    }
}
