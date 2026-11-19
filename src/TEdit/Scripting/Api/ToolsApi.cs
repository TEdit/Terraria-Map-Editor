using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using TEdit.Configuration;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Scripting.Api;

public class ToolsApi
{
    private readonly WorldViewModel _wvm;
    private readonly Func<bool>? _canLoad;
    private readonly Action? _loaded;
    private readonly CancellationToken _cancellation;

    public ToolsApi(WorldViewModel wvm, Func<bool>? canLoad = null, Action? loaded = null, CancellationToken cancellation = default)
    {
        _wvm = wvm;
        _canLoad = canLoad;
        _loaded = loaded;
        _cancellation = cancellation;
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

    /// <summary>Returns the Steam Cloud Terraria worlds folder.</summary>
    public string GetSteamWorldsFolder() => DependencyChecker.PathToSteamWorlds ?? "";

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
        return SaveToFile(filename, version);
    }

    // ── Load ──────────────────────────────────────────────────────

    /// <summary>
    /// Load a world file, replacing the current world. Blocks until complete.
    /// If filename is just a name (no directory separator), loads from the default worlds folder.
    /// </summary>
    public bool Load(string filename)
    {
        // Replacing a world inside a batch callback would invalidate the outer loop.
        if (_canLoad?.Invoke() == false) return false;
        // Script host methods run on the worker. Never block the UI on its own
        // asynchronous undo/render completion.
        if (System.Windows.Application.Current?.Dispatcher.CheckAccess() != false) return false;
        using var operation = _wvm.BeginWorldOperation("Script load");
        if (operation == null) return false;
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        filename = ResolveWorldPath(filename);

        if (!File.Exists(filename))
            return false;

        bool installed = false;
        try
        {
            _cancellation.ThrowIfCancellationRequested();
            var (world, error) = World.LoadWorld(filename);
            if (world == null)
                return false;
            _cancellation.ThrowIfCancellationRequested();

            // Must set CurrentWorld on UI thread since it triggers rendering
            System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await _wvm.UndoManager.SaveUndoAsync();
                _cancellation.ThrowIfCancellationRequested();
                _wvm.CurrentWorld = world;
                installed = true;
                _loaded?.Invoke();
                _wvm.CurrentFile = filename;
                _wvm.Selection.IsActive = false;
                _wvm.PixelMap = new TEdit.Render.PixelMapManager();
                _wvm.PixelMap.InitializeBuffers(world.TilesWide, world.TilesHigh);
                await _wvm.UpdateRenderWorldAsync();
            }).Task.Unwrap().GetAwaiter().GetResult();

            return true;
        }
        catch (OperationCanceledException) { throw; }
        catch when (!installed)
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
        if (System.Windows.Application.Current?.Dispatcher.CheckAccess() != false) return false;
        using var operation = _wvm.BeginWorldOperation("Script save");
        if (operation == null) return false;
        try
        {
            _cancellation.ThrowIfCancellationRequested();
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() => _wvm.UndoManager.SaveUndoAsync())
                .Task.Unwrap().GetAwaiter().GetResult();
            // Preserve the world's version unless an explicit override is given.
            // Version numbers are never clamped — config lookup finds the best match.
            uint worldVer = _wvm.CurrentWorld?.Version ?? 0;
            uint v = version > 0 ? (uint)version : worldVer;

            World.SaveAsync(
                _wvm.CurrentWorld,
                filename,
                versionOverride: (int)v,
                progress: new Progress<ProgressChangedEventArgs>(_ => { })
            ).GetAwaiter().GetResult();

            System.Windows.Application.Current.Dispatcher.Invoke(() => _wvm.MarkScriptSaveCompleted(filename));

            return true;
        }
        catch
        {
            return false;
        }
    }
}
