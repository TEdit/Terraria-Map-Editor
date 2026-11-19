using System;
using System.Threading;
using System.Threading.Tasks;
using TEdit.Scripting.Api;
using TEdit.Scripting.Engine;

namespace TEdit.Scripting.Execution;

public sealed record ScriptRunResult(ScriptResult Script, OperationRejection Rejection = OperationRejection.None,
    bool Started = false, bool AccessDenied = false);

/// <summary>
/// Owns one script's capture, worker execution, and UI finalization. The lease is
/// retained until finalization completes, including error and cancellation paths.
/// </summary>
public sealed class ScriptExecutionService(IScriptExecutionHost host, WorldOperationCoordinator coordinator)
{
    public async Task<ScriptRunResult> ExecuteAsync(IScriptEngine engine, string code,
        ScriptExecutionContext context, WorldRevision? expected = null)
    {
        var denial = ScriptAccessPolicy.GetDenial(context.AccessProfile);
        if (denial != null) return new(new(false, denial, 0), AccessDenied: true);
        if (string.IsNullOrWhiteSpace(code)) return new(new(false, Properties.Language.scripting_empty, 0));
        if (context.TimeoutMs <= 0) return new(new(false, Properties.Language.scripting_timeout_positive, 0));

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
        timeout.CancelAfter(context.TimeoutMs);
        var executionContext = context.WithCancellation(timeout.Token);

        WorldOperationCoordinator.Lease? lease = null;
        ScriptApi? api = null;
        var rejection = OperationRejection.None;
        var started = false;
        CancellationTokenRegistration cancelRegistration = default;
        ScriptResult result = new(false, Properties.Language.scripting_start, 0);
        try
        {
            await host.InvokeAsync(() =>
            {
                executionContext.CancellationToken.ThrowIfCancellationRequested();
                rejection = coordinator.TrySetWorld(host.CurrentWorld);
                if (rejection != OperationRejection.None) return;
                rejection = coordinator.TryAcquire(true, expected, out lease, name: "Script");
                if (rejection != OperationRejection.None) return;
                using var ownership = lease!.Enter();
                cancelRegistration = coordinator.ActiveCancellation.Register(timeout.Cancel);
                api = host.CreateApi(executionContext);
                api.BeginExecution();
            });

            if (rejection != OperationRejection.None)
                result = new(false, rejection switch
                {
                    OperationRejection.NoWorld => Properties.Language.find_no_world_loaded,
                    OperationRejection.Busy => Properties.Language.scripting_busy,
                    _ => Properties.Language.scripting_stale
                }, 0);
            else
            {
                // Do not cancel scheduling: the worker checks cancellation and the finally
                // path must still finalize an API that has already been captured.
                result = await Task.Run(() =>
                {
                    using var ownership = lease!.Enter();
                    executionContext.CancellationToken.ThrowIfCancellationRequested();
                    started = true;
                    return engine.Execute(code, api!, executionContext);
                });
            }
        }
        catch (OperationCanceledException)
        {
            result = new(false, Properties.Language.scripting_cancel, 0);
        }
        catch (Exception ex)
        {
            result = new(false, ex.Message, 0);
        }
        finally
        {
            if (!result.Success && timeout.IsCancellationRequested)
                result = result with { Error = context.CancellationToken.IsCancellationRequested || coordinator.ActiveCancellation.IsCancellationRequested
                    ? Properties.Language.scripting_cancel : string.Format(Properties.Language.scripting_timeout, context.TimeoutMs) };
            try
            {
                if (api != null)
                {
                    Task? finalization = null;
                    await host.InvokeAsync(() =>
                    {
                        using var ownership = lease!.Enter();
                        finalization = host.FinalizeExecutionAsync(api);
                    });
                    await finalization!;
                }
            }
            catch (Exception ex)
            {
                result = result with
                {
                    Success = false,
                    Error = string.Format(Properties.Language.scripting_finalize, result.Error, ex.Message).Trim()
                };
            }
            finally
            {
                api?.Dispose();
                cancelRegistration.Dispose();
                lease?.Dispose();
            }
        }

        return new(result, rejection, started);
    }
}
