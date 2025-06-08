using System.Collections.ObjectModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using TEdit.Configuration;
using System;

namespace TEdit.ViewModel
{
    /// <summary>
    /// Provides static logic for managing all filter settings in TEdit's advanced filter popup,
    /// including filtered tiles, walls, liquids, and wires. 
    /// Maintains filter selection sets, filter modes (Hide, Grayscale), and background display modes.
    /// Used for both UI data binding and core filtering logic throughout TEdit.
    /// </summary>
    public static class FilterManager
    {
        public enum FilterMode { Hide, Grayscale }
        public enum BackgroundMode { Normal, Transparent, Custom }

        // When these sets are empty, it means "no filter" -> show every tile/wall/liquid
        private static readonly HashSet<int> _selectedTileIDs = [];
        private static readonly HashSet<int> _selectedWallIDs = [];
        private static readonly HashSet<LiquidType> _selectedLiquids = [];
        private static readonly HashSet<WireType> _selectedWires = [];
        public static ObservableCollection<string> SelectedTileNames { get; } = [];
        public static ObservableCollection<string> SelectedWallNames { get; } = [];
        public static ObservableCollection<string> SelectedLiquidNames { get; } = [];
        public static ObservableCollection<string> SelectedWireNames { get; } = [];
        public static IReadOnlyCollection<int> SelectedTileIDs => _selectedTileIDs;
        public static IReadOnlyCollection<int> SelectedWallIDs => _selectedWallIDs;

        public static FilterMode CurrentFilterMode { get; set; }         = FilterMode.Hide;
        public static Color FilterModeCustomColor { get; set; }          = Color.Transparent;
        public static BackgroundMode CurrentBackgroundMode { get; set; } = BackgroundMode.Normal;
        public static Color BackgroundModeCustomColor { get; set; }      = Color.Lime;

        /// <summary>
        /// Returns true if any tile‐filter is active.
        /// </summary>
        public static bool AnyFilterActive => SelectedTileIDs.Count > 0 || SelectedWallIDs.Count > 0 || SelectedLiquidNames.Count > 0 || _selectedWires.Count > 0;

        /// <summary>
        /// Returns true if tile‐filter is active and the tileId is not in the set.
        /// </summary>
        public static bool TileIsNotAllowed(int tileId) => (_selectedTileIDs.Count > 0 || AnyFilterActive) && !_selectedTileIDs.Contains(tileId);

        /// <summary>
        /// Returns true if wall‐filter is active and the wallId is not in the set.
        /// </summary>
        public static bool WallIsNotAllowed(int wallId) => (_selectedWallIDs.Count > 0 || AnyFilterActive) && !_selectedWallIDs.Contains(wallId);

        /// <summary>
        /// Returns true if no liquid‐filter is active and the liquidId is not in the set.
        /// </summary>
        public static bool LiquidIsNotAllowed(LiquidType liquidId) => (_selectedLiquids.Count > 0 || AnyFilterActive) && !_selectedLiquids.Contains(liquidId);

        /// <summary>
        /// Returns true if no wire‐filter is active and the wire is not in the set.
        /// </summary>
        public static bool WireIsNotAllowed(WireType wire) => (_selectedWires.Count > 0 || AnyFilterActive) && !_selectedWires.Contains(wire);

        /// <summary>
        /// Returns true if the wire is in the set.
        /// </summary>
        public static bool WireIsAllowed(WireType wire) => _selectedWires.Contains(wire);

        /// <summary>
        /// Clears everything – both tile, wall, liquid, and wire filters – and resets the modes to hide & normal.
        /// </summary>
        public static void ClearAll()
        {
            // _selectedTileIDs.Clear();
            // _selectedWallIDs.Clear();
            // _selectedLiquids.Clear();
            // _selectedWires.Clear();

            // Clear all filters.
            // Also clear the "names" lists to prevent UI resync issues.
            FilterManager.SelectedTileNames.Clear();
            FilterManager.SelectedWallNames.Clear();
            FilterManager.SelectedLiquidNames.Clear();
            FilterManager.SelectedWireNames.Clear();

            FilterManager.ClearWallFilters();
            FilterManager.ClearTileFilters();
            FilterManager.ClearLiquidFilters();
            FilterManager.ClearWireFilters();

            // Reset the filter modes.
            CurrentFilterMode = FilterManager.FilterMode.Hide;
            CurrentBackgroundMode = FilterManager.BackgroundMode.Normal;
        }

