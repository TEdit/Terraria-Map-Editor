using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TEdit.Terraria;
using TEdit.View.Popups;
using TEdit.ViewModel;

namespace TEdit.View;

/// <summary>
/// Interaction logic for SpriteView2.xaml
/// </summary>
public partial class SpriteView2 : UserControl
{
    public SpriteView2()
    {
        InitializeComponent();
    }

    private void EditSpriteSheet_Click(object sender, RoutedEventArgs e)
    {
        var wvm = DataContext as WorldViewModel;
        if (wvm?.SelectedSpriteSheet == null) return;

        var spriteSheet = wvm.SelectedSpriteSheet;
        var tileId = spriteSheet.Tile;

        // Get the TileProperty for this sprite
        if (tileId >= WorldConfiguration.TileProperties.Count) return;
        var tileProperty = WorldConfiguration.TileProperties[tileId];

        // Try to get the tile texture as a WriteableBitmap
        WriteableBitmap? texture = null;
        if (wvm.Textures?.Tiles.TryGetValue(tileId, out var tex2d) == true && tex2d != null)
        {
            texture = tex2d.Texture2DToWriteableBitmap();
        }

        var editorVm = new SpriteSheetEditorViewModel(tileProperty, texture);
        var editorWindow = new SpriteSheetEditorWindow(editorVm)
        {
            Owner = Window.GetWindow(this)
        };

        editorWindow.Closed += (_, _) =>
        {
            if (editorVm.WasSaved)
            {
                // Rebuild only the edited tile's sprite sheet, not all sprites
                wvm.RebuildSpriteForTile(tileId);
                wvm.RegeneratePreviewsForTile(tileId);
                wvm.InitSpriteViews();
            }
        };

        editorWindow.Show();
    }
}
