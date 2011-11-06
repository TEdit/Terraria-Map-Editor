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
        private int _MoonPhase;
        private int _ShadowOrbCount;
        private PointInt32 _SpawnTile;
        private double _Time;
        private RectI _WorldBounds;
        private int _WorldId;
        private string _WorldName;
        private double _WorldRockLayer;
        private double _WorldSurface;

        public WorldHeader()
        {
            _FileName = "";
            _WorldName = "No World Loaded";
        }

        public string FileName
        {
            get { return _FileName; }
            set { SetProperty(ref _FileName, ref value, "FileName"); }
        }

        [Category("World"), DescriptionAttribute("Terraria Save File Version")]
        public int FileVersion
        {
            get { return _FileVersion; }
            set { SetProperty(ref _FileVersion, ref value, "FileVersion"); }
        }

        [CategoryAttribute("World"), DescriptionAttribute("World Name")]
        public string WorldName
        {
            get { return _WorldName; }
            set { SetProperty(ref _WorldName, ref value, "WorldName"); }
        }

        [CategoryAttribute("World"), DescriptionAttribute("World ID"), ReadOnly(true)]
        public int WorldId
        {
            get { return _WorldId; }
            set { SetProperty(ref _WorldId, ref value, "WorldId"); }
        }

        [CategoryAttribute("World"), DescriptionAttribute("World Size"), ReadOnly(true)]
        public RectI WorldBounds
        {
            get { return _WorldBounds; }
            set { SetProperty(ref _WorldBounds, ref value, "WorldBounds"); }
        }

        [CategoryAttribute("World"), DescriptionAttribute("Spawn Location")]
        public PointInt32 SpawnTile
        {
            get { return _SpawnTile; }
            set { SetProperty(ref _SpawnTile, ref value, "SpawnTile"); }
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
                if (validValue > WorldBounds.H)
                    validValue = WorldBounds.Bottom;

                SetProperty(ref _WorldSurface, ref value, "WorldSurface");                
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
                if (validValue > WorldBounds.H)
                    validValue = WorldBounds.Bottom;

                SetProperty(ref _WorldRockLayer, ref value, "WorldRockLayer");
            }
        }

        [CategoryAttribute("Time"), DescriptionAttribute("Time of Day")]
        public double Time
        {
            get { return _Time; }
            set { SetProperty(ref _Time, ref value, "Time"); }
        }

        [CategoryAttribute("Time"), DescriptionAttribute("Is it Daytime")]
        public bool IsDayTime
        {
            get { return _IsDayTime; }
            set { SetProperty(ref _IsDayTime, ref value, "IsDayTime"); }
        }

        [CategoryAttribute("Time"), DescriptionAttribute("Moon Phase")]
        public int MoonPhase
        {
            get { return _MoonPhase; }
            set { SetProperty(ref _MoonPhase, ref value, "MoonPhase"); }
        }

        [CategoryAttribute("Time"), DescriptionAttribute("Is it a Blood Moon")]
        public bool IsBloodMoon
        {
            get { return _IsBloodMoon; }
            set { SetProperty(ref _IsBloodMoon, ref value, "IsBloodMoon"); }
        }

        [Category("World"), DescriptionAttribute("Dungeon Location"), ReadOnly(true)]
        public PointInt32 DungeonEntrance
        {
            get { return _DungeonEntrance; }
            set { SetProperty(ref _DungeonEntrance, ref value, "DungeonEntrance"); }
        }

        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Eye of Cthulhu Dead")]
        public bool IsBossDowned1
        {
            get { return _IsBossDowned1; }
            set { SetProperty(ref _IsBossDowned1, ref value, "IsBossDowned1"); }
        }

        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Eater of Worlds Dead")]
        public bool IsBossDowned2
        {
            get { return _IsBossDowned2; }
            set { SetProperty(ref _IsBossDowned2, ref value, "IsBossDowned2"); }
        }

        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Skeletron Dead")]
        public bool IsBossDowned3
        {
            get { return _IsBossDowned3; }
            set { SetProperty(ref _IsBossDowned3, ref value, "IsBossDowned3"); }
        }

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Have any Shadow Orbs been Smashed?")]
        public bool IsShadowOrbSmashed
        {
            get { return _IsShadowOrbSmashed; }
            set { SetProperty(ref _IsShadowOrbSmashed, ref value, "IsShadowOrbSmashed"); }
        }

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Spawn the Meteor?")]
        public bool IsSpawnMeteor
        {
            get { return _IsSpawnMeteor; }
            set { SetProperty(ref _IsSpawnMeteor, ref value, "IsSpawnMeteor"); }
        }

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Number of Shadow Orbs Smashed")]
        public int ShadowOrbCount
        {
            get { return _ShadowOrbCount; }
            set { SetProperty(ref _ShadowOrbCount, ref value, "ShadowOrbCount"); IsShadowOrbSmashed = ShadowOrbCount > 0; }
        }

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Delay")]
        public int InvasionDelay
        {
            get { return _InvasionDelay; }
            set { SetProperty(ref _InvasionDelay, ref value, "InvasionDelay"); }
        }

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Size")]
        public int InvasionSize
        {
            get { return _InvasionSize; }
            set { SetProperty(ref _InvasionSize, ref value, "InvasionSize"); }
        }

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Type")]
        public int InvasionType
        {
            get { return _InvasionType; }
            set { SetProperty(ref _InvasionType, ref value, "InvasionType"); }
        }

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion X Coordinate")]
        public double InvasionX
        {
            get { return _InvasionX; }
            set { SetProperty(ref _InvasionX, ref value, "InvasionX"); }
        }

        public WorldHeader Clone()
        {
            return (WorldHeader) MemberwiseClone();
        }
    }
}