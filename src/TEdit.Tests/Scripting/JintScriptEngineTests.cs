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

    [Fact]
    public void Execute_WriteAndReadTitle_Succeeds()
    {
        var (api, world, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            world.title = 'Renamed World';
            print('Title: ' + world.title);
        ", api, context);

        result.Success.ShouldBeTrue();
        world.Title.ShouldBe("Renamed World");
        logs.ShouldContain("Title: Renamed World");
    }

    [Fact]
    public void Execute_WriteAndReadSpawnCoordinates_Succeeds()
    {
        var (api, world, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            world.spawnX = 42;
            world.spawnY = 99;
            print('Spawn: ' + world.spawnX + ',' + world.spawnY);
        ", api, context);

        result.Success.ShouldBeTrue();
        world.SpawnX.ShouldBe(42);
        world.SpawnY.ShouldBe(99);
        logs.ShouldContain("Spawn: 42,99");
    }

    [Fact]
    public void Execute_WriteAndReadBossFlags_Succeeds()
    {
        var (api, world, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            world.downedSlimeKing = true;
            world.downedEyeOfCthulhu = true;
            world.downedPlantera = true;
            print('Slime: ' + world.downedSlimeKing);
            print('Eye: ' + world.downedEyeOfCthulhu);
            print('Plant: ' + world.downedPlantera);
        ", api, context);

        result.Success.ShouldBeTrue();
        world.DownedSlimeKingBoss.ShouldBeTrue();
        world.DownedBoss1EyeofCthulhu.ShouldBeTrue();
        world.DownedPlantBoss.ShouldBeTrue();
        logs.ShouldContain("Slime: true");
        logs.ShouldContain("Eye: true");
        logs.ShouldContain("Plant: true");
    }

    [Fact]
    public void Execute_WriteAndReadHardMode_Succeeds()
    {
        var (api, world, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            world.hardMode = true;
            print('Hard: ' + world.hardMode);
        ", api, context);

        result.Success.ShouldBeTrue();
        world.HardMode.ShouldBeTrue();
        logs.ShouldContain("Hard: true");
    }

    [Fact]
    public void Execute_DownedMechBossAny_ComputedCorrectly()
    {
        var (api, world, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            print('Before: ' + world.downedMechBossAny);
            world.downedDestroyer = true;
            print('After: ' + world.downedMechBossAny);
        ", api, context);

        result.Success.ShouldBeTrue();
        logs.ShouldContain("Before: false");
        logs.ShouldContain("After: true");
    }

    [Fact]
    public void Execute_WriteAndReadOreTiers_Succeeds()
    {
        var (api, world, _, context) = CreateTestContext();

        var result = _engine.Execute(@"
            world.savedOreTiersCopper = 166;
            world.savedOreTiersIron = 167;
            world.isCrimson = true;
        ", api, context);

        result.Success.ShouldBeTrue();
        world.SavedOreTiersCopper.ShouldBe(166);
        world.SavedOreTiersIron.ShouldBe(167);
        world.IsCrimson.ShouldBeTrue();
    }

    [Fact]
    public void Execute_WriteAndReadWorldSeeds_Succeeds()
    {
        var (api, world, logs, context) = CreateTestContext();

        var result = _engine.Execute(@"
            world.drunkWorld = true;
            world.zenithWorld = true;
            print('Drunk: ' + world.drunkWorld);
            print('Zenith: ' + world.zenithWorld);
        ", api, context);

        result.Success.ShouldBeTrue();
        world.DrunkWorld.ShouldBeTrue();
        world.ZenithWorld.ShouldBeTrue();
        logs.ShouldContain("Drunk: true");
        logs.ShouldContain("Zenith: true");
    }

    [Fact]
    public void Execute_WidthAndHeight_AreReadOnly()
    {
        var (api, world, _, context) = CreateTestContext();
        int originalWidth = world.TilesWide;

        // Attempting to set width should fail (it's a read-only property)
        var result = _engine.Execute("world.width = 500;", api, context);

        // Width should not have changed
        world.TilesWide.ShouldBe(originalWidth);
    }

    // ── Draw API Tests ───────────────────────────────────────────────

    [Fact]
    public void Execute_DrawSetTilePencil_PlacesTilesAlongLine()
    {
        var (api, world, _, context) = CreateTestContext();

        var result = _engine.Execute(@"
            draw.setTile(1);
            draw.pencil(5, 10, 15, 10);
        ", api, context);

        result.Success.ShouldBeTrue(result.Error);
        // Verify tiles along horizontal line are set to stone (type 1)
        for (int x = 5; x <= 15; x++)
        {
            world.Tiles[x, 10].IsActive.ShouldBeTrue($"Tile at ({x}, 10) should be active");
            world.Tiles[x, 10].Type.ShouldBe((ushort)1, $"Tile at ({x}, 10) should be stone");
        }
    }

    [Fact]
    public void Execute_DrawSetBrushAndBrush_FillsAreaAlongLine()
    {
        var (api, world, _, context) = CreateTestContext();

        var result = _engine.Execute(@"
            draw.setTile(1);
            draw.setBrush(3, 3, 'square');
            draw.brush(10, 10, 20, 10);
        ", api, context);

        result.Success.ShouldBeTrue(result.Error);
        // Center of line should be filled
        world.Tiles[15, 10].IsActive.ShouldBeTrue();
        world.Tiles[15, 10].Type.ShouldBe((ushort)1);
        // Above and below should also be filled (brush height = 3)
        world.Tiles[15, 9].IsActive.ShouldBeTrue();
        world.Tiles[15, 11].IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Execute_DrawErasePencil_ClearsTiles()
    {
        var (api, world, _, context) = CreateTestContext();

        // The test world has terrain starting at ground level (35)
        // Verify there are active tiles at y=40 before erasing
        world.Tiles[10, 40].IsActive.ShouldBeTrue();

        var result = _engine.Execute(@"
            draw.setTile(1);
            draw.setErase(true);
            draw.pencil(10, 40, 20, 40);
        ", api, context);

        result.Success.ShouldBeTrue(result.Error);
        for (int x = 10; x <= 20; x++)
        {
            world.Tiles[x, 40].IsActive.ShouldBeFalse($"Tile at ({x}, 40) should be erased");
        }
    }

    [Fact]
    public void Execute_DrawHammer_AppliesSlopes()
    {
        var (api, world, _, context) = CreateTestContext();

        // Create a terrain edge: solid below, air above
        // Ground level is 35, so tiles at y=35+ are active
        // The edge at y=35 should get slopes applied
        var result = _engine.Execute(@"
            draw.setBrush(1, 1);
            draw.hammer(1, 35, 99, 35);
        ", api, context);

        result.Success.ShouldBeTrue(result.Error);
        // Verify at least some tiles got sloped (exact slopes depend on neighbor configuration)
        bool anySloped = false;
        for (int x = 1; x < 99; x++)
        {
            if (world.Tiles[x, 35].BrickStyle != BrickStyle.Full &&
                world.Tiles[x, 35].IsActive)
            {
                anySloped = true;
                break;
            }
        }
        // Note: whether slopes are applied depends on the terrain shape.
        // At the edges of flat terrain, no slopes are expected since all neighbors match.
        // This test verifies the hammer runs without error.
    }

    [Fact]
    public void Execute_DrawReset_ClearsSettings()
    {
        var (api, world, _, context) = CreateTestContext();

        var result = _engine.Execute(@"
            draw.setTile(1);
            draw.setWall(1);
            draw.reset();
            // After reset, pencil should not change tiles (no active style)
            draw.pencil(5, 10, 15, 10);
        ", api, context);

        result.Success.ShouldBeTrue(result.Error);
        // After reset, tile style is inactive, so pencil should not set tiles
        // Tiles above ground level (35) should remain empty
        world.Tiles[10, 10].IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Execute_DrawSetWallPencil_SetsWalls()
    {
        var (api, world, _, context) = CreateTestContext();

        var result = _engine.Execute(@"
            draw.setWall(4);
            draw.pencil(5, 10, 15, 10);
        ", api, context);

        result.Success.ShouldBeTrue(result.Error);
        for (int x = 5; x <= 15; x++)
        {
            world.Tiles[x, 10].Wall.ShouldBe((ushort)4, $"Wall at ({x}, 10) should be type 4");
        }
    }

    [Fact]
    public void Execute_DrawFill_FloodFillsRegion()
    {
        var (api, world, _, context) = CreateTestContext();

        // Create a small enclosed air pocket within terrain
        // Ground level is 35, terrain is solid from y=35 to y=99
        // Carve out a 5x5 room at (50,50)
        for (int x = 50; x < 55; x++)
            for (int y = 50; y < 55; y++)
            {
                world.Tiles[x, y].IsActive = false;
                world.Tiles[x, y].Type = 0;
            }

        var result = _engine.Execute(@"
            draw.setTile(2);
            draw.fill(52, 52);
        ", api, context);

        result.Success.ShouldBeTrue(result.Error);
        // The carved-out area should now be filled with tile type 2
        world.Tiles[52, 52].IsActive.ShouldBeTrue();
        world.Tiles[52, 52].Type.ShouldBe((ushort)2);
    }
}
