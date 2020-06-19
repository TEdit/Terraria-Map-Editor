using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace SettingsFileUpdater
{
    static class Program
    {
        static void Main(string[] args)
        {
            RegisterAssemblyResolver();


            var wrapper = TerrariaHost.TerrariaWrapper.Initialize();
            (var tilecolors, var wallcolors) = wrapper.GetMapTileColors();

            XDocument xdoc = XDocument.Load("settings.xml");
            XDocument xdoc2 = XDocument.Load("settings2.xml");

            var tiles = xdoc.Root.Element("Tiles");

            foreach (var item in tilecolors)
            {
                var tile = tiles.Elements().FirstOrDefault(t => int.Parse(t.Attribute("Id").Value) == item.Key);
                tile.SetAttributeValue("Color", item.Value);
            }
            
            var walls = xdoc.Root.Element("Walls");
            foreach (var item in wallcolors)
            {
                var wall = tiles.Elements().FirstOrDefault(t => int.Parse(t.Attribute("Id").Value) == item.Key);
                wall.SetAttributeValue("Color", item.Value);
            }


            var tiles2 = xdoc2.Root.Element("Tiles");
            for (int i = 0; i < 473; i++)
            {
                var tile = tiles.Elements().FirstOrDefault(t => int.Parse(t.Attribute("Id").Value) == i);
                var tile2 = tiles2.Elements().FirstOrDefault(t => int.Parse(t.Attribute("Id").Value) == i);
                tile.SetAttributeValue("Color", tile2.Attribute("Color").Value);

            }


            xdoc.Save("settings3.xml");

            //Console.WriteLine(wrapper.GetTilesXml());
            //Console.WriteLine(wrapper.GetWallsXml());
            Console.WriteLine(wrapper.GetItemsXml());
            //Console.WriteLine(wrapper.GetNpcsXml());
            //Console.WriteLine(wrapper.GetPrefixesXml());

            Console.ReadLine();
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
