using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using TEdit.Common.Reactive;
using TEdit.Common.Reactive.Command;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Terraria.Objects;
using TEdit.Utility;
namespace TEdit.Terraria;
public partial class World : ReactiveObject, ITileData
{
    public Vector2Int32 Size => new Vector2Int32(TilesWide, TilesHigh);

    Tile[,] ITileData.Tiles => this.Tiles;
    public Tile[,] Tiles;

    public ObservableCollection<string> Anglers { get; } = new ObservableCollection<string>();
    public ObservableCollection<int> ShimmeredTownNPCs { get; } = new ObservableCollection<int>(Enumerable.Repeat(0, WorldConfiguration.MaxNpcID));
    public ObservableCollection<NPC> NPCs { get; } = new ObservableCollection<NPC>();
    public ObservableCollection<int> KilledMobs { get; } = new ObservableCollection<int>(Enumerable.Repeat(0, WorldConfiguration.MaxNpcID));
    public ObservableCollection<NPC> Mobs { get; } = new ObservableCollection<NPC>();
    public List<Sign> Signs { get; } = new();
    public List<Chest> Chests { get; } = new();
    public ObservableCollection<NpcName> CharacterNames { get; } = new ObservableCollection<NpcName>();
    public List<TileEntity> TileEntities { get; } = new();
    public ObservableCollection<int> PartyingNPCs { get; } = new ObservableCollection<int>();
    public ObservableCollection<PressurePlate> PressurePlates { get; } = new ObservableCollection<PressurePlate>();
    public ObservableCollection<TownManager> PlayerRooms { get; } = new ObservableCollection<TownManager>();

    [ReadOnly(true)]
    public DateTime LastSave { get; set; }

    public NpcName GetNpc(int id)
    {
        NpcName npc = CharacterNames.FirstOrDefault(c => c.Id == id);
        if (npc != null) return npc;
        return GetNewNpc(id);
    }
    private static NpcName GetNewNpc(int id)
    {
        string name;
        if (WorldConfiguration.NpcNames.TryGetValue(id, out name))
        {
            return new NpcName(id, name);
        }
        return new NpcName(id, "Unknown");
    }
    private ICommand _fixLayerGapCommand;
    public ICommand FixLayerGapCommand => _fixLayerGapCommand ??= new RelayCommand(() => FixLayerGap());
    private void FixLayerGap()
    {
        var gapModSix = (RockLevel - GroundLevel) % 6;
        var gapModSixInvert = 6 - gapModSix;
        if (gapModSix == 0) return; // nothing to do if divisible by 6
        bool canAdjustCavernDown = RockLevel <= MaxCavernLevel - 6;
        bool canAdjustCavernUp = RockLevel >= 12;
        bool canAdjustGroundUp = GroundLevel > 6;
        // priority is 
        // 1) cavern level: down
        // 3) cavern level: up
        // 2) ground level: up
        // 4) ground level: down
        if (canAdjustCavernDown)
        {
            RockLevel += gapModSixInvert;
        }
        else if (canAdjustCavernUp)
        {
            RockLevel -= gapModSix;
        }
        else if (canAdjustGroundUp)
        {
            GroundLevel -= gapModSix;
        }
        else
        {
            GroundLevel += gapModSixInvert;
        }
    }

    private void UpdateMaxLayerLevels()
    {
        if (!SafeGroundLayers)
        {
            // Defualt: Use unsafe levels.
            MaxCavernLevel = Calc.Clamp(TilesHigh, 0, TilesHigh);
            MaxGroundLevel = Calc.Clamp(TilesHigh - 6, 0, TilesHigh);
        }
        else
        {
            MaxCavernLevel = Calc.Clamp(TilesHigh - WorldConfiguration.CavernLevelToBottomOfWorld, 6, TilesHigh);
            MaxGroundLevel = Calc.Clamp(MaxCavernLevel - 6, 0, TilesHigh);
            
            // Adjust the sliders to reflect new values if over the max.
            if (GroundLevel > MaxGroundLevel)
                GroundLevel = MaxGroundLevel;
            if (RockLevel > MaxCavernLevel)
                RockLevel = MaxCavernLevel;
        }
    }
    
