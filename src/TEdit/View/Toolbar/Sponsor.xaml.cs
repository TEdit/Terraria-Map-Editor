using System.Windows;
using System.Windows.Controls;
using TEdit.ViewModel;

namespace TEdit.View;

/// <summary>
/// Interaction logic for Sponsor.xaml
/// </summary>
public partial class Sponsor : UserControl
{
    public Sponsor()
    {
        InitializeComponent();
    }

    private async void PatreonButtonClick(object sender, RoutedEventArgs e)
    {
        await WorldViewModel.LaunchUrlAsync("https://www.patreon.com/bePatron?u=2278324");
    }

    private async void GithubButtonClick(object sender, RoutedEventArgs e)
    {
        await WorldViewModel.LaunchUrlAsync("https://github.com/TEdit/Terraria-Map-Editor");
    }

    private async void DiscordButtonClick(object sender, RoutedEventArgs e)
    {
        await WorldViewModel.LaunchUrlAsync("https://discord.gg/xHcHd7mfpn");
    }
}
