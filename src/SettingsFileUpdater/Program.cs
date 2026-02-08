using SettingsFileUpdater.TerrariaHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TEdit.Common.Serialization;
using Terraria;
using Terraria.Social;

namespace SettingsFileUpdater
{
    static class Program
    {
        private static bool ConsoleCtrlCheck(WindowsLaunch.CtrlTypes ctrlType)
        {
            bool flag = false;
            switch (ctrlType)
            {
                case WindowsLaunch.CtrlTypes.CTRL_C_EVENT:
                    flag = true;
                    break;
                case WindowsLaunch.CtrlTypes.CTRL_BREAK_EVENT:
                    flag = true;
                    break;
                case WindowsLaunch.CtrlTypes.CTRL_CLOSE_EVENT:
                    flag = true;
                    break;
                case WindowsLaunch.CtrlTypes.CTRL_LOGOFF_EVENT:
                case WindowsLaunch.CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    flag = true;
                    break;
            }
            if (flag)
                SocialAPI.Shutdown();
            return true;
        }

        private static WindowsLaunch.HandlerRoutine _handleRoutine;

        static void Main(string[] args)
        {
            //LoadTerrariaAsm();
            RegisterAssemblyResolver();

            _handleRoutine = new WindowsLaunch.HandlerRoutine(ConsoleCtrlCheck);
            WindowsLaunch.SetConsoleCtrlHandler(_handleRoutine, true);

            LoadTerrariaAsm();
            Thread.CurrentThread.Name = "Main Thread";
            var savePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Terraria.Program.SavePath = savePath;
            Terraria.Program.LaunchParameters = new Dictionary<string, string>
            {
                { "-config", "serverconfig.txt" },
                { "-world", Path.Combine(savePath, "Worlds", "1234.wld") }
            };

            var wrapper = TerrariaHost.TerrariaWrapper.Initialize();


            //XDocument xdoc = XDocument.Load("settings.xml");

            //var tiles = xdoc.Root.Element("Tiles");
            //foreach (var tile in tiles.Elements())
            //{
            //    int id = int.Parse(tile.Attribute("Id").Value);
            //    if (Terraria.ID.TileID.Sets.NonSolidSaveSlopes[id])
            //    {
            //        tile.SetAttributeValue("SaveSlope", "true");
            //    }
            //    if (Terraria.Main.tileSolid[id])
            //    {
            //        tile.SetAttributeValue("Solid", "true");
            //    }
            //    if (Terraria.Main.tileSolidTop[id])
            //    {
            //        tile.SetAttributeValue("SolidTop", "true");
            //    }
            //}
            //xdoc.Save("settings3.xml");

            //            foreach (var item in tilecolors)
            //            {
            //                if (item.Key < 470) continue;
            //                var tile = tiles.Elements().FirstOrDefault(t => int.Parse(t.Attribute("Id").Value) == item.Key);
            //                tile.SetAttributeValue("Color", item.Value);
            //            }

            //            var walls = xdoc.Root.Element("Walls");
            //            foreach (var item in wallcolors)
            //            {
            //                var wall = tiles.Elements().FirstOrDefault(t => int.Parse(t.Attribute("Id").Value) == item.Key);
            //                wall.SetAttributeValue("Color", item.Value);
            //            }
            //            //return;
            Thread.Sleep(5 * 1000);

            string exeDir = AppDomain.CurrentDomain.BaseDirectory;

            // JSON output directory (relative to SettingsFileUpdater project)
            string jsonOutputDir = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\TEdit.Terraria\Data"));
            Directory.CreateDirectory(jsonOutputDir);

            Console.WriteLine($"Writing JSON files to: {jsonOutputDir}");

            // Use TEditJsonSerializer.DefaultOptions for consistent formatting
            var jsonOptions = TEditJsonSerializer.DefaultOptions;

            // Write tiles.json
            Console.WriteLine("Generating tiles.json...");
            var tiles = wrapper.GetTilesJson();
            WriteJson(Path.Combine(jsonOutputDir, "tiles.json"), tiles, jsonOptions);
            Console.WriteLine($"  Wrote {tiles.Count} tiles");

            // Write walls.json
            Console.WriteLine("Generating walls.json...");
            var walls = wrapper.GetWallsJson();
            WriteJson(Path.Combine(jsonOutputDir, "walls.json"), walls, jsonOptions);
            Console.WriteLine($"  Wrote {walls.Count} walls");

            // Write items.json
            Console.WriteLine("Generating items.json...");
            var items = wrapper.GetItemsJson();
            WriteJson(Path.Combine(jsonOutputDir, "items.json"), items, jsonOptions);
            Console.WriteLine($"  Wrote {items.Count} items");

            // Write npcs.json (friendly NPCs)
            Console.WriteLine("Generating npcs.json...");
            var npcs = wrapper.GetNpcsJson();
            WriteJson(Path.Combine(jsonOutputDir, "npcs.json"), npcs, jsonOptions);
            Console.WriteLine($"  Wrote {npcs.Count} NPCs");

            // Write prefixes.json
            Console.WriteLine("Generating prefixes.json...");
            var prefixes = wrapper.GetPrefixesJson();
            WriteJson(Path.Combine(jsonOutputDir, "prefixes.json"), prefixes, jsonOptions);
            Console.WriteLine($"  Wrote {prefixes.Count} prefixes");

            // Write paints.json
            Console.WriteLine("Generating paints.json...");
            var paints = wrapper.GetPaintsJson();
            WriteJson(Path.Combine(jsonOutputDir, "paints.json"), paints, jsonOptions);
            Console.WriteLine($"  Wrote {paints.Count} paints");

            // Write globalColors.json
            Console.WriteLine("Generating globalColors.json...");
            var globalColors = wrapper.GetGlobalColorsJson();
            WriteJson(Path.Combine(jsonOutputDir, "globalColors.json"), globalColors, jsonOptions);
            Console.WriteLine($"  Wrote {globalColors.Count} global colors");

            // Write bestiaryNpcs.json
            Console.WriteLine("Generating bestiaryNpcs.json...");
            var bestiaryConfig = wrapper.GetBestiaryConfigJson();
            WriteJson(Path.Combine(jsonOutputDir, "bestiaryNpcs.json"), bestiaryConfig, jsonOptions);
            Console.WriteLine($"  Wrote {bestiaryConfig.NpcData.Count} bestiary NPCs");

            // Output version info for manual update to versions.json
            Console.WriteLine("\nVersion info for versions.json:");
            Console.WriteLine(wrapper.GetMaxCounts());

            // Legacy XML output (for reference)
            Console.WriteLine("\n--- Legacy XML Output (for reference) ---");
            Console.WriteLine(wrapper.GetItemsXml());
            Console.WriteLine(wrapper.GetMobsText());
            Console.WriteLine(wrapper.GetNpcsXml());
            Console.WriteLine(wrapper.GetPrefixesXml());
            Console.WriteLine(wrapper.GetWallsXml());

            // Write MapColorsUpdated.xml next to the exe.
            string outPath = Path.Combine(savePath, "MapColorsUpdated.xml");

            // Optional override file (if you have one).
            // Prefer MapColors.xml next to the exe; fallback to repo-relative ..\..\..\..\TEdit\MapColors.xml.
            string originalPath = Path.Combine(exeDir, "MapColors.xml");
            if (!File.Exists(originalPath))
            {
                originalPath = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\TEdit\MapColors.xml"));
            }

            wrapper.WriteMapColorsXml(outPath, File.Exists(originalPath) ? originalPath : null);
            Console.WriteLine("Wrote: " + outPath);

            // Proper shutdown
            Console.WriteLine("\nJSON generation complete. Shutting down...");
            SocialAPI.Shutdown();
            Environment.Exit(0);
        }

