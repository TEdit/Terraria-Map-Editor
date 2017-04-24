using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TEditXNA.Terraria
{

    /* SBLogic
     * Rudimentary tally output, based on WorldAnalysis object.
     * Tally indices map to banners, not specific NPCs, mapping is provided by settings.xml
     */

    public static class KillTally
    {

        private const string tallyFormat = "{0}: {1}";

        public static string LoadTally(World world)
        {
            if (world == null) return string.Empty;

            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            using (var reader = new StreamReader(ms))
            {
                WriteTallyCount(writer, world, true);
                writer.Flush();
                ms.Position = 0;

                var text = reader.ReadToEnd();
                return text;
            }
        }

        public static void SaveTally(World world, string file)
        {
            if (world == null) return;

            using (var writer = new StreamWriter(file, false))
            {
                WriteTallyCount(writer, world, true);
            }
        }

        private static void WriteProperty(this StreamWriter sb, string prop, object value)
        {
            sb.WriteLine(tallyFormat, prop, value);
        }

        private static void WriteTallyCount(StreamWriter sb, World world, bool fullAnalysis = false)
        {
            WriteHeader(sb, world);
            WriteTally(sb, world);
        }

        private static void WriteHeader(StreamWriter sb, World world)
        {
            sb.WriteProperty("Compatible Version", world.Version);
            sb.Write(Environment.NewLine);
        }

        private static void WriteTally(StreamWriter sb, World world)
        {

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
                    if (index > 0 && index <= World.TallyNames.Count)
                    {
                        World.TallyNames[index] = Regex.Replace(World.TallyNames[index], @" Banner", "");
                        bufferNoKill += $"[{index}] {World.TallyNames[index]}\n";
                    }
                        
                }
                else if (count < 50)
                {
                    // Monster killed, but banner never obtained (less than 50 kills)
                    World.TallyNames[index] = Regex.Replace(World.TallyNames[index], @" Banner", "");
                    bufferNoBanner += $"[{index}] {World.TallyNames[index]} - {count}\n";
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

                    World.TallyNames[index] = Regex.Replace(World.TallyNames[index], @" Banner", "");
                    bufferBanner += $"[{index}] {World.TallyNames[index]} - {count} ({banners} {bannerText} earned)\n";
                    killcount = killcount + count;
                    uniquecount = uniquecount + 1;
                    bannercount = bannercount + banners;
                }
                index++;
            }

            // Print lines ...
            sb.WriteLine("=== Kills ===");
            sb.WriteLine(bufferBanner);
            sb.Write(Environment.NewLine);

            sb.WriteLine("=== Less than 50 kills ===");
            sb.WriteLine(bufferNoBanner);
            sb.Write(Environment.NewLine);

            sb.WriteLine("=== No kills ===");
            sb.WriteLine(bufferNoKill);
            sb.Write(Environment.NewLine);

            sb.WriteLine("Total kills counted: {0}", killcount);
            sb.WriteLine("Total banners awarded: {0}", bannercount);
            sb.WriteLine("Total unique banners: {0}", uniquecount);
        }

    }
}
