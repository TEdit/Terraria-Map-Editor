using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using TEdit.Input;
using TEdit.ViewModel;

namespace TEdit.Configuration;

public class UserSettings : INotifyPropertyChanged
{
    private string _terrariaPath = "";
    private bool _autosave = true;
    private UpdateMode _updateMode = UpdateMode.AutoDownload;
    private LanguageSelection _language = LanguageSelection.Automatic;
    private int _telemetry = -1;
    private float _textureVisibilityZoomLevel = 6f;
    private bool _showNews = true;
    private bool _realisticColors = false;
    private int _spriteThumbnailSize = 64;
    private int _pickerHoldThresholdMs = 150;
    private string _telemetryDeclinedVersion = "";
    private UpdateChannel _updateChannel = UpdateChannel.Stable;
    private bool _showAllWeaponRackItems = false;
    private bool _enablePlayerEditor = false;
    private bool _showBuffRadii = false;
    private bool _minimapBackground = false;
    private FilterManager.FilterMode _filterMode = FilterManager.FilterMode.Darken;
    private int _filterDarkenAmount = 60;
    private int _maxBackups = 10;
    private int _scriptTimeoutSeconds = 60;
    private List<string> _pinnedWorlds = new();
    private List<string> _recentWorlds = new();
    private Dictionary<string, List<InputBinding>> _inputBindings = new();

    public string TerrariaPath
    {
        get => _terrariaPath;
        set => SetField(ref _terrariaPath, value ?? "");
    }

    public bool Autosave
    {
        get => _autosave;
        set => SetField(ref _autosave, value);
    }

    [JsonConverter(typeof(UpdateModeJsonConverter))]
    public UpdateMode UpdateMode
    {
        get => _updateMode;
        set => SetField(ref _updateMode, value);
    }

    public LanguageSelection Language
    {
        get => _language;
        set => SetField(ref _language, value);
    }

    public int Telemetry
    {
        get => _telemetry;
        set => SetField(ref _telemetry, value);
    }

    public float TextureVisibilityZoomLevel
    {
        get => _textureVisibilityZoomLevel;
        set => SetField(ref _textureVisibilityZoomLevel, value);
    }

    public bool ShowNews
    {
        get => _showNews;
        set => SetField(ref _showNews, value);
    }

    public bool RealisticColors
    {
        get => _realisticColors;
        set => SetField(ref _realisticColors, value);
    }

    public int SpriteThumbnailSize
    {
        get => _spriteThumbnailSize;
        set => SetField(ref _spriteThumbnailSize, value);
    }

    public int PickerHoldThresholdMs
    {
        get => _pickerHoldThresholdMs;
        set => SetField(ref _pickerHoldThresholdMs, Math.Clamp(value, 100, 500));
    }

    public string TelemetryDeclinedVersion
    {
        get => _telemetryDeclinedVersion;
        set => SetField(ref _telemetryDeclinedVersion, value ?? "");
    }

    [JsonConverter(typeof(UpdateChannelJsonConverter))]
    public UpdateChannel UpdateChannel
    {
        get => _updateChannel;
        set => SetField(ref _updateChannel, value);
    }

    public bool ShowAllWeaponRackItems
    {
        get => _showAllWeaponRackItems;
        set => SetField(ref _showAllWeaponRackItems, value);
    }

    public bool EnablePlayerEditor
    {
        get => _enablePlayerEditor;
        set => SetField(ref _enablePlayerEditor, value);
    }

    public bool ShowBuffRadii
    {
        get => _showBuffRadii;
        set => SetField(ref _showBuffRadii, value);
    }

    public bool MinimapBackground
    {
        get => _minimapBackground;
        set => SetField(ref _minimapBackground, value);
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FilterManager.FilterMode FilterMode
    {
        get => _filterMode;
        set => SetField(ref _filterMode, value);
    }

    public int FilterDarkenAmount
    {
        get => _filterDarkenAmount;
        set => SetField(ref _filterDarkenAmount, Math.Clamp(value, 0, 100));
    }

    public int MaxBackups
    {
        get => _maxBackups;
        set => SetField(ref _maxBackups, Math.Clamp(value, 1, 50));
    }

    public int ScriptTimeoutSeconds
    {
        get => _scriptTimeoutSeconds;
        set => SetField(ref _scriptTimeoutSeconds, Math.Clamp(value, 10, 300));
    }

    public List<string> PinnedWorlds
    {
        get => _pinnedWorlds;
        set => SetField(ref _pinnedWorlds, value ?? new());
    }

    public List<string> RecentWorlds
    {
        get => _recentWorlds;
        set => SetField(ref _recentWorlds, value ?? new());
    }

    [JsonConverter(typeof(InputBindingsDictionaryJsonConverter))]
    public Dictionary<string, List<InputBinding>> InputBindings
    {
        get => _inputBindings;
        set => SetField(ref _inputBindings, value ?? new());
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Handles migration from the old string-based UpdateChannel (e.g. "") to the new enum.
/// Falls back to Stable for unrecognized or empty values.
/// </summary>
public class UpdateChannelJsonConverter : JsonConverter<UpdateChannel>
{
    public override UpdateChannel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (Enum.TryParse<UpdateChannel>(value, ignoreCase: true, out var channel))
                return channel;
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(UpdateChannel), intValue))
                return (UpdateChannel)intValue;
        }

        return UpdateChannel.Stable;
    }

    public override void Write(Utf8JsonWriter writer, UpdateChannel value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

/// <summary>
/// Handles migration from the old bool CheckUpdates (true → AutoDownload, false → Disabled)
/// to the new UpdateMode enum. Supports string, number, and bool JSON values.
/// </summary>
public class UpdateModeJsonConverter : JsonConverter<UpdateMode>
{
    public override UpdateMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var value = reader.GetString();
                if (Enum.TryParse<UpdateMode>(value, ignoreCase: true, out var mode))
                    return mode;
                // Legacy: "true"/"false" as strings
                if (bool.TryParse(value, out var boolVal))
                    return boolVal ? UpdateMode.AutoDownload : UpdateMode.Disabled;
                break;

            case JsonTokenType.Number:
                var intValue = reader.GetInt32();
                if (Enum.IsDefined(typeof(UpdateMode), intValue))
                    return (UpdateMode)intValue;
                break;

            case JsonTokenType.True:
                return UpdateMode.AutoDownload;

            case JsonTokenType.False:
                return UpdateMode.Disabled;
        }

        return UpdateMode.AutoDownload;
    }

    public override void Write(Utf8JsonWriter writer, UpdateMode value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
