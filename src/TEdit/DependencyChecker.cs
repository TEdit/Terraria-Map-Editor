using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Documents;
using System.Collections.Generic;

namespace TEdit
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
            int? steamUserId = TEdit.Terraria.World.SteamUserId;

            // if hard coded in settings.xml try that location first
            if (!string.IsNullOrWhiteSpace(TEdit.Terraria.World.AltC))
            {
                if (Directory.Exists(TEdit.Terraria.World.AltC))
                    path = TEdit.Terraria.World.AltC;
            }

            // if the folder is missing, reset.
            if (!Directory.Exists(path))
            {
                Properties.Settings.Default.TerrariaPath = null;
                Properties.Settings.Default.Save();
                path = string.Empty;
            }

            // SBLogic - attempt to find GOG version
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GOG.com\Games\1207665503\"))
                {
                    if (key != null)
                        path = Path.Combine((string)key.GetValue("PATH"), "Content");
                }
            }

            // find steam
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                // try with dionadar's fix
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 105600"))
                {
                    if (key != null)
                        path = Path.Combine((string)key.GetValue("InstallLocation"), "Content");
                }
            }

            // if that fails, try steam path
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
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

            // if that fails, try steam path - the long way
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Valve\\Steam"))
                {
                    if (key != null)
                    {
                        path = key.GetValue("InstallPath") as string;
                    }
                    else
                    {
                        using (RegistryKey key2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\WOW6432Node\\Valve\\Steam"))
                        {
                            if (key2 != null)
                                path = key2.GetValue("InstallPath") as string;
                        }
                    }


                    //no steam key, let's try steam in program files
                    if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                    {
                        var vdfFile = Path.Combine(path, "steamapps", "libraryfolders.vdf");

                        using (var file = File.Open(vdfFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (TextReader tr = new StreamReader(file))
                        {
                            var libraryPaths = new List<string>();
                            string line = null;
                            bool foundPath = false;
                            while ((line = tr.ReadLine()) != null && !foundPath)
                            {
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    var split = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var item in split)
                                    {
                                        var trimmed = item.Trim('\"').Replace("\\\\", "\\");
                                        if (Directory.Exists(trimmed))
                                        {

                                            var testpath = Path.Combine(trimmed, "steamapps", "common", "terraria", "Content");
                                            if (Directory.Exists(testpath))
                                            {
                                                path = testpath;
                                                foundPath = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }




            }

            // ug...we still don't have a path. Prompt the user.
            if (!Directory.Exists(path))
            {
                string tempPath = BrowseForTerraria();

                bool retry = true;
                while (!DirectoryHasContentFolder(tempPath) && retry)
                {
                    if (MessageBox.Show(
                        string.Format(Properties.Language.dialog_error_terriaria_folder_message, path),
                        Properties.Language.dialog_error_terriaria_folder_title,
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

        public static string HKLM_GetString(string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(path);
                if (rk == null) return "";
                return (string)rk.GetValue(key);
            }
            catch { return ""; }
        }

        public static string GetOsVersion()
        {
            string ProductName = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");
            string CSDVersion = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CSDVersion");
            if (ProductName != "")
            {
                return (ProductName.StartsWith("Microsoft") ? "" : "Microsoft ") + ProductName +
                            (CSDVersion != "" ? " " + CSDVersion : "");
            }
            return "";
        }

        internal static string GetDotNetVersion()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    return $".NET Framework Version: {CheckFor45PlusVersion((int)ndpKey.GetValue("Release"))}";
                }
                else
                {
                    return ".NET Framework Version 4.5 or later is not detected.";
                }
            }

            // Checking the version using >= enables forward compatibility.
            string CheckFor45PlusVersion(int releaseKey)
            {
                if (releaseKey >= 528040)
                    return "4.8 or later";
                if (releaseKey >= 461808)
                    return "4.7.2";
                if (releaseKey >= 461308)
                    return "4.7.1";
                if (releaseKey >= 460798)
                    return "4.7";
                if (releaseKey >= 394802)
                    return "4.6.2";
                if (releaseKey >= 394254)
                    return "4.6.1";
                if (releaseKey >= 393295)
                    return "4.6";
                if (releaseKey >= 379893)
                    return "4.5.2";
                if (releaseKey >= 378675)
                    return "4.5.1";
                if (releaseKey >= 378389)
                    return "4.5";
                // This code should never execute. A non-null release key should mean
                // that 4.5 or later is installed.
                return "No 4.5 or later version detected";
            }
        }

        public static int GetDirectXMajorVersion()
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
