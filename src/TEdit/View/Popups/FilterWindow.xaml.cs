using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using TEdit.Configuration; // For WorldConfiguration.TileBricks, WallProperties, LiquidType.
using TEdit.ViewModel;     // For FilterItem, FilterManager, etc.
using System.Windows;
using System.Linq;
using System;
using System.Collections.Generic;

namespace TEdit.View.Popups
{
    public partial class FilterWindow : Window, INotifyPropertyChanged
    {
        // WVM
        private readonly WorldViewModel _wvm;

        // Tiles
        public ObservableCollection<FilterCheckItem> TileItems { get; } = [];
        public ObservableCollection<FilterCheckItem> FilteredTileItems { get; } = [];
        private string _tileSearchText;
        public string TileSearchText { get => _tileSearchText; set { _tileSearchText = value; OnPropertyChanged(nameof(TileSearchText)); FilterTileItems(); } }

        // Walls
        public ObservableCollection<FilterCheckItem> WallItems { get; } = [];
        public ObservableCollection<FilterCheckItem> FilteredWallItems { get; } = [];
        private string _wallSearchText;
        public string WallSearchText { get => _wallSearchText; set { _wallSearchText = value; OnPropertyChanged(nameof(WallSearchText)); FilterWallItems(); } }

        // Liquids
        public ObservableCollection<FilterCheckItem> LiquidItems { get; } = [];
        public ObservableCollection<FilterCheckItem> FilteredLiquidItems { get; } = [];
        private string _liquidSearchText;
        public string LiquidSearchText { get => _liquidSearchText; set { _liquidSearchText = value; OnPropertyChanged(nameof(LiquidSearchText)); FilterLiquidItems(); } }
        private static string TranslateLiquidName(LiquidType type)
        {
            switch (type)
            {
                case LiquidType.Water:
                    return "水";
                case LiquidType.Lava:
                    return "岩浆";
                case LiquidType.Honey:
                    return "蜂蜜";
                case LiquidType.Shimmer:
                    return "微光";
                default:
                    return "未知液体";
            }
        }

        // Wires
        public ObservableCollection<FilterCheckItem> WireItems { get; } = [];
        public ObservableCollection<FilterCheckItem> FilteredWireItems { get; } = [];
        private string _wireSearchText;
        public string WireSearchText { get => _wireSearchText; set { _wireSearchText = value; OnPropertyChanged(nameof(WireSearchText)); FilterWireItems(); } }

        // Sprites
        public ObservableCollection<FilterCheckItem> SpriteItems { get; } = [];
        public ObservableCollection<FilterCheckItem> FilteredSpriteItems { get; } = [];
        private string _spriteSearchText;
        public string SpriteSearchText { get => _spriteSearchText; set { _spriteSearchText = value; OnPropertyChanged(nameof(SpriteSearchText)); FilterSpriteItems(); } }

        // Checkboxes
        private bool _isFilterClipboardEnabled;
        public bool IsFilterClipboardEnabled { get => _isFilterClipboardEnabled; set { _isFilterClipboardEnabled = value; OnPropertyChanged(nameof(IsFilterClipboardEnabled)); } }

        // RadioButtons
        private FilterManager.FilterMode _pendingFilterMode;
        private FilterManager.BackgroundMode _pendingBackgroundMode;

        public FilterManager.FilterMode PendingFilterMode { get => _pendingFilterMode; set { _pendingFilterMode = value; OnPropertyChanged(nameof(PendingFilterMode)); } }
        public FilterManager.FilterMode CurrentFilterMode { get => FilterManager.CurrentFilterMode; set { if (FilterManager.CurrentFilterMode != value) { FilterManager.CurrentFilterMode = value; OnPropertyChanged(nameof(CurrentFilterMode)); } } }
        public FilterManager.BackgroundMode PendingBackgroundMode { get => _pendingBackgroundMode; set { _pendingBackgroundMode = value; OnPropertyChanged(nameof(PendingBackgroundMode)); } }
        public FilterManager.BackgroundMode CurrentBackgroundMode { get => FilterManager.CurrentBackgroundMode; set { if (FilterManager.CurrentBackgroundMode != value) { FilterManager.CurrentBackgroundMode = value; OnPropertyChanged(nameof(CurrentBackgroundMode)); } } }

