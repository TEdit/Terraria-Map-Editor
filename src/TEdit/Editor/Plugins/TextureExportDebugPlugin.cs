#if DEBUG
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

public sealed class TextureExportDebugPlugin : BasePlugin
{
    public TextureExportDebugPlugin(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Name = "Export Textures (DEBUG)";
        TitleKey = "plugin_textureexport_menu";
    }

    public override async void Execute()
    {
        if (_wvm.ExportTexturesAction == null)
        {
            await App.DialogService.ShowWarningAsync(
                Properties.Language.dialog_export_textures_title,
                Properties.Language.dialog_export_textures_not_loaded);
            return;
        }

        _wvm.ExportTexturesAction();
    }
}
#endif
