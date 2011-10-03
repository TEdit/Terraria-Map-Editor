using System.ComponentModel;
using TEdit.Common;
using TEdit.Common.Structures;

namespace TEdit.TerrariaWorld
{
    public class WorldHeader : ObservableObject
    {
        private PointInt32 _DungeonEntrance;
        private string _FileName;
        private int _FileVersion;
        private int _InvasionDelay;
        private int _InvasionSize;
        private int _InvasionType;
        private double _InvasionX;
        private bool _IsBloodMoon;
        private bool _IsBossDowned1;
        private bool _IsBossDowned2;
        private bool _IsBossDowned3;
        private bool _IsDayTime;
        private bool _IsShadowOrbSmashed;
        private bool _IsSpawnMeteor;
        private PointInt32 _MaxTiles;
        private int _MoonPhase;
        private int _ShadowOrbCount;
        private PointInt32 _SpawnTile;
        private double _Time;
        private RectF _WorldBounds;
        private int _WorldId;
        private string _WorldName;
        private double _WorldRockLayer;
        private double _WorldSurface;

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

        [Category("World"), DescriptionAttribute("Terraria Save File Version")]
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

        [CategoryAttribute("World"), DescriptionAttribute("World Name")]
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

        [CategoryAttribute("World"), DescriptionAttribute("World ID"), ReadOnly(true)]
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

        [CategoryAttribute("World"), DescriptionAttribute("World Size"), ReadOnly(true)]
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

        [CategoryAttribute("World"), DescriptionAttribute("World Size"), ReadOnly(true)]
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

        [CategoryAttribute("World"), DescriptionAttribute("Spawn Location")]
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

        [CategoryAttribute("World"), DescriptionAttribute("Surface Level"), ReadOnly(true)]
        public double WorldSurface
        {
            get { return _WorldSurface; }
            set
            {
                var validValue = value;
                if (validValue < 0)
                    validValue = 0;
                if (validValue > MaxTiles.Y)
                    validValue = MaxTiles.Y;

                if (_WorldSurface != validValue)
                {
                    _WorldSurface = validValue;
                    RaisePropertyChanged("WorldSurface");

                    if (_WorldSurface > _WorldRockLayer)
                        WorldRockLayer = _WorldSurface;
                }
            }
        }

        [CategoryAttribute("World"), DescriptionAttribute("Rock Level"), ReadOnly(true)]
        public double WorldRockLayer
        {
            get { return _WorldRockLayer; }
            set
            {
                var validValue = value;
                if (validValue < 0)
                    validValue = 0;
                if (validValue > MaxTiles.Y)
                    validValue = MaxTiles.Y;

                if (_WorldRockLayer != validValue)
                {
                    _WorldRockLayer = validValue;
                    RaisePropertyChanged("WorldRockLayer");

                    if (_WorldRockLayer < _WorldSurface)
                        WorldSurface = _WorldRockLayer;
                }
            }
        }

        [CategoryAttribute("Time"), DescriptionAttribute("Time of Day")]
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

        [CategoryAttribute("Time"), DescriptionAttribute("Is it Daytime")]
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

        [CategoryAttribute("Time"), DescriptionAttribute("Moon Phase")]
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

        [CategoryAttribute("Time"), DescriptionAttribute("Is it a Blood Moon")]
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

        [Category("World"), DescriptionAttribute("Dungeon Location"), ReadOnly(true)]
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

        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Eye of Cthulhu Dead")]
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

        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Eater of Worlds Dead")]
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

        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Skeletron Dead")]
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

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Have any Shadow Orbs been Smashed?")]
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

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Spawn the Meteor?")]
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

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Number of Shadow Orbs Smashed")]
        public int ShadowOrbCount
        {
            get { return _ShadowOrbCount; }
            set
            {
                if (_ShadowOrbCount != value)
                {
                    _ShadowOrbCount = value;
                    RaisePropertyChanged("ShadowOrbCount");
                    IsShadowOrbSmashed = ShadowOrbCount > 0;
                }
            }
        }

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Delay")]
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

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Size")]
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

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Type")]
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

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion X Coordinate")]
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