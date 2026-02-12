using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEdit.Terraria;

namespace TEdit.Editor;

public static class WorldAnalysis
{
    private const string propFormat = "{0}: {1}";

    private static void WriteProperty(this StreamWriter sb, string prop, object value)
    {
        sb.WriteLine(propFormat, prop, value);
    }


    public static string AnalyzeWorld(World world)
    {
        if (world == null)
            return string.Empty;

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
        if (world == null)
            return;

        using (var writer = new StreamWriter(file, false))
        {
            WriteAnalyzeWorld(writer, world, true);
        }
    }

    private static void WriteAnalyzeWorld(StreamWriter sb, World world, bool fullAnalysis = false)
    {
        // try
        // {
        //     world.ValidateAsync();
        // }
        // catch (Exception ex)
        // {
        //     sb.WriteLine(ex.Message);
        // }

        WriteHeader(sb, world);
        WriteFlags(sb, world);

        if (!fullAnalysis)
            return;

        sb.WriteLine("===物块部分===");

        var tileCounts = new Dictionary<int, int>();
        var wireCounts = new List<int>() { 0, 0, 0, 0 };



        int activeTiles = 0;
        for (int x = 0; x < world.TilesWide; x++)
        {
            for (int y = 0; y < world.TilesHigh; y++)
            {

                var tile = world.Tiles[x, y];

                if (tile.WireBlue) { wireCounts[0]++; }
                if (tile.WireGreen) { wireCounts[1]++; }
                if (tile.WireRed) { wireCounts[2]++; }
                if (tile.WireYellow) { wireCounts[3]++; }

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
        sb.WriteLine("空气: {0} ({1:P2})", airTiles, airTiles / totalTiles);


        var tiles = tileCounts.OrderByDescending(kvp => kvp.Value);
        foreach (var tilePair in tiles)
        {
            string name = tilePair.Key.ToString();
            if (WorldConfiguration.TileProperties.Count >= tilePair.Key)
            {
                var prop = WorldConfiguration.TileProperties[tilePair.Key];
                if (prop != null)
                {
                    name = prop.Name;
                }
            }

            sb.WriteLine("{0}: {1} ({2:P2})", name, tilePair.Value, tilePair.Value / totalTiles);
        }


        sb.WriteLine("===Wires===");
        sb.WriteLine("红电线: {0}", wireCounts[2]);
        sb.WriteLine("蓝电线: {0}", wireCounts[0]);
        sb.WriteLine("绿电线: {0}", wireCounts[1]);
        sb.WriteLine("黄电线: {0}", wireCounts[3]);
        sb.WriteLine("总共: {0}", wireCounts[0] + wireCounts[1] + wireCounts[2] + wireCounts[3]);


        sb.WriteLine("===宝箱部分===");
        sb.WriteProperty("宝箱统计", world.Chests.Count);
        sb.WriteProperty("宝箱最大物品数", Chest.LegacyMaxItems);

        foreach (var chest in world.Chests)
        {
            sb.Write("[{0}, {1}] {2} - 内容: ", chest.X, chest.Y, chest.Name);

            for (int i = 0; i < Chest.LegacyMaxItems; i++)
            {
                Item item = chest.Items[i];
                if (item == null)
                    sb.Write("[{0}]: 空,", i.ToString());
                else
                {
                    sb.Write("[{0}]: {1} - {2}{3},", i.ToString(), item.StackSize, item.PrefixName, item.Name);
                }
            }

            sb.WriteLine();
        }



        sb.WriteLine("===标牌部分===");
        sb.WriteProperty("Sign Count", world.Signs.Count);

        foreach (var sign in world.Signs)
        {
            sb.Write("[{0}, {1}] {2}\r\n", sign.X, sign.Y, sign.Text);
        }

        sb.WriteLine("===NPC部分===");
        sb.WriteProperty("NPC Count", world.NPCs.Count);
        foreach (var npc in world.NPCs)
        {
            sb.Write("ID: {0}, 名字: {1}, 位置: [{2:0.00}, {3:0.00}], {4}: [{5}, {6}]\r\n", npc.Name, npc.DisplayName, npc.Position.X, npc.Position.Y, npc.IsHomeless ? "无家可归" : "家", npc.Home.X, npc.Home.Y);
        }

        sb.WriteLine("===实体块部分===");
        sb.WriteProperty("Tile Entities Count", world.TileEntities.Count);
        foreach (var entity in world.TileEntities)
        {
            switch (entity.Type)
            {
                case 0:
                    sb.Write("虚拟 - ID: {0}, 位置: [{1}, {2}], NPC: {3}\r\n", entity.Id, entity.PosX, entity.PosY, entity.Npc);
                    break;
                case 1:
                    sb.Write("物品框 - ID: {0}, 位置: [{1}, {2}], 物品ID: {3}, 前缀: {4}, 堆叠数: {5}\r\n", entity.Id, entity.PosX, entity.PosY, entity.NetId, entity.Prefix, entity.StackSize);
                    break;
                case 2:
                    sb.Write("逻辑感应器 - ID: {0}, 位置: [{1}, {2}], 逻辑检查: {3}, 开: {4}\r\n", entity.Id, entity.PosX, entity.PosY, entity.LogicCheck, entity.On);
                    break;
            }
        }
    }



    private static void WriteHeader(StreamWriter sb, World world)
    {
        sb.WriteLine("===标题部分===");
        sb.WriteProperty("兼容版本", world.Version);
        sb.WriteProperty("**部分数量", world.GetSectionCount());
        sb.Write("框架: ");
        foreach (bool t in world.TileFrameImportant)
        {
            sb.Write(t ? 1 : 0);
        }
        sb.Write(Environment.NewLine);
    }

    private static void WriteFlags(StreamWriter sb, World world)
    {
        sb.WriteProperty("世界文件修订", world.FileRevision);
        sb.WriteProperty("是否收藏世界", world.IsFavorite);

        sb.WriteLine("===标志部分===");

        sb.WriteProperty("世界标题", world.Title);
        sb.WriteProperty("世界ID", world.WorldId);
        sb.WriteProperty("世界左边", world.LeftWorld);
        sb.WriteProperty("世界右边", world.RightWorld);
        sb.WriteProperty("世界上边", world.TopWorld);
        sb.WriteProperty("世界下边", world.BottomWorld);
        sb.WriteProperty("物块高度", world.TilesHigh);
        sb.WriteProperty("物块宽度", world.TilesWide);

        sb.WriteProperty("游戏难度", world.GameMode);
        sb.WriteProperty("醉酒世界", world.DrunkWorld);
        sb.WriteProperty("创建时间", world.CreationTime);
        sb.WriteProperty("最后游玩", world.LastPlayed);

        sb.WriteProperty("月亮样式", world.MoonType);
        sb.WriteProperty("树X[0]", world.TreeX0);
        sb.WriteProperty("树X[1]", world.TreeX1);
        sb.WriteProperty("树X[2]", world.TreeX2);
        sb.WriteProperty("树样式[0]", world.TreeStyle0);
        sb.WriteProperty("树样式[1]", world.TreeStyle1);
        sb.WriteProperty("树样式[2]", world.TreeStyle2);
        sb.WriteProperty("树样式[3]", world.TreeStyle3);
        sb.WriteProperty("洞穴背景X[0]", world.CaveBackX0);
        sb.WriteProperty("洞穴背景X[1]", world.CaveBackX1);
        sb.WriteProperty("洞穴背景X[2]", world.CaveBackX2);
        sb.WriteProperty("洞穴背景样式[0]", world.CaveBackStyle0);
        sb.WriteProperty("洞穴背景样式[1]", world.CaveBackStyle1);
        sb.WriteProperty("洞穴背景样式[2]", world.CaveBackStyle2);
        sb.WriteProperty("洞穴背景样式[3]", world.CaveBackStyle3);
        sb.WriteProperty("雪原背景样式", world.IceBackStyle);
        sb.WriteProperty("丛林背景样式", world.JungleBackStyle);
        sb.WriteProperty("地狱背景样式", world.HellBackStyle);

        sb.WriteProperty("生成X", world.SpawnX);
        sb.WriteProperty("生成Y", world.SpawnY);
        sb.WriteProperty("地表层", world.GroundLevel);
        sb.WriteProperty("洞穴层", world.RockLevel);
        sb.WriteProperty("时间", world.Time);
        sb.WriteProperty("日间", world.DayTime);
        sb.WriteProperty("月相", world.MoonPhase);
        sb.WriteProperty("血月", world.BloodMoon);
        sb.WriteProperty("日食", world.IsEclipse);
        sb.WriteProperty("地牢X", world.DungeonX);
        sb.WriteProperty("地牢Y", world.DungeonY);

        sb.WriteProperty("猩红", world.IsCrimson);

        sb.WriteProperty("已击败的Boss1", world.DownedBoss1EyeofCthulhu);
        sb.WriteProperty("已击败的Boss2", world.DownedBoss2EaterofWorlds);
        sb.WriteProperty("已击败的Boss3", world.DownedBoss3Skeletron);
        sb.WriteProperty("已击败的QueenBee", world.DownedQueenBee);
        sb.WriteProperty("已击败的MechBoss1", world.DownedMechBoss1TheDestroyer);
        sb.WriteProperty("已击败的MechBoss2", world.DownedMechBoss2TheTwins);
        sb.WriteProperty("已击败的MechBoss3", world.DownedMechBoss3SkeletronPrime);
        sb.WriteProperty("已击败的MechBossAny", world.DownedMechBossAny);
        sb.WriteProperty("已击败的PlantBoss", world.DownedPlantBoss);
        sb.WriteProperty("已击败的GolemBoss", world.DownedGolemBoss);
        sb.WriteProperty("已击败的SlimeKingBoss", world.DownedSlimeKingBoss);
        sb.WriteProperty("已拯救哥布林", world.SavedGoblin);
        sb.WriteProperty("已拯救巫师", world.SavedWizard);
        sb.WriteProperty("已拯救机械师", world.SavedMech);
        sb.WriteProperty("已击败哥布林", world.DownedGoblins);
        sb.WriteProperty("已击败小丑", world.DownedClown);
        sb.WriteProperty("已击败雪人", world.DownedFrost);
        sb.WriteProperty("已击败海盗", world.DownedPirates);

        sb.WriteProperty("暗影珠被粉碎", world.ShadowOrbSmashed);
        sb.WriteProperty("生成陨石", world.SpawnMeteor);
        sb.WriteProperty("暗影珠数量", world.ShadowOrbCount);
        sb.WriteProperty("祭坛数量", world.AltarCount);
        sb.WriteProperty("困难模式", world.HardMode);
        sb.WriteProperty("入侵延迟", world.InvasionDelay);
        sb.WriteProperty("入侵规模", world.InvasionSize);
        sb.WriteProperty("入侵类型", world.InvasionType);
        sb.WriteProperty("入侵X", world.InvasionX);

        sb.WriteProperty("正在下雨", world.IsRaining);
        sb.WriteProperty("下雨时长", world.TempRainTime);
        sb.WriteProperty("最大降雨", world.TempMaxRain);
        sb.WriteProperty("生成钴矿", world.SavedOreTiersCobalt);
        sb.WriteProperty("生成秘银矿", world.SavedOreTiersMythril);
        sb.WriteProperty("生成钛金", world.SavedOreTiersAdamantite);
        sb.WriteProperty("树背景", world.BgTree);
        sb.WriteProperty("腐化背景", world.BgCorruption);
        sb.WriteProperty("丛林背景", world.BgJungle);
        sb.WriteProperty("雪原背景", world.BgSnow);
        sb.WriteProperty("神圣背景", world.BgHallow);
        sb.WriteProperty("猩红背景", world.BgCrimson);
        sb.WriteProperty("沙漠背景", world.BgDesert);
        sb.WriteProperty("海洋背景", world.BgOcean);
        sb.WriteProperty("云背景活动", world.CloudBgActive);
        sb.WriteProperty("云量", world.NumClouds);
        sb.WriteProperty("风速", world.WindSpeedSet);
        sb.WriteProperty("渔数", world.Anglers.Count);

        for (int i = 0; i < world.Anglers.Count; i++)
        {
            sb.WriteProperty("钓鱼 " + i, world.Anglers[i]);
        }

        sb.WriteProperty("已拯救渔夫", world.SavedAngler);
        sb.WriteProperty("渔夫任务", world.AnglerQuest);

        sb.WriteProperty("蘑菇背景", world.MushroomBg);
        sb.WriteProperty("地下蘑菇背景", world.UnderworldBg);
        sb.WriteProperty("树背景2", world.BgTree2);
        sb.WriteProperty("树背景3", world.BgTree3);
        sb.WriteProperty("树背景4", world.BgTree4);
        sb.WriteProperty("先进战斗技术", world.CombatBookUsed);

        // tree tops
        for (int i = 0; i < world.TreeTopVariations.Count; i++)
        {
            sb.WriteProperty("树顶样式 " + i, world.TreeTopVariations[i]);
        }

        sb.WriteProperty("今日强制神圣", world.ForceHalloweenForToday);
        sb.WriteProperty("今日强制圣诞", world.ForceXMasForToday);
        sb.WriteProperty("生成铜", world.SavedOreTiersCopper);
        sb.WriteProperty("生成铁", world.SavedOreTiersIron);
        sb.WriteProperty("生成银", world.SavedOreTiersSilver);
        sb.WriteProperty("生成金", world.SavedOreTiersGold);

        sb.WriteProperty("已购猫", world.BoughtCat);
        sb.WriteProperty("已购狗", world.BoughtDog);
        sb.WriteProperty("已购兔", world.BoughtBunny);

        sb.WriteProperty("已击败光之女皇", world.DownedEmpressOfLight);
        sb.WriteProperty("已击败史莱姆女王", world.DownedQueenSlime);

        if (world.UnknownData != null && world.UnknownData.Length > 0)
            sb.WriteProperty("未知数据", BitConverter.ToString(world.UnknownData));
    }
}
