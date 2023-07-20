using Avalonia.Controls;
using TEdit.Desktop.ViewModels;

namespace TEdit.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = this.CreateInstance<MainWindowViewModel>();
    }
}
