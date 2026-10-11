using System.Runtime.CompilerServices;
using ReactiveUI.Builder;

namespace TEdit.Terraria.Tests;

internal static class ModuleInit
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        TestWorldArchives.Prepare();

        // ReactiveUI's WPF scheduler needs a dispatcher on the initializing thread; test hosts have none by default.
        _ = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp();
    }
}