        private static void LoadTerrariaAsm()
        {
            var TerrariaAsm = typeof(Terraria.Program).Assembly;
            Assembly assembly;
            foreach (var name in TerrariaAsm.GetManifestResourceNames().Where(n => n.EndsWith(".dll")))
            {
                using (Stream manifestResourceStream = TerrariaAsm.GetManifestResourceStream(name))
                {
                    byte[] numArray = new byte[manifestResourceStream.Length];
                    manifestResourceStream.Read(numArray, 0, (int)numArray.Length);
                    assembly = Assembly.Load(numArray);
                }
            }

            var path = Path.GetDirectoryName(TerrariaAsm.Location);

        }

        private static void RegisterAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler((object sender, ResolveEventArgs sargs) =>
            {
                var TerrariaAsm = typeof(Terraria.Program).Assembly;
                Assembly assembly;
                string str = string.Concat((new AssemblyName(sargs.Name)).Name, ".dll");
                string str1 = Array.Find<string>(TerrariaAsm.GetManifestResourceNames(), (string element) => element.EndsWith(str));
                if (str1 == null)
                {
                    return null;
                }
                using (Stream manifestResourceStream = TerrariaAsm.GetManifestResourceStream(str1))
                {
                    byte[] numArray = new byte[manifestResourceStream.Length];
                    manifestResourceStream.Read(numArray, 0, (int)numArray.Length);
                    assembly = Assembly.Load(numArray);
                }


                return assembly;
            });
        }

        /// <summary>
        /// Writes data to a JSON file using the specified options.
        /// </summary>
        private static void WriteJson<T>(string path, T data, JsonSerializerOptions options)
        {
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(stream, data, options);
        }
    }
}
