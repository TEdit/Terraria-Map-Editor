using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SettingsFileUpdater
{
    static class Program
    {
        static void Main(string[] args)
        {
            //LoadTerrariaAsm();
            RegisterAssemblyResolver();


            LoadTerrariaAsm();
            Thread.CurrentThread.Name = "Main Thread";
            Terraria.Program.SavePath = Assembly.GetExecutingAssembly().Location;
            var wrapper = TerrariaHost.TerrariaWrapper.Initialize();


            // XDocument xdoc = XDocument.Load("settings.xml");
            // 
            // var tiles = xdoc.Root.Element("Tiles");
            // foreach (var tile in tiles.Elements())
            // {
            //     int id = int.Parse(tile.Attribute("Id").Value);
            //     if (Terraria.ID.TileID.Sets.NonSolidSaveSlopes[id])
            //     {
            //         tile.SetAttributeValue("SaveSlope", "true");
            //     }
            //     if (Terraria.Main.tileSolid[id])
            //     {
            //         tile.SetAttributeValue("Solid", "true");
            //     }
            //     if (Terraria.Main.tileSolidTop[id])
            //     {
            //         tile.SetAttributeValue("SolidTop", "true");
            //     }
            // }
            // xdoc.Save("settings3.xml");


            //foreach (var item in tilecolors)
            //{
            //    if (item.Key < 470) continue;
            //    var tile = tiles.Elements().FirstOrDefault(t => int.Parse(t.Attribute("Id").Value) == item.Key);
            //    tile.SetAttributeValue("Color", item.Value);
            //}

            // var walls = xdoc.Root.Element("Walls");
            // foreach (var item in wallcolors)
            // {
            //     var wall = tiles.Elements().FirstOrDefault(t => int.Parse(t.Attribute("Id").Value) == item.Key);
            //     wall.SetAttributeValue("Color", item.Value);
            // }

            var bestiaryNpcs = wrapper.GetBestiaryData().ToList();



            Console.WriteLine(wrapper.GetMaxCounts());

            //Console.WriteLine(wrapper.GetTilesXml());
            //Console.WriteLine(wrapper.GetWallsXml());
            Console.WriteLine(wrapper.GetItemsXml());
            Console.WriteLine(wrapper.GetMobsText());
            Console.WriteLine(wrapper.GetNpcsXml());
            Console.WriteLine(wrapper.GetPrefixesXml());
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
    }
}
