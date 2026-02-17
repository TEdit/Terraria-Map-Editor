using Shouldly;
using TEdit.Terraria.Player;

namespace TEdit.Terraria.Tests.Player;

public class PlayerAppearanceTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(5, false)]
    [InlineData(9, false)]
    public void Male_DerivedFromSkinVariant(byte skinVariant, bool expectedMale)
    {
        var appearance = new PlayerAppearance { SkinVariant = skinVariant };
        appearance.Male.ShouldBe(expectedMale);
    }

    [Fact]
    public void DefaultColors_AreNotBlack()
    {
        var appearance = new PlayerAppearance();
        appearance.HairColor.R.ShouldBeGreaterThan((byte)0);
        appearance.SkinColor.R.ShouldBeGreaterThan((byte)0);
        appearance.EyeColor.R.ShouldBeGreaterThan((byte)0);
    }
}
