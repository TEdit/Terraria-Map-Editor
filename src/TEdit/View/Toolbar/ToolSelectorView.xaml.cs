using System.Windows;
using System.Windows.Controls;
using TEdit.Editor;
using TEdit.Editor.Tools;

namespace TEdit.View;

/// <summary>
/// Interaction logic for ToolSelectorView.xaml
/// </summary>
public partial class ToolSelectorView : UserControl
{
    public ToolSelectorView()
    {
        InitializeComponent();
    }

    // Photoshop-style: right-click the selection tool to pick its shape.
    private void ToolButton_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: SelectionTool tool } button || button.ContextMenu != null)
            return;

        var menu = new ContextMenu();
        foreach (var (shape, label) in new[]
        {
            (SelectionShape.Rectangle, Properties.Language.toolbar_selection_rectangle),
            (SelectionShape.Brush, Properties.Language.toolbar_selection_brush),
            (SelectionShape.Lasso, Properties.Language.toolbar_selection_lasso),
        })
        {
            var item = new MenuItem { Header = label, Tag = shape };
            if (shape != SelectionShape.Rectangle)
                item.ToolTip = Properties.Language.toolbar_selection_freeform_hint;
            item.Click += (_, _) => tool.SelectMode(shape);
            menu.Items.Add(item);
        }
        menu.Opened += (_, _) =>
        {
            foreach (MenuItem item in menu.Items)
                item.IsChecked = (SelectionShape)item.Tag == tool.Mode;
        };
        button.ContextMenu = menu;
    }
}
