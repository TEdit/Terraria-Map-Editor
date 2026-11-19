using TEdit.Editor;
using TEdit.Geometry;
using TEdit.Scripting.Api;
using TEdit.Tests.Scripting;
using Xunit;

namespace TEdit.Tests.Mcp;

[Collection("Production editor")]
public class BoundedDrawingTests
{
    [Theory]
    [InlineData("line", 2, 2, 0, 0, false, "0,0 1,1 2,2")]
    [InlineData("rectangle", 0, 0, 2, 2, false, "0,0 1,0 2,0 0,1 2,1 0,2 1,2 2,2")]
    [InlineData("ellipse", 0, 0, 3, 3, false, "1,0 2,0 0,1 3,1 0,2 3,2 1,3 2,3")]
    [InlineData("ellipse", 98, 97, 98, 99, true, "98,97 98,98 98,99")]
    [InlineData("fill", 98, 98, 99, 99, true, "98,98 99,98 98,99 99,99")]
    public void InclusiveGeometryAtWorldEdges(string shape, int x, int y, int endX, int endY, bool filled, string expected)
    {
        var world = ExecutionWorldFixture.Create();
        var spec = new DrawingSpec(shape, x, y, endX, endY, filled, 1, "wall", 1, false, -1);
        Assert.Null(spec.Validate(world, null, out _));
        Assert.Equal(expected.Split(' '), spec.Points().OrderBy(p => p.Y).ThenBy(p => p.X).Select(p => $"{p.X},{p.Y}"));
    }

    [Fact]
    public void EvenBrushIncludesThePositiveEdgeAndHonorsTheWorkBudget()
    {
        var world = ExecutionWorldFixture.Create();
        var line = new DrawingSpec("line", 98, 98, 98, 98, false, 2, "tile", 1, false, -1);
        Assert.Null(line.Validate(world, null, out _));
        Assert.Equal(new[] { (98,98), (99,98), (98,99), (99,99) }, line.Points().Select(p => (p.X,p.Y)));
        Assert.Equal("invalid_bounds", (line with { EndX = 99 }).Validate(world, null, out _));
        Assert.Equal("drawing_limit", (line with { X = 15, Y = 15, EndX = 83, EndY = 83, Thickness = 32 }).Validate(world, null, out _));
    }

    [Theory]
    [InlineData("line")]
    [InlineData("rectangle")]
    [InlineData("ellipse")]
    [InlineData("fill")]
    public void MaskAndSelectionAreExplicitAndInvalidRequestsDoNotWrite(string shape)
    {
        var world = ExecutionWorldFixture.Create();
        var spec = new DrawingSpec(shape, 1, 1, 3, 3, true, 1, "wall", 1, true, 2);
        var selection = new RectangleInt32(2, 2, 1, 1);
        var undo = new NoOpUndoManager();
        world.Tiles[2, 2].Wall = 2;
        world.Tiles[3, 3].Wall = 2;
        Assert.Null(spec.Validate(world, selection, out _));
        Assert.Equal("invalid_bounds", (spec with { EndY = world.TilesHigh }).Validate(world, selection, out _));
        if (shape != "line") Assert.Equal("invalid_bounds", (spec with { EndX = 0 }).Validate(world, selection, out _));
        Assert.Equal("no_selection", spec.Validate(world, null, out _));
        Assert.Equal("invalid_bounds", (spec with { X = -1 }).Validate(world, selection, out _));
        Assert.Equal("invalid_material", (spec with { Material = -1 }).Validate(world, selection, out _));
        Assert.Equal("drawing_limit", (spec with { Thickness = 33 }).Validate(world, selection, out _));
        Assert.Equal("unsupported_material", (spec with { Material = int.MaxValue }).Validate(world, selection, out _));
        Assert.Single(spec.Points(), p => spec.Paint(world, undo, selection, p));
        Assert.Equal([(2, 2)], undo.SavedTiles);
        Assert.DoesNotContain(spec.Points(), p => spec.Paint(world, undo, selection, p));
        Assert.Equal(2, world.Tiles[3, 3].Wall);
    }
}
