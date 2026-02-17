using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Xna.Framework;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Configuration;
using TEdit.ViewModel.Shared;
using Wpf.Ui.Controls;

namespace TEdit.ViewModel;

/// <summary>
/// ViewModel for the Filter sidebar tab. Manages filter selection for tiles, walls,
/// liquids, wires, and sprites, and applies filters to the rendering pipeline.
/// </summary>
[IReactiveObject]
public partial class FilterSidebarViewModel
{
    private readonly WorldViewModel _wvm;

    // Picker sub-viewmodels (multi-select mode)
    public TileWallPickerViewModel TilePicker { get; }
    public TileWallPickerViewModel WallPicker { get; }
    public TileWallPickerViewModel LiquidPicker { get; }
    public TileWallPickerViewModel WirePicker { get; }
    public SpriteTreePickerViewModel SpritePicker { get; }

    // Filter mode settings
    [Reactive]
    private FilterManager.FilterMode _filterMode = FilterManager.FilterMode.Hide;

    [Reactive]
    private FilterManager.BackgroundMode _backgroundMode = FilterManager.BackgroundMode.Normal;

    [Reactive]
    private System.Windows.Media.Color _customBackgroundColor = Colors.Lime;

    [Reactive]
    private bool _filterClipboardEnabled;

    [Reactive]
    private double _darkenAmount = 60;

    [Reactive]
    private int _selectedTabIndex;

    public FilterSidebarViewModel(WorldViewModel worldViewModel)
    {
        _wvm = worldViewModel;

        // Initialize pickers
        TilePicker = new TileWallPickerViewModel(PickerDataSource.TileBricks);
        WallPicker = new TileWallPickerViewModel(PickerDataSource.Walls);
        LiquidPicker = new TileWallPickerViewModel(PickerDataSource.Liquids);
        WirePicker = new TileWallPickerViewModel(PickerDataSource.Wires);
        SpritePicker = new SpriteTreePickerViewModel();

        // Load current filter state
        LoadFilterState();
    }

    private void LoadFilterState()
    {
        var settings = UserSettingsService.Current;
        FilterMode = settings.FilterMode;
        FilterManager.CurrentFilterMode = settings.FilterMode;
        FilterManager.DarkenAmount = settings.FilterDarkenAmount / 100f;
        BackgroundMode = FilterManager.CurrentBackgroundMode;
        FilterClipboardEnabled = FilterManager.FilterClipboard;
        DarkenAmount = settings.FilterDarkenAmount;

        var bgColor = FilterManager.BackgroundModeCustomColor;
        CustomBackgroundColor = System.Windows.Media.Color.FromArgb(bgColor.A, bgColor.R, bgColor.G, bgColor.B);

        // Restore checked state from FilterManager
        foreach (var id in FilterManager.SelectedTileIDs)
        {
            var item = TilePicker.AllItems.FirstOrDefault(x => x.Id == id);
            if (item != null) item.IsChecked = true;
        }

        foreach (var id in FilterManager.SelectedWallIDs)
        {
            var item = WallPicker.AllItems.FirstOrDefault(x => x.Id == id);
            if (item != null) item.IsChecked = true;
        }

        foreach (var id in FilterManager.SelectedSpriteIDs)
        {
            var node = SpritePicker.RootNodes.FirstOrDefault(x => x.TileId == id);
            if (node != null) node.IsChecked = true;
        }
    }

    [ReactiveCommand]
    private void Apply()
    {
        // Clear existing filters
        FilterManager.ClearAll();

        // Apply tile filters
        foreach (var item in TilePicker.AllItems.Where(x => x.IsChecked))
        {
            FilterManager.AddTileFilter(item.Id);
            FilterManager.SelectedTileNames.Add(item.Name);
        }

        // Apply wall filters
        foreach (var item in WallPicker.AllItems.Where(x => x.IsChecked))
        {
            FilterManager.AddWallFilter(item.Id);
            FilterManager.SelectedWallNames.Add(item.Name);
        }

        // Apply liquid filters
        foreach (var item in LiquidPicker.AllItems.Where(x => x.IsChecked))
        {
            FilterManager.AddLiquidFilter((Terraria.LiquidType)item.Id);
            FilterManager.SelectedLiquidNames.Add(item.Name);
        }

        // Apply wire filters
        foreach (var item in WirePicker.AllItems.Where(x => x.IsChecked))
        {
            FilterManager.AddWireFilter((FilterManager.WireType)item.Id);
            FilterManager.SelectedWireNames.Add(item.Name);
        }

        // Apply sprite filters
        foreach (var sprite in SpritePicker.GetSelectedSprites())
        {
            FilterManager.AddSpriteFilter(sprite.TileId);
            var name = SpritePicker.RootNodes.FirstOrDefault(n => n.TileId == sprite.TileId)?.Name ?? $"Sprite {sprite.TileId}";
            FilterManager.SelectedSpriteNames.Add(name);
        }

        // Set filter modes
        FilterManager.CurrentFilterMode = FilterMode;
        FilterManager.CurrentBackgroundMode = BackgroundMode;
        FilterManager.BackgroundModeCustomColor = new Microsoft.Xna.Framework.Color(
            CustomBackgroundColor.R,
            CustomBackgroundColor.G,
            CustomBackgroundColor.B,
            CustomBackgroundColor.A);
        FilterManager.FilterClipboard = FilterClipboardEnabled;
        FilterManager.DarkenAmount = (float)(DarkenAmount / 100.0);

        // Persist filter mode and darken amount to user settings
        var settings = UserSettingsService.Current;
        settings.FilterMode = FilterMode;
        settings.FilterDarkenAmount = (int)DarkenAmount;

        // Refresh all renderings (pixel map and minimap)
        _wvm.RefreshAllRenderings(useFilter: true);
    }

    [ReactiveCommand]
    private void Disable()
    {
        // Clear all selections
        foreach (var item in TilePicker.AllItems) item.IsChecked = false;
        foreach (var item in WallPicker.AllItems) item.IsChecked = false;
        foreach (var item in LiquidPicker.AllItems) item.IsChecked = false;
        foreach (var item in WirePicker.AllItems) item.IsChecked = false;
        SpritePicker.ClearSelectionCommand.Execute(System.Reactive.Unit.Default).Subscribe();

        // Clear filter manager
        FilterManager.ClearAll();

        // Reset modes
        FilterMode = FilterManager.FilterMode.Hide;
        BackgroundMode = FilterManager.BackgroundMode.Normal;
        FilterClipboardEnabled = false;

        // Refresh all renderings without filter (default rendering)
        _wvm.RefreshAllRenderings(useFilter: false);
    }

    [ReactiveCommand]
    private async Task PickCustomColor()
    {
        // Use a simple color picker dialog
        var colorDialog = new System.Windows.Forms.ColorDialog
        {
            Color = System.Drawing.Color.FromArgb(
                CustomBackgroundColor.A,
                CustomBackgroundColor.R,
                CustomBackgroundColor.G,
                CustomBackgroundColor.B)
        };

        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            CustomBackgroundColor = System.Windows.Media.Color.FromArgb(
                colorDialog.Color.A,
                colorDialog.Color.R,
                colorDialog.Color.G,
                colorDialog.Color.B);
        }

        await Task.CompletedTask;
    }
}
