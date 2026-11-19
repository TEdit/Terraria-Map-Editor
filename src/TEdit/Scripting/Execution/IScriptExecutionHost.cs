using System;
using System.Threading.Tasks;
using TEdit.Scripting.Api;
using TEdit.Scripting.Engine;

namespace TEdit.Scripting.Execution;

/// <summary>UI-affine capture and finalization, independent of the script interpreter.</summary>
public interface IScriptExecutionHost
{
    object? CurrentWorld { get; }
    Task InvokeAsync(Action action);
    ScriptApi CreateApi(ScriptExecutionContext context);
    Task FinalizeExecutionAsync(ScriptApi api);
}
