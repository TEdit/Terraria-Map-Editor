using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TEdit.Common.Reactive;
using TEdit.Common.Reactive.Command;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Terraria.Objects;
using TEdit.Utility;

namespace TEdit.Terraria;

public partial class World : ObservableObject, ITileData
{
    public Vector2Int32 Size => new Vector2Int32(TilesWide, TilesHigh);

    Tile[,] ITileData.Tiles => this.Tiles;

    // backgrounds
    // 0 = tree 1 2 3 31 4 5 51 6 7 71 72 73 8
    // 1 = corruption 1
    // 2 = jungle 1
    // 3 = snow 1 2 21 22 3 31 32 4 41 42
    // 4 = hallow 1
    // 5 = crimson 1 2
    // 6 = desert 1
    // 7 = ocean 1 2

    private readonly ObservableCollection<String> _anglers = new ObservableCollection<String>();
    private readonly ObservableCollection<NpcName> _charNames = new ObservableCollection<NpcName>();
    private readonly ObservableCollection<Chest> _chests = new ObservableCollection<Chest>();
    private readonly ObservableCollection<NPC> _npcs = new ObservableCollection<NPC>();
    private readonly ObservableCollection<int> _shimmeredTownNPCs = new ObservableCollection<int>(Enumerable.Repeat(0, WorldConfiguration.MaxNpcID));
    private readonly ObservableCollection<int> _killedMobs = new ObservableCollection<int>(Enumerable.Repeat(0, WorldConfiguration.MaxNpcID));
    private readonly ObservableCollection<NPC> _mobs = new ObservableCollection<NPC>();
    private readonly ObservableCollection<Sign> _signs = new ObservableCollection<Sign>();
    private readonly ObservableCollection<TileEntity> _tileEntities = new ObservableCollection<TileEntity>();
    private readonly ObservableCollection<int> _partyingNPCs = new ObservableCollection<int>();
    private readonly ObservableCollection<PressurePlate> _pressurePlates = new ObservableCollection<PressurePlate>();
    private readonly ObservableCollection<TownManager> _playerRooms = new ObservableCollection<TownManager>();
    private Bestiary _bestiary = new Bestiary();
    private CreativePowers _creativePowers = new CreativePowers();

    public ObservableCollection<String> Anglers => _anglers;

    public ObservableCollection<int> ShimmeredTownNPCs => _shimmeredTownNPCs;

    public ObservableCollection<NPC> NPCs => _npcs;

    public ObservableCollection<int> KilledMobs => _killedMobs;

    public ObservableCollection<NPC> Mobs => _mobs;

    public List<Sign> Signs { get; } = new();

    public List<Chest> Chests { get; } = new();

    public ObservableCollection<NpcName> CharacterNames => _charNames;

    public List<TileEntity> TileEntities { get; } = new();

    public ObservableCollection<int> PartyingNPCs => _partyingNPCs;

    public ObservableCollection<PressurePlate> PressurePlates => _pressurePlates;

    public ObservableCollection<TownManager> PlayerRooms => _playerRooms;

    public Tile[,] Tiles;


    public DateTime LastSave
    {
        get => _lastSave;
        set => Set(nameof(LastSave), ref _lastSave, value);
    }

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
        bool bypassLimits = UnsafeGroundLayers;

