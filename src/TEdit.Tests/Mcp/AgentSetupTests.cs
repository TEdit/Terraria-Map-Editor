using System.IO;
using System.Text.Json;
using System.IO.Pipelines;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using ModelContextProtocol.Client;
using TEdit.Mcp;
using Xunit;

namespace TEdit.Tests.Mcp;

[Collection("Production editor")]
public class AgentSetupTests
{
    [Fact]
    public void ClientExportsUseTheirOwnKeysAndKeepCredentialsOutOfThePreview()
    {
        const string exe = @"C:\TEdit user's folder 地图🌎\TEdit.exe";
        const string url = "http://127.0.0.1:5500/mcp/1234567890ab4def8123456789abcdef";
        var id = Guid.NewGuid();
        foreach (var product in Enum.GetValues<AgentProduct>())
        foreach (bool workspace in AgentSetup.SupportsWorkspace(product) ? new[] { true, false } : [false])
        foreach (bool stdio in product == AgentProduct.ClaudeDesktop ? new[] { true } : [true, false])
        {
            var setup = AgentSetup.Create(product, workspace, stdio, url, exe, stdio ? id : null);
            Assert.DoesNotContain("Bearer", setup.Preview);
            Assert.Contains(url, setup.Preview);
            if (setup.Extension == "ps1")
            {
                Assert.Contains("--scope user", setup.Preview);
                if (stdio) Assert.Contains("user''s folder", setup.Preview);
                continue;
            }
            if (setup.Extension == "toml")
            {
                Assert.StartsWith("[mcp_servers.tedit]", setup.Preview);
                if (stdio) Assert.Contains("地图🌎", setup.Preview);
                continue;
            }
            using var parsed = JsonDocument.Parse(setup.Preview);
            var entry = product == AgentProduct.Generic ? parsed.RootElement : parsed.RootElement
                .GetProperty(product == AgentProduct.VsCode ? "servers" : "mcpServers").GetProperty("tedit");
            if (stdio)
            {
                Assert.Equal(exe, entry.GetProperty("command").GetString());
                Assert.Equal(new[] { "--mcp-stdio", url, "--profile", id.ToString("N") },
                    entry.GetProperty("args").EnumerateArray().Select(a => a.GetString()));
            }
            else Assert.Equal(url, entry.GetProperty(product == AgentProduct.Antigravity ? "serverUrl" : "url").GetString());
        }
    }

    [Fact]
    public void ExportOnlyCreatesNewFiles()
    {
        var folder = Directory.CreateTempSubdirectory("tedit-agent-");
        try
        {
            var target = Path.Combine(folder.FullName, "mcp.json");
            File.WriteAllText(target, """{"servers":{"other":{}}}""");
            var setup = new AgentSetup("mcp.json", """{"servers":{"tedit":{}}}""", "json");
            Assert.Throws<IOException>(() => setup.ExportNewFile(target));
            Assert.Equal("""{"servers":{"other":{}}}""", File.ReadAllText(target));
            var fresh = Path.Combine(folder.FullName, "export.json");
            setup.ExportNewFile(fresh);
            Assert.Equal(setup.Preview, File.ReadAllText(fresh).TrimEnd());
        }
        finally { folder.Delete(true); }
    }

    [Fact]
    public async Task SecondProfileCannotReadOrCancelPrivateWorkOrBypassAnEditorLease()
    {
        await using var s = new McpEditorSession();
        await s.Open();
        await s.Host.ApplyAsync(s.Host.Preferences with { SharedLocalAccess = false });
        async Task<McpClient> NewClient(string name)
        {
            var id = Guid.ParseExact(s.Host.Profiles.Issue(name)[..32], "N");
            return await AgentBridge.ConnectAsync(s.Host.Endpoint, id, s.Temp.FullName, s.Deadline.Token);
        }

        await using var first = await NewClient("first");
        await using var second = await NewClient("second");
        var status = await s.Call(second, "get_status");
        var revision = status.GetProperty("world");
        var args = new Dictionary<string, object?>
        {
            ["instanceId"] = s.Host.Instance, ["requestId"] = status.GetProperty("nextRequestId").GetGuid(),
            ["worldId"] = revision.GetProperty("worldId").GetGuid(), ["worldRevision"] = revision.GetProperty("revision").GetInt64(),
            ["x"] = 2, ["y"] = 2, ["endX"] = 3, ["endY"] = 3, ["layer"] = "wall", ["material"] = 1,
            ["filled"] = true, ["thickness"] = 1, ["selectionOnly"] = false, ["matchId"] = -1
        };
        Assert.Equal(TEdit.Scripting.Execution.OperationRejection.None, s.Vm.WorldOperations.TryAcquire(true, null, out var editing));
        using (editing)
        {
            var operation = await s.Call(second, "draw_rectangle", args);
            var lookup = new Dictionary<string, object?> { ["operationId"] = operation.GetProperty("id").GetGuid() };
            foreach (var name in new[] { "get_operation", "cancel_operation" })
            {
                var hidden = await first.CallToolAsync(name, lookup, cancellationToken: s.Deadline.Token);
                Assert.Equal("operation_not_found", hidden.StructuredContent!.Value.GetProperty("errorCode").GetString());
            }
            do { operation = await s.Call(second, "get_operation", lookup); }
            while (operation.GetProperty("status").GetString() == "running");
            Assert.Equal("busy", operation.GetProperty("errorCode").GetString());
            Assert.Equal(0, s.Vm.UndoManager.CurrentIndex);
        }
    }

