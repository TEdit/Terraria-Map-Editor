using Shouldly;
using TEdit.Scripting.Api;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Scripting;

public class SignApiTests
{
    private readonly World _world;
    private readonly SignApi _api;

    public SignApiTests()
    {
        _world = TestWorldFactory.CreateWorldWithChestsAndSigns();
        _api = new SignApi(_world);
    }

    [Fact]
    public void Count_ReturnsSignCount()
    {
        _api.Count.ShouldBe(1);
    }

    [Fact]
    public void GetAll_ReturnsAllSigns()
    {
        var signs = _api.GetAll();
        signs.Count.ShouldBe(1);
        signs[0]["text"].ShouldBe("Hello from TEdit!");
    }

    [Fact]
    public void GetAt_ReturnsSignAtPosition()
    {
        var sign = _api.GetAt(15, 40);
        sign.ShouldNotBeNull();
        sign!["text"].ShouldBe("Hello from TEdit!");
    }

    [Fact]
    public void GetAt_ReturnsNullForNoSign()
    {
        var sign = _api.GetAt(0, 0);
        sign.ShouldBeNull();
    }

    [Fact]
    public void SetText_UpdatesSignText()
    {
        _api.SetText(15, 40, "New text!");

        var sign = _world.GetSignAtTile(15, 40);
        sign.ShouldNotBeNull();
        sign!.Text.ShouldBe("New text!");
    }
}
