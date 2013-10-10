using System;
using System.Collections.ObjectModel;
using BCCL.MvvmLight;
using TEditXNA.Terraria.Objects;

namespace TEditXNA.Terraria
{
    public partial class World : ObservableObject
    {
        // backgrounds
        // 0 = tree 1 2 3 31 4 5 51 6 7 71 72 73 8 
        // 1 = corruption 1
        // 2 = jungle 1
        // 3 = snow 1 2 21 22 3 31 32 4 41 42
        // 4 = hallow 1
        // 5 = crimson 1 2
        // 6 = desert 1
        // 7 = ocean 1 2

        public static int MaxMoons = 3;
        private readonly ObservableCollection<NpcName> _charNames = new ObservableCollection<NpcName>();
        private readonly ObservableCollection<Chest> _chests = new ObservableCollection<Chest>();
        private readonly ObservableCollection<NPC> _npcs = new ObservableCollection<NPC>();
        private readonly ObservableCollection<Sign> _signs = new ObservableCollection<Sign>();
        public int[] CaveBackStyle = new int[4];
        public int[] CaveBackX = new int[4];
        public int[] CorruptBG = new int[3];
        public int[] CrimsonBG = new int[3];
        public int[] DesertBG = new int[2];
        public int[] HallowBG = new int[3];
        public int HellBackStyle;
        public int IceBackStyle;
        public int[] JungleBG = new int[3];
        public int JungleBackStyle;
        public int[] SnowBG = new int[3];
        public int[] SnowMntBG = new int[2];
        public Tile[,] Tiles;
        public int[] TreeBG = new int[3];
        public int[] TreeMntBG = new int[2];
        public int[] TreeStyle = new int[4];
        public int[] TreeX = new int[4];
        public uint Version;
        private int _altarCount;
        private byte _bgCorruption;
        private byte _bgCrimson;
        private byte _bgDesert;
        private byte _bgHallow;
        private byte _bgJungle;
        private byte _bgOcean;
        private byte _bgSnow;
        private byte _bgTree;
        private bool _bloodMoon;
        private float _bottomWorld;
        private bool _dayTime;
        private bool _downedBoss1;
        private bool _downedBoss2;
        private bool _downedBoss3;
        private bool _downedClown;
        private bool _downedFrost;
        private bool _downedGoblins;
        private bool _downedGolemBoss;
        private bool _downedMechBoss1;
        private bool _downedMechBoss2;
        private bool _downedMechBoss3;
        private bool _downedMechBossAny;
        private bool _downedPirates;
        private bool _downedPlantBoss;
        private bool _downedQueenBee;
        private int _dungeonX;
        private int _dungeonY;
        private double _groundLevel;
        private bool _hardMode;
        private int _invasionDelay;
        private int _invasionSize;
        private int _invasionType;
        private double _invasionX;
        private DateTime _lastSave;
        private float _leftWorld;
        private int _moonPhase;
        private int _moonType;
        private int _oreTier1;
        private int _oreTier2;
        private int _oreTier3;
        private float _rightWorld;
        private double _rockLevel;
        private bool _savedGoblin;
        private bool _savedMech;
        private bool _savedWizard;
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
        private double _time;
        private string _title;
        private float _topWorld;
        private int _worldId;
		private float _cloudBgActive;
        private short _numClouds;
        private float _windSpeedSet;
        public Random Rand;
        private bool _isCrimson;
        private bool _isEclipse;


        public bool IsEclipse
        {
            get { return _isEclipse; }
            set { Set("IsEclipse", ref _isEclipse, value); }
        } 

        public bool IsCrimson
        {
            get { return _isCrimson; }
            set { Set("IsCrimson", ref _isCrimson, value); }
        }

        public float WindSpeedSet
        {
            get { return _windSpeedSet; }
            set { Set("WindSpeedSet", ref _windSpeedSet, value); }
        } 

        public short NumClouds
        {
            get { return _numClouds; }
            set { Set("NumClouds", ref _numClouds, value); }
        }

        public float CloudBgActive
        {
            get { return _cloudBgActive; }
            set { Set("CloudBgActive", ref _cloudBgActive, value); }
        }

