using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using TEdit.Scripting.Api;
using TEdit.Scripting.Engine;
using TEdit.ViewModel;

namespace TEdit.Scripting.Execution;

public sealed class WorldViewModelScriptHost(WorldViewModel worldViewModel, Dispatcher dispatcher) : IScriptExecutionHost
{
    public object? CurrentWorld => worldViewModel.CurrentWorld;

    public Task InvokeAsync(Action action)
    {
        if (dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }
        return dispatcher.InvokeAsync(action).Task;
    }

    public ScriptApi CreateApi(ScriptExecutionContext context) => new(worldViewModel, context);
    public Task FinalizeExecutionAsync(ScriptApi api) => api.EndExecutionAsync();
}
