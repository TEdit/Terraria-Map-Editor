using System;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using TEdit.Editor;
using TEdit.Geometry.Primitives;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria
{
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

        public static byte MaxMoons = 3;
        private readonly ObservableCollection<String> _anglers = new ObservableCollection<String>();
        private readonly ObservableCollection<NpcName> _charNames = new ObservableCollection<NpcName>();
        private readonly ObservableCollection<Chest> _chests = new ObservableCollection<Chest>();
        private readonly ObservableCollection<NPC> _npcs = new ObservableCollection<NPC>();
        private readonly ObservableCollection<int> _killedMobs = new ObservableCollection<int>();
        private readonly ObservableCollection<NPC> _mobs = new ObservableCollection<NPC>();
        private readonly ObservableCollection<Sign> _signs = new ObservableCollection<Sign>();
        private readonly ObservableCollection<TileEntity> _tileEntities = new ObservableCollection<TileEntity>();
        private readonly ObservableCollection<int> _partyingNPCs = new ObservableCollection<int>();
        private readonly ObservableCollection<PressurePlate> _pressurePlates = new ObservableCollection<PressurePlate>();
        private readonly ObservableCollection<TownManager> _playerRooms = new ObservableCollection<TownManager>();

        private ObservableCollection<int> _treeTopVariations = new ObservableCollection<int>(new int[13]);

        // [SBLogic] These variables are used internally for composing background layers, not currently needed here:
        // public int[] CorruptBG = new int[3];
        // public int[] CrimsonBG = new int[3];
        // public int[] DesertBG = new int[2];
        // public int[] HallowBG = new int[3];
        // public int[] JungleBG = new int[3];
        // public int[] SnowBG = new int[3];
        // public int[] SnowMntBG = new int[2];

        // [SBLogic] Moved these to private with a public interface method:
        // public int HellBackStyle;
        // public int IceBackStyle;
        // public int JungleBackStyle;

        // [SBLogic] Unrolled arrays to discrete private variables (possibly a better way to do this?):
        // public int[] TreeStyle = new int[4];
        // public int[] CaveBackStyle = new int[4];
        public Random Rand;
        public Tile[,] Tiles;
        public int[] TreeBG = new int[3];
        public int[] TreeMntBG = new int[2];
        // [SBLogic] Still using TreeX and CaveBackX to read in from file to ensure sliders work correctly:
        public int[] TreeX = new int[3];
        public int[] CaveBackX = new int[4];
        public uint Version;
        private int _tileEntitiesNumber;
        private int _numberOfMobs;
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
        private int _spawnX;
        private int _spawnY;
        private float _tempMaxRain;
        private int _tempRainTime;
        private bool _tempRaining;
        private int _tilesHigh;
        private int _tilesWide;
        private bool _expertMode;
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
        private int _tempLanternNightCooldown;
        private bool _tempLanternNightGenuine;
        private bool _tempLanternNightManual;
        private bool _tempLanternNightNextNightIsGenuine;
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
        private int _savedOreTiersCobalt;
        private int _savedOreTiersMythril;
        private int _savedOreTiersAdamantite;
        private Bestiary _bestiary = new Bestiary();
        private CreativePowers _creativePowers = new CreativePowers();
        private bool _gooWorld;

        

        public Bestiary Bestiary
        {
            get { return _bestiary; }
            set { Set(nameof(Bestiary), ref _bestiary, value); }
        }

        public CreativePowers CreativePowers
        {
            get { return _creativePowers; }
            set { Set(nameof(CreativePowers), ref _creativePowers, value); }
        }

        public UInt64 WorldGenVersion
        {
            get { return _worldGenVersion; }
            set { Set(nameof(WorldGenVersion), ref _worldGenVersion, value); }
        }

        public string Seed
        {
            get { return _seed; }
            set { Set(nameof(Seed), ref _seed, value); }
        }

        public bool SandStormHappening
        {
            get { return _sandStormHappening; }
            set { Set(nameof(SandStormHappening), ref _sandStormHappening, value); }
        }

        public int SandStormTimeLeft
        {
            get { return _sandStormTimeLeft; }
            set { Set(nameof(SandStormTimeLeft), ref _sandStormTimeLeft, value); }
        }

        public float SandStormSeverity
        {
            get { return _sandStormSeverity; }
            set { Set(nameof(SandStormSeverity), ref _sandStormSeverity, value); }
        }

        public float SandStormIntendedSeverity
        {
            get { return _sandStormIntendedSeverity; }
            set { Set(nameof(SandStormIntendedSeverity), ref _sandStormIntendedSeverity, value); }
        }

        public bool SavedBartender
        {
            get { return _savedBartender; }
            set { Set(nameof(SavedBartender), ref _savedBartender, value); }
        }

        public bool DownedDD2InvasionT1
        {
            get { return _downedDD2InvasionT1; }
            set { Set("DownedDD2InvasionT1", ref _downedDD2InvasionT1, value); }
        }

        public bool DownedDD2InvasionT2
        {
            get { return _downedDD2InvasionT2; }
            set { Set("DownedDD2InvasionT2", ref _downedDD2InvasionT2, value); }
        }

        public bool DownedDD2InvasionT3
        {
            get { return _downedDD2InvasionT3; }
            set { Set("DownedDD2InvasionT3", ref _downedDD2InvasionT3, value); }
        }

        public bool PartyManual
        {
            get { return _partyManual; }
            set { Set(nameof(PartyManual), ref _partyManual, value); }
        }

        public bool PartyGenuine
        {
            get { return _partyGenuine; }
            set { Set(nameof(PartyGenuine), ref _partyGenuine, value); }
        }

        public int PartyCooldown
        {
            get { return _partyCooldown; }
            set { Set(nameof(PartyCooldown), ref _partyCooldown, value); }
        }

        public int TileEntitiesNumber
        {
            get { return _tileEntitiesNumber; }
            set { Set(nameof(TileEntitiesNumber), ref _tileEntitiesNumber, value); }
        }

        public int NumberOfMobs
        {
            get { return _numberOfMobs; }
            set { Set(nameof(NumberOfMobs), ref _numberOfMobs, value); }
        }

        public double SlimeRainTime
        {
            get { return _slimeRainTime; }
            set { Set(nameof(SlimeRainTime), ref _slimeRainTime, value); }
        }

        public byte SundialCooldown
        {
            get { return _sundialCooldown; }
            set { Set(nameof(SundialCooldown), ref _sundialCooldown, value); }
        }

        public uint FileRevision
        {
            get { return _fileRevision; }
            set { Set(nameof(FileRevision), ref _fileRevision, value); }
        }

        public bool IsFavorite
        {
            get { return _isFavorite; }
            set { Set(nameof(IsFavorite), ref _isFavorite, value); }
        }

        public byte[] UnknownData
        {
            get { return _unknownData; }
            set { Set(nameof(UnknownData), ref _unknownData, value); }
        }

        public Int64 CreationTime
        {
            get { return _creationTime; }
            set { Set(nameof(CreationTime), ref _creationTime, value); }
        }


        public int AnglerQuest
        {
            get { return _anglerQuest; }
            set { Set(nameof(AnglerQuest), ref _anglerQuest, value); }
        }

        public bool SavedAngler
        {
            get { return _savedAngler; }
            set { Set(nameof(SavedAngler), ref _savedAngler, value); }
        }

        public bool SavedStylist
        {
            get { return _savedStylist; }
            set { Set(nameof(SavedStylist), ref _savedStylist, value); }
        }

        public bool SavedTaxCollector
        {
            get { return _savedTaxCollector; }
            set { Set(nameof(SavedTaxCollector), ref _savedTaxCollector, value); }
        }

        public bool SavedGolfer
        {
            get { return _savedGolfer; }
            set { Set(nameof(SavedGolfer), ref _savedGolfer, value); }
        }

        public bool ForceHalloweenForToday
        {
            get { return _forceHalloweenForToday; }
            set { Set(nameof(ForceHalloweenForToday), ref _forceHalloweenForToday, value); }
        }

        public bool ForceXMasForToday
        {
            get { return _forceXMasForToday; }
            set { Set(nameof(ForceXMasForToday), ref _forceXMasForToday, value); }
        }
        public int SavedOreTiersCobalt
        {
            get { return _savedOreTiersCobalt; }
            set { Set(nameof(SavedOreTiersCobalt), ref _savedOreTiersCobalt, value); }
        }

        public int SavedOreTiersMythril
        {
            get { return _savedOreTiersMythril; }
            set { Set(nameof(SavedOreTiersMythril), ref _savedOreTiersMythril, value); }
        }

        public int SavedOreTiersAdamantite
        {
            get { return _savedOreTiersAdamantite; }
            set { Set(nameof(SavedOreTiersAdamantite), ref _savedOreTiersAdamantite, value); }
        }


        public int SavedOreTiersCopper
        {
            get { return _savedOreTiersCopper; }
            set { Set(nameof(SavedOreTiersCopper), ref _savedOreTiersCopper, value); }
        }

        public int SavedOreTiersIron
        {
            get { return _savedOreTiersIron; }
            set { Set(nameof(SavedOreTiersIron), ref _savedOreTiersIron, value); }
        }

        public int SavedOreTiersSilver
        {
            get { return _savedOreTiersSilver; }
            set { Set(nameof(SavedOreTiersSilver), ref _savedOreTiersSilver, value); }
        }


        public int SavedOreTiersGold
        {
            get { return _savedOreTiersGold; }
            set { Set(nameof(SavedOreTiersGold), ref _savedOreTiersGold, value); }
        }

        public bool BoughtCat
        {
            get { return _boughtCat; }
            set { Set(nameof(BoughtCat), ref _boughtCat, value); }
        }

        public bool BoughtDog
        {
            get { return _boughtDog; }
            set { Set(nameof(BoughtDog), ref _boughtDog, value); }
        }

        public bool BoughtBunny
        {
            get { return _boughtBunny; }
            set { Set(nameof(BoughtBunny), ref _boughtBunny, value); }
        }

        public bool DownedEmpressOfLight
        {
            get { return _downedEmpressOfLight; }
            set { Set(nameof(DownedEmpressOfLight), ref _downedEmpressOfLight, value); }
        }

        public bool DownedQueenSlime
        {
            get { return _downedQueenSlime; }
            set { Set(nameof(DownedQueenSlime), ref _downedQueenSlime, value); }
        }

        public int IceBackStyle
        {
            get { return _iceBackStyle; }
            set { Set(nameof(IceBackStyle), ref _iceBackStyle, value); }
        }

        public int JungleBackStyle
        {
            get { return _jungleBackStyle; }
            set { Set(nameof(JungleBackStyle), ref _jungleBackStyle, value); }
        }

        public int HellBackStyle
        {
            get { return _hellBackStyle; }
            set { Set(nameof(HellBackStyle), ref _hellBackStyle, value); }
        }

        public bool IsEclipse
        {
            get { return _isEclipse; }
            set { Set(nameof(IsEclipse), ref _isEclipse, value); }
        }

        public bool IsCrimson
        {
            get { return _isCrimson; }
            set { Set(nameof(IsCrimson), ref _isCrimson, value); }
        }

        public float WindSpeedSet
        {
            get { return _windSpeedSet; }
            set { Set(nameof(WindSpeedSet), ref _windSpeedSet, value); }
        }

        public short NumClouds
        {
            get { return _numClouds; }
            set { Set(nameof(NumClouds), ref _numClouds, value); }
        }

        public float CloudBgActive
        {
            get { return _cloudBgActive; }
            set { Set(nameof(CloudBgActive), ref _cloudBgActive, value); }
        }


        public ObservableCollection<String> Anglers
        {
            get { return _anglers; }
        }

        public ObservableCollection<NPC> NPCs
        {
            get { return _npcs; }
        }

        public ObservableCollection<int> KilledMobs
        {
            get { return _killedMobs; }
        }

        public ObservableCollection<NPC> Mobs
        {
            get { return _mobs; }
        }

        public ObservableCollection<Sign> Signs
        {
            get { return _signs; }
        }

        public ObservableCollection<Chest> Chests
        {
            get { return _chests; }
        }

        public ObservableCollection<NpcName> CharacterNames
        {
            get { return _charNames; }
        }

        public ObservableCollection<TileEntity> TileEntities
        {
            get { return _tileEntities; }
        }

        public ObservableCollection<int> PartyingNPCs
        {
            get { return _partyingNPCs; }
        }

        public ObservableCollection<PressurePlate> PressurePlates
        {
            get { return _pressurePlates; }
        }

        public ObservableCollection<TownManager> PlayerRooms
        {
            get { return _playerRooms; }
        }

        public ObservableCollection<int> TreeTopVariations
        {
            get { return _treeTopVariations; }
            set { Set(nameof(TreeTopVariations), ref _treeTopVariations, value); }
        }

        public int TreeTop1
        {
            get { return _treeTopVariations[0]; }
            set
            {
                _treeTopVariations[0] = (int)value;
                RaisePropertyChanged(nameof(TreeTop1));
            }
        }

        public int TreeTop2
        {
            get { return _treeTopVariations[1]; }
            set
            {
                _treeTopVariations[1] = (int)value;
                RaisePropertyChanged(nameof(TreeTop2));
            }
        }

        public int TreeTop3
        {
            get { return _treeTopVariations[2]; }
            set
            {
                _treeTopVariations[2] = (int)value;
                RaisePropertyChanged(nameof(TreeTop3));
            }
        }

        public int TreeTop4
        {
            get { return _treeTopVariations[3]; }
            set
            {
                _treeTopVariations[3] = (int)value;
                RaisePropertyChanged(nameof(TreeTop4));
            }
        }

        public bool GooWorld
        {
            get { return _gooWorld; }
            set { Set(nameof(GooWorld), ref _gooWorld, value); }
        }

        public byte MoonType
        {
            get { return _moonType; }
            set { Set(nameof(MoonType), ref _moonType, value); }
        }


        public byte BgOcean
        {
            get { return _bgOcean; }
            set { Set(nameof(BgOcean), ref _bgOcean, value); }
        }

        public byte BgDesert
        {
            get { return _bgDesert; }
            set { Set(nameof(BgDesert), ref _bgDesert, value); }
        }

        public byte BgCrimson
        {
            get { return _bgCrimson; }
            set { Set(nameof(BgCrimson), ref _bgCrimson, value); }
        }

        public byte BgHallow
        {
            get { return _bgHallow; }
            set { Set(nameof(BgHallow), ref _bgHallow, value); }
        }

        public byte BgSnow
        {
            get { return _bgSnow; }
            set { Set(nameof(BgSnow), ref _bgSnow, value); }
        }

        public byte BgJungle
        {
            get { return _bgJungle; }
            set { Set(nameof(BgJungle), ref _bgJungle, value); }
        }

        public byte BgCorruption
        {
            get { return _bgCorruption; }
            set { Set(nameof(BgCorruption), ref _bgCorruption, value); }
        }

        public byte BgTree
        {
            get { return _bgTree; }
            set { Set(nameof(BgTree), ref _bgTree, value); }
        }

        public byte Bg8
        {
            get { return _bgTree; }
            set { Set(nameof(BgTree), ref _bgTree, value); }
        }

        public byte BgTree2
        {
            get { return _bgTree2; }
            set { Set("BgTree2", ref _bgTree2, value); }
        }

        public byte BgTree3
        {
            get { return _bgTree3; }
            set { Set("BgTree3", ref _bgTree3, value); }
        }

        public byte BgTree4
        {
            get { return _bgTree4; }
            set { Set("BgTree4", ref _bgTree4, value); }
        }

        public byte UnderworldBg
        {
            get { return _underworldBg; }
            set { Set(nameof(UnderworldBg), ref _underworldBg, value); }
        }

        public byte MushroomBg
        {
            get { return _mushroomBg; }
            set { Set(nameof(MushroomBg), ref _mushroomBg, value); }
        }



        public bool CombatBookUsed
        {
            get { return _combatBookUsed; }
            set { Set(nameof(CombatBookUsed), ref _combatBookUsed, value); }
        }

        public int TempLanternNightCooldown
        {
            get { return _tempLanternNightCooldown; }
            set { Set(nameof(TempLanternNightCooldown), ref _tempLanternNightCooldown, value); }
        }

        public bool TempLanternNightGenuine
        {
            get { return _tempLanternNightGenuine; }
            set { Set(nameof(TempLanternNightGenuine), ref _tempLanternNightGenuine, value); }
        }

        public bool TempLanternNightManual
        {
            get { return _tempLanternNightManual; }
            set { Set(nameof(TempLanternNightManual), ref _tempLanternNightManual, value); }
        }

        public bool TempLanternNightNextNightIsGenuine
        {
            get { return _tempLanternNightNextNightIsGenuine; }
            set { Set(nameof(TempLanternNightNextNightIsGenuine), ref _tempLanternNightNextNightIsGenuine, value); }
        }

        public float TempMaxRain
        {
            get { return _tempMaxRain; }
            set { Set(nameof(TempMaxRain), ref _tempMaxRain, value); }
        }

        public int TempRainTime
        {
            get { return _tempRainTime; }
            set { Set(nameof(TempRainTime), ref _tempRainTime, value); }
        }

        public bool TempRaining
        {
            get { return _tempRaining; }
            set { Set(nameof(TempRaining), ref _tempRaining, value); }
        }

        public bool DownedPirates
        {
            get { return _downedPirates; }
            set { Set(nameof(DownedPirates), ref _downedPirates, value); }
        }

        public bool DownedGolemBoss
        {
            get { return _downedGolemBoss; }
            set { Set(nameof(DownedGolemBoss), ref _downedGolemBoss, value); }
        }

        public bool DownedSlimeKingBoss
        {
            get { return _downedSlimeKingBoss; }
            set { Set(nameof(DownedSlimeKingBoss), ref _downedSlimeKingBoss, value); }
        }

        public bool DownedPlantBoss
        {
            get { return _downedPlantBoss; }
            set { Set(nameof(DownedPlantBoss), ref _downedPlantBoss, value); }
        }

        public bool DownedMechBossAny
        {
            get { return _downedMechBossAny; }
            set { Set(nameof(DownedMechBossAny), ref _downedMechBossAny, value); }
        }

        public bool DownedMechBoss3
        {
            get { return _downedMechBoss3; }
            set
            {
                _downedMechBoss3 = value;
                if (value)
                    DownedMechBossAny = true;
                if (!value && !DownedMechBoss2 && !DownedMechBoss1)
                    DownedMechBossAny = false;
            }
        }

        public bool DownedMechBoss2
        {
            get { return _downedMechBoss2; }
            set
            {
                _downedMechBoss2 = value;
                if (value)
                    DownedMechBossAny = true;
                if (!value && !DownedMechBoss3 && !DownedMechBoss1)
                    DownedMechBossAny = false;
            }
        }

        public bool DownedMechBoss1
        {
            get { return _downedMechBoss1; }
            set
            {
                _downedMechBoss1 = value;
                if (value)
                    DownedMechBossAny = true;
                if (!value && !DownedMechBoss2 && !DownedMechBoss3)
                    DownedMechBossAny = false;
            }
        }

        public bool DownedQueenBee
        {
            get { return _downedQueenBee; }
            set { Set(nameof(DownedQueenBee), ref _downedQueenBee, value); }
        }

        public bool DownedFishron
        {
            get { return _downedFishron; }
            set { Set(nameof(DownedFishron), ref _downedFishron, value); }
        }

        public bool DownedMartians
        {
            get { return _downedMartians; }
            set { Set(nameof(DownedMartians), ref _downedMartians, value); }
        }

        public bool DownedLunaticCultist
        {
            get { return _downedLunaticCultist; }
            set { Set(nameof(DownedLunaticCultist), ref _downedLunaticCultist, value); }
        }

        public bool DownedMoonlord
        {
            get { return _downedMoonlord; }
            set { Set(nameof(DownedMoonlord), ref _downedMoonlord, value); }
        }

        public bool DownedHalloweenKing
        {
            get { return _downedHalloweenKing; }
            set { Set(nameof(DownedHalloweenKing), ref _downedHalloweenKing, value); }
        }

        public bool DownedHalloweenTree
        {
            get { return _downedHalloweenTree; }
            set { Set(nameof(DownedHalloweenTree), ref _downedHalloweenTree, value); }
        }

        public bool DownedChristmasQueen
        {
            get { return _downedChristmasQueen; }
            set { Set(nameof(DownedChristmasQueen), ref _downedChristmasQueen, value); }
        }

        public bool DownedSanta
        {
            get { return _downedSanta; }
            set { Set(nameof(DownedSanta), ref _downedSanta, value); }
        }

        public bool DownedChristmasTree
        {
            get { return _downedChristmasTree; }
            set { Set(nameof(DownedChristmasTree), ref _downedChristmasTree, value); }
        }

        public bool DownedCelestialSolar
        {
            get { return _downedCelestialSolar; }
            set { Set(nameof(DownedCelestialSolar), ref _downedCelestialSolar, value); }
        }

        public bool DownedCelestialVortex
        {
            get { return _downedCelestialVortex; }
            set { Set(nameof(DownedCelestialVortex), ref _downedCelestialVortex, value); }
        }

        public bool DownedCelestialNebula
        {
            get { return _downedCeslestialNebula; }
            set { Set(nameof(DownedCelestialNebula), ref _downedCeslestialNebula, value); }
        }

        public bool DownedCelestialStardust
        {
            get { return _downedCelestialStardust; }
            set { Set(nameof(DownedCelestialStardust), ref _downedCelestialStardust, value); }
        }

        public bool CelestialSolarActive
        {
            get { return _celestialSolarActive; }
            set { Set(nameof(CelestialSolarActive), ref _celestialSolarActive, value); }
        }

        public bool CelestialVortexActive
        {
            get { return _celestialVortexActive; }
            set { Set(nameof(CelestialVortexActive), ref _celestialVortexActive, value); }
        }

        public bool CelestialNebulaActive
        {
            get { return _celestialNebulaActive; }
            set { Set(nameof(CelestialNebulaActive), ref _celestialNebulaActive, value); }
        }

        public bool CelestialStardustActive
        {
            get { return _celestialStardustActive; }
            set { Set(nameof(CelestialStardustActive), ref _celestialStardustActive, value); }
        }

        public int GameMode
        {
            get { return _gameMode; }
            set { Set(nameof(GameMode), ref _gameMode, value); }
        }

        public bool DrunkWorld
        {
            get { return _drunkWorld; }
            set { Set(nameof(DrunkWorld), ref _drunkWorld, value); }
        }

        public int TilesWide
        {
            get { return _tilesWide; }
            set { Set(nameof(TilesWide), ref _tilesWide, value); }
        }

        public int TilesHigh
        {
            get { return _tilesHigh; }
            set { Set(nameof(TilesHigh), ref _tilesHigh, value); }
        }

        public float BottomWorld
        {
            get { return _bottomWorld; }
            set { Set(nameof(BottomWorld), ref _bottomWorld, value); }
        }

        public float TopWorld
        {
            get { return _topWorld; }
            set { Set(nameof(TopWorld), ref _topWorld, value); }
        }

        public float RightWorld
        {
            get { return _rightWorld; }
            set { Set(nameof(RightWorld), ref _rightWorld, value); }
        }

        public float LeftWorld
        {
            get { return _leftWorld; }
            set { Set(nameof(LeftWorld), ref _leftWorld, value); }
        }

        public int WorldId
        {
            get { return _worldId; }
            set { Set(nameof(WorldId), ref _worldId, value); }
        }

        public System.Guid WorldGUID
        {
            get { return Guid; }
            set { Set(nameof(WorldGUID), ref Guid, value); }
        }

        public bool DownedFrost
        {
            get { return _downedFrost; }
            set { Set(nameof(DownedFrost), ref _downedFrost, value); }
        }

        public string Title
        {
            get { return _title; }
            set { Set(nameof(Title), ref _title, value); }
        }

        public int SpawnX
        {
            get { return _spawnX; }
            set { Set(nameof(SpawnX), ref _spawnX, value); }
        }

        public int SpawnY
        {
            get { return _spawnY; }
            set { Set(nameof(SpawnY), ref _spawnY, value); }
        }

        public double GroundLevel
        {
            get { return _groundLevel; }
            set
            {
                Set(nameof(GroundLevel), ref _groundLevel, value);
                if (_groundLevel > _rockLevel)
                    RockLevel = _groundLevel;
            }
        }

        public double RockLevel
        {
            get { return _rockLevel; }
            set
            {
                Set(nameof(RockLevel), ref _rockLevel, value);
                if (_groundLevel > _rockLevel)
                    GroundLevel = _rockLevel;
            }
        }

        public double Time
        {
            get { return _time; }
            set { Set(nameof(Time), ref _time, value); }
        }

        public bool DayTime
        {
            get { return _dayTime; }
            set { Set(nameof(DayTime), ref _dayTime, value); }
        }

        public int MoonPhase
        {
            get { return _moonPhase; }
            set { Set(nameof(MoonPhase), ref _moonPhase, value); }
        }

        public bool BloodMoon
        {
            get { return _bloodMoon; }
            set { Set(nameof(BloodMoon), ref _bloodMoon, value); }
        }

        public int DungeonX
        {
            get { return _dungeonX; }
            set { Set(nameof(DungeonX), ref _dungeonX, value); }
        }

        public int DungeonY
        {
            get { return _dungeonY; }
            set { Set(nameof(DungeonY), ref _dungeonY, value); }
        }

        public bool DownedBoss1
        {
            get { return _downedBoss1; }
            set { Set("DownedBoss1", ref _downedBoss1, value); }
        }

        public bool DownedBoss2
        {
            get { return _downedBoss2; }
            set { Set("DownedBoss2", ref _downedBoss2, value); }
        }

        public bool DownedBoss3
        {
            get { return _downedBoss3; }
            set { Set("DownedBoss3", ref _downedBoss3, value); }
        }

        public bool SavedGoblin
        {
            get { return _savedGoblin; }
            set { Set(nameof(SavedGoblin), ref _savedGoblin, value); }
        }

        public bool SavedWizard
        {
            get { return _savedWizard; }
            set { Set(nameof(SavedWizard), ref _savedWizard, value); }
        }

        public bool DownedGoblins
        {
            get { return _downedGoblins; }
            set { Set(nameof(DownedGoblins), ref _downedGoblins, value); }
        }

        public bool SavedMech
        {
            get { return _savedMech; }
            set { Set(nameof(SavedMech), ref _savedMech, value); }
        }

        public bool DownedClown
        {
            get { return _downedClown; }
            set { Set(nameof(DownedClown), ref _downedClown, value); }
        }

        public bool ShadowOrbSmashed
        {
            get { return _shadowOrbSmashed; }
            set { Set(nameof(ShadowOrbSmashed), ref _shadowOrbSmashed, value); }
        }

        public bool SpawnMeteor
        {
            get { return _spawnMeteor; }
            set { Set(nameof(SpawnMeteor), ref _spawnMeteor, value); }
        }

        public int ShadowOrbCount
        {
            get { return _shadowOrbCount; }
            set
            {
                Set(nameof(ShadowOrbCount), ref _shadowOrbCount, value);
                ShadowOrbSmashed = _shadowOrbCount > 0;
            }
        }

        public int AltarCount
        {
            get { return _altarCount; }
            set { Set(nameof(AltarCount), ref _altarCount, value); }
        }

        public bool HardMode
        {
            get { return _hardMode; }
            set { Set(nameof(HardMode), ref _hardMode, value); }
        }

        public double InvasionX
        {
            get { return _invasionX; }
            set { Set(nameof(InvasionX), ref _invasionX, value); }
        }

        public int InvasionType
        {
            get { return _invasionType; }
            set { Set(nameof(InvasionType), ref _invasionType, value); }
        }

        public int InvasionSize
        {
            get { return _invasionSize; }
            set { Set(nameof(InvasionSize), ref _invasionSize, value); }
        }

        public int InvasionSizeStart
        {
            get { return _invasionSizeStart; }
            set { Set(nameof(InvasionSizeStart), ref _invasionSizeStart, value); }
        }

        public int InvasionDelay
        {
            get { return _invasionDelay; }
            set { Set(nameof(InvasionDelay), ref _invasionDelay, value); }
        }

        public int TreeX0
        {
            get { return _treeX0; }
            set
            {
                Set("TreeX0", ref _treeX0, value);
                if (_treeX0 > _treeX1)
                    TreeX0 = _treeX1;
            }

        }

        public int TreeX1
        {
            get { return _treeX1; }
            set
            {
                Set("TreeX1", ref _treeX1, value);
                if (_treeX1 < _treeX0)
                    TreeX1 = _treeX0;
                if (_treeX1 > _treeX2)
                    TreeX1 = _treeX2;
            }
        }

        public int TreeX2
        {
            get { return _treeX2; }
            set
            {
                Set("TreeX2", ref _treeX2, value);
                if (_treeX2 < _treeX1)
                    TreeX2 = _treeX1;
            }
        }

        public int TreeStyle0
        {
            get { return _treeStyle0; }
            set { Set("TreeStyle0", ref _treeStyle0, value); }
        }

        public int TreeStyle1
        {
            get { return _treeStyle1; }
            set { Set("TreeStyle1", ref _treeStyle1, value); }
        }

        public int TreeStyle2
        {
            get { return _treeStyle2; }
            set { Set("TreeStyle2", ref _treeStyle2, value); }
        }

        public int TreeStyle3
        {
            get { return _treeStyle3; }
            set { Set("TreeStyle3", ref _treeStyle3, value); }
        }

        public int CaveBackX0
        {
            get { return _caveBackX0; }
            set
            {
                Set("CaveBackX0", ref _caveBackX0, value);
                if (_caveBackX0 > _caveBackX1)
                    CaveBackX0 = _caveBackX1;
            }

        }

        public int CaveBackX1
        {
            get { return _caveBackX1; }
            set
            {
                Set("CaveBackX1", ref _caveBackX1, value);
                if (_caveBackX1 < _caveBackX0)
                    CaveBackX1 = _caveBackX0;
                if (_caveBackX1 > _caveBackX2)
                    CaveBackX1 = _caveBackX2;
            }
        }

        public int CaveBackX2
        {
            get { return _caveBackX2; }
            set
            {
                Set("CaveBackX2", ref _caveBackX2, value);
                if (_caveBackX2 < _caveBackX1)
                    CaveBackX2 = _caveBackX1;
            }
        }

        public int CaveBackStyle0
        {
            get { return _caveBackStyle0; }
            set { Set("CaveBackStyle0", ref _caveBackStyle0, value); }
        }

        public int CaveBackStyle1
        {
            get { return _caveBackStyle1; }
            set { Set("CaveBackStyle1", ref _caveBackStyle1, value); }
        }

        public int CaveBackStyle2
        {
            get { return _caveBackStyle2; }
            set { Set("CaveBackStyle2", ref _caveBackStyle2, value); }
        }

        public int CaveBackStyle3
        {
            get { return _caveBackStyle3; }
            set { Set("CaveBackStyle3", ref _caveBackStyle3, value); }
        }

        public int CultistDelay
        {
            get { return _cultistDelay; }
            set { Set(nameof(CultistDelay), ref _cultistDelay, value); }
        }

        public bool FastForwardTime
        {
            get { return _fastForwardTime; }
            set { Set(nameof(FastForwardTime), ref _fastForwardTime, value); }
        }

        public bool Apocalypse
        {
            get { return _apocalypse; }
            set { Set(nameof(Apocalypse), ref _apocalypse, value); }
        }



        public DateTime LastSave
        {
            get { return _lastSave; }
            set { Set(nameof(LastSave), ref _lastSave, value); }
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
            if (NpcNames.TryGetValue(id, out name))
            {
                return new NpcName(id, name);
            }

            return new NpcName(id, "Unknown");
        }
    }
}
