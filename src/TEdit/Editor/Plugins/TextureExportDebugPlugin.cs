#if DEBUG
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

public sealed class TextureExportDebugPlugin : BasePlugin
{
    public TextureExportDebugPlugin(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Name = "Export Textures (DEBUG)";
    }

    public override async void Execute()
    {
        if (_wvm.ExportTexturesAction == null)
        {
            await App.DialogService.ShowWarningAsync("Export Textures",
                "Textures not loaded yet. Please wait for texture loading to complete.");
            return;
        }

        _wvm.ExportTexturesAction();
    }
}
#endif
