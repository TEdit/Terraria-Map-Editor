using System.Windows;
using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View;

/// <summary>
/// Interaction logic for PaintModeView.xaml
/// </summary>
public partial class PaintModeView : UserControl
{
    public PaintModeView()
    {
        InitializeComponent();
    }

    private WorldViewModel? Vm => DataContext as WorldViewModel;

    private void WireModeOff_Click(object sender, RoutedEventArgs e) => Vm?.SetWireModeOff();
    private void WireMode90_Click(object sender, RoutedEventArgs e) => Vm?.SetWireMode90();
    private void WireMode45_Click(object sender, RoutedEventArgs e) => Vm?.SetWireMode45();
    private void WireDirAuto_Click(object sender, RoutedEventArgs e) => Vm?.SetWireDirectionAuto();
    private void WireDirH_Click(object sender, RoutedEventArgs e) => Vm?.SetWireDirectionH();
    private void WireDirV_Click(object sender, RoutedEventArgs e) => Vm?.SetWireDirectionV();
}