        // This will hold the selected color for custom modes.
        private Color _customModeColor       = Colors.Transparent;
        private Color _customBackgroundColor = Colors.Lime;
        public Color CustomModeColor { get => _customModeColor; set { if (_customModeColor != value) { _customModeColor = value; OnPropertyChanged(nameof(CustomModeColor)); } } }
        public Color CustomBackgroundColor { get => _customBackgroundColor; set { if (_customBackgroundColor != value) { _customBackgroundColor = value; OnPropertyChanged(nameof(CustomBackgroundColor)); } } }

        // Helper for quickly converting Windows.Media.Color to Xna.Framework.Color.
        public static Microsoft.Xna.Framework.Color ToXnaColor(System.Windows.Media.Color c) => new(c.R, c.G, c.B, c.A);
        public static System.Windows.Media.Color FromXnaColor(Microsoft.Xna.Framework.Color c) => System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        public static Microsoft.Xna.Framework.Color ToGrayscale(Microsoft.Xna.Framework.Color c)
        {
            // Calculate luminance
            byte gray = (byte)(c.R * 0.299 + c.G * 0.587 + c.B * 0.114);
            return new Microsoft.Xna.Framework.Color(gray, gray, gray, c.A);
        }

        public FilterWindow(WorldViewModel worldViewModel)
        {
            InitializeComponent();
            _wvm = worldViewModel;
            DataContext = this;

            #region Populate Tiles

            // Populate tile items.
            TileItems.Clear();
            foreach (var tile in TEdit.Configuration.WorldConfiguration.TileBricks)
                TileItems.Add(new FilterCheckItem(tile.Id, tile.Name, false));
            foreach (var item in TileItems)   // Populate IsChecked with previous saved values.
                item.IsChecked = FilterManager.SelectedTileIDs.Contains(item.Id);
            FilterTileItems();

            #endregion

            #region Populate Walls

            // Populate wall items.
            WallItems.Clear();
            foreach (var wall in TEdit.Configuration.WorldConfiguration.WallProperties)
                WallItems.Add(new FilterCheckItem(wall.Id, wall.Name, false));
            foreach (var item in WallItems)   // Populate IsChecked with previous saved values.
                item.IsChecked = FilterManager.SelectedWallIDs.Contains(item.Id);
            FilterWallItems();

            #endregion

            #region Populate Liquids

            // Populate liquid items.
            LiquidItems.Clear();
            foreach (var val in Enum.GetValues<LiquidType>())
            {
                int v = Convert.ToInt32(val);
                if (v != 0) // Exclude None
//                    LiquidItems.Add(new FilterCheckItem(v, val.ToString(), false));
                    LiquidItems.Add(new FilterCheckItem(v, TranslateLiquidName(val), false));
            }
            foreach (var item in LiquidItems) // Populate IsChecked with previous saved values.
                item.IsChecked = FilterManager.SelectedLiquidNames.Contains(item.Name); // or use IDs.
            FilterLiquidItems();

            #endregion

            #region Populate Wires

            // Populate wires (red, blue, green, yellow).
            WireItems.Clear();
            WireItems.Add(new FilterCheckItem((int)FilterManager.WireType.Red, "红电线", false));       // 1. _wvm.ShowXXXWires
            WireItems.Add(new FilterCheckItem((int)FilterManager.WireType.Blue, "蓝电线", false));     // 2. 
            WireItems.Add(new FilterCheckItem((int)FilterManager.WireType.Green, "绿电线", false));   // 4.
            WireItems.Add(new FilterCheckItem((int)FilterManager.WireType.Yellow, "黄电线", false)); // 8.
            foreach (var item in WireItems)   // Populate IsChecked with previous saved values.
                item.IsChecked = FilterManager.WireIsAllowed((FilterManager.WireType)item.Id);
            FilterWireItems();

            #endregion

            #region Populate Sprites

            // Get all tile IDs that are considered "brick" tiles (from TileBricks).
            // We'll use these to exclude tiles that are already in the main tile list.
            var tileIds = TEdit.Configuration.WorldConfiguration.TileBricks.Select(tb => tb.Id).ToHashSet();

            // Get all unique sprite IDs for sprites whose base tile type isn't a main tile (i.e., not in TileBricks).
            var spriteIds = WorldConfiguration.Sprites2
                .Select(s => s.Tile)                // Get the tile ID each sprite is based on.
                .Distinct()                         // Only consider each ID once.
                .Where(id => !tileIds.Contains(id)) // Exclude IDs that are already in TileBricks (plain tiles).
                .ToList();                          // Convert to list.

            // Build a display list of sprites for UI (ID and name), only including those not already in the tile list.
            var spriteDisplayList = spriteIds
               .Select(id => new {
                   Id = id,
                   WorldConfiguration.Sprites2.First(s => s.Tile == id).Name
               })
               .ToList();

            // Populate sprite items.
            SpriteItems.Clear();
            foreach (var sprite in spriteDisplayList)
                SpriteItems.Add(new FilterCheckItem(sprite.Id, sprite.Name, false));
            foreach (var item in SpriteItems)   // Populate IsChecked with previous saved values.
                item.IsChecked = FilterManager.SelectedSpriteIDs.Contains(item.Id);
            FilterSpriteItems();

            #endregion

            // Synchronize color fields with FilterManager for persistence.
            CustomModeColor = FromXnaColor(FilterManager.FilterModeCustomColor);
            CustomBackgroundColor = FromXnaColor(FilterManager.BackgroundModeCustomColor);

            // Sync and toggle filter clipboard checkbox with the actual value.
            IsFilterClipboardEnabled = FilterManager.FilterClipboard;

            // Sync pending filter / background modes with the actual value.
            PendingFilterMode = FilterManager.CurrentFilterMode;
            PendingBackgroundMode = FilterManager.CurrentBackgroundMode;
        }

