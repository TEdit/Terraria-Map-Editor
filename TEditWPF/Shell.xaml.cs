using System.ComponentModel.Composition;
using System.Windows;
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
        }

        [Import]
        public WorldViewModel ViewModel
        {
            get { return (WorldViewModel) DataContext; }
            set { DataContext = value; }
        }
    }
}