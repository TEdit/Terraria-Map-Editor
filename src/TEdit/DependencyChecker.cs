using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;

namespace TEditXna
{
    public static class DependencyChecker
    {
        public const string REGISTRY_DOTNET = @"SOFTWARE\Microsoft\.NETFramework\policy\v4.0";
        public const string REGISTRY_XNA = @"Software\Microsoft\XNA\Framework\v4.0";
        public static string PathToContent;
        public static string PathToWorlds;

        public static void CheckPaths()
        {
            Properties.Settings.Default.Reload();

            string path = Properties.Settings.Default.TerrariaPath;
            int? steamUserId = TEditXNA.Terraria.World.SteamUserId;

            // if hard coded in settings.xml try that location first
            if (!string.IsNullOrWhiteSpace(TEditXNA.Terraria.World.AltC))
            {
                if (Directory.Exists(TEditXNA.Terraria.World.AltC))
                    path = TEditXNA.Terraria.World.AltC;
            }

            // if the folder is missing, reset.
            if (!Directory.Exists(path))
            {
                Properties.Settings.Default.TerrariaPath = null;
                Properties.Settings.Default.Save();
                path = string.Empty;
            }

            // SBLogic - attempt to find GOG version
            if (string.IsNullOrWhiteSpace(path))
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GOG.com\Games\1207665503\"))
                {
                    if (key != null)
                        path = Path.Combine((string)key.GetValue("PATH"), "Content");
                }
            }

            // find steam
            if (string.IsNullOrWhiteSpace(path))
            {
                // try with dionadar's fix
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 105600"))
                {
                    if (key != null)
                        path = Path.Combine((string)key.GetValue("InstallLocation"), "Content");
                }
            }

            // if that fails, try steam path
            if (string.IsNullOrWhiteSpace(path))
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\\Valve\\Steam"))
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
                string tempPath = BrowseForTerraria();

                bool retry = true;
                while (!DirectoryHasContentFolder(tempPath) && retry)
                {
                    if (MessageBox.Show(
                            $"Directory does not appear to contain Content.\r\nPress retry to pick a new folder or cancel to use \r\n{path}\r\n as your terraria path.",
                        "Terraria Content not Found",
                        MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    {
                        tempPath = BrowseForTerraria();
                    }
                    else
                    {
                        retry = false;
                    }
                }
                Properties.Settings.Default.TerrariaPath = Path.Combine(tempPath, "Content");
                Properties.Settings.Default.Save();
            }

            if (!string.IsNullOrWhiteSpace(path) && path.IndexOf("Content", StringComparison.OrdinalIgnoreCase) < 0)
            {
                path = Path.Combine(path, "Content");
            }

            path = Path.GetFullPath(path);
            PathToContent = path;
            PathToWorlds = GetPathToWorlds(steamUserId);
        }

        /**
         *  TODO:  Update this to work with OS X
         */
        private static string GetPathToWorlds(int? steamUserId)
        {
            //  Are we editing Steam Cloud worlds?
            if (steamUserId != null)
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\\Valve\\Steam"))
                {
                    if (key != null)
                    {
                        string steamWorlds = Path.Combine(key.GetValue("SteamPath") as string, "userdata");

                        //  No Steam UserID was specified; we'll guess it if there's only a single user
                        if (steamUserId == 0)
                        {
                            string[] userDirectories = Directory.GetDirectories(steamWorlds);

                            if (userDirectories.Length == 1)
                            {
                                steamUserId = Convert.ToInt32(Path.GetFileName(userDirectories[0]));
                            }
                        }

                        steamWorlds = Path.Combine(steamWorlds, steamUserId.ToString(), "105600", "remote", "worlds").Replace("/", "\\");

                        if (Directory.Exists(steamWorlds))
                        {
                            return Path.GetFullPath(steamWorlds);
                        }
                    }
                }
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Worlds");
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

        public static int GetDirectxMajorVersion()
        {
            int directxMajorVersion = 0;

            var OSVersion = Environment.OSVersion;

            // if Windows Vista or later
            if (OSVersion.Version.Major >= 6)
            {
                // if Windows 7 or later
                if (OSVersion.Version.Major > 6 || OSVersion.Version.Minor >= 1)
                {
                    directxMajorVersion = 11;
                }
                // if Windows Vista
                else
                {
                    directxMajorVersion = 10;
                }
            }
            // if Windows XP or earlier.
            else
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DirectX"))
                {
                    string versionStr = key.GetValue("Version") as string;
                    if (!string.IsNullOrEmpty(versionStr))
                    {
                        var versionComponents = versionStr.Split('.');
                        if (versionComponents.Length > 1)
                        {
                            int directXLevel;
                            if (int.TryParse(versionComponents[1], out directXLevel))
                            {
                                directxMajorVersion = directXLevel;
                            }
                        }
                    }
                }
            }

            return directxMajorVersion;
        }

        public static bool VerifyTerraria()
        {
            return Directory.Exists(PathToContent);
        }

        public static string GetTerrariaVersion()
        {
            string version = null;
            try
            {
                string terrariaExePath = Directory.GetParent(PathToContent).FullName + "\\Terraria.exe";
                version = FileVersionInfo.GetVersionInfo(terrariaExePath).FileVersion;
            }
            catch { /*no message*/}
            return version;
        }

    }
}
