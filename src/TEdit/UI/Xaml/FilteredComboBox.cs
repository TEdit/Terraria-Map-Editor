using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TEdit.UI.Xaml;

public class FilteredComboBox : ComboBox
{
    static FilteredComboBox()
    {
        // Enable virtualization for the dropdown items
        ItemsPanelProperty.OverrideMetadata(
            typeof(FilteredComboBox),
            new FrameworkPropertyMetadata(CreateVirtualizingItemsPanel()));
    }

    private static ItemsPanelTemplate CreateVirtualizingItemsPanel()
    {
        var factory = new FrameworkElementFactory(typeof(VirtualizingStackPanel));
        factory.SetValue(VirtualizingPanel.IsVirtualizingProperty, true);
        factory.SetValue(VirtualizingPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
        return new ItemsPanelTemplate(factory);
    }

    /// <summary>
    /// Dot-delimited property path used for filtering (e.g. "Value.Name").
    /// When not set, falls back to ToString().
    /// </summary>
    public static readonly DependencyProperty FilterMemberPathProperty =
        DependencyProperty.Register(nameof(FilterMemberPath), typeof(string), typeof(FilteredComboBox),
            new PropertyMetadata(null));

    public string FilterMemberPath
    {
        get => (string)GetValue(FilterMemberPathProperty);
        set => SetValue(FilterMemberPathProperty, value);
    }

    public FilteredComboBox()
    {
        SetResourceReference(StyleProperty, typeof(ComboBox));

        // Disable built-in auto-complete; we handle filtering ourselves
        IsTextSearchEnabled = false;

        // Suppress the red validation error border during filtering
        Validation.SetErrorTemplate(this, new ControlTemplate());

        // Enable virtualization properties on this instance
        VirtualizingPanel.SetIsVirtualizing(this, true);
        VirtualizingPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
        VirtualizingPanel.SetScrollUnit(this, ScrollUnit.Pixel);
    }

    private string currentFilter = string.Empty;
    private bool _isFiltering;

    private object SelectedItemWhenDropDownOpened = null;
    private object SelectedValueWhenDropDownOpened = null;

    protected TextBox EditableTextBox => GetTemplateChild("PART_EditableTextBox") as TextBox;

    protected override void OnDropDownOpened(System.EventArgs e)
    {
        SelectedItemWhenDropDownOpened = SelectedItem;
        SelectedValueWhenDropDownOpened = SelectedValue;

        // Clear the filter so all items are visible, then restore selection
        currentFilter = string.Empty;
        var savedItem = SelectedItem;
        var savedValue = SelectedValue;
        var savedText = Text;
        RefreshFilter();
        SelectedItem = savedItem;
        SelectedValue = savedValue;
        Text = savedText;

        base.OnDropDownOpened(e);
    }

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        if (newValue != null)
        {
            var view = CollectionViewSource.GetDefaultView(newValue);
            view.Filter += FilterItem;
        }

        if (oldValue != null)
        {
            var view = CollectionViewSource.GetDefaultView(oldValue);
            if (view != null) view.Filter -= FilterItem;
        }

        base.OnItemsSourceChanged(oldValue, newValue);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Tab:
            case Key.Enter:
                // Commit: pick the first visible item if filtering, then close
                if (_isFiltering && Items.Count > 0)
                    SelectedIndex = 0;
                IsDropDownOpen = false;
                _isFiltering = false;
                e.Handled = true;
                break;
            case Key.Escape:
                RevertToPrevious();
                _isFiltering = false;
                e.Handled = true;
                break;
            case Key.Back:
                if (!IsEditable && _isFiltering && currentFilter.Length > 0)
                {
                    currentFilter = currentFilter[..^1];
                    RefreshFilter();
                    e.Handled = true;
                }
                else
                {
                    if (e.Key == Key.Down) IsDropDownOpen = true;
                    base.OnPreviewKeyDown(e);
                }
                break;
            default:
                if (e.Key == Key.Down) IsDropDownOpen = true;
                base.OnPreviewKeyDown(e);
                break;
        }
    }

    private void RevertToPrevious()
    {
        IsDropDownOpen = false;
        if (SelectedItemWhenDropDownOpened == null)
        {
            Text = "";
            SelectedValue = 0;
        }
        else
        {
            Text = GetDisplayText(SelectedItemWhenDropDownOpened);
            SelectedValue = SelectedValueWhenDropDownOpened;
        }
    }

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        // Handle typing for non-editable mode (no PART_EditableTextBox)
        if (!IsEditable)
        {
            _isFiltering = true;
            currentFilter += e.Text;
            RefreshFilter();
            IsDropDownOpen = true;
            e.Handled = true;
            return;
        }

        base.OnPreviewTextInput(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (!IsEditable) return; // non-editable filtering handled in OnPreviewTextInput

        switch (e.Key)
        {
            case Key.Up:
            case Key.Down:
            case Key.Tab:
            case Key.Enter:
            case Key.Escape:
                break;
            default:
                _isFiltering = true;
                currentFilter = Text;
                RefreshFilter();

                // Restore the typed text (RefreshFilter may alter it)
                Text = currentFilter;
                IsDropDownOpen = true;
                EditableTextBox.SelectionStart = int.MaxValue;

                base.OnKeyUp(e);
                break;
        }
    }

    protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        if (_isFiltering && Items.Count == 0)
            RevertToPrevious();

        _isFiltering = false;
        ClearFilter();
        base.OnPreviewLostKeyboardFocus(e);
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        // When user clicks an item from the dropdown, stop filtering
        if (_isFiltering && SelectedIndex >= 0)
        {
            _isFiltering = false;
            currentFilter = string.Empty;
            IsDropDownOpen = false;
        }

        // Show the friendly display name instead of ToString() (e.g. KeyValuePair shows "[id, name]")
        if (IsEditable && !_isFiltering && SelectedItem != null && !string.IsNullOrEmpty(FilterMemberPath))
        {
            Text = GetFilterText(SelectedItem);
        }
    }

    private void RefreshFilter()
    {
        if (ItemsSource == null) return;

        var view = CollectionViewSource.GetDefaultView(ItemsSource);
        view.Refresh();
    }

    private void ClearFilter()
    {
        currentFilter = string.Empty;
        RefreshFilter();
    }

    private bool FilterItem(object value)
    {
        if (value == null) return false;
        if (currentFilter.Length == 0) return true;

        string displayText = GetFilterText(value);
        return displayText.Contains(currentFilter, System.StringComparison.OrdinalIgnoreCase);
    }

    private string GetDisplayText(object value)
    {
        if (value == null) return string.Empty;
        if (!string.IsNullOrEmpty(FilterMemberPath))
            return GetFilterText(value);
        return value.ToString() ?? string.Empty;
    }

    private string GetFilterText(object value)
    {
        if (string.IsNullOrEmpty(FilterMemberPath))
            return value.ToString() ?? string.Empty;

        // Walk dot-delimited property path
        object current = value;
        foreach (var segment in FilterMemberPath.Split('.'))
        {
            if (current == null) return string.Empty;
            var prop = current.GetType().GetProperty(segment);
            if (prop == null) return value.ToString() ?? string.Empty;
            current = prop.GetValue(current);
        }

        return current?.ToString() ?? string.Empty;
    }
}
