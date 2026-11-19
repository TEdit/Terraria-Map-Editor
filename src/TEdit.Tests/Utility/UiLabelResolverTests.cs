using System.Collections;
using System.Globalization;
using System.Linq;
using Shouldly;
using TEdit.Properties;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Utility;

public class UiLabelResolverTests
{
    [Fact]
    public void EveryFrameTextResource_IsReachableFromItsEnglishText()
    {
        var entries = Language.ResourceManager
            .GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: true)!
            .Cast<DictionaryEntry>()
            .Where(e => ((string)e.Key).StartsWith("frame_text_", System.StringComparison.Ordinal))
            .ToList();

        entries.Count.ShouldBeGreaterThan(50);
        foreach (var entry in entries)
        {
            var english = (string)entry.Value!;
            UiLabelResolver.Resolve(LocalizationUiLabels.FrameTextPrefix + english)
                .ShouldNotBeNull($"no resource reachable for \"{english}\" ({entry.Key})");
        }
    }

    [Theory]
    [InlineData("Anchor.Left")]
    [InlineData(LocalizationUiLabels.WallNaturalFormat)]
    [InlineData(LocalizationUiLabels.WallUnusedFormat)]
    [InlineData(LocalizationUiLabels.WallSky)]
    [InlineData(LocalizationUiLabels.WallUnbreakable)]
    [InlineData(LocalizationUiLabels.PaintNone)]
    public void FixedLabels_Resolve(string key)
    {
        UiLabelResolver.Resolve(key).ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void UnknownFrameText_ReturnsNull()
    {
        UiLabelResolver.Resolve(LocalizationUiLabels.FrameTextPrefix + "Not A Real Frame Label").ShouldBeNull();
    }
}
