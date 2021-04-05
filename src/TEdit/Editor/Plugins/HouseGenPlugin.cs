using System.Windows;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins
{
    public class HouseGenPlugin : BasePlugin
    {
        HouseGenPluginView view;

        public HouseGenPlugin(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Name = "Procedural House Generator";
        }

        public override void Execute()
        {
            if (view == null)
            {
                view = new();
                view.Owner = Application.Current.MainWindow;
                view.WorldViewModel = _wvm;
                view.DataContext = view;
                view.Show();
                _wvm.SelectedTabIndex = 3;
            }
            else
            {
                view.Show();
                _wvm.SelectedTabIndex = 3;
            }
        }
    }
}
