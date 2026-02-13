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
                if (settings != null) return settings;
            }
        }
        catch
        {
            // Corrupted file — fall through to defaults
        }

        return new UserSettings();
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
