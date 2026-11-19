using System;
using System.Threading;

namespace TEdit.Scripting.Engine;

public class ScriptExecutionContext
{
    public Execution.ScriptAccessProfile AccessProfile { get; init; }
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

    internal ScriptExecutionContext WithCancellation(CancellationToken token) => new()
    {
        AccessProfile = AccessProfile,
        TimeoutMs = TimeoutMs,
        CancellationToken = token,
        OnLog = OnLog,
        OnWarn = OnWarn,
        OnError = OnError,
        OnProgress = OnProgress,
        OnFindResult = OnFindResult,
        OnFindClear = OnFindClear,
        OnFindNavigate = OnFindNavigate
    };
}
