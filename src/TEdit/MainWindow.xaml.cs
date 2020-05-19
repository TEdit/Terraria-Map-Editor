using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TEdit.Geometry.Primitives;
using TEditXNA.Terraria;
using TEditXna.Editor;
using TEditXna.ViewModel;

namespace TEditXna
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WorldViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            Width = World.AppSize.X;
            Height = World.AppSize.Y;
            DataContext = ViewModelLocator.WorldViewModel;
            _vm = (WorldViewModel)DataContext;
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {



            //string fname = Application.Current.Properties["OpenFile"].ToString();
            //if (!string.IsNullOrWhiteSpace(fname))
            //{
            //    _vm.LoadWorld(fname);
            //}
            //e.Handled = false;
        }

        private void HandleKeyDownEvent(object sender, KeyEventArgs e)
        {
            try
            {
                if (!(e.Source is View.WorldRenderXna))
                    return;

                if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (_vm.CopyCommand.CanExecute(null))
                        _vm.CopyCommand.Execute(null);
                }
                else if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (_vm.PasteCommand.CanExecute(null))
                        _vm.PasteCommand.Execute(null);
                }
                else if (e.Key == Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    _vm.UndoCommand.Execute(null);
                }
                else if (e.Key == Key.OemPlus && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (_vm.RequestZoomCommand.CanExecute(true))
                        _vm.RequestZoomCommand.Execute(true);
                }
                else if (e.Key == Key.OemMinus && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (_vm.RequestZoomCommand.CanExecute(false))
                        _vm.RequestZoomCommand.Execute(false);
                }
                else if (e.Key == Key.Y && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    _vm.RedoCommand.Execute(null);
                }
                else if (e.Key == Key.A && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (_vm.CurrentWorld != null)
                    {
                        _vm.Selection.IsActive = true;
                        _vm.Selection.SetRectangle(new Vector2Int32(0, 0),
                            new Vector2Int32(_vm.CurrentWorld.TilesWide - 1, _vm.CurrentWorld.TilesHigh - 1));
                    }
                }
                else if (e.Key == Key.D && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (_vm.CurrentWorld != null)
                    {
                        _vm.Selection.IsActive = false;
                    }
                }
                else if (e.Key == Key.S && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (_vm.SaveCommand.CanExecute(null))
                        _vm.SaveCommand.Execute(null);
                }
                else if (e.Key == Key.O && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (_vm.OpenCommand.CanExecute(null))
                        _vm.OpenCommand.Execute(null);
                }
                else if (e.Key == Key.Delete)
                {
                    if (_vm.DeleteCommand.CanExecute(null))
                        _vm.DeleteCommand.Execute(null);
                }
                else if (e.Key == Key.Escape)
                {
                    if (_vm.ActiveTool != null)
                    {
                        if (_vm.ActiveTool.Name == "Paste")
                            SetActiveTool("Arrow");
                        else
                            _vm.Selection.IsActive = false;
                    }
                }
                else if (e.Key == Key.Up)
                {
                    if (_vm.RequestScrollCommand.CanExecute(ScrollDirection.Up))
                        _vm.RequestScrollCommand.Execute(ScrollDirection.Up);
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    if (_vm.RequestScrollCommand.CanExecute(ScrollDirection.Down))
                        _vm.RequestScrollCommand.Execute(ScrollDirection.Down);
                    e.Handled = true;
                }
                else if (e.Key == Key.Left)
                {
                    if (_vm.RequestScrollCommand.CanExecute(ScrollDirection.Left))
                        _vm.RequestScrollCommand.Execute(ScrollDirection.Left);
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    if (_vm.RequestScrollCommand.CanExecute(ScrollDirection.Right))
                        _vm.RequestScrollCommand.Execute(ScrollDirection.Right);
                    e.Handled = true;
                }
                else if (World.ShortcutKeys.ContainsKey(e.Key))
                {
                    string command = World.ShortcutKeys[e.Key];
                    if (string.Equals("Eraser", command, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _vm.TilePicker.IsEraser = !_vm.TilePicker.IsEraser;
                    }
                    else if (string.Equals("Swap", command, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _vm.TilePicker.Swap(Keyboard.Modifiers);
                    }
                    else
                    {
                        SetActiveTool(command);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogException(ex);
            }
        }

        private void SetActiveTool(string toolName)
        {
            var tool = _vm.Tools.FirstOrDefault(t => t.Name == toolName);
            if (tool != null)
                _vm.SetTool.Execute(tool);
        }

        public void ZoomFocus(int x, int y)
        {
            if (MapView != null)
            {
                MapView.ZoomFocus(x, y);
            }
        }

    }
}
