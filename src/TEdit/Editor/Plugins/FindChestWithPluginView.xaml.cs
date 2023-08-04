using System.Windows;
using System.Windows.Forms;

namespace TEdit.Editor.Plugins;

/// <summary>
/// Interaction logic for ReplaceAllPlugin.xaml
/// </summary>
public partial class FindChestWithPluginView : Window
{
    public string ItemToFind { get; private set; }
    public bool CalculateDistance { get; private set; }
    public FindChestWithPluginView()
    {
        InitializeComponent();
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void SearchButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        ItemToFind = ItemLookup.Text;
        Close();
    }

    private void NUDCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        CalculateDistance = true;
    }

    private void NUDCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        CalculateDistance = false;
    }
}
