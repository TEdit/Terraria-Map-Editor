using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TEdit.Mcp.Scripting;

internal static class ScriptProcess
{
    internal static async Task<ScriptDiagnostic?> Run(string executable, ScriptInput input,
        Func<ScriptMessage, Task<JsonElement>> dispatch, Action ready, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true,
            WorkingDirectory = AppContext.BaseDirectory
        };
        start.ArgumentList.Add(ScriptProtocol.WorkerArgument);
        using var worker = Process.Start(start) ?? throw new InvalidOperationException();
        try
        {
            using var job = ScriptJob.Attach(worker);
            using var registration = token.Register(job.Dispose);
            var writer = worker.StandardInput.BaseStream;
            var reader = worker.StandardOutput.BaseStream;
            if (await ScriptProtocol.Read<string>(reader, token) != "ready") throw new InvalidDataException();
            ready();
            await ScriptProtocol.Write(writer, input, token);
            int calls = 0;
            while (true)
            {
                var message = await ScriptProtocol.Read<ScriptMessage>(reader, token);
                token.ThrowIfCancellationRequested();
                if (message.Method == "finished") return message.Error;
                if (++calls > ScriptProtocol.CallLimit) throw new ScriptLimitException("call_limit");
                var reply = await dispatch(message);
                await ScriptProtocol.Write(writer, reply, token);
            }
        }
        catch (Exception) when (token.IsCancellationRequested) { throw new OperationCanceledException(token); }
        catch (EndOfStreamException) { return new("worker_terminated"); }
        finally
        {
            Stop(worker);
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            try { await worker.WaitForExitAsync(cleanup.Token); }
            catch (OperationCanceledException)
            {
                token.ThrowIfCancellationRequested();
                throw new ScriptLimitException("worker_cleanup_failed");
            }
        }
    }

    private static void Stop(Process worker)
    {
        try { if (!worker.HasExited) worker.Kill(); }
        catch (InvalidOperationException) { } // Already exited.
        catch (System.ComponentModel.Win32Exception) { } // Job closure independently terminates an assigned worker.
    }
}
