using System.ComponentModel;
using TEditWPF.Common;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.TerrariaWorld
{
    public class WorldHeader : ObservableObject
    {
        public WorldHeader()
        {
            this._FileName = "";
            this._WorldName = "No World Loaded";
            this._MaxTiles = new PointInt32(0,0);
        }

        public WorldHeader Clone()
        {
            return (WorldHeader)this.MemberwiseClone();
        }


        [Browsable(false)]
        private string _FileName;
        public string FileName
        {
            get { return this._FileName; }
            set
            {
                if (this._FileName != value)
                {
                    this._FileName = value;
                    this.RaisePropertyChanged("FileName");
                }
            }
        }

        [CategoryAttribute("World"), DescriptionAttribute("Terraria Save File Version")]
        private int _FileVersion;
        public int FileVersion
        {
            get { return this._FileVersion; }
            set
            {
                if (this._FileVersion != value)
                {
                    this._FileVersion = value;
                    this.RaisePropertyChanged("FileVersion");
                }
            }
        }



        [CategoryAttribute("World"), DescriptionAttribute("World Name")]
        private string _WorldName;
        public string WorldName
        {
            get { return this._WorldName; }
            set
            {
                if (this._WorldName != value)
                {
                    this._WorldName = value;
                    this.RaisePropertyChanged("WorldName");
                }
            }
        }

        [CategoryAttribute("World"), DescriptionAttribute("World ID"), ReadOnly(true)]
        private int _WorldId;
        public int WorldId
        {
            get { return this._WorldId; }
            set
            {
                if (this._WorldId != value)
                {
                    this._WorldId = value;
                    this.RaisePropertyChanged("WorldId");
                }
            }
        }

        [CategoryAttribute("World"), DescriptionAttribute("World Size"), ReadOnly(true)]
        private RectF _WorldBounds;
        public RectF WorldBounds
        {
            get { return this._WorldBounds; }
            set
            {
                if (this._WorldBounds != value)
                {
                    this._WorldBounds = value;
                    this.RaisePropertyChanged("WorldBounds");
                }
            }
        }

        [CategoryAttribute("World"), DescriptionAttribute("World Size"), ReadOnly(true)]
        private PointInt32 _MaxTiles;
        public PointInt32 MaxTiles
        {
            get { return this._MaxTiles; }
            set
            {
                if (this._MaxTiles != value)
                {
                    this._MaxTiles = value;
                    this.RaisePropertyChanged("MaxTiles");
                }
            }
        }

        [CategoryAttribute("World"), DescriptionAttribute("Spawn Location")]
        private PointInt32 _SpawnTile;
        public PointInt32 SpawnTile
        {
            get { return this._SpawnTile; }
            set
            {
                if (this._SpawnTile != value)
                {
                    this._SpawnTile = value;
                    this.RaisePropertyChanged("SpawnTile");
                }
            }
        }

        [CategoryAttribute("World"), DescriptionAttribute("Surface Level"), ReadOnly(true)]
        private double _WorldSurface;
        public double WorldSurface
        {
            get { return this._WorldSurface; }
            set
            {
                if (this._WorldSurface != value)
                {
                    this._WorldSurface = value;
                    this.RaisePropertyChanged("WorldSurface");
                }
            }
        }



        [CategoryAttribute("World"), DescriptionAttribute("Rock Level"), ReadOnly(true)]
        private double _WorldRockLayer;
        public double WorldRockLayer
        {
            get { return this._WorldRockLayer; }
            set
            {
                if (this._WorldRockLayer != value)
                {
                    this._WorldRockLayer = value;
                    this.RaisePropertyChanged("WorldRockLayer");
                }
            }
        }



        [CategoryAttribute("Time"), DescriptionAttribute("Time of Day")]
        private double _Time;
        public double Time
        {
            get { return this._Time; }
            set
            {
                if (this._Time != value)
                {
                    this._Time = value;
                    this.RaisePropertyChanged("Time");
                }
            }
        }



        [CategoryAttribute("Time"), DescriptionAttribute("Is it Daytime")]
        private bool _IsDayTime;
        public bool IsDayTime
        {
            get { return this._IsDayTime; }
            set
            {
                if (this._IsDayTime != value)
                {
                    this._IsDayTime = value;
                    this.RaisePropertyChanged("IsDayTime");
                }
            }
        }



        [CategoryAttribute("Time"), DescriptionAttribute("Moon Phase")]
        private int _MoonPhase;
        public int MoonPhase
        {
            get { return this._MoonPhase; }
            set
            {
                if (this._MoonPhase != value)
                {
                    this._MoonPhase = value;
                    this.RaisePropertyChanged("MoonPhase");
                }
            }
        }



        [CategoryAttribute("Time"), DescriptionAttribute("Is it a Blood Moon")]
        private bool _IsBloodMoon;
        public bool IsBloodMoon
        {
            get { return this._IsBloodMoon; }
            set
            {
                if (this._IsBloodMoon != value)
                {
                    this._IsBloodMoon = value;
                    this.RaisePropertyChanged("IsBloodMoon");
                }
            }
        }



        [CategoryAttribute("World"), DescriptionAttribute("Dungeon Location"), ReadOnly(true)]
        private PointInt32 _DungeonEntrance;
        public PointInt32 DungeonEntrance
        {
            get { return this._DungeonEntrance; }
            set
            {
                if (this._DungeonEntrance != value)
                {
                    this._DungeonEntrance = value;
                    this.RaisePropertyChanged("DungeonEntrance");
                }
            }
        }



        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Eater of Worlds Dead")]
        private bool _IsBossDowned1;
        public bool IsBossDowned1
        {
            get { return this._IsBossDowned1; }
            set
            {
                if (this._IsBossDowned1 != value)
                {
                    this._IsBossDowned1 = value;
                    this.RaisePropertyChanged("IsBossDowned1");
                }
            }
        }



        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Eye of Cuthulu Dead")]
        private bool _IsBossDowned2;
        public bool IsBossDowned2
        {
            get { return this._IsBossDowned2; }
            set
            {
                if (this._IsBossDowned2 != value)
                {
                    this._IsBossDowned2 = value;
                    this.RaisePropertyChanged("IsBossDowned2");
                }
            }
        }



        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Skeletor Dead")]
        private bool _IsBossDowned3;
        public bool IsBossDowned3
        {
            get { return this._IsBossDowned3; }
            set
            {
                if (this._IsBossDowned3 != value)
                {
                    this._IsBossDowned3 = value;
                    this.RaisePropertyChanged("IsBossDowned3");
                }
            }
        }



        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Have any Shadow Orbs been Smashed?")]
        private bool _IsShadowOrbSmashed;
        public bool IsShadowOrbSmashed
        {
            get { return this._IsShadowOrbSmashed; }
            set
            {
                if (this._IsShadowOrbSmashed != value)
                {
                    this._IsShadowOrbSmashed = value;
                    this.RaisePropertyChanged("IsShadowOrbSmashed");
                }
            }
        }



        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Spawn the Meteor?")]
        private bool _IsSpawnMeteor;
        public bool IsSpawnMeteor
        {
            get { return this._IsSpawnMeteor; }
            set
            {
                if (this._IsSpawnMeteor != value)
                {
                    this._IsSpawnMeteor = value;
                    this.RaisePropertyChanged("IsSpawnMeteor");
                }
            }
        }



        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Number of Shadow Orbs Smashed")]
        private int _ShadowOrbCount;
        public int ShadowOrbCount
        {
            get { return this._ShadowOrbCount; }
            set
            {
                if (this._ShadowOrbCount != value)
                {
                    this._ShadowOrbCount = value;
                    this.RaisePropertyChanged("ShadowOrbCount");
                }
            }
        }



        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Delay")]
        private int _InvasionDelay;
        public int InvasionDelay
        {
            get { return this._InvasionDelay; }
            set
            {
                if (this._InvasionDelay != value)
                {
                    this._InvasionDelay = value;
                    this.RaisePropertyChanged("InvasionDelay");
                }
            }
        }



        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Size")]
        private int _InvasionSize;
        public int InvasionSize
        {
            get { return this._InvasionSize; }
            set
            {
                if (this._InvasionSize != value)
                {
                    this._InvasionSize = value;
                    this.RaisePropertyChanged("InvasionSize");
                }
            }
        }



        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Type")]
        private int _InvasionType;
        public int InvasionType
        {
            get { return this._InvasionType; }
            set
            {
                if (this._InvasionType != value)
                {
                    this._InvasionType = value;
                    this.RaisePropertyChanged("InvasionType");
                }
            }
        }



        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion X Coordinate")]
        private double _InvasionX;
        public double InvasionX
        {
            get { return this._InvasionX; }
            set
            {
                if (this._InvasionX != value)
                {
                    this._InvasionX = value;
                    this.RaisePropertyChanged("InvasionX");
                }
            }
        }


    }
}