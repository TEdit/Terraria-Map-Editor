using System.Windows;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins
{
    public class ReplaceAllPlugin : BasePlugin
    {
        public ReplaceAllPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Replace All Tiles";
        }

        public override async void Execute()
        {
            ReplaceAllPluginView view = new ReplaceAllPluginView();
            view.Owner = Application.Current.MainWindow;
            view.DataContext = _wvm;
            if (view.ShowDialog() == true)
            {
                if (!_wvm.ReplaceAll())
                {
                    await App.DialogService.ShowWarningAsync(
                        "Replace",
                        "Enable at least one mask to use replace. Masks define which tiles to match.");
                }
            }
        }
    }
}