        if (bypassLimits)
        {
            MaxCavernLevel = Calc.Clamp(TilesHigh, 0, TilesHigh);
            MaxGroundLevel = Calc.Clamp(TilesHigh - 6, 0, TilesHigh);
        }
        else
        {
            MaxCavernLevel = Calc.Clamp(TilesHigh - WorldConfiguration.CavernLevelToBottomOfWorld, 6, TilesHigh);
            MaxGroundLevel = Calc.Clamp(MaxCavernLevel - 6, 0, TilesHigh);
        }
    }

    public bool UnsafeGroundLayers
    {
        get => _unsafeGroundLayers;
        set
        {
            Set(nameof(UnsafeGroundLayers), ref _unsafeGroundLayers, value);
            UpdateMaxLayerLevels();
        }
    }

    public uint WorldVersion => Version;

    public Random Rand;
    public int[] TreeBG = new int[3];
    public int[] TreeMntBG = new int[2];
    public int[] TreeX = new int[3];
    public int[] CaveBackX = new int[4];
    public uint Version;
    private int _tileEntitiesNumber;
    private int _altarCount;
    private int _anglerQuest;
    private int _iceBackStyle;
    private int _jungleBackStyle;
    private int _hellBackStyle;
    private byte _bgCorruption;
    private byte _bgCrimson;
    private byte _bgDesert;
    private byte _bgHallow;
    private byte _bgJungle;
    private byte _bgOcean;
    private byte _bgSnow;
    private byte _bgTree;
    private bool _bloodMoon;
    private byte _sundialCooldown;
    private float _bottomWorld;
    private float _cloudBgActive;
    private bool _dayTime;
    private bool _downedBoss1;
    private bool _downedBoss2;
    private bool _downedBoss3;
    private bool _downedClown;
    private bool _downedFrost;
    private bool _downedGoblins;
    private bool _downedGolemBoss;
    private bool _downedSlimeKingBoss;
    private bool _downedMechBoss1;
    private bool _downedMechBoss2;
    private bool _downedMechBoss3;
    private bool _downedMechBossAny;
    private bool _downedPirates;
    private bool _downedPlantBoss;
    private bool _downedQueenBee;
    private bool _downedFishron;
    private bool _downedMartians;
    private bool _downedLunaticCultist;
    private bool _downedMoonlord;
    private bool _downedHalloweenKing;
    private bool _downedHalloweenTree;
    private bool _downedChristmasQueen;
    private bool _downedSanta;
    private bool _downedChristmasTree;
    private bool _downedCelestialSolar;
    private bool _downedCelestialVortex;
    private bool _downedCeslestialNebula;
    private bool _downedCelestialStardust;
    private bool _celestialSolarActive;
    private bool _celestialVortexActive;
    private bool _celestialNebulaActive;
    private bool _celestialStardustActive;
    private bool _apocalypse;
    private int _dungeonX;
    private int _dungeonY;
    private double _groundLevel;
    private bool _hardMode;
    private int _invasionDelay;
    private int _invasionSize;
    private int _invasionType;
    private int _invasionSizeStart;
    private int _cultistDelay;
    private double _invasionX;
    private double _slimeRainTime;
    private bool _isCrimson;
    private bool _isEclipse;
    private DateTime _lastSave;
    private float _leftWorld;
    private int _moonPhase;
    private byte _moonType;
    private short _numClouds;
    private float _rightWorld;
    private double _rockLevel;
    private bool _savedAngler;
    private bool _savedGoblin;
    private bool _savedMech;
    private bool _savedWizard;
    private bool _savedStylist;
    private bool _savedTaxCollector;
    private int _shadowOrbCount;
    private bool _shadowOrbSmashed;
    private bool _spawnMeteor;
    private bool _unsafeGroundLayers;
    private int _spawnX;
    private int _spawnY;
    private float _tempMaxRain;
    private int _tempRainTime;
    private bool _tempRaining;
    private int _tilesHigh;
    private int _tilesWide;
    private Int64 _creationTime;
    private double _time;
    private string _title;
    private float _topWorld;
    private byte[] _unknownData;
    private float _windSpeedSet;
    private int _worldId;
    private int _treeX0;
    private int _treeX1;
    private int _treeX2;
    private int _treeStyle0;
    private int _treeStyle1;
    private int _treeStyle2;
    private int _treeStyle3;
    private int _caveBackX0;
    private int _caveBackX1;
    private int _caveBackX2;
    private int _caveBackStyle0;
    private int _caveBackStyle1;
    private int _caveBackStyle2;
    private int _caveBackStyle3;
    private bool _fastForwardTime;
    private uint _fileRevision;
    private bool _isFavorite;
    private bool _partyManual;
    private bool _partyGenuine;
    private int _partyCooldown;
    private bool _sandStormHappening;
    private int _sandStormTimeLeft;
    private float _sandStormSeverity;
    private float _sandStormIntendedSeverity;
    private bool _savedBartender;
    private bool _downedDD2InvasionT1;
    private bool _downedDD2InvasionT2;
    private bool _downedDD2InvasionT3;
    private string _seed;
    private UInt64 _worldGenVersion;
    public Guid Guid;
    private int _gameMode;
    private bool _drunkWorld;
    private bool _savedGolfer;
    private byte _bgTree2;
    private byte _bgTree3;
    private byte _bgTree4;
    private byte _underworldBg;
    private byte _mushroomBg;
    private bool _combatBookUsed;
    private int _LanternNightCooldown;
    private bool _LanternNightGenuine;
    private bool _LanternNightManual;
    private bool _LanternNightNextNightIsGenuine;
    private bool _forceHalloweenForToday;
    private bool _forceXMasForToday;
    private int _savedOreTiersCopper;
    private int _savedOreTiersIron;
    private int _savedOreTiersSilver;
    private int _savedOreTiersGold;
    private bool _boughtCat;
    private bool _boughtDog;
    private bool _boughtBunny;
    private bool _downedEmpressOfLight;
    private bool _downedQueenSlime;
    private bool _downedDeerclops;
    private int _savedOreTiersCobalt;
    private int _savedOreTiersMythril;
    private int _savedOreTiersAdamantite;
    private bool _gooWorld;
    private bool _tenthAnniversaryWorld;
    private bool _dontStarveWorld;
    private bool _notTheBeesWorld;
    private int _maxCavernLevel;
    private int _maxGroundLevel;
    private bool _isV0;
    private bool _isChinese;
    private bool _zenithWorld;
    private bool _noTrapsWorld;
    private bool _remixWorld;
    private bool _isConsole;
    private bool _UnlockedSlimeBlueSpawn;
    private bool _UnlockedMerchantSpawn;
    private bool _UnlockedDemolitionistSpawn;
    private bool _UnlockedPartyGirlSpawn;
    private bool _UnlockedDyeTraderSpawn;
    private bool _UnlockedTruffleSpawn;
    private bool _UnlockedArmsDealerSpawn;
    private bool _UnlockedNurseSpawn;
    private bool _UnlockedPrincessSpawn;
    private bool _CombatBookVolumeTwoWasUsed;
    private bool _PeddlersSatchelWasUsed;
    private bool _UnlockedSlimeGreenSpawn;
    private bool _UnlockedSlimeOldSpawn;
    private bool _UnlockedSlimePurpleSpawn;
    private bool _UnlockedSlimeRainbowSpawn;
    private bool _UnlockedSlimeRedSpawn;
    private bool _UnlockedSlimeYellowSpawn;
    private bool _UnlockedSlimeCopperSpawn;
    private bool _FastForwardTimeToDusk;
    private byte _MoondialCooldown;
    private bool _AfterPartyOfDoom;
    private ObservableCollection<int> _treeTopVariations = new ObservableCollection<int>(new int[13]);

    public ObservableCollection<int> TreeTopVariations
    {
        get => _treeTopVariations;
        set => Set(nameof(TreeTopVariations), ref _treeTopVariations, value);
    }

    public Bestiary Bestiary
    {
        get => _bestiary;
        set => Set(nameof(Bestiary), ref _bestiary, value);
    }

    public CreativePowers CreativePowers
    {
        get => _creativePowers;
        set => Set(nameof(CreativePowers), ref _creativePowers, value);
    }

    public UInt64 WorldGenVersion
    {
        get => _worldGenVersion;
        set => Set(nameof(WorldGenVersion), ref _worldGenVersion, value);
    }


    [Category("World")]
    public string Seed
    {
        get => _seed;
        set => Set(nameof(Seed), ref _seed, value);
    }

    public bool SandStormHappening
    {
        get => _sandStormHappening;
        set => Set(nameof(SandStormHappening), ref _sandStormHappening, value);
    }

    public int SandStormTimeLeft
    {
        get => _sandStormTimeLeft;
        set => Set(nameof(SandStormTimeLeft), ref _sandStormTimeLeft, value);
    }

    public float SandStormSeverity
    {
        get => _sandStormSeverity;
        set => Set(nameof(SandStormSeverity), ref _sandStormSeverity, value);
    }

    public float SandStormIntendedSeverity
    {
        get => _sandStormIntendedSeverity;
        set => Set(nameof(SandStormIntendedSeverity), ref _sandStormIntendedSeverity, value);
    }

    public bool SavedBartender
    {
        get => _savedBartender;
        set => Set(nameof(SavedBartender), ref _savedBartender, value);
    }

    [Category("Bosses")]

    public bool DownedDD2InvasionT1
    {
        get => _downedDD2InvasionT1;
        set => Set(nameof(DownedDD2InvasionT1), ref _downedDD2InvasionT1, value);
    }

    [Category("Bosses")]
    public bool DownedDD2InvasionT2
    {
        get => _downedDD2InvasionT2;
        set => Set(nameof(DownedDD2InvasionT2), ref _downedDD2InvasionT2, value);
    }

    [Category("Bosses")]
    public bool DownedDD2InvasionT3
    {
        get => _downedDD2InvasionT3;
        set => Set(nameof(DownedDD2InvasionT3), ref _downedDD2InvasionT3, value);
    }

    public bool PartyManual
    {
        get => _partyManual;
        set => Set(nameof(PartyManual), ref _partyManual, value);
    }

    public bool PartyGenuine
    {
        get => _partyGenuine;
        set => Set(nameof(PartyGenuine), ref _partyGenuine, value);
    }

    public int PartyCooldown
    {
        get => _partyCooldown;
        set => Set(nameof(PartyCooldown), ref _partyCooldown, value);
    }

    public int TileEntitiesNumber
    {
        get => _tileEntitiesNumber;
        set => Set(nameof(TileEntitiesNumber), ref _tileEntitiesNumber, value);
    }

    public double SlimeRainTime
    {
        get => _slimeRainTime;
        set => Set(nameof(SlimeRainTime), ref _slimeRainTime, value);
    }

    public byte SundialCooldown
    {
        get => _sundialCooldown;
        set => Set(nameof(SundialCooldown), ref _sundialCooldown, value);
    }

    [Category("World")]
    public uint FileRevision
    {
        get => _fileRevision;
        set => Set(nameof(FileRevision), ref _fileRevision, value);
    }

    [Category("World")]
    public bool IsFavorite
    {
        get => _isFavorite;
        set => Set(nameof(IsFavorite), ref _isFavorite, value);
    }

    public byte[] UnknownData
    {
        get => _unknownData;
        set => Set(nameof(UnknownData), ref _unknownData, value);
    }

    public Int64 CreationTime
    {
        get => _creationTime;
        set => Set(nameof(CreationTime), ref _creationTime, value);
    }


    public int AnglerQuest
    {
        get => _anglerQuest;
        set => Set(nameof(AnglerQuest), ref _anglerQuest, value);
    }

    public bool SavedAngler
    {
        get => _savedAngler;
        set => Set(nameof(SavedAngler), ref _savedAngler, value);
    }

    public bool SavedStylist
    {
        get => _savedStylist;
        set => Set(nameof(SavedStylist), ref _savedStylist, value);
    }

    public bool SavedTaxCollector
    {
        get => _savedTaxCollector;
        set => Set(nameof(SavedTaxCollector), ref _savedTaxCollector, value);
    }

    public bool SavedGolfer
    {
        get => _savedGolfer;
        set => Set(nameof(SavedGolfer), ref _savedGolfer, value);
    }

    public bool ForceHalloweenForToday
    {
        get => _forceHalloweenForToday;
        set => Set(nameof(ForceHalloweenForToday), ref _forceHalloweenForToday, value);
    }

    public bool ForceXMasForToday
    {
        get => _forceXMasForToday;
        set => Set(nameof(ForceXMasForToday), ref _forceXMasForToday, value);
    }
    public int SavedOreTiersCobalt
    {
        get => _savedOreTiersCobalt;
        set => Set(nameof(SavedOreTiersCobalt), ref _savedOreTiersCobalt, value);
    }

    public int SavedOreTiersMythril
    {
        get => _savedOreTiersMythril;
        set => Set(nameof(SavedOreTiersMythril), ref _savedOreTiersMythril, value);
    }

    public int SavedOreTiersAdamantite
    {
        get => _savedOreTiersAdamantite;
        set => Set(nameof(SavedOreTiersAdamantite), ref _savedOreTiersAdamantite, value);
    }

    public int SavedOreTiersCopper
    {
        get => _savedOreTiersCopper;
        set => Set(nameof(SavedOreTiersCopper), ref _savedOreTiersCopper, value);
    }

    public int SavedOreTiersIron
    {
        get => _savedOreTiersIron;
        set => Set(nameof(SavedOreTiersIron), ref _savedOreTiersIron, value);
    }

    public int SavedOreTiersSilver
    {
        get => _savedOreTiersSilver;
        set => Set(nameof(SavedOreTiersSilver), ref _savedOreTiersSilver, value);
    }

    public int SavedOreTiersGold
    {
        get => _savedOreTiersGold;
        set => Set(nameof(SavedOreTiersGold), ref _savedOreTiersGold, value);
    }

    public bool BoughtCat
    {
        get => _boughtCat;
        set => Set(nameof(BoughtCat), ref _boughtCat, value);
    }

    public bool BoughtDog
    {
        get => _boughtDog;
        set => Set(nameof(BoughtDog), ref _boughtDog, value);
    }

    public bool BoughtBunny
    {
        get => _boughtBunny;
        set => Set(nameof(BoughtBunny), ref _boughtBunny, value);
    }

    public bool DownedEmpressOfLight
    {
        get => _downedEmpressOfLight;
        set => Set(nameof(DownedEmpressOfLight), ref _downedEmpressOfLight, value);
    }

    public bool DownedQueenSlime
    {
        get => _downedQueenSlime;
        set => Set(nameof(DownedQueenSlime), ref _downedQueenSlime, value);
    }

    public bool DownedDeerclops
    {
        get => _downedDeerclops;
        set => Set(nameof(DownedDeerclops), ref _downedDeerclops, value);
    }

    public int IceBackStyle
    {
        get => _iceBackStyle;
        set => Set(nameof(IceBackStyle), ref _iceBackStyle, value);
    }

    public int JungleBackStyle
    {
        get => _jungleBackStyle;
        set => Set(nameof(JungleBackStyle), ref _jungleBackStyle, value);
    }

    public int HellBackStyle
    {
        get => _hellBackStyle;
        set => Set(nameof(HellBackStyle), ref _hellBackStyle, value);
    }

    public bool IsEclipse
    {
        get => _isEclipse;
        set => Set(nameof(IsEclipse), ref _isEclipse, value);
    }

    public bool IsCrimson
    {
        get => _isCrimson;
        set => Set(nameof(IsCrimson), ref _isCrimson, value);
    }

    public float WindSpeedSet
    {
        get => _windSpeedSet;
        set => Set(nameof(WindSpeedSet), ref _windSpeedSet, value);
    }

    public short NumClouds
    {
        get => _numClouds;
        set => Set(nameof(NumClouds), ref _numClouds, value);
    }

    public float CloudBgActive
    {
        get => _cloudBgActive;
        set => Set(nameof(CloudBgActive), ref _cloudBgActive, value);
    }

    public int TreeTop1
    {
        get => _treeTopVariations[0];
        set
        {
            _treeTopVariations[0] = (int)value;
            RaisePropertyChanged(nameof(TreeTop1));
        }
    }

    public int TreeTop2
    {
        get => _treeTopVariations[1];
        set
        {
            _treeTopVariations[1] = (int)value;
            RaisePropertyChanged(nameof(TreeTop2));
        }
    }

    public int TreeTop3
    {
        get => _treeTopVariations[2];
        set
        {
            _treeTopVariations[2] = (int)value;
            RaisePropertyChanged(nameof(TreeTop3));
        }
    }

    public int TreeTop4
    {
        get => _treeTopVariations[3];
        set
        {
            _treeTopVariations[3] = (int)value;
            RaisePropertyChanged(nameof(TreeTop4));
        }
    }

    public bool GoodWorld
    {
        get => _gooWorld;
        set => Set(nameof(GoodWorld), ref _gooWorld, value);
    }

    public bool TenthAnniversaryWorld
    {
        get => _tenthAnniversaryWorld;
        set => Set(nameof(TenthAnniversaryWorld), ref _tenthAnniversaryWorld, value);
    }

    public bool DontStarveWorld
    {
        get => _dontStarveWorld;
        set => Set(nameof(DontStarveWorld), ref _dontStarveWorld, value);
    }
    public bool NotTheBeesWorld
    {
        get => _notTheBeesWorld;
        set => Set(nameof(NotTheBeesWorld), ref _notTheBeesWorld, value);
    }

    public bool RemixWorld
    {
        get => _remixWorld;
        set => Set(nameof(RemixWorld), ref _remixWorld, value);
    }


    public bool NoTrapsWorld
    {
        get => _noTrapsWorld;
        set => Set(nameof(NoTrapsWorld), ref _noTrapsWorld, value);
    }


    public bool ZenithWorld
    {
        get => _zenithWorld;
        set => Set(nameof(ZenithWorld), ref _zenithWorld, value);
    }


    public byte MoonType
    {
        get => _moonType;
        set => Set(nameof(MoonType), ref _moonType, value);
    }


    public byte BgOcean
    {
        get => _bgOcean;
        set => Set(nameof(BgOcean), ref _bgOcean, value);
    }

    public byte BgDesert
    {
        get => _bgDesert;
        set => Set(nameof(BgDesert), ref _bgDesert, value);
    }

    public byte BgCrimson
    {
        get => _bgCrimson;
        set => Set(nameof(BgCrimson), ref _bgCrimson, value);
    }

    public byte BgHallow
    {
        get => _bgHallow;
        set => Set(nameof(BgHallow), ref _bgHallow, value);
    }

    public byte BgSnow
    {
        get => _bgSnow;
        set => Set(nameof(BgSnow), ref _bgSnow, value);
    }

    public byte BgJungle
    {
        get => _bgJungle;
        set => Set(nameof(BgJungle), ref _bgJungle, value);
    }

    public byte BgCorruption
    {
        get => _bgCorruption;
        set => Set(nameof(BgCorruption), ref _bgCorruption, value);
    }

    public byte BgTree
    {
        get => _bgTree;
        set => Set(nameof(BgTree), ref _bgTree, value);
    }

    public byte Bg8
    {
        get => _bgTree;
        set => Set(nameof(BgTree), ref _bgTree, value);
    }

    public byte BgTree2
    {
        get => _bgTree2;
        set => Set(nameof(BgTree2), ref _bgTree2, value);
    }

    public byte BgTree3
    {
        get => _bgTree3;
        set => Set(nameof(BgTree3), ref _bgTree3, value);
    }

    public byte BgTree4
    {
        get => _bgTree4;
        set => Set(nameof(BgTree4), ref _bgTree4, value);
    }

    public byte UnderworldBg
    {
        get => _underworldBg;
        set => Set(nameof(UnderworldBg), ref _underworldBg, value);
    }

    public byte MushroomBg
    {
        get => _mushroomBg;
        set => Set(nameof(MushroomBg), ref _mushroomBg, value);
    }

    public bool CombatBookUsed
    {
        get => _combatBookUsed;
        set => Set(nameof(CombatBookUsed), ref _combatBookUsed, value);
    }

    public int LanternNightCooldown
    {
        get => _LanternNightCooldown;
        set => Set(nameof(LanternNightCooldown), ref _LanternNightCooldown, value);
    }

    public bool LanternNightGenuine
    {
        get => _LanternNightGenuine;
        set => Set(nameof(LanternNightGenuine), ref _LanternNightGenuine, value);
    }

    public bool LanternNightManual
    {
        get => _LanternNightManual;
        set => Set(nameof(LanternNightManual), ref _LanternNightManual, value);
    }

    public bool LanternNightNextNightIsGenuine
    {
        get => _LanternNightNextNightIsGenuine;
        set => Set(nameof(LanternNightNextNightIsGenuine), ref _LanternNightNextNightIsGenuine, value);
    }

    public float TempMaxRain
    {
        get => _tempMaxRain;
        set => Set(nameof(TempMaxRain), ref _tempMaxRain, value);
    }

    public int TempRainTime
    {
        get => _tempRainTime;
        set => Set(nameof(TempRainTime), ref _tempRainTime, value);
    }

    public bool TempRaining
    {
        get => _tempRaining;
        set => Set(nameof(TempRaining), ref _tempRaining, value);
    }

    [Category("Bosses")]
    public bool DownedPirates
    {
        get => _downedPirates;
        set => Set(nameof(DownedPirates), ref _downedPirates, value);
    }

    [Category("Bosses")]
    public bool DownedGolemBoss
    {
        get => _downedGolemBoss;
        set => Set(nameof(DownedGolemBoss), ref _downedGolemBoss, value);
    }

    [Category("Bosses")]
    public bool DownedSlimeKingBoss
    {
        get => _downedSlimeKingBoss;
        set => Set(nameof(DownedSlimeKingBoss), ref _downedSlimeKingBoss, value);
    }

    [Category("Bosses")]
    public bool DownedPlantBoss
    {
        get => _downedPlantBoss;
        set => Set(nameof(DownedPlantBoss), ref _downedPlantBoss, value);
    }

    [Category("Bosses")]
    public bool DownedMechBossAny
    {
        get => _downedMechBossAny;
        set => Set(nameof(DownedMechBossAny), ref _downedMechBossAny, value);
    }

    [Category("Bosses")]
    public bool DownedMechBoss3
    {
        get => _downedMechBoss3;
        set
        {
            _downedMechBoss3 = value;
            if (value)
                DownedMechBossAny = true;
            if (!value && !DownedMechBoss2 && !DownedMechBoss1)
                DownedMechBossAny = false;
        }
    }

    [Category("Bosses")]
    public bool DownedMechBoss2
    {
        get => _downedMechBoss2;
        set
        {
            _downedMechBoss2 = value;
            if (value)
                DownedMechBossAny = true;
            if (!value && !DownedMechBoss3 && !DownedMechBoss1)
                DownedMechBossAny = false;
        }
    }

    [Category("Bosses")]
    public bool DownedMechBoss1
    {
        get => _downedMechBoss1;
        set
        {
            _downedMechBoss1 = value;
            if (value)
                DownedMechBossAny = true;
            if (!value && !DownedMechBoss2 && !DownedMechBoss3)
                DownedMechBossAny = false;
        }
    }

    [Category("Bosses")]
    public bool DownedQueenBee
    {
        get => _downedQueenBee;
        set => Set(nameof(DownedQueenBee), ref _downedQueenBee, value);
    }

    [Category("Bosses")]
    public bool DownedFishron
    {
        get => _downedFishron;
        set => Set(nameof(DownedFishron), ref _downedFishron, value);
    }

    [Category("Bosses")]
    public bool DownedMartians
    {
        get => _downedMartians;
        set => Set(nameof(DownedMartians), ref _downedMartians, value);
    }

    [Category("Bosses")]
    public bool DownedLunaticCultist
    {
        get => _downedLunaticCultist;
        set => Set(nameof(DownedLunaticCultist), ref _downedLunaticCultist, value);
    }

    [Category("Bosses")]
    public bool DownedMoonlord
    {
        get => _downedMoonlord;
        set => Set(nameof(DownedMoonlord), ref _downedMoonlord, value);
    }

    [Category("Bosses")]
    public bool DownedHalloweenKing
    {
        get => _downedHalloweenKing;
        set => Set(nameof(DownedHalloweenKing), ref _downedHalloweenKing, value);
    }

    [Category("Bosses")]
    public bool DownedHalloweenTree
    {
        get => _downedHalloweenTree;
        set => Set(nameof(DownedHalloweenTree), ref _downedHalloweenTree, value);
    }

    [Category("Bosses")]
    public bool DownedChristmasQueen
    {
        get => _downedChristmasQueen;
        set => Set(nameof(DownedChristmasQueen), ref _downedChristmasQueen, value);
    }

    [Category("Bosses")]
    public bool DownedSanta
    {
        get => _downedSanta;
        set => Set(nameof(DownedSanta), ref _downedSanta, value);
    }

    [Category("Bosses")]
    [DisplayName("Target Name")]
    public bool DownedChristmasTree
    {
        get => _downedChristmasTree;
        set => Set(nameof(DownedChristmasTree), ref _downedChristmasTree, value);
    }

    [Category("Bosses")]
    public bool DownedCelestialSolar
    {
        get => _downedCelestialSolar;
        set => Set(nameof(DownedCelestialSolar), ref _downedCelestialSolar, value);
    }

    [Category("Bosses")]
    public bool DownedCelestialVortex
    {
        get => _downedCelestialVortex;
        set => Set(nameof(DownedCelestialVortex), ref _downedCelestialVortex, value);
    }

    [Category("Bosses")]
    public bool DownedCelestialNebula
    {
        get => _downedCeslestialNebula;
        set => Set(nameof(DownedCelestialNebula), ref _downedCeslestialNebula, value);
    }

    [Category("Bosses")]
    public bool DownedCelestialStardust
    {
        get => _downedCelestialStardust;
        set => Set(nameof(DownedCelestialStardust), ref _downedCelestialStardust, value);
    }

    [Category("Bosses")]
    public bool CelestialSolarActive
    {
        get => _celestialSolarActive;
        set => Set(nameof(CelestialSolarActive), ref _celestialSolarActive, value);
    }

    [Category("Bosses")]
    public bool CelestialVortexActive
    {
        get => _celestialVortexActive;
        set => Set(nameof(CelestialVortexActive), ref _celestialVortexActive, value);
    }

    [Category("Bosses")]
    public bool CelestialNebulaActive
    {
        get => _celestialNebulaActive;
        set => Set(nameof(CelestialNebulaActive), ref _celestialNebulaActive, value);
    }

    [Category("Bosses")]
    public bool CelestialStardustActive
    {
        get => _celestialStardustActive;
        set => Set(nameof(CelestialStardustActive), ref _celestialStardustActive, value);
    }

    public int GameMode
    {
        get => _gameMode;
        set => Set(nameof(GameMode), ref _gameMode, value);
    }

    public bool DrunkWorld
    {
        get => _drunkWorld;
        set => Set(nameof(DrunkWorld), ref _drunkWorld, value);
    }

    public int TilesWide
    {
        get => _tilesWide;
        set => Set(nameof(TilesWide), ref _tilesWide, value);
    }

    public int TilesHigh
    {
        get => _tilesHigh;
        set
        {
            Set(nameof(TilesHigh), ref _tilesHigh, value);
            UpdateMaxLayerLevels();
        }
    }

    public float BottomWorld
    {
        get => _bottomWorld;
        set => Set(nameof(BottomWorld), ref _bottomWorld, value);
    }

    public float TopWorld
    {
        get => _topWorld;
        set => Set(nameof(TopWorld), ref _topWorld, value);
    }

    public float RightWorld
    {
        get => _rightWorld;
        set => Set(nameof(RightWorld), ref _rightWorld, value);
    }

    public float LeftWorld
    {
        get => _leftWorld;
        set => Set(nameof(LeftWorld), ref _leftWorld, value);
    }

    [Category("World")]
    public int WorldId
    {
        get => _worldId;
        set => Set(nameof(WorldId), ref _worldId, value);
    }

    [Category("World")]
    public System.Guid WorldGUID
    {
        get => Guid;
        set => Set(nameof(WorldGUID), ref Guid, value);
    }

    public bool DownedFrost
    {
        get => _downedFrost;
        set => Set(nameof(DownedFrost), ref _downedFrost, value);
    }

    public bool IsV0
    {
        get => _isV0;
        set => Set(nameof(IsV0), ref _isV0, value);
    }

    [Category("World")]
    public bool IsChinese
    {
        get => _isChinese;
        set => Set(nameof(IsChinese), ref _isChinese, value);
    }

    [Category("World")]
    public bool IsConsole
    {
        get => _isConsole;
        set => Set(nameof(IsConsole), ref _isConsole, value);
    }

    [Category("World")]
    public string Title
    {
        get => _title;
        set => Set(nameof(Title), ref _title, value);
    }

    public int SpawnX
    {
        get => _spawnX;
        set => Set(nameof(SpawnX), ref _spawnX, value);
    }

    public int SpawnY
    {
        get => _spawnY;
        set => Set(nameof(SpawnY), ref _spawnY, value);
    }

    public int MaxCavernLevel
    {
        get => _maxCavernLevel;
        set => Set(nameof(MaxCavernLevel), ref _maxCavernLevel, value);
    }

    public int MaxGroundLevel
    {
        get => _maxGroundLevel;
        set => Set(nameof(MaxGroundLevel), ref _maxGroundLevel, value);
    }

    public double GroundLevel
    {
        get => _groundLevel;
        set
        {
            if (value < 0) { return; } // skip if negative
            if (value > MaxGroundLevel) value = MaxGroundLevel;

            Set(nameof(GroundLevel), ref _groundLevel, value);

            // if levels touch, shift by 6
            if (GroundLevel >= RockLevel)
            {
                RockLevel = GroundLevel + 6;
            }
        }
    }

    public double RockLevel
    {
        get => _rockLevel;
        set
        {
            if (value < 0) { return; } // skip if negative
            if (value > MaxCavernLevel) value = MaxCavernLevel;

            Set(nameof(RockLevel), ref _rockLevel, value);

            // if levels touch, shift by 6
            if (GroundLevel >= RockLevel)
            {
                GroundLevel = RockLevel - 6;
            }
        }
    }

    public double Time
    {
        get => _time;
        set => Set(nameof(Time), ref _time, value);
    }

    public bool DayTime
    {
        get => _dayTime;
        set => Set(nameof(DayTime), ref _dayTime, value);
    }

    public int MoonPhase
    {
        get => _moonPhase;
        set => Set(nameof(MoonPhase), ref _moonPhase, value);
    }

    public bool BloodMoon
    {
        get => _bloodMoon;
        set => Set(nameof(BloodMoon), ref _bloodMoon, value);
    }

    public int DungeonX
    {
        get => _dungeonX;
        set => Set(nameof(DungeonX), ref _dungeonX, value);
    }

    public int DungeonY
    {
        get => _dungeonY;
        set => Set(nameof(DungeonY), ref _dungeonY, value);
    }

    public bool DownedBoss1
    {
        get => _downedBoss1;
        set => Set(nameof(DownedBoss1), ref _downedBoss1, value);
    }

    public bool DownedBoss2
    {
        get => _downedBoss2;
        set => Set(nameof(DownedBoss2), ref _downedBoss2, value);
    }

    public bool DownedBoss3
    {
        get => _downedBoss3;
        set => Set(nameof(DownedBoss3), ref _downedBoss3, value);
    }

    public bool SavedGoblin
    {
        get => _savedGoblin;
        set => Set(nameof(SavedGoblin), ref _savedGoblin, value);
    }

    public bool SavedWizard
    {
        get => _savedWizard;
        set => Set(nameof(SavedWizard), ref _savedWizard, value);
    }

    public bool DownedGoblins
    {
        get => _downedGoblins;
        set => Set(nameof(DownedGoblins), ref _downedGoblins, value);
    }

    public bool SavedMech
    {
        get => _savedMech;
        set => Set(nameof(SavedMech), ref _savedMech, value);
    }

    public bool DownedClown
    {
        get => _downedClown;
        set => Set(nameof(DownedClown), ref _downedClown, value);
    }

    public bool ShadowOrbSmashed
    {
        get => _shadowOrbSmashed;
        set => Set(nameof(ShadowOrbSmashed), ref _shadowOrbSmashed, value);
    }

    public bool SpawnMeteor
    {
        get => _spawnMeteor;
        set => Set(nameof(SpawnMeteor), ref _spawnMeteor, value);
    }

    public int ShadowOrbCount
    {
        get => _shadowOrbCount;
        set
        {
            Set(nameof(ShadowOrbCount), ref _shadowOrbCount, value);
            ShadowOrbSmashed = _shadowOrbCount > 0;
        }
    }

    public int AltarCount
    {
        get => _altarCount;
        set => Set(nameof(AltarCount), ref _altarCount, value);
    }

    public bool HardMode
    {
        get => _hardMode;
        set => Set(nameof(HardMode), ref _hardMode, value);
    }

    public double InvasionX
    {
        get => _invasionX;
        set => Set(nameof(InvasionX), ref _invasionX, value);
    }

    public int InvasionType
    {
        get => _invasionType;
        set => Set(nameof(InvasionType), ref _invasionType, value);
    }

    public int InvasionSize
    {
        get => _invasionSize;
        set => Set(nameof(InvasionSize), ref _invasionSize, value);
    }

    public int InvasionSizeStart
    {
        get => _invasionSizeStart;
        set => Set(nameof(InvasionSizeStart), ref _invasionSizeStart, value);
    }

    public int InvasionDelay
    {
        get => _invasionDelay;
        set => Set(nameof(InvasionDelay), ref _invasionDelay, value);
    }

    public int TreeX0
    {
        get => _treeX0;
        set
        {
            Set(nameof(TreeX0), ref _treeX0, value);
            if (_treeX0 > _treeX1)
                TreeX0 = _treeX1;
        }
    }

    public int TreeX1
    {
        get => _treeX1;
        set
        {
            Set(nameof(TreeX1), ref _treeX1, value);
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
            Set(nameof(TreeX2), ref _treeX2, value);
            if (_treeX2 < _treeX1)
                TreeX2 = _treeX1;
        }
    }

    public int TreeStyle0
    {
        get => _treeStyle0;
        set => Set(nameof(TreeStyle0), ref _treeStyle0, value);
    }

    public int TreeStyle1
    {
        get => _treeStyle1;
        set => Set(nameof(TreeStyle1), ref _treeStyle1, value);
    }

    public int TreeStyle2
    {
        get => _treeStyle2;
        set => Set(nameof(TreeStyle2), ref _treeStyle2, value);
    }

    public int TreeStyle3
    {
        get => _treeStyle3;
        set => Set(nameof(TreeStyle3), ref _treeStyle3, value);
    }

    public int CaveBackX0
    {
        get => _caveBackX0;
        set
        {
            Set(nameof(CaveBackX0), ref _caveBackX0, value);
            if (_caveBackX0 > _caveBackX1)
                CaveBackX0 = _caveBackX1;
        }
    }

    public int CaveBackX1
    {
        get => _caveBackX1;
        set
        {
            Set(nameof(CaveBackX1), ref _caveBackX1, value);
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
            Set(nameof(CaveBackX2), ref _caveBackX2, value);
            if (_caveBackX2 < _caveBackX1)
                CaveBackX2 = _caveBackX1;
        }
    }

    public int CaveBackStyle0
    {
        get => _caveBackStyle0;
        set => Set(nameof(CaveBackStyle0), ref _caveBackStyle0, value);
    }

    public int CaveBackStyle1
    {
        get => _caveBackStyle1;
        set => Set(nameof(CaveBackStyle1), ref _caveBackStyle1, value);
    }

    public int CaveBackStyle2
    {
        get => _caveBackStyle2;
        set => Set(nameof(CaveBackStyle2), ref _caveBackStyle2, value);
    }

    public int CaveBackStyle3
    {
        get => _caveBackStyle3;
        set => Set(nameof(CaveBackStyle3), ref _caveBackStyle3, value);
    }

    public int CultistDelay
    {
        get => _cultistDelay;
        set => Set(nameof(CultistDelay), ref _cultistDelay, value);
    }

    public bool FastForwardTime
    {
        get => _fastForwardTime;
        set => Set(nameof(FastForwardTime), ref _fastForwardTime, value);
    }

    public bool Apocalypse
    {
        get => _apocalypse;
        set => Set(nameof(Apocalypse), ref _apocalypse, value);
    }

    public bool AfterPartyOfDoom
    {
        get => _AfterPartyOfDoom;
        set => Set(nameof(AfterPartyOfDoom), ref _AfterPartyOfDoom, value);
    }
    public byte MoondialCooldown
    {
        get => _MoondialCooldown;
        set => Set(nameof(MoondialCooldown), ref _MoondialCooldown, value);
    }
    public bool FastForwardTimeToDusk
    {
        get => _FastForwardTimeToDusk;
        set => Set(nameof(FastForwardTimeToDusk), ref _FastForwardTimeToDusk, value);
    }
    public bool UnlockedSlimeCopperSpawn
    {
        get => _UnlockedSlimeCopperSpawn;
        set => Set(nameof(UnlockedSlimeCopperSpawn), ref _UnlockedSlimeCopperSpawn, value);
    }
    public bool UnlockedSlimeYellowSpawn
    {
        get => _UnlockedSlimeYellowSpawn;
        set => Set(nameof(UnlockedSlimeYellowSpawn), ref _UnlockedSlimeYellowSpawn, value);
    }
    public bool UnlockedSlimeRedSpawn
    {
        get => _UnlockedSlimeRedSpawn;
        set => Set(nameof(UnlockedSlimeRedSpawn), ref _UnlockedSlimeRedSpawn, value);
    }
    public bool UnlockedSlimeRainbowSpawn
    {
        get => _UnlockedSlimeRainbowSpawn;
        set => Set(nameof(UnlockedSlimeRainbowSpawn), ref _UnlockedSlimeRainbowSpawn, value);
    }
    public bool UnlockedSlimePurpleSpawn
    {
        get => _UnlockedSlimePurpleSpawn;
        set => Set(nameof(UnlockedSlimePurpleSpawn), ref _UnlockedSlimePurpleSpawn, value);
    }
    public bool UnlockedSlimeOldSpawn
    {
        get => _UnlockedSlimeOldSpawn;
        set => Set(nameof(UnlockedSlimeOldSpawn), ref _UnlockedSlimeOldSpawn, value);
    }
    public bool UnlockedSlimeGreenSpawn
    {
        get => _UnlockedSlimeGreenSpawn;
        set => Set(nameof(UnlockedSlimeGreenSpawn), ref _UnlockedSlimeGreenSpawn, value);
    }
    public bool PeddlersSatchelWasUsed
    {
        get => _PeddlersSatchelWasUsed;
        set => Set(nameof(PeddlersSatchelWasUsed), ref _PeddlersSatchelWasUsed, value);
    }
    public bool CombatBookVolumeTwoWasUsed
    {
        get => _CombatBookVolumeTwoWasUsed;
        set => Set(nameof(CombatBookVolumeTwoWasUsed), ref _CombatBookVolumeTwoWasUsed, value);
    }
    public bool UnlockedPrincessSpawn
    {
        get => _UnlockedPrincessSpawn;
        set => Set(nameof(UnlockedPrincessSpawn), ref _UnlockedPrincessSpawn, value);
    }
    public bool UnlockedNurseSpawn
    {
        get => _UnlockedNurseSpawn;
        set => Set(nameof(UnlockedNurseSpawn), ref _UnlockedNurseSpawn, value);
    }
    public bool UnlockedArmsDealerSpawn
    {
        get => _UnlockedArmsDealerSpawn;
        set => Set(nameof(UnlockedArmsDealerSpawn), ref _UnlockedArmsDealerSpawn, value);
    }
    public bool UnlockedTruffleSpawn
    {
        get => _UnlockedTruffleSpawn;
        set => Set(nameof(UnlockedTruffleSpawn), ref _UnlockedTruffleSpawn, value);
    }
    public bool UnlockedDyeTraderSpawn
    {
        get => _UnlockedDyeTraderSpawn;
        set => Set(nameof(UnlockedDyeTraderSpawn), ref _UnlockedDyeTraderSpawn, value);
    }
    public bool UnlockedPartyGirlSpawn
    {
        get => _UnlockedPartyGirlSpawn;
        set => Set(nameof(UnlockedPartyGirlSpawn), ref _UnlockedPartyGirlSpawn, value);
    }
    public bool UnlockedDemolitionistSpawn
    {
        get => _UnlockedDemolitionistSpawn;
        set => Set(nameof(UnlockedDemolitionistSpawn), ref _UnlockedDemolitionistSpawn, value);
    }
    public bool UnlockedMerchantSpawn
    {
        get => _UnlockedMerchantSpawn;
        set => Set(nameof(UnlockedMerchantSpawn), ref _UnlockedMerchantSpawn, value);
    }
    public bool UnlockedSlimeBlueSpawn
    {
        get => _UnlockedSlimeBlueSpawn;
        set => Set(nameof(UnlockedSlimeBlueSpawn), ref _UnlockedSlimeBlueSpawn, value);
    }
}
