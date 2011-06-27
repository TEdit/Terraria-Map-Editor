using System.ComponentModel;
using TEditWPF.Common;
using TEditWPF.TerrariaWorld.Structures;
using PointInt32 = TEditWPF.TerrariaWorld.Structures.PointInt32;

namespace TEditWPF.TerrariaWorld
{
    public class WorldHeader : ObservableObject
    {
        [CategoryAttribute("World"), DescriptionAttribute("Dungeon Location"), ReadOnly(true)] private
            PointInt32 _DungeonEntrance;

        [Browsable(false)] private string _FileName;

        [CategoryAttribute("World"), DescriptionAttribute("Terraria Save File Version")] private int _FileVersion;
        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Delay")] private int _InvasionDelay;
        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Size")] private int _InvasionSize;
        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Type")] private int _InvasionType;
        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion X Coordinate")] private double _InvasionX;
        [CategoryAttribute("Time"), DescriptionAttribute("Is it a Blood Moon")] private bool _IsBloodMoon;
        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Eater of Worlds Dead")] private bool _IsBossDowned1;
        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Eye of Cuthulu Dead")] private bool _IsBossDowned2;
        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Skeletor Dead")] private bool _IsBossDowned3;
        [CategoryAttribute("Time"), DescriptionAttribute("Is it Daytime")] private bool _IsDayTime;

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Have any Shadow Orbs been Smashed?")] private bool
            _IsShadowOrbSmashed;

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Spawn the Meteor?")] private bool _IsSpawnMeteor;

        [CategoryAttribute("World"), DescriptionAttribute("World Size"), ReadOnly(true)] private PointInt32
            _MaxTiles;

        [CategoryAttribute("Time"), DescriptionAttribute("Moon Phase")] private int _MoonPhase;

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Number of Shadow Orbs Smashed")] private int
            _ShadowOrbCount;

        [CategoryAttribute("World"), DescriptionAttribute("Spawn Location")] private PointInt32 _SpawnTile;
        [CategoryAttribute("Time"), DescriptionAttribute("Time of Day")] private double _Time;
        [CategoryAttribute("World"), DescriptionAttribute("World Size"), ReadOnly(true)] private RectF _WorldBounds;
        [CategoryAttribute("World"), DescriptionAttribute("World ID"), ReadOnly(true)] private int _WorldId;


        [CategoryAttribute("World"), DescriptionAttribute("World Name")] private string _WorldName;
        [CategoryAttribute("World"), DescriptionAttribute("Rock Level"), ReadOnly(true)] private double _WorldRockLayer;
        [CategoryAttribute("World"), DescriptionAttribute("Surface Level"), ReadOnly(true)] private double _WorldSurface;

