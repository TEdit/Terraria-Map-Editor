using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Configuration;
using TEdit.Input;
using TEdit.Properties;

namespace TEdit.ViewModel;

[IReactiveObject]
public partial class SettingsViewModel
{
    public ObservableCollection<SettingItem> AllSettings { get; } = [];

    // Track manual expansion state (only applies when not searching)
    private readonly Dictionary<string, bool> _categoryExpansionState = new();

    // Store original values for Cancel functionality
    private readonly Dictionary<SettingItem, object> _originalValues = new();
    private readonly Dictionary<SettingItem, List<InputBinding>> _originalBindings = new();

    [Reactive]
    private string _searchText;

    public ICollectionView SettingsView { get; }

    public SettingsViewModel(WorldViewModel wvm)
    {
        PopulateSettings(wvm);
        CaptureOriginalValues();

        // Initialize all categories as expanded
        foreach (var category in AllSettings.Select(s => s.Category).Distinct())
            _categoryExpansionState[category] = true;

        var cvs = new CollectionViewSource { Source = AllSettings };
        SettingsView = cvs.View;
        SettingsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SettingItem.Category)));
        SettingsView.Filter = FilterSetting;

        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(_ =>
            {
                SettingsView.Refresh();
                // Notify that expansion states may have changed due to search
                this.RaisePropertyChanged(nameof(SearchText));
            });
    }

    /// <summary>
    /// Checks if a category should be expanded.
    /// When searching, auto-expands categories with matching items.
    /// Otherwise, uses manual expansion state.
    /// </summary>
    public bool IsCategoryExpanded(string category)
    {
        // If searching, expand categories with matching items
        if (!string.IsNullOrWhiteSpace(SearchText))
            return AllSettings.Any(s => s.Category == category && FilterSetting(s));

        // Otherwise use manual state (default: expanded)
        return _categoryExpansionState.GetValueOrDefault(category, true);
    }

    /// <summary>
    /// Sets the manual expansion state for a category.
    /// </summary>
    public void SetCategoryExpanded(string category, bool expanded)
    {
        _categoryExpansionState[category] = expanded;
    }

    /// <summary>
    /// Captures the current values of all settings for Cancel functionality.
    /// </summary>
    private void CaptureOriginalValues()
    {
        foreach (var setting in AllSettings)
        {
            if (setting.EditorType == SettingEditorType.Keybinding)
            {
                // For keybindings, capture a copy of the bindings list
                _originalBindings[setting] = new List<InputBinding>(setting.Bindings);
            }
            else if (setting.Getter != null)
            {
                _originalValues[setting] = setting.Getter();
            }
        }
    }

    /// <summary>
    /// Reverts all settings to their original values (called on Cancel).
    /// </summary>
    public void RevertChanges()
    {
        foreach (var setting in AllSettings)
        {
            if (setting.EditorType == SettingEditorType.Keybinding)
            {
                // Restore keybindings
                if (_originalBindings.TryGetValue(setting, out var originalBindings))
                {
                    setting.Bindings.Clear();
                    foreach (var binding in originalBindings)
                        setting.Bindings.Add(binding);

                    // Persist the restored bindings
                    App.Input.Registry.SetUserBindings(setting.ActionId, originalBindings);
                }
            }
            else if (_originalValues.TryGetValue(setting, out var originalValue))
            {
                // Only revert if value has changed
                var currentValue = setting.Getter?.Invoke();
                if (!Equals(currentValue, originalValue))
                {
                    setting.Setter?.Invoke(originalValue);
                }
            }
        }

        // Save keybinding changes
        App.Input.SaveUserCustomizations();
    }

    private bool FilterSetting(object obj)
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        if (obj is not SettingItem item) return false;

        return item.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || (item.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
            || item.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    private void PopulateSettings(WorldViewModel wvm)
    {
        // ── General ──
        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_language,
            Description = Language.settings_language_desc,
            Category = Language.settings_category_general,
            EditorType = SettingEditorType.ComboBox,
            Getter = () => wvm.CurrentLanguage,
            Setter = v =>
            {
                var lang = (LanguageSelection)v;
                wvm.CurrentLanguage = lang;
                wvm.SetLanguageCommand.Execute(lang).Subscribe();
            },
            ComboBoxItems = Enum.GetValues<LanguageSelection>()
        });

        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_update_mode,
            Description = Language.settings_update_mode_desc,
            Category = Language.settings_category_general,
            EditorType = SettingEditorType.ComboBox,
            Getter = () => wvm.UpdateMode,
            Setter = v => wvm.UpdateMode = (UpdateMode)v,
            ComboBoxItems = Enum.GetValues<UpdateMode>()
        });

        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_update_channel,
            Description = Language.settings_update_channel_desc,
            Category = Language.settings_category_general,
            EditorType = SettingEditorType.ComboBox,
            Getter = () => wvm.UpdateChannel,
            Setter = v => wvm.UpdateChannel = (UpdateChannel)v,
            ComboBoxItems = Enum.GetValues<UpdateChannel>()
        });

        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_autosave,
            Description = Language.settings_autosave_desc,
            Category = Language.settings_category_general,
            EditorType = SettingEditorType.CheckBox,
            Getter = () => wvm.IsAutoSaveEnabled,
            Setter = v => wvm.IsAutoSaveEnabled = (bool)v
        });

        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_show_news,
            Description = Language.settings_show_news_desc,
            Category = Language.settings_category_general,
            EditorType = SettingEditorType.CheckBox,
            Getter = () => wvm.ShowNews,
            Setter = v => wvm.ShowNews = (bool)v
        });

        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_show_all_weapon_rack_items,
            Description = Language.settings_show_all_weapon_rack_items_desc,
            Category = Language.settings_category_general,
            EditorType = SettingEditorType.CheckBox,
            Getter = () => UserSettingsService.Current.ShowAllWeaponRackItems,
            Setter = v => UserSettingsService.Current.ShowAllWeaponRackItems = (bool)v
        });

        // ── Experimental Features ──
        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_enable_player_editor,
            Description = Language.settings_enable_player_editor_desc,
            Category = Language.settings_category_experimental,
            EditorType = SettingEditorType.CheckBox,
            Getter = () => UserSettingsService.Current.EnablePlayerEditor,
            Setter = v => UserSettingsService.Current.EnablePlayerEditor = (bool)v
        });

        // ── Rendering ──
        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_realistic_colors,
            Description = Language.settings_realistic_colors_desc,
            Category = Language.settings_category_rendering,
            EditorType = SettingEditorType.CheckBox,
            Getter = () => wvm.RealisticColors,
            Setter = v => wvm.RealisticColors = (bool)v
        });

        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_texture_zoom,
            Description = Language.settings_texture_zoom_desc,
            Category = Language.settings_category_rendering,
            EditorType = SettingEditorType.Slider,
            SliderMin = 3,
            SliderMax = 10,
            SliderStep = 1,
            Getter = () => (double)wvm.TextureVisibilityZoomLevel,
            Setter = v => wvm.TextureVisibilityZoomLevel = (float)(double)v
        });

        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_sprite_thumbnail,
            Description = Language.settings_sprite_thumbnail_desc,
            Category = Language.settings_category_rendering,
            EditorType = SettingEditorType.Slider,
            SliderMin = 16,
            SliderMax = 128,
            SliderStep = 16,
            Getter = () => (double)wvm.SpriteThumbnailSize,
            Setter = v => wvm.SpriteThumbnailSize = (int)(double)v
        });

        // ── Layers ──
        var layers = Language.settings_category_layers;

        AddLayerCheckBox(Language.settings_show_textures, Language.settings_show_textures_desc, layers,
            () => wvm.ShowTextures, v => wvm.ShowTextures = v);

        AddLayerCheckBox(Language.settings_show_grid, Language.settings_show_grid_desc, layers,
            () => wvm.ShowGrid, v => wvm.ShowGrid = v);

        AddLayerCheckBox(Language.settings_show_backgrounds, Language.settings_show_backgrounds_desc, layers,
            () => wvm.ShowBackgrounds, v => wvm.ShowBackgrounds = v);

        AddLayerCheckBox(Language.settings_show_walls, Language.settings_show_walls_desc, layers,
            () => wvm.ShowWalls, v => wvm.ShowWalls = v);

        AddLayerCheckBox(Language.settings_show_tiles, Language.settings_show_tiles_desc, layers,
            () => wvm.ShowTiles, v => wvm.ShowTiles = v);

        AddLayerCheckBox(Language.settings_show_coatings, Language.settings_show_coatings_desc, layers,
            () => wvm.ShowCoatings, v => wvm.ShowCoatings = v);

        AddLayerCheckBox(Language.settings_show_liquid, Language.settings_show_liquid_desc, layers,
            () => wvm.ShowLiquid, v => wvm.ShowLiquid = v);

        AddLayerCheckBox(Language.settings_show_actuators, Language.settings_show_actuators_desc, layers,
            () => wvm.ShowActuators, v => wvm.ShowActuators = v);

        AddLayerCheckBox(Language.settings_show_red_wires, Language.settings_show_red_wires_desc, layers,
            () => wvm.ShowRedWires, v => wvm.ShowRedWires = v);

        AddLayerCheckBox(Language.settings_show_blue_wires, Language.settings_show_blue_wires_desc, layers,
            () => wvm.ShowBlueWires, v => wvm.ShowBlueWires = v);

        AddLayerCheckBox(Language.settings_show_green_wires, Language.settings_show_green_wires_desc, layers,
            () => wvm.ShowGreenWires, v => wvm.ShowGreenWires = v);

        AddLayerCheckBox(Language.settings_show_yellow_wires, Language.settings_show_yellow_wires_desc, layers,
            () => wvm.ShowYellowWires, v => wvm.ShowYellowWires = v);

        AddLayerCheckBox(Language.settings_show_all_wires, Language.settings_show_all_wires_desc, layers,
            () => wvm.ShowAllWires, v => wvm.ShowAllWires = v);

        AddLayerCheckBox(Language.settings_wire_transparency, Language.settings_wire_transparency_desc, layers,
            () => wvm.ShowWireTransparency, v => wvm.ShowWireTransparency = v);

        AddLayerCheckBox(Language.settings_show_points, Language.settings_show_points_desc, layers,
            () => wvm.ShowPoints, v => wvm.ShowPoints = v);

        AddLayerCheckBox(Language.settings_minimap_background, Language.settings_minimap_background_desc, layers,
            () => wvm.MinimapBackground, v => wvm.MinimapBackground = v);

        // ── Filter & Search ──
        var filterCategory = "Filter & Search";
        var settings = UserSettingsService.Current;

        AllSettings.Add(new SettingItem
        {
            Name = "Filter Mode",
            Description = "Default filter mode when applying tile/wall/liquid/wire filters",
            Category = filterCategory,
            EditorType = SettingEditorType.ComboBox,
            Getter = () => settings.FilterMode,
            Setter = v => settings.FilterMode = (FilterManager.FilterMode)v,
            ComboBoxItems = Enum.GetValues<FilterManager.FilterMode>()
        });

        AllSettings.Add(new SettingItem
        {
            Name = "Darken Amount",
            Description = "Opacity of the darken overlay for non-selected tiles (0–100%)",
            Category = filterCategory,
            EditorType = SettingEditorType.Slider,
            SliderMin = 0,
            SliderMax = 100,
            SliderStep = 10,
            Getter = () => (double)settings.FilterDarkenAmount,
            Setter = v => settings.FilterDarkenAmount = (int)(double)v
        });

        // ── Scripting ──
        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_script_timeout,
            Description = Language.settings_script_timeout_desc,
            Category = Language.settings_category_scripting,
            EditorType = SettingEditorType.Slider,
            SliderMin = 10,
            SliderMax = 300,
            SliderStep = 10,
            Getter = () => (double)settings.ScriptTimeoutSeconds,
            Setter = v => settings.ScriptTimeoutSeconds = (int)(double)v
        });

        // ── Privacy ──
        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_error_reporting,
            Description = Language.settings_error_reporting_desc,
            Category = Language.settings_category_privacy,
            EditorType = SettingEditorType.CheckBox,
            Getter = () => wvm.EnableTelemetry,
            Setter = v => wvm.EnableTelemetry = (bool)v
        });

        // ── Paths ──
        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_terraria_path,
            Description = Language.settings_terraria_path_desc,
            Category = Language.settings_category_paths,
            EditorType = SettingEditorType.Path,
            Placeholder = DependencyChecker.PathToContent ?? "",
            Getter = () => UserSettingsService.Current.TerrariaPath,
            Setter = v => UserSettingsService.Current.TerrariaPath = (string)v
        });

        // ── Keybindings ──
        PopulateKeybindings();

        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_picker_hold_threshold,
            Description = Language.settings_picker_hold_threshold_desc,
            Category = "Keybindings - Tools",
            EditorType = SettingEditorType.Slider,
            SliderMin = 100,
            SliderMax = 500,
            SliderStep = 50,
            Getter = () => (double)wvm.PickerHoldThresholdMs,
            Setter = v => wvm.PickerHoldThresholdMs = (int)(double)v
        });
    }

    private void PopulateKeybindings()
    {
        var registry = App.Input.Registry;
        var actionsByCategory = registry.GetActionsByCategory();

        foreach (var (category, actions) in actionsByCategory.OrderBy(kvp => kvp.Key))
        {
            foreach (var action in actions.OrderBy(a => a.Name))
            {
                var bindings = new ObservableCollection<InputBinding>(registry.GetBindings(action.Id));

                AllSettings.Add(new SettingItem
                {
                    Name = action.Name,
                    Description = action.Description ?? "",
                    Category = $"Keybindings - {category}",
                    EditorType = SettingEditorType.Keybinding,
                    ActionId = action.Id,
                    Bindings = bindings
                });
            }
        }
    }

    private void AddLayerCheckBox(string name, string description, string category,
        Func<bool> getter, Action<bool> setter)
    {
        AllSettings.Add(new SettingItem
        {
            Name = name,
            Description = description,
            Category = category,
            EditorType = SettingEditorType.CheckBox,
            Getter = () => getter(),
            Setter = v => setter((bool)v)
        });
    }
}

/// <summary>
/// Converter for binding Expander.IsExpanded to SettingsViewModel's category expansion state.
/// </summary>
public class CategoryExpandedConverter : IMultiValueConverter
{
    public static CategoryExpandedConverter Instance { get; } = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0] = category name (from CollectionViewGroup.Name)
        // values[1] = SettingsViewModel
        if (values[0] is string category && values[1] is SettingsViewModel vm)
            return vm.IsCategoryExpanded(category);
        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("CategoryExpandedConverter is one-way only");
    }
}
