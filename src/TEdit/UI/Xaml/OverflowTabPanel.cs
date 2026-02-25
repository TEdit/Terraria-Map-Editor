using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace TEdit.UI.Xaml;

/// <summary>
/// A panel for TabControl activity bars that shows an overflow button when
/// tab items exceed available height. Hidden tabs appear in a context menu
/// accessible from the overflow button, similar to VS Code's activity bar.
/// Designed to replace TabPanel as the items host in a TabControl template.
/// </summary>
public class OverflowTabPanel : Panel
{
    private readonly System.Windows.Controls.Button _overflowButton;
    private readonly List<UIElement> _hiddenChildren = new();

    private static readonly SolidColorBrush InactiveBrush = new(Color.FromRgb(0x85, 0x85, 0x85));
    private static readonly SolidColorBrush HoverBrush = new(Color.FromRgb(0xCC, 0xCC, 0xCC));

    static OverflowTabPanel()
    {
        InactiveBrush.Freeze();
        HoverBrush.Freeze();
    }

    public OverflowTabPanel()
    {
        ClipToBounds = true;

        var style = new Style(typeof(System.Windows.Controls.Button));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, InactiveBrush));
        style.Setters.Add(new Setter(FrameworkElement.CursorProperty, System.Windows.Input.Cursors.Hand));
        style.Setters.Add(new Setter(Control.TemplateProperty, CreateButtonTemplate()));
        var hoverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hoverTrigger.Setters.Add(new Setter(Control.ForegroundProperty, HoverBrush));
        style.Triggers.Add(hoverTrigger);
        style.Seal();

        _overflowButton = new System.Windows.Controls.Button
        {
            Width = 48,
            Height = 48,
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            ToolTip = "More panels...",
            Style = style,
        };

        var icon = new Wpf.Ui.Controls.SymbolIcon
        {
            Symbol = Wpf.Ui.Controls.SymbolRegular.MoreVertical20,
            FontSize = 24,
        };
        // Re-apply the Foreground binding after Loaded to work around
        // WPF-UI SymbolIcon resetting Foreground during initialization.
        icon.Loaded += (_, _) =>
        {
            icon.SetBinding(
                Wpf.Ui.Controls.SymbolIcon.ForegroundProperty,
                new System.Windows.Data.Binding(nameof(Control.Foreground))
                {
                    Source = _overflowButton,
                });
        };
        // Also set binding now for immediate effect when possible.
        icon.SetBinding(
            Wpf.Ui.Controls.SymbolIcon.ForegroundProperty,
            new System.Windows.Data.Binding(nameof(Control.Foreground))
            {
                Source = _overflowButton,
            });
        _overflowButton.Content = icon;
        _overflowButton.Click += OnOverflowButtonClick;

        AddVisualChild(_overflowButton);
        AddLogicalChild(_overflowButton);
    }

    private static ControlTemplate CreateButtonTemplate()
    {
        // Minimal template: just a border with content presenter, no chrome
        var template = new ControlTemplate(typeof(System.Windows.Controls.Button));
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.BackgroundProperty, Brushes.Transparent);
        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        border.AppendChild(presenter);
        template.VisualTree = border;
        template.Seal();
        return template;
    }

    protected override int VisualChildrenCount => InternalChildren.Count + 1;

    protected override Visual GetVisualChild(int index)
    {
        if (index < InternalChildren.Count)
            return InternalChildren[index];
        if (index == InternalChildren.Count)
            return _overflowButton;
        throw new ArgumentOutOfRangeException(nameof(index));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double maxWidth = 0;
        double totalHeight = 0;

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
            totalHeight += child.DesiredSize.Height;
            maxWidth = Math.Max(maxWidth, child.DesiredSize.Width);
        }

        _overflowButton.Measure(new Size(availableSize.Width, double.PositiveInfinity));
        double overflowHeight = _overflowButton.DesiredSize.Height;

        double constrainedHeight = double.IsPositiveInfinity(availableSize.Height)
            ? totalHeight
            : availableSize.Height;

        bool hasOverflow = totalHeight > constrainedHeight;
        double budgetHeight = hasOverflow ? constrainedHeight - overflowHeight : constrainedHeight;

        _hiddenChildren.Clear();
        double usedHeight = 0;

        foreach (UIElement child in InternalChildren)
        {
            if (usedHeight + child.DesiredSize.Height <= budgetHeight)
            {
                usedHeight += child.DesiredSize.Height;
            }
            else
            {
                _hiddenChildren.Add(child);
            }
        }

        double desiredHeight = hasOverflow ? usedHeight + overflowHeight : usedHeight;
        return new Size(maxWidth, Math.Min(desiredHeight, constrainedHeight));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double totalHeight = InternalChildren.Cast<UIElement>().Sum(c => c.DesiredSize.Height);
        bool hasOverflow = totalHeight > finalSize.Height;
        double overflowHeight = _overflowButton.DesiredSize.Height;
        double budgetHeight = hasOverflow ? finalSize.Height - overflowHeight : finalSize.Height;

        _hiddenChildren.Clear();
        double y = 0;
        bool overflowing = false;

        foreach (UIElement child in InternalChildren)
        {
            if (!overflowing && y + child.DesiredSize.Height <= budgetHeight)
            {
                child.Arrange(new Rect(0, y, finalSize.Width, child.DesiredSize.Height));
                y += child.DesiredSize.Height;
            }
            else
            {
                overflowing = true;
                _hiddenChildren.Add(child);
                child.Arrange(new Rect(0, finalSize.Height, 0, 0));
            }
        }

        if (hasOverflow)
        {
            _overflowButton.Arrange(new Rect(0, y, finalSize.Width, overflowHeight));
        }
        else
        {
            _overflowButton.Arrange(new Rect(0, finalSize.Height, 0, 0));
        }

        return finalSize;
    }

    private void OnOverflowButtonClick(object sender, RoutedEventArgs e)
    {
        var menu = new ContextMenu();

        foreach (var child in _hiddenChildren)
        {
            if (child is TabItem tabItem)
            {
                AddTabItemToMenu(menu, tabItem);
            }
        }

        if (menu.Items.Count > 0)
        {
            menu.PlacementTarget = _overflowButton;
            menu.Placement = PlacementMode.Custom;
            menu.CustomPopupPlacementCallback = (popupSize, targetSize, offset) =>
            {
                // Anchor at bottom-right of overflow button, popup opens left and up
                var placement = new CustomPopupPlacement(
                    new Point(-popupSize.Width, targetSize.Height - popupSize.Height),
                    PopupPrimaryAxis.None);
                return [placement];
            };
            menu.IsOpen = true;
        }
    }

    private void AddTabItemToMenu(ContextMenu menu, TabItem tabItem)
    {
        // Skip collapsed tabs (e.g., conditionally hidden)
        if (tabItem.Visibility == Visibility.Collapsed)
            return;

        string header = ExtractTooltipText(tabItem) ?? $"Tab {InternalChildren.IndexOf(tabItem) + 1}";
        var item = new MenuItem { Header = header };

        // Find the parent TabControl so we can select this tab
        item.Click += (s, e) =>
        {
            var tabControl = ItemsControl.ItemsControlFromItemContainer(tabItem) as TabControl;
            if (tabControl != null)
            {
                tabControl.SelectedItem = tabItem;
            }
            else
            {
                tabItem.IsSelected = true;
            }
        };

        // Mark active tab with a check
        if (tabItem.IsSelected)
        {
            item.IsChecked = true;
        }

        menu.Items.Add(item);
    }

    private static string ExtractTooltipText(FrameworkElement element)
    {
        var tooltip = element.ToolTip;
        if (tooltip is string s) return s;
        if (tooltip is TextBlock tb) return tb.Text;
        if (tooltip != null) return tooltip.ToString();
        return null;
    }
}
