using System.Collections.Generic;
using System.Threading;
using Shouldly;
using TEdit.Editor;
using TEdit.Scripting.Api;
using TEdit.Scripting.Engine;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class JintScriptEngineTests
{
    private readonly JintScriptEngine _engine = new();

    private (ScriptApi api, World world, List<string> logs, ScriptExecutionContext context) CreateTestContext()
    {
        var world = TestWorldFactory.CreateWorldWithTerrain();
        var undo = new NoOpUndoManager();
        var selection = new Selection();
        var logs = new List<string>();

        var context = new ScriptExecutionContext
        {
            CancellationToken = CancellationToken.None,
            OnLog = msg => logs.Add(msg),
            OnWarn = msg => logs.Add($"[Warn] {msg}"),
            OnError = msg => logs.Add($"[Error] {msg}")
        };

        var api = new ScriptApi(world, undo, selection, context);
        return (api, world, logs, context);
    }

    [Fact]
    public void Execute_SimpleScript_Succeeds()
    {
        var (api, _, logs, context) = CreateTestContext();

        var result = _engine.Execute("print('Hello from JavaScript!')", api, context);

        result.Success.ShouldBeTrue();
        logs.ShouldContain("Hello from JavaScript!");
    }

    [Fact]
    public void Execute_ReadWorldInfo_Succeeds()
    {
        var (api, _, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            print('World: ' + world.title);
            print('Size: ' + world.width + 'x' + world.height);
        ", api, context);

        result.Success.ShouldBeTrue();
        logs.ShouldContain("World: Test World");
        logs.ShouldContain("Size: 100x100");
    }

    [Fact]
    public void Execute_ModifyTile_ChangesWorld()
    {
        var (api, world, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            tile.setType(5, 5, 1);
            print('Type: ' + tile.getTileType(5, 5));
        ", api, context);

        result.Success.ShouldBeTrue();
        world.Tiles[5, 5].Type.ShouldBe((ushort)1);
        world.Tiles[5, 5].IsActive.ShouldBeTrue();
        logs.ShouldContain("Type: 1");
    }

    [Fact]
    public void Execute_BatchReplace_ChangesMultipleTiles()
    {
        var (api, world, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            var count = batch.replaceTile(1, 56);
            print('Replaced: ' + count);
        ", api, context);

        result.Success.ShouldBeTrue();
        logs.ShouldContain(l => l.StartsWith("Replaced:"));

        // Verify no stone tiles remain
        for (int x = 0; x < world.TilesWide; x++)
            for (int y = 0; y < world.TilesHigh; y++)
                if (world.Tiles[x, y].IsActive)
                    world.Tiles[x, y].Type.ShouldNotBe((ushort)1);
    }

    [Fact]
    public void Execute_SyntaxError_ReturnsFailure()
    {
        var (api, _, _, context) = CreateTestContext();

        var result = _engine.Execute("this is not valid javascript {{{", api, context);

        result.Success.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
    }

    [Fact]
    public void Execute_InfiniteLoop_TimesOut()
    {
        var (api, _, _, _) = CreateTestContext();

        var result = _engine.Execute("while(true) {}", api, new ScriptExecutionContext
        {
            TimeoutMs = 500,
            MaxStatements = int.MaxValue,
            CancellationToken = CancellationToken.None
        });

        result.Success.ShouldBeFalse();
        result.Error.ShouldContain("timed out", Case.Insensitive);
    }

    [Fact]
    public void Execute_Cancellation_StopsScript()
    {
        var (api, _, _, _) = CreateTestContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancel

        var result = _engine.Execute("while(true) {}", api, new ScriptExecutionContext
        {
            CancellationToken = cts.Token
        });

        result.Success.ShouldBeFalse();
        result.Error.ShouldContain("cancel", Case.Insensitive);
    }

    [Fact]
    public void Execute_FindTiles_ReturnsResults()
    {
        var (api, _, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            var results = batch.findTiles(function(x, y) {
                return tile.isActive(x, y) && tile.getTileType(x, y) === 0;
            });
            print('Found: ' + results.length);
        ", api, context);

        result.Success.ShouldBeTrue();
        logs.ShouldContain(l => l.StartsWith("Found:") && l != "Found: 0");
    }

    [Fact]
    public void Execute_GeometryFillRect_CreatesCoordinates()
    {
        var (api, _, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            var coords = geometry.fillRect(5, 5, 3, 3);
            print('Coords: ' + coords.length);
        ", api, context);

        result.Success.ShouldBeTrue();
        logs.ShouldContain("Coords: 9");
    }

    [Fact]
    public void Execute_SetAndReadLiquid()
    {
        var (api, world, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            tile.setLiquid(10, 10, 255, 1);
            print('Liquid: ' + tile.getLiquidAmount(10, 10));
            print('Type: ' + tile.getLiquidType(10, 10));
        ", api, context);

        result.Success.ShouldBeTrue();
        logs.ShouldContain("Liquid: 255");
        logs.ShouldContain("Type: 1");
    }

    [Fact]
    public void Execute_SetAndReadWires()
    {
        var (api, _, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            tile.setWire(10, 10, 1, true);
            tile.setWire(10, 10, 3, true);
            print('Red: ' + tile.getWire(10, 10, 1));
            print('Green: ' + tile.getWire(10, 10, 3));
            print('Blue: ' + tile.getWire(10, 10, 2));
        ", api, context);

        result.Success.ShouldBeTrue();
        logs.ShouldContain("Red: true");
        logs.ShouldContain("Green: true");
        logs.ShouldContain("Blue: false");
    }

    [Fact]
    public void Execute_ClearTile_ResetsTile()
    {
        var (api, world, _, context) = CreateTestContext();

        // Set up a tile then clear it
        world.Tiles[5, 5].IsActive = true;
        world.Tiles[5, 5].Type = 42;
        world.Tiles[5, 5].WireRed = true;

        var result = _engine.Execute("tile.clear(5, 5)", api, context);

        result.Success.ShouldBeTrue();
        world.Tiles[5, 5].IsActive.ShouldBeFalse();
        world.Tiles[5, 5].WireRed.ShouldBeFalse();
    }

    [Fact]
    public void Execute_OutOfBoundsAccess_ReturnsError()
    {
        var (api, _, _, context) = CreateTestContext();

        var result = _engine.Execute("tile.getTileType(999, 999)", api, context);

        result.Success.ShouldBeFalse();
        result.Error.ShouldContain("out of bounds");
    }
}