    public uint WorldVersion => Version;
    public Random Rand;
    public int[] TreeBG = new int[3];
    public int[] TreeMntBG = new int[2];
    public int[] TreeX = new int[3];
    public int[] CaveBackX = new int[4];
    public uint Version;
    private bool _downedMechBoss1TheDestroyer;
    private bool _downedMechBoss2TheTwins;
    private bool _downedMechBoss3SkeletronPrime;
    private double _groundLevel;
    private double _rockLevel;
    private int _shadowOrbCount;
    private bool _safeGroundLayers;
    private int _tilesHigh;
    private int _treeX0;
    private int _treeX1;
    private int _treeX2;
    private int _caveBackX0;
    private int _caveBackX1;
    private int _caveBackX2;
    private bool _hardMode;
    private bool _downedFishron;

    public ObservableCollection<int> TreeTopVariations { get; set; } = new ObservableCollection<int>(new int[13]);
    public Bestiary Bestiary { get; set; } = new Bestiary();
    public CreativePowers CreativePowers { get; set; } = new CreativePowers();

    [Reactive] public UInt64 WorldGenVersion { get; set; }

    #region World
    [Category("World")]
    [Reactive] public string Title { get; set; }

    [Category("World")]
    [Reactive] public int WorldId { get; set; }

    [Category("World")]
    [Reactive] public Guid WorldGUID { get; set; }

    [Category("World")]
    [Reactive] public uint FileRevision { get; set; }

    [Category("World")]
    [Reactive] public string Seed { get; set; }

    [Category("World")]
    [Reactive] public bool IsFavorite { get; set; }

    [Category("World")]
    [Reactive] public bool IsChinese { get; set; }

    [Category("World")]
    [Reactive] public bool IsConsole { get; set; }

    #endregion 

    #region Moon

    [Category("Moon")]
    [Reactive] public int MoonPhase { get; set; }

    [Category("Moon")]
    [Reactive] public bool BloodMoon { get; set; }

    [Category("Moon")]
    [Reactive] public byte MoonType { get; set; }

    [Category("Moon")]
    [Reactive] public bool IsEclipse { get; set; }

    #endregion Moon

    #region Time

    [Category("Time")]
    [Reactive] public double Time { get; set; }

    [Category("Time")]
    [Reactive] public bool DayTime { get; set; }

    [Category("Time")]
    [Reactive] public bool FastForwardTime { get; set; } // sundial

    [Category("Time")]
    [Reactive] public byte SundialCooldown { get; set; }

    [Category("Time")]
    [Reactive] public bool FastForwardTimeToDusk { get; set; } // moondial

    [Category("Time")]
    [Reactive] public byte MoondialCooldown { get; set; }
    #endregion Time

    #region Weather

    [Reactive] public bool IsRaining { get; set; }

    [Category("Weather")]
    [Reactive] public int TempRainTime { get; set; }

    [Category("Weather")]
    [Reactive] public float TempMaxRain { get; set; }

    [Category("Weather")]
    [Reactive] public double SlimeRainTime { get; set; }
    #endregion Weather

    #region Holidays

    [Category("Weather")]
    [Reactive] public bool ForceHalloweenForToday { get; set; }

    [Category("Weather")]
    [Reactive] public bool ForceXMasForToday { get; set; }

    [Category("Weather")]
    [Reactive] public short NumClouds { get; set; }

    [Category("Weather")]
    [Reactive] public float WindSpeedSet { get; set; }

    [Category("Weather")]
    [Reactive] public float CloudBgActive { get; set; }
    #endregion Holidays

    #region Sandstorm

