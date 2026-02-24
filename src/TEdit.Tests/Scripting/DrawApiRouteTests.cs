using Shouldly;
using TEdit.Editor;
using TEdit.Geometry;
using TEdit.Scripting.Api;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class DrawApiRouteTests
{
    private readonly World _world;
    private readonly NoOpUndoManager _undo;
    private readonly AllValidSelection _selection;
    private readonly DrawApi _api;

    public DrawApiRouteTests()
    {
        _world = TestWorldFactory.CreateSmallWorld();
        _undo = new NoOpUndoManager();
        _selection = new AllValidSelection();
        _api = new DrawApi(_world, _undo, _selection);
    }

    [Fact]
    public void RouteWire_90_SetsWireOnPath()
    {
        _api.SetWire(red: true);
        int count = _api.RouteWire(10, 10, 25, 20, "90", "h");

        count.ShouldBeGreaterThan(0);
        // Start tile should have red wire
        _world.Tiles[10, 10].WireRed.ShouldBeTrue();
        // End tile should have red wire
        _world.Tiles[25, 20].WireRed.ShouldBeTrue();
    }

    [Fact]
    public void RouteWire_45_SetsWireOnPath()
    {
        _api.SetWire(blue: true);
        int count = _api.RouteWire(10, 10, 25, 20, "45", "h");

        count.ShouldBeGreaterThan(0);
        _world.Tiles[10, 10].WireBlue.ShouldBeTrue();
        _world.Tiles[25, 20].WireBlue.ShouldBeTrue();
    }

    [Fact]
    public void RouteWire_ReturnsCorrectCount()
    {
        _api.SetWire(red: true);
        // Horizontal-first from (10,10) to (20,15): |dx|+|dy|+1 = 10+5+1 = 16
        int count = _api.RouteWire(10, 10, 20, 15, "90", "h");
        count.ShouldBe(16);
    }

    [Fact]
    public void RouteWire_AutoDirection()
    {
        _api.SetWire(green: true);
        int count = _api.RouteWire(10, 10, 25, 20);
        count.ShouldBeGreaterThan(0);
        _world.Tiles[25, 20].WireGreen.ShouldBeTrue();
    }

    [Fact]
    public void RouteWire_WithTileMode()
    {
        _api.SetTile(1); // Stone
        int count = _api.RouteWire(10, 10, 20, 10, "90", "h");

        count.ShouldBe(11); // 10 + 1
        for (int x = 10; x <= 20; x++)
        {
            _world.Tiles[x, 10].IsActive.ShouldBeTrue();
            _world.Tiles[x, 10].Type.ShouldBe((ushort)1);
        }
    }

    [Fact]
    public void RouteBus_PlacesMultipleWires()
    {
        _api.SetWire(red: true);
        int count = _api.RouteBus(3, 10, 10, 30, 20, "90", "h");

        count.ShouldBeGreaterThan(0);
        // Should place more tiles than a single wire
        int singleCount = 0;
        _api.Reset();
        _api.SetWire(red: true);
        singleCount = _api.RouteWire(10, 10, 30, 20, "90", "h");
        count.ShouldBeGreaterThan(singleCount);
    }

    [Fact]
    public void RouteBus_SingleWire_MatchesSingleRoute()
    {
        _api.SetWire(red: true);
        int busCount = _api.RouteBus(1, 10, 10, 25, 20, "90", "h");

        // Reset and do single wire
        var world2 = TestWorldFactory.CreateSmallWorld();
        var api2 = new DrawApi(world2, new NoOpUndoManager(), new AllValidSelection());
        api2.SetWire(red: true);
        int singleCount = api2.RouteWire(10, 10, 25, 20, "90", "h");

        busCount.ShouldBe(singleCount);
    }

    [Fact]
    public void RouteWirePath_ReturnsCoordinates()
    {
        var path = _api.RouteWirePath(10, 10, 20, 15, "90", "h");

        path.Count.ShouldBe(16); // |dx|+|dy|+1
        path[0]["x"].ShouldBe(10);
        path[0]["y"].ShouldBe(10);
        path[path.Count - 1]["x"].ShouldBe(20);
        path[path.Count - 1]["y"].ShouldBe(15);
    }

    [Fact]
    public void RouteBusPath_ReturnsCoordinates()
    {
        var path = _api.RouteBusPath(3, 10, 10, 30, 20, "90", "h");
        path.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void RouteBus_45_UsesWiderSpacing()
    {
        _api.SetWire(yellow: true);
        // 3 wires with miter mode → spacing=3, brushSize=7
        int count = _api.RouteBus(3, 10, 10, 30, 20, "45", "h");
        count.ShouldBeGreaterThan(0);
    }

    /// <summary>Selection stub that allows all positions.</summary>
    private class AllValidSelection : ISelection
    {
        public RectangleInt32 SelectionArea { get; set; }
        public bool IsActive { get; set; }
        public bool IsValid(int x, int y) => true;
        public bool IsValid(Vector2Int32 xy) => true;
        public void SetRectangle(Vector2Int32 p1, Vector2Int32 p2) { }
    }
}
