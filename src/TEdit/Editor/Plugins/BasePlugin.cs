using TEdit.Common.Reactive;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

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