using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

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

            sb.WriteLine("=== Kills ===");
            foreach (int count in world.KilledMobs)
            {
                if (count > 0)
                {
                    int banners = (int)Math.Floor((double)count / 50f);
                    // sb.WriteProperty(index.ToString(), count.ToString());
                    sb.WriteLine("[{0}] {1} - {2} ({3} earned)", index, World.TallyNames[index], count, banners);
                    killcount = killcount + count;
                    if (banners > 0)
                        uniquecount = uniquecount + 1;
                    bannercount = bannercount + banners;
                }
                index++;
            }
            sb.Write(Environment.NewLine);
            sb.WriteLine("=== No Kills ===");
            index = 0;
            foreach (int count in world.KilledMobs)
            {
                if (count == 0)
                {
                    if (index > 0 && index <= World.TallyNames.Count)
                        sb.WriteLine("[{0}] {1}", index, World.TallyNames[index]);
                }
                index++;
            }
            sb.Write(Environment.NewLine);
            sb.WriteLine("Total kills counted: {0}", killcount);
            sb.WriteLine("Total banners awarded: {0}", bannercount);
            sb.WriteLine("Total unique banners: {0}", uniquecount);

        }

    }
}
