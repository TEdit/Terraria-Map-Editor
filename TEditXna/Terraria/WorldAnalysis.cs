using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

namespace TEditXNA.Terraria
{
    public static class WorldAnalysis
    {
        private const string propFormat = "{0}: {1}\r\n";

        public static void Write(this StringBuilder sb, string prop, object value)
        {
            sb.AppendFormat(propFormat, prop, value);
        }

        public static string AnalyseWorld(World world)
        {
            var sb = new StringBuilder();

            WriteHeader(sb, world);
            WriteFlags(sb, world);

            sb.AppendLine("===SECTION: Chests===");
            sb.Write("Chest Count", world.Chests.Count);
            sb.Write("Chest Max Items", Chest.MaxItems);

            foreach (var chest in world.Chests)
            {
                sb.AppendFormat("[{0}, {1}] {2} - Contents: ", chest.X, chest.Y, chest.Name);

                for (int i = 0; i < Chest.MaxItems; i++)
                {
                    Item item = chest.Items[i];
                    if (item == null)
                        sb.AppendFormat("[{0}]: Empty,", i.ToString());
                    else
                    {
                        sb.AppendFormat("[{0}]: {1} - {2}{3},", i.ToString(), item.StackSize, item.PrefixName, item.Name);
                    }
                }

                sb.AppendLine();
            }

            sb.AppendLine("===SECTION: Signs===");
            sb.Write("Sign Count", world.Signs.Count);

            foreach (var sign in world.Signs)
            {
                sb.AppendFormat("[{0}, {1}] {2}", sign.X, sign.Y, sign.Text);
            }

            sb.AppendLine("===SECTION: NPCs===");
            sb.Write("NPC Count", world.NPCs.Count);
            foreach (var npc in world.NPCs)
            {
                sb.AppendFormat("ID: {0}, Name: {1}, Position: [{2:0.00}, {3:0.00}], {4}: [{5}, {6}]\r\n", npc.Name, npc.DisplayName, npc.Position.X, npc.Position.Y, npc.IsHomeless ? "Homeless" : "Home", npc.Home.X, npc.Home.Y);
            }


            return sb.ToString();
        }



        private static void WriteHeader(StringBuilder sb, World world)
        {
            sb.AppendLine("===SECTION: Header===");
            sb.Write("Compatible Version", world.Version);
            sb.Write("Section Count", World.SectionCount);
            sb.Append("Frames: ");
            foreach (bool t in World.TileFrameImportant)
            {
                sb.Append(t ? 1 : 0);
            }
            sb.Append(Environment.NewLine);
        }

