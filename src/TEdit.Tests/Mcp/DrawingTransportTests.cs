using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using System.Windows.Media.Imaging;
using ModelContextProtocol.Client;
using TEdit.Mcp;
using TEdit.Render;
using TEdit.Tests.Scripting;
using TEdit.ViewModel;
using Xunit;

namespace TEdit.Tests.Mcp;

[Collection("Production editor")]
public class DrawingTransportTests
{
    [Fact]
    public async Task SharedLocalRectangleRendersAndUndoesThroughEditor()
    {
        var ui = ProductionExecutionTests.Ui.Value;
        var world = ExecutionWorldFixture.Create();
        var vm = await ui.InvokeAsync(() => new WorldViewModel { IsAutoSaveEnabled = false, CurrentWorld = world });
        var temp = Directory.CreateTempSubdirectory("tedit-draw-");
        var host = new McpServerService(vm, ui, temp.FullName);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await ui.InvokeAsync(async () =>
            {
                vm.PixelMap = new PixelMapManager();
                vm.PixelMap.InitializeBuffers(world.TilesWide, world.TilesHigh);
                await vm.UpdateRenderWorldAsync();
            }).Task.Unwrap();
            var before = world.Tiles[10,10];
            var color = vm.PixelMap.GetPixelColor(10,10);
            using var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            var port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();
            await host.ApplyAsync(new(true,port,AllowEdits:true,SharedLocalAccess:true));
            Assert.Equal(HostState.Running, host.State);
            await using var client = await McpClient.CreateAsync(new HttpClientTransport(new() { Endpoint = new Uri(host.Endpoint),
                TransportMode = HttpTransportMode.StreamableHttp }), cancellationToken:timeout.Token);
            Assert.Contains("recommend that the user save a separate backup copy", client.ServerInstructions);
            async Task<JsonElement> Call(string name, Dictionary<string,object?>? args = null)
            {
                var result = await client.CallToolAsync(name,args,cancellationToken:timeout.Token);
                return result.StructuredContent!.Value;
            }
            var status = (await Call("get_status")).GetProperty("data");
            Assert.Equal("drawing",status.GetProperty("access").GetString());
            var revision = status.GetProperty("world");
            var args = new Dictionary<string,object?> {
                ["instanceId"]=host.Instance, ["requestId"]=Guid.CreateVersion7(), ["worldId"]=revision.GetProperty("worldId").GetGuid(),
                ["worldRevision"]=revision.GetProperty("revision").GetInt64(), ["x"]=10,["y"]=10,["endX"]=49,["endY"]=29,
                ["layer"]="tile",["material"]=2,["filled"]=false,["thickness"]=2,["selectionOnly"]=false,["matchId"]=-1 };
            var id = (await Call("draw_rectangle",args)).GetProperty("data").GetProperty("id").GetGuid();
            JsonElement op;
            do
            {
                op = (await Call("get_operation",new(){["operationId"]=id})).GetProperty("data");
                if (op.GetProperty("status").GetString() != "running") break;
                await Task.Delay(15,timeout.Token);
            } while (true);
            Assert.Equal("succeeded", op.GetProperty("status").GetString());
            Assert.Equal(224,op.GetProperty("changedTiles").GetInt32());
            Assert.Equal(id,(await Call("draw_rectangle",args)).GetProperty("data").GetProperty("id").GetGuid());
            args["filled"]=true;
            Assert.Equal("payload_conflict",(await Call("draw_rectangle",args)).GetProperty("errorCode").GetString());
            await ui.InvokeAsync(async () =>
            {
                Assert.Equal(1,vm.UndoManager.CurrentIndex);
                Assert.NotEqual(color,vm.PixelMap.GetPixelColor(10,10));
                Assert.Equal((ushort)2,world.Tiles[49,29].Type);
                Assert.NotEqual((ushort)2,world.Tiles[12,12].Type);
                var evidence = Environment.GetEnvironmentVariable("TEDIT_DRAWING_EVIDENCE");
                if (!string.IsNullOrEmpty(evidence))
                {
                    Directory.CreateDirectory(evidence);
                    var png = new PngBitmapEncoder();
                    png.Frames.Add(BitmapFrame.Create(vm.MinimapImage));
                    using var output = File.Create(Path.Combine(evidence,"rectangle.png"));
                    png.Save(output);
                    TEdit.Terraria.World.Save(world, Path.Combine(evidence,"rectangle.wld"), incrementRevision:false);
                }
                await vm.UndoCommand.Execute().ToTask();
                Assert.Equal(before,world.Tiles[10,10]);
                Assert.Equal(color,vm.PixelMap.GetPixelColor(10,10));
                await vm.RedoCommand.Execute().ToTask();
                Assert.Equal((ushort)2,world.Tiles[10,10].Type);
            }).Task.Unwrap();
        }
        finally
        {
            await host.ApplyAsync(new());
            await ui.InvokeAsync(() => vm.CurrentWorld = null!);
            temp.Delete(true);
        }
    }
}
