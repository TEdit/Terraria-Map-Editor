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
            this.DataContext = ViewModelLocator.WorldViewModel;
            _vm = (WorldViewModel)DataContext;
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
        }

        private void HandleKeyDownEvent(object sender, KeyEventArgs e)
        { 
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
                SetActiveTool(World.ShortcutKeys[e.Key]);
            }
        }

        private void SetActiveTool(string toolName)
        {
            var tool = _vm.Tools.FirstOrDefault(t => t.Name == toolName);
            if (tool != null)
                _vm.SetTool.Execute(tool);
        }
    }
}
