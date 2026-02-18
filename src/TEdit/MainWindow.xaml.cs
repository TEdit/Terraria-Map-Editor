using System;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using ReactiveUI;
using System.Windows.Input;
using System.Collections.Generic;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Editor;
using TEdit.Editor.Tools;
using TEdit.ViewModel;
using TEdit.Configuration;
using TEdit.View.Popups;
using System.IO;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace TEdit;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    [DllImport("user32.dll")]
    private static extern nint SendMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern nint ExtractIcon(nint hInst, string lpszExeFileName, int nIconIndex);

    private const uint WM_SETICON = 0x0080;
    private const nint ICON_SMALL = 0;
    private const nint ICON_BIG = 1;

    private nint _appIcon;
    private nint _hwnd;

    private WorldViewModel _vm;
    private ITool _toolBeforePicker;
    private DateTime _pickerKeyDownTime;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModelLocator.WorldViewModel;
        _vm = (WorldViewModel)DataContext;
        AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
        AddHandler(Keyboard.KeyUpEvent, (KeyEventHandler)HandleKeyUpEvent);

        // Auto-expand side panel when tab is activated programmatically
        _vm.WhenAnyValue(vm => vm.SelectedTabIndex)
            .Skip(1)  // Skip initial value
            .Subscribe(_ => ExpandSidePanel());

        SourceInitialized += MainWindow_SourceInitialized;
        Loaded += MainWindow_IconFixLoaded;
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;
        if (_hwnd == nint.Zero) return;

        // Extract the app icon once from the exe resource
        var exePath = Environment.ProcessPath;
        if (exePath != null)
            _appIcon = ExtractIcon(nint.Zero, exePath, 0);
    }

    private void MainWindow_IconFixLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindow_IconFixLoaded;

        // FluentWindow's Loaded handler removes WS_SYSMENU which clears the taskbar icon.
        // Use BeginInvoke to re-apply after all Loaded handlers have completed.
        Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
        {
            if (_appIcon != nint.Zero && _hwnd != nint.Zero)
            {
                SendMessage(_hwnd, WM_SETICON, ICON_BIG, _appIcon);
                SendMessage(_hwnd, WM_SETICON, ICON_SMALL, _appIcon);
            }
        });
    }

    async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Initialize snackbar service with UI element
        // Note: DialogService uses WPF-UI MessageBox (separate window) to avoid
        // WPF airspace issues with the DirectX/XNA WorldRenderXna control
        App.SnackbarService.SetSnackbarPresenter(SnackbarPresenter);

        // Set up navigation delegates for Find sidebar
        _vm.ZoomFocus = ZoomFocus;
        _vm.PanTo = PanTo;

        bool shouldAsk = false;
        string currentVersion = App.Version.ToString();

        switch (UserSettingsService.Current.Telemetry)
        {
            case -1: // first run
                shouldAsk = true;
                break;
            case 0: // previously declined
                shouldAsk = UserSettingsService.Current.TelemetryDeclinedVersion != currentVersion;
                break;
            case 1: // approved permanently
                break;
        }

        if (shouldAsk)
        {
            var result = await App.DialogService.ShowConfirmationAsync(
                Properties.Language.telemetry_prompt_title,
                Properties.Language.telemetry_prompt_message);

            if (result)
            {
                UserSettingsService.Current.Telemetry = 1;
                _vm.EnableTelemetry = true;
            }
            else
            {
                UserSettingsService.Current.Telemetry = 0;
                UserSettingsService.Current.TelemetryDeclinedVersion = currentVersion;
                _vm.EnableTelemetry = false;
            }
        }

        // Auto-open World Explorer if no file was passed via command line
        if (Application.Current.Properties["OpenFile"] == null)
        {
            Dispatcher.InvokeAsync(() => ShowWorldExplorer(), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }

    private void HandleKeyUpEvent(object sender, KeyEventArgs e)
    {
        // Try new InputService first
        var actions = App.Input.HandleKeyboard(e.Key, e.KeyboardDevice.Modifiers, TEdit.Input.InputScope.Application);
        foreach (var actionId in actions)
        {
            if (HandleKeyUpAction(actionId))
            {
                e.Handled = true;
                return;
            }
        }

        // Fall back to old system for backward compatibility
        var command = App.ShortcutKeys.Get(e);
        if (command == null) return;

        switch (command)
        {
            case "pan":
                _vm.RequestPanCommand.Execute(false).Subscribe();
                break;
        }
    }

    private bool HandleKeyUpAction(string actionId)
    {
        switch (actionId)
        {
            case "nav.pan":
                _vm.RequestPanCommand.Execute(false).Subscribe();
                return true;
            case "tool.picker":
                var held = (DateTime.UtcNow - _pickerKeyDownTime).TotalMilliseconds > _vm.PickerHoldThresholdMs;
                if (held && _toolBeforePicker != null)
                {
                    _vm.SetActiveTool(_toolBeforePicker);
                }
                _toolBeforePicker = null;
                return true;
            default:
                return false;
        }
    }

    private void HandleKeyDownEvent(object sender, KeyEventArgs e)
    {
        if (!(e.Source is View.WorldRenderXna)) return;

        try
        {
            // Try new InputService first
            var actions = App.Input.HandleKeyboard(e.Key, e.KeyboardDevice.Modifiers, TEdit.Input.InputScope.Application);
            foreach (var actionId in actions)
            {
                if (HandleKeyDownAction(actionId, e))
                {
                    return;
                }
            }

            // Fall back to old system for backward compatibility
            ScrollEventArgs scrollValue = null;

            var command = App.ShortcutKeys.Get(e.Key, e.KeyboardDevice.Modifiers);
            if (command == null) return;

            switch (command)
            {
                case "copy":
                    if (_vm.CurrentWorld != null && ((ICommand)_vm.CopyCommand).CanExecute(null))
                    {
                        ((ICommand)_vm.CopyCommand).Execute(null);
                        _vm.SelectedTabIndex = 3;
                    }
                    break;
                case "paste":
                    if (_vm.CurrentWorld != null && ((ICommand)_vm.PasteCommand).CanExecute(null))
                    {
                        ((ICommand)_vm.PasteCommand).Execute(null);
                        _vm.SelectedTabIndex = 3;
                    }
                    break;
                case "undo":
                    if (_vm.CurrentWorld != null)
                    {
                        ((ICommand)_vm.UndoCommand).Execute(null);
                    }
                    break;
                case "redo":
                    if (_vm.CurrentWorld != null)
                    {
                        ((ICommand)_vm.RedoCommand).Execute(null);
                    }
                    break;
                case "selectall":
                    if (_vm.CurrentWorld != null)
                    {
                        _vm.Selection.IsActive = true;
                        _vm.Selection.SetRectangle(new Vector2Int32(0, 0),
                            new Vector2Int32(_vm.CurrentWorld.TilesWide - 1, _vm.CurrentWorld.TilesHigh - 1));
                    }
                    break;
                case "selectnone":
                    if (_vm.CurrentWorld != null)
                    {
                        _vm.Selection.IsActive = false;
                    }
                    break;
                case "open":
                    if (((ICommand)_vm.OpenCommand).CanExecute(null))
                        ((ICommand)_vm.OpenCommand).Execute(null);
                    break;
                case "save":
                    if (((ICommand)_vm.SaveCommand).CanExecute(null))
                        ((ICommand)_vm.SaveCommand).Execute(null);
                    break;
                case "saveas":
                    if (((ICommand)_vm.SaveAsCommand).CanExecute(null))
                        ((ICommand)_vm.SaveAsCommand).Execute(null);
                    break;
                case "saveasversion":
                    if (((ICommand)_vm.SaveAsVersionCommand).CanExecute(null))
                        ((ICommand)_vm.SaveAsVersionCommand).Execute(null);
                    break;
                case "reloadworld":
                    if (((ICommand)_vm.ReloadCommand).CanExecute(null))
                        ((ICommand)_vm.ReloadCommand).Execute(null);
                    break;
                case "deleteselection":
                    if (((ICommand)_vm.DeleteCommand).CanExecute(null))
                        ((ICommand)_vm.DeleteCommand).Execute(null);
                    break;
                case "resettool":
                    if (_vm.ActiveTool != null)
                    {
                        if (_vm.ActiveTool.Name == "Paste")
                            SetActiveTool("Arrow");
                        else
                            _vm.Selection.IsActive = false;
                    }
                    break;
                case "scrollup":
                    scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Up, 10);
                    _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                    e.Handled = true;
                    break;
                case "scrollupfast":
                    scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Up, 50);
                    _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                    e.Handled = true;
                    break;
                case "scrollright":
                    scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Right, 10);
                    _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                    e.Handled = true;
                    break;
                case "scrollrightfast":
                    scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Right, 50);
                    _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                    e.Handled = true;
                    break;
                case "scrolldown":
                    scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Down, 10);
                    _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                    e.Handled = true;
                    break;
                case "scrolldownfast":
                    scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Down, 50);
                    _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                    e.Handled = true;
                    break;
                case "scrollleft":
                    scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Left, 10);
                    _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                    e.Handled = true;
                    break;
                case "scrollleftfast":
                    scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Left, 50);
                    _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                    e.Handled = true;
                    break;
                case "pan":
                    _vm.RequestPanCommand.Execute(true).Subscribe();
                    break;
                case "zoomin":
                    _vm.RequestZoomCommand.Execute(true).Subscribe();
                    break;
                case "zoomout":
                    _vm.RequestZoomCommand.Execute(false).Subscribe();
                    break;
                case "eraser":
                    _vm.TilePicker.IsEraser = !_vm.TilePicker.IsEraser;
                    break;
                case "swap":
                    _vm.TilePicker.Swap(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                    break;
                case "toggletile":
                    _vm.TilePicker.TileStyleActive = !_vm.TilePicker.TileStyleActive;
                    break;
                case "togglewall":
                    _vm.TilePicker.WallStyleActive = !_vm.TilePicker.WallStyleActive;
                    break;
                default:
                    SetActiveTool(command);
                    break;
            }

        }
        catch (Exception ex)
        {
            ErrorLogging.LogException(ex);
        }
    }

    private bool HandleKeyDownAction(string actionId, KeyEventArgs e)
    {
        ScrollEventArgs scrollValue = null;

        switch (actionId)
        {
            // File operations
            case "file.open":
                if (((ICommand)_vm.OpenCommand).CanExecute(null))
                    ((ICommand)_vm.OpenCommand).Execute(null);
                return true;
            case "file.save":
                if (((ICommand)_vm.SaveCommand).CanExecute(null))
                    ((ICommand)_vm.SaveCommand).Execute(null);
                return true;
            case "file.saveas":
                if (((ICommand)_vm.SaveAsCommand).CanExecute(null))
                    ((ICommand)_vm.SaveAsCommand).Execute(null);
                return true;
            case "file.reload":
                if (((ICommand)_vm.ReloadCommand).CanExecute(null))
                    ((ICommand)_vm.ReloadCommand).Execute(null);
                return true;

            // Editing
            case "edit.copy":
                if (_vm.CurrentWorld != null && ((ICommand)_vm.CopyCommand).CanExecute(null))
                {
                    ((ICommand)_vm.CopyCommand).Execute(null);
                    _vm.SelectedTabIndex = 3;
                }
                return true;
            case "edit.paste":
                if (_vm.CurrentWorld != null && ((ICommand)_vm.PasteCommand).CanExecute(null))
                {
                    ((ICommand)_vm.PasteCommand).Execute(null);
                    _vm.SelectedTabIndex = 3;
                }
                return true;
            case "edit.undo":
                if (_vm.CurrentWorld != null)
                    ((ICommand)_vm.UndoCommand).Execute(null);
                return true;
            case "edit.redo":
                if (_vm.CurrentWorld != null)
                    ((ICommand)_vm.RedoCommand).Execute(null);
                return true;

            // Selection
            case "selection.all":
                if (_vm.CurrentWorld != null)
                {
                    _vm.Selection.IsActive = true;
                    _vm.Selection.SetRectangle(new Vector2Int32(0, 0),
                        new Vector2Int32(_vm.CurrentWorld.TilesWide - 1, _vm.CurrentWorld.TilesHigh - 1));
                }
                return true;
            case "selection.none":
                if (_vm.CurrentWorld != null)
                    _vm.Selection.IsActive = false;
                return true;
            case "selection.delete":
                if (((ICommand)_vm.DeleteCommand).CanExecute(null))
                    ((ICommand)_vm.DeleteCommand).Execute(null);
                return true;

            // Selection movement and resizing
            case "selection.move.up":
            case "selection.move.down":
            case "selection.move.left":
            case "selection.move.right":
            case "selection.resize.up":
            case "selection.resize.down":
            case "selection.resize.left":
            case "selection.resize.right":
                if (_vm.ActiveTool?.Name == "Selection" && _vm.Selection.IsActive && _vm.CurrentWorld != null)
                {
                    var area = _vm.Selection.SelectionArea;
                    if (actionId.StartsWith("selection.move."))
                    {
                        int dx = actionId == "selection.move.left" ? -1 : actionId == "selection.move.right" ? 1 : 0;
                        int dy = actionId == "selection.move.up" ? -1 : actionId == "selection.move.down" ? 1 : 0;
                        area.Offset(dx, dy);
                        area.X = Math.Max(0, Math.Min(area.X, _vm.CurrentWorld.TilesWide - area.Width));
                        area.Y = Math.Max(0, Math.Min(area.Y, _vm.CurrentWorld.TilesHigh - area.Height));
                    }
                    else
                    {
                        int dw = actionId == "selection.resize.right" ? 1 : actionId == "selection.resize.left" ? -1 : 0;
                        int dh = actionId == "selection.resize.down" ? 1 : actionId == "selection.resize.up" ? -1 : 0;
                        area.Width = Math.Max(1, Math.Min(area.Width + dw, _vm.CurrentWorld.TilesWide - area.X));
                        area.Height = Math.Max(1, Math.Min(area.Height + dh, _vm.CurrentWorld.TilesHigh - area.Y));
                    }
                    _vm.Selection.SelectionArea = area;
                }
                e.Handled = true;
                return true;

            // Navigation
            case "nav.scroll.up":
                scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Up, 10);
                _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                e.Handled = true;
                return true;
            case "nav.scroll.down":
                scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Down, 10);
                _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                e.Handled = true;
                return true;
            case "nav.scroll.left":
                scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Left, 10);
                _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                e.Handled = true;
                return true;
            case "nav.scroll.right":
                scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Right, 10);
                _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                e.Handled = true;
                return true;
            case "nav.scroll.up.fast":
                scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Up, 50);
                _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                e.Handled = true;
                return true;
            case "nav.scroll.down.fast":
                scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Down, 50);
                _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                e.Handled = true;
                return true;
            case "nav.scroll.left.fast":
                scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Left, 50);
                _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                e.Handled = true;
                return true;
            case "nav.scroll.right.fast":
                scrollValue = new ScrollEventArgs(TEdit.Editor.ScrollDirection.Right, 50);
                _vm.RequestScrollCommand.Execute(scrollValue).Subscribe();
                e.Handled = true;
                return true;
            case "nav.pan":
                _vm.RequestPanCommand.Execute(true).Subscribe();
                return true;
            case "nav.zoom.in":
                _vm.RequestZoomCommand.Execute(true).Subscribe();
                return true;
            case "nav.zoom.out":
                _vm.RequestZoomCommand.Execute(false).Subscribe();
                return true;
            case "nav.reset":
                if (_vm.ActiveTool != null)
                {
                    if (_vm.ActiveTool.Name == "Paste")
                        SetActiveTool("Arrow");
                    else
                        _vm.Selection.IsActive = false;
                }
                return true;

            // Tools
            case "tool.arrow":
                SetActiveTool("Arrow");
                return true;
            case "tool.brush":
                SetActiveTool("Brush");
                return true;
            case "tool.pencil":
                SetActiveTool("Pencil");
                return true;
            case "tool.fill":
                SetActiveTool("Fill");
                return true;
            case "tool.picker":
                if (!e.IsRepeat)
                {
                    _toolBeforePicker = _vm.ActiveTool?.Name == "Picker" ? null : _vm.ActiveTool;
                    _pickerKeyDownTime = DateTime.UtcNow;
                    SetActiveTool("Picker");
                }
                return true;
            case "tool.point":
                SetActiveTool("Point");
                return true;
            case "tool.selection":
                SetActiveTool("Selection");
                return true;
            case "tool.sprite":
                SetActiveTool("Sprite2");
                return true;
            case "tool.hammer":
                SetActiveTool("Hammer");
                return true;
            case "tool.morph":
                SetActiveTool("Morph");
                return true;

            // Toggles
            case "toggle.eraser":
                _vm.TilePicker.IsEraser = !_vm.TilePicker.IsEraser;
                return true;
            case "toggle.swap":
                _vm.TilePicker.Swap(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                return true;
            case "toggle.tile":
                _vm.TilePicker.TileStyleActive = !_vm.TilePicker.TileStyleActive;
                return true;
            case "toggle.wall":
                _vm.TilePicker.WallStyleActive = !_vm.TilePicker.WallStyleActive;
                return true;

            default:
                return false;
        }
    }

    private void SetActiveTool(string toolName)
    {
        var tool = _vm.Tools.FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase));
        if (tool != null)
        {
            _vm.SetActiveTool(tool);
        }
    }

    public void ZoomFocus(int x, int y)
    {
        if (MapView != null)
        {
            MapView.ZoomFocus(x, y);
        }
    }

    public void PanTo(int x, int y)
    {
        if (MapView != null)
        {
            MapView.CenterOnTile(x, y);
        }
    }

    private void Image_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var image = sender as System.Windows.Controls.Image;

        if (image == null) return;

        var clickLocation = e.GetPosition(image);
        var mapPointX = (int)(clickLocation.X * (TEdit.Render.RenderMiniMap.Resolution));
        var mapPointY = (int)(clickLocation.Y * (TEdit.Render.RenderMiniMap.Resolution));

        this.MapView.ZoomFocus(mapPointX, mapPointY);
    }

    private void WorldFileDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            string filelocation = Path.GetFullPath(files[0]);

            _vm.LoadWorld(filelocation);
        }
    }
	
	private void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Check if the left mouse button was clicked
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            // Prevent the default focus behavior on click
            e.Handled = true;

            // Ensure the selection area is active.
            if (!_vm.Selection.IsActive)
                return;

            // Build the list of tiles per the selection area.
            List<Tuple<Tile, Vector2Int32>> tileList = new();
            for (int x = _vm.Selection.SelectionArea.Left; x < _vm.Selection.SelectionArea.Right; x++)
            {
                for (int y = _vm.Selection.SelectionArea.Top; y < _vm.Selection.SelectionArea.Bottom; y++)
                {
                    tileList.Add(new Tuple<Tile, Vector2Int32>(_vm.CurrentWorld.Tiles[x, y], new Vector2Int32(x, y)));
                }
            }

            // Clear the selection for better viewing.
            _vm.Selection.SetRectangle(new Vector2Int32(0, 0), new Vector2Int32(0, 0));

            // Pass the tile data onto the new controller.
            UVEditorWindow uvEditorWindow = new(tileList, _vm);
            uvEditorWindow.Owner = this;
            uvEditorWindow.ShowDialog();
        }
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_vm);
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.Owner = this;
        aboutWindow.ShowDialog();
    }

    private void ShowWorldExplorer()
    {
        var explorerWindow = new WorldExplorerWindow(_vm);
        explorerWindow.Owner = this;
        explorerWindow.ShowDialog();
    }


    private GridLength _previousSidePanelWidth = new GridLength(440);
    private bool _isSidePanelCollapsed = false;
    private int _lastSelectedTabIndex = 0;

    private void ExpandSidePanel()
    {
        if (!_isSidePanelCollapsed)
            return;

        var sidePanelColumn = SidePanelColumn;
        sidePanelColumn.Width = _previousSidePanelWidth;
        sidePanelColumn.MinWidth = 200;
        SidePanelSplitter.IsEnabled = true;
        _isSidePanelCollapsed = false;
    }

    private void CollapseSidePanel()
    {
        if (_isSidePanelCollapsed)
            return;

        var sidePanelColumn = SidePanelColumn;
        _previousSidePanelWidth = sidePanelColumn.Width;
        sidePanelColumn.Width = new GridLength(50);
        sidePanelColumn.MinWidth = 50;
        SidePanelSplitter.IsEnabled = false;
        _isSidePanelCollapsed = true;
    }

    private void SidePanelTabs_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Find if a TabItem was clicked
        var clickedElement = e.OriginalSource as DependencyObject;
        while (clickedElement != null && clickedElement is not TabItem)
            clickedElement = System.Windows.Media.VisualTreeHelper.GetParent(clickedElement);

        if (clickedElement is not TabItem clickedTab)
            return;

        int clickedIndex = SidePanelTabs.Items.IndexOf(clickedTab);
        if (clickedIndex < 0)
            return;

        if (_isSidePanelCollapsed)
        {
            ExpandSidePanel();
        }
        else if (clickedIndex == SidePanelTabs.SelectedIndex)
        {
            CollapseSidePanel();
            e.Handled = true; // prevent tab from re-selecting
        }

        _lastSelectedTabIndex = clickedIndex;
    }
}
