using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using TEdit.Configuration;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Scripting.Api;

public class ToolsApi
{
    private readonly WorldViewModel _wvm;

    public ToolsApi(WorldViewModel wvm)
    {
        _wvm = wvm;
    }

    public List<string> ListTools()
    {
        return _wvm.Tools.Select(t => t.Name).ToList();
    }

    public void CopySelection()
    {
        _wvm.EditCopy();
    }

    public int GetTilePickerTile() => _wvm.TilePicker.Tile;
    public int GetTilePickerWall() => _wvm.TilePicker.Wall;

    // ── File Path ─────────────────────────────────────────────────
    public string GetFilePath() => _wvm.CurrentFile ?? "";
    public void SetFilePath(string path) => _wvm.CurrentFile = path;

    // ── World Folders ─────────────────────────────────────────────

    /// <summary>Returns the local Terraria worlds folder (Documents\My Games\Terraria\Worlds).</summary>
    public string GetWorldsFolder() => DependencyChecker.PathToWorlds ?? "";

    /// <summary>Returns all Steam Cloud world folder paths as [{userId, path}].</summary>
    public List<Dictionary<string, string>> GetCloudWorldsFolders()
    {
        return DependencyChecker.GetAllSteamCloudWorldPaths()
            .Select(p => new Dictionary<string, string>
            {
                ["userId"] = p.UserId,
                ["path"] = p.WorldsPath,
            })
            .ToList();
    }

    // ── Save ──────────────────────────────────────────────────────

    /// <summary>Save to the current file path (no UI dialog).</summary>
    public bool Save()
    {
        if (_wvm.CurrentWorld == null || string.IsNullOrWhiteSpace(_wvm.CurrentFile))
            return false;

        return SaveToFile(_wvm.CurrentFile, 0);
    }

    /// <summary>
    /// Save to a specific file path (no UI dialog).
    /// If filename is just a name (no directory separator), saves to the default worlds folder.
    /// </summary>
    public bool SaveAs(string filename)
    {
        return SaveAs(filename, 0);
    }

    /// <summary>
    /// Save to a specific file path with a version override (no UI dialog).
    /// If filename is just a name (no directory separator), saves to the default worlds folder.
    /// </summary>
    public bool SaveAs(string filename, int version)
    {
        if (_wvm.CurrentWorld == null || string.IsNullOrWhiteSpace(filename))
            return false;

        filename = ResolveWorldPath(filename);
        _wvm.CurrentFile = filename;
        return SaveToFile(filename, version);
    }

    // ── Load ──────────────────────────────────────────────────────

    /// <summary>
    /// Load a world file, replacing the current world. Blocks until complete.
    /// If filename is just a name (no directory separator), loads from the default worlds folder.
    /// </summary>
    public bool Load(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        filename = ResolveWorldPath(filename);

        if (!File.Exists(filename))
            return false;

        try
        {
            var (world, error) = World.LoadWorld(filename);
            if (world == null)
                return false;

            // Must set CurrentWorld on UI thread since it triggers rendering
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _wvm.CurrentWorld = world;
                _wvm.CurrentFile = filename;
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    // ── Internals ─────────────────────────────────────────────────

    /// <summary>
    /// If filename has no directory component (just "MyWorld.wld"),
    /// prepends the default worlds folder. Also ensures .wld extension.
    /// </summary>
    private static string ResolveWorldPath(string filename)
    {
        // Ensure .wld extension
        if (!filename.EndsWith(".wld", StringComparison.OrdinalIgnoreCase))
            filename += ".wld";

        // If it's just a filename with no path separators, use default worlds folder
        if (filename.IndexOf(Path.DirectorySeparatorChar) < 0 &&
            filename.IndexOf(Path.AltDirectorySeparatorChar) < 0)
        {
            string worldsDir = DependencyChecker.PathToWorlds;
            if (!string.IsNullOrEmpty(worldsDir))
                filename = Path.Combine(worldsDir, filename);
        }

        return filename;
    }

    private bool SaveToFile(string filename, int version)
    {
        try
        {
            WorldConfiguration.Initialize();
            uint max = WorldConfiguration.CompatibleVersion;
            uint v = version > 0 ? (uint)version : max;
            if (v > max) v = max;

            World.SaveAsync(
                _wvm.CurrentWorld,
                filename,
                versionOverride: (int)v,
                progress: new Progress<ProgressChangedEventArgs>(_ => { })
            ).GetAwaiter().GetResult();

            return true;
        }
        catch
        {
            return false;
        }
    }
}
