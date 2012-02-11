using System;
using System.IO;

namespace TEditXna
{
    public static class DependencyChecker
    {
        public const string REGISTRY_DOTNET = @"SOFTWARE\Microsoft\.NETFramework\policy\v4.0";
        public const string REGISTRY_XNA = @"Software\Microsoft\XNA\Framework\v4.0";
        public static string PathToContent;

        static DependencyChecker()
        {
            // find steam
            string path = "";
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\\Valve\\Steam");
            if (key != null)
                path = key.GetValue("SteamPath") as string;

            //no steam key, let's try the default
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                path = Path.Combine(path, "Steam");
            }
            path = Path.Combine(path, "steamapps", "common", "terraria", "Content");
            PathToContent = path;
        }

        public static bool VerifyDotNet()
        {
            Microsoft.Win32.RegistryKey subKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REGISTRY_DOTNET);
            return subKey != null;
        }

        public static bool VerifyXna()
        {
            Microsoft.Win32.RegistryKey subKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REGISTRY_XNA);
            if (subKey != null)
            {
                int i = (int)subKey.GetValue("Installed");
                if (i == 1)
                    return true;
            }
            return false;
        }

        public static bool VerifyTerraria()
        {
            return Directory.Exists(PathToContent);
        }

    }
}