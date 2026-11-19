using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using System.Windows.Threading;
using ModelContextProtocol.Client;
using TEdit.Configuration;
using TEdit.Editor;
using TEdit.Mcp;
using TEdit.Render;
using TEdit.Scripting.Api;
using TEdit.Scripting.Engine;
using TEdit.Scripting.Execution;
using TEdit.Terraria;
using TEdit.Tests.Scripting;
using TEdit.ViewModel;
using Xunit;

namespace TEdit.Tests.Mcp;

[Collection("Production editor")]
public class DrawingSessionTests
{
    [Fact]
    public void FreshSettingsAllowSharedDrawingAndSavedPermissionsArePreserved()
    {
        var fresh = JsonSerializer.Deserialize<UserSettings>("{}")!.McpServer;
        Assert.True(fresh.SharedLocalAccess);
        Assert.False(fresh.Enabled);
        Assert.True(fresh.AllowEdits);
        Assert.False(fresh.AllowScripts);
        var legacy = JsonSerializer.Deserialize<UserSettings>("""{"McpServer":{"Enabled":true,"Port":48190}}""")!.McpServer;
        Assert.True(legacy.Enabled);
        Assert.False(legacy.SharedLocalAccess);
        Assert.False(legacy.AllowEdits);

        var readOnly = new UserSettings { McpServer = fresh with { Enabled = true, AllowEdits = false } };
        var saved = JsonSerializer.Serialize(readOnly);
        Assert.Equal(readOnly.McpServer, JsonSerializer.Deserialize<UserSettings>(saved)!.McpServer);
    }

    private static Dictionary<string, object?> Region(int x, int y, int ex, int ey, string layer, int material) =>
        new() { ["x"] = x, ["y"] = y, ["endX"] = ex, ["endY"] = ey, ["layer"] = layer,
            ["material"] = material, ["selectionOnly"] = false, ["matchId"] = -1 };