        // Filtering logic for each section.
        private void FilterTileItems()
        {
            FilteredTileItems.Clear();
            foreach (var item in TileItems.Where(i => string.IsNullOrWhiteSpace(TileSearchText) || i.Name.Contains(TileSearchText, StringComparison.CurrentCultureIgnoreCase)))
                FilteredTileItems.Add(item);
        }
        private void FilterWallItems()
        {
            FilteredWallItems.Clear();
            foreach (var item in WallItems.Where(i => string.IsNullOrWhiteSpace(WallSearchText) || i.Name.Contains(WallSearchText, StringComparison.CurrentCultureIgnoreCase)))
                FilteredWallItems.Add(item);
        }
        private void FilterLiquidItems()
        {
            FilteredLiquidItems.Clear();
            foreach (var item in LiquidItems.Where(i => string.IsNullOrWhiteSpace(LiquidSearchText) || i.Name.Contains(LiquidSearchText, StringComparison.CurrentCultureIgnoreCase)))
                FilteredLiquidItems.Add(item);
        }
        private void FilterWireItems()
        {
            FilteredWireItems.Clear();
            foreach (var item in WireItems.Where(i => string.IsNullOrWhiteSpace(WireSearchText) || i.Name.Contains(WireSearchText, StringComparison.CurrentCultureIgnoreCase)))
                FilteredWireItems.Add(item);
        }
        private void FilterSpriteItems()
        {
            FilteredSpriteItems.Clear();
            foreach (var item in SpriteItems.Where(i => string.IsNullOrWhiteSpace(SpriteSearchText) || i.Name.Contains(SpriteSearchText, StringComparison.CurrentCultureIgnoreCase)))
                FilteredSpriteItems.Add(item);
        }

        // Check All / Uncheck All.
        private void TileCheckAll_Click(object sender, RoutedEventArgs e) => FilteredTileItems.ToList().ForEach(i => i.IsChecked = true);
        private void TileUncheckAll_Click(object sender, RoutedEventArgs e) => FilteredTileItems.ToList().ForEach(i => i.IsChecked = false);

        private void WallCheckAll_Click(object sender, RoutedEventArgs e) => FilteredWallItems.ToList().ForEach(i => i.IsChecked = true);
        private void WallUncheckAll_Click(object sender, RoutedEventArgs e) => FilteredWallItems.ToList().ForEach(i => i.IsChecked = false);

