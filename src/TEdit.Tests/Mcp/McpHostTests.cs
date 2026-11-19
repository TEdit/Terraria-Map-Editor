using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using TEdit.Mcp;
using TEdit.Configuration;
using TEdit.Tests.Scripting;
using TEdit.ViewModel;
using Xunit;

namespace TEdit.Tests.Mcp;

[Collection("Production editor")]
public class McpHostTests
{
    [Fact]
    public async Task ObserveActualTransportAndLifecycle()
    {
        var ui = ProductionExecutionTests.Ui.Value;
        var vm = await ui.InvokeAsync(() => new WorldViewModel { IsAutoSaveEnabled = false });
        var directory = Directory.CreateTempSubdirectory("tedit-mcp-");
        var host = new McpServerService(vm, ui, directory.FullName);
        using var occupied = new TcpListener(IPAddress.Loopback, 0);
        occupied.Start();
        var port = ((IPEndPoint)occupied.LocalEndpoint).Port;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var http = new HttpClient(new HttpClientHandler { UseProxy = false });
        var settings = new McpPreferences(true, port);
        var releasing = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            await host.ApplyAsync(settings);
            Assert.Equal(HostState.PortBusy, host.State);
            occupied.Stop();
            await host.ApplyAsync(settings);
            Assert.Equal(HostState.Running, host.State);
            var other = new McpServerService(vm, ui, directory.FullName);
            await other.ApplyAsync(settings);
            Assert.Equal(HostState.OtherEditor, other.State);
            var firstEndpoint = host.Endpoint;
            var listeners = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            Assert.All(listeners.Where(e => e.Port == port), e => Assert.True(IPAddress.IsLoopback(e.Address)));
            Assert.Contains(listeners, e => e.Port == port && e.Address.Equals(IPAddress.Loopback));
            var token = host.Profiles.Issue("Test A");
            var tokenB = host.Profiles.Issue("Test B");
            await ui.InvokeAsync(() =>
            {
                var options = new View.Popups.SettingsWindow(vm, host);
                try
                {
                    var prompt = ((System.Windows.Controls.TextBox)options.FindName("McpSetup")).Text;
                    Assert.Contains(host.Endpoint, prompt);
                    Assert.Contains("get_status", prompt);
                    Assert.DoesNotContain(token, prompt);
                    Assert.DoesNotContain(tokenB, prompt);
                }
                finally { options.Close(); }
            });
            Assert.DoesNotContain(token, File.ReadAllText(Path.Combine(directory.FullName, "endpoint.json")));
            Assert.DoesNotContain(token, JsonSerializer.Serialize(settings));
            async Task<HttpStatusCode> Probe(string? auth, string? authority = null, string? origin = null)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, host.Endpoint);
                if (authority != null) request.Headers.Host = authority;
                if (origin != null) request.Headers.Add("Origin", origin);
                if (auth != null) request.Headers.Add("Authorization", "Bearer " + auth);
                using var response = await http.SendAsync(request, timeout.Token);
                return response.StatusCode;
            }
            Assert.Equal(HttpStatusCode.Unauthorized, await Probe(null));
            Assert.Equal(HttpStatusCode.Unauthorized, await Probe("invalid"));
            Assert.Equal(HttpStatusCode.Forbidden, await Probe(token, "example.invalid"));
            Assert.Equal(HttpStatusCode.Forbidden, await Probe(token, origin: "https://example.invalid"));
            await using var client = await McpClient.CreateAsync(new HttpClientTransport(new()
            {
                Endpoint = new Uri(host.Endpoint), TransportMode = HttpTransportMode.StreamableHttp,
                AdditionalHeaders = new Dictionary<string, string> { ["Authorization"] = "Bearer " + token }
            }), cancellationToken: timeout.Token);
            Assert.Contains("Before the first edit in every editing session", client.ServerInstructions);
            Assert.Contains("save a separate backup copy", client.ServerInstructions);
            var tools = await client.ListToolsAsync(cancellationToken: timeout.Token);
            Assert.Equal(new[] { "cancel_operation", "get_api_reference", "get_operation", "get_selection", "get_status", "get_world_info", "read_region", "search_metadata" }, tools.Select(t => t.Name).Order().ToArray());
            Assert.All(tools, t => Assert.Equal("object", t.ProtocolTool.OutputSchema!.Value.GetProperty("type").GetString()));
            async Task<JsonElement> Call(string name, Dictionary<string,object?>? args = null)
            {
                var result = await client.CallToolAsync(name, args, cancellationToken: timeout.Token);
                Assert.Equal(result.StructuredContent!.Value.GetProperty("errorCode").ValueKind != JsonValueKind.Null, result.IsError == true);
                var json = Assert.Single(result.Content.OfType<TextContentBlock>()).Text;
                Assert.InRange(System.Text.Encoding.UTF8.GetByteCount(json), 1, 16384);
                var parsed = JsonSerializer.Deserialize<JsonElement>(json);
                Assert.True(JsonElement.DeepEquals(parsed, result.StructuredContent!.Value));
                return parsed;
            }
            Assert.Equal("no_world", (await Call("get_world_info")).GetProperty("errorCode").GetString());
            Assert.Equal(host.Instance, (await Call("get_status")).GetProperty("data").GetProperty("instanceId").GetGuid());
            Assert.Equal(2, (await Call("search_metadata", new() { ["kind"]="tile", ["limit"]=2 })).GetProperty("data").GetProperty("items").GetArrayLength());
            Assert.Equal("invalid_page", (await Call("get_api_reference", new(){["limit"]=17})).GetProperty("errorCode").GetString());
            Assert.Equal(2, (await Call("get_api_reference",new(){["limit"]=2})).GetProperty("data").GetProperty("items").GetArrayLength());
            Assert.Equal("operation_not_found", (await Call("cancel_operation", new(){["operationId"]=Guid.NewGuid()})).GetProperty("errorCode").GetString());
            await ui.InvokeAsync(() => vm.CurrentWorld = ExecutionWorldFixture.Create());
            Assert.Equal(TEdit.Scripting.Execution.OperationRejection.None, vm.WorldOperations.TryAcquire(true, null, out var editing));
            using (editing) Assert.Equal("busy", (await Call("get_world_info")).GetProperty("errorCode").GetString());
            await Call("get_selection");
            await Call("get_world_info");
            var args = new Dictionary<string,object?> { ["x"]=0, ["y"]=0, ["width"]=8, ["height"]=8 };
            Assert.Equal(64,(await Call("read_region",args)).GetProperty("data").GetProperty("snapshot").GetProperty("cells").GetArrayLength());
            args["width"] = int.MaxValue;
            Assert.Equal("invalid_region",(await Call("read_region",args)).GetProperty("errorCode").GetString());
            var owner = host.Profiles.Authenticate(token)!.Value;
            var entered = new TaskCompletionSource();
            var op = host.Operations.Start(owner, host.Instance, Guid.CreateVersion7(), "hash", async ct =>
            {
                try { await Task.Delay(Timeout.Infinite, ct); }
                finally { entered.SetResult(); await releasing.Task; }
            }).Operation!;
            Assert.Equal("running",(await Call("get_operation", new(){["operationId"]=op.Id})).GetProperty("data").GetProperty("status").GetString());
            var stopping = host.ApplyAsync(new());
            await entered.Task.WaitAsync(timeout.Token);
            try
            {
                Assert.False(stopping.IsCompleted);
                await other.ApplyAsync(settings);
                Assert.Equal(HostState.OtherEditor, other.State);
                Assert.Equal(HttpStatusCode.ServiceUnavailable, await Probe(token));
            }
            finally { releasing.TrySetResult(); }
            await stopping;
            Assert.Equal(HostState.Disabled, host.State);
            await Assert.ThrowsAsync<HttpRequestException>(() => http.GetAsync(firstEndpoint));
            await host.ApplyAsync(settings);
            Assert.NotEqual(firstEndpoint, host.Endpoint);
            using var obsolete = new HttpRequestMessage(HttpMethod.Post, firstEndpoint);
            obsolete.Headers.Add("Authorization", "Bearer " + tokenB);
            using var stale = await http.SendAsync(obsolete, timeout.Token);
            Assert.Equal(HttpStatusCode.NotFound, stale.StatusCode);
            var revokedWork = host.Operations.Start(owner, host.Instance, Guid.CreateVersion7(), "hash", ct => Task.Delay(Timeout.Infinite, ct)).Operation!;
            await host.RevokeAsync(owner);
            Assert.Equal("canceled", host.Operations.Find(null, revokedWork.Id, false)!.Status);
            Assert.Equal("profile_revoked", host.Operations.Start(owner, host.Instance, Guid.CreateVersion7(), "hash", _ => Task.CompletedTask).ErrorCode);
            Assert.Equal(HttpStatusCode.Unauthorized, await Probe(token));
            Assert.NotEqual(HttpStatusCode.Unauthorized, await Probe(tokenB));
            Assert.Null(host.Profiles.Authenticate(token));
        }
        finally
        {
            releasing.TrySetResult();
            await host.ApplyAsync(new());
            await ui.InvokeAsync(() => vm.CurrentWorld = null!);
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task OptionsCancelDiscardsHostDraft()
    {
        var ui = ProductionExecutionTests.Ui.Value;
        await ui.InvokeAsync(() =>
        {
            var vm = new WorldViewModel { IsAutoSaveEnabled = false };
            var server = new McpServerService(vm, ui, Path.GetTempPath());
            var saved = UserSettingsService.Current.McpServer;
            var dialog = new View.Popups.SettingsWindow(vm, server);
            var toggle = (System.Windows.Controls.CheckBox)dialog.FindName("McpEnabled");
            toggle.IsChecked = !saved.Enabled;
            var port = (System.Windows.Controls.TextBox)dialog.FindName("McpPort");
            port.Text = "49211";
            ((System.Windows.Controls.Button)dialog.FindName("CancelSettings")).RaiseEvent(
                new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            Assert.Equal(saved, UserSettingsService.Current.McpServer);
            Assert.Equal(HostState.Disabled, server.State);
        });
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public async Task LedgerScopesRetriesAndRetainsOwnershipUntilCleanup()
    {
        var clock = new Clock();
        var instance = Guid.NewGuid();
        var ledger = new McpOperations(instance, clock, _ => true);
        var owner = Guid.NewGuid();
        var request = Guid.CreateVersion7(clock.Now);
        var op = ledger.Start(owner, instance, request, "a", ct => Task.Delay(Timeout.Infinite, ct));
        Assert.Null(op.ErrorCode);
        Assert.Equal(op.Operation, ledger.Start(owner, instance, request, "a", _ => throw new Exception()).Operation);
        Assert.Equal("payload_conflict", ledger.Start(owner, instance, request, "b", _ => throw new Exception()).ErrorCode);
        Assert.Null(ledger.Find(Guid.NewGuid(),op.Operation!.Id,true));
        Assert.Equal("instance_changed", ledger.Start(owner, Guid.NewGuid(),request,"a", _ => Task.CompletedTask).ErrorCode);
        Assert.Equal("invalid_request_id", ledger.Start(owner, instance,Guid.Empty,"a", _ => Task.CompletedTask).ErrorCode);
        using var detached = new CancellationTokenSource();
        ledger.ObserveRequest(owner, op.Operation.Id, detached.Token).Dispose();
        detached.Cancel();
        Assert.Equal("running", ledger.Find(owner, op.Operation.Id, false)!.Status);
        using var protocol = new CancellationTokenSource();
        using var registration = ledger.ObserveRequest(owner, op.Operation.Id, protocol.Token);
        protocol.Cancel();
        Assert.Contains(ledger.Find(owner, op.Operation.Id, false)!.Status, new[] { "cancel_requested", "canceled" });
        await ledger.DrainAsync(owner);
        Assert.Equal("canceled",ledger.Find(owner,op.Operation.Id,false)!.Status);
        clock.Now += TimeSpan.FromMinutes(11);
        Assert.Null(ledger.Find(owner,op.Operation.Id,false));
        Assert.Equal("expired_request",ledger.Start(owner, instance, request,"a",_=>Task.CompletedTask).ErrorCode);
    }
}
