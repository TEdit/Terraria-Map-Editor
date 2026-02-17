using System.Collections.ObjectModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Globalization;
using System.Windows.Data;
using TEdit.Terraria;
using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace TEdit.ViewModel
{
    /// <summary>
    /// Provides static logic for managing all filter settings in TEdit's advanced filter popup,
    /// including filtered tiles, walls, liquids, and wires.
    /// Maintains filter selection sets, filter modes (Hide, Darken), and background display modes.
    /// Used for both UI data binding and core filtering logic throughout TEdit.
    /// </summary>
    public static class FilterManager
    {
        public enum FilterMode { Hide, Darken }
        public enum BackgroundMode { Normal, Transparent, Custom }

        /// <summary>Monotonically increasing counter. Bumped on every filter mutation so the renderer knows when to rebuild the overlay.</summary>
        public static int Revision { get; private set; }

        // When these sets are empty, it means "no filter" -> show every tile/wall/liquid
        private static readonly HashSet<int> _selectedTileIDs = [];
        private static readonly HashSet<int> _selectedWallIDs = [];
        private static readonly HashSet<LiquidType> _selectedLiquids = [];
        private static readonly HashSet<WireType> _selectedWires = [];
        private static readonly HashSet<int> _selectedSpriteIDs = [];
        public static ObservableCollection<string> SelectedTileNames { get; } = [];
        public static ObservableCollection<string> SelectedWallNames { get; } = [];
        public static ObservableCollection<string> SelectedLiquidNames { get; } = [];
        public static ObservableCollection<string> SelectedWireNames { get; } = [];
        public static ObservableCollection<string> SelectedSpriteNames { get; } = [];
        public static IReadOnlyCollection<int> SelectedTileIDs => _selectedTileIDs;
        public static IReadOnlyCollection<int> SelectedWallIDs => _selectedWallIDs;
        public static IReadOnlyCollection<int> SelectedSpriteIDs => _selectedSpriteIDs;

        private static FilterMode _currentFilterMode = FilterMode.Darken;
        public static FilterMode CurrentFilterMode
        {
            get => _currentFilterMode;
            set { if (_currentFilterMode != value) { _currentFilterMode = value; Revision++; } }
        }

        public static Color FilterModeCustomColor { get; set; }          = Color.Transparent;

        private static BackgroundMode _currentBackgroundMode = BackgroundMode.Normal;
        public static BackgroundMode CurrentBackgroundMode
        {
            get => _currentBackgroundMode;
            set { if (_currentBackgroundMode != value) { _currentBackgroundMode = value; Revision++; } }
        }

        public static Color BackgroundModeCustomColor { get; set; }      = Color.Lime;

        private static float _darkenAmount = 0.6f;
        /// <summary>Darken overlay opacity (0.0 = invisible, 1.0 = fully black). Default 0.6.</summary>
        public static float DarkenAmount
        {
            get => _darkenAmount;
            set { if (_darkenAmount != value) { _darkenAmount = value; Revision++; } }
        }

        public static bool FilterClipboard { get; set; } = false;

        /// <summary>
        /// Returns true if any tile‐filter is active.
        /// </summary>
        public static bool AnyFilterActive => SelectedTileNames.Count > 0 || SelectedWallNames.Count > 0 || SelectedLiquidNames.Count > 0 || SelectedWireNames.Count > 0 || SelectedSpriteNames.Count > 0;

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
        /// Returns true if sprite‐filter is active and the spriteId is not in the set.
        /// </summary>
        public static bool SpriteIsNotAllowed(int spriteId) => (_selectedSpriteIDs.Count > 0 || AnyFilterActive) && !_selectedSpriteIDs.Contains(spriteId);

        /// <summary>
        /// Clears everything – both tile, wall, liquid, and wire filters – and resets the modes to hide & normal.
        /// </summary>
        public static void ClearAll()
        {
            // Clear all filters.
            _selectedTileIDs.Clear();
            _selectedWallIDs.Clear();
            _selectedLiquids.Clear();
            _selectedWires.Clear();
            _selectedSpriteIDs.Clear();

            // Also clear the "names" lists to prevent UI resync issues.
            FilterManager.SelectedTileNames.Clear();
            FilterManager.SelectedWallNames.Clear();
            FilterManager.SelectedLiquidNames.Clear();
            FilterManager.SelectedWireNames.Clear();
            FilterManager.SelectedSpriteNames.Clear();

            FilterManager.ClearWallFilters();
            FilterManager.ClearTileFilters();
            FilterManager.ClearLiquidFilters();
            FilterManager.ClearWireFilters();
            FilterManager.ClearSpriteFilters();

            // Reset the filter modes.
            CurrentFilterMode = FilterManager.FilterMode.Hide;
            CurrentBackgroundMode = FilterManager.BackgroundMode.Normal;

            // Reset the clipboard settings.
            FilterClipboard = false;

            Revision++;
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
                Revision++;
            }
        }

        /// <summary>
        /// Remove a tile ID from the filter (i.e. stop showing this ID).
        /// </summary>
        public static void RemoveTileFilter(int tileId) { if (_selectedTileIDs.Remove(tileId)) Revision++; }

        /// <summary>
        /// Clear all tile filters (i.e. show every tile).
        /// </summary>
        public static void ClearTileFilters() { _selectedTileIDs.Clear(); Revision++; }

        #endregion

        #region Wall Filter Methods

        /// <summary>
        /// Add a wall ID to the filter.
        /// </summary>
        public static void AddWallFilter(int wallId)
        {
            if (wallId >= 0 && _selectedWallIDs.Add(wallId))
            {
                Revision++;
            }
        }

        /// <summary>
        /// Remove a wall ID from the filter.
        /// </summary>
        public static void RemoveWallFilter(int wallId) { if (_selectedWallIDs.Remove(wallId)) Revision++; }

        /// <summary>
        /// Clear all wall filters.
        /// </summary>
        public static void ClearWallFilters() { _selectedWallIDs.Clear(); Revision++; }

        #endregion

        #region Liquid Filter Methods

        // --- Liquid filter methods ---
        /// <summary>
        /// Add a liquid ID to the filter.
        /// </summary>
        public static void AddLiquidFilter(LiquidType liq)
        {
            if (_selectedLiquids.Add(liq)) { Revision++; }
        }

        /// <summary>
        /// Remove a liquid ID from the filter.
        /// </summary>
        public static void RemoveLiquidFilter(LiquidType liq) { if (_selectedLiquids.Remove(liq)) Revision++; }

        /// <summary>
        /// Clear all liquid filters.
        /// </summary>
        public static void ClearLiquidFilters() { _selectedLiquids.Clear(); Revision++; }

        #endregion

        #region Wire Filter Methods

        // ------- Wire filter (new) -------
        [Flags]
        public enum WireType : byte
        {
            Red = 1 << 0,    // 1.
            Blue = 1 << 1,   // 2.
            Green = 1 << 2,  // 4.
            Yellow = 1 << 3, // 8.
        }
        public static void AddWireFilter(WireType wire) { if (_selectedWires.Add(wire)) Revision++; }
        public static void RemoveWireFilter(WireType wire) { if (_selectedWires.Remove(wire)) Revision++; }
        public static void ClearWireFilters()
        {
            _selectedWires.Clear();
            SelectedWireNames.Clear();
            Revision++;
        }

        #endregion

        #region Sprite Filter Methods

        /// <summary>
        /// Add a sprite ID to the filter.
        /// </summary>
        public static void AddSpriteFilter(int spriteId)
        {
            if (spriteId >= -1 && _selectedSpriteIDs.Add(spriteId))
            {
                Revision++;
            }
        }

        /// <summary>
        /// Remove a sprite ID from the filter (i.e. stop showing this ID).
        /// </summary>
        public static void RemoveSpriteFilter(int spriteId) { if (_selectedSpriteIDs.Remove(spriteId)) Revision++; }

        /// <summary>
        /// Clear all sprite filters (i.e. show every sprite).
        /// </summary>
        public static void ClearSpriteFilters() { _selectedSpriteIDs.Clear(); Revision++; }

        #endregion
    }

    /// <summary>
    /// Represents an item (such as a tile, wall, liquid, or wire) that can be filtered
    /// in the TEdit filter popup window. Each item has an ID, a display name, and a
    /// check state indicating whether it is included in the filter.
    ///
    /// Example usage:
    /// - Used as the data model for each checkbox row in the filter lists.
    /// - Supports binding to the IsChecked property to track filter selections.
    /// </summary>
    public partial class FilterCheckItem : ReactiveObject
    {
        public string Name { get; }
        public int Id { get; }

        [Reactive]
        private bool _isChecked;

        public FilterCheckItem(int id, string name, bool isChecked = false)
        {
            Id = id;
            Name = name;
            IsChecked = isChecked;
        }
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
