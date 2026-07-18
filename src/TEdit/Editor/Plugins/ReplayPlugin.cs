using System.Windows;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

public class ReplayPlugin : BasePlugin
{
    private ReplayPluginRecorderView _currentView;

    public ReplayPlugin(WorldViewModel worldViewModel) : base(worldViewModel)
    {
        Name = "Replay";
    }

    public override void Execute()
    {
        if (_wvm.CurrentWorld == null)
        {
            MessageBox.Show("Open a world first.", Name, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (_currentView != null && _currentView.IsLoaded)
        {
            _currentView.Activate();
            return;
        }

        _currentView = new ReplayPluginRecorderView();
        _currentView.Closed += (_, _) => _currentView = null;
        _currentView.Show();
    }
}
