using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TEditXNA.Terraria
{
    public static class WorldAnalysis
    {
        private const string propFormat = "{0}: {1}";

        private static void WriteProperty(this StreamWriter sb, string prop, object value)
        {
            sb.WriteLine(propFormat, prop, value);
        }


        public static string AnalyzeWorld(World world)
        {
            if (world == null) return string.Empty;

            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            using (var reader = new StreamReader(ms))
            {
                WriteAnalyzeWorld(writer, world, true);
                writer.Flush();
                ms.Position = 0;

                var text = reader.ReadToEnd();
                return text;
            }
        }

        public static void AnalyzeWorld(World world, string file)
        {
            if (world == null) return;

            using (var writer = new StreamWriter(file, false))
            {
                WriteAnalyzeWorld(writer, world, true);
            }
        }

        private static void WriteAnalyzeWorld(StreamWriter sb, World world, bool fullAnalysis = false)
        {
            world.Validate();
            WriteHeader(sb, world);
            WriteFlags(sb, world);

            if (!fullAnalysis) return;

            sb.WriteLine("===SECTION: Tiles===");

            var tileCounts = new Dictionary<int, int>();

            int activeTiles = 0;
            for (int x = 0; x < world.TilesWide; x++)
            {
                for (int y = 0; y < world.TilesHigh; y++)
                {
                    
                    var tile = world.Tiles[x, y];

                    if (tile.IsActive)
                    {
                        if (tileCounts.ContainsKey(tile.Type))
                        {
                            tileCounts[tile.Type] += 1;
                        }
                        else
                        {
                            tileCounts.Add(tile.Type, 1);
                        }
                        activeTiles++;
                    }

                }
            }

            float totalTiles = world.TilesWide * world.TilesHigh;
            int airTiles = (int)(totalTiles - activeTiles);
            sb.WriteLine("Air: {0} ({1:P2})", airTiles, airTiles / totalTiles);


            var tiles = tileCounts.OrderByDescending(kvp => kvp.Value);
            foreach (var tilePair in tiles)
            {
                string name = tilePair.Key.ToString();
                if (World.TileProperties.Count >= tilePair.Key)
                {
                    var prop = World.TileProperties[tilePair.Key];
                    if (prop != null)
                    {
                        name = prop.Name;
                    }
                }

                sb.WriteLine("{0}: {1} ({2:P2})", name, tilePair.Value, tilePair.Value / totalTiles);
            }


            sb.WriteLine("===SECTION: Chests===");
            sb.WriteProperty("Chest Count", world.Chests.Count);
            sb.WriteProperty("Chest Max Items", Chest.MaxItems);

            foreach (var chest in world.Chests)
            {
                sb.Write("[{0}, {1}] {2} - Contents: ", chest.X, chest.Y, chest.Name);

                for (int i = 0; i < Chest.MaxItems; i++)
                {
                    Item item = chest.Items[i];
                    if (item == null)
                        sb.Write("[{0}]: Empty,", i.ToString());
                    else
                    {
                        sb.Write("[{0}]: {1} - {2}{3},", i.ToString(), item.StackSize, item.PrefixName, item.Name);
                    }
                }

                sb.WriteLine();
            }



            sb.WriteLine("===SECTION: Signs===");
            sb.WriteProperty("Sign Count", world.Signs.Count);

            foreach (var sign in world.Signs)
            {
                sb.Write("[{0}, {1}] {2}\r\n", sign.X, sign.Y, sign.Text);
            }

            sb.WriteLine("===SECTION: NPCs===");
            sb.WriteProperty("NPC Count", world.NPCs.Count);
            foreach (var npc in world.NPCs)
            {
                sb.Write("ID: {0}, Name: {1}, Position: [{2:0.00}, {3:0.00}], {4}: [{5}, {6}]\r\n", npc.Name, npc.DisplayName, npc.Position.X, npc.Position.Y, npc.IsHomeless ? "Homeless" : "Home", npc.Home.X, npc.Home.Y);
            }
            
            sb.WriteLine("===SECTION: Tile Entities===");
            sb.WriteProperty("Tile Entities Count", world.TileEntities.Count);
            foreach (var entity in world.TileEntities)
            {
                switch (entity.Type)
                {
                    case 0:
                        sb.Write("Dummy - ID: {0}, Position: [{1}, {2}], NPC: {3}\r\n", entity.Id, entity.PosX, entity.PosY, entity.Npc);
                        break;
                    case 1:
                        sb.Write("ItemFrame - ID: {0}, Position: [{1}, {2}], ItemID: {3}, Prefix: {4}, Stack: {5}\r\n", entity.Id, entity.PosX, entity.PosY, entity.NetId, entity.Prefix, entity.StackSize);
                        break;
                    case 2:
                        sb.Write("Logic Sensor - ID: {0}, Position: [{1}, {2}], LogicCheck: {3}, On: {4}\r\n", entity.Id, entity.PosX, entity.PosY, entity.LogicCheck, entity.On);
                        break;
                }
            }
        }



        private static void WriteHeader(StreamWriter sb, World world)
        {
            sb.WriteLine("===SECTION: Header===");
            sb.WriteProperty("Compatible Version", world.Version);
            sb.WriteProperty("Section Count", World.SectionCount);
            sb.Write("Frames: ");
            foreach (bool t in World.TileFrameImportant)
            {
                sb.Write(t ? 1 : 0);
            }
            sb.Write(Environment.NewLine);
        }

        private static void WriteFlags(StreamWriter sb, World world)
        {
            sb.WriteProperty("world.FileRevision", world.FileRevision);
            sb.WriteProperty("world.IsFavorite", world.IsFavorite);

            sb.WriteLine("===SECTION: Flags===");

            sb.WriteProperty("world.Title", world.Title);
            sb.WriteProperty("world.WorldId", world.WorldId);
            sb.WriteProperty("world.LeftWorld", world.LeftWorld);
            sb.WriteProperty("world.RightWorld", world.RightWorld);
            sb.WriteProperty("world.TopWorld", world.TopWorld);
            sb.WriteProperty("world.BottomWorld", world.BottomWorld);
            sb.WriteProperty("world.TilesHigh", world.TilesHigh);
            sb.WriteProperty("world.TilesWide", world.TilesWide);

            sb.WriteProperty("world.ExpertMode", world.ExpertMode);
            sb.WriteProperty("world.CreationTime", world.CreationTime);

            sb.WriteProperty("world.MoonType", world.MoonType);
            sb.WriteProperty("world.TreeX[0]", world.TreeX0);
            sb.WriteProperty("world.TreeX[1]", world.TreeX1);
            sb.WriteProperty("world.TreeX[2]", world.TreeX2);
            sb.WriteProperty("world.TreeStyle[0]", world.TreeStyle0);
            sb.WriteProperty("world.TreeStyle[1]", world.TreeStyle1);
            sb.WriteProperty("world.TreeStyle[2]", world.TreeStyle2);
            sb.WriteProperty("world.TreeStyle[3]", world.TreeStyle3);
            sb.WriteProperty("world.CaveBackX[0]", world.CaveBackX0);
            sb.WriteProperty("world.CaveBackX[1]", world.CaveBackX1);
            sb.WriteProperty("world.CaveBackX[2]", world.CaveBackX2);
            sb.WriteProperty("world.CaveBackStyle[0]", world.CaveBackStyle0);
            sb.WriteProperty("world.CaveBackStyle[1]", world.CaveBackStyle1);
            sb.WriteProperty("world.CaveBackStyle[2]", world.CaveBackStyle2);
            sb.WriteProperty("world.CaveBackStyle[3]", world.CaveBackStyle3);
            sb.WriteProperty("world.IceBackStyle", world.IceBackStyle);
            sb.WriteProperty("world.JungleBackStyle", world.JungleBackStyle);
            sb.WriteProperty("world.HellBackStyle", world.HellBackStyle);

            sb.WriteProperty("world.SpawnX", world.SpawnX);
            sb.WriteProperty("world.SpawnY", world.SpawnY);
            sb.WriteProperty("world.GroundLevel", world.GroundLevel);
            sb.WriteProperty("world.RockLevel", world.RockLevel);
            sb.WriteProperty("world.Time", world.Time);
            sb.WriteProperty("world.DayTime", world.DayTime);
            sb.WriteProperty("world.MoonPhase", world.MoonPhase);
            sb.WriteProperty("world.BloodMoon", world.BloodMoon);
            sb.WriteProperty("world.IsEclipse", world.IsEclipse);
            sb.WriteProperty("world.DungeonX", world.DungeonX);
            sb.WriteProperty("world.DungeonY", world.DungeonY);

            sb.WriteProperty("world.IsCrimson", world.IsCrimson);

            sb.WriteProperty("world.DownedBoss1", world.DownedBoss1);
            sb.WriteProperty("world.DownedBoss2", world.DownedBoss2);
            sb.WriteProperty("world.DownedBoss3", world.DownedBoss3);
            sb.WriteProperty("world.DownedQueenBee", world.DownedQueenBee);
            sb.WriteProperty("world.DownedMechBoss1", world.DownedMechBoss1);
            sb.WriteProperty("world.DownedMechBoss2", world.DownedMechBoss2);
            sb.WriteProperty("world.DownedMechBoss3", world.DownedMechBoss3);
            sb.WriteProperty("world.DownedMechBossAny", world.DownedMechBossAny);
            sb.WriteProperty("world.DownedPlantBoss", world.DownedPlantBoss);
            sb.WriteProperty("world.DownedGolemBoss", world.DownedGolemBoss);
            sb.WriteProperty("world.DownedSlimeKingBoss", world.DownedSlimeKingBoss);
            sb.WriteProperty("world.SavedGoblin", world.SavedGoblin);
            sb.WriteProperty("world.SavedWizard", world.SavedWizard);
            sb.WriteProperty("world.SavedMech", world.SavedMech);
            sb.WriteProperty("world.DownedGoblins", world.DownedGoblins);
            sb.WriteProperty("world.DownedClown", world.DownedClown);
            sb.WriteProperty("world.DownedFrost", world.DownedFrost);
            sb.WriteProperty("world.DownedPirates", world.DownedPirates);

            sb.WriteProperty("world.ShadowOrbSmashed", world.ShadowOrbSmashed);
            sb.WriteProperty("world.SpawnMeteor", world.SpawnMeteor);
            sb.WriteProperty("world.ShadowOrbCount", world.ShadowOrbCount);
            sb.WriteProperty("world.AltarCount", world.AltarCount);
            sb.WriteProperty("world.HardMode", world.HardMode);
            sb.WriteProperty("world.InvasionDelay", world.InvasionDelay);
            sb.WriteProperty("world.InvasionSize", world.InvasionSize);
            sb.WriteProperty("world.InvasionType", world.InvasionType);
            sb.WriteProperty("world.InvasionX", world.InvasionX);

            sb.WriteProperty("world.TempRaining", world.TempRaining);
            sb.WriteProperty("world.TempRainTime", world.TempRainTime);
            sb.WriteProperty("world.TempMaxRain", world.TempMaxRain);
            sb.WriteProperty("world.OreTier1", world.OreTier1);
            sb.WriteProperty("world.OreTier2", world.OreTier2);
            sb.WriteProperty("world.OreTier3", world.OreTier3);
            sb.WriteProperty("world.BgTree", world.BgTree);
            sb.WriteProperty("world.BgCorruption", world.BgCorruption);
            sb.WriteProperty("world.BgJungle", world.BgJungle);
            sb.WriteProperty("world.BgSnow", world.BgSnow);
            sb.WriteProperty("world.BgHallow", world.BgHallow);
            sb.WriteProperty("world.BgCrimson", world.BgCrimson);
            sb.WriteProperty("world.BgDesert", world.BgDesert);
            sb.WriteProperty("world.BgOcean", world.BgOcean);
            sb.WriteProperty("world.CloudBgActive", world.CloudBgActive);
            sb.WriteProperty("world.NumClouds", world.NumClouds);
            sb.WriteProperty("world.WindSpeedSet", world.WindSpeedSet);
            sb.WriteProperty("world.Anglers.Count", world.Anglers.Count);

            for (int i = 0; i < world.Anglers.Count; i++)
            {
                sb.WriteProperty("Angler " + i, world.Anglers[i]);
            }

            sb.WriteProperty("world.SavedAngler", world.SavedAngler);
            sb.WriteProperty("world.AnglerQuest", world.AnglerQuest);

            if (world.UnknownData != null && world.UnknownData.Length > 0)
                sb.WriteProperty("world.UnknownData", BitConverter.ToString(world.UnknownData));            
        }
    }
}