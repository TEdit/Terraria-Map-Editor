using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.AspNetCore;
using TEdit.ViewModel;

namespace TEdit.Mcp;

public sealed record McpPreferences(bool Enabled = false, int Port = 48190, bool AllowEdits = false, bool SharedLocalAccess = false, bool AllowScripts = false);
internal enum HostState { Disabled, Running, OtherEditor, PortBusy, Fault }

internal sealed class McpServerService(WorldViewModel world, Dispatcher dispatcher, string directory)
{
    internal static string UserDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TEdit", "Mcp");
    private readonly SemaphoreSlim _lifecycle = new(1);
    private WebApplication? _host;
    private CancellationTokenSource? _requests;
    private FileStream? _ownership;
    private volatile bool _accepting;
    private string DiscoveryPath => Path.Combine(directory, "endpoint.json");
    internal readonly McpProfiles Profiles = new(directory);
    internal McpOperations Operations { get; private set; } = null!;
    internal HostState State { get; private set; }
    internal Guid Instance { get; private set; }
    internal string Endpoint => $"http://127.0.0.1:{Preferences.Port}/mcp/{Instance:N}";
    internal McpPreferences Preferences { get; private set; } = new();

    internal async Task ApplyAsync(McpPreferences preferences)
    {
        await _lifecycle.WaitAsync();
        try
        {
            if (State == HostState.Running && Preferences == preferences) return;
            await StopCoreAsync();
            Preferences = preferences;
            if (!preferences.Enabled) return;
            if (preferences.Port is < 1024 or > 65535) { State = HostState.Fault; return; }
            Directory.CreateDirectory(directory);
            try { _ownership = new FileStream(Path.Combine(directory, "host.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException ex) when ((ex.HResult & 0xffff) is 32 or 33) { State = HostState.OtherEditor; return; }

            if (!preferences.SharedLocalAccess) Profiles.Load();
            Instance = Guid.NewGuid();
            Operations = new(Instance, TimeProvider.System, id => preferences.SharedLocalAccess ? id == Guid.Empty : Profiles.Exists(id));
            var port = preferences.Port;
            var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [], ContentRootPath = AppContext.BaseDirectory });
            builder.Configuration.Sources.Clear();
            builder.Logging.ClearProviders();
            builder.WebHost.ConfigureKestrel(k => {
                k.Limits.MaxRequestBodySize = 16384;
                k.Limits.MaxConcurrentConnections = 16;
                k.Listen(IPAddress.Loopback, port);
            });
            builder.Services.AddSingleton(this);
            builder.Services.AddSingleton(new InspectionSource(world, dispatcher));
            builder.Services.AddHttpContextAccessor();
            var mcp = builder.Services.AddMcpServer(options => options.ServerInstructions =
                "Before the first edit in every editing session, recommend that the user save a separate backup copy " +
                "of the current world in TEdit. Give this reminder in the user's language, including when switching " +
                "from inspection to drawing or running a script that may change the world.")
                .WithHttpTransport(o => o.SessionMode = HttpServerSessionMode.Stateless)
                .WithTools<InspectionTools>();
            if (preferences.AllowEdits)
            {
                builder.Services.AddSingleton(new DrawingHost(world, dispatcher));
                mcp.WithTools<DrawingTools>();
                if (preferences.AllowScripts)
                {
                    builder.Services.AddSingleton(new Scripting.ScriptHost(world, dispatcher, Path.Combine(AppContext.BaseDirectory, "TEdit.exe")));
                    mcp.WithTools<Scripting.ScriptTools>();
                }
            }
            _host = builder.Build();
            _requests = new CancellationTokenSource();
            var stopping = _requests.Token;
            _host.Use(async (context, next) =>
            {
                var req = context.Request;
                if (!_accepting) { context.Response.StatusCode = 503; return; }
                if (!string.Equals(req.Host.Value, $"127.0.0.1:{port}", StringComparison.Ordinal) ||
                    (req.Headers.ContainsKey("Origin") && (preferences.SharedLocalAccess || req.Headers.Origin.ToString() != $"http://127.0.0.1:{port}")))
                { context.Response.StatusCode = 403; return; }
                Guid? id = Guid.Empty; // All clients explicitly share the local operation history.
                if (!preferences.SharedLocalAccess)
                {
                    var auth = req.Headers.Authorization.ToString();
                    id = auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? Profiles.Authenticate(auth[7..]) : null;
                }
                if (id == null) { context.Response.Headers.WWWAuthenticate = "Bearer"; context.Response.StatusCode = 401; return; }
                context.Items[typeof(McpProfiles)] = id.Value;
                using var request = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, stopping);
                context.RequestAborted = request.Token;
                await next();
            });
            _host.MapMcp($"/mcp/{Instance:N}");
            await _host.StartAsync();
            // Local discovery is for setup. Clients pin the exact instance URL.
            File.WriteAllText(DiscoveryPath + ".tmp", System.Text.Json.JsonSerializer.Serialize(new { instanceId = Instance, endpoint = Endpoint }));
            File.Move(DiscoveryPath + ".tmp", DiscoveryPath, true);
            _accepting = true;
            State = HostState.Running;
        }
        catch (Exception ex)
        {
            try { await StopCoreAsync(); }
            catch { State = HostState.Fault; return; }
            State = ex.GetBaseException() is SocketException { SocketErrorCode: SocketError.AddressAlreadyInUse } ? HostState.PortBusy : HostState.Fault;
        }
        finally { _lifecycle.Release(); }
    }

    internal async Task RevokeAsync(Guid owner)
    {
        await _lifecycle.WaitAsync();
        try { Profiles.Revoke(owner); await Operations.DrainAsync(owner); }
        finally { _lifecycle.Release(); }
    }

    private async Task StopCoreAsync()
    {
        _accepting = false;
        Profiles.Close();
        try
        {
            var cancelRequests = _requests?.CancelAsync() ?? Task.CompletedTask;
            var drain = Operations?.DrainAsync() ?? Task.CompletedTask;
            await Task.WhenAll(cancelRequests, drain);
        }
        finally
        {
            try
            {
                if (_host != null)
                {
                    try { await _host.StopAsync(); }
                    finally { await _host.DisposeAsync(); }
                }
            }
            finally
            {
                _host = null;
                _requests?.Dispose();
                _requests = null;
                if (_ownership != null)
                {
                    try { File.Delete(DiscoveryPath); }
                    finally { _ownership.Dispose(); _ownership = null; }
                }
                State = HostState.Disabled;
            }
        }
    }
}
