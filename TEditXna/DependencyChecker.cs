using System;
using System.IO;
using System.Windows.Forms;

namespace TEditXna
{
    public static class DependencyChecker
    {
        public const string REGISTRY_DOTNET = @"SOFTWARE\Microsoft\.NETFramework\policy\v4.0";
        public const string REGISTRY_XNA = @"Software\Microsoft\XNA\Framework\v4.0";
        public static string PathToContent;

        static DependencyChecker()
        {
            Properties.Settings.Default.Reload();
            // find steam
            string path = Properties.Settings.Default.TerrariaPath;


            if (string.IsNullOrWhiteSpace(path))
            {
                // try with dionadar's fix
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 105600"))
                {
                    if (key != null)
                        path = Path.Combine((string)key.GetValue("InstallLocation"), "Content");
                }
            }

            // if that fails, try steam path
            if (string.IsNullOrWhiteSpace(path))
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\\Valve\\Steam"))
                {
                    if (key != null)
                        path = key.GetValue("SteamPath") as string;
                }

                //no steam key, let's try steam in program files
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                    path = Path.Combine(path, "Steam");
                }

                path = Path.Combine(path, "steamapps", "common", "terraria", "Content");
            }

            // ug...we still don't have a path. Prompt the user.
            if (!Directory.Exists(path))
            {
                path = BrowseForTerraria();

                bool retry = true;
                while (!DirectoryHasContentFolder(path) && retry)
                {
                    if (MessageBox.Show(
                        string.Format("Directory does not appear to contain Content.\r\nPress retry to pick a new folder or cancel to use \r\n{0}\r\n as your terraria path.", path),
                        "Terraria Content not Found",
                        MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    {
                        path = BrowseForTerraria();
                    }
                    else
                    {
                        retry = false;
                    }
                }
                Properties.Settings.Default.TerrariaPath = path;
                Properties.Settings.Default.Save();
            }

            if (!Directory.Exists(path))
            {
                path = TEditXNA.Terraria.World.AltC;
            }

            PathToContent = path;
        }

        public static bool DirectoryHasContentFolder(string path)
        {
            if (!Directory.Exists(Path.Combine(path, "Content")))
                return false;

            return true;
        }

        private static string BrowseForTerraria()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Locate Terraria Folder";
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                return fbd.SelectedPath;
            }

            return null;
        }

        public static bool VerifyDotNet()
        {
            Microsoft.Win32.RegistryKey subKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REGISTRY_DOTNET);
            bool dotNetExists = subKey != null;
            return dotNetExists;
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