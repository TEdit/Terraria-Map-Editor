using GalaSoft.MvvmLight;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Plugins
{
    public abstract class BasePlugin : ObservableObject, IPlugin
    {
        protected WorldViewModel _wvm;

        protected BasePlugin(WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
        }

        public string Name { get; protected set; }

        public abstract void Execute();
    }
}