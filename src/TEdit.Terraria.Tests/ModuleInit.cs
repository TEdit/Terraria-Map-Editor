using System.Runtime.CompilerServices;
using ReactiveUI.Builder;

namespace TEdit.Terraria.Tests;

internal static class ModuleInit
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        RxAppBuilder.CreateReactiveUIBuilder().BuildApp();
    }
}
