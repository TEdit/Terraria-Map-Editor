using System;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Server;
using TEdit.Properties;

namespace TEdit.Mcp;

internal static class AgentBridge
{
    internal const string Argument = "--mcp-stdio";

    // Check local discovery before each send, including later calls on an established bridge.
    private sealed class PinnedHost(string directory, Uri endpoint, Guid instance, Guid? profile) : DelegatingHandler
    {
        private InvalidDataException MissingHost()
        {
            bool occupied = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
                .Any(local => local.Port == endpoint.Port && (local.Address.Equals(System.Net.IPAddress.Any)
                    || local.Address.Equals(System.Net.IPAddress.Loopback)));
            return new InvalidDataException(occupied ? Language.ResourceManager.GetString("mcp_host_PortBusy") : Language.agent_missing);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.RequestUri != endpoint) throw new InvalidDataException(Language.agent_refresh);
            try
            {
                // A discovery file can survive a crash. A live editor holds this file exclusively.
                using var unused = File.Open(Path.Combine(directory, "host.lock"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                throw MissingHost();
            }
            catch (IOException ex) when ((ex.HResult & 0xffff) is 32 or 33) { }
            catch (FileNotFoundException) { throw MissingHost(); }
            catch (DirectoryNotFoundException) { throw MissingHost(); }

            string path = Path.Combine(directory, "endpoint.json");
            if (!File.Exists(path)) throw new InvalidDataException(Language.agent_missing);
            using var current = JsonDocument.Parse(await File.ReadAllBytesAsync(path, token));
            if (current.RootElement.GetProperty("endpoint").GetString() != endpoint.AbsoluteUri ||
                current.RootElement.GetProperty("instanceId").GetGuid() != instance)
                throw new InvalidDataException(Language.agent_refresh);
            if (profile is Guid id)
            {
                try { request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", McpProfiles.ReadCredential(directory, id)); }
                catch (Exception ex) when (ex is IOException or CryptographicException)
                { throw new InvalidDataException(Language.agent_profile); }
            }
            return await base.SendAsync(request, token);
        }
    }

    internal static async Task<McpClient> ConnectAsync(string endpoint, Guid? profile, string directory, CancellationToken token)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttp ||
            !Guid.TryParseExact(uri.Segments[^1], "N", out var instance) || uri.Port < 1024 ||
            endpoint != $"http://127.0.0.1:{uri.Port}/mcp/{instance:N}")
            throw new InvalidDataException(Language.agent_refresh);

        var handler = new PinnedHost(directory, uri, instance, profile)
        { InnerHandler = new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false } };
        var upstream = new HttpClientTransport(new() { Endpoint = uri, TransportMode = HttpTransportMode.StreamableHttp },
            new HttpClient(handler), ownsHttpClient: true);
        try { return await McpClient.CreateAsync(upstream, cancellationToken: token); }
        catch { await upstream.DisposeAsync(); throw; }
    }

    private static string Diagnostic(Exception ex) =>
        ex.GetBaseException() is InvalidDataException detail ? detail.Message : Language.agent_failed;

    private static async ValueTask<T> Forward<T>(ValueTask<T> response)
    {
        try { return await response; }
        catch (Exception ex) when (ex is not (McpProtocolException or OperationCanceledException))
        {
            string message = Diagnostic(ex);
            Console.Error.WriteLine(message);
            throw new McpProtocolException(message, McpErrorCode.InternalError);
        }
    }

    internal static McpServerOptions CreateOptions(McpClient client) => new()
    {
        ServerInfo = client.ServerInfo,
        ServerInstructions = client.ServerInstructions,
        Handlers = new()
        {
            ListToolsHandler = (request, token) => Forward(client.ListToolsAsync(request.Params ?? new(), token)),
            CallToolHandler = (request, token) => Forward(client.CallToolAsync(request.Params!, token))
        }
    };

    internal static async Task<int> RunAsync(string[] args)
    {
        try
        {
            Guid? profile = null;
            if (args is [Argument, _, "--profile", var value] && Guid.TryParseExact(value, "N", out var id)) profile = id;
            else if (args is not [Argument, _]) throw new InvalidDataException(Language.agent_refresh);

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await using var client = await ConnectAsync(args[1], profile, McpServerService.UserDirectory, timeout.Token);
            timeout.CancelAfter(Timeout.InfiniteTimeSpan);
            var options = CreateOptions(client);
            await using var server = McpServer.Create(new StdioServerTransport(options), options);
            await server.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(Diagnostic(ex));
            return 1;
        }
    }
}
