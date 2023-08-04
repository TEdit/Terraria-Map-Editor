using System.Windows;
using TEdit.ViewModel;

namespace TEdit.UI.Xaml;

/// <summary>
/// Interaction logic for NotificationsWindow.xaml
/// </summary>
public partial class NotificationsWindow : Window
{
    public NotificationsWindow()
    {
        InitializeComponent();
        DataContext = ViewModelLocator.WorldViewModel;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
