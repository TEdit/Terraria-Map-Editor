using System.Text;
using System.Windows;
using TEdit.Editor;
using TEdit.Terraria;
using Wpf.Ui.Controls;
using Lang = TEdit.Properties.Language;

namespace TEdit.View.Popups;

public partial class AboutWindow : FluentWindow
{
    public AboutWindow()
    {
        InitializeComponent();
        PopulateInfo();
    }

    private void PopulateInfo()
    {
        var sb = new StringBuilder();

        var version = App.Version?.WithoutMetadata().ToString() ?? "Unknown";
        var terrariaVersion = DependencyChecker.GetTerrariaVersion() ?? Lang.about_terraria_not_found;
        var texturesAvailable = DependencyChecker.VerifyTerraria();
        var contentPath = DependencyChecker.PathToContent ?? Lang.about_terraria_not_found;

        sb.AppendLine($"TEdit {version}");
        sb.AppendLine();

        sb.AppendLine($"{Lang.about_compatible_version}:  {WorldConfiguration.CompatibleVersion}");
        sb.AppendLine($"{Lang.about_terraria_version}:  {terrariaVersion}");
        sb.AppendLine($"{Lang.about_textures_available}:  {(texturesAvailable ? "Yes" : "No")}");
        sb.AppendLine($"{Lang.about_content_path}:");
        sb.AppendLine($"  {contentPath}");
        sb.AppendLine();

        sb.AppendLine($"— {Lang.about_data_stats} —");
        sb.AppendLine($"{Lang.about_tiles_loaded}:  {WorldConfiguration.TileProperties.Count:N0}");
        sb.AppendLine($"{Lang.about_walls_loaded}:  {WorldConfiguration.WallProperties.Count:N0}");
        sb.AppendLine($"{Lang.about_items_loaded}:  {WorldConfiguration.ItemProperties.Count:N0}");
        sb.AppendLine();

        sb.AppendLine($"— System —");
        sb.AppendLine($"OS:  {DependencyChecker.GetOsVersion()}");
        sb.AppendLine($".NET:  {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");

        InfoText.Text = sb.ToString();
    }
}
