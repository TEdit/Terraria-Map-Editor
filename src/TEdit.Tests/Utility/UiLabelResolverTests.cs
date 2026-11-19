using System;
using System.Globalization;
using Shouldly;
using TEdit.Properties;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Utility;

[Collection("UI localization")]
public class UiLabelResolverTests : IDisposable
{
    private readonly CultureInfo? _previousCulture = Language.Culture;

    public UiLabelResolverTests() => Language.Culture = CultureInfo.GetCultureInfo("en");
    public void Dispose() => Language.Culture = _previousCulture;

    // Original labels are identities in tiles.json; each has a whole-label translation resource.
    [Theory]
    [InlineData("Off", "Off")]
    [InlineData("On", "On")]
    [InlineData("Monster Banner", "Monster Banner")]
    [InlineData("Left", "Left")]
    [InlineData("Right", "Right")]
    [InlineData("Small Decos", "Small Decos")]
    [InlineData("Forest Tree", "Forest Tree")]
    [InlineData("Topaz Tree", "Topaz Tree")]
    [InlineData("Amethyst Tree", "Amethyst Tree")]
    [InlineData("Sapphire Tree", "Sapphire Tree")]
    [InlineData("Emerald Tree", "Emerald Tree")]
    [InlineData("Ruby Tree", "Ruby Tree")]
    [InlineData("Diamond Tree", "Diamond Tree")]
    [InlineData("Amber Tree", "Amber Tree")]
    [InlineData("Vanity Tree Sakura", "Vanity Tree Sakura")]
    [InlineData("Vanity Tree Yellow Willow", "Vanity Tree Yellow Willow")]
    [InlineData("Ash Tree", "Ash Tree")]
    [InlineData("Single", "Single")]
    [InlineData("Small Rubble (1x1)", "Small Rubble (1x1)")]
    [InlineData("Flat", "Flat")]
    [InlineData("Endcap Right", "Endcap Right")]
    [InlineData("Endcap Left", "Endcap Left")]
    [InlineData("Attach Left", "Attach Left")]
    [InlineData("Attach Right", "Attach Right")]
    [InlineData("Attach Left Endcap", "Attach Left Endcap")]
    [InlineData("Attach Right Endcap", "Attach Right Endcap")]
    [InlineData("Stair Up-Right Riser", "Stair Up-Right Riser")]
    [InlineData("Stair Up-Right Stringer", "Stair Up-Right Stringer")]
    [InlineData("Stair Up-Left Riser", "Stair Up-Left Riser")]
    [InlineData("Stair Up-Left Stringer", "Stair Up-Left Stringer")]
    [InlineData("Stair Top Landing R", "Stair Top Landing Right")]
    [InlineData("Stair Top Landing L", "Stair Top Landing Left")]
    [InlineData("Stair Top Landing L-R", "Stair Top Landing Left–Right")]
    [InlineData("Stair Top Landing R Endcap", "Stair Top Landing Right Endcap")]
    [InlineData("Stair Top Landing L Endcap", "Stair Top Landing Left Endcap")]
    [InlineData("Stair Bottom Landing R", "Stair Bottom Landing Right")]
    [InlineData("Stair Bottom Landing L", "Stair Bottom Landing Left")]
    [InlineData("Stair Inset Up-Right 1", "Stair Inset Up-Right 1")]
    [InlineData("Stair Inset Up-Left 1", "Stair Inset Up-Left 1")]
    [InlineData("Stair Inset Up-Right 2", "Stair Inset Up-Right 2")]
    [InlineData("Stair Inset Up-Left 2", "Stair Inset Up-Left 2")]
    [InlineData("Stair Inset Up-Right 3", "Stair Inset Up-Right 3")]
    [InlineData("Stair Inset Up-Left 3", "Stair Inset Up-Left 3")]
    [InlineData("Stair Inverted Right", "Stair Inverted Right")]
    [InlineData("Stair Inverted Left", "Stair Inverted Left")]
    [InlineData("Medium Rubble (2x1)", "Medium Rubble (2x1)")]
    [InlineData("Decos (3x2) (Group 2)", "Decorations (3x2) (Group 2)")]
    [InlineData("Large Rubble (3x2) (Group 2)", "Large Rubble (3x2) (Group 2)")]
    [InlineData("Forest Short Plants", "Forest Short Plants")]
    [InlineData("Forest Tall Plants", "Forest Tall Plants")]
    [InlineData("Text Statue", "Text Statue")]
    [InlineData("Decos (3x2)", "Decorations (3x2)")]
    [InlineData("Large Rubble (3x2)", "Large Rubble (3x2)")]
    [InlineData("Pumpkin Plant", "Pumpkin Plant")]
    [InlineData("Boss Trophy", "Boss Trophy")]
    [InlineData("Critter Anchor", "Critter Anchor")]
    [InlineData("Forest Pot", "Forest Pot")]
    [InlineData("Corruption Short Plants", "Corruption Short Plants")]
    [InlineData("Jungle Short Plants", "Jungle Short Plants")]
    [InlineData("Hallow Short Plants", "Hallow Short Plants")]
    [InlineData("Crimson Short Plants", "Crimson Short Plants")]
    [InlineData("Ore Bars", "Ore Bars")]
    [InlineData("Jungle Plants Echo", "Jungle Plants Echo")]
    [InlineData("Locked", "Locked")]
    [InlineData("Jungle Large Plants", "Jungle Large Plants")]
    public void OriginalFrameLabels_ResolveToReadableEnglish(string original, string expected)
    {
        UiLabelResolver.Resolve(LocalizationUiLabels.FrameTextPrefix + original).ShouldBe(expected);
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

    [Theory]
    [InlineData("Not A Real Frame Label")]
    [InlineData("Stair Top Landing Z")]
    [InlineData("Stair Inset Up-Right 99")]
    [InlineData("Endcap Left Endcap")]
    [InlineData("Stair Bottom Landing R Endcap")]
    public void UnknownFrameText_ReturnsNull(string original)
    {
        UiLabelResolver.Resolve(LocalizationUiLabels.FrameTextPrefix + original).ShouldBeNull();
    }

    [Fact]
    public void MissingCultureTranslation_UsesReadableEnglishLabel()
    {
        Language.Culture = CultureInfo.GetCultureInfo("ja-JP");
        UiLabelResolver.Resolve(LocalizationUiLabels.FrameTextPrefix + "Stair Top Landing L-R")
            .ShouldBe("Stair Top Landing Left–Right");
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("zh-Hans")]
    [InlineData("ja-JP")]
    [InlineData("fr-FR")]
    [InlineData("es-ES")]
    [InlineData("ru-RU")]
    [InlineData("it-IT")]
    [InlineData("ko-KR")]
    [InlineData("pl-PL")]
    [InlineData("pt-BR")]
    [InlineData("zh-Hant")]
    [InlineData("ar-BH")]
    public void FullLabels_AreReadableInEveryUiCulture(string locale)
    {
        Language.Culture = CultureInfo.GetCultureInfo(locale);
        foreach (var original in new[]
        {
            "Endcap Left", "Attach Right", "Attach Left Endcap",
            "Stair Up-Right Riser", "Stair Up-Left Stringer",
            "Stair Top Landing L-R", "Stair Top Landing R Endcap",
            "Stair Bottom Landing L", "Stair Inset Up-Right 3", "Stair Inverted Left",
            "Small Rubble (1x1)", "Medium Rubble (2x1)",
            "Large Rubble (3x2) (Group 2)", "Decos (3x2) (Group 2)"
        })
        {
            var label = UiLabelResolver.Resolve(LocalizationUiLabels.FrameTextPrefix + original);
            label.ShouldNotBeNullOrWhiteSpace();
            label.ShouldNotContain("{0}");
            label.ShouldNotContain("{1}");
            label.ShouldNotContain("\n");
        }
    }
}
