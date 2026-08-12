using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace TEdit.Terraria.Tests;

public sealed class WorldEdgeBrickStyleTests
{
    private readonly ITestOutputHelper _output;

    public WorldEdgeBrickStyleTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.4.4.wld")]
    [InlineData(".\\WorldFiles\\v1.4.5.5.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    public void Report_NonFullTileDistanceFromWorldEdges(string fileName)
    {
        var (world, error) = World.LoadWorld(fileName);
        Assert.Null(error);
        Assert.NotNull(world);

        var nearestByStyle = new Dictionary<BrickStyle, EdgeDistances>();
        long nonFullCount = 0;

        for (int x = 0; x < world.TilesWide; x++)
        {
            for (int y = 0; y < world.TilesHigh; y++)
            {
                ref readonly var tile = ref world.Tiles[x, y];
                if (!tile.IsActive || tile.BrickStyle == BrickStyle.Full)
                    continue;

                nonFullCount++;
                if (!nearestByStyle.TryGetValue(tile.BrickStyle, out var nearest))
                    nearest = EdgeDistances.Empty;

                nearestByStyle[tile.BrickStyle] = nearest.Include(
                    x,
                    world.TilesWide - 1 - x,
                    y,
                    world.TilesHigh - 1 - y);
            }
        }

        _output.WriteLine($"{Path.GetFileName(fileName)}: {world.TilesWide}x{world.TilesHigh}, non-full={nonFullCount:N0}");
        foreach (var (style, nearest) in nearestByStyle.OrderBy(pair => pair.Key))
        {
            _output.WriteLine(
                $"  {style}: left={nearest.Left}, right={nearest.Right}, top={nearest.Top}, bottom={nearest.Bottom}");

            Assert.True(nearest.Left >= World.SafeBorderTileCount);
            Assert.True(nearest.Right >= World.SafeBorderTileCount);
            Assert.True(nearest.Top >= World.SafeBorderTileCount);
            Assert.True(nearest.Bottom >= World.SafeBorderTileCount);
        }
    }

    [Fact]
    public void Validate_RepairsOnlyOuterTwentyTiles()
    {
        var world = new World(50, 50, "Border test")
        {
            Tiles = new Tile[50, 50],
        };

        SetShape(world, 0, 25, BrickStyle.HalfBrick);
        SetShape(world, 19, 25, BrickStyle.SlopeTopRight);
        SetShape(world, 20, 25, BrickStyle.SlopeTopLeft);
        SetShape(world, 29, 25, BrickStyle.SlopeBottomRight);
        SetShape(world, 30, 25, BrickStyle.SlopeBottomLeft);
        SetShape(world, 25, 0, BrickStyle.HalfBrick);
        SetShape(world, 25, 19, BrickStyle.SlopeTopRight);
        SetShape(world, 25, 20, BrickStyle.SlopeTopLeft);
        SetShape(world, 25, 29, BrickStyle.SlopeBottomRight);
        SetShape(world, 25, 30, BrickStyle.SlopeBottomLeft);

        world.Validate();

        Assert.Equal(BrickStyle.Full, world.Tiles[0, 25].BrickStyle);
        Assert.Equal(BrickStyle.Full, world.Tiles[19, 25].BrickStyle);
        Assert.Equal(BrickStyle.SlopeTopLeft, world.Tiles[20, 25].BrickStyle);
        Assert.Equal(BrickStyle.SlopeBottomRight, world.Tiles[29, 25].BrickStyle);
        Assert.Equal(BrickStyle.Full, world.Tiles[30, 25].BrickStyle);
        Assert.Equal(BrickStyle.Full, world.Tiles[25, 0].BrickStyle);
        Assert.Equal(BrickStyle.Full, world.Tiles[25, 19].BrickStyle);
        Assert.Equal(BrickStyle.SlopeTopLeft, world.Tiles[25, 20].BrickStyle);
        Assert.Equal(BrickStyle.SlopeBottomRight, world.Tiles[25, 29].BrickStyle);
        Assert.Equal(BrickStyle.Full, world.Tiles[25, 30].BrickStyle);
    }

    private static void SetShape(World world, int x, int y, BrickStyle style)
    {
        world.Tiles[x, y] = new Tile
        {
            IsActive = true,
            Type = (ushort)TileType.StoneBlock,
            BrickStyle = style,
        };
    }

    private readonly record struct EdgeDistances(int Left, int Right, int Top, int Bottom)
    {
        public static EdgeDistances Empty { get; } = new(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

        public EdgeDistances Include(int left, int right, int top, int bottom) => new(
            Math.Min(Left, left),
            Math.Min(Right, right),
            Math.Min(Top, top),
            Math.Min(Bottom, bottom));
    }
}
