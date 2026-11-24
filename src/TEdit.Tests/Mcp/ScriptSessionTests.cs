using System.Diagnostics;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Windows.Threading;
using TEdit.Mcp.Scripting;
using Xunit;
using Xunit.Abstractions;

namespace TEdit.Tests.Mcp;

[Collection("Production editor")]
public class ScriptSessionTests(ITestOutputHelper output)
{
    private static Dictionary<string, object?> Args(string language, string source) =>
        new() { ["language"] = language, ["source"] = source };

    [Theory]
    [InlineData("javascript")]
    [InlineData("lua")]
    public async Task PublishedExamplesRunAndShareOneUndoStep(string language)
    {
        await using var s = new McpEditorSession();
        await s.Open(scripts: true);
        Assert.Contains("backup copy", s.Writer.ServerInstructions);
        var reference = await s.Call(s.Writer, "get_script_reference", new() { ["language"] = language });
        Assert.Equal(ScriptWorker.Api.Sum(m => m.Methods.Length), reference.GetProperty("functions").GetArrayLength());
        foreach (string example in new[] { "inspect", "draw", "compose" })
        {
            string source = reference.GetProperty("examples").GetProperty(example).GetString()!;
            var args = Args(language, source);
            var before = s.Vm.UndoManager.CurrentIndex;
            var id = await s.Start("execute_script", args);
            var op = await s.Finish(id);
            Assert.True(op.GetProperty("status").GetString() == "succeeded", op.ToString());
            var log = op.GetProperty("script").GetProperty("log");
            if (example == "inspect")
            {
                Assert.Equal(3, log.GetArrayLength());
                Assert.Equal(before, s.Vm.UndoManager.CurrentIndex);
                continue;
            }
            Assert.Equal(before + 1, s.Vm.UndoManager.CurrentIndex);
            Assert.Equal(example == "draw" ? 16 : 25, op.GetProperty("changedTiles").GetInt32());
            if (example == "compose") Assert.Equal("1", log[0].GetString());
            await using var reconnected = await s.Connect();
            Assert.Equal(id, (await s.Call(reconnected, "execute_script", args)).GetProperty("id").GetGuid());
            args["source"] = "log.print(42)";
            var conflict = await reconnected.CallToolAsync("execute_script", args, cancellationToken: s.Deadline.Token);
            Assert.Equal("payload_conflict", conflict.StructuredContent!.Value.GetProperty("errorCode").GetString());
            await s.Ui.InvokeAsync(async () =>
            {
                Assert.True(s.World.Tiles[10, 10].IsActive);
                await s.Vm.UndoCommand.Execute().ToTask();
                Assert.False(s.World.Tiles[10, 10].IsActive);
                Assert.Equal(0, s.World.Tiles[12, 12].Wall);
            }).Task.Unwrap();
        }
    }

    [Theory]
    [InlineData("javascript")]
    [InlineData("lua")]
    public async Task ComposedDrawingUsesCapturedSelectionAndRestoresEntities(string language)
    {
        await using var s = new McpEditorSession();
        await s.Open(scripts: true);
        await s.Ui.InvokeAsync(() => s.Vm.Selection.SetRectangle(new(20, 20), new(32, 32)));
        var source = """
            draw.line(23,20,19,20,1,'wall',1,true,-1)
            draw.ellipse(25,20,28,23,false,1,'wall',1,true,-1)
            draw.placeSprite(30,28,21,0,true)
            """;
        var result = await s.Finish(await s.Start("execute_script", Args(language, source)));
        Assert.True(result.GetProperty("status").GetString() == "succeeded", result.ToString());
        Assert.Equal(16, result.GetProperty("changedTiles").GetInt32());
        Assert.Equal(0, s.World.Tiles[19,20].Wall);
        Assert.Single(s.World.Chests);
        await s.Ui.InvokeAsync(async () =>
        {
            Assert.Equal(1, s.Vm.UndoManager.CurrentIndex);
            await s.Vm.UndoCommand.Execute().ToTask();
            Assert.Empty(s.World.Chests);
            Assert.Equal(0, s.World.Tiles[20,20].Wall);
        }).Task.Unwrap();
    }

