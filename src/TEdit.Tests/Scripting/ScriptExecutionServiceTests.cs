using System.Windows.Threading;
using Shouldly;
using TEdit.Editor;
using TEdit.Scripting.Api;
using TEdit.Scripting.Engine;
using TEdit.Scripting.Execution;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class ScriptExecutionServiceTests
{
    private static ScriptExecutionContext LocalContext() => new() { AccessProfile = ScriptAccessProfile.LocalSidebar };
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EnginesRunOnWorkerAndFinalizeOnUi(bool lua)
    {
        using var host = new TestHost();
        var gate = new WorldOperationCoordinator();
        var service = new ScriptExecutionService(host, gate);
        var logs = new List<string>();
        var context = new ScriptExecutionContext
        {
            AccessProfile = ScriptAccessProfile.LocalSidebar,
            OnLog = logs.Add
        };
        host.OnFinalize = () =>
        {
            gate.IsBusy.ShouldBeTrue();
            gate.TryAcquire(false, null, out _).ShouldBe(OperationRejection.Busy);
        };
        var engine = new CheckingEngine(lua ? new LuaScriptEngine() : new JintScriptEngine(), host.UiThreadId);
        var run = await service.ExecuteAsync(engine, "tile.setType(2, 3, 1); print('finished')", context);
        run.Script.Success.ShouldBeTrue(run.Script.Error);
        run.Started.ShouldBeTrue();
        host.World!.Tiles[2, 3].Type.ShouldBe((ushort)1);
        host.Undo.SavedTiles.ShouldContain((2, 3));
        logs.ShouldContain("finished");
        host.Finalizations.ShouldBe(1);
        host.CaptureThread.ShouldBe(host.UiThreadId);
        host.FinalizationThread.ShouldBe(host.UiThreadId);
        gate.IsBusy.ShouldBeFalse();
        gate.GetRevision(host.World)!.Value.Revision.ShouldBe(1);
    }

    [Fact]
    public async Task BusyRequestCannotCaptureApiOrRunEngine()
    {
        using var host = new TestHost();
        var gate = new WorldOperationCoordinator();
        var service = new ScriptExecutionService(host, gate);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = service.ExecuteAsync(new DelegateEngine(() =>
        {
            entered.SetResult();
            release.Task.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            return new(true, null, 0);
        }), "test", LocalContext());
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var second = await service.ExecuteAsync(new DelegateEngine(() => throw new Exception("must not run")), "test", LocalContext());
            second.Rejection.ShouldBe(OperationRejection.Busy);
            second.Started.ShouldBeFalse();
            host.Captures.ShouldBe(1);
        }
        finally { release.TrySetResult(); }
        (await first).Script.Success.ShouldBeTrue();
        host.Finalizations.ShouldBe(1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FailedAndCanceledEnginesStillFinalize(bool cancel)
    {
        using var host = new TestHost();
        var gate = new WorldOperationCoordinator();
        var service = new ScriptExecutionService(host, gate);
        var run = await service.ExecuteAsync(new DelegateEngine(() => cancel
            ? throw new OperationCanceledException()
            : throw new InvalidOperationException("engine failed")), "test", LocalContext());
        run.Script.Success.ShouldBeFalse();
        host.Finalizations.ShouldBe(1);
        gate.IsBusy.ShouldBeFalse();
        gate.GetRevision(host.World)!.Value.Revision.ShouldBe(1);
    }

    [Fact]
    public async Task FinalizationFailureIsReportedAndReleasesGate()
    {
        using var host = new TestHost { OnFinalize = () => throw new InvalidOperationException("render failed") };
        var gate = new WorldOperationCoordinator();
        var service = new ScriptExecutionService(host, gate);
        var run = await service.ExecuteAsync(new DelegateEngine(() => new(false, "original failure", 5)), "test", LocalContext());
        run.Script.Error!.ShouldContain("original failure");
        run.Script.Error!.ShouldBe(string.Format(Properties.Language.scripting_finalize, "original failure", "render failed"));
        gate.IsBusy.ShouldBeFalse();
        host.OnFinalize = null;
        (await service.ExecuteAsync(new DelegateEngine(() => new(true, null, 0)), "test", LocalContext())).Script.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task CanceledBeforeCaptureAndMissingWorldHaveNoSideEffects()
    {
        using var host = new TestHost();
        var gate = new WorldOperationCoordinator();
        var service = new ScriptExecutionService(host, gate);
        var never = new DelegateEngine(() => throw new Exception("must not run"));
        var canceled = await service.ExecuteAsync(never, "test", new() { AccessProfile = ScriptAccessProfile.LocalSidebar, CancellationToken = new(true) });
        canceled.Started.ShouldBeFalse();
        host.Captures.ShouldBe(0);
        host.World = null;
        var missing = await service.ExecuteAsync(never, "test", LocalContext());
        missing.Rejection.ShouldBe(OperationRejection.NoWorld);
        host.Captures.ShouldBe(0);
        host.Finalizations.ShouldBe(0);
        gate.IsBusy.ShouldBeFalse();
    }

    [Fact]
    public async Task StaleRequestIsRejectedBeforeCapture()
    {
        using var host = new TestHost();
        var gate = new WorldOperationCoordinator();
        gate.TrySetWorld(host.World);
        var old = gate.GetRevision(host.World);
        using (AcquireMutation(gate)) { }
        var run = await new ScriptExecutionService(host, gate).ExecuteAsync(new JintScriptEngine(), "tile.clear(2, 3)", LocalContext(), old);
        run.Rejection.ShouldBe(OperationRejection.StaleWorld);
        host.Captures.ShouldBe(0);
    }

    private static IDisposable AcquireMutation(WorldOperationCoordinator gate)
    {
        gate.TryAcquire(true, null, out var lease).ShouldBe(OperationRejection.None);
        return lease!;
    }

    [Theory]
    [InlineData(ScriptAccessProfile.Unspecified)]
    [InlineData(ScriptAccessProfile.InspectOnly)]
    [InlineData(ScriptAccessProfile.McpEditing)]
    [InlineData((ScriptAccessProfile)999)]
    public async Task NonlocalProfilesFailClosedBeforeCapturingHostObjects(ScriptAccessProfile profile)
    {
        using var host = new TestHost();
        var run = await new ScriptExecutionService(host, new()).ExecuteAsync(new JintScriptEngine(), "tile.clear(2,3)",
            new() { AccessProfile = profile });
        run.AccessDenied.ShouldBeTrue();
        host.Captures.ShouldBe(0);
        host.Finalizations.ShouldBe(0);
    }

    [Fact]
    public async Task OmittedProfileIsDeniedBeforeCapture()
    {
        using var host = new TestHost();
        var result = await new ScriptExecutionService(host, new()).ExecuteAsync(new JintScriptEngine(), "tile.clear(2,3)", new());
        result.AccessDenied.ShouldBeTrue();
        host.Captures.ShouldBe(0);
    }

    [Fact]
    public async Task UiOriginatingRunUsesInlineCaptureWithoutDeadlock()
    {
        using var host = new TestHost();
        var service = new ScriptExecutionService(host, new());
        var run = await host.RunOnUiAsync(() => service.ExecuteAsync(new JintScriptEngine(), "print('ui run')", LocalContext()))
            .WaitAsync(TimeSpan.FromSeconds(10));
        run.Script.Success.ShouldBeTrue(run.Script.Error);
        host.CaptureThread.ShouldBe(host.UiThreadId);
        host.FinalizationThread.ShouldBe(host.UiThreadId);
        host.InlineInvocations.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UserCancellationStopsActualEngineAndFinalizes(bool lua)
    {
        using var host = new TestHost();
        using var cancel = new CancellationTokenSource();
        var gate = new WorldOperationCoordinator();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var context = new ScriptExecutionContext
        {
            AccessProfile = ScriptAccessProfile.LocalSidebar,
            CancellationToken = cancel.Token,
            TimeoutMs = 5000,
            OnLog = _ => entered.TrySetResult()
        };
        var source = lua ? "print('ready'); while true do end" : "print('ready'); while(true) {}";
        var runTask = new ScriptExecutionService(host, gate).ExecuteAsync(
            lua ? new LuaScriptEngine() : new JintScriptEngine(), source, context);
        try { await entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); }
        finally { cancel.Cancel(); }
        var run = await runTask.WaitAsync(TimeSpan.FromSeconds(5));
        run.Script.Success.ShouldBeFalse();
        run.Script.Error.ShouldBe(Properties.Language.scripting_cancel);
        host.Finalizations.ShouldBe(1);
        gate.IsBusy.ShouldBeFalse();
        gate.GetRevision(host.World)!.Value.Revision.ShouldBe(1);
    }

    [Fact]
    public async Task TimeoutTokenReachesHostAndKeepsTimeoutClassification()
    {
        using var host = new TestHost();
        var gate = new WorldOperationCoordinator();
        var run = await new ScriptExecutionService(host, gate).ExecuteAsync(new HostCancellationEngine(), "test",
            new() { AccessProfile = ScriptAccessProfile.LocalSidebar, TimeoutMs = 100 });
        run.Script.Success.ShouldBeFalse();
        run.Script.Error.ShouldBe(string.Format(Properties.Language.scripting_timeout, 100));
        host.CapturedContext!.CancellationToken.IsCancellationRequested.ShouldBeTrue();
        host.Finalizations.ShouldBe(1);
        gate.IsBusy.ShouldBeFalse();
    }

    private sealed class HostCancellationEngine : IScriptEngine
    {
        public string Name => "host cancellation test";
        public string FileExtension => ".test";
        public ScriptResult Execute(string code, ScriptApi api, ScriptExecutionContext context)
        {
            context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(5)).ShouldBeTrue();
            context.CancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException("Timeout was not propagated");
        }
    }

    private sealed class DelegateEngine(Func<ScriptResult> execute) : IScriptEngine
    {
        public string Name => "test";
        public string FileExtension => ".test";
        public ScriptResult Execute(string code, ScriptApi api, ScriptExecutionContext context) => execute();
    }

    private sealed class CheckingEngine(IScriptEngine inner, int uiThreadId) : IScriptEngine
    {
        public string Name => inner.Name;
        public string FileExtension => inner.FileExtension;
        public ScriptResult Execute(string code, ScriptApi api, ScriptExecutionContext context)
        {
            Environment.CurrentManagedThreadId.ShouldNotBe(uiThreadId);
            return inner.Execute(code, api, context);
        }
    }

    private sealed class TestHost : IScriptExecutionHost, IDisposable
    {
        private readonly Thread _thread;
        private readonly Dispatcher _dispatcher;
        public World? World { get; set; } = TestWorldFactory.CreateSmallWorld();
        public object? CurrentWorld => World;
        public NoOpUndoManager Undo { get; } = new();
        public int Captures { get; private set; }
        public int Finalizations { get; private set; }
        public int CaptureThread { get; private set; }
        public int FinalizationThread { get; private set; }
        public int UiThreadId => _thread.ManagedThreadId;
        public Action? OnFinalize { get; set; }
        public ScriptExecutionContext? CapturedContext { get; private set; }
        public int InlineInvocations { get; private set; }

        public TestHost()
        {
            var ready = new TaskCompletionSource<Dispatcher>(TaskCreationOptions.RunContinuationsAsynchronously);
            _thread = new Thread(() =>
            {
                ready.SetResult(Dispatcher.CurrentDispatcher);
                Dispatcher.Run();
            }) { IsBackground = true };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
            _dispatcher = ready.Task.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
        }

        public Task<T> RunOnUiAsync<T>(Func<Task<T>> action) => _dispatcher.InvokeAsync(action).Task.Unwrap();
        public Task InvokeAsync(Action action)
        {
            if (_dispatcher.CheckAccess())
            {
                InlineInvocations++;
                action();
                return Task.CompletedTask;
            }
            return _dispatcher.InvokeAsync(action).Task;
        }
        public ScriptApi CreateApi(ScriptExecutionContext context)
        {
            CaptureThread = Environment.CurrentManagedThreadId;
            Captures++;
            CapturedContext = context;
            return new ScriptApi(World!, Undo, new Selection(), context);
        }

        public Task FinalizeExecutionAsync(ScriptApi api)
        {
            FinalizationThread = Environment.CurrentManagedThreadId;
            Finalizations++;
            OnFinalize?.Invoke();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
            _thread.Join(TimeSpan.FromSeconds(10)).ShouldBeTrue();
        }
    }
}
