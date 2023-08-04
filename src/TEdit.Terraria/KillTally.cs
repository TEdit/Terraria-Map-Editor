using System;
using System.Text;
using System.Text.RegularExpressions;
using TEdit.Configuration;

namespace TEdit.Terraria;


/* SBLogic
 * Rudimentary tally output, based on WorldAnalysis object.
 * Tally indices map to banners, not specific NPCs, mapping is provided by settings.xml
 */

public static class KillTally
{
    public static string LoadTally(World world)
    {
        if (world == null) return string.Empty;


        var sb = new StringBuilder();
        int index = 0;
        int killcount = 0;
        int bannercount = 0;
        int uniquecount = 0;

        string bufferBanner = String.Empty;
        string bufferNoBanner = String.Empty;
        string bufferNoKill = String.Empty;

        // Let's explore each monster
        foreach (int count in world.KilledMobs)
        {

            if (count == 0)
            {
                // Monster never killed
                if (index > 0 && index <= WorldConfiguration.TallyNames.Count)
                {
                    WorldConfiguration.TallyNames[index] = Regex.Replace(WorldConfiguration.TallyNames[index], @" Banner", "");
                    bufferNoKill += $"[{index}] {WorldConfiguration.TallyNames[index]}\n";
                }

            }
            else if (count < 50)
            {
                // Monster killed, but banner never obtained (less than 50 kills)
                WorldConfiguration.TallyNames[index] = Regex.Replace(WorldConfiguration.TallyNames[index], @" Banner", "");
                bufferNoBanner += $"[{index}] {WorldConfiguration.TallyNames[index]}: {count}\n";
                killcount = killcount + count;
            }
            else
            {
                // Banners ! 50+ kills for this monster
                int banners = (int)Math.Floor((double)count / 50f);
                string bannerText = String.Empty;

                // "banner" or "bannerS" ?
                if (banners > 1)
                    bannerText = "banners";
                else
                    bannerText = "banner";

                WorldConfiguration.TallyNames[index] = Regex.Replace(WorldConfiguration.TallyNames[index], @" Banner", "");
                bufferBanner += $"[{index}] {WorldConfiguration.TallyNames[index]}: {count} ({banners} {bannerText} earned)\n";
                killcount = killcount + count;
                uniquecount = uniquecount + 1;
                bannercount = bannercount + banners;
            }
            index++;
        }

        // Print lines ...
        sb.AppendLine("=== Kills ===");
        sb.AppendLine(bufferBanner);
        sb.Append(Environment.NewLine);
           
        sb.AppendLine("=== Less than 50 kills ===");
        sb.AppendLine(bufferNoBanner);
        sb.Append(Environment.NewLine);
           
        sb.AppendLine("=== No kills ===");
        sb.AppendLine(bufferNoKill);
        sb.Append(Environment.NewLine);
           
        sb.AppendLine($"Total kills counted: {killcount}");
        sb.AppendLine($"Total banners awarded: {bannercount}");
        sb.AppendLine($"Total unique banners: {uniquecount}");

        sb.AppendLine("=== BESTIARY ===");
        sb.AppendLine("=== NPCs Near Player ===");
        foreach (var item in world.Bestiary.NPCNear)
        {
            sb.AppendLine(item);
        }
        sb.AppendLine("=== NPCs Talked To ===");
        foreach (var item in world.Bestiary.NPCChat)
        {
            sb.AppendLine(item);
        }
        sb.AppendLine("=== NPCs Killed ===");
        foreach (var item in world.Bestiary.NPCKills)
        {
            sb.AppendLine($"{item.Key}: {item.Value}");
        }

        return sb.ToString();
    }
}
