using Shouldly;
using TEdit.Scripting.Api;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class TileApiTests
{
    private readonly World _world;
    private readonly NoOpUndoManager _undo;
    private readonly TileApi _api;

    public TileApiTests()
    {
        _world = TestWorldFactory.CreateWorldWithTerrain();
        _undo = new NoOpUndoManager();
        _api = new TileApi(_world, _undo);
    }

    [Fact]
    public void IsActive_ReturnsTrueForActiveTile()
    {
        // Below ground level (40) should be active
        _api.IsActive(10, 40).ShouldBeTrue();
    }

    [Fact]
    public void IsActive_ReturnsFalseForEmptyTile()
    {
        // Above ground level should be empty
        _api.IsActive(10, 5).ShouldBeFalse();
    }

    [Fact]
    public void GetType_ReturnsCorrectTileType()
    {
        // Just below ground level should be dirt (0)
        _api.GetTileType(10, 36).ShouldBe(0); // Dirt

        // Deep underground should be stone (1)
        _api.GetTileType(10, 45).ShouldBe(1); // Stone
    }

    [Fact]
    public void SetType_SetsTypeAndActivatesTile()
    {
        _api.SetType(5, 5, 1); // Set to stone

        _world.Tiles[5, 5].Type.ShouldBe((ushort)1);
        _world.Tiles[5, 5].IsActive.ShouldBeTrue();
        _undo.SavedTiles.ShouldContain((5, 5));
    }

    [Fact]
    public void SetWall_SetsWallType()
    {
        _api.SetWall(5, 5, 16); // Set to pearlstone wall

        _world.Tiles[5, 5].Wall.ShouldBe((ushort)16);
        _undo.SavedTiles.ShouldContain((5, 5));
    }

    [Fact]
    public void SetPaint_SetsTileColor()
    {
        _api.SetPaint(5, 5, 12);

        _world.Tiles[5, 5].TileColor.ShouldBe((byte)12);
    }

    [Fact]
    public void SetLiquid_SetsLiquidProperties()
    {
        _api.SetLiquid(5, 5, 255, 1); // Full water

        _world.Tiles[5, 5].LiquidAmount.ShouldBe((byte)255);
        _world.Tiles[5, 5].LiquidType.ShouldBe(LiquidType.Water);
    }

    [Fact]
    public void SetWire_SetsWireColors()
    {
        _api.SetWire(5, 5, 1, true); // Red wire
        _api.SetWire(5, 5, 3, true); // Green wire

        _world.Tiles[5, 5].WireRed.ShouldBeTrue();
        _world.Tiles[5, 5].WireGreen.ShouldBeTrue();
        _world.Tiles[5, 5].WireBlue.ShouldBeFalse();
    }

    [Fact]
    public void GetWire_ReadsWireState()
    {
        _world.Tiles[5, 5].WireBlue = true;

        _api.GetWire(5, 5, 2).ShouldBeTrue();
        _api.GetWire(5, 5, 1).ShouldBeFalse();
    }

    [Fact]
    public void Clear_ResetsAllTileProperties()
    {
        _world.Tiles[5, 5].IsActive = true;
        _world.Tiles[5, 5].Type = 1;
        _world.Tiles[5, 5].Wall = 2;
        _world.Tiles[5, 5].WireRed = true;

        _api.Clear(5, 5);

        _world.Tiles[5, 5].IsActive.ShouldBeFalse();
        _world.Tiles[5, 5].Type.ShouldBe((ushort)0);
        _world.Tiles[5, 5].Wall.ShouldBe((ushort)0);
        _world.Tiles[5, 5].WireRed.ShouldBeFalse();
    }

    [Fact]
    public void Copy_CopiesTileProperties()
    {
        _world.Tiles[5, 5].IsActive = true;
        _world.Tiles[5, 5].Type = 42;
        _world.Tiles[5, 5].Wall = 7;
        _world.Tiles[5, 5].WireRed = true;

        _api.Copy(5, 5, 10, 10);

        _world.Tiles[10, 10].IsActive.ShouldBeTrue();
        _world.Tiles[10, 10].Type.ShouldBe((ushort)42);
        _world.Tiles[10, 10].Wall.ShouldBe((ushort)7);
        _world.Tiles[10, 10].WireRed.ShouldBeTrue();
    }

    [Fact]
    public void SetFrameUV_SetsFrameCoordinates()
    {
        _api.SetFrameUV(5, 5, 18, 36);

        _world.Tiles[5, 5].U.ShouldBe((short)18);
        _world.Tiles[5, 5].V.ShouldBe((short)36);
    }

    [Fact]
    public void OutOfBounds_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _api.IsActive(-1, 0));
        Should.Throw<ArgumentException>(() => _api.IsActive(0, -1));
        Should.Throw<ArgumentException>(() => _api.IsActive(200, 0)); // world is 100 wide
        Should.Throw<ArgumentException>(() => _api.IsActive(0, 200)); // world is 100 high
    }
}
