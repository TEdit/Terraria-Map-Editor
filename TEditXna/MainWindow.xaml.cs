using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BCCL.Geometry.Primitives;
using TEditXNA.Terraria;
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
            else if (e.Key == Key.Y && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                _vm.RedoCommand.Execute(null);
            }
            else if (e.Key == Key.A && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (_vm.CurrentWorld != null)
                {
                    _vm.Selection.IsActive = true;
                    _vm.Selection.SetRectangle(new Vector2Int32(0,0), new Vector2Int32(_vm.CurrentWorld.TilesWide-1, _vm.CurrentWorld.TilesHigh-1));
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
            else if (World.ShortcutKeys.ContainsKey(e.Key))
            {
                string command = World.ShortcutKeys[e.Key];
                if (string.Equals("Eraser", command, StringComparison.InvariantCultureIgnoreCase))
                {
                    _vm.TilePicker.IsEraser = !_vm.TilePicker.IsEraser;
                }
                else
                {
                    SetActiveTool(command);
                }
            }
            e.Handled = true;
        }

        private void SetActiveTool(string toolName)
        {
            var tool = _vm.Tools.FirstOrDefault(t => t.Name == toolName);
            if (tool != null)
                _vm.SetTool.Execute(tool);
        }
    }
}