    [Theory]
    [InlineData("javascript", "let x = ;")]
    [InlineData("lua", "local x = )")]
    public async Task ReportsSourceLocationsAndUndoableRuntimeErrors(string language, string invalid)
    {
        await using var s = new McpEditorSession();
        await s.Open(scripts: true);
        var syntax = await s.Finish(await s.Start("execute_script", Args(language, "\n" + invalid)));
        Assert.Equal("syntax_error", syntax.GetProperty("errorCode").GetString());
        Assert.Equal(2, syntax.GetProperty("script").GetProperty("diagnostic").GetProperty("line").GetInt32());
        var partial = await s.Finish(await s.Start("execute_script", Args(language,
            "draw.fillRegion(10,10,12,10,'wall',1,false,-1)\ntools.save()")));
        Assert.Equal("runtime_error", partial.GetProperty("errorCode").GetString());
        Assert.True(partial.GetProperty("partialChanges").GetBoolean());
        Assert.Equal(3, partial.GetProperty("changedTiles").GetInt32());
        Assert.Equal(2, partial.GetProperty("script").GetProperty("diagnostic").GetProperty("line").GetInt32());
        await s.Ui.InvokeAsync(async () =>
        {
            await s.Vm.UndoCommand.Execute().ToTask();
            Assert.Equal(0, s.World.Tiles[11, 10].Wall);
        }).Task.Unwrap();
    }

    [Theory]
    [InlineData("javascript", "while(true){}", "instruction_limit")]
    [InlineData("lua", "while true do end", "instruction_limit")]
    [InlineData("javascript", "function nest(n){return 1+nest(n+1)} nest(0)", "recursion_limit")]
    [InlineData("lua", "local function nest(n) return 1+nest(n+1) end nest(0)", "recursion_limit")]
    [InlineData("javascript", "let a='x'; for(let i=0;i<26;i++) a=a+a; log.print(a.length)", "memory_limit")]
    [InlineData("lua", "local a='x'; for i=1,24 do a=a..a; for j=1,256 do end end", "memory_limit")]
    [InlineData("javascript", "for(let i=0;i<40;i++) log.print(i)", "log_limit")]
    [InlineData("lua", "for i=1,40 do log.print(i) end", "log_limit")]
    [InlineData("javascript", "log.print('x'.repeat(1025))", "argument_limit")]
    [InlineData("lua", "local a='x'; for i=1,11 do a=a..a end; log.print(a)", "argument_limit")]
    [InlineData("lua", "for i=1,300 do tile.getWall(0,0) end", "call_limit")]
    [InlineData("javascript", "for(let i=0;i<8;i++) draw.fillRegion(0,0,99,99,'wall',1,false,-1)", "work_limit")]
    public async Task LimitsRunInDisposableWorkers(string language, string source, string expected)
    {
        await using var s = new McpEditorSession();
        await s.Open(scripts: true);
        var elapsed = Stopwatch.StartNew();
        var op = await s.Finish(await s.Start("execute_script", Args(language, source)));
        output.WriteLine($"{language} {expected}: {elapsed.ElapsedMilliseconds} ms");
        Assert.True(op.GetProperty("errorCode").GetString() == expected, op.ToString());
        Assert.InRange(Encoding.UTF8.GetByteCount(op.GetRawText()), 1, 16384);
        Assert.False(s.Vm.WorldOperations.IsBusy);
    }

