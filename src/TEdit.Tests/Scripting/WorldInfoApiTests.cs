using Shouldly;
using TEdit.Scripting.Api;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class WorldInfoApiTests
{
    private readonly World _world;
    private readonly WorldInfoApi _api;

    public WorldInfoApiTests()
    {
        _world = TestWorldFactory.CreateSmallWorld();
        _api = new WorldInfoApi(_world);
    }

    [Fact]
    public void Width_ReturnsWorldWidth()
    {
        _api.Width.ShouldBe(100);
    }

    [Fact]
    public void Height_ReturnsWorldHeight()
    {
        _api.Height.ShouldBe(100);
    }

    [Fact]
    public void Title_ReturnsWorldTitle()
    {
        _api.Title.ShouldBe("Test World");
    }

    [Fact]
    public void SpawnX_ReturnsSpawnPosition()
    {
        _api.SpawnX.ShouldBe(50);
    }

    [Fact]
    public void SurfaceLevel_ReturnsGroundLevel()
    {
        _api.SurfaceLevel.ShouldBe(35);
    }
}
