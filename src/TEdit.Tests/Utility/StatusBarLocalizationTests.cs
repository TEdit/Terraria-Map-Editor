using System.Globalization;
using TEdit.Properties;
using TEdit.Terraria;
using TEdit.UI;
using Xunit;

namespace TEdit.Tests.Utility;

[CollectionDefinition("UI localization", DisableParallelization = true)]
public class UiLocalizationCollection;

[Collection("UI localization")]
public class StatusBarLocalizationTests
{
    [Theory]
    [InlineData("de-DE", "[leer]")]
    [InlineData("zh-Hans", "[空]")]
    [InlineData("ja-JP", "[空]")]
    public void EmptyTileAndPaint_UseSelectedLanguage(string locale, string emptyLabel)
    {
        var previous = Language.Culture;
        try
        {
            Language.Culture = CultureInfo.GetCultureInfo(locale);
            var status = new MouseTile { Tile = new Tile() };
            Assert.Equal(emptyLabel, status.TileName);
            Assert.Equal(Language.common_none, status.Paint);
        }
        finally { Language.Culture = previous; }
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("zh-Hans")]
    [InlineData("ja-JP")]
    public void ExtrasAndPaintLabels_UseSelectedLanguage(string locale)
    {
        var previous = Language.Culture;
        try
        {
            Language.Culture = CultureInfo.GetCultureInfo(locale);
            var status = new MouseTile
            {
                Tile = new Tile
                {
                    InActive = true, Actuator = true, WireRed = true, WireBlue = true,
                    TileColor = 1, WallColor = 2
                }
            };
            Assert.Contains(Language.mask_section_inactive, status.TileExtras);
            Assert.Contains(Language.mask_section_actuator, status.TileExtras);
            Assert.Contains(Language.tool_paint_wire_red, status.TileExtras);
            Assert.Contains(Language.tool_paint_wire_blue, status.TileExtras);
            Assert.StartsWith(Language.statusbar_tile_label + ":", status.Paint);
            Assert.Contains(Language.statusbar_wall_label + ":", status.Paint);
        }
        finally { Language.Culture = previous; }
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("zh-Hans")]
    [InlineData("ja-JP")]
    public void ReportedMissingLabels_HaveTranslationsWithoutParentFallback(string locale)
    {
        var resources = Language.ResourceManager.GetResourceSet(
            CultureInfo.GetCultureInfo(locale), true, false)!;
        foreach (var key in new[]
        {
            "wall_sky", "common_air", "statusbar_empty_tile", "about_version",
            "dialog_legacy_world_prompt", "dialog_legacy_world_title",
            "dialog_legacy_tmodloader_prompt", "dialog_legacy_tmodloader_title"
        })
        {
            var text = resources.GetString(key);
            Assert.False(string.IsNullOrWhiteSpace(text), $"Missing {locale}: {key}");
            if (key.EndsWith("_prompt", StringComparison.Ordinal))
            {
                Assert.Contains("{0}", text);
                Assert.Contains("123", string.Format(text!, 123));
            }
        }
    }

    [Fact]
    public void InvalidTileWallAndPaint_UseLocalizedMessages()
    {
        var previous = Language.Culture;
        try
        {
            Language.Culture = CultureInfo.GetCultureInfo("zh-Hans");
            var status = new MouseTile
            {
                Tile = new Tile { IsActive = true, Type = ushort.MaxValue, Wall = ushort.MaxValue, TileColor = byte.MaxValue }
            };
            Assert.Equal("无效物块（65535）", status.TileName);
            Assert.Equal("无效墙壁（65535）", status.WallName);
            Assert.Contains("未知油漆（255）", status.Paint);
        }
        finally { Language.Culture = previous; }
    }
}