        private static void WriteFlags(StringBuilder sb, World world)
        {
            sb.AppendLine("===SECTION: Flags===");

            sb.Write("world.Title", world.Title);
            sb.Write("world.WorldId", world.WorldId);
            sb.Write("world.LeftWorld", world.LeftWorld);
            sb.Write("world.RightWorld", world.RightWorld);
            sb.Write("world.TopWorld", world.TopWorld);
            sb.Write("world.BottomWorld", world.BottomWorld);
            sb.Write("world.TilesHigh", world.TilesHigh);
            sb.Write("world.TilesWide", world.TilesWide);

            sb.Write("world.MoonType", world.MoonType);
            sb.Write("world.TreeX[0]", world.TreeX[0]);
            sb.Write("world.TreeX[1]", world.TreeX[1]);
            sb.Write("world.TreeX[2]", world.TreeX[2]);
            sb.Write("world.TreeStyle[0]", world.TreeStyle[0]);
            sb.Write("world.TreeStyle[1]", world.TreeStyle[1]);
            sb.Write("world.TreeStyle[2]", world.TreeStyle[2]);
            sb.Write("world.TreeStyle[3]", world.TreeStyle[3]);
            sb.Write("world.CaveBackX[0]", world.CaveBackX[0]);
            sb.Write("world.CaveBackX[1]", world.CaveBackX[1]);
            sb.Write("world.CaveBackX[2]", world.CaveBackX[2]);
            sb.Write("world.CaveBackStyle[0]", world.CaveBackStyle[0]);
            sb.Write("world.CaveBackStyle[1]", world.CaveBackStyle[1]);
            sb.Write("world.CaveBackStyle[2]", world.CaveBackStyle[2]);
            sb.Write("world.CaveBackStyle[3]", world.CaveBackStyle[3]);
            sb.Write("world.IceBackStyle", world.IceBackStyle);
            sb.Write("world.JungleBackStyle", world.JungleBackStyle);
            sb.Write("world.HellBackStyle", world.HellBackStyle);

            sb.Write("world.SpawnX", world.SpawnX);
            sb.Write("world.SpawnY", world.SpawnY);
            sb.Write("world.GroundLevel", world.GroundLevel);
            sb.Write("world.RockLevel", world.RockLevel);
            sb.Write("world.Time", world.Time);
            sb.Write("world.DayTime", world.DayTime);
            sb.Write("world.MoonPhase", world.MoonPhase);
            sb.Write("world.BloodMoon", world.BloodMoon);
            sb.Write("world.IsEclipse", world.IsEclipse);
            sb.Write("world.DungeonX", world.DungeonX);
            sb.Write("world.DungeonY", world.DungeonY);

            sb.Write("world.IsCrimson", world.IsCrimson);

            sb.Write("world.DownedBoss1", world.DownedBoss1);
            sb.Write("world.DownedBoss2", world.DownedBoss2);
            sb.Write("world.DownedBoss3", world.DownedBoss3);
            sb.Write("world.DownedQueenBee", world.DownedQueenBee);
            sb.Write("world.DownedMechBoss1", world.DownedMechBoss1);
            sb.Write("world.DownedMechBoss2", world.DownedMechBoss2);
            sb.Write("world.DownedMechBoss3", world.DownedMechBoss3);
            sb.Write("world.DownedMechBossAny", world.DownedMechBossAny);
            sb.Write("world.DownedPlantBoss", world.DownedPlantBoss);
            sb.Write("world.DownedGolemBoss", world.DownedGolemBoss);
            sb.Write("world.SavedGoblin", world.SavedGoblin);
            sb.Write("world.SavedWizard", world.SavedWizard);
            sb.Write("world.SavedMech", world.SavedMech);
            sb.Write("world.DownedGoblins", world.DownedGoblins);
            sb.Write("world.DownedClown", world.DownedClown);
            sb.Write("world.DownedFrost", world.DownedFrost);
            sb.Write("world.DownedPirates", world.DownedPirates);

            sb.Write("world.ShadowOrbSmashed", world.ShadowOrbSmashed);
            sb.Write("world.SpawnMeteor", world.SpawnMeteor);
            sb.Write("world.ShadowOrbCount", world.ShadowOrbCount);
            sb.Write("world.AltarCount", world.AltarCount);
            sb.Write("world.HardMode", world.HardMode);
            sb.Write("world.InvasionDelay", world.InvasionDelay);
            sb.Write("world.InvasionSize", world.InvasionSize);
            sb.Write("world.InvasionType", world.InvasionType);
            sb.Write("world.InvasionX", world.InvasionX);

            sb.Write("world.TempRaining", world.TempRaining);
            sb.Write("world.TempRainTime", world.TempRainTime);
            sb.Write("world.TempMaxRain", world.TempMaxRain);
            sb.Write("world.OreTier1", world.OreTier1);
            sb.Write("world.OreTier2", world.OreTier2);
            sb.Write("world.OreTier3", world.OreTier3);
            sb.Write("world.BgTree", world.BgTree);
            sb.Write("world.BgCorruption", world.BgCorruption);
            sb.Write("world.BgJungle", world.BgJungle);
            sb.Write("world.BgSnow", world.BgSnow);
            sb.Write("world.BgHallow", world.BgHallow);
            sb.Write("world.BgCrimson", world.BgCrimson);
            sb.Write("world.BgDesert", world.BgDesert);
            sb.Write("world.BgOcean", world.BgOcean);
            sb.Write("world.CloudBgActive", world.CloudBgActive);
            sb.Write("world.NumClouds", world.NumClouds);
            sb.Write("world.WindSpeedSet", world.WindSpeedSet);
            sb.Write("world.Anglers.Count", world.Anglers.Count);

            for (int i = 0; i < world.Anglers.Count; i++)
            {
                sb.Write("Angler " + i, world.Anglers[i]);
            }

            sb.Write("world.SavedAngler", world.SavedAngler);
            sb.Write("world.AnglerQuest", world.AnglerQuest);

            if (world.UnknownData != null && world.UnknownData.Length > 0)
                sb.Write("world.UnknownData", BitConverter.ToString(world.UnknownData));
        }
    }
}