    [Fact]
    public async Task TypedToolsMatchSidebarEnginesAndPreserveFramesEntitiesAndUndo()
    {
        await using var s = new McpEditorSession();
        await s.Open();
        s.World.Tiles[28, 40].BrickStyle = BrickStyle.SlopeTopRight;
        await s.Ui.InvokeAsync(() =>
        {
            s.Vm.TilePicker.IsEraser = true;
            s.Vm.Selection.SetRectangle(new(10, 10), new(23, 23));
        });
        var line = Region(30, 40, 28, 40, "tile", 2);
        line["thickness"] = 1;
        await s.Expect("draw_line", line, 3);
        var ellipse = Region(0, 0, 3, 3, "wall", 1);
        ellipse["filled"] = false;
        ellipse["thickness"] = 1;
        await s.Expect("draw_ellipse", ellipse, 8);
        var rectangle = Region(9, 9, 12, 12, "wall", 1);
        rectangle["selectionOnly"] = true;
        rectangle["filled"] = true;
        rectangle["thickness"] = 1;
        await s.Expect("draw_rectangle", rectangle, 9);
        var fill = Region(28, 40, 29, 42, "tile", 1);
        fill["matchId"] = 2;
        await s.Expect("fill_region", fill, 2);
        Dictionary<string, object?> Sprite(int x, int y, int style = 0) =>
            new() { ["x"] = x, ["y"] = y, ["tileId"] = 21, ["style"] = style, ["selectionOnly"] = true };
        await s.Expect("place_sprite", Sprite(23, 23), 0, "outside_selection");
        await s.Expect("place_sprite", Sprite(99, 99), 0, "invalid_bounds");
        await s.Expect("place_sprite", Sprite(20, 20, -1), 0, "unknown_sprite");
        await s.Expect("place_sprite", Sprite(20, 20), 4);
        await s.Expect("place_sprite", Sprite(20, 20), 0, "occupied");
        Assert.Equal(5, s.Vm.UndoManager.CurrentIndex);
        Assert.True(s.World.IsAnchor(20, 20));
        Assert.NotNull(s.World.GetChestAtTile(20, 20, true));
        Assert.Equal((short)18, s.World.Tiles[21, 21].U);
        Assert.Equal((short)18, s.World.Tiles[21, 21].V);
        Assert.Equal(BrickStyle.SlopeTopRight, s.World.Tiles[28, 40].BrickStyle);
        foreach (IScriptEngine engine in new IScriptEngine[] { new JintScriptEngine(), new LuaScriptEngine() })
        {
            var copy = ExecutionWorldFixture.Create();
            copy.Tiles[28, 40].BrickStyle = BrickStyle.SlopeTopRight;
            var selection = new Selection();
            selection.SetRectangle(new(10, 10), new(23, 23));
            var context = new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.LocalSidebar };
            using var api = new ScriptApi(copy, new NoOpUndoManager(), selection, context);
            var run = engine.Execute("""
                draw.line(30,40,28,40,1,"tile",2,false,-1)
                draw.ellipse(0,0,3,3,false,1,"wall",1,false,-1)
                draw.rectangle(9,9,12,12,true,1,"wall",1,true,-1)
                draw.fillRegion(28,40,29,42,"tile",1,false,2)
                draw.placeSprite(20,20,21,0,true)
                """, api, context);
            Assert.True(run.Success, run.Error);
            for (int y = 0; y < copy.TilesHigh; y++)
            for (int x = 0; x < copy.TilesWide; x++)
                Assert.Equal(copy.Tiles[x, y], s.World.Tiles[x, y]);
            Assert.NotNull(copy.GetChestAtTile(20, 20, true));
        }
        await s.Ui.InvokeAsync(async () =>
        {
            await s.Vm.UndoCommand.Execute().ToTask();
            Assert.Empty(s.World.Chests);
            Assert.Null(s.World.GetChestAtTile(20, 20, true));
            Assert.False(s.World.Tiles[21, 21].IsActive);
            await s.Vm.RedoCommand.Execute().ToTask();
            Assert.Single(s.World.Chests);
            Assert.NotNull(s.World.GetChestAtTile(20, 20, true));
            Assert.True(s.World.IsAnchor(20, 20));
            Assert.Equal((short)18, s.World.Tiles[21, 21].V);
        }).Task.Unwrap();
        var path = Path.Combine(s.Temp.FullName, "drawn.wld");
        await World.SaveAsync(s.World, path, incrementRevision: false);
        var (loaded, error) = World.LoadWorld(path);
        Assert.Null(error);
        Assert.Equal(BrickStyle.SlopeTopRight, loaded.Tiles[28, 40].BrickStyle);
        Assert.Equal(1, loaded.Tiles[1, 0].Wall);
        Assert.Equal(2, loaded.Tiles[30, 40].Type);
        Assert.Equal(18, loaded.Tiles[21, 21].U);
        Assert.True(loaded.IsAnchor(20, 20));
        Assert.NotNull(loaded.GetChestAtTile(20, 20, true));
    }

    [Theory]
    [InlineData(55)]  // sign
    [InlineData(395)] // item frame
    public async Task SpriteUndoAndRedoRestoreAssociatedEntity(int tileId)
    {
        await using var s = new McpEditorSession();
        await s.Open();
        var args = new Dictionary<string, object?> { ["x"] = 22, ["y"] = 23, ["tileId"] = tileId,
            ["style"] = 0, ["selectionOnly"] = false };
        var id = await s.Start("place_sprite", args);
        var op = await s.Finish(id);
        Assert.True(JsonElement.DeepEquals(op, await s.Call(s.Observer, "cancel_operation", new() { ["operationId"] = id })));
        Assert.Equal("succeeded", op.GetProperty("status").GetString());
        await s.Ui.InvokeAsync(async () =>
        {
            object? Entity() => tileId == 55 ? s.World.GetSignAtTile(22, 23, true) : s.World.GetTileEntityAtTile(22, 23, true);
            Assert.NotNull(Entity());
            Assert.Equal(1, s.Vm.UndoManager.CurrentIndex);
            await s.Vm.UndoCommand.Execute().ToTask();
            Assert.Equal(0, tileId == 55 ? s.World.Signs.Count : s.World.TileEntities.Count);
            Assert.Null(Entity());
            Assert.False(s.World.Tiles[22, 23].IsActive);
            await s.Vm.RedoCommand.Execute().ToTask();
            Assert.NotNull(Entity());
            Assert.Equal(1, tileId == 55 ? s.World.Signs.Count : s.World.TileEntities.Count);
            Assert.True(s.World.IsAnchor(22, 23));
        }).Task.Unwrap();
    }

    [Fact]
    public async Task SecondLocalClientCanCancelPartialDrawingAndInspectOnlyCannotDraw()
    {
        await using var s = new McpEditorSession();
        await s.Open();
        await s.Ui.InvokeAsync(() =>
        {
            var options = new View.Popups.SettingsWindow(s.Vm, s.Host);
            try
            {
                var panel = (System.Windows.Controls.StackPanel)options.FindName("McpProfilesPanel");
                Assert.Equal(System.Windows.Visibility.Collapsed, panel.Visibility);
                var setup = ((System.Windows.Controls.TextBox)options.FindName("McpSetup")).Text;
                Assert.Equal(string.Format(Properties.Language.mcp_shared_setup, s.Host.Endpoint), setup);
            }
            finally { options.Close(); }
        });
        Assert.Empty(s.Temp.GetFiles("*.profile"));
        var paused = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var release = new ManualResetEventSlim();
        // Interpose a higher priority dispatcher callback at the first cooperative yield.
        void OnGate()
        {
            if (s.Vm.WorldOperations.ActiveName != "MCP drawing") return;
            s.Ui.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                paused.TrySetResult();
                release.Wait(TimeSpan.FromSeconds(12));
            }));
        }
        s.Vm.WorldOperations.StateChanged += OnGate;
        var fill = Region(0, 0, 99, 99, "wall", 1);
        Guid id;
        try
        {
            id = await s.Start("fill_region", fill);
            await paused.Task.WaitAsync(s.Deadline.Token);
            var rejection = s.Vm.WorldOperations.TryAcquire(true, null, out var competing);
            using (competing) Assert.Equal(OperationRejection.Busy, rejection);
            var args = new Dictionary<string, object?> { ["operationId"] = id };
            await s.Call(s.Observer, "cancel_operation", args);
        }
        finally
        {
            s.Vm.WorldOperations.StateChanged -= OnGate;
            release.Set();
        }
        var op = await s.Finish(id);
        Assert.True(JsonElement.DeepEquals(op, await s.Call(s.Observer, "cancel_operation", new() { ["operationId"] = id })));
        Assert.Equal("canceled", op.GetProperty("status").GetString());
        Assert.True(op.GetProperty("partialChanges").GetBoolean());
        Assert.Equal(256, op.GetProperty("changedTiles").GetInt32());
        Assert.Equal(id, (await s.Call(s.Observer, "fill_region", fill)).GetProperty("id").GetGuid());
        await s.Ui.InvokeAsync(async () =>
        {
            Assert.Equal(256, s.World.Tiles.Cast<Tile>().Count(t => t.Wall == 1));
            Assert.Equal(1, s.Vm.UndoManager.CurrentIndex);
            await s.Vm.UndoCommand.Execute().ToTask();
            Assert.DoesNotContain(s.World.Tiles.Cast<Tile>(), t => t.Wall == 1);
        }).Task.Unwrap();
        fill["requestId"] = Guid.CreateVersion7();
        var stale = (await s.Call(s.Writer, "fill_region", fill)).GetProperty("id").GetGuid();
        Assert.Equal("stale_world", (await s.Finish(stale)).GetProperty("errorCode").GetString());
        await s.Host.ApplyAsync(s.Host.Preferences with { AllowEdits = false });
        await using var reader = await s.Connect();
        Assert.DoesNotContain(await reader.ListToolsAsync(cancellationToken: s.Deadline.Token), t => t.Name == "fill_region");
        // The SDK reports an unknown tool as a protocol error or a tool error, depending on transport.
        try
        {
            var denied = await reader.CallToolAsync("fill_region", fill, cancellationToken: s.Deadline.Token);
            Assert.True(denied.IsError);
        }
        catch (ModelContextProtocol.McpProtocolException e) { Assert.Contains("fill_region", e.Message); }
        using var http = new HttpClient(new HttpClientHandler { UseProxy = false });
        using var baseline = new HttpRequestMessage(HttpMethod.Get, s.Host.Endpoint);
        using var okHost = await http.SendAsync(baseline, HttpCompletionOption.ResponseHeadersRead, s.Deadline.Token);
        Assert.NotEqual(HttpStatusCode.Forbidden, okHost.StatusCode);
        foreach (string header in new[] { "Host", "Origin" })
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, s.Host.Endpoint);
            req.Headers.Add(header, header == "Host" ? "example.invalid" : new Uri(s.Host.Endpoint).GetLeftPart(UriPartial.Authority));
            using var response = await http.SendAsync(req, s.Deadline.Token);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
