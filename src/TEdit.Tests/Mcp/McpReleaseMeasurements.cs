using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Threading;
using Xunit;
using Xunit.Abstractions;

namespace TEdit.Tests.Mcp;

[Collection("Production editor")]
public class McpReleaseMeasurements(ITestOutputHelper output)
{
    private sealed class MeasurementFactAttribute : FactAttribute
    {
        public MeasurementFactAttribute()
        {
            if (Environment.GetEnvironmentVariable("TEDIT_MCP_MEASURE") is not ("small" or "large"))
                Skip = "Run each world size in a fresh test process with TEDIT_MCP_MEASURE=small or large.";
        }
    }

    [MeasurementFact]
    public async Task MeasureLoadedEditor()
    {
        bool large = Environment.GetEnvironmentVariable("TEDIT_MCP_MEASURE") == "large";
        var (width, height) = large ? (8400, 2400) : (4200, 1200);
        await using var editor = new McpEditorSession(width, height);
        editor.Deadline.CancelAfter(TimeSpan.FromMinutes(2));
        await editor.Open(scripts: true);
        using var stopSamples = CancellationTokenSource.CreateLinkedTokenSource(editor.Deadline.Token);
        var delays = new List<double>();
        async Task SampleInput()
        {
            while (!stopSamples.IsCancellationRequested)
            {
                var start = Stopwatch.GetTimestamp();
                var atInput = await editor.Ui.InvokeAsync(Stopwatch.GetTimestamp, DispatcherPriority.Input, stopSamples.Token);
                delays.Add(Stopwatch.GetElapsedTime(start, atInput).TotalMilliseconds);
                await Task.Delay(10, stopSamples.Token);
            }
        }

        var sampler = SampleInput();
        double cancelMs;
        int resultBytes;
        try
        {
            await editor.Expect("draw_rectangle", new()
            {
                ["x"] = 0, ["y"] = 0, ["endX"] = 255, ["endY"] = 255, ["material"] = 3,
                ["layer"] = "wall", ["filled"] = true, ["thickness"] = 1, ["selectionOnly"] = false, ["matchId"] = -1
            }, 65536);
            var region = await editor.Writer.CallToolAsync("read_region", new Dictionary<string, object?>
            { ["x"] = 0, ["y"] = 0, ["width"] = 8, ["height"] = 8 }, cancellationToken: editor.Deadline.Token);
            Assert.False(region.IsError);
            resultBytes = Encoding.UTF8.GetByteCount(region.StructuredContent!.Value.GetRawText());

            // Wait for an actual write before canceling. Keep the worker busy with bounded reads.
            var changed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var timer = await editor.Ui.InvokeAsync(() =>
            {
                var t = new DispatcherTimer(DispatcherPriority.Input) { Interval = TimeSpan.FromMilliseconds(1) };
                t.Tick += (_, _) => { if (editor.World.Tiles[0, 0].Wall == 1) changed.TrySetResult(); };
                t.Start();
                return t;
            });
            try
            {
                var id = await editor.Start("execute_script", new() { ["language"] = "javascript",
                    ["source"] = "draw.fillRegion(0,0,99,99,'wall',1,false,-1); for(let i=0;i<240;i++) tile.getWall(0,0);" });
                await changed.Task.WaitAsync(editor.Deadline.Token);
                var clock = Stopwatch.StartNew();
                await editor.Call(editor.Observer, "cancel_operation", new() { ["operationId"] = id });
                var op = await editor.Finish(id);
                cancelMs = clock.Elapsed.TotalMilliseconds;
                Assert.Equal("canceled", op.GetProperty("status").GetString());
                Assert.True(op.GetProperty("partialChanges").GetBoolean());
            }
            finally { await editor.Ui.InvokeAsync(timer.Stop); }
        }
        finally
        {
            await stopSamples.CancelAsync();
            try { await sampler; } catch (OperationCanceledException) when (stopSamples.IsCancellationRequested) { }
        }

        var port = editor.Host.Preferences.Port;
        var shutdown = Stopwatch.StartNew();
        await editor.Host.ApplyAsync(new());
        shutdown.Stop();
        Assert.False(editor.Vm.WorldOperations.IsBusy);
        Assert.False(File.Exists(Path.Combine(editor.Temp.FullName, "endpoint.json")));
        using var probe = new TcpListener(IPAddress.Loopback, port);
        probe.Start();
        using var ownership = File.Open(Path.Combine(editor.Temp.FullName, "host.lock"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        using var process = Process.GetCurrentProcess();
        double peakMiB = process.PeakWorkingSet64 / 1048576.0;
        delays.Sort();
        Assert.NotEmpty(delays);
        output.WriteLine($"World {width}x{height}, samples {delays.Count}, input p95 {delays[(int)Math.Ceiling(.95 * delays.Count) - 1]:F2} ms, max {delays[^1]:F2} ms, peak editor {peakMiB:F1} MiB, result {resultBytes} bytes, cancel {cancelMs:F2} ms, shutdown {shutdown.Elapsed.TotalMilliseconds:F2} ms.");
        Assert.InRange(delays[^1], 0, 1000);
        Assert.InRange(peakMiB, 1, large ? 6144 : 4096);
        Assert.InRange(resultBytes, 1, 16384);
        Assert.InRange(cancelMs, 0, 2500);
        Assert.InRange(shutdown.Elapsed.TotalMilliseconds, 0, 5000);
    }
}