    [Theory]
    [InlineData("javascript", false)]
    [InlineData("lua", false)]
    [InlineData("lua", true)]
    public async Task ActiveScriptCleansUpBeforeCancelOrDeadlineBecomesTerminal(string language, bool timeout)
    {
        await using var s = new McpEditorSession();
        await s.Open(scripts: true);
        using var release = new ManualResetEventSlim();
        var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var timer = await s.Ui.InvokeAsync(() =>
        {
            var t = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromMilliseconds(1) };
            t.Tick += (_, _) =>
            {
                if (s.World.Tiles[0,0].Wall != 1) return;
                t.Stop();
                reached.TrySetResult();
                release.Wait(TimeSpan.FromSeconds(8));
            };
            t.Start();
            return t;
        });
        Guid id;
        var elapsed = Stopwatch.StartNew();
        try
        {
            var code = "draw.fillRegion(0,0,99,99,'wall',1,false,-1)\n" +
                (language == "lua" ? "for i=1,200 do tile.getWall(0,0) end" : "for(let i=0;i<200;i++) tile.getWall(0,0)");
            id = await s.Start("execute_script", Args(language, code));
            await reached.Task.WaitAsync(TimeSpan.FromSeconds(4));
            elapsed.Restart();
            if (timeout)
                await Task.Delay(ScriptProtocol.TimeoutMs + ScriptProtocol.DeadlineGraceMs + 100);
            else
                await s.Call(s.Observer, "cancel_operation", new() { ["operationId"] = id });
        }
        finally
        {
            release.Set();
            await s.Ui.InvokeAsync(timer.Stop);
        }
        var op = await s.Finish(id);
        output.WriteLine($"{language} {(timeout ? "deadline" : "cancel")} to terminal: {elapsed.ElapsedMilliseconds} ms");
        Assert.Equal(timeout ? "failed" : "canceled", op.GetProperty("status").GetString());
        Assert.Equal(timeout ? "timeout" : null, op.GetProperty("errorCode").GetString());
        Assert.True(op.GetProperty("partialChanges").GetBoolean());
        var changed = op.GetProperty("changedTiles").GetInt32();
        Assert.InRange(changed, 1, 10000);
        Assert.Equal(changed, s.World.Tiles.Cast<TEdit.Terraria.Tile>().Count(t => t.Wall == 1));
        Assert.False(s.Vm.WorldOperations.IsBusy);
        await s.Ui.InvokeAsync(async () =>
        {
            await s.Vm.UndoCommand.Execute().ToTask();
            Assert.DoesNotContain(s.World.Tiles.Cast<TEdit.Terraria.Tile>(), t => t.Wall == 1);
        }).Task.Unwrap();
    }

    [Fact]
    public async Task ExpiredAndPreviousInstanceRequestsCannotRunAgain()
    {
        await using var s = new McpEditorSession();
        await s.Open(scripts: true);
        var args = Args("javascript", "draw.fillRegion(15,15,15,15,'wall',1,false,-1)");
        var id = await s.Start("execute_script", args);
        Assert.Equal("succeeded", (await s.Finish(id)).GetProperty("status").GetString());
        var originalRequest = args["requestId"];
        args["requestId"] = Guid.CreateVersion7(DateTimeOffset.UtcNow.AddMinutes(-12));
        var expired = await s.Writer.CallToolAsync("execute_script", args);
        Assert.Equal("expired_request", expired.StructuredContent!.Value.GetProperty("errorCode").GetString());
        var preferences = s.Host.Preferences;
        await s.Host.ApplyAsync(new());
        await s.Host.ApplyAsync(preferences);
        await using var reconnected = await s.Connect();
        args["requestId"] = originalRequest;
        var stale = await reconnected.CallToolAsync("execute_script", args);
        Assert.Equal("instance_changed", stale.StructuredContent!.Value.GetProperty("errorCode").GetString());
        var gone = await reconnected.CallToolAsync("get_operation", new Dictionary<string,object?> { ["operationId"] = id });
        Assert.Equal("operation_not_found", gone.StructuredContent!.Value.GetProperty("errorCode").GetString());
        Assert.Equal(1, s.Vm.UndoManager.CurrentIndex);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task ExecutionRequiresBothEditAndScriptPermission(bool edit, bool scripts)
    {
        await using var s = new McpEditorSession();
        await s.Open();
        await s.Host.ApplyAsync(s.Host.Preferences with { AllowEdits = edit, AllowScripts = scripts });
        await using var reader = await s.Connect();
        Assert.False((await s.Call(reader, "get_status")).GetProperty("scriptExecution").GetBoolean());
        Assert.DoesNotContain(await reader.ListToolsAsync(), t => t.Name == "execute_script");
        var error = await Assert.ThrowsAsync<ModelContextProtocol.McpProtocolException>(() =>
            reader.CallToolAsync("execute_script", Args("lua", "log.print(1)")).AsTask());
        Assert.Contains("execute_script", error.Message);
        Assert.Empty(s.Host.Operations.History());
    }

    [Theory]
    [InlineData("javascript")]
    [InlineData("lua")]
    public async Task OnlyAuditedGlobalsAreAvailable(string language)
    {
        await using var s = new McpEditorSession();
        await s.Open(scripts: true);
        string code = language == "lua"
            ? "log.print(tools==nil and world.title==nil and world.get~=nil and io==nil and os==nil and chests==nil and package==nil and debug==nil and tile.setType==nil)"
            : "log.print(typeof tools==='undefined' && typeof world.title==='undefined' && typeof world.get==='function' && typeof System==='undefined' && typeof require==='undefined' && typeof chests==='undefined' && typeof tile.setType==='undefined')";
        var op = await s.Finish(await s.Start("execute_script", Args(language, code)));
        Assert.Equal("True", op.GetProperty("script").GetProperty("log")[0].GetString(), ignoreCase: true);
        var oversized = await s.Writer.CallToolAsync("execute_script", new Dictionary<string, object?>
        {
            ["source"] = new string('a', ScriptProtocol.SourceBytes + 1), ["language"] = language,
            ["instanceId"] = s.Host.Instance, ["worldId"] = Guid.NewGuid(), ["worldRevision"] = 0, ["requestId"] = Guid.CreateVersion7()
        });
        Assert.Equal("invalid_source", oversized.StructuredContent!.Value.GetProperty("errorCode").GetString());
    }
}
