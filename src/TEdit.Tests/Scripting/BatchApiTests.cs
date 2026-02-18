using Shouldly;
using TEdit.Editor;
using TEdit.Scripting.Api;
using TEdit.Scripting.Engine;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class BatchApiTests
{
    private readonly World _world;
    private readonly NoOpUndoManager _undo;
    private readonly BatchApi _api;
    private readonly ScriptExecutionContext _context;

    public BatchApiTests()
    {
        _world = TestWorldFactory.CreateWorldWithTerrain();
        _undo = new NoOpUndoManager();
        var selection = new Selection();
        _context = new ScriptExecutionContext
        {
            CancellationToken = CancellationToken.None
        };
        _api = new BatchApi(_world, selection, _undo, _context);
    }

    [Fact]
    public void ReplaceTile_ReplacesMatchingTiles()
    {
        // Count stone tiles before
        int stoneCount = 0;
        for (int x = 0; x < _world.TilesWide; x++)
            for (int y = 0; y < _world.TilesHigh; y++)
                if (_world.Tiles[x, y].IsActive && _world.Tiles[x, y].Type == 1)
                    stoneCount++;

        stoneCount.ShouldBeGreaterThan(0);

        // Replace stone (1) with obsidian (56)
        int replaced = _api.ReplaceTile(1, 56);

        replaced.ShouldBe(stoneCount);

        // Verify no stone tiles remain
        for (int x = 0; x < _world.TilesWide; x++)
            for (int y = 0; y < _world.TilesHigh; y++)
                if (_world.Tiles[x, y].IsActive)
                    _world.Tiles[x, y].Type.ShouldNotBe((ushort)1);
    }

    [Fact]
    public void ReplaceWall_ReplacesMatchingWalls()
    {
        // Replace dirt wall (2) with stone wall (1)
        int replaced = _api.ReplaceWall(2, 1);

        replaced.ShouldBeGreaterThan(0);

        // Verify no dirt walls remain
        for (int x = 0; x < _world.TilesWide; x++)
            for (int y = 0; y < _world.TilesHigh; y++)
                _world.Tiles[x, y].Wall.ShouldNotBe((ushort)2);
    }

    [Fact]
    public void FindTiles_FindsMatchingTiles()
    {
        var results = _api.FindTiles((x, y) =>
            _world.Tiles[x, y].IsActive && _world.Tiles[x, y].Type == 1);

        results.Count.ShouldBeGreaterThan(0);
        foreach (var r in results)
        {
            _world.Tiles[r["x"], r["y"]].Type.ShouldBe((ushort)1);
        }
    }

    [Fact]
    public void ForEachTile_IteratesAllTiles()
    {
        int count = 0;
        _api.ForEachTile((x, y) => count++);

        count.ShouldBe(_world.TilesWide * _world.TilesHigh);
    }

    [Fact]
    public void ReplaceTile_SavesUndoForEachModifiedTile()
    {
        _undo.SavedTiles.Clear();
        int replaced = _api.ReplaceTile(1, 56);

        _undo.SavedTiles.Count.ShouldBe(replaced);
    }
}
