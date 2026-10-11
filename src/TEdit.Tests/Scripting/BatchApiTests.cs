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
    private readonly Selection _selection;

    public BatchApiTests()
    {
        _world = TestWorldFactory.CreateWorldWithTerrain();
        _undo = new NoOpUndoManager();
        var selection = new Selection();
        _selection = selection;
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
    public void ForEachInSelection_NoSelection_DoesNothing()
    {
        int count = 0;
        _api.ForEachInSelection((x, y) => count++);

        count.ShouldBe(0);
    }

    [Fact]
    public void ForEachInSelection_Rectangle_VisitsEveryTile()
    {
        _selection.SetRectangle(new Geometry.Vector2Int32(10, 10), new Geometry.Vector2Int32(14, 12));
        var visited = new List<(int, int)>();

        _api.ForEachInSelection((x, y) => visited.Add((x, y)));

        visited.Count.ShouldBe(15);
        visited.ShouldContain((10, 10));
        visited.ShouldContain((14, 12));
    }

    [Fact]
    public void ForEachInSelection_FreeForm_VisitsOnlySelectedTiles()
    {
        _selection.SetTiles(new[] { new Geometry.Vector2Int32(10, 10), new Geometry.Vector2Int32(20, 15) }, selected: true);
        var visited = new List<(int, int)>();

        _api.ForEachInSelection((x, y) => visited.Add((x, y)));

        visited.ShouldBe(new[] { (10, 10), (20, 15) }, ignoreOrder: true);
    }

    [Fact]
    public void ReplaceTileInSelection_Rectangle_ReplacesInsideOnly()
    {
        FillStone(5, 5, 10, 10);
        _selection.SetRectangle(new Geometry.Vector2Int32(5, 5), new Geometry.Vector2Int32(9, 9));

        int replaced = _api.ReplaceTileInSelection(1, 56);

        replaced.ShouldBe(25);
        _world.Tiles[9, 9].Type.ShouldBe((ushort)56);
        _world.Tiles[10, 10].Type.ShouldBe((ushort)1);
    }

    [Fact]
    public void ReplaceTileInSelection_FreeForm_SkipsUnselectedTilesInsideBounds()
    {
        FillStone(5, 5, 10, 10);
        _selection.SetTiles(new[] { new Geometry.Vector2Int32(5, 5), new Geometry.Vector2Int32(9, 9) }, selected: true);

        int replaced = _api.ReplaceTileInSelection(1, 56);

        replaced.ShouldBe(2);
        _world.Tiles[5, 5].Type.ShouldBe((ushort)56);
        _world.Tiles[9, 9].Type.ShouldBe((ushort)56);
        _world.Tiles[7, 7].Type.ShouldBe((ushort)1);
    }

    [Fact]
    public void SelectionApi_Contains_FollowsFreeFormShape()
    {
        var api = new SelectionApi(_selection);
        _selection.SetTiles(new[] { new Geometry.Vector2Int32(3, 3), new Geometry.Vector2Int32(6, 6) }, selected: true);

        api.IsActive.ShouldBeTrue();
        api.Contains(3, 3).ShouldBeTrue();
        api.Contains(4, 4).ShouldBeFalse();
        api.X.ShouldBe(3);
        api.Width.ShouldBe(4);
    }

    [Fact]
    public void SelectionApi_Set_ReplacesFreeFormWithRectangle()
    {
        var api = new SelectionApi(_selection);
        _selection.SetTiles(new[] { new Geometry.Vector2Int32(3, 3), new Geometry.Vector2Int32(6, 6) }, selected: true);

        api.Set(3, 3, 3, 3);

        api.Contains(4, 4).ShouldBeTrue();
        _selection.HasMask.ShouldBeFalse();
    }

    private void FillStone(int x0, int y0, int width, int height)
    {
        for (int x = x0; x < x0 + width; x++)
            for (int y = y0; y < y0 + height; y++)
                _world.Tiles[x, y] = new Tile { IsActive = true, Type = 1 };
    }

    [Fact]
    public void ReplaceTile_SavesUndoForEachModifiedTile()
    {
        _undo.SavedTiles.Clear();
        int replaced = _api.ReplaceTile(1, 56);

        _undo.SavedTiles.Count.ShouldBe(replaced);
    }
}
