using System.Globalization;
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
        : Properties.Language.ResourceManager.GetString(TitleKey, CultureInfo.CurrentUICulture) ?? Name;

    protected string TitleKey { get; set; }

    public async System.Threading.Tasks.Task ExecuteAsync()
    {
        using var operation = _wvm.BeginWorldOperation("Plugin");
        if (operation == null) return;
        await ExecuteCoreAsync();
    }

    protected abstract System.Threading.Tasks.Task ExecuteCoreAsync();
}