    [Fact]
    public async Task PinnedClientsHonorPermissionChangesRevocationAndHostShutdown()
    {
        await using var s = new McpEditorSession();
        await s.Open();
        string original = s.Host.Endpoint;
        await using var shared = await Connect(original, null);
        Assert.Contains("backup copy", shared.ServerInstructions);
        var settings = s.Host.Preferences with { SharedLocalAccess = false, AllowEdits = false };
        await s.Host.ApplyAsync(settings);
        await Assert.ThrowsAnyAsync<Exception>(async () => await shared.ListToolsAsync(cancellationToken: s.Deadline.Token));
        var obsolete = await Assert.ThrowsAnyAsync<Exception>(() => Connect(original, null));
        Assert.Equal(TEdit.Properties.Language.agent_refresh, obsolete.GetBaseException().Message);
        Guid NewProfile() => Guid.ParseExact(s.Host.Profiles.Issue("test client")[..32], "N");
        var id = NewProfile();
        await using var client = await Connect(s.Host.Endpoint, id);
        // Run the production stdio handlers over two independent streams without using the user's profile directory.
        var requests = new Pipe();
        var replies = new Pipe();
        await using var bridge = McpServer.Create(new StreamServerTransport(requests.Reader.AsStream(), replies.Writer.AsStream()),
            AgentBridge.CreateOptions(client));
        using var stopping = CancellationTokenSource.CreateLinkedTokenSource(s.Deadline.Token);
        var serving = bridge.RunAsync(stopping.Token);
        await using var downstream = await McpClient.CreateAsync(
            new StreamClientTransport(requests.Writer.AsStream(), replies.Reader.AsStream()), cancellationToken: s.Deadline.Token);
        Assert.Equal("client_profile", (await s.Call(downstream, "get_status")).GetProperty("connectionMode").GetString());
        var tools = await downstream.ListToolsAsync(cancellationToken: s.Deadline.Token);
        Assert.DoesNotContain(tools, t => t.Name is "execute_script" or "draw_rectangle");
        var status = await s.Call(client, "get_status");
        Assert.Equal("inspect_only", status.GetProperty("access").GetString());
        await s.Host.RevokeAsync(id);
        await Assert.ThrowsAnyAsync<Exception>(async () => await client.ListToolsAsync(cancellationToken: s.Deadline.Token));
        await Assert.ThrowsAnyAsync<Exception>(async () => await downstream.ListToolsAsync(cancellationToken: s.Deadline.Token));
        await stopping.CancelAsync();
        try { await serving; } catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
        await using var replacement = await Connect(s.Host.Endpoint, NewProfile());
        Assert.Equal("client_profile", (await s.Call(replacement, "get_status")).GetProperty("connectionMode").GetString());
        var previous = s.Host.Endpoint;
        await s.Host.ApplyAsync(new());
        var disabled = await Assert.ThrowsAnyAsync<Exception>(() => Connect(previous, null));
        Assert.Equal(TEdit.Properties.Language.agent_missing, disabled.GetBaseException().Message);
        using var occupied = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, new Uri(previous).Port);
        occupied.Start();
        var busy = await Assert.ThrowsAnyAsync<Exception>(() => Connect(previous, null));
        Assert.Equal(TEdit.Properties.Language.ResourceManager.GetString("mcp_host_PortBusy"), busy.GetBaseException().Message);

        Task<McpClient> Connect(string endpoint, Guid? owner) =>
            AgentBridge.ConnectAsync(endpoint, owner, s.Temp.FullName, s.Deadline.Token);
    }
}
