using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace TEdit.UI.Controls;

public partial class FilterablePickerControl : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(FilterablePickerControl),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(FilterablePickerControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(nameof(SelectedValue), typeof(object), typeof(FilterablePickerControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

    public object SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public static readonly DependencyProperty SelectedValuePathProperty =
        DependencyProperty.Register(nameof(SelectedValuePath), typeof(string), typeof(FilterablePickerControl),
            new PropertyMetadata(null));

    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(FilterablePickerControl),
            new PropertyMetadata(null));

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public static readonly DependencyProperty FilterMemberPathProperty =
        DependencyProperty.Register(nameof(FilterMemberPath), typeof(string), typeof(FilterablePickerControl),
            new PropertyMetadata(null));

    public string FilterMemberPath
    {
        get => (string)GetValue(FilterMemberPathProperty);
        set => SetValue(FilterMemberPathProperty, value);
    }

    public static readonly DependencyProperty FilterIdMemberPathProperty =
        DependencyProperty.Register(nameof(FilterIdMemberPath), typeof(string), typeof(FilterablePickerControl),
            new PropertyMetadata(null));

    public string FilterIdMemberPath
    {
        get => (string)GetValue(FilterIdMemberPathProperty);
        set => SetValue(FilterIdMemberPathProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(FilterablePickerControl),
            new PropertyMetadata(null, OnItemTemplateChanged));

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemContainerStyleProperty =
        DependencyProperty.Register(nameof(ItemContainerStyle), typeof(Style), typeof(FilterablePickerControl),
            new PropertyMetadata(null, OnItemContainerStyleChanged));

    public Style ItemContainerStyle
    {
        get => (Style)GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(FilterablePickerControl),
            new PropertyMetadata("Search..."));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(FilterablePickerControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDropDownOpenChanged));

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(FilterablePickerControl),
            new PropertyMetadata(320.0, OnMaxDropDownHeightChanged));

    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    #endregion

    #region Private State

    private CollectionViewSource _privateViewSource;
    private ICollectionView _filteredView;
    private readonly DispatcherTimer _debounceTimer;
    private bool _isInFilterMode;
    private bool _isSyncingSelection;
    private bool _isCommitting;
    private string _pendingFilterText;
    private object _selectionBeforeFilter;
    private object _selectionValueBeforeFilter;

    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo[]> _propertyCache = new();

    #endregion

    public FilterablePickerControl()
    {
        InitializeComponent();

        _debounceTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _debounceTimer.Tick += DebounceTimer_Tick;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    #region Window-level outside-click handling

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window != null)
            window.PreviewMouseDown += Window_PreviewMouseDown;

        // Wire up the toggle button — it sits on top and intercepts clicks
        PART_DropDownButton.Checked += DropDownButton_Checked;
        PART_DropDownButton.Unchecked += DropDownButton_Unchecked;

        // Hover states on the content border
        ContentBorder.MouseEnter += ContentBorder_MouseEnter;
        ContentBorder.MouseLeave += ContentBorder_MouseLeave;

        UpdateSelectedPresenter();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window != null)
            window.PreviewMouseDown -= Window_PreviewMouseDown;
    }

    private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!PART_Popup.IsOpen) return;

        // Check if click is inside our control or popup
        if (ContentBorder.IsMouseOver) return;
        if (PART_Popup.Child is FrameworkElement popupChild && popupChild.IsMouseOver) return;

        CloseDropDown(commitSelection: false);
    }

    #endregion

    #region Visual States (hover, focus, chevron)

    private void ContentBorder_MouseEnter(object sender, MouseEventArgs e)
    {
        if (!PART_FilterBox.IsKeyboardFocused && !PART_Popup.IsOpen)
            ContentBorder.Background = TryFindBrush("ComboBoxBackgroundPointerOver") ?? ContentBorder.Background;
    }

    private void ContentBorder_MouseLeave(object sender, MouseEventArgs e)
    {
        if (!PART_FilterBox.IsKeyboardFocused && !PART_Popup.IsOpen)
            ContentBorder.Background = TryFindBrush("ComboBoxBackground") ?? ContentBorder.Background;
    }

    private void SetFocusedVisualState()
    {
        ContentBorder.Background = TryFindBrush("ComboBoxBackgroundFocused") ?? ContentBorder.Background;
        AccentBorder.Visibility = Visibility.Visible;
    }

    private void ClearFocusedVisualState()
    {
        ContentBorder.Background = TryFindBrush("ComboBoxBackground") ?? ContentBorder.Background;
        AccentBorder.Visibility = Visibility.Collapsed;
    }

    private void AnimateChevron(bool open)
    {
        var animation = new DoubleAnimation
        {
            To = open ? 180 : 0,
            Duration = TimeSpan.FromMilliseconds(167)
        };
        ChevronRotate.BeginAnimation(RotateTransform.AngleProperty, animation);
    }

    private Brush TryFindBrush(string key)
    {
        return TryFindResource(key) as Brush;
    }

    private void UpdateSelectedPresenter()
    {
        if (SelectedItem != null && !_isInFilterMode)
        {
            PART_SelectedPresenter.ContentTemplate = ItemTemplate;

            if (ItemTemplate != null)
            {
                // Use template to render the full item (color swatch, icon, etc.)
                PART_SelectedPresenter.Content = SelectedItem;
            }
            else
            {
                // No template — show display text directly
                PART_SelectedPresenter.Content = GetDisplayText(SelectedItem);
            }

            PART_SelectedPresenter.Visibility = Visibility.Visible;
            PART_FilterBox.Visibility = Visibility.Collapsed;
            PART_Placeholder.Visibility = Visibility.Collapsed;
        }
        else if (!_isInFilterMode)
        {
            // No selection — show placeholder, hide presenter
            PART_SelectedPresenter.Content = null;
            PART_SelectedPresenter.Visibility = Visibility.Collapsed;
            PART_FilterBox.Visibility = Visibility.Collapsed;
            PART_Placeholder.Visibility = Visibility.Visible;
        }
    }

    private void ShowFilterMode()
    {
        PART_SelectedPresenter.Visibility = Visibility.Collapsed;
        PART_FilterBox.Visibility = Visibility.Visible;
        PART_Placeholder.Visibility = Visibility.Collapsed;
    }

    private void ShowDisplayMode()
    {
        PART_FilterBox.Visibility = Visibility.Collapsed;
        UpdateSelectedPresenter();
    }

    private void UpdatePlaceholder()
    {
        // Only relevant when not in filter mode and no selection
        if (_isInFilterMode || SelectedItem != null) return;
        PART_Placeholder.Visibility = Visibility.Visible;
    }

    #endregion

    #region ToggleButton wiring

    private void DropDownButton_Checked(object sender, RoutedEventArgs e)
    {
        if (!PART_Popup.IsOpen)
            OpenDropDown();

        // Switch to filter mode: show textbox with current display text
        ShowFilterMode();
        PART_FilterBox.Focus();
        PART_FilterBox.SelectAll();
    }

    private void DropDownButton_Unchecked(object sender, RoutedEventArgs e)
    {
        if (PART_Popup.IsOpen)
            CloseDropDown(commitSelection: false);
    }

    #endregion

    #region ItemsSource / CVS Management

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterablePickerControl ctrl)
            ctrl.RebuildViewSource();
    }

    private void RebuildViewSource()
    {
        var source = ItemsSource;
        if (source is ICollectionView existingView)
            source = existingView.SourceCollection;

        _privateViewSource = new CollectionViewSource { Source = source };
        _filteredView = _privateViewSource.View;

        if (_filteredView != null)
            _filteredView.Filter = FilterItem;

        PART_ItemsList.ItemsSource = _filteredView;
        SyncSelectedItemFromValue();
    }

    #endregion

    #region Template/Style Propagation

    private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterablePickerControl ctrl)
        {
            ctrl.PART_ItemsList.ItemTemplate = e.NewValue as DataTemplate;
            ctrl.PART_SelectedPresenter.ContentTemplate = e.NewValue as DataTemplate;
        }
    }

    private static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Only apply styles targeting ListBoxItem — ignore ComboBoxItem styles
        if (d is FilterablePickerControl ctrl && e.NewValue is Style style &&
            style.TargetType == typeof(ListBoxItem))
        {
            ctrl.PART_ItemsList.ItemContainerStyle = style;
        }
    }

    private static void OnMaxDropDownHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterablePickerControl ctrl && e.NewValue is double h)
        {
            ctrl.PART_ItemsList.MaxHeight = h - 10;
        }
    }

    #endregion

    #region Selection Sync

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterablePickerControl ctrl && !ctrl._isSyncingSelection)
            ctrl.SyncValueFromItem();
    }

    private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterablePickerControl ctrl && !ctrl._isSyncingSelection)
            ctrl.SyncSelectedItemFromValue();
    }

    private void SyncValueFromItem()
    {
        if (_isSyncingSelection) return;
        _isSyncingSelection = true;
        try
        {
            if (SelectedItem != null && !string.IsNullOrEmpty(SelectedValuePath))
                SelectedValue = GetPropertyValue(SelectedItem, SelectedValuePath);
            UpdateDisplayText();
        }
        finally { _isSyncingSelection = false; }
    }

    private void SyncSelectedItemFromValue()
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
                        SelectedItem = item;
                        UpdateDisplayText();
                        return;
                    }
                }
            }
            else if (SelectedValue == null || IsDefaultValue(SelectedValue))
            {
                SelectedItem = null;
            }
            UpdateDisplayText();
        }
        finally { _isSyncingSelection = false; }
    }

    private static bool IsDefaultValue(object value)
    {
        if (value is int i) return i == 0;
        if (value is byte b) return b == 0;
        if (value is short s) return s == 0;
        if (value is long l) return l == 0;
        return false;
    }

    private static bool ValuesEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (a.Equals(b)) return true;
        try
        {
            return Convert.ToDouble(a) == Convert.ToDouble(b);
        }
        catch { return false; }
    }

    private void UpdateDisplayText()
    {
        if (_isInFilterMode) return;

        string text = SelectedItem != null ? GetDisplayText(SelectedItem) : string.Empty;
        SetFilterBoxText(text);
        UpdateSelectedPresenter();
    }

    private void SetFilterBoxText(string text)
    {
        _isCommitting = true;
        try
        {
            PART_FilterBox.Text = text;
            if (PART_FilterBox.IsFocused)
                PART_FilterBox.CaretIndex = text?.Length ?? 0;
        }
        finally
        {
            _isCommitting = false;
            UpdatePlaceholder();
        }
    }

    #endregion

    #region Filter Logic

    private bool FilterItem(object item)
    {
        if (item == null) return false;
        if (string.IsNullOrEmpty(_pendingFilterText)) return true;

        string filterText = GetFilterText(item);
        if (filterText.Contains(_pendingFilterText, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrEmpty(FilterIdMemberPath))
        {
            string idText = GetPropertyValue(item, FilterIdMemberPath)?.ToString() ?? string.Empty;
            if (idText.Contains(_pendingFilterText, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private void RefreshFilter()
    {
        _filteredView?.Refresh();
    }

    private void DebounceTimer_Tick(object sender, EventArgs e)
    {
        _debounceTimer.Stop();
        _pendingFilterText = PART_FilterBox.Text;
        RefreshFilter();

        if (_filteredView != null)
        {
            _filteredView.MoveCurrentToFirst();
            if (_filteredView.CurrentItem != null)
                PART_ItemsList.SelectedItem = _filteredView.CurrentItem;
        }
    }

    #endregion

    #region Display / Property Helpers

    private string GetDisplayText(object item)
    {
        if (item == null) return string.Empty;
        string path = DisplayMemberPath ?? FilterMemberPath;
        if (!string.IsNullOrEmpty(path))
            return GetPropertyValue(item, path)?.ToString() ?? string.Empty;
        return item.ToString() ?? string.Empty;
    }

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

    #region UI Event Handlers

    private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isCommitting) return;

        if (!_isInFilterMode)
            EnterFilterMode();

        _debounceTimer.Stop();
        _debounceTimer.Start();

        UpdatePlaceholder();
    }

    private void FilterBox_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        ShowFilterMode();
        PART_FilterBox.SelectAll();

        // Make the toggle button non-hittestable while textbox is focused so typing works
        PART_DropDownButton.IsHitTestVisible = false;

        SetFocusedVisualState();
    }

    private void FilterBox_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
        {
            if (!PART_Popup.IsOpen)
            {
                // Re-enable the toggle button
                PART_DropDownButton.IsHitTestVisible = true;
                ClearFocusedVisualState();
                ShowDisplayMode();
                return;
            }

            if (IsKeyboardFocusWithin) return;
            if (PART_Popup.Child is UIElement popupChild && popupChild.IsKeyboardFocusWithin) return;

            PART_DropDownButton.IsHitTestVisible = true;
            CloseDropDown(commitSelection: false);
        });
    }

    private void FilterBox_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down:
                if (!PART_Popup.IsOpen)
                    OpenDropDown();
                if (PART_ItemsList.Items.Count > 0)
                {
                    PART_ItemsList.SelectedIndex = 0;
                    PART_ItemsList.UpdateLayout();
                    (PART_ItemsList.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem)?.Focus();
                }
                e.Handled = true;
                break;

            case Key.Enter:
                if (PART_Popup.IsOpen)
                    CommitHighlightedItem();
                else
                    OpenDropDown();
                e.Handled = true;
                break;

            case Key.Escape:
                CloseDropDown(commitSelection: false);
                e.Handled = true;
                break;

            case Key.Tab:
                if (PART_Popup.IsOpen)
                    CommitHighlightedItem();
                break;
        }
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private void ItemsList_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (PART_ItemsList.SelectedItem != null)
            CommitHighlightedItem();
    }

    private void ItemsList_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                CommitHighlightedItem();
                e.Handled = true;
                break;
            case Key.Escape:
                CloseDropDown(commitSelection: false);
                e.Handled = true;
                break;
            default:
                if (e.Key >= Key.A && e.Key <= Key.Z ||
                    e.Key >= Key.D0 && e.Key <= Key.D9 ||
                    e.Key == Key.Back || e.Key == Key.Space)
                {
                    PART_FilterBox.Focus();
                }
                break;
        }
    }

    private void Popup_Opened(object sender, EventArgs e)
    {
        _isSyncingDropDown = true;
        IsDropDownOpen = true;
        _isSyncingDropDown = false;
        AnimateChevron(true);
        SetFocusedVisualState();
    }

    private void Popup_Closed(object sender, EventArgs e)
    {
        _isSyncingDropDown = true;
        IsDropDownOpen = false;
        _isSyncingDropDown = false;
        AnimateChevron(false);

        PART_DropDownButton.IsHitTestVisible = true;
        if (!PART_FilterBox.IsKeyboardFocused)
            ClearFocusedVisualState();

        ShowDisplayMode();
    }

    private bool _isSyncingDropDown;

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterablePickerControl ctrl && !ctrl._isSyncingDropDown)
        {
            if ((bool)e.NewValue)
                ctrl.OpenDropDown();
            else
                ctrl.CloseDropDown(commitSelection: false);
        }
    }

    #endregion

    #region Mode Management

    private void EnterFilterMode()
    {
        if (_isInFilterMode) return;
        _isInFilterMode = true;
        _selectionBeforeFilter = SelectedItem;
        _selectionValueBeforeFilter = SelectedValue;

        ShowFilterMode();

        if (!PART_Popup.IsOpen)
            OpenDropDown();
    }

    private void OpenDropDown()
    {
        if (PART_Popup.IsOpen) return;

        // Snapshot current selection so cancel restores it
        _selectionBeforeFilter = SelectedItem;
        _selectionValueBeforeFilter = SelectedValue;

        _pendingFilterText = null;
        RefreshFilter();
        PART_Popup.IsOpen = true;

        if (SelectedItem != null)
            PART_ItemsList.SelectedItem = SelectedItem;
    }

    private void CloseDropDown(bool commitSelection)
    {
        if (!PART_Popup.IsOpen && !_isInFilterMode) return;

        _debounceTimer.Stop();
        _isInFilterMode = false;

        if (!commitSelection)
        {
            _isSyncingSelection = true;
            try
            {
                SelectedItem = _selectionBeforeFilter;
                SelectedValue = _selectionValueBeforeFilter;
            }
            finally { _isSyncingSelection = false; }
        }

        _pendingFilterText = null;
        RefreshFilter();
        PART_Popup.IsOpen = false;

        string text = SelectedItem != null ? GetDisplayText(SelectedItem) : string.Empty;
        SetFilterBoxText(text);
        ShowDisplayMode();
    }

    private void CommitHighlightedItem()
    {
        var item = PART_ItemsList.SelectedItem;
        if (item != null)
        {
            _isSyncingSelection = true;
            try
            {
                SelectedItem = item;
                if (!string.IsNullOrEmpty(SelectedValuePath))
                    SelectedValue = GetPropertyValue(item, SelectedValuePath);
            }
            finally { _isSyncingSelection = false; }
        }

        _debounceTimer.Stop();
        _isInFilterMode = false;
        _pendingFilterText = null;
        RefreshFilter();
        PART_Popup.IsOpen = false;

        string text = SelectedItem != null ? GetDisplayText(SelectedItem) : string.Empty;
        SetFilterBoxText(text);
        ShowDisplayMode();
    }

    #endregion
}
