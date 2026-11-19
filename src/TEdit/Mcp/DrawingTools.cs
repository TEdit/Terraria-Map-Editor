using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TEdit.Geometry;
using TEdit.Scripting.Api;
using TEdit.Scripting.Execution;
using TEdit.ViewModel;

namespace TEdit.Mcp;

internal sealed class DrawingHost(WorldViewModel vm, Dispatcher dispatcher)
{
    public async Task<EditOutcome?> Run(DrawingSpec spec, WorldRevision expected, CancellationToken token)
    {
        try { return await dispatcher.InvokeAsync(() => RunOnUi(spec, expected, token), DispatcherPriority.Normal, token).Task.Unwrap(); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { return new("canceled", null, 0); }
    }

    private async Task<EditOutcome?> RunOnUi(DrawingSpec spec, WorldRevision expected, CancellationToken token)
    {
        var rejection = vm.WorldOperations.TryAcquire(true, expected, out var lease, "MCP drawing");
        if (lease == null) return new("failed", rejection switch
        {
            OperationRejection.NoWorld => "no_world", OperationRejection.StaleWorld => "stale_world", _ => "busy"
        }, 0);
        using (lease)
        using (lease.Enter())
        using (var stop = CancellationTokenSource.CreateLinkedTokenSource(token, vm.WorldOperations.ActiveCancellation))
        {
            var world = vm.CurrentWorld;
            RectangleInt32? selection = vm.Selection.IsActive ? vm.Selection.SelectionArea : null;
            if (spec.Validate(world, selection, out var sprite) is string error) return new("failed", error, 0);
            int changed = 0, visited = 0;
            string status = "succeeded";
            string? code = null;
            try
            {
                stop.Token.ThrowIfCancellationRequested();
                if (sprite != null)
                {
                    try { new SpriteApi(world, vm.UndoManager).PlaceSprite(sprite, spec.X, spec.Y); }
                    finally
                    {
                        // Validation required an empty footprint. Count writes even if entity creation fails.
                        for (int y = 0; y < sprite.SizeTiles.Y; y++)
                        for (int x = 0; x < sprite.SizeTiles.X; x++)
                            if (world.Tiles[spec.X + x, spec.Y + y].IsActive) changed++;
                    }
                }
                else foreach (var point in spec.Points())
                {
                    stop.Token.ThrowIfCancellationRequested();
                    if (spec.Paint(world, vm.UndoManager, selection, point)) changed++;
                    if (++visited % 256 == 0) await Dispatcher.Yield(DispatcherPriority.Background);
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { status = "canceled"; }
            catch { status = "failed"; code = "drawing_failed"; }
            // A failed sprite may have staged undo tiles before its first world write.
            if (changed > 0 || (sprite != null && status == "failed"))
            {
                try { await vm.FinalizeScriptExecutionAsync(); }
                catch { return new("failed", "finalization_failed", changed); }
            }
            return new(status, code, changed);
        }
    }
}

[McpServerToolType]
internal sealed class DrawingTools(McpServerService server, DrawingHost host, IHttpContextAccessor http)
{
    [McpServerTool(Name = "draw_rectangle", UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult))]
    [Description("Thickness must be 1 to 32, including for a filled shape. Draw a rectangle between inclusive top-left and bottom-right tile coordinates. Layer: tile or wall. matchId: -1 for any. Use the inspected world identity and revision, instanceId and nextRequestId as requestId. Poll get_operation.")]
    public CallToolResult Rectangle(Guid instanceId, Guid requestId, Guid worldId, long worldRevision,
        int x, int y, int endX, int endY, bool filled, int thickness, string layer, int material, bool selectionOnly, int matchId) =>
        Submit(instanceId, requestId, new(worldId, worldRevision), new("rectangle", x, y, endX, endY, filled, thickness, layer, material, selectionOnly, matchId));

    [McpServerTool(Name = "draw_line", UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult))]
    [Description("Draw a line with a square brush between inclusive endpoints. Layer is tile or wall. Use matchId -1 for any. Use get_status for instanceId and world identity. Pass nextRequestId as requestId, then poll get_operation.")]
    public CallToolResult Line(Guid instanceId, Guid requestId, Guid worldId, long worldRevision,
        int x, int y, int endX, int endY, int thickness, string layer, int material, bool selectionOnly, int matchId) =>
        Submit(instanceId, requestId, new(worldId, worldRevision), new("line", x, y, endX, endY, false, thickness, layer, material, selectionOnly, matchId));

    [McpServerTool(Name = "draw_ellipse", UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult))]
    [Description("Draw a filled or outlined ellipse inside inclusive corners. Supply thickness 1 to 32 even when filled. Layer is tile or wall. Use matchId -1 for any. Use get_status for instanceId and world identity. Pass nextRequestId as requestId, then poll get_operation.")]
    public CallToolResult Ellipse(Guid instanceId, Guid requestId, Guid worldId, long worldRevision,
        int x, int y, int endX, int endY, bool filled, int thickness, string layer, int material, bool selectionOnly, int matchId) =>
        Submit(instanceId, requestId, new(worldId, worldRevision), new("ellipse", x, y, endX, endY, filled, thickness, layer, material, selectionOnly, matchId));

    [McpServerTool(Name = "fill_region", UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult))]
    [Description("Fill an inclusive rectangular region, optionally replacing a matching material. Layer is tile or wall. Use matchId -1 for any. Use get_status for instanceId and world identity. Pass nextRequestId as requestId, then poll get_operation.")]
    public CallToolResult FillRegion(Guid instanceId, Guid requestId, Guid worldId, long worldRevision,
        int x, int y, int endX, int endY, string layer, int material, bool selectionOnly, int matchId) =>
        Submit(instanceId, requestId, new(worldId, worldRevision), new("fill", x, y, endX, endY, true, 1, layer, material, selectionOnly, matchId));

    [McpServerTool(Name = "place_sprite", UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult))]
    [Description("Place a sprite style at its top-left tile. The entire footprint must be empty and within the requested bounds and selection. Use get_status for instanceId and world identity. Pass nextRequestId as requestId, then poll get_operation.")]
    public CallToolResult PlaceSprite(Guid instanceId, Guid requestId, Guid worldId, long worldRevision,
        int x, int y, int tileId, int style, bool selectionOnly) =>
        Submit(instanceId, requestId, new(worldId, worldRevision), new("sprite", x, y, x, y, true, 1, "tile", tileId, selectionOnly, -1, style));

    private CallToolResult Submit(Guid instance, Guid request, WorldRevision expected, DrawingSpec spec)
    {
        if (!server.Preferences.AllowEdits || http.HttpContext?.Items[typeof(McpProfiles)] is not Guid owner)
            return InspectionResult.Error("editing_disabled").ToProtocol();
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { expected, spec })));
        var admission = server.Operations.StartEdit(owner, instance, request, hash, ct => host.Run(spec, expected, ct));
        return (admission.ErrorCode is string code ? InspectionResult.Error(code) : InspectionResult.Ok(admission.Operation!)).ToProtocol();
    }
}
