using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using TEdit.ViewModels;

namespace TEdit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Export("MainWindow", typeof (Window))]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
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
        }

        [Import]
        public WorldViewModel ViewModel
        {
            get { return (WorldViewModel) DataContext; }
            set { DataContext = value; }
        }
    }
}