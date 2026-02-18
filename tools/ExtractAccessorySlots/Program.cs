using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// One-time extraction tool: reads accessory slot fields from Terraria.exe via reflection
/// and outputs a JSON mapping file for merging into items.json.
///
/// Copies the assembly resolver pattern from SettingsFileUpdater to load
/// embedded DLLs (ReLogic, etc.) from Terraria.exe.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        string outputPath = args.Length > 0 ? args[0] : "accessory-slots.json";

        // Register assembly resolver BEFORE loading Terraria types
        RegisterAssemblyResolver();

        var terrariaAsm = typeof(Terraria.Program).Assembly;

        // Load embedded DLLs
        foreach (var name in terrariaAsm.GetManifestResourceNames().Where(n => n.EndsWith(".dll")))
        {
            using (Stream stream = terrariaAsm.GetManifestResourceStream(name))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                Assembly.Load(bytes);
            }
        }

        // Set dedServ to avoid graphics initialization
        Terraria.Main.dedServ = true;

        // Get max item count
        int maxItems = Terraria.ID.ItemID.Count;
        Console.Error.WriteLine($"ItemID.Count = {maxItems}");

        // Slot field names and their corresponding JSON property names
        string[] fields = {
            "wingSlot", "backSlot", "balloonSlot", "shoeSlot", "waistSlot",
            "neckSlot", "faceSlot", "shieldSlot", "handOnSlot", "handOffSlot", "frontSlot"
        };
        string[] jsonNames = {
            "WingSlot", "BackSlot", "BalloonSlot", "ShoeSlot", "WaistSlot",
            "NeckSlot", "FaceSlot", "ShieldSlot", "HandOnSlot", "HandOffSlot", "FrontSlot"
        };

        var itemType = typeof(Terraria.Item);
        var fieldInfos = new FieldInfo[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            fieldInfos[i] = itemType.GetField(fields[i], BindingFlags.Public | BindingFlags.Instance);
            if (fieldInfos[i] == null)
                Console.Error.WriteLine($"Warning: field '{fields[i]}' not found");
        }

        Console.Error.WriteLine("Scanning items...");

        var sb = new StringBuilder();
        sb.AppendLine("{");
        bool first = true;
        int found = 0;
        int errors = 0;

        for (int id = 1; id < maxItems; id++)
        {
            try
            {
                var item = new Terraria.Item();
                item.SetDefaults(id);

                var slots = new List<(string name, int value)>();
                for (int f = 0; f < fieldInfos.Length; f++)
                {
                    if (fieldInfos[f] == null) continue;
                    var val = fieldInfos[f].GetValue(item);
                    int intVal = Convert.ToInt32(val);
                    if (intVal > 0)
                        slots.Add((jsonNames[f], intVal));
                }

                if (slots.Count > 0)
                {
                    if (!first) sb.AppendLine(",");
                    first = false;
                    sb.Append($"  \"{id}\": {{");
                    sb.Append(string.Join(", ", slots.Select(s => $"\"{s.name}\": {s.value}")));
                    sb.Append("}");
                    found++;
                }
            }
            catch (Exception ex)
            {
                errors++;
                if (errors <= 3)
                    Console.Error.WriteLine($"Item {id} failed: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("}");

        File.WriteAllText(outputPath, sb.ToString());
        Console.Error.WriteLine($"Extracted {found} items with accessory slots to {outputPath}");
        if (errors > 0)
            Console.Error.WriteLine($"{errors} items failed");
    }

    private static void RegisterAssemblyResolver()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
        {
            var terrariaAsm = typeof(Terraria.Program).Assembly;
            string dllName = new AssemblyName(resolveArgs.Name).Name + ".dll";
            string resourceName = terrariaAsm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(dllName));
            if (resourceName == null) return null;

            using (Stream stream = terrariaAsm.GetManifestResourceStream(resourceName))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return Assembly.Load(bytes);
            }
        };
    }
}
