using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
using TEdit.Scripting;
using TEdit.Scripting.Execution;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Mcp;

internal sealed record InspectionResult(string? ErrorCode, object? Data)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    internal CallToolResult ToProtocol()
    {
        var content = JsonSerializer.SerializeToElement(this, Json);
        var text = content.GetRawText();
        if (System.Text.Encoding.UTF8.GetByteCount(text) > 16384) return Error("response_too_large").ToProtocol();
        return new() { IsError = ErrorCode != null, StructuredContent = content, Content = [new TextContentBlock { Text = text }] };
    }
    public static InspectionResult Error(string code) => new(code, null);
    public static InspectionResult Ok(object data) => new(null, data);
}

internal sealed class InspectionSource(WorldViewModel vm, Dispatcher dispatcher)
{
    internal Task<InspectionResult> Read(Func<World, WorldViewModel, object> capture, CancellationToken token) =>
        OnUiAsync(() =>
        {
            var rejection = vm.WorldOperations.TryAcquire(false, null, out var lease, name: "MCP inspection");
            if (rejection == OperationRejection.NoWorld) return new InspectionResult("no_world", new { action = "open_world_in_tedit" });
            if (lease == null) return InspectionResult.Error("busy");
            using (lease)
            {
                var world = vm.CurrentWorld;
                var revision = vm.WorldOperations.GetRevision(world)!.Value;
                var snapshot = capture(world, vm);
                if (snapshot is InspectionResult error) return error;
                return InspectionResult.Ok(new { revision.WorldId, worldRevision = revision.Revision, snapshot });
            }
        }, token);

    internal Task<InspectionResult> Status(Guid instance, McpPreferences settings, CancellationToken token) => OnUiAsync(() =>
        InspectionResult.Ok(new { instanceId = instance, nextRequestId = Guid.CreateVersion7(),
            connectionMode = settings.SharedLocalAccess ? "shared_local" : "client_profile",
            scriptExecution = settings.AllowEdits && settings.AllowScripts,
            access = settings.AllowEdits ? "drawing" : "inspect_only", busy = vm.WorldOperations.IsBusy,
            world = vm.WorldOperations.GetRevision(vm.CurrentWorld) }), token);

    private async Task<InspectionResult> OnUiAsync(Func<InspectionResult> read, CancellationToken caller)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(caller);
        deadline.CancelAfter(TimeSpan.FromSeconds(3));
        try { return await dispatcher.InvokeAsync(read, DispatcherPriority.Background, deadline.Token).Task; }
        catch (OperationCanceledException) when (!caller.IsCancellationRequested) { return InspectionResult.Error("editor_timeout"); }
    }
}

[McpServerToolType]
internal sealed class InspectionTools(McpServerService server, InspectionSource source, IHttpContextAccessor http)
{
    [McpServerTool(Name = "get_status", ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult)), Description("Inspect the TEdit instance, access mode, and current world revision.")]
    public async Task<CallToolResult> Status(CancellationToken token) => (await source.Status(server.Instance, server.Preferences, token)).ToProtocol();

    [McpServerTool(Name = "get_world_info", ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult)), Description("Read world dimensions and spawn position.")]
    public async Task<CallToolResult> WorldInfo(CancellationToken token) => (await source.Read((w, _) =>
        new { width = w.TilesWide, height = w.TilesHigh, w.SpawnX, w.SpawnY }, token)).ToProtocol();

    [McpServerTool(Name = "get_selection", ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult)), Description("Read the current rectangular selection.")]
    public async Task<CallToolResult> Selection(CancellationToken token) => (await source.Read((_, vm) =>
    {
        var area = vm.Selection.SelectionArea;
        return new { vm.Selection.IsActive, area.X, area.Y, area.Width, area.Height };
    }, token)).ToProtocol();

    [McpServerTool(Name = "read_region", ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult)), Description("Read up to 64 tiles in row-major order. Request adjacent rectangles for larger areas.")]
    public async Task<CallToolResult> Region(int x, int y, int width, int height, CancellationToken token) => (await source.Read((w, _) =>
    {
        if (x < 0 || y < 0 || width < 1 || height < 1 || (long)width * height > 64 ||
            x > w.TilesWide - width || y > w.TilesHigh - height) return InspectionResult.Error("invalid_region");
        var cells = new List<object>(width * height);
        for (int j = y; j < y + height; j++)
        for (int i = x; i < x + width; i++)
        {
            var t = w.Tiles[i, j];
            cells.Add(new { active = t.IsActive, tile = t.Type, wall = t.Wall, liquid = t.LiquidAmount });
        }
        return new { x, y, width, height, cells };
    }, token)).ToProtocol();

    private static InspectionResult Page<T>(IEnumerable<T> rows, int offset, int limit)
    {
        if (offset < 0 || limit is < 1 or > 16) return InspectionResult.Error("invalid_page");
        var items = rows.Skip(offset).Take(limit + 1).ToArray();
        return InspectionResult.Ok(new { items = items.Take(limit).ToArray(), nextOffset = items.Length > limit ? (int?)(offset + limit) : null });
    }

    [McpServerTool(Name = "search_metadata", ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult)), Description("Search tile, wall, or item names. Pages contain at most 16 entries.")]
    public CallToolResult Metadata(string kind, string query = "", int offset = 0, int limit = 16)
    {
        if (query == null || query.Length > 80) return InspectionResult.Error("invalid_query").ToProtocol();
        IEnumerable<(int id, string name)>? catalog = kind switch
        {
            "tile" => WorldConfiguration.TileProperties.Select(p => ((int)p.Id, p.Name ?? "")),
            "wall" => WorldConfiguration.WallProperties.Select(p => ((int)p.Id, p.Name ?? "")),
            "item" => WorldConfiguration.ItemProperties.Select(p => ((int)p.Id, p.Name ?? "")),
            _ => null
        };
        return (catalog == null ? InspectionResult.Error("invalid_kind") : Page(catalog.Where(p => p.name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Select(p => new { p.id, name = p.name[..Math.Min(96, p.name.Length)] }), offset, limit)).ToProtocol();
    }

    [McpServerTool(Name = "get_api_reference", ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult)), Description("Browse local sidebar API signatures. These methods are documentation only.")]
    public CallToolResult Reference(string module = "", int offset = 0, int limit = 16) =>
        Page(ScriptApiMetadata.Modules.Where(m => module == "" || m.Name == module)
            .SelectMany(m => m.Methods.Select(f => new { module = m.Name, f.Signature })), offset, limit).ToProtocol();

    [McpServerTool(Name = "get_operation", ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult)), Description("Get the state of an operation in the current profile or shared local session.")]
    public CallToolResult GetOperation(Guid operationId) => Operation(operationId, false).ToProtocol();
    [McpServerTool(Name = "cancel_operation", UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult)), Description("Request cancellation of an operation in the current profile or shared local session.")]
    public CallToolResult CancelOperation(Guid operationId) => Operation(operationId, true).ToProtocol();
    private InspectionResult Operation(Guid id, bool cancel) =>
        http.HttpContext?.Items[typeof(McpProfiles)] is Guid owner && server.Operations.Find(owner, id, cancel) is { } op
            ? InspectionResult.Ok(op) : InspectionResult.Error("operation_not_found");
}
