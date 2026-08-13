using Shouldly;
using TEdit.Editor;
using TEdit.Terraria;
using TEdit.Terraria.DataModel;
using Xunit;

namespace TEdit.Tests.Editor;

public class MorphToolOptionsTests
{
    [Fact]
    public void DefaultsToSafeConversionTargets()
    {
        var options = new MorphToolOptions();

        options.Mode.ShouldBe(MorphMode.SafeConvert);
        options.TargetBiomes.ShouldBe(WorldConfiguration.SafeBiomes, ignoreOrder: false);
    }

    [Fact]
    public void GenerateModeSwitchesToDestructiveTargetList()
    {
        var options = new MorphToolOptions
        {
            Mode = MorphMode.GenerateBiome
        };

        options.TargetBiomes.ShouldBe(WorldConfiguration.DestructiveBiomes, ignoreOrder: false);
        WorldConfiguration.GetMorphSettings(options.Mode)
            .ShouldBeSameAs(WorldConfiguration.DestructiveMorphSettings);
    }
}
