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
            if (ItemPicker.SelectedCount > 0) parts.Add(string.Format(Properties.Language.find_selected_items, ItemPicker.SelectedCount));
            if (TilePicker.SelectedCount > 0) parts.Add(string.Format(Properties.Language.find_selected_tiles, TilePicker.SelectedCount));
            if (WallPicker.SelectedCount > 0) parts.Add(string.Format(Properties.Language.find_selected_walls, WallPicker.SelectedCount));
            if (SpritePicker.SelectedCount > 0) parts.Add(string.Format(Properties.Language.find_selected_sprites, SpritePicker.SelectedCount));
            return parts.Count > 0 ? string.Format(Properties.Language.find_selection_summary, string.Join(", ", parts)) : Properties.Language.find_no_selection;
        }
    }

    public string ResultSummary => Results.Count > 0
        ? string.Format(Properties.Language.find_result_summary, CurrentResultIndex + 1, Results.Count)
        : Properties.Language.find_no_results;

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

    // All matching tile positions for the find overlay (includes every tile of multi-tile sprites)
    private readonly List<(int X, int Y)> _overlayPositions = [];

    [ReactiveCommand]
    private void ExecuteSearch()
    {
        if (_wvm.CurrentWorld == null)
        {
            _ = App.DialogService.ShowWarningAsync(Properties.Language.find_dialog_title, Properties.Language.find_no_world_loaded);
            return;
        }

        Results.Clear();
        _overlayPositions.Clear();
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

        // Activate find overlay — use ALL matched positions (entire sprites highlighted),
        // even though Results list is deduped to anchors only
        FilterManager.SetFindResults(_overlayPositions);
        _wvm.RebuildFilterOverlay();

        // Navigate to first result if found
        if (Results.Count > 0)
        {
            CurrentResultIndex = 0;
            GoToCurrentResult();
        }

        this.RaisePropertyChanged(nameof(ResultSummary));
    }

    /// <summary>
    /// Adds all tile positions occupied by the sprite at anchor (ax, ay) to the overlay.
    /// </summary>
    private void AddSpriteToOverlay(World world, int ax, int ay)
    {
        var tile = world.Tiles[ax, ay];
        if (!tile.IsActive || tile.Type >= WorldConfiguration.TileProperties.Count)
        {
            _overlayPositions.Add((ax, ay));
            return;
        }

        var prop = WorldConfiguration.TileProperties[tile.Type];
        var size = prop.GetFrameSize(tile.V);

        for (int dx = 0; dx < size.X; dx++)
        {
            for (int dy = 0; dy < size.Y; dy++)
            {
                int px = ax + dx, py = ay + dy;
                if (px < world.TilesWide && py < world.TilesHigh)
                    _overlayPositions.Add((px, py));
            }
        }
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
                        ? string.Format(Properties.Language.find_distance_label, Math.Round(Vector2.Distance(spawn, new Vector2(chest.X, chest.Y))))
                        : null;

                    AddSpriteToOverlay(world, chest.X, chest.Y);
                    Results.Add(new FindResultItem(
                        item.GetName(),
                        chest.X,
                        chest.Y,
                        Properties.Language.find_result_type_chest,
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
                            ? string.Format(Properties.Language.find_distance_label, Math.Round(Vector2.Distance(spawn, new Vector2(entity.PosX, entity.PosY))))
                            : null;

                        // Get item name from WorldConfiguration
                        var itemProp = WorldConfiguration.ItemProperties.FirstOrDefault(p => p.Id == item.Id);
                        var itemName = itemProp?.Name ?? $"Item {item.Id}";

                        AddSpriteToOverlay(world, entity.PosX, entity.PosY);
                        Results.Add(new FindResultItem(
                            itemName,
                            entity.PosX,
                            entity.PosY,
                            Properties.Language.find_result_type_tile_entity,
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

        // Track which sprite anchors we've already added to Results (dedup)
        var seenSpriteAnchors = new HashSet<long>();

        for (int x = 0; x < world.TilesWide && Results.Count < MaxResults; x++)
        {
            for (int y = 0; y < world.TilesHigh && Results.Count < MaxResults; y++)
            {
                var tile = world.Tiles[x, y];

                // Check tiles (non-framed blocks)
                if (tile.IsActive && tileIdSet.Contains(tile.Type))
                {
                    _overlayPositions.Add((x, y));

                    var tileName = GetTileName(tile.Type);
                    var extraInfo = CalculateDistance
                        ? string.Format(Properties.Language.find_distance_label, Math.Round(Vector2.Distance(spawn, new Vector2(x, y))))
                        : null;

                    Results.Add(new FindResultItem(tileName, x, y, Properties.Language.find_result_type_tile, extraInfo));
                    if (Results.Count >= MaxResults) return;
                }

                // Check walls
                if (tile.Wall > 0 && wallIdSet.Contains(tile.Wall))
                {
                    _overlayPositions.Add((x, y));

                    var wallName = GetWallName(tile.Wall);
                    var extraInfo = CalculateDistance
                        ? string.Format(Properties.Language.find_distance_label, Math.Round(Vector2.Distance(spawn, new Vector2(x, y))))
                        : null;

                    Results.Add(new FindResultItem(wallName, x, y, Properties.Language.find_result_type_wall, extraInfo));
                    if (Results.Count >= MaxResults) return;
                }

                // Check sprites — highlight ALL matching tiles, but dedupe Results to anchors
                if (tile.IsActive && selectedSprites.Count > 0)
                {
                    foreach (var (spriteId, uv) in selectedSprites)
                    {
                        if (tile.Type != spriteId) continue;

                        // If UV is specified, must match
                        if (uv.HasValue)
                        {
                            var tileUV = tile.GetUV();
                            if (tileUV.X != uv.Value.X || tileUV.Y != uv.Value.Y)
                                continue;
                        }

                        // Add every matching tile to overlay (entire sprite highlights)
                        _overlayPositions.Add((x, y));

                        // Only add one Result entry per sprite instance (anchor)
                        var anchor = world.GetAnchor(x, y);
                        long anchorKey = (long)anchor.Y * 65536L + anchor.X;
                        if (!seenSpriteAnchors.Add(anchorKey)) continue;

                        var spriteName = GetSpriteName(spriteId, uv);
                        var extraInfo = CalculateDistance
                            ? string.Format(Properties.Language.find_distance_label, Math.Round(Vector2.Distance(spawn, new Vector2(anchor.X, anchor.Y))))
                            : null;

                        Results.Add(new FindResultItem(spriteName, anchor.X, anchor.Y, Properties.Language.find_result_type_sprite, extraInfo));
                        if (Results.Count >= MaxResults) return;
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

        // Deactivate find overlay
        FilterManager.ClearFindResults();
        _wvm.RebuildFilterOverlay();

        this.RaisePropertyChanged(nameof(TotalSelectedCount));
        this.RaisePropertyChanged(nameof(SelectionSummary));
        this.RaisePropertyChanged(nameof(ResultSummary));
    }
}
