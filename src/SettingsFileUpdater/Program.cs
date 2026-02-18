using SettingsFileUpdater;
using SettingsFileUpdater.TerrariaHost;
using SettingsFileUpdater.TerrariaHost.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
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

            Thread.Sleep(5 * 1000);

            string exeDir = AppDomain.CurrentDomain.BaseDirectory;

            // JSON output directory (relative to SettingsFileUpdater project)
            // Path: bin/Debug/net48 -> bin/Debug -> bin -> SettingsFileUpdater -> src/TEdit.Terraria/Data
            string jsonOutputDir = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\TEdit.Terraria\Data"));
            Directory.CreateDirectory(jsonOutputDir);

            // Use TEditJsonSerializer.DefaultOptions for consistent formatting
            var jsonOptions = TEditJsonSerializer.DefaultOptions;

            // Check for --items-only flag to only extract and merge items
            bool itemsOnly = args.Any(a => a.Equals("--items-only", StringComparison.OrdinalIgnoreCase));

            // Extract data from Terraria
            Console.WriteLine("Extracting data from Terraria...");
            var items = wrapper.GetItemsJson();

            if (!itemsOnly)
            {
                var tiles = wrapper.GetTilesJson();
                var walls = wrapper.GetWallsJson();
                var npcs = wrapper.GetNpcsJson();
                var prefixes = wrapper.GetPrefixesJson();
                var paints = wrapper.GetPaintsJson();
                var globalColors = wrapper.GetGlobalColorsJson();
                var bestiaryConfig = wrapper.GetBestiaryConfigJson();
                var versionData = wrapper.GetVersionData();
                Console.WriteLine("  Extraction complete.");

                // Write raw extractor output to .generated/ for review
                string generatedDir = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\SettingsFileUpdater\.generated"));
                Directory.CreateDirectory(generatedDir);
                Console.WriteLine($"\nWriting raw extractor output to: {generatedDir}");
                WriteJson(Path.Combine(generatedDir, "tiles.json"), tiles, jsonOptions);
                WriteJson(Path.Combine(generatedDir, "walls.json"), walls, jsonOptions);
                WriteJson(Path.Combine(generatedDir, "items.json"), items, jsonOptions);
                WriteJson(Path.Combine(generatedDir, "npcs.json"), npcs, jsonOptions);
                WriteJson(Path.Combine(generatedDir, "prefixes.json"), prefixes, jsonOptions);
                WriteJson(Path.Combine(generatedDir, "paints.json"), paints, jsonOptions);
                WriteJson(Path.Combine(generatedDir, "globalColors.json"), globalColors, jsonOptions);
                WriteJson(Path.Combine(generatedDir, "bestiaryNpcs.json"), bestiaryConfig, jsonOptions);
                WriteJson(Path.Combine(generatedDir, "versionData.json"), versionData, jsonOptions);
                Console.WriteLine("  Raw output written.");

                // Merge into existing JSON files (append-only, preserves hand-curated data)
                Console.WriteLine($"\nMerging into: {jsonOutputDir}");
                var results = new Dictionary<string, MergeResult>();

                Console.WriteLine("  Merging tiles.json...");
                results["Tiles"] = JsonMerger.MergeById(
                    Path.Combine(jsonOutputDir, "tiles.json"), tiles, t => t.Id, jsonOptions);

                Console.WriteLine("  Merging walls.json...");
                results["Walls"] = JsonMerger.MergeById(
                    Path.Combine(jsonOutputDir, "walls.json"), walls, w => w.Id, jsonOptions);

                Console.WriteLine("  Merging items.json (with property updates)...");
                results["Items"] = JsonMerger.MergeByIdWithPropertyUpdate(
                    Path.Combine(jsonOutputDir, "items.json"), items, i => i.Id, jsonOptions);

                Console.WriteLine("  Merging npcs.json...");
                results["NPCs"] = JsonMerger.MergeById(
                    Path.Combine(jsonOutputDir, "npcs.json"), npcs, n => n.Id, jsonOptions);

                Console.WriteLine("  Merging prefixes.json...");
                results["Prefixes"] = JsonMerger.MergeById(
                    Path.Combine(jsonOutputDir, "prefixes.json"), prefixes, p => p.Id, jsonOptions);

                Console.WriteLine("  Merging paints.json...");
                results["Paints"] = JsonMerger.MergeById(
                    Path.Combine(jsonOutputDir, "paints.json"), paints, p => p.Id, jsonOptions);

                Console.WriteLine("  Merging globalColors.json...");
                results["GlobalColors"] = JsonMerger.MergeByName(
                    Path.Combine(jsonOutputDir, "globalColors.json"), globalColors, c => c.Name, jsonOptions);

                Console.WriteLine("  Merging bestiaryNpcs.json...");
                results["Bestiary"] = JsonMerger.MergeBestiary(
                    Path.Combine(jsonOutputDir, "bestiaryNpcs.json"),
                    bestiaryConfig,
                    bestiaryConfig.NpcData.Cast<object>().ToList(),
                    o => ((NpcData)o).Id,
                    jsonOptions);

                // Auto-update version data
                Console.WriteLine("\nChecking version data...");

                string versionsJsonPath = Path.Combine(jsonOutputDir, "versions.json");
                string docsVersionPath = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\..\..\docs\TerrariaVersionTileData.json"));

                bool versionsUpdated = JsonMerger.MergeVersionData(
                    versionsJsonPath, versionData, jsonOptions);
                Console.WriteLine(versionsUpdated
                    ? $"  Added save version {versionData.SaveVersion} ({versionData.GameVersion}) to versions.json"
                    : $"  versions.json already has save version {versionData.SaveVersion}");

                if (File.Exists(docsVersionPath))
                {
                    bool docsUpdated = JsonMerger.MergeVersionData(
                        docsVersionPath, versionData, jsonOptions);
                    Console.WriteLine(docsUpdated
                        ? $"  Added save version {versionData.SaveVersion} to docs/TerrariaVersionTileData.json"
                        : $"  docs/TerrariaVersionTileData.json already has save version {versionData.SaveVersion}");
                }

                // Write MapColorsUpdated.xml next to the exe.
                string outPath = Path.Combine(savePath, "MapColorsUpdated.xml");
                string originalPath = Path.Combine(exeDir, "MapColors.xml");
                if (!File.Exists(originalPath))
                {
                    originalPath = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\TEdit\MapColors.xml"));
                }
                wrapper.WriteMapColorsXml(outPath, File.Exists(originalPath) ? originalPath : null);
                Console.WriteLine($"Wrote: {outPath}");

                // Print merge summary
                Console.WriteLine("\n=== MERGE SUMMARY ===");
                foreach (var kvp in results)
                {
                    var r = kvp.Value;
                    Console.WriteLine($"  {kvp.Key,-14} {r}");
                }
                Console.WriteLine($"  {"Version",-14} save={versionData.SaveVersion} game={versionData.GameVersion}");
            }
            else
            {
                Console.WriteLine("  Extraction complete (items only).");

                // Write raw items output to .generated/ for review
                string generatedDir = Path.GetFullPath(Path.Combine(exeDir, @"..\..\..\..\SettingsFileUpdater\.generated"));
                Directory.CreateDirectory(generatedDir);
                WriteJson(Path.Combine(generatedDir, "items.json"), items, jsonOptions);
                Console.WriteLine($"  Wrote raw items to: {generatedDir}");

                // Merge items only
                Console.WriteLine($"\nMerging items.json into: {jsonOutputDir}");
                var result = JsonMerger.MergeByIdWithPropertyUpdate(
                    Path.Combine(jsonOutputDir, "items.json"), items, i => i.Id, jsonOptions);

                Console.WriteLine($"\n=== MERGE SUMMARY ===");
                Console.WriteLine($"  {"Items",-14} {result}");
            }

            // Proper shutdown
            Console.WriteLine("\nMerge complete. Shutting down...");
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

        private static void WriteJson<T>(string path, T data, JsonSerializerOptions options)
        {
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(stream, data, options);
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

    }
}