    [Category("Sandstorm")]
    [Reactive] public bool SandStormHappening { get; set; }

    [Category("Sandstorm")]
    [Reactive] public int SandStormTimeLeft { get; set; }

    [Category("Sandstorm")]
    [Reactive] public float SandStormSeverity { get; set; }

    [Category("Sandstorm")]
    [Reactive] public float SandStormIntendedSeverity { get; set; }

    #endregion Sandstorm

    #region Levels

    [Category("Levels")]
    [Reactive] public int MaxCavernLevel { get; set; }
    [Category("Levels")]
    [Reactive] public int MaxGroundLevel { get; set; }

    [Category("Levels")]
    public double GroundLevel
    {
        get => _groundLevel;
        set
        {
            if (value < 0) { return; } // skip if negative
            if (value > MaxGroundLevel) value = MaxGroundLevel;
            this.RaiseAndSetIfChanged(ref _groundLevel, value);
            // if levels touch, shift by 6
            if (GroundLevel >= RockLevel)
            {
                RockLevel = GroundLevel + 6;
            }
        }
    }

    [Category("Levels")]
    public double RockLevel
    {
        get => _rockLevel;
        set
        {
            if (value < 0) { return; } // skip if negative
            if (value > MaxCavernLevel) value = MaxCavernLevel;
            this.RaiseAndSetIfChanged(ref _rockLevel, value);

            // if levels touch, shift by 6
            if (GroundLevel >= RockLevel)
            {
                GroundLevel = RockLevel - 6;
            }
        }
    }
    
    [Category("Levels")]
    public bool SafeGroundLayers
    {
        get => _safeGroundLayers;
        set
        {
            this.RaiseAndSetIfChanged(ref _safeGroundLayers, value);
            UpdateMaxLayerLevels();
        }
    }

    #endregion Levels

    #region Difficulty

    [Category("Difficulty")]
    [Reactive] public bool SpawnMeteor { get; set; }

