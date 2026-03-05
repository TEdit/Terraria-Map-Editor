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
    private float _backgroundScaleZoom = 9f;
    private bool _showNews = true;
    private PixelMapColorMode _colorMode = PixelMapColorMode.Default;
    private int _spriteThumbnailSize = 64;
    private int _pickerHoldThresholdMs = 150;
    private string _telemetryDeclinedVersion = "";
    private UpdateChannel _updateChannel = UpdateChannel.Stable;
    private bool _showAllWeaponRackItems = false;
    private bool _enablePlayerEditor = false;
    private bool _showBuffRadii = false;
    private bool _showGlowMasks = true;
    private float _lightGlowIntensity = 0.5f;
    private bool _minimapBackground = false;
    private FilterManager.FilterMode _filterMode = FilterManager.FilterMode.Darken;
    private int _filterDarkenAmount = 60;
    private int _maxBackups = 10;
    private int _scriptTimeoutSeconds = 60;
    private List<string> _pinnedWorlds = new();
    private List<string> _recentWorlds = new();
    private Dictionary<string, List<InputBinding>> _inputBindings = new();
    private bool _showTextureLoadingNotice = true;
    private bool _enableMica = true;
    private bool _highQualityBrushPreview = true;

    // Tool Options
    private bool _wireChainMode = true;
    private bool _instantPaste = false;
    private bool _trackTunnelEnabled = true;

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

    public float BackgroundScaleZoom
    {
        get => _backgroundScaleZoom;
        set => SetField(ref _backgroundScaleZoom, value);
    }

    public bool ShowNews
    {
        get => _showNews;
        set => SetField(ref _showNews, value);
    }

    [JsonConverter(typeof(PixelMapColorModeJsonConverter))]
    public PixelMapColorMode ColorMode
    {
        get => _colorMode;
        set => SetField(ref _colorMode, value);
    }

    /// <summary>Legacy property for backward compatibility. Maps to ColorMode.</summary>
    [JsonIgnore]
    public bool RealisticColors
    {
        get => _colorMode == PixelMapColorMode.Realistic;
        set => ColorMode = value ? PixelMapColorMode.Realistic : PixelMapColorMode.Default;
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

    public bool ShowGlowMasks
    {
        get => _showGlowMasks;
        set => SetField(ref _showGlowMasks, value);
    }

    public float LightGlowIntensity
    {
        get => _lightGlowIntensity;
        set => SetField(ref _lightGlowIntensity, Math.Clamp(value, 0f, 1f));
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

    public bool ShowTextureLoadingNotice
    {
        get => _showTextureLoadingNotice;
        set => SetField(ref _showTextureLoadingNotice, value);
    }

    public bool EnableMica
    {
        get => _enableMica;
        set => SetField(ref _enableMica, value);
    }

    public bool HighQualityBrushPreview
    {
        get => _highQualityBrushPreview;
        set => SetField(ref _highQualityBrushPreview, value);
    }

    // ── Tool Options ──

    public bool WireChainMode
    {
        get => _wireChainMode;
        set => SetField(ref _wireChainMode, value);
    }

    public bool InstantPaste
    {
        get => _instantPaste;
        set => SetField(ref _instantPaste, value);
    }

    public bool TrackTunnelEnabled
    {
        get => _trackTunnelEnabled;
        set => SetField(ref _trackTunnelEnabled, value);
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

/// <summary>
/// Handles migration from the old bool RealisticColors to the new PixelMapColorMode enum.
/// Supports string enum names, numbers, and bool (true → Realistic, false → Default).
/// Also handles old "RealisticColors" key being deserialized as this property.
/// </summary>
public class PixelMapColorModeJsonConverter : JsonConverter<PixelMapColorMode>
{
    public override PixelMapColorMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var value = reader.GetString();
                if (Enum.TryParse<PixelMapColorMode>(value, ignoreCase: true, out var mode))
                    return mode;
                // Legacy: "true"/"false" as strings
                if (bool.TryParse(value, out var boolVal))
                    return boolVal ? PixelMapColorMode.Realistic : PixelMapColorMode.Default;
                break;

            case JsonTokenType.Number:
                var intValue = reader.GetInt32();
                if (Enum.IsDefined(typeof(PixelMapColorMode), intValue))
                    return (PixelMapColorMode)intValue;
                break;

            case JsonTokenType.True:
                return PixelMapColorMode.Realistic;

            case JsonTokenType.False:
                return PixelMapColorMode.Default;
        }

        return PixelMapColorMode.Default;
    }

    public override void Write(Utf8JsonWriter writer, PixelMapColorMode value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