        #region Tile Filter Methods

        /// <summary>
        /// Add a tile ID to the filter (i.e. show this ID).
        /// Air has an id of -1, so tiles have an >= -1 rule.
        /// </summary>
        public static void AddTileFilter(int tileId)
        {
            if (tileId >= -1 && _selectedTileIDs.Add(tileId))
            {
                // no direct UI work here—UI binds to SelectedTileNames
            }
        }

        /// <summary>
        /// Remove a tile ID from the filter (i.e. stop showing this ID).
        /// </summary>
        public static void RemoveTileFilter(int tileId) => _selectedTileIDs.Remove(tileId);

        /// <summary>
        /// Clear all tile filters (i.e. show every tile).
        /// </summary>
        public static void ClearTileFilters() => _selectedTileIDs.Clear();

        #endregion

        #region Wall Filter Methods

        /// <summary>
        /// Add a wall ID to the filter.
        /// </summary>
        public static void AddWallFilter(int wallId)
        {
            if (wallId >= 0 && _selectedWallIDs.Add(wallId))
            {
                // no direct UI work here—UI binds to SelectedWallNames
            }
        }

        /// <summary>
        /// Remove a wall ID from the filter.
        /// </summary>
        public static void RemoveWallFilter(int wallId) => _selectedWallIDs.Remove(wallId);

        /// <summary>
        /// Clear all wall filters.
        /// </summary>
        public static void ClearWallFilters() => _selectedWallIDs.Clear();

        #endregion

        #region Liquid Filter Methods

        // --- Liquid filter methods ---
        /// <summary>
        /// Add a liquid ID to the filter.
        /// </summary>
        public static void AddLiquidFilter(LiquidType liq)
        {
            if (_selectedLiquids.Add(liq)) { }
        }

        /// <summary>
        /// Remove a liquid ID from the filter.
        /// </summary>
        public static void RemoveLiquidFilter(LiquidType liq) => _selectedLiquids.Remove(liq);

        /// <summary>
        /// Clear all liquid filters.
        /// </summary>
        public static void ClearLiquidFilters() => _selectedLiquids.Clear();

        #endregion

        #region Wire Filter Methods

        // ------- Wire filter (new) -------
        [Flags]
        public enum WireType : byte
        {
            Red = 1 << 0,
            Blue = 1 << 1,
            Green = 1 << 2,
            Yellow = 1 << 3,
        }
        public static void AddWireFilter(WireType wire) { _selectedWires.Add(wire); }
        public static void RemoveWireFilter(WireType wire) { _selectedWires.Remove(wire); }
        public static void ClearWireFilters()
        {
            _selectedWires.Clear();
            SelectedWireNames.Clear();
        }

        #endregion
    }

    /// <summary>
    /// Represents an item (such as a tile, wall, liquid, or wire) that can be filtered
    /// in the TEdit filter popup window. Each item has an ID, a display name, and a
    /// check state indicating whether it is included in the filter. Implements
    /// INotifyPropertyChanged so that WPF data binding reflects changes in checkboxes
    /// in the UI.
    /// 
    /// Example usage:
    /// - Used as the data model for each checkbox row in the filter lists.
    /// - Supports binding to the IsChecked property to track filter selections.
    /// </summary>
    public class FilterCheckItem : INotifyPropertyChanged
    {
        public string Name { get; }
        public int Id { get; }
        private bool _isChecked;

        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(nameof(IsChecked)); }
        }

        public FilterCheckItem(int id, string name, bool isChecked = false)
        {
            Id = id;
            Name = name;
            IsChecked = isChecked;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    /// <summary>
    /// Converts between an enum value and a boolean for use with RadioButton bindings in WPF.
    /// When bound to a RadioButton, it returns true if the enum's value matches the button's ConverterParameter,
    /// allowing for simple two-way binding between a group of RadioButtons and an enum property.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value?.ToString() == parameter?.ToString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? Enum.Parse(targetType, parameter.ToString()) : Binding.DoNothing;
    }
}
