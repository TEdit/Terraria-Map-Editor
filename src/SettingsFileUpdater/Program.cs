using System;
using System.IO;
using System.Reflection;

namespace SettingsFileUpdater
{
    static class Program
    {
        static void Main(string[] args)
        {
            RegisterAssemblyResolver();


            var wrapper = TerrariaHost.TerrariaWrapper.Initialize();

            Console.WriteLine(wrapper.GetTilesXml());
            Console.WriteLine(wrapper.GetWallsXml());
            Console.WriteLine(wrapper.GetItemsXml());
            Console.WriteLine(wrapper.GetNpcs());
            Console.WriteLine(wrapper.GetPrefixesXml());

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
