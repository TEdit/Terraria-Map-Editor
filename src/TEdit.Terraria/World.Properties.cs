using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
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
    public ObservableCollection<ushort> ClaimableBanners { get; } = new ObservableCollection<ushort>(Enumerable.Repeat<ushort>(0, WorldConfiguration.MaxNpcID));

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
    [ReactiveCommand]
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

    [Reactive] private ulong _worldGenVersion;

    #region World
    [property: Category("World")]
    [Reactive] private string _title;

    [property: Category("World")]
    [Reactive] private int _worldId;

    [property: Category("World")]
    [Reactive] private Guid _worldGUID;

    [property: Category("World")]
    [Reactive] private uint _fileRevision;

    [property: Category("World")]
    [Reactive] private string _seed;

    [property: Category("World")]
    [Reactive] private bool _isFavorite;

    [property: Category("World")]
    [Reactive] private bool _isChinese;

    [property: Category("World")]
    [Reactive] private bool _isConsole;

    #endregion

    #region Moon

    [property: Category("Moon")]
    [Reactive] private int _moonPhase;

    [property: Category("Moon")]
    [Reactive] private bool _bloodMoon;

    [property: Category("Moon")]
    [Reactive] private byte _moonType;

    [property: Category("Moon")]
    [Reactive] private bool _isEclipse;

    #endregion Moon

    #region Time

    [property: Category("Time")]
    [Reactive] private double _time;

    [property: Category("Time")]
    [Reactive] private bool _dayTime;

    [property: Category("Time")]
    [Reactive] private bool _fastForwardTime; // sundial

    [property: Category("Time")]
    [Reactive] private byte _sundialCooldown;

    [property: Category("Time")]
    [Reactive] private bool _fastForwardTimeToDusk; // moondial

    [property: Category("Time")]
    [Reactive] private byte _moondialCooldown;
    #endregion Time

    #region Weather

    [property: Category("Weather")]
    [Reactive] private bool _isRaining;

    [property: Category("Weather")]
    [Reactive] private int _tempRainTime;

    [property: Category("Weather")]
    [Reactive] private float _tempMaxRain;

    [property: Category("Weather")]
    [Reactive] private double _slimeRainTime;

    [property: Category("Weather")]
    [Reactive] private int _tempMeteorShowerCount;
    [property: Category("Weather")]
    [Reactive] private int _tempCoinRain;

    #endregion Weather

    #region Holidays

    [property: Category("Weather")]
    [Reactive] private bool _forceHalloweenForToday;

    [property: Category("Weather")]
    [Reactive] private bool _forceXMasForToday;

    [property: Category("Weather")]
    [Reactive] private short _numClouds;

    [property: Category("Weather")]
    [Reactive] private float _windSpeedSet;

    [property: Category("Weather")]
    [Reactive] private float _cloudBgActive;

    [property: Category("Difficulty")]
    [Reactive] private bool _forceHalloweenForever;

    [property: Category("Difficulty")]
    [Reactive] private bool _forceXMasForever;
    #endregion Holidays

    #region Sandstorm

    [property: Category("Sandstorm")]
    [Reactive] private bool _sandStormHappening;

    [property: Category("Sandstorm")]
    [Reactive] private int _sandStormTimeLeft;

    [property: Category("Sandstorm")]
    [Reactive] private float _sandStormSeverity;

    [property: Category("Sandstorm")]
    [Reactive] private float _sandStormIntendedSeverity;

    #endregion Sandstorm

    #region Levels

    [property: Category("Levels")]
    [Reactive] private int _maxCavernLevel;
    [property: Category("Levels")]
    [Reactive] private int _maxGroundLevel;

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

    [property: Category("Difficulty")]
    [Reactive] private bool _spawnMeteor;

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

    [property: Category("Difficulty")]
    [Reactive] private int _gameMode;

    [property: Category("Difficulty")]
    [Reactive] private bool _combatBookUsed;

    [property: Category("Difficulty")]
    [Reactive] private bool _combatBookVolumeTwoWasUsed;

    [property: Category("Difficulty")]
    [Reactive] private bool _peddlersSatchelWasUsed;

    [property: Category("Difficulty")]
    [Reactive] private bool _partyOfDoom;


    #endregion Difficulty

    #region Seed

    [property: Category("Seed")]
    [Reactive] private bool _drunkWorld;

    [property: Category("Seed")]
    [Reactive] private bool _goodWorld;

    [property: Category("Seed")]
    [Reactive] private bool _tenthAnniversaryWorld;

    [property: Category("Seed")]
    [Reactive] private bool _dontStarveWorld;

    [property: Category("Seed")]
    [Reactive] private bool _notTheBeesWorld;

    [property: Category("Seed")]
    [Reactive] private bool _remixWorld;

    [property: Category("Seed")]
    [Reactive] private bool _noTrapsWorld;

    [property: Category("Seed")]
    [Reactive] private bool _zenithWorld;

    [property: Category("Seed")]
    [Reactive] private bool _skyblockWorld;

    [property: Category("Seed")]
    [Reactive] private bool _vampireSeed;

    [property: Category("Seed")]
    [Reactive] private bool _infectedSeed;

    [property: Category("Seed")]
    [Reactive] private bool _dualDungeonsSeed;

    #endregion Seed

    #region Ore Tier

    [property: Category("Ore Tier")]
    [Reactive] private bool _isCrimson;

    [property: Category("Ore Tier")]
    [Reactive] private int _altarCount;

    [property: Category("Ore Tier")]
    [Reactive] private bool _shadowOrbSmashed;

    [property: Category("Ore Tier")]
    [Reactive] private int _savedOreTiersCopper;

    [property: Category("Ore Tier")]
    [Reactive] private int _savedOreTiersIron;

    [property: Category("Ore Tier")]
    [Reactive] private int _savedOreTiersSilver;

    [property: Category("Ore Tier")]
    [Reactive] private int _savedOreTiersGold;

    [property: Category("Ore Tier")]
    [Reactive] private int _savedOreTiersCobalt;

    [property: Category("Ore Tier")]
    [Reactive] private int _savedOreTiersMythril;

    [property: Category("Ore Tier")]
    [Reactive] private int _savedOreTiersAdamantite;

    #endregion Ore Tier

    #region Pre-Hardmode Bosses

    [property: Category("Pre-Hardmode Bosses")]
    [Reactive] private bool _downedSlimeKingBoss;

    [property: Category("Pre-Hardmode Bosses")]
    [Reactive] private bool _downedBoss1EyeofCthulhu;

    [property: Category("Pre-Hardmode Bosses")]
    [Reactive] private bool _downedBoss2EaterofWorlds;

    [property: Category("Pre-Hardmode Bosses")]
    [Reactive] private bool _downedQueenBee;

    [property: Category("Pre-Hardmode Bosses")]
    [Reactive] private bool _downedBoss3Skeletron;

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

    [property: Category("Hardmode Bosses")]
    [Reactive] private bool _downedPlantBoss;

    [property: Category("Hardmode Bosses")]
    [Reactive] private bool _downedGolemBoss;

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

    [property: Category("Hardmode Bosses")]
    [Reactive] private bool _downedLunaticCultist;

    [property: Category("Hardmode Bosses")]
    [Reactive] private bool _downedMoonlord;

    #endregion Hardmode Bosses

    #region Boss Events

    [property: Category("Boss Events")]
    [Reactive] private bool _downedHalloweenTree; // Mourning Wood

    [property: Category("Boss Events")]
    [Reactive] private bool _downedHalloweenKing; // Pumpking

    [property: Category("Boss Events")]
    [Reactive] private bool _downedChristmasTree;

    [property: Category("Boss Events")]
    [Reactive] private bool _downedSanta;

    [property: Category("Boss Events")]
    [Reactive] private bool _downedChristmasQueen;

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

    [property: Category("Boss Events")]
    [Reactive] private bool _downedCelestialSolar;

    [property: Category("Boss Events")]
    [Reactive] private bool _downedCelestialNebula;

    [property: Category("Boss Events")]
    [Reactive] private bool _downedCelestialVortex;

    [property: Category("Boss Events")]
    [Reactive] private bool _downedCelestialStardust;

    [property: Category("Boss Events")]
    [Reactive] private bool _celestialSolarActive;

    [property: Category("Boss Events")]
    [Reactive] private bool _celestialVortexActive;

    [property: Category("Boss Events")]
    [Reactive] private bool _celestialNebulaActive;

    [property: Category("Boss Events")]
    [Reactive] private bool _celestialStardustActive;

    #endregion Boss Events

    #region Team Spawns

    [property: Category("Team Spawns")]
    [Reactive] private bool _teamBasedSpawnsSeed;
    [Reactive] private ObservableCollection<Vector2Int32> _teamSpawns = new ObservableCollection<Vector2Int32>();

    #endregion

    #region Lantern Night

    [property: Category("Lantern Night")]
    [Reactive] private int _lanternNightCooldown;

    [property: Category("Lantern Night")]
    [Reactive] private bool _lanternNightManual;

    [property: Category("Lantern Night")]
    [Reactive] private bool _lanternNightGenuine;

    [property: Category("Lantern Night")]
    [Reactive] private bool _lanternNightNextNightIsGenuine;

    #endregion Lantern Night

    #region Journey's End

    [property: Category("Journey's End")]
    [Reactive] private bool _downedEmpressOfLight;

    [property: Category("Journey's End")]
    [Reactive] private bool _downedQueenSlime;

    [property: Category("Journey's End")]
    [Reactive] private bool _downedDeerclops;

    #endregion Journey's End

    #region Old One's Army

    [property: Category("Old One's Army")]
    [Reactive] private bool _downedDD2InvasionT1;

    [property: Category("Old One's Army")]
    [Reactive] private bool _downedDD2InvasionT2;

    [property: Category("Old One's Army")]
    [Reactive] private bool _downedDD2InvasionT3;

    #endregion Old One's Army

    #region NPCs Bought

    [property: Category("NPCs Bought")]
    [Reactive] private bool _boughtCat;

    [property: Category("NPCs Bought")]
    [Reactive] private bool _boughtDog;

    [property: Category("NPCs Bought")]
    [Reactive] private bool _boughtBunny;

    #endregion NPCs Bought

    #region NPCs Saved

    [property: Category("NPCs Saved")]
    [Reactive] private bool _savedGoblin;

    [property: Category("NPCs Saved")]
    [Reactive] private bool _savedMech;

    [property: Category("NPCs Saved")]
    [Reactive] private bool _savedWizard;

    [property: Category("NPCs Saved")]
    [Reactive] private bool _savedStylist;

    [property: Category("NPCs Saved")]
    [Reactive] private bool _savedTaxCollector;

    [property: Category("NPCs Saved")]
    [Reactive] private bool _savedBartender;

    [property: Category("NPCs Saved")]
    [Reactive] private bool _savedGolfer;

    [property: Category("NPCs Saved")]
    [Reactive] private bool _savedAngler;

    [property: Category("NPCs Saved")]
    [Reactive] private int _anglerQuest;

    #endregion NPCs Saved

    #region NPCs Unlocked

    [property: Category("NPCs Unlocked")]
    [Reactive] private bool _unlockedMerchantSpawn;

    [property: Category("NPCs Unlocked")]
    [Reactive] private bool _unlockedDemolitionistSpawn;

    [property: Category("NPCs Unlocked")]
    [Reactive] private bool _unlockedPartyGirlSpawn;

    [property: Category("NPCs Unlocked")]
    [Reactive] private bool _unlockedDyeTraderSpawn;

    [property: Category("NPCs Unlocked")]
    [Reactive] private bool _unlockedTruffleSpawn;

    [property: Category("NPCs Unlocked")]
    [Reactive] private bool _unlockedArmsDealerSpawn;

    [property: Category("NPCs Unlocked")]
    [Reactive] private bool _unlockedNurseSpawn;

    [property: Category("NPCs Unlocked")]
    [Reactive] private bool _unlockedPrincessSpawn;

    #endregion NPCs Unlocked

    #region Town Slimes Unlocked

    [property: Category("Town Slimes Unlocked")]
    [Reactive] private bool _unlockedSlimeBlueSpawn;

    [property: Category("Town Slimes Unlocked")]
    [Reactive] private bool _unlockedSlimeGreenSpawn;

    [property: Category("Town Slimes Unlocked")]
    [Reactive] private bool _unlockedSlimeOldSpawn;

    [property: Category("Town Slimes Unlocked")]
    [Reactive] private bool _unlockedSlimePurpleSpawn;

    [property: Category("Town Slimes Unlocked")]
    [Reactive] private bool _unlockedSlimeRainbowSpawn;

    [property: Category("Town Slimes Unlocked")]
    [Reactive] private bool _unlockedSlimeRedSpawn;

    [property: Category("Town Slimes Unlocked")]
    [Reactive] private bool _unlockedSlimeYellowSpawn;

    [property: Category("Town Slimes Unlocked")]
    [Reactive] private bool _unlockedSlimeCopperSpawn;

    #endregion Town Slimes Unlocked

    #region Invasions

    [property: Category("Invasions")]
    [Reactive] private bool _downedGoblins;

    [property: Category("Invasions")]
    [Reactive] private bool _downedFrost;

    [property: Category("Invasions")]
    [Reactive] private bool _downedPirates;

    [property: Category("Invasions")]
    [Reactive] private bool _downedMartians;

    [property: Category("Invasions")]
    [Reactive] private int _invasionType;

    [property: Category("Invasions")]
    [Reactive] private int _invasionSize;

    [property: Category("Invasions")]
    [Reactive] private double _invasionX;

    [property: Category("Invasions")]
    [property: ReadOnly(true)]
    [Reactive] private int _invasionSizeStart;

    [property: Category("Invasions")]
    [property: ReadOnly(true)]
    [Reactive] private int _invasionDelay;

    #endregion Invasions

    #region Custom World Generation

    [Reactive] private double _hillSize;
    [Reactive] private bool _generateGrass;
    [Reactive] private bool _generateWalls;
    [Reactive] private bool _generateCaves;
    [Reactive] private ObservableCollection<string> _cavePresets;
    [Reactive] private int _cavePresetIndex;
    [Reactive] private bool _surfaceCaves;
    [Reactive] private double _caveNoise;
    [Reactive] private double _caveMultiplier;
    [Reactive] private double _caveDensity;
    [Reactive] private bool _generateUnderworld;
    [Reactive] private bool _generateAsh;
    [Reactive] private bool _generateLava;
    [Reactive] private double _underworldRoofNoise;
    [Reactive] private double _underworldFloorNoise;
    [Reactive] private double _underworldLavaNoise;
    [Reactive] private bool _generateOres;
    #endregion

    [Reactive] private bool _partyManual;
    [Reactive] private bool _partyGenuine;
    [Reactive] private int _partyCooldown;

    public int TileEntitiesNumber => TileEntities.Count;

    [property: ReadOnly(true)]
    [Reactive] private byte[] _unknownData;

    [Reactive] private long _creationTime;
    [Reactive] private long _lastPlayed;

    [Reactive] private int _iceBackStyle;
    [Reactive] private int _jungleBackStyle;
    [Reactive] private int _hellBackStyle;

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

    [Reactive] private byte _bgOcean;
    [Reactive] private byte _bgDesert;
    [Reactive] private byte _bgCrimson;
    [Reactive] private byte _bgHallow;
    [Reactive] private byte _bgSnow;
    [Reactive] private byte _bgJungle;
    [Reactive] private byte _bgCorruption;
    [Reactive] private byte _bgTree;

    public byte Bg8
    {
        get => BgTree;
        set => BgTree = value;
    }

    [Reactive] private byte _bgTree2;
    [Reactive] private byte _bgTree3;
    [Reactive] private byte _bgTree4;
    [Reactive] private byte _underworldBg;
    [Reactive] private byte _mushroomBg;

    [Reactive] private int _tilesWide;
    [Reactive] private int _tilesHighReactive;
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
    [Reactive] private float _bottomWorld;
    [Reactive] private float _topWorld;
    [Reactive] private float _rightWorld;
    [Reactive] private float _leftWorld;
    [Reactive] private bool _isV0;
    [Reactive] private int _spawnX;
    [Reactive] private int _spawnY;

    [Reactive] private int _dungeonX;
    [Reactive] private int _dungeonY;

    [Reactive] private bool _downedClown;

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
    [Reactive] private int _treeStyle0;
    [Reactive] private int _treeStyle1;
    [Reactive] private int _treeStyle2;
    [Reactive] private int _treeStyle3;

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
    [Reactive] private int _caveBackStyle0;
    [Reactive] private int _caveBackStyle1;
    [Reactive] private int _caveBackStyle2;
    [Reactive] private int _caveBackStyle3;
    [Reactive] private int _cultistDelay;
    [Reactive] private bool _apocalypse;
}
