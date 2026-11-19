using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows.Threading;
using ModelContextProtocol.Client;
using TEdit.Mcp;
using TEdit.Render;
using TEdit.Terraria;
using TEdit.Tests.Scripting;
using TEdit.ViewModel;
using Xunit;

namespace TEdit.Tests.Mcp;

internal sealed class McpEditorSession(int width = 100, int height = 100) : IAsyncDisposable
{
    public readonly Dispatcher Ui = ProductionExecutionTests.Ui.Value;
    public readonly World World = ExecutionWorldFixture.Create(width, height);
    public readonly DirectoryInfo Temp = Directory.CreateTempSubdirectory("tedit-tools-");
    public readonly CancellationTokenSource Deadline = new(TimeSpan.FromSeconds(30));
    public WorldViewModel Vm = null!;
    public McpServerService Host = null!;
    public McpClient Writer = null!, Observer = null!;

    public async Task Open(bool scripts = false, string? hostDirectory = null, bool edits = true)
    {
        Vm = await Ui.InvokeAsync(() => new WorldViewModel { IsAutoSaveEnabled = false, CurrentWorld = World });
        await Ui.InvokeAsync(async () =>
        {
            Vm.PixelMap = new PixelMapManager();
            Vm.PixelMap.InitializeBuffers(World.TilesWide, World.TilesHigh);
            await Vm.UpdateRenderWorldAsync();
        }).Task.Unwrap();
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        int port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        Host = new(Vm, Ui, hostDirectory ?? Temp.FullName);
        await Host.ApplyAsync(new(true, port, AllowEdits: edits, SharedLocalAccess: true, AllowScripts: scripts));
        Assert.Equal(HostState.Running, Host.State);
        Writer = await Connect();
        Observer = await Connect();
    }

    public Task<McpClient> Connect() => McpClient.CreateAsync(new HttpClientTransport(new()
    {
        Endpoint = new(Host.Endpoint), TransportMode = HttpTransportMode.StreamableHttp
    }), cancellationToken: Deadline.Token);

    public async Task<JsonElement> Call(McpClient client, string name, Dictionary<string, object?>? args = null)
    {
        var result = await client.CallToolAsync(name, args, cancellationToken: Deadline.Token);
        Assert.False(result.IsError, result.StructuredContent.ToString());
        return result.StructuredContent!.Value.GetProperty("data");
    }

    public async Task<Guid> Start(string tool, Dictionary<string, object?> args)
    {
        var status = await Call(Writer, "get_status");
        Assert.Equal("shared_local", status.GetProperty("connectionMode").GetString());
        var revision = status.GetProperty("world");
        args["instanceId"] = status.GetProperty("instanceId").GetGuid();
        args["requestId"] = status.GetProperty("nextRequestId").GetGuid();
        args["worldId"] = revision.GetProperty("worldId").GetGuid();
        args["worldRevision"] = revision.GetProperty("revision").GetInt64();
        return (await Call(Writer, tool, args)).GetProperty("id").GetGuid();
    }

    public async Task<JsonElement> Finish(Guid id)
    {
        while (true)
        {
            var op = await Call(Observer, "get_operation", new() { ["operationId"] = id });
            if (op.GetProperty("status").GetString() is not ("running" or "cancel_requested")) return op;
            await Task.Delay(5, Deadline.Token);
        }
    }

    public async Task Expect(string tool, Dictionary<string, object?> args, int count, string? error = null)
    {
        var op = await Finish(await Start(tool, args));
        Assert.Equal(error, op.GetProperty("errorCode").GetString());
        Assert.Equal(error == null ? "succeeded" : "failed", op.GetProperty("status").GetString());
        Assert.Equal(count, op.GetProperty("changedTiles").GetInt32());
    }

    public async ValueTask DisposeAsync()
    {
        if (Observer != null) await Observer.DisposeAsync();
        if (Writer != null) await Writer.DisposeAsync();
        if (Host != null) await Host.ApplyAsync(new());
        if (Vm != null) await Ui.InvokeAsync(() => Vm.CurrentWorld = null!);
        Temp.Delete(true);
        Deadline.Dispose();
    }
}
