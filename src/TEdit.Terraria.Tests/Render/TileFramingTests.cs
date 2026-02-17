using Shouldly;
using TEdit.Geometry;
using TEdit.Terraria.Render;

namespace TEdit.Terraria.Tests.Render;

[Collection("SharedState")]
public class TileFramingTests
{
    static TileFramingTests()
    {
        WorldConfiguration.Initialize();
    }

    private static World CreateSmallWorld(int width = 20, int height = 20)
    {
        var world = new World(height, width, "Test");
        world.Tiles = new Tile[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                world.Tiles[x, y] = new Tile();
        return world;
    }

    private static void SetTile(World world, int x, int y, ushort tileType, BrickStyle style = BrickStyle.Full)
    {
        world.Tiles[x, y].IsActive = true;
        world.Tiles[x, y].Type = tileType;
        world.Tiles[x, y].BrickStyle = style;
    }

    // Gemspark Amber = 255 (first gemspark type)
    private const ushort GemsparkAmber = 255;

    #region SelfFrame8Way Basic Tests

    [Fact]
    public void SelfFrame8Way_NoNeighbors_ReturnsIsolatedFrame()
    {
        var world = CreateSmallWorld();
        SetTile(world, 5, 5, GemsparkAmber);

        var uv = TileFraming.CalculateSelfFrame8Way(world, 5, 5);

        // Index 0 (no neighbors): grid positions (9,3), (10,3), (11,3)
        uv.Y.ShouldBe(3);
        uv.X.ShouldBeOneOf(9, 10, 11);
    }

    [Fact]
    public void SelfFrame8Way_AllNeighbors_ReturnsCenterFrame()
    {
        var world = CreateSmallWorld();
        // Fill a 3x3 area with same gemspark
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                SetTile(world, 5 + dx, 5 + dy, GemsparkAmber);

        var uv = TileFraming.CalculateSelfFrame8Way(world, 5, 5);

        // Index 255 (all neighbors): grid positions (1,1), (2,1), (3,1)
        uv.Y.ShouldBe(1);
        uv.X.ShouldBeOneOf(1, 2, 3);
    }

    [Fact]
    public void SelfFrame8Way_UpOnly_ReturnsCorrectFrame()
    {
        var world = CreateSmallWorld();
        SetTile(world, 5, 5, GemsparkAmber);
        SetTile(world, 5, 4, GemsparkAmber); // Up

        var uv = TileFraming.CalculateSelfFrame8Way(world, 5, 5);

        // Index 1 (Up only): grid positions (6,3), (7,3), (8,3)
        uv.Y.ShouldBe(3);
        uv.X.ShouldBeOneOf(6, 7, 8);
    }

    [Fact]
    public void SelfFrame8Way_DownOnly_ReturnsCorrectFrame()
    {
        var world = CreateSmallWorld();
        SetTile(world, 5, 5, GemsparkAmber);
        SetTile(world, 5, 6, GemsparkAmber); // Down

        var uv = TileFraming.CalculateSelfFrame8Way(world, 5, 5);

        // Index 8 (Down only): grid positions (6,0), (7,0), (8,0)
        uv.Y.ShouldBe(0);
        uv.X.ShouldBeOneOf(6, 7, 8);
    }

    [Fact]
    public void SelfFrame8Way_LeftRight_ReturnsCorrectFrame()
    {
        var world = CreateSmallWorld();
        SetTile(world, 5, 5, GemsparkAmber);
        SetTile(world, 4, 5, GemsparkAmber); // Left
        SetTile(world, 6, 5, GemsparkAmber); // Right

        var uv = TileFraming.CalculateSelfFrame8Way(world, 5, 5);

        // Index 6 (L+R): grid positions (6,4), (7,4), (8,4)
        uv.Y.ShouldBe(4);
        uv.X.ShouldBeOneOf(6, 7, 8);
    }

    #endregion

    #region Corner Bit Gating

    [Fact]
    public void SelfFrame8Way_CornerBits_RequireAdjacentCardinals()
    {
        var world = CreateSmallWorld();
        SetTile(world, 5, 5, GemsparkAmber);
        // Place a corner neighbor without the adjacent cardinals
        SetTile(world, 4, 4, GemsparkAmber); // Upper-left corner only

        var uv = TileFraming.CalculateSelfFrame8Way(world, 5, 5);

        // Without Up and Left cardinals connected, the corner bit should NOT be set
        // Should be index 0 (no neighbors)
        uv.Y.ShouldBe(3);
        uv.X.ShouldBeOneOf(9, 10, 11);
    }

    [Fact]
    public void SelfFrame8Way_CornerBits_SetWhenCardinalsPresent()
    {
        var world = CreateSmallWorld();
        SetTile(world, 5, 5, GemsparkAmber);
        SetTile(world, 5, 4, GemsparkAmber); // Up
        SetTile(world, 4, 5, GemsparkAmber); // Left
        SetTile(world, 4, 4, GemsparkAmber); // UpperLeft corner

        var uv = TileFraming.CalculateSelfFrame8Way(world, 5, 5);

        // Index = 1(Up) | 2(Left) | 16(UpLeft) = 19
        // Grid positions (1,4), (3,4), (5,4)
        uv.Y.ShouldBe(4);
        uv.X.ShouldBeOneOf(1, 3, 5);
    }

    #endregion

    #region Slope Awareness

    [Fact]
    public void SelfFrame8Way_SlopeBlocksFaces()
    {
        var world = CreateSmallWorld();
        // Center tile is SlopeTopRight: top=false, left=true, right=false, bottom=true
        SetTile(world, 5, 5, GemsparkAmber, BrickStyle.SlopeTopRight);
        SetTile(world, 5, 4, GemsparkAmber); // Up — but center's top face is blocked
        SetTile(world, 4, 5, GemsparkAmber); // Left — center's left face is open
        SetTile(world, 6, 5, GemsparkAmber); // Right — but center's right face is blocked

        var uv = TileFraming.CalculateSelfFrame8Way(world, 5, 5);

        // Only Left should connect (index = 2)
        // Grid positions (12,0), (12,1), (12,2)
        uv.X.ShouldBe(12);
        uv.Y.ShouldBeOneOf(0, 1, 2);
    }

    [Fact]
    public void SelfFrame8Way_NeighborSlopeBlocksConnection()
    {
        var world = CreateSmallWorld();
        SetTile(world, 5, 5, GemsparkAmber);
        // Up neighbor is SlopeBottomRight: top=true, left=false, right=true, bottom=false
        // Its bottom face is blocked, so it shouldn't connect downward to center
        SetTile(world, 5, 4, GemsparkAmber, BrickStyle.SlopeBottomRight);

        var uv = TileFraming.CalculateSelfFrame8Way(world, 5, 5);

        // Up neighbor's bottom is blocked → no connection → index 0
        uv.Y.ShouldBe(3);
        uv.X.ShouldBeOneOf(9, 10, 11);
    }

    #endregion

    #region Determinism

    [Fact]
    public void SelfFrame8Way_IsDeterministic()
    {
        var world = CreateSmallWorld();
        SetTile(world, 5, 5, GemsparkAmber);
        SetTile(world, 5, 4, GemsparkAmber);

        var uv1 = TileFraming.CalculateSelfFrame8Way(world, 5, 5);
        var uv2 = TileFraming.CalculateSelfFrame8Way(world, 5, 5);

        uv1.ShouldBe(uv2);
    }

    [Fact]
    public void DetermineFrameNumber_DefaultMode_Returns0Through2()
    {
        var seen = new HashSet<int>();
        for (int x = 0; x < 100; x++)
            for (int y = 0; y < 100; y++)
                seen.Add(TileFraming.DetermineFrameNumber(GemsparkAmber, x, y));

        seen.ShouldBeSubsetOf(new[] { 0, 1, 2 });
        seen.Count.ShouldBe(3);
    }

    #endregion

    #region WillItBlend

    [Fact]
    public void SelfFrame8Way_WillItBlend_SameTypeOnly()
    {
        var world = CreateSmallWorld();
        SetTile(world, 5, 5, GemsparkAmber);
        // Different gemspark type should NOT blend
        SetTile(world, 5, 4, 256); // Gemspark Amethyst

        var uv = TileFraming.CalculateSelfFrame8Way(world, 5, 5);

        // Different type → no neighbor → index 0
        uv.Y.ShouldBe(3);
        uv.X.ShouldBeOneOf(9, 10, 11);
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void SelfFrame8Way_BoundaryTiles_HandledSafely()
    {
        var world = CreateSmallWorld();
        SetTile(world, 0, 0, GemsparkAmber);

        // Should not throw
        var uv = TileFraming.CalculateSelfFrame8Way(world, 0, 0);
        (uv.X >= 0 && uv.Y >= 0).ShouldBeTrue();
    }

    [Fact]
    public void SelfFrame8Way_OutOfBounds_ReturnsZero()
    {
        var world = CreateSmallWorld();
        var uv = TileFraming.CalculateSelfFrame8Way(world, -1, -1);
        uv.ShouldBe(new Vector2Int32(0, 0));
    }

    #endregion

    #region Lookup Table Validation

    [Fact]
    public void SelfFrame8Way_LookupTable_Has47DefinedEntries()
    {
        var lookup = TileFraming.GetSelfFrame8WayLookup();
        int definedCount = 0;
        for (int i = 0; i < 256; i++)
            if (lookup[i] != null) definedCount++;

        definedCount.ShouldBe(47);
    }

    [Fact]
    public void SelfFrame8Way_LookupTable_AllEntriesHave3Variants()
    {
        var lookup = TileFraming.GetSelfFrame8WayLookup();
        for (int i = 0; i < 256; i++)
        {
            if (lookup[i] != null)
                lookup[i].Length.ShouldBe(3, $"Entry {i} should have 3 variants");
        }
    }

    [Fact]
    public void SelfFrame8Way_LookupTable_AllCoordinatesAreMultiplesOf18()
    {
        var lookup = TileFraming.GetSelfFrame8WayLookup();
        for (int i = 0; i < 256; i++)
        {
            if (lookup[i] == null) continue;
            for (int j = 0; j < 3; j++)
            {
                (lookup[i][j].X % 18).ShouldBe(0, $"Entry [{i}][{j}].X = {lookup[i][j].X}");
                (lookup[i][j].Y % 18).ShouldBe(0, $"Entry [{i}][{j}].Y = {lookup[i][j].Y}");
            }
        }
    }

    [Fact]
    public void GemsparkTileIds_Has18Entries()
    {
        TileFraming.GetGemsparkTileIds().Count.ShouldBe(18);
    }

    [Theory]
    [InlineData(255)]
    [InlineData(268)]
    [InlineData(385)]
    [InlineData(446)]
    [InlineData(447)]
    [InlineData(448)]
    public void IsGemSpark_ReturnsTrue_ForGemsparkTypes(ushort tileType)
    {
        TileFraming.IsGemSpark(tileType).ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(254)]
    [InlineData(269)]
    public void IsGemSpark_ReturnsFalse_ForNonGemsparkTypes(ushort tileType)
    {
        TileFraming.IsGemSpark(tileType).ShouldBeFalse();
    }

    #endregion
}
