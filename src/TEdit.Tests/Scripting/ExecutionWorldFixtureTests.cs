using System.IO;
using Shouldly;
using TEdit.Terraria;
using Xunit;
using Xunit.Abstractions;
using System.Diagnostics;
using TEdit.Editor;
using TEdit.Scripting.Api;
using TEdit.Scripting.Engine;
using TEdit.Scripting.Execution;

namespace TEdit.Tests.Scripting;

public class ExecutionWorldFixtureTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(false, 1000, 500)]
    [InlineData(true, 1000, 500)]
    [InlineData(false, 4200, 1200)]
    [InlineData(true, 4200, 1200)]
    public void SyntheticRegionsSupportBoundedDrawingAtWorldEdges(bool lua, int width, int height)
    {
        var elapsed = Stopwatch.StartNew();
        var world = ExecutionWorldFixture.Create(width, height);
        var undo = new NoOpUndoManager();
        var context = new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.LocalSidebar };
        using var api = new ScriptApi(world, undo, new Selection(), context);
        IScriptEngine engine = lua ? new LuaScriptEngine() : new JintScriptEngine();
        var result = engine.Execute($"draw.setTile(2); draw.pencil(-3,0,8,0); draw.pencil({width - 4},{height - 1},{width + 3},{height - 1})", api, context);
        result.Success.ShouldBeTrue(result.Error);
        world.Tiles[0, 0].Type.ShouldBe((ushort)2);
        world.Tiles[width - 1, height - 1].Type.ShouldBe((ushort)2);
        undo.SavedTiles.ShouldAllBe(p => p.X >= 0 && p.Y >= 0 && p.X < width && p.Y < height);
        output.WriteLine($"{engine.Name}, {width}x{height}: fixture + clipped edge drawing {elapsed.ElapsedMilliseconds} ms");
        elapsed.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(30));
    }

    [Theory]
    [InlineData(100, 100)]
    [InlineData(1000, 500)]
    public void GeneratedFixturesAreDeterministicAndIndependent(int width, int height)
    {
        var first = ExecutionWorldFixture.Create(width, height);
        var second = ExecutionWorldFixture.Create(width, height);
        first.WorldGUID.ShouldBe(second.WorldGUID);
        first.WorldId.ShouldBe(second.WorldId);
        first.CreationTime.ShouldBe(second.CreationTime);
        first.Tiles.ShouldNotBeSameAs(second.Tiles);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                first.Tiles[x, y].ShouldBe(second.Tiles[x, y]);
    }

    [Fact]
    public async Task GeneratedWorldRoundTripsWithoutExternalFixtures()
    {
        var directory = Path.Combine(Path.GetTempPath(), "tedit-execution-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "fixture.wld");
        try
        {
            var original = ExecutionWorldFixture.Create();
            await World.SaveAsync(original, path, incrementRevision: false);
            File.Exists(path).ShouldBeTrue();
            var (loaded, error) = World.LoadWorld(path);
            error.ShouldBeNull();
            loaded.ShouldNotBeNull();
            loaded.WorldGUID.ShouldBe(original.WorldGUID);
            loaded.Title.ShouldBe(original.Title);
            loaded.TilesWide.ShouldBe(original.TilesWide);
            loaded.TilesHigh.ShouldBe(original.TilesHigh);
            loaded.Tiles[0, 0].Type.ShouldBe((ushort)1);
            loaded.Tiles[99, 99].Type.ShouldBe((ushort)1);
            loaded.Tiles[50, 50].Type.ShouldBe(original.Tiles[50, 50].Type);
        }
        finally
        {
            // Only this test's freshly created directory is removed.
            Directory.Delete(directory, recursive: true);
        }
    }
}
