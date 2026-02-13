using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Configuration;
using TEdit.Properties;

namespace TEdit.ViewModel;

[IReactiveObject]
public partial class SettingsViewModel
{
    public ObservableCollection<SettingItem> AllSettings { get; } = [];

    [Reactive]
    private string _searchText;

    public ICollectionView SettingsView { get; }

    public SettingsViewModel(WorldViewModel wvm)
    {
        PopulateSettings(wvm);

        var cvs = new CollectionViewSource { Source = AllSettings };
        SettingsView = cvs.View;
        SettingsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SettingItem.Category)));
        SettingsView.Filter = FilterSetting;

        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(_ => SettingsView.Refresh());
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
            Name = Language.settings_check_updates,
            Description = Language.settings_check_updates_desc,
            Category = Language.settings_category_general,
            EditorType = SettingEditorType.CheckBox,
            Getter = () => wvm.CheckUpdates,
            Setter = v => wvm.CheckUpdates = (bool)v
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
        var detectedPath = DependencyChecker.PathToContent;
        var pathDesc = !string.IsNullOrWhiteSpace(detectedPath)
            ? $"{Language.settings_terraria_path_desc} ({detectedPath})"
            : Language.settings_terraria_path_desc;

        AllSettings.Add(new SettingItem
        {
            Name = Language.settings_terraria_path,
            Description = pathDesc,
            Category = Language.settings_category_paths,
            EditorType = SettingEditorType.Path,
            Getter = () => UserSettingsService.Current.TerrariaPath,
            Setter = v => UserSettingsService.Current.TerrariaPath = (string)v
        });
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
