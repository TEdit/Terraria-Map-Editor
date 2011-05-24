using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace TerrariaWorld.Game
{
    public class WorldHeader
    {
        public WorldHeader()
        {
            this.FileName = "";
            this.WorldName = "No World Loaded";
            this.MaxTiles = new Common.Point(0, 0);
        }

        public WorldHeader Clone()
        {
            return (WorldHeader)this.MemberwiseClone();
        }

        [Browsable(false)]
        public string FileName { get; set; }

        [CategoryAttribute("World"), DescriptionAttribute("Terraria Save File Version")]
        public int FileVersion { get; set; }

        [CategoryAttribute("World"), DescriptionAttribute("World Name")]
        public string WorldName { get; set; }

        [CategoryAttribute("World"), DescriptionAttribute("World ID"), ReadOnly(true)]
        public int WorldID { get; set; }

        [CategoryAttribute("World"), DescriptionAttribute("World Size"), ReadOnly(true)]
        public Common.RectF WorldBounds { get; set; }

        [CategoryAttribute("World"), DescriptionAttribute("World Size"), ReadOnly(true)]
        public Common.Point MaxTiles { get; set; }

        [CategoryAttribute("World"), DescriptionAttribute("Spawn Location")]
        public Common.Point SpawnTile { get; set; }

        [CategoryAttribute("World"), DescriptionAttribute("Surface Level"), ReadOnly(true)]
        public double WorldSurface { get; set; }

        [CategoryAttribute("World"), DescriptionAttribute("Rock Level"), ReadOnly(true)]
        public double WorldRockLayer { get; set; }

        [CategoryAttribute("Time"), DescriptionAttribute("Time of Day")]
        public double Time { get; set; }

        [CategoryAttribute("Time"), DescriptionAttribute("Is it Daytime")]
        public bool IsDayTime { get; set; }

        [CategoryAttribute("Time"), DescriptionAttribute("Moon Phase")]
        public int MoonPhase { get; set; }

        [CategoryAttribute("Time"), DescriptionAttribute("Is it a Blood Moon")]
        public bool IsBloodMoon { get; set; }

        [CategoryAttribute("World"), DescriptionAttribute("Dungeon Location"), ReadOnly(true)]
        public Common.Point DungeonEntrance { get; set; }

        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Eater of Worlds Dead")]
        public bool IsBossDowned1 { get; set; }

        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Eye of Cuthulu Dead")]
        public bool IsBossDowned2 { get; set; }

        [CategoryAttribute("Bosses"), DescriptionAttribute("Is Skeletor Dead")]
        public bool IsBossDowned3 { get; set; }

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Have any Shadow Orbs been Smashed?")]
        public bool IsShadowOrbSmashed { get; set; }

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Spawn the Meteor?")]
        public bool IsSpawnMeteor { get; set; }

        [CategoryAttribute("Shadow Orbs"), DescriptionAttribute("Number of Shadow Orbs Smashed")]
        public int ShadowOrbCount { get; set; }

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Delay")]
        public int InvasionDelay { get; set; }

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Size")]
        public int InvasionSize { get; set; }

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion Type")]
        public int InvasionType { get; set; }

        [CategoryAttribute("Invasion"), DescriptionAttribute("Invasion X Coordinate")]
        public double InvasionX { get; set; }
    }
}
