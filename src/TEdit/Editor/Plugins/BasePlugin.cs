using ReactiveUI;
using TEdit.ViewModel;

namespace TEdit.Editor.Plugins;

public abstract class BasePlugin : ReactiveObject, IPlugin
{
    protected WorldViewModel _wvm;

    protected BasePlugin(WorldViewModel worldViewModel)
    {
        _wvm = worldViewModel;
    }

    public string Name { get; protected set; }

    public abstract void Execute();
}
