using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TEdit.Configuration;

/// <summary>
/// Manages per-world camera state (zoom, scroll position).
/// Persists to worldViewState.json in the TEdit app data folder.
/// </summary>
public static class WorldViewStateManager
{
    private static readonly string StatePath = Path.Combine(
        AppDataPaths.DataDir, "worldViewState.json");

    private static Dictionary<string, WorldViewState> _states;
    private static bool _dirty;

    public static WorldViewState GetState(string worldPath)
    {
        EnsureLoaded();
        if (string.IsNullOrEmpty(worldPath)) return null;

        var key = NormalizeKey(worldPath);
        return _states.TryGetValue(key, out var state) ? state : null;
    }

    public static void SaveState(string worldPath, float scrollX, float scrollY, float zoom)
    {
        EnsureLoaded();
        if (string.IsNullOrEmpty(worldPath)) return;

        var key = NormalizeKey(worldPath);
        _states[key] = new WorldViewState
        {
            ScrollX = scrollX,
            ScrollY = scrollY,
            Zoom = zoom
        };
        _dirty = true;
    }

    /// <summary>
    /// Writes to disk if there are pending changes. Call periodically (e.g., every 30s) or on exit.
    /// </summary>
    public static void Flush()
    {
        if (!_dirty) return;
        _dirty = false;

        try
        {
            var dir = Path.GetDirectoryName(StatePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(_states, new JsonSerializerOptions { WriteIndented = true });
            var tempPath = StatePath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, StatePath, overwrite: true);
        }
        catch
        {
            // Best-effort persistence
        }
    }

    private static void EnsureLoaded()
    {
        if (_states != null) return;

        try
        {
            if (File.Exists(StatePath))
            {
                var json = File.ReadAllText(StatePath);
                _states = JsonSerializer.Deserialize<Dictionary<string, WorldViewState>>(json);
            }
        }
        catch
        {
            // Corrupted file, start fresh
        }

        _states ??= new Dictionary<string, WorldViewState>();
    }

    private static string NormalizeKey(string path)
    {
        return path.Replace('\\', '/').ToLowerInvariant();
    }
}

public class WorldViewState
{
    public float ScrollX { get; set; }
    public float ScrollY { get; set; }
    public float Zoom { get; set; }
}
