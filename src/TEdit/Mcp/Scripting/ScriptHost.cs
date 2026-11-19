using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Scripting.Api;
using TEdit.Scripting.Execution;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Mcp.Scripting;

internal sealed class ScriptHost(WorldViewModel vm, Dispatcher dispatcher, string executable)
{
    internal async Task<EditOutcome?> Run(ScriptInput input, WorldRevision expected, CancellationToken token)
    {
        try { return await dispatcher.InvokeAsync(() => RunOnUi(input, expected, token), DispatcherPriority.Normal, token).Task.Unwrap(); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { return new("canceled", null, 0); }
    }

    private async Task<EditOutcome?> RunOnUi(ScriptInput input, WorldRevision expected, CancellationToken caller)
    {
        if (caller.IsCancellationRequested) return new("canceled", null, 0);
        var rejection = vm.WorldOperations.TryAcquire(true, expected, out var lease, "MCP script");
        if (lease == null) return new("failed", rejection switch
        {
            OperationRejection.StaleWorld => "stale_world", OperationRejection.NoWorld => "no_world", _ => "busy"
        }, 0);
        using (lease)
        using (lease.Enter())
        using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(caller, vm.WorldOperations.ActiveCancellation))
        using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancel.Token))
        {
            bool ready = false;
            deadline.CancelAfter(ScriptProtocol.StartupMs);
            var session = new ScriptSession(vm.CurrentWorld, vm.UndoManager,
                vm.Selection.IsActive ? vm.Selection.SelectionArea : null);
            string status = "succeeded";
            ScriptDiagnostic? error = null;
            try
            {
                // Only validated scalar requests cross back to the editor dispatcher.
                async Task<JsonElement> Dispatch(ScriptMessage message)
                {
                    try
                    {
                        return await dispatcher.InvokeAsync(() => session.Call(message, deadline.Token),
                            DispatcherPriority.Background, deadline.Token).Task.Unwrap();
                    }
                    catch (Exception e) when (e is not (OperationCanceledException or ScriptLimitException))
                    {
                        throw new ScriptLimitException("editor_operation_failed");
                    }
                }
                error = await Task.Run(() => ScriptProcess.Run(executable, input, Dispatch, () =>
                {
                    ready = true;
                    deadline.CancelAfter(ScriptProtocol.TimeoutMs + ScriptProtocol.DeadlineGraceMs);
                }, deadline.Token));
                if (error != null) status = "failed";
            }
            catch (OperationCanceledException) when (deadline.IsCancellationRequested)
            {
                status = cancel.IsCancellationRequested ? "canceled" : "failed";
                if (status == "failed") error = new(ready ? "timeout" : "worker_start_timeout");
            }
            catch (ScriptLimitException e) { status = "failed"; error = new(e.Message); }
            catch (Exception) { status = "failed"; error = new("worker_unavailable"); }

            if (session.ChangedTiles > 0 || session.SpriteUndoPending)
            {
                try { await vm.FinalizeScriptExecutionAsync(); }
                catch { status = "failed"; error = new("finalization_failed"); }
            }
            return new(status, error?.Code, session.ChangedTiles, new(session.Log.ToArray(), error));
        }
    }
}

internal sealed class ScriptSession(World world, IUndoManager undo, RectangleInt32? selection)
{
    private readonly HashSet<Vector2Int32> _changed = [];
    private int _work, _logChars;
    internal bool SpriteUndoPending { get; private set; }
    internal int ChangedTiles => _changed.Count;
    internal List<string> Log { get; } = [];

    internal async Task<JsonElement> Call(ScriptMessage message, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var args = message.Arguments ?? throw new ScriptLimitException("invalid_arguments");
        int I(int i) => args[i].ValueKind == JsonValueKind.Number && args[i].TryGetInt32(out int n)
            ? n : throw new ScriptLimitException("invalid_arguments");
        bool B(int i) => args[i].ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false,
            _ => throw new ScriptLimitException("invalid_arguments") };
        string S(int i) => args[i].ValueKind == JsonValueKind.String ? args[i].GetString()! : throw new ScriptLimitException("invalid_arguments");
        switch (message.Method)
        {
            case "log.print" when args.Length == 1:
                if (args[0].ValueKind is not (JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False))
                    throw new ScriptLimitException("invalid_arguments");
                var line = args[0].ToString();
                if (Log.Count >= ScriptProtocol.MaxLogEntries || line.Length + _logChars > ScriptProtocol.MaxLogChars)
                    throw new ScriptLimitException("log_limit");
                _logChars += line.Length;
                Log.Add(line);
                return JsonSerializer.SerializeToElement(0);
            case "tile.isActive" or "tile.getTileType" or "tile.getWall" when args.Length == 2:
                int x = I(0), y = I(1);
                if (!world.ValidTileLocation(x, y)) throw new ScriptLimitException("invalid_bounds");
                var tile = world.Tiles[x,y];
                if (message.Method == "tile.isActive") return JsonSerializer.SerializeToElement(tile.IsActive);
                return JsonSerializer.SerializeToElement(message.Method == "tile.getWall" ? tile.Wall : tile.Type);
            case "draw.line" when args.Length == 9:
                return await Draw(new("line", I(0), I(1), I(2), I(3), false, I(4), S(5), I(6), B(7), I(8)), token);
            case "draw.rectangle" or "draw.ellipse" when args.Length == 10:
                return await Draw(new(message.Method == "draw.rectangle" ? "rectangle" : "ellipse",
                    I(0), I(1), I(2), I(3), B(4), I(5), S(6), I(7), B(8), I(9)), token);
            case "draw.placeSprite" when args.Length == 5:
                return await Draw(new("sprite", I(0), I(1), I(0), I(1), true, 1, "tile", I(2), B(4), -1, I(3)), token);
            case "draw.fillRegion" when args.Length == 8:
                return await Draw(new("fill", I(0), I(1), I(2), I(3), true, 1, S(4), I(5), B(6), I(7)), token);
            default:
                throw new ScriptLimitException("invalid_arguments");
        }
    }

    private async Task<JsonElement> Draw(DrawingSpec spec, CancellationToken token)
    {
        if (spec.Validate(world, selection, out var sprite) is string error) throw new ScriptLimitException(error);
        int area = checked((int)spec.CandidateCells(sprite));
        if (area > ScriptProtocol.WorkLimit - _work) throw new ScriptLimitException("work_limit");
        _work += area;
        if (sprite != null)
        {
            SpriteUndoPending = true;
            try { new SpriteApi(world, undo).PlaceSprite(sprite, spec.X, spec.Y); }
            finally
            {
                // The validated footprint was empty. Count actual writes even on a placement failure.
                for (int y = spec.Y; y < spec.Y + sprite.SizeTiles.Y; y++)
                for (int x = spec.X; x < spec.X + sprite.SizeTiles.X; x++)
                    if (world.Tiles[x,y].IsActive) _changed.Add(new(x,y));
            }
            return JsonSerializer.SerializeToElement(area);
        }
        int writes = 0, visited = 0;
        foreach (var point in spec.Points())
        {
            token.ThrowIfCancellationRequested();
            if (spec.Paint(world, undo, selection, point))
            {
                _changed.Add(point);
                writes++;
            }
            if (++visited % 256 == 0) await Dispatcher.Yield(DispatcherPriority.Background);
        }
        return JsonSerializer.SerializeToElement(writes);
    }
}
