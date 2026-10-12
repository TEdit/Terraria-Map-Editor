using System.Runtime.CompilerServices;
using System.Windows.Threading;
using ReactiveUI.Builder;

namespace TEdit.Terraria.Tests;

internal static class ModuleInit
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        TestWorldArchives.Prepare();
        // The test host has no Application dispatcher for ReactiveUI's WPF scheduler.
        _ = Dispatcher.CurrentDispatcher;
        RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp();
    }
}
