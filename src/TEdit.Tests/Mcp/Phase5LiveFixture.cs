using System.IO;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using TEdit.Mcp;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Mcp;

// Each external agent needs its own test process because the editor has one current world.
[Collection("Production editor")]
public class Phase5LiveFixture
{
    private sealed class LiveClientFactAttribute : FactAttribute
    {
        public LiveClientFactAttribute()
        {
            if (Environment.GetEnvironmentVariable("TEDIT_PHASE5_LIVE") == null)
                Skip = "Set TEDIT_PHASE5_LIVE to an empty evidence directory for an external MCP client.";
        }
    }

    [LiveClientFact]
    public async Task ServeDisposableClient()
    {
        var folder = Path.GetFullPath(Environment.GetEnvironmentVariable("TEDIT_PHASE5_LIVE")!);
        Directory.CreateDirectory(folder);
        Assert.Empty(Directory.EnumerateFileSystemEntries(folder));
        bool publishedBridge = Environment.GetEnvironmentVariable("TEDIT_PHASE5_BRIDGE") == "1";
        bool inspectOnly = Environment.GetEnvironmentVariable("TEDIT_PHASE5_INSPECT") == "1";
        await using var session = new McpEditorSession();
        await session.Open(scripts: !inspectOnly, hostDirectory: publishedBridge ? McpServerService.UserDirectory : null,
            edits: !inspectOnly);
        await World.SaveAsync(session.World, Path.Combine(folder, "backup.wld"), incrementRevision: false);
        string exe = Path.Combine(AppContext.BaseDirectory, "TEdit.exe");
        foreach (var product in Enum.GetValues<AgentProduct>())
        {
            var setup = AgentSetup.Create(product, AgentSetup.SupportsWorkspace(product),
                product == AgentProduct.ClaudeDesktop, session.Host.Endpoint, exe, null);
            File.WriteAllText(Path.Combine(folder, product + "." + setup.Extension), setup.Preview);
        }

        // Model latency varies. A reviewer can opt into a busy dispatcher to keep the
        // fourth operation pending until the external client has time to request cancellation.
        if (Environment.GetEnvironmentVariable("TEDIT_PHASE5_SLOW_CANCEL") == "1")
            session.Vm.WorldOperations.StateChanged += () =>
            {
                if (session.Vm.WorldOperations.ActiveName != "MCP script") return;
                var ops = session.Host.Operations.History();
                if (ops.Length == 4 && ops[3].Status == "running")
                    SpinWait.SpinUntil(() => session.Host.Operations.Find(null, ops[3].Id, false)?.Status != "running",
                        TimeSpan.FromSeconds(15));
            };

        File.WriteAllText(Path.Combine(folder, "url"), session.Host.Endpoint);
        using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(15));
        string previous = "";
        while (!File.Exists(Path.Combine(folder, "stop")))
        {
            deadline.Token.ThrowIfCancellationRequested();
            var ops = session.Host.Operations.History();
            string json = JsonSerializer.Serialize(ops);
            if (previous != json)
            {
                File.WriteAllText(Path.Combine(folder, "operations.json"), json);
                previous = json;
            }
            if (ops.Length == 4 && ops.All(op => op.Status is not ("running" or "cancel_requested"))
                && !File.Exists(Path.Combine(folder, "undo.json")))
            {
                var evidence = await session.Ui.InvokeAsync(async () =>
                {
                    var before = session.Vm.UndoManager.CurrentIndex;
                    int wallBefore = session.World.Tiles[45, 11].Wall;
                    await session.Vm.UndoCommand.Execute().ToTask();
                    return new { before, after = session.Vm.UndoManager.CurrentIndex, wallBefore, wallAfter = session.World.Tiles[45, 11].Wall };
                }).Task.Unwrap();
                File.WriteAllText(Path.Combine(folder, "undo.json"), JsonSerializer.Serialize(evidence));
            }
            await Task.Delay(100, deadline.Token);
        }
    }
}
