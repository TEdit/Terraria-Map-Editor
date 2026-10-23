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

    public string Title => string.IsNullOrEmpty(TitleKey)
        ? Name
        : Properties.Language.ResourceManager.GetString(TitleKey) ?? Name;

    protected string TitleKey { get; set; }

    public abstract void Execute();
}