        public ObservableCollection<NPC> NPCs
        {
            get { return _npcs; }
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

        public int MoonType
        {
            get { return _moonType; }
            set { Set("MoonType", ref _moonType, value); }
        }


        public byte BgOcean
        {
            get { return _bgOcean; }
            set { Set("BgOcean", ref _bgOcean, value); }
        }

        public byte BgDesert
        {
            get { return _bgDesert; }
            set { Set("BgDesert", ref _bgDesert, value); }
        }

        public byte BgCrimson
        {
            get { return _bgCrimson; }
            set { Set("BgCrimson", ref _bgCrimson, value); }
        }

        public byte BgHallow
        {
            get { return _bgHallow; }
            set { Set("BgHallow", ref _bgHallow, value); }
        }

        public byte BgSnow
        {
            get { return _bgSnow; }
            set { Set("BgSnow", ref _bgSnow, value); }
        }

        public byte BgJungle
        {
            get { return _bgJungle; }
            set { Set("BgJungle", ref _bgJungle, value); }
        }

        public byte BgCorruption
        {
            get { return _bgCorruption; }
            set { Set("BgCorruption", ref _bgCorruption, value); }
        }

        public byte BgTree
        {
            get { return _bgTree; }
            set { Set("BgTree", ref _bgTree, value); }
        }

        public int OreTier3
        {
            get { return _oreTier3; }
            set { Set("OreTier3", ref _oreTier3, value); }
        }

        public int OreTier2
        {
            get { return _oreTier2; }
            set { Set("OreTier2", ref _oreTier2, value); }
        }

        public int OreTier1
        {
            get { return _oreTier1; }
            set { Set("OreTier1", ref _oreTier1, value); }
        }

        public float TempMaxRain
        {
            get { return _tempMaxRain; }
            set { Set("TempMaxRain", ref _tempMaxRain, value); }
        }

        public int TempRainTime
        {
            get { return _tempRainTime; }
            set { Set("TempRainTime", ref _tempRainTime, value); }
        }

        public bool TempRaining
        {
            get { return _tempRaining; }
            set { Set("TempRaining", ref _tempRaining, value); }
        }

        public bool DownedPirates
        {
            get { return _downedPirates; }
            set { Set("DownedPirates", ref _downedPirates, value); }
        }

        public bool DownedGolemBoss
        {
            get { return _downedGolemBoss; }
            set { Set("DownedGolemBoss", ref _downedGolemBoss, value); }
        }

        public bool DownedPlantBoss
        {
            get { return _downedPlantBoss; }
            set { Set("DownedPlantBoss", ref _downedPlantBoss, value); }
        }

        public bool DownedMechBossAny
        {
            get { return _downedMechBossAny; }
            set { Set("DownedMechBossAny", ref _downedMechBossAny, value); }
        }

        public bool DownedMechBoss3
        {
            get { return _downedMechBoss3; }
            set { Set("DownedMechBoss3", ref _downedMechBoss3, value); }
        }

        public bool DownedMechBoss2
        {
            get { return _downedMechBoss2; }
            set { Set("DownedMechBoss2", ref _downedMechBoss2, value); }
        }

        public bool DownedMechBoss1
        {
            get { return _downedMechBoss1; }
            set { Set("DownedMechBoss1", ref _downedMechBoss1, value); }
        }

        public bool DownedQueenBee
        {
            get { return _downedQueenBee; }
            set { Set("DownedQueenBee", ref _downedQueenBee, value); }
        }

        public int TilesWide
        {
            get { return _tilesWide; }
            set { Set("TilesWide", ref _tilesWide, value); }
        }

        public int TilesHigh
        {
            get { return _tilesHigh; }
            set { Set("TilesHigh", ref _tilesHigh, value); }
        }

        public float BottomWorld
        {
            get { return _bottomWorld; }
            set { Set("BottomWorld", ref _bottomWorld, value); }
        }

        public float TopWorld
        {
            get { return _topWorld; }
            set { Set("TopWorld", ref _topWorld, value); }
        }

        public float RightWorld
        {
            get { return _rightWorld; }
            set { Set("RightWorld", ref _rightWorld, value); }
        }

        public float LeftWorld
        {
            get { return _leftWorld; }
            set { Set("LeftWorld", ref _leftWorld, value); }
        }

        public int WorldId
        {
            get { return _worldId; }
            set { Set("WorldId", ref _worldId, value); }
        }

        public bool DownedFrost
        {
            get { return _downedFrost; }
            set { Set("DownedFrost", ref _downedFrost, value); }
        }

        public string Title
        {
            get { return _title; }
            set { Set("Title", ref _title, value); }
        }

        public int SpawnX
        {
            get { return _spawnX; }
            set { Set("SpawnX", ref _spawnX, value); }
        }

        public int SpawnY
        {
            get { return _spawnY; }
            set { Set("SpawnY", ref _spawnY, value); }
        }

        public double GroundLevel
        {
            get { return _groundLevel; }
            set
            {
                Set("GroundLevel", ref _groundLevel, value);
                if (_groundLevel > _rockLevel)
                    RockLevel = _groundLevel;
            }
        }

        public double RockLevel
        {
            get { return _rockLevel; }
            set
            {
                Set("RockLevel", ref _rockLevel, value);
                if (_groundLevel > _rockLevel)
                    GroundLevel = _rockLevel;
            }
        }

        public double Time
        {
            get { return _time; }
            set { Set("Time", ref _time, value); }
        }

        public bool DayTime
        {
            get { return _dayTime; }
            set { Set("DayTime", ref _dayTime, value); }
        }

        public int MoonPhase
        {
            get { return _moonPhase; }
            set { Set("MoonPhase", ref _moonPhase, value); }
        }

        public bool BloodMoon
        {
            get { return _bloodMoon; }
            set { Set("BloodMoon", ref _bloodMoon, value); }
        }

        public int DungeonX
        {
            get { return _dungeonX; }
            set { Set("DungeonX", ref _dungeonX, value); }
        }

        public int DungeonY
        {
            get { return _dungeonY; }
            set { Set("DungeonY", ref _dungeonY, value); }
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
            set { Set("SavedGoblin", ref _savedGoblin, value); }
        }

        public bool SavedWizard
        {
            get { return _savedWizard; }
            set { Set("SavedWizard", ref _savedWizard, value); }
        }

        public bool DownedGoblins
        {
            get { return _downedGoblins; }
            set { Set("DownedGoblins", ref _downedGoblins, value); }
        }

        public bool SavedMech
        {
            get { return _savedMech; }
            set { Set("SavedMech", ref _savedMech, value); }
        }

        public bool DownedClown
        {
            get { return _downedClown; }
            set { Set("DownedClown", ref _downedClown, value); }
        }

        public bool ShadowOrbSmashed
        {
            get { return _shadowOrbSmashed; }
            set { Set("ShadowOrbSmashed", ref _shadowOrbSmashed, value); }
        }

        public bool SpawnMeteor
        {
            get { return _spawnMeteor; }
            set { Set("SpawnMeteor", ref _spawnMeteor, value); }
        }

        public int ShadowOrbCount
        {
            get { return _shadowOrbCount; }
            set
            {
                Set("ShadowOrbCount", ref _shadowOrbCount, value);
                ShadowOrbSmashed = _shadowOrbCount > 0;
            }
        }

        public int AltarCount
        {
            get { return _altarCount; }
            set { Set("AltarCount", ref _altarCount, value); }
        }

        public bool HardMode
        {
            get { return _hardMode; }
            set { Set("HardMode", ref _hardMode, value); }
        }

        public double InvasionX
        {
            get { return _invasionX; }
            set { Set("InvasionX", ref _invasionX, value); }
        }

        public int InvasionType
        {
            get { return _invasionType; }
            set { Set("InvasionType", ref _invasionType, value); }
        }

        public int InvasionSize
        {
            get { return _invasionSize; }
            set { Set("InvasionSize", ref _invasionSize, value); }
        }

        public int InvasionDelay
        {
            get { return _invasionDelay; }
            set { Set("InvasionDelay", ref _invasionDelay, value); }
        }

        public DateTime LastSave
        {
            get { return _lastSave; }
            set { Set("LastSave", ref _lastSave, value); }
        }
    }
}