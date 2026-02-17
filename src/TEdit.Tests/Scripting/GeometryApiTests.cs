using Shouldly;
using TEdit.Scripting.Api;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class GeometryApiTests
{
    private readonly World _world;
    private readonly NoOpUndoManager _undo;
    private readonly GeometryApi _api;

    public GeometryApiTests()
    {
        _world = TestWorldFactory.CreateSmallWorld();
        _undo = new NoOpUndoManager();
        _api = new GeometryApi(_world, _undo);
    }

    [Fact]
    public void Line_ReturnsCoordinates()
    {
        var coords = _api.Line(0, 0, 10, 0);
        coords.Count.ShouldBeGreaterThan(0);
        coords[0]["x"].ShouldBe(0);
        coords[0]["y"].ShouldBe(0);
    }

    [Fact]
    public void Rect_ReturnsPerimeterCoordinates()
    {
        var coords = _api.Rect(5, 5, 10, 10);
        coords.Count.ShouldBeGreaterThan(0);
        // Should contain edge points
        coords.ShouldContain(c => c["x"] == 5 && c["y"] == 5);
    }

    [Fact]
    public void FillRect_ReturnsFillCoordinates()
    {
        var coords = _api.FillRect(5, 5, 3, 3);
        coords.Count.ShouldBe(9); // 3x3 = 9 tiles
    }

    [Fact]
    public void SetTiles_FillsAreaWithTileType()
    {
        _api.SetTiles(1, 10, 10, 3, 3); // Stone block in 3x3 area

        for (int x = 10; x < 13; x++)
            for (int y = 10; y < 13; y++)
            {
                _world.Tiles[x, y].IsActive.ShouldBeTrue();
                _world.Tiles[x, y].Type.ShouldBe((ushort)1);
            }
    }

    [Fact]
    public void SetWalls_FillsAreaWithWallType()
    {
        _api.SetWalls(1, 10, 10, 3, 3); // Stone wall in 3x3 area

        for (int x = 10; x < 13; x++)
            for (int y = 10; y < 13; y++)
                _world.Tiles[x, y].Wall.ShouldBe((ushort)1);
    }

    [Fact]
    public void ClearTiles_ResetsArea()
    {
        // First set some tiles
        _api.SetTiles(1, 10, 10, 3, 3);

        // Then clear them
        _api.ClearTiles(10, 10, 3, 3);

        for (int x = 10; x < 13; x++)
            for (int y = 10; y < 13; y++)
                _world.Tiles[x, y].IsActive.ShouldBeFalse();
    }

    [Fact]
    public void SetTiles_SavesUndo()
    {
        _undo.SavedTiles.Clear();
        _api.SetTiles(1, 10, 10, 3, 3);

        _undo.SavedTiles.Count.ShouldBe(9);
    }
}