        public WorldHeader()
        {
            _FileName = "";
            _WorldName = "No World Loaded";
            _MaxTiles = new PointInt32(0, 0);
        }

        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (_FileName != value)
                {
                    _FileName = value;
                    RaisePropertyChanged("FileName");
                }
            }
        }

        public int FileVersion
        {
            get { return _FileVersion; }
            set
            {
                if (_FileVersion != value)
                {
                    _FileVersion = value;
                    RaisePropertyChanged("FileVersion");
                }
            }
        }

        public string WorldName
        {
            get { return _WorldName; }
            set
            {
                if (_WorldName != value)
                {
                    _WorldName = value;
                    RaisePropertyChanged("WorldName");
                }
            }
        }

        public int WorldId
        {
            get { return _WorldId; }
            set
            {
                if (_WorldId != value)
                {
                    _WorldId = value;
                    RaisePropertyChanged("WorldId");
                }
            }
        }

        public RectF WorldBounds
        {
            get { return _WorldBounds; }
            set
            {
                if (_WorldBounds != value)
                {
                    _WorldBounds = value;
                    RaisePropertyChanged("WorldBounds");
                }
            }
        }

        public PointInt32 MaxTiles
        {
            get { return _MaxTiles; }
            set
            {
                if (_MaxTiles != value)
                {
                    _MaxTiles = value;
                    RaisePropertyChanged("MaxTiles");
                }
            }
        }

        public PointInt32 SpawnTile
        {
            get { return _SpawnTile; }
            set
            {
                if (_SpawnTile != value)
                {
                    _SpawnTile = value;
                    RaisePropertyChanged("SpawnTile");
                }
            }
        }

        public double WorldSurface
        {
            get { return _WorldSurface; }
            set
            {
                if (_WorldSurface != value)
                {
                    _WorldSurface = value;
                    RaisePropertyChanged("WorldSurface");
                }
            }
        }


        public double WorldRockLayer
        {
            get { return _WorldRockLayer; }
            set
            {
                if (_WorldRockLayer != value)
                {
                    _WorldRockLayer = value;
                    RaisePropertyChanged("WorldRockLayer");
                }
            }
        }


        public double Time
        {
            get { return _Time; }
            set
            {
                if (_Time != value)
                {
                    _Time = value;
                    RaisePropertyChanged("Time");
                }
            }
        }


        public bool IsDayTime
        {
            get { return _IsDayTime; }
            set
            {
                if (_IsDayTime != value)
                {
                    _IsDayTime = value;
                    RaisePropertyChanged("IsDayTime");
                }
            }
        }


        public int MoonPhase
        {
            get { return _MoonPhase; }
            set
            {
                if (_MoonPhase != value)
                {
                    _MoonPhase = value;
                    RaisePropertyChanged("MoonPhase");
                }
            }
        }


        public bool IsBloodMoon
        {
            get { return _IsBloodMoon; }
            set
            {
                if (_IsBloodMoon != value)
                {
                    _IsBloodMoon = value;
                    RaisePropertyChanged("IsBloodMoon");
                }
            }
        }


        public PointInt32 DungeonEntrance
        {
            get { return _DungeonEntrance; }
            set
            {
                if (_DungeonEntrance != value)
                {
                    _DungeonEntrance = value;
                    RaisePropertyChanged("DungeonEntrance");
                }
            }
        }


        public bool IsBossDowned1
        {
            get { return _IsBossDowned1; }
            set
            {
                if (_IsBossDowned1 != value)
                {
                    _IsBossDowned1 = value;
                    RaisePropertyChanged("IsBossDowned1");
                }
            }
        }


        public bool IsBossDowned2
        {
            get { return _IsBossDowned2; }
            set
            {
                if (_IsBossDowned2 != value)
                {
                    _IsBossDowned2 = value;
                    RaisePropertyChanged("IsBossDowned2");
                }
            }
        }


        public bool IsBossDowned3
        {
            get { return _IsBossDowned3; }
            set
            {
                if (_IsBossDowned3 != value)
                {
                    _IsBossDowned3 = value;
                    RaisePropertyChanged("IsBossDowned3");
                }
            }
        }


        public bool IsShadowOrbSmashed
        {
            get { return _IsShadowOrbSmashed; }
            set
            {
                if (_IsShadowOrbSmashed != value)
                {
                    _IsShadowOrbSmashed = value;
                    RaisePropertyChanged("IsShadowOrbSmashed");
                }
            }
        }


        public bool IsSpawnMeteor
        {
            get { return _IsSpawnMeteor; }
            set
            {
                if (_IsSpawnMeteor != value)
                {
                    _IsSpawnMeteor = value;
                    RaisePropertyChanged("IsSpawnMeteor");
                }
            }
        }


        public int ShadowOrbCount
        {
            get { return _ShadowOrbCount; }
            set
            {
                if (_ShadowOrbCount != value)
                {
                    _ShadowOrbCount = value;
                    RaisePropertyChanged("ShadowOrbCount");
                }
            }
        }


        public int InvasionDelay
        {
            get { return _InvasionDelay; }
            set
            {
                if (_InvasionDelay != value)
                {
                    _InvasionDelay = value;
                    RaisePropertyChanged("InvasionDelay");
                }
            }
        }


        public int InvasionSize
        {
            get { return _InvasionSize; }
            set
            {
                if (_InvasionSize != value)
                {
                    _InvasionSize = value;
                    RaisePropertyChanged("InvasionSize");
                }
            }
        }


        public int InvasionType
        {
            get { return _InvasionType; }
            set
            {
                if (_InvasionType != value)
                {
                    _InvasionType = value;
                    RaisePropertyChanged("InvasionType");
                }
            }
        }


        public double InvasionX
        {
            get { return _InvasionX; }
            set
            {
                if (_InvasionX != value)
                {
                    _InvasionX = value;
                    RaisePropertyChanged("InvasionX");
                }
            }
        }

        public WorldHeader Clone()
        {
            return (WorldHeader) MemberwiseClone();
        }
    }
}