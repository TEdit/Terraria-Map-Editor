using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using TEdit.Input;
using TEdit.ViewModel;

namespace TEdit.Configuration;

public class UserSettings : INotifyPropertyChanged
{
    private string _terrariaPath = "";
    private bool _autosave = true;
    private bool _checkUpdates = true;
    private LanguageSelection _language = LanguageSelection.Automatic;
    private int _telemetry = -1;
    private float _textureVisibilityZoomLevel = 6f;
    private bool _showNews = true;
    private bool _realisticColors = false;
    private int _spriteThumbnailSize = 64;
    private string _telemetryDeclinedVersion = "";
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

    public bool CheckUpdates
    {
        get => _checkUpdates;
        set => SetField(ref _checkUpdates, value);
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

    public string TelemetryDeclinedVersion
    {
        get => _telemetryDeclinedVersion;
        set => SetField(ref _telemetryDeclinedVersion, value ?? "");
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
