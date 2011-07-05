using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using TEdit.Tools;
using TEdit.ViewModels;

namespace TEdit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Export("MainWindow", typeof(Window))]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
        }

        [Import]
        public WorldViewModel ViewModel
        {
            get { return (WorldViewModel)DataContext; }
            set { DataContext = value; }
        }

        private void HandleKeyDownEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (ViewModel.CopyToClipboard.CanExecute(null))
                    ViewModel.CopyToClipboard.Execute(null);
            }
            else if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (ViewModel.PasteFromClipboard.CanExecute(null))
                    ViewModel.PasteFromClipboard.Execute(null);
            }
            else if (e.Key == Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                ViewModel.Undo.Execute(null);
            }
            else if (e.Key == Key.Y && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                ViewModel.Redo.Execute(null);
            }
            else if (e.Key == Key.Delete)
            {
                ViewModel.DeleteSelection();
            }
            else if (e.Key == Key.S)
            {
                Lazy<ITool, IOrderMetadata> tool = ViewModel.Tools.FirstOrDefault(t => t.Value.Name == "Selection");
                if (tool != null)
                {
                    ViewModel.SetTool.Execute(tool.Value);
                }
            }
            else if (e.Key == Key.A)
            {
                Lazy<ITool, IOrderMetadata> tool = ViewModel.Tools.FirstOrDefault(t => t.Value.Name == "Arrow");
                if (tool != null)
                {
                    ViewModel.SetTool.Execute(tool.Value);
                }
            }
            else if (e.Key == Key.P)
            {
                Lazy<ITool, IOrderMetadata> tool = ViewModel.Tools.FirstOrDefault(t => t.Value.Name == "Spawn Point Tool");
                if (tool != null)
                {
                    ViewModel.SetTool.Execute(tool.Value);
                }
            }
            else if (e.Key == Key.D)
            {
                Lazy<ITool, IOrderMetadata> tool = ViewModel.Tools.FirstOrDefault(t => t.Value.Name == "Dungeon Point Tool");
                if (tool != null)
                {
                    ViewModel.SetTool.Execute(tool.Value);
                }
            }
            else if (e.Key == Key.E)
            {
                Lazy<ITool, IOrderMetadata> tool = ViewModel.Tools.FirstOrDefault(t => t.Value.Name == "Pencil");
                if (tool != null)
                {
                    ViewModel.SetTool.Execute(tool.Value);
                }
            }
            else if (e.Key == Key.B)
            {
                Lazy<ITool, IOrderMetadata> tool = ViewModel.Tools.FirstOrDefault(t => t.Value.Name == "Brush");
                if (tool != null)
                {
                    ViewModel.SetTool.Execute(tool.Value);
                }
            }

        }
    }
}