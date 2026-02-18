using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TEdit.Configuration;

public static class UserSettingsService
{
    private static readonly object _lock = new();
    private static UserSettings _current;

    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TEdit");

    private static readonly string SettingsPath =
        Path.Combine(SettingsDir, "userSettings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static UserSettings Current
    {
        get
        {
            if (_current != null) return _current;
            lock (_lock)
            {
                if (_current != null) return _current;
                _current = Load();
                _current.PropertyChanged += (_, _) => Save();
                return _current;
            }
        }
    }

    private static UserSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<UserSettings>(json, JsonOptions);
                if (settings != null)
                {
                    // Migrate legacy RealisticColors bool → ColorMode enum
                    MigrateLegacySettings(json, settings);
                    return settings;
                }
            }
        }
        catch
        {
            // Corrupted file — fall through to defaults
        }

        return new UserSettings();
    }

    /// <summary>
    /// Migrates legacy settings keys to their new equivalents.
    /// Currently handles: RealisticColors (bool) → ColorMode (PixelMapColorMode)
    /// </summary>
    private static void MigrateLegacySettings(string json, UserSettings settings)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // If old RealisticColors exists but new ColorMode does not, migrate
            if (root.TryGetProperty("RealisticColors", out var realisticProp) &&
                !root.TryGetProperty("ColorMode", out _))
            {
                if (realisticProp.ValueKind == JsonValueKind.True)
                    settings.ColorMode = ViewModel.PixelMapColorMode.Realistic;
                else
                    settings.ColorMode = ViewModel.PixelMapColorMode.Default;
            }
        }
        catch
        {
            // Migration is best-effort
        }
    }

    private static void Save()
    {
        try
        {
            if (!Directory.Exists(SettingsDir))
                Directory.CreateDirectory(SettingsDir);

            var json = JsonSerializer.Serialize(_current, JsonOptions);
            var tempPath = SettingsPath + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, SettingsPath, overwrite: true);
        }
        catch
        {
            // Best-effort save — don't crash the app over settings persistence
        }
    }
}