    [Category("Difficulty")]
    public bool HardMode
    {
        get => _hardMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _hardMode, value);
            this.RaisePropertyChanged(nameof(DownedWallOfFlesh));
        }
    }

    [Category("Difficulty")]
    [Reactive] public int GameMode { get; set; }

    [Category("Difficulty")]
    [Reactive] public bool CombatBookUsed { get; set; }

    [Category("Difficulty")]
    [Reactive] public bool CombatBookVolumeTwoWasUsed { get; set; }

    [Category("Difficulty")]
    [Reactive] public bool PeddlersSatchelWasUsed { get; set; }

    [Category("Difficulty")]
    [Reactive] public bool PartyOfDoom { get; set; }

    #endregion Difficulty

    #region Seed

    [Category("Seed")]
    [Reactive] public bool DrunkWorld { get; set; }

    [Category("Seed")]
    [Reactive] public bool GoodWorld { get; set; }

    [Category("Seed")]
    [Reactive] public bool TenthAnniversaryWorld { get; set; }

    [Category("Seed")]
    [Reactive] public bool DontStarveWorld { get; set; }

    [Category("Seed")]
    [Reactive] public bool NotTheBeesWorld { get; set; }

    [Category("Seed")]
    [Reactive] public bool RemixWorld { get; set; }

    [Category("Seed")]
    [Reactive] public bool NoTrapsWorld { get; set; }

    [Category("Seed")]
    [Reactive] public bool ZenithWorld { get; set; }

    #endregion Seed

    #region Ore Tier

    [Category("Ore Tier")]
    [Reactive] public bool IsCrimson { get; set; }

    [Category("Ore Tier")]
    [Reactive] public int AltarCount { get; set; }

    [Category("Ore Tier")]
    [Reactive] public bool ShadowOrbSmashed { get; set; }

    [Category("Ore Tier")]
    [Reactive] public int SavedOreTiersCopper { get; set; }

    [Category("Ore Tier")]
    [Reactive] public int SavedOreTiersIron { get; set; }

    [Category("Ore Tier")]
    [Reactive] public int SavedOreTiersSilver { get; set; }

    [Category("Ore Tier")]
    [Reactive] public int SavedOreTiersGold { get; set; }

    [Category("Ore Tier")]
    [Reactive] public int SavedOreTiersCobalt { get; set; }

    [Category("Ore Tier")]
    [Reactive] public int SavedOreTiersMythril { get; set; }

    [Category("Ore Tier")]
    [Reactive] public int SavedOreTiersAdamantite { get; set; }

    #endregion Ore Tier

    #region Pre-Hardmode Bosses

    [Category("Pre-Hardmode Bosses")]
    [Reactive] public bool DownedSlimeKingBoss { get; set; }

    [Category("Pre-Hardmode Bosses")]
    [Reactive] public bool DownedBoss1EyeofCthulhu { get; set; }

    [Category("Pre-Hardmode Bosses")]
    [Reactive] public bool DownedBoss2EaterofWorlds { get; set; }

    [Category("Pre-Hardmode Bosses")]
    [Reactive] public bool DownedQueenBee { get; set; }

    [Category("Pre-Hardmode Bosses")]
    [Reactive] public bool DownedBoss3Skeletron { get; set; }

    [Category("Pre-Hardmode Bosses")]
    public bool DownedWallOfFlesh
    {
        get => HardMode;
        set
        {
            HardMode = value;
            this.RaisePropertyChanged(nameof(DownedWallOfFlesh));
        }
    }

    #endregion Pre-Hardmode Bosses

    #region Hardmode Bosses

    [Category("Hardmode Bosses")]
    public bool DownedMechBossAny
    {
        get => DownedMechBoss1TheDestroyer || DownedMechBoss2TheTwins || DownedMechBoss3SkeletronPrime;
        set {; }
    }

    [Category("Hardmode Bosses")]
    public bool DownedMechBoss2TheTwins
    {
        get => _downedMechBoss2TheTwins;
        set
        {
            this.RaiseAndSetIfChanged(ref _downedMechBoss2TheTwins, value);
            this.RaisePropertyChanged(nameof(DownedMechBossAny));
        }
    }

    [Category("Hardmode Bosses")]
    public bool DownedMechBoss1TheDestroyer
    {
        get => _downedMechBoss1TheDestroyer;
        set
        {
            this.RaiseAndSetIfChanged(ref _downedMechBoss1TheDestroyer, value);
            this.RaisePropertyChanged(nameof(DownedMechBossAny));
        }
    }

    [Category("Hardmode Bosses")]
    public bool DownedMechBoss3SkeletronPrime
    {
        get => _downedMechBoss3SkeletronPrime;
        set
        {
            this.RaiseAndSetIfChanged(ref _downedMechBoss3SkeletronPrime, value);
            this.RaisePropertyChanged(nameof(DownedMechBossAny));
        }
    }

    [Category("Hardmode Bosses")]
    [Reactive] public bool DownedPlantBoss { get; set; }

    [Category("Hardmode Bosses")]
    [Reactive] public bool DownedGolemBoss { get; set; }

    [Category("Hardmode Bosses")]
    public bool DownedFishron
    {
        get => _downedFishron;
        set
        {
            this.RaiseAndSetIfChanged(ref _downedFishron, value);
            this.RaisePropertyChanged(nameof(DownedFlyingDutchman));
        }
    }

    [Category("Hardmode Bosses")]
    [Reactive] public bool DownedLunaticCultist { get; set; }

    [Category("Hardmode Bosses")]
    [Reactive] public bool DownedMoonlord { get; set; }

    #endregion Hardmode Bosses

    #region Boss Events

    [Category("Boss Events")]
    [Reactive] public bool DownedHalloweenTree { get; set; } // Mourning Wood

    [Category("Boss Events")]
    [Reactive] public bool DownedHalloweenKing { get; set; } // Pumpking

    [Category("Boss Events")]
    [Reactive] public bool DownedChristmasTree { get; set; }

    [Category("Boss Events")]
    [Reactive] public bool DownedSanta { get; set; }

    [Category("Boss Events")]
    [Reactive] public bool DownedChristmasQueen { get; set; }

    [Category("Boss Events")]
    public bool DownedFlyingDutchman
    {
        get => DownedFishron;
        set
        {
            DownedFishron = value;
            this.RaisePropertyChanged(nameof(DownedFlyingDutchman));
        }
    }

    [Category("Boss Events")]
    [Reactive] public bool DownedCelestialSolar { get; set; }

    [Category("Boss Events")]
    [Reactive] public bool DownedCelestialNebula { get; set; }

    [Category("Boss Events")]
    [Reactive] public bool DownedCelestialVortex { get; set; }

    [Category("Boss Events")]
    [Reactive] public bool DownedCelestialStardust { get; set; }

    [Category("Boss Events")]
    [Reactive] public bool CelestialSolarActive { get; set; }

    [Category("Boss Events")]
    [Reactive] public bool CelestialVortexActive { get; set; }

    [Category("Boss Events")]
    [Reactive] public bool CelestialNebulaActive { get; set; }

    [Category("Boss Events")]
    [Reactive] public bool CelestialStardustActive { get; set; }

    #endregion Boss Events

    #region Lantern Night

    [Category("Lantern Night")]
    [Reactive] public int LanternNightCooldown { get; set; }

    [Category("Lantern Night")]
    [Reactive] public bool LanternNightManual { get; set; }

    [Category("Lantern Night")]
    [Reactive] public bool LanternNightGenuine { get; set; }

    [Category("Lantern Night")]
    [Reactive] public bool LanternNightNextNightIsGenuine { get; set; }

    #endregion Lantern Night

    #region Journey's End

    [Category("Journey's End")]
    [Reactive] public bool DownedEmpressOfLight { get; set; }

    [Category("Journey's End")]
    [Reactive] public bool DownedQueenSlime { get; set; }

    [Category("Journey's End")]
    [Reactive] public bool DownedDeerclops { get; set; }

    #endregion Journey's End

    #region Old One's Army

    [Category("Old One's Army")]
    [Reactive] public bool DownedDD2InvasionT1 { get; set; }

    [Category("Old One's Army")]
    [Reactive] public bool DownedDD2InvasionT2 { get; set; }

    [Category("Old One's Army")]
    [Reactive] public bool DownedDD2InvasionT3 { get; set; }

    #endregion Old One's Army

    #region NPCs Bought

    [Category("NPCs Bought")]
    [Reactive] public bool BoughtCat { get; set; }

    [Category("NPCs Bought")]
    [Reactive] public bool BoughtDog { get; set; }

    [Category("NPCs Bought")]
    [Reactive] public bool BoughtBunny { get; set; }

    #endregion NPCs Bought

    #region NPCs Saved

    [Category("NPCs Saved")]
    [Reactive] public bool SavedGoblin { get; set; }

    [Category("NPCs Saved")]
    [Reactive] public bool SavedMech { get; set; }

    [Category("NPCs Saved")]
    [Reactive] public bool SavedWizard { get; set; }

    [Category("NPCs Saved")]
    [Reactive] public bool SavedStylist { get; set; }

    [Category("NPCs Saved")]
    [Reactive] public bool SavedTaxCollector { get; set; }

    [Category("NPCs Saved")]
    [Reactive] public bool SavedBartender { get; set; }

    [Category("NPCs Saved")]
    [Reactive] public bool SavedGolfer { get; set; }

    [Category("NPCs Saved")]
    [Reactive] public bool SavedAngler { get; set; }

    [Category("NPCs Saved")]
    [Reactive] public int AnglerQuest { get; set; }

    #endregion NPCs Saved

    #region NPCs Unlocked

    [Category("NPCs Unlocked")]
    [Reactive] public bool UnlockedMerchantSpawn { get; set; }

    [Category("NPCs Unlocked")]
    [Reactive] public bool UnlockedDemolitionistSpawn { get; set; }

    [Category("NPCs Unlocked")]
    [Reactive] public bool UnlockedPartyGirlSpawn { get; set; }

    [Category("NPCs Unlocked")]
    [Reactive] public bool UnlockedDyeTraderSpawn { get; set; }

    [Category("NPCs Unlocked")]
    [Reactive] public bool UnlockedTruffleSpawn { get; set; }

    [Category("NPCs Unlocked")]
    [Reactive] public bool UnlockedArmsDealerSpawn { get; set; }

    [Category("NPCs Unlocked")]
    [Reactive] public bool UnlockedNurseSpawn { get; set; }

    [Category("NPCs Unlocked")]
    [Reactive] public bool UnlockedPrincessSpawn { get; set; }

    #endregion NPCs Unlocked

    #region Town Slimes Unlocked

    [Category("Town Slimes Unlocked")]
    [Reactive] public bool UnlockedSlimeBlueSpawn { get; set; }

    [Category("Town Slimes Unlocked")]
    [Reactive] public bool UnlockedSlimeGreenSpawn { get; set; }

    [Category("Town Slimes Unlocked")]
    [Reactive] public bool UnlockedSlimeOldSpawn { get; set; }

    [Category("Town Slimes Unlocked")]
    [Reactive] public bool UnlockedSlimePurpleSpawn { get; set; }

    [Category("Town Slimes Unlocked")]
    [Reactive] public bool UnlockedSlimeRainbowSpawn { get; set; }

    [Category("Town Slimes Unlocked")]
    [Reactive] public bool UnlockedSlimeRedSpawn { get; set; }

    [Category("Town Slimes Unlocked")]
    [Reactive] public bool UnlockedSlimeYellowSpawn { get; set; }

    [Category("Town Slimes Unlocked")]
    [Reactive] public bool UnlockedSlimeCopperSpawn { get; set; }

    #endregion Town Slimes Unlocked

    #region Invasions

    [Category("Invasions")]
    [Reactive] public bool DownedGoblins { get; set; }

    [Category("Invasions")]
    [Reactive] public bool DownedFrost { get; set; }

    [Category("Invasions")]
    [Reactive] public bool DownedPirates { get; set; }

    [Category("Invasions")]
    [Reactive] public bool DownedMartians { get; set; }

    [Category("Invasions")]
    [Reactive] public int InvasionType { get; set; }

    [Category("Invasions")]
    [Reactive] public int InvasionSize { get; set; }

    [Category("Invasions")]
    [Reactive] public double InvasionX { get; set; }

    [Category("Invasions")]
    [ReadOnly(true)]
    [Reactive] public int InvasionSizeStart { get; set; }

    [Category("Invasions")]
    [ReadOnly(true)]
    [Reactive] public int InvasionDelay { get; set; }

    #endregion Invasions

    #region Custom World Generation

    private double _hillSize;
    private bool _generateGrass;
    private bool _generateWalls;
    private bool _generateCaves;
    private ObservableCollection<string> _cavePresets;
    private int _cavePresetIndex;
    private bool _surfaceCaves;
    private double _caveNoise;
    private double _caveMultiplier;
    private double _caveDensity;
    private bool _generateUnderworld;
    private bool _generateAsh;
    private bool _generateLava;
    private double _underworldRoofNoise;
    private double _underworldFloorNoise;
    private double _underworldLavaNoise;
    private bool _generateOres;

	public double HillSize
    {
        get => _hillSize;
        set => this.RaiseAndSetIfChanged(ref _hillSize, value);
    }
    public bool GenerateGrass
    {
        get => _generateGrass;
        set => this.RaiseAndSetIfChanged(ref _generateGrass, value);
    }
    public bool GenerateWalls
    {
        get => _generateWalls;
        set => this.RaiseAndSetIfChanged(ref _generateWalls, value);
    }
    public bool GenerateCaves
    {
        get => _generateCaves;
        set => this.RaiseAndSetIfChanged(ref _generateCaves, value);
    }
    public int CavePresetIndex
    {
        get => _cavePresetIndex;
        set => this.RaiseAndSetIfChanged(ref _cavePresetIndex, value);
    }
    public ObservableCollection<string> CavePresets
    {
        get => _cavePresets;
        set => this.RaiseAndSetIfChanged(ref _cavePresets, value);
    }
    public bool SurfaceCaves
    {
        get => _surfaceCaves;
        set => this.RaiseAndSetIfChanged(ref _surfaceCaves, value);
    }
    public double CaveNoise
    {
        get => _caveNoise;
        set => this.RaiseAndSetIfChanged(ref _caveNoise, value);
    }
    public double CaveMultiplier
    {
        get => _caveMultiplier;
        set => this.RaiseAndSetIfChanged(ref _caveMultiplier, value);
    }
    public double CaveDensity
    {
        get => _caveDensity;
        set => this.RaiseAndSetIfChanged(ref _caveDensity, value);
    }
    public bool GenerateUnderworld
    {
        get => _generateUnderworld;
        set => this.RaiseAndSetIfChanged(ref _generateUnderworld, value);
    }
    public bool GenerateAsh
    {
        get => _generateAsh;
        set => this.RaiseAndSetIfChanged(ref _generateAsh, value);
    }
    public bool GenerateLava
    {
        get => _generateLava;
        set => this.RaiseAndSetIfChanged(ref _generateLava, value);
    }
    public double UnderworldRoofNoise
    {
        get => _underworldRoofNoise;
        set => this.RaiseAndSetIfChanged(ref _underworldRoofNoise, value);
    }
    public double UnderworldFloorNoise
    {
        get => _underworldFloorNoise;
        set => this.RaiseAndSetIfChanged(ref _underworldFloorNoise, value);
    }
    public double UnderworldLavaNoise
    {
        get => _underworldLavaNoise;
        set => this.RaiseAndSetIfChanged(ref _underworldLavaNoise, value);
    }
    public bool GenerateOres
    {
        get => _generateOres;
        set => this.RaiseAndSetIfChanged(ref _generateOres, value);
    }
    #endregion

    [Reactive] public bool PartyManual { get; set; }
    [Reactive] public bool PartyGenuine { get; set; }
    [Reactive] public int PartyCooldown { get; set; }

    public int TileEntitiesNumber => TileEntities.Count;

    [Reactive][ReadOnly(true)] public byte[] UnknownData { get; set; }

    [Reactive] public Int64 CreationTime { get; set; }

    [Reactive] public int IceBackStyle { get; set; }
    [Reactive] public int JungleBackStyle { get; set; }
    [Reactive] public int HellBackStyle { get; set; }

    public int TreeTop1
    {
        get => TreeTopVariations[0];
        set
        {
            TreeTopVariations[0] = (int)value;
            this.RaisePropertyChanged();
        }
    }
    public int TreeTop2
    {
        get => TreeTopVariations[1];
        set
        {
            TreeTopVariations[1] = (int)value;
            this.RaisePropertyChanged();
        }
    }
    public int TreeTop3
    {
        get => TreeTopVariations[2];
        set
        {

            TreeTopVariations[2] = value;
            this.RaisePropertyChanged();

        }
    }
    public int TreeTop4
    {
        get => TreeTopVariations[3];
        set => TreeTopVariations[3] = (int)value;
    }

    [Reactive] public byte BgOcean { get; set; }
    [Reactive] public byte BgDesert { get; set; }
    [Reactive] public byte BgCrimson { get; set; }
    [Reactive] public byte BgHallow { get; set; }
    [Reactive] public byte BgSnow { get; set; }
    [Reactive] public byte BgJungle { get; set; }
    [Reactive] public byte BgCorruption { get; set; }
    [Reactive] public byte BgTree { get; set; }

    public byte Bg8
    {
        get => BgTree;
        set => BgTree = value;
    }

    [Reactive] public byte BgTree2 { get; set; }
    [Reactive] public byte BgTree3 { get; set; }
    [Reactive] public byte BgTree4 { get; set; }
    [Reactive] public byte UnderworldBg { get; set; }
    [Reactive] public byte MushroomBg { get; set; }
    
    [Reactive] public int TilesWide { get; set; }
    [Reactive] public int TilesHighReactive { get; set; }
    public int TilesHigh
    {
        get => _tilesHigh;
        set
        {
            // Update the reactive property to ensure UI and other bindings are notified.
            TilesHighReactive = value;
            
            _tilesHigh = value;
            this.RaiseAndSetIfChanged(ref _tilesHigh, value);
            UpdateMaxLayerLevels();
        }
    }
    [Reactive] public float BottomWorld { get; set; }
    [Reactive] public float TopWorld { get; set; }
    [Reactive] public float RightWorld { get; set; }
    [Reactive] public float LeftWorld { get; set; }
    [Reactive] public bool IsV0 { get; set; }
    [Reactive] public int SpawnX { get; set; }
    [Reactive] public int SpawnY { get; set; }

    [Reactive] public int DungeonX { get; set; }
    [Reactive] public int DungeonY { get; set; }

    [Reactive] public bool DownedClown { get; set; }

    public int ShadowOrbCount
    {
        get => _shadowOrbCount;
        set
        {
            _shadowOrbCount = value;
            ShadowOrbSmashed = _shadowOrbCount > 0;
        }
    }
    public int TreeX0
    {
        get => _treeX0;
        set
        {
            _treeX0 = value;
            if (_treeX0 > _treeX1)
                TreeX0 = _treeX1;
        }
    }
    public int TreeX1
    {
        get => _treeX1;
        set
        {
            _treeX1 = value;
            if (_treeX1 < _treeX0)
                TreeX1 = _treeX0;
            if (_treeX1 > _treeX2)
                TreeX1 = _treeX2;
        }
    }
    public int TreeX2
    {
        get => _treeX2;
        set
        {
            _treeX2 = value;
            if (_treeX2 < _treeX1)
                TreeX2 = _treeX1;
        }
    }
    [Reactive] public int TreeStyle0 { get; set; }
    [Reactive] public int TreeStyle1 { get; set; }
    [Reactive] public int TreeStyle2 { get; set; }
    [Reactive] public int TreeStyle3 { get; set; }

    public int CaveBackX0
    {
        get => _caveBackX0;
        set
        {
            _caveBackX0 = value;
            if (_caveBackX0 > _caveBackX1)
                CaveBackX0 = _caveBackX1;
        }
    }
    public int CaveBackX1
    {
        get => _caveBackX1;
        set
        {
            _caveBackX1 = value;
            if (_caveBackX1 < _caveBackX0)
                CaveBackX1 = _caveBackX0;
            if (_caveBackX1 > _caveBackX2)
                CaveBackX1 = _caveBackX2;
        }
    }
    public int CaveBackX2
    {
        get => _caveBackX2;
        set
        {
            _caveBackX2 = value;
            if (_caveBackX2 < _caveBackX1)
                CaveBackX2 = _caveBackX1;
        }
    }
    [Reactive] public int CaveBackStyle0 { get; set; }
    [Reactive] public int CaveBackStyle1 { get; set; }
    [Reactive] public int CaveBackStyle2 { get; set; }
    [Reactive] public int CaveBackStyle3 { get; set; }
    [Reactive] public int CultistDelay { get; set; }
    [Reactive] public bool Apocalypse { get; set; }
}
