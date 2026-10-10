using System.Runtime.CompilerServices;
using ReactiveUI.Builder;

namespace TEdit.Terraria.Tests;

internal static class ModuleInit
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        TestWorldArchives.Prepare();
        RxAppBuilder.CreateReactiveUIBuilder().BuildApp();

        // new World() reads static config; load it once so no test depends on another having run first.
        WorldConfiguration.Initialize();
    }
}
