using System;
using System.Threading;

namespace TEdit.Scripting.Engine;

public class ScriptExecutionContext
{
    public int TimeoutMs { get; init; } = 60_000;
    public CancellationToken CancellationToken { get; init; }
    public Action<string>? OnLog { get; init; }
    public Action<string>? OnWarn { get; init; }
    public Action<string>? OnError { get; init; }
    public Action<double>? OnProgress { get; init; } // 0.0 - 1.0

    // Find results integration
    public Action<string, int, int, string, string?>? OnFindResult { get; init; }
    public Action? OnFindClear { get; init; }
    public Action<int>? OnFindNavigate { get; init; }
}
