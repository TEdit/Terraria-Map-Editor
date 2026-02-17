using System;
using TEdit.Scripting.Engine;

namespace TEdit.Scripting.Api;

public class LogApi
{
    private readonly ScriptExecutionContext _context;

    public LogApi(ScriptExecutionContext context)
    {
        _context = context;
    }

    public void Print(string message) => _context.OnLog?.Invoke(message);
    public void Warn(string message) => _context.OnWarn?.Invoke(message);
    public void Error(string message) => _context.OnError?.Invoke(message);
    public void Progress(double value) => _context.OnProgress?.Invoke(Math.Clamp(value, 0.0, 1.0));
}
