using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Editor;
using TEdit.ViewModel;
using TEdit.Properties;
using TEdit.UI.Xaml;
using TEdit.View.Popups;
using System.IO;

namespace TEdit;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    private WorldViewModel _vm;
    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModelLocator.WorldViewModel;
        _vm = (WorldViewModel)DataContext;
        AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
        AddHandler(Keyboard.KeyUpEvent, (KeyEventHandler)HandleKeyUpEvent);
    }

    void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (Settings.Default.Telemetry == -1)
        {
            _vm.EnableTelemetry = true;
        }

        if (Settings.Default.ShowNews)
        {
            var w = new NotificationsWindow();
            w.Owner = Application.Current.MainWindow;
            w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            w.ShowDialog();
        }
    }

    private void HandleKeyUpEvent(object sender, KeyEventArgs e)
    {
        var command = App.ShortcutKeys.Get(e);
        if (command == null) return;

        switch (command)
        {
            case "pan":
                if (_vm.RequestPanCommand.CanExecute(false))
                    _vm.RequestPanCommand.Execute(false);
                break;
        }
    }

    private void HandleKeyDownEvent(object sender, KeyEventArgs e)
    {
        if (!(e.Source is View.WorldRenderXna)) return;

        try
        {
            ScrollEventArgs scrollValue = null;

            var command = App.ShortcutKeys.Get(e.Key, e.KeyboardDevice.Modifiers);
            if (command == null) return;

            switch (command)
            {
                case "copy":
                    if (_vm.CopyCommand.CanExecute(null))
                    {
                        _vm.CopyCommand.Execute(null);
                        _vm.SelectedTabIndex = 3;
                    }
                    break;
                case "paste":
                    if (_vm.PasteCommand.CanExecute(null))
                    {
                        _vm.PasteCommand.Execute(null);
                        _vm.SelectedTabIndex = 3;
                    }
                    break;
                case "undo":
                    _vm.UndoCommand.Execute(null);
                    break;
                case "redo":
                    _vm.RedoCommand.Execute(null);
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
                    if (_vm.OpenCommand.CanExecute(null))
                        _vm.OpenCommand.Execute(null);
                    break;
                case "save":
                    if (_vm.SaveCommand.CanExecute(null))
                        _vm.SaveCommand.Execute(null);
                    break;
                case "saveas":
                    if (_vm.SaveAsCommand.CanExecute(null))
                        _vm.SaveAsCommand.Execute(null);
                    break;
                case "saveasversion":
                    if (_vm.SaveAsVersionCommand.CanExecute(null))
                        _vm.SaveAsVersionCommand.Execute(null);
                    break;
                case "reloadworld":
                    if (_vm.ReloadCommand.CanExecute(null))
                        _vm.ReloadCommand.Execute(null);
                    break;
                case "deleteselection":
                    if (_vm.DeleteCommand.CanExecute(null))
                        _vm.DeleteCommand.Execute(null);
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
                    scrollValue = new ScrollEventArgs(ScrollDirection.Up, 10);
                    if (_vm.RequestScrollCommand.CanExecute(scrollValue))
                        _vm.RequestScrollCommand.Execute(scrollValue);
                    e.Handled = true;
                    break;
                case "scrollupfast":
                    scrollValue = new ScrollEventArgs(ScrollDirection.Up, 50);
                    if (_vm.RequestScrollCommand.CanExecute(scrollValue))
                        _vm.RequestScrollCommand.Execute(scrollValue);
                    e.Handled = true;
                    break;
                case "scrollright":
                    scrollValue = new ScrollEventArgs(ScrollDirection.Right, 10);
                    if (_vm.RequestScrollCommand.CanExecute(scrollValue))
                        _vm.RequestScrollCommand.Execute(scrollValue);
                    e.Handled = true;
                    break;
                case "scrollrightfast":
                    scrollValue = new ScrollEventArgs(ScrollDirection.Right, 50);
                    if (_vm.RequestScrollCommand.CanExecute(scrollValue))
                        _vm.RequestScrollCommand.Execute(scrollValue);
                    e.Handled = true;
                    break;
                case "scrolldown":
                    scrollValue = new ScrollEventArgs(ScrollDirection.Down, 10);
                    if (_vm.RequestScrollCommand.CanExecute(scrollValue))
                        _vm.RequestScrollCommand.Execute(scrollValue);
                    e.Handled = true;
                    break;
                case "scrolldownfast":
                    scrollValue = new ScrollEventArgs(ScrollDirection.Down, 50);
                    if (_vm.RequestScrollCommand.CanExecute(scrollValue))
                        _vm.RequestScrollCommand.Execute(scrollValue);
                    e.Handled = true;
                    break;
                case "scrollleft":
                    scrollValue = new ScrollEventArgs(ScrollDirection.Left, 10);
                    if (_vm.RequestScrollCommand.CanExecute(scrollValue))
                        _vm.RequestScrollCommand.Execute(scrollValue);
                    e.Handled = true;
                    break;
                case "scrollleftfast":
                    scrollValue = new ScrollEventArgs(ScrollDirection.Left, 50);
                    if (_vm.RequestScrollCommand.CanExecute(scrollValue))
                        _vm.RequestScrollCommand.Execute(scrollValue);
                    e.Handled = true;
                    break;
                case "pan":
                    if (_vm.RequestPanCommand.CanExecute(true))
                        _vm.RequestPanCommand.Execute(true);
                    break;
                case "zoomin":
                    if (_vm.RequestZoomCommand.CanExecute(true))
                        _vm.RequestZoomCommand.Execute(true);
                    break;
                case "zoomout":
                    if (_vm.RequestZoomCommand.CanExecute(false))
                        _vm.RequestZoomCommand.Execute(false);
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

    private void SetActiveTool(string toolName)
    {
        var tool = _vm.Tools.FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase));
        if (tool != null)
        {
            _vm.SetTool.Execute(tool);
        }
    }

    public void ZoomFocus(int x, int y)
    {
        if (MapView != null)
        {
            MapView.ZoomFocus(x, y);
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
            uvEditorWindow.ShowDialog();
        }
    }
}
