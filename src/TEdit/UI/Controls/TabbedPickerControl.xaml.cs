using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace TEdit.UI.Controls;

public partial class TabbedPickerControl : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TabbedPickerControl),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(nameof(SelectedValue), typeof(object), typeof(TabbedPickerControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

    public object SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public static readonly DependencyProperty SelectedValuePathProperty =
        DependencyProperty.Register(nameof(SelectedValuePath), typeof(string), typeof(TabbedPickerControl),
            new PropertyMetadata(null));

    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(TabbedPickerControl),
            new PropertyMetadata(null));

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public static readonly DependencyProperty FilterMemberPathProperty =
        DependencyProperty.Register(nameof(FilterMemberPath), typeof(string), typeof(TabbedPickerControl),
            new PropertyMetadata(null));

    public string FilterMemberPath
    {
        get => (string)GetValue(FilterMemberPathProperty);
        set => SetValue(FilterMemberPathProperty, value);
    }

    public static readonly DependencyProperty FilterIdMemberPathProperty =
        DependencyProperty.Register(nameof(FilterIdMemberPath), typeof(string), typeof(TabbedPickerControl),
            new PropertyMetadata(null));

    public string FilterIdMemberPath
    {
        get => (string)GetValue(FilterIdMemberPathProperty);
        set => SetValue(FilterIdMemberPathProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(TabbedPickerControl),
            new PropertyMetadata(null, OnItemTemplateChanged));

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(TabbedPickerControl),
            new PropertyMetadata("Search..."));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// ID threshold separating vanilla (Id &lt; threshold) from modded (Id &gt;= threshold) items.
    /// Bind to WorldConfiguration.TileCount or WallCount.
    /// </summary>
    public static readonly DependencyProperty ModIdThresholdProperty =
        DependencyProperty.Register(nameof(ModIdThreshold), typeof(short), typeof(TabbedPickerControl),
            new PropertyMetadata(short.MaxValue, OnFilterPropertyChanged));

    public short ModIdThreshold
    {
        get => (short)GetValue(ModIdThresholdProperty);
        set => SetValue(ModIdThresholdProperty, value);
    }

    /// <summary>Number of visible items in the list. Defaults to 5.</summary>
    public static readonly DependencyProperty VisibleItemCountProperty =
        DependencyProperty.Register(nameof(VisibleItemCount), typeof(int), typeof(TabbedPickerControl),
            new PropertyMetadata(5, OnVisibleItemCountChanged));

    public int VisibleItemCount
    {
        get => (int)GetValue(VisibleItemCountProperty);
        set => SetValue(VisibleItemCountProperty, value);
    }

    #endregion

    #region Private State

    private CollectionViewSource _viewSource;
    private ICollectionView _filteredView;
    private readonly DispatcherTimer _debounceTimer;
    private string _searchText;
    private bool _isSyncingSelection;
    private bool _showMods; // false = Terraria tab, true = Mods tab

    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo[]> _propertyCache = new();

    // Approximate row height for sizing (measured from ColorPickerTemplate)
    private const double ItemRowHeight = 24.0;

    #endregion

    public TabbedPickerControl()
    {
        InitializeComponent();

        _debounceTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _debounceTimer.Tick += DebounceTimer_Tick;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateListHeight();
        PART_SearchBox.PlaceholderText = Placeholder;
    }

    private void UpdateListHeight()
    {
        PART_ItemsList.Height = VisibleItemCount * ItemRowHeight;
    }

    #region ItemsSource / CVS Management

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabbedPickerControl ctrl)
            ctrl.RebuildViewSource();
    }

    private void RebuildViewSource()
    {
        var source = ItemsSource;
        if (source is ICollectionView existingView)
            source = existingView.SourceCollection;

        _viewSource = new CollectionViewSource { Source = source };
        _filteredView = _viewSource.View;

        if (_filteredView != null)
            _filteredView.Filter = FilterItem;

        PART_ItemsList.ItemsSource = _filteredView;
        SyncListSelectionFromValue();
    }

    #endregion

    #region Template Propagation

    private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabbedPickerControl ctrl)
            ctrl.PART_ItemsList.ItemTemplate = e.NewValue as DataTemplate;
    }

    private static void OnVisibleItemCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabbedPickerControl ctrl)
            ctrl.UpdateListHeight();
    }

    private static void OnFilterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabbedPickerControl ctrl)
            ctrl._filteredView?.Refresh();
    }

    #endregion

    #region Selection Sync

    private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabbedPickerControl ctrl && !ctrl._isSyncingSelection)
            ctrl.SyncListSelectionFromValue();
    }

    private void SyncListSelectionFromValue()
    {
        if (_isSyncingSelection) return;
        _isSyncingSelection = true;
        try
        {
            if (SelectedValue != null && !string.IsNullOrEmpty(SelectedValuePath) && ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                {
                    var val = GetPropertyValue(item, SelectedValuePath);
                    if (ValuesEqual(val, SelectedValue))
                    {
                        PART_ItemsList.SelectedItem = item;
                        PART_ItemsList.ScrollIntoView(item);
                        return;
                    }
                }
            }
            // Value not found in current view — don't clear ListBox selection
            // (item may be in the other tab)
        }
        finally { _isSyncingSelection = false; }
    }

    private void SyncValueFromListSelection()
    {
        if (_isSyncingSelection) return;
        _isSyncingSelection = true;
        try
        {
            var item = PART_ItemsList.SelectedItem;
            if (item != null && !string.IsNullOrEmpty(SelectedValuePath))
                SelectedValue = GetPropertyValue(item, SelectedValuePath);
        }
        finally { _isSyncingSelection = false; }
    }

    private static bool ValuesEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (a.Equals(b)) return true;
        try { return Convert.ToDouble(a) == Convert.ToDouble(b); }
        catch { return false; }
    }

    #endregion

    #region Filter Logic

    private bool FilterItem(object item)
    {
        if (item == null) return false;

        // Tab filter: check ID against threshold
        int id = GetItemId(item);
        if (id >= 0) // skip items with Id < 0 (like "Air" with Id=-1) — show in Terraria tab
        {
            bool isMod = id >= ModIdThreshold;
            if (_showMods != isMod) return false;
        }
        else
        {
            // Negative IDs (Air, etc.) only show in Terraria tab
            if (_showMods) return false;
        }

        // Text search filter
        if (string.IsNullOrEmpty(_searchText)) return true;

        string filterText = GetFilterText(item);
        if (filterText.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrEmpty(FilterIdMemberPath))
        {
            string idText = GetPropertyValue(item, FilterIdMemberPath)?.ToString() ?? string.Empty;
            if (idText.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private int GetItemId(object item)
    {
        string idPath = FilterIdMemberPath ?? "Id";
        var val = GetPropertyValue(item, idPath);
        if (val is int i) return i;
        if (val is ushort u) return u;
        if (val is short s) return s;
        return -1;
    }

    #endregion

    #region UI Event Handlers

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private void DebounceTimer_Tick(object sender, EventArgs e)
    {
        _debounceTimer.Stop();
        _searchText = PART_SearchBox.Text;
        _filteredView?.Refresh();
    }

    private void TerrariaTab_Checked(object sender, RoutedEventArgs e)
    {
        if (PART_ModsTab == null) return; // not yet initialized
        PART_ModsTab.IsChecked = false;
        if (_showMods)
        {
            _showMods = false;
            ApplyModGrouping(false);
            _filteredView?.Refresh();
            SyncListSelectionFromValue();
        }
    }

    private void ModsTab_Checked(object sender, RoutedEventArgs e)
    {
        if (PART_TerrariaTab == null) return; // not yet initialized
        PART_TerrariaTab.IsChecked = false;
        if (!_showMods)
        {
            _showMods = true;
            ApplyModGrouping(true);
            _filteredView?.Refresh();
            SyncListSelectionFromValue();
        }
    }

    private void ApplyModGrouping(bool enable)
    {
        if (_filteredView == null) return;

        if (enable)
        {
            // Switch to mod template (ShortName) and group by ModName
            PART_ItemsList.ItemTemplate = TryFindResource("ModItemTemplate") as DataTemplate;
            if (_filteredView.GroupDescriptions.Count == 0)
                _filteredView.GroupDescriptions.Add(new PropertyGroupDescription("ModName"));
            if (_filteredView.SortDescriptions.Count == 0)
            {
                _filteredView.SortDescriptions.Add(new SortDescription("ModName", ListSortDirection.Ascending));
                _filteredView.SortDescriptions.Add(new SortDescription("ShortName", ListSortDirection.Ascending));
            }
        }
        else
        {
            // Restore vanilla template and remove grouping
            PART_ItemsList.ItemTemplate = ItemTemplate;
            _filteredView.GroupDescriptions.Clear();
            _filteredView.SortDescriptions.Clear();
        }
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PART_ItemsList.SelectedItem != null)
            SyncValueFromListSelection();
    }

    #endregion

    #region Property Helpers

    private string GetFilterText(object item)
    {
        if (item == null) return string.Empty;
        string path = FilterMemberPath ?? DisplayMemberPath;
        if (!string.IsNullOrEmpty(path))
            return GetPropertyValue(item, path)?.ToString() ?? string.Empty;
        return item.ToString() ?? string.Empty;
    }

    private static object GetPropertyValue(object obj, string path)
    {
        if (obj == null || string.IsNullOrEmpty(path)) return null;
        var key = (obj.GetType(), path);
        var chain = _propertyCache.GetOrAdd(key, k => BuildPropertyChain(k.Item1, k.Item2));

        object current = obj;
        foreach (var prop in chain)
        {
            if (current == null) return null;
            current = prop.GetValue(current);
        }
        return current;
    }

    private static PropertyInfo[] BuildPropertyChain(Type rootType, string path)
    {
        var segments = path.Split('.');
        var chain = new PropertyInfo[segments.Length];
        var currentType = rootType;

        for (int i = 0; i < segments.Length; i++)
        {
            var prop = currentType.GetProperty(segments[i]);
            if (prop == null) return [];
            chain[i] = prop;
            currentType = prop.PropertyType;
        }
        return chain;
    }

    #endregion
}
