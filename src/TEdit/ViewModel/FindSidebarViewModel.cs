using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Xna.Framework;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
using TEdit.ViewModel.Shared;

namespace TEdit.ViewModel;

/// <summary>
/// Represents a search result with name and coordinates.
/// </summary>
public record FindResultItem(string Name, int X, int Y, string ResultType, string? ExtraInfo = null)
{
    public string DisplayText => ExtraInfo != null
        ? $"{Name} @ {X}, {Y} ({ResultType}) {ExtraInfo}"
        : $"{Name} @ {X}, {Y} ({ResultType})";
}

/// <summary>
/// ViewModel for the Find sidebar tab. Manages search for items in containers
/// and tiles/walls/sprites on the map, with aggregated results and navigation.
/// </summary>
[IReactiveObject]
public partial class FindSidebarViewModel
{
    private readonly WorldViewModel _wvm;
    private const int MaxResults = 1000;

    // Picker sub-viewmodels (all multi-select, selections persist when switching tabs)
    public TileWallPickerViewModel ItemPicker { get; }    // Items in containers
    public TileWallPickerViewModel TilePicker { get; }    // Tiles on map
    public TileWallPickerViewModel WallPicker { get; }    // Walls on map
    public SpriteTreePickerViewModel SpritePicker { get; } // Sprites on map

    [Reactive]
    private int _selectedTabIndex;

    [Reactive]
    private bool _calculateDistance;

    [Reactive]
    private bool _autoZoomOnNavigate; // Default false - just pan, don't zoom

    // Aggregated results (capped at MaxResults)
    public ObservableCollection<FindResultItem> Results { get; } = [];

    [Reactive]
    private int _currentResultIndex = -1;

    [Reactive]
    private FindResultItem? _selectedResult;

    // Crosshair overlay state
    [Reactive]
    private bool _showCrosshair;

    [Reactive]
    private int _crosshairTileX;

    [Reactive]
    private int _crosshairTileY;

    // Summary of what's selected
    public int TotalSelectedCount =>
        ItemPicker.SelectedCount +
        TilePicker.SelectedCount +
        WallPicker.SelectedCount +
        SpritePicker.SelectedCount;

    public string SelectionSummary
    {
        get
        {
            var parts = new List<string>();
            if (ItemPicker.SelectedCount > 0) parts.Add($"{ItemPicker.SelectedCount} items");
            if (TilePicker.SelectedCount > 0) parts.Add($"{TilePicker.SelectedCount} tiles");
            if (WallPicker.SelectedCount > 0) parts.Add($"{WallPicker.SelectedCount} walls");
            if (SpritePicker.SelectedCount > 0) parts.Add($"{SpritePicker.SelectedCount} sprites");
            return parts.Count > 0 ? $"Selected: {string.Join(", ", parts)}" : "No selection";
        }
    }

    public string ResultSummary => Results.Count > 0
        ? $"{CurrentResultIndex + 1} of {Results.Count}"
        : "No results";