        private void LiquidCheckAll_Click(object sender, RoutedEventArgs e) => FilteredLiquidItems.ToList().ForEach(i => i.IsChecked = true);
        private void LiquidUncheckAll_Click(object sender, RoutedEventArgs e) => FilteredLiquidItems.ToList().ForEach(i => i.IsChecked = false);

        private void WireCheckAll_Click(object sender, RoutedEventArgs e) => FilteredWireItems.ToList().ForEach(i => i.IsChecked = true);
        private void WireUncheckAll_Click(object sender, RoutedEventArgs e) => FilteredWireItems.ToList().ForEach(i => i.IsChecked = false);

        private void SpriteCheckAll_Click(object sender, RoutedEventArgs e) => FilteredSpriteItems.ToList().ForEach(i => i.IsChecked = true);
        private void SpriteUncheckAll_Click(object sender, RoutedEventArgs e) => FilteredSpriteItems.ToList().ForEach(i => i.IsChecked = false);

        // Final buttons.
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Disable_Click(object sender, RoutedEventArgs e)
        {
            // Save the existing filter modes.
            FilterManager.CurrentFilterMode = PendingFilterMode;
            FilterManager.CurrentBackgroundMode = PendingBackgroundMode;

            // Uncheck all checkboxes.
            foreach (var item in TileItems) item.IsChecked = false;
            foreach (var item in WallItems) item.IsChecked = false;
            foreach (var item in LiquidItems) item.IsChecked = false;
            foreach (var item in WireItems) item.IsChecked = false;

            // Clear all filters.
            FilterManager.ClearAll();

            // Clear the grayscale cache.
            GrayscaleManager.GrayscaleCache.Clear();

            // Schedule RedrawMap to run after this method (and window) closes.
            // This eliminates the awkward wait time for redraw before closing.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                RedrawMap(true);
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

            DialogResult = true;
            Close();
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            // Clear the grayscale cache.
            GrayscaleManager.GrayscaleCache.Clear();

            // Tiles.
            FilterManager.ClearTileFilters();
            FilterManager.SelectedTileNames.Clear();
            foreach (var tile in TileItems.Where(x => x.IsChecked))
            {
                FilterManager.AddTileFilter(tile.Id);
                FilterManager.SelectedTileNames.Add(tile.Name);
            }

            // Walls.
            FilterManager.ClearWallFilters();
            FilterManager.SelectedWallNames.Clear();
            foreach (var wall in WallItems.Where(x => x.IsChecked))
            {
                FilterManager.AddWallFilter(wall.Id);
                FilterManager.SelectedWallNames.Add(wall.Name);
            }

            // Liquids.
            FilterManager.ClearLiquidFilters();
            FilterManager.SelectedLiquidNames.Clear();
            foreach (var liquid in LiquidItems.Where(x => x.IsChecked))
            {
                FilterManager.AddLiquidFilter((LiquidType)liquid.Id);
                FilterManager.SelectedLiquidNames.Add(liquid.Name);
            }

            // Wires.
            FilterManager.ClearWireFilters();
            FilterManager.SelectedWireNames.Clear();
            foreach (var wire in WireItems.Where(x => x.IsChecked))
            {
                FilterManager.AddWireFilter((FilterManager.WireType)wire.Id);
                FilterManager.SelectedWireNames.Add(wire.Name);
            }

            #region Toggle UI Wire Layers

            // Toggle on each UI wire selection if hidden and choosen.
            if (!_wvm.ShowRedWires    && FilterManager.WireIsAllowed(FilterManager.WireType.Red)) _wvm.ShowRedWires = true;
            if (!_wvm.ShowBlueWires   && FilterManager.WireIsAllowed(FilterManager.WireType.Blue)) _wvm.ShowBlueWires = true;
            if (!_wvm.ShowGreenWires  && FilterManager.WireIsAllowed(FilterManager.WireType.Green)) _wvm.ShowGreenWires = true;
            if (!_wvm.ShowYellowWires && FilterManager.WireIsAllowed(FilterManager.WireType.Yellow)) _wvm.ShowYellowWires = true;

            #region REMOVED: Toggle UI wire layers based on the wire selections.
            /*
            // Wires.
            FilterManager.ClearWireFilters();
            FilterManager.SelectedWireNames.Clear();
            foreach (var wire in WireItems.Where(x => x.IsChecked))
            {
                switch (wire.Name)
                {
                    case "Red Wire":
                        _wvm.ShowRedWires = true;
                        break;
                    case "Blue Wire":
                        _wvm.ShowBlueWires = true;
                        break;
                    case "Green Wire":
                        _wvm.ShowGreenWires = true;
                        break;
                    case "Yellow Wire":
                        _wvm.ShowYellowWires = true;
                        break;
                }
                FilterManager.AddWireFilter((FilterManager.WireType)wire.Id);
                FilterManager.SelectedWireNames.Add(wire.Name);
            }

            // For *unchecked* wires, set _wvm.ShowXXXWires to false.
            foreach (var wire in WireItems.Where(x => !x.IsChecked))
            {
                switch (wire.Name)
                {
                    case "Red Wire":
                        _wvm.ShowRedWires = false;
                        break;
                    case "Blue Wire":
                        _wvm.ShowBlueWires = false;
                        break;
                    case "Green Wire":
                        _wvm.ShowGreenWires = false;
                        break;
                    case "Yellow Wire":
                        _wvm.ShowYellowWires = false;
                        break;
                }
            }
            */
            #endregion

            #endregion

            // Sprites.
            FilterManager.ClearSpriteFilters();
            FilterManager.SelectedSpriteNames.Clear();
            foreach (var sprite in SpriteItems.Where(x => x.IsChecked))
            {
                FilterManager.AddSpriteFilter(sprite.Id);
                FilterManager.SelectedSpriteNames.Add(sprite.Name);
            }

            // Filter mode.
            FilterManager.CurrentFilterMode = PendingFilterMode;
            RadioButton[] filterModeRadios = [HideRadio, GrayscaleRadio];
            foreach (var rb in filterModeRadios)
            {
                if (rb.IsChecked == true && Enum.TryParse(typeof(FilterManager.FilterMode), rb.Tag?.ToString(), out var mode))
                {
                    FilterManager.CurrentFilterMode = (FilterManager.FilterMode)mode;
                    break;
                }
            }
            FilterManager.FilterModeCustomColor = ToXnaColor(CustomModeColor);

            // Background mode.
            FilterManager.CurrentBackgroundMode = PendingBackgroundMode;
            RadioButton[] backgroundModeRadios = [NormalRadio, CustomRadio];
            foreach (var rb in backgroundModeRadios)
            {
                if (rb.IsChecked == true && Enum.TryParse(typeof(FilterManager.BackgroundMode), rb.Tag?.ToString(), out var mode))
                {
                    FilterManager.CurrentBackgroundMode = (FilterManager.BackgroundMode)mode;
                    break;
                }
            }
            FilterManager.BackgroundModeCustomColor = ToXnaColor(CustomBackgroundColor);

            // Update the filter clipboard preferences.
            FilterManager.FilterClipboard = IsFilterClipboardEnabled;

            // Schedule RedrawMap to run after this method (and window) closes.
            // This eliminates the akward wait time for redraw before closing.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                RedrawMap();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Forces a complete re-render of the map display in TEdit to reflect current filters or edits.
        /// </summary>
        private void RedrawMap(bool loadDefualt = false)
        {
            var main = (MainWindow)Application.Current.MainWindow;
            
            // Reload the map without any filtration applied.
            if (loadDefualt)
            {
                main.MapView.DrawTileWalls();
                main.MapView.DrawTileTextures();

                _wvm.UpdateRenderWorld();                                             // Re-render map.
                _wvm.MinimapImage = Render.RenderMiniMap.Render(_wvm.CurrentWorld);   // Update Minimap.
                return;
            }

            main.MapView.DrawTileWallsFiltered();
            main.MapView.DrawTileTexturesFiltered();

            _wvm.UpdateRenderWorldUsingFilter();                                      // Re-render map.
            _wvm.MinimapImage = Render.RenderMiniMap.Render(_wvm.CurrentWorld, true); // Update Minimap.
        }

        // Custom color picker logic for the map backdrop.
        private void PickCustomColor_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.ColorDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CustomBackgroundColor = Color.FromArgb(dlg.Color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B);
                FilterManager.BackgroundModeCustomColor = ToXnaColor(CustomBackgroundColor);
            }
        }

        // INotifyPropertyChanged.
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
