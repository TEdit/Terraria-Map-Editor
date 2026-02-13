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

    public override void Execute()
    {
        if (_wvm.ExportTexturesAction == null)
        {
            System.Windows.MessageBox.Show("Textures not loaded yet. Please wait for texture loading to complete.");
            return;
        }

        _wvm.ExportTexturesAction();
    }
}
#endif