    public FindSidebarViewModel(WorldViewModel worldViewModel)
    {
        _wvm = worldViewModel;

        // Initialize pickers
        ItemPicker = new TileWallPickerViewModel(PickerDataSource.Items);
        TilePicker = new TileWallPickerViewModel(PickerDataSource.TileBricks);
        WallPicker = new TileWallPickerViewModel(PickerDataSource.Walls);
        SpritePicker = new SpriteTreePickerViewModel();

        // Subscribe to selection changes to update summary
        this.WhenAnyValue(
            x => x.ItemPicker.SelectedCount,
            x => x.TilePicker.SelectedCount,
            x => x.WallPicker.SelectedCount,
            x => x.SpritePicker.SelectedCount)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(TotalSelectedCount));
                this.RaisePropertyChanged(nameof(SelectionSummary));
            });

        // Subscribe to result index changes
        this.WhenAnyValue(x => x.CurrentResultIndex)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(ResultSummary)));

        // Subscribe to selected result changes (user clicking in the list)
        this.WhenAnyValue(x => x.SelectedResult)
            .Subscribe(result =>
            {
                if (result != null)
                {
                    var index = Results.IndexOf(result);
                    if (index >= 0 && index != CurrentResultIndex)
                    {
                        CurrentResultIndex = index;
                        GoToCurrentResult();
                    }
                }
            });
    }

    [ReactiveCommand]
    private void ExecuteSearch()
    {
        if (_wvm.CurrentWorld == null)
        {
            MessageBox.Show("No world loaded.", "Find", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Results.Clear();
        CurrentResultIndex = -1;
        ShowCrosshair = false;

        var world = _wvm.CurrentWorld;
        var spawn = new Vector2(world.SpawnX, world.SpawnY);

        // 1. Search CONTAINERS for selected items
        var selectedItemIds = ItemPicker.GetSelectedIds();
        if (selectedItemIds.Count > 0 && Results.Count < MaxResults)
        {
            SearchContainers(selectedItemIds, spawn);
        }

        // 2. Search MAP for selected tiles/walls/sprites
        var selectedTileIds = TilePicker.GetSelectedIds();
        var selectedWallIds = WallPicker.GetSelectedIds();
        var selectedSprites = SpritePicker.GetSelectedSprites();

        if ((selectedTileIds.Count > 0 || selectedWallIds.Count > 0 || selectedSprites.Count > 0)
            && Results.Count < MaxResults)
        {
            SearchMap(selectedTileIds, selectedWallIds, selectedSprites, spawn);
        }

        // Sort by distance if enabled
        if (CalculateDistance && Results.Count > 0)
        {
            var sorted = Results
                .OrderBy(r => Vector2.Distance(spawn, new Vector2(r.X, r.Y)))
                .ToList();
            Results.Clear();
            foreach (var item in sorted) Results.Add(item);
        }

        // Navigate to first result if found
        if (Results.Count > 0)
        {
            CurrentResultIndex = 0;
            GoToCurrentResult();
        }

        this.RaisePropertyChanged(nameof(ResultSummary));
    }

    private void SearchContainers(IReadOnlyList<int> selectedItemIds, Vector2 spawn)
    {
        var world = _wvm.CurrentWorld;
        var itemIdSet = selectedItemIds.ToHashSet();

        // Search chests
        foreach (var chest in world.Chests)
        {
            if (Results.Count >= MaxResults) return;

            foreach (var item in chest.Items)
            {
                if (item.NetId > 0 && itemIdSet.Contains(item.NetId))
                {
                    var extraInfo = CalculateDistance
                        ? $"Distance: {Math.Round(Vector2.Distance(spawn, new Vector2(chest.X, chest.Y)))}"
                        : null;

                    Results.Add(new FindResultItem(
                        item.GetName(),
                        chest.X,
                        chest.Y,
                        "Chest",
                        extraInfo));

                    if (Results.Count >= MaxResults) return;
                }
            }
        }

        // Search tile entities that can hold items (item frames, mannequins, etc.)
        foreach (var entity in world.TileEntities)
        {
            if (Results.Count >= MaxResults) return;

            // Check if this entity has items
            if (entity.Items != null)
            {
                foreach (var item in entity.Items)
                {
                    if (item.Id > 0 && itemIdSet.Contains(item.Id))
                    {
                        var extraInfo = CalculateDistance
                            ? $"Distance: {Math.Round(Vector2.Distance(spawn, new Vector2(entity.PosX, entity.PosY)))}"
                            : null;

                        // Get item name from WorldConfiguration
                        var itemProp = WorldConfiguration.ItemProperties.FirstOrDefault(p => p.Id == item.Id);
                        var itemName = itemProp?.Name ?? $"Item {item.Id}";

                        Results.Add(new FindResultItem(
                            itemName,
                            entity.PosX,
                            entity.PosY,
                            "TileEntity",
                            extraInfo));

                        if (Results.Count >= MaxResults) return;
                    }
                }
            }
        }
    }

    private void SearchMap(
        IReadOnlyList<int> selectedTileIds,
        IReadOnlyList<int> selectedWallIds,
        IReadOnlyList<(int TileId, Geometry.Vector2Short? UV)> selectedSprites,
        Vector2 spawn)
    {
        var world = _wvm.CurrentWorld;
        var tileIdSet = selectedTileIds.ToHashSet();
        var wallIdSet = selectedWallIds.ToHashSet();

        for (int x = 0; x < world.TilesWide && Results.Count < MaxResults; x++)
        {
            for (int y = 0; y < world.TilesHigh && Results.Count < MaxResults; y++)
            {
                var tile = world.Tiles[x, y];

                // Check tiles (non-framed blocks)
                if (tile.IsActive && tileIdSet.Contains(tile.Type))
                {
                    var tileName = GetTileName(tile.Type);
                    var extraInfo = CalculateDistance
                        ? $"Distance: {Math.Round(Vector2.Distance(spawn, new Vector2(x, y)))}"
                        : null;

                    Results.Add(new FindResultItem(tileName, x, y, "Tile", extraInfo));
                    if (Results.Count >= MaxResults) return;
                }

                // Check walls
                if (tile.Wall > 0 && wallIdSet.Contains(tile.Wall))
                {
                    var wallName = GetWallName(tile.Wall);
                    var extraInfo = CalculateDistance
                        ? $"Distance: {Math.Round(Vector2.Distance(spawn, new Vector2(x, y)))}"
                        : null;

                    Results.Add(new FindResultItem(wallName, x, y, "Wall", extraInfo));
                    if (Results.Count >= MaxResults) return;
                }

                // Check sprites (framed tiles with optional UV match)
                if (tile.IsActive)
                {
                    foreach (var (spriteId, uv) in selectedSprites)
                    {
                        if (tile.Type == spriteId)
                        {
                            // If UV is specified, must match
                            if (uv.HasValue)
                            {
                                var tileUV = tile.GetUV();
                                if (tileUV.X != uv.Value.X || tileUV.Y != uv.Value.Y)
                                    continue;
                            }

                            var spriteName = GetSpriteName(spriteId, uv);
                            var extraInfo = CalculateDistance
                                ? $"Distance: {Math.Round(Vector2.Distance(spawn, new Vector2(x, y)))}"
                                : null;

                            Results.Add(new FindResultItem(spriteName, x, y, "Sprite", extraInfo));
                            if (Results.Count >= MaxResults) return;
                        }
                    }
                }
            }
        }
    }

    private static string GetTileName(int tileId)
    {
        var prop = WorldConfiguration.TileProperties.FirstOrDefault(t => t.Id == tileId);
        return prop?.Name ?? $"Tile {tileId}";
    }

    private static string GetWallName(int wallId)
    {
        var prop = WorldConfiguration.WallProperties.FirstOrDefault(w => w.Id == wallId);
        return prop?.Name ?? $"Wall {wallId}";
    }

    private string GetSpriteName(int spriteId, Geometry.Vector2Short? uv)
    {
        var prop = WorldConfiguration.TileProperties.FirstOrDefault(t => t.Id == spriteId);
        if (prop == null) return $"Sprite {spriteId}";

        if (uv.HasValue && prop.Frames != null)
        {
            var frame = prop.Frames.FirstOrDefault(f => f.UV.X == uv.Value.X && f.UV.Y == uv.Value.Y);
            if (frame != null) return $"{prop.Name} ({frame})";
        }

        return prop.Name;
    }

    [ReactiveCommand]
    private void NavigateNext()
    {
        if (Results.Count == 0) return;
        CurrentResultIndex = (CurrentResultIndex + 1) % Results.Count;
        GoToCurrentResult();
    }

    [ReactiveCommand]
    private void NavigatePrevious()
    {
        if (Results.Count == 0) return;
        CurrentResultIndex = (CurrentResultIndex - 1 + Results.Count) % Results.Count;
        GoToCurrentResult();
    }

    [ReactiveCommand]
    private void GoToResult(FindResultItem? item)
    {
        if (item == null) return;

        var index = Results.IndexOf(item);
        if (index >= 0)
        {
            CurrentResultIndex = index;
            GoToCurrentResult();
        }
    }

    private void GoToCurrentResult()
    {
        if (CurrentResultIndex < 0 || CurrentResultIndex >= Results.Count) return;

        var result = Results[CurrentResultIndex];
        SelectedResult = result;

        // Update crosshair position
        CrosshairTileX = result.X;
        CrosshairTileY = result.Y;
        ShowCrosshair = true;

        // Navigate map to the result
        if (AutoZoomOnNavigate)
        {
            _wvm.ZoomFocus?.Invoke(result.X, result.Y);
        }
        else
        {
            _wvm.PanTo?.Invoke(result.X, result.Y);
        }

        this.RaisePropertyChanged(nameof(ResultSummary));
    }

    [ReactiveCommand]
    private void ClearAllSelections()
    {
        ItemPicker.ClearSelectionCommand.Execute(System.Reactive.Unit.Default).Subscribe();
        TilePicker.ClearSelectionCommand.Execute(System.Reactive.Unit.Default).Subscribe();
        WallPicker.ClearSelectionCommand.Execute(System.Reactive.Unit.Default).Subscribe();
        SpritePicker.ClearSelectionCommand.Execute(System.Reactive.Unit.Default).Subscribe();

        Results.Clear();
        CurrentResultIndex = -1;
        ShowCrosshair = false;

        this.RaisePropertyChanged(nameof(TotalSelectedCount));
        this.RaisePropertyChanged(nameof(SelectionSummary));
        this.RaisePropertyChanged(nameof(ResultSummary));
    }
}
