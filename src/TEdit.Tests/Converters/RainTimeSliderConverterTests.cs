using System.Globalization;
using Shouldly;
using TEdit.Converters;
using TEdit.Properties;
using Xunit;

namespace TEdit.Tests.Converters;

public class RainTimeSliderConverterTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    private readonly RainTimeSliderConverter _slider = new();
    private readonly RainTimeDisplayConverter _display = new();
    private readonly PermanentRainTimeConverter _permanent = new();

    [Theory]
    [InlineData(0, 0)]
    [InlineData(RainTimeValues.OneHour, RainTimeValues.OneHourSliderValue)]
    [InlineData(RainTimeValues.OneYear, RainTimeValues.OneYearSliderValue)]
    public void Convert_MapsLandmarksToExpectedSliderPositions(int rainTime, double sliderValue)
    {
        ((double)_slider.Convert(rainTime, typeof(double), null!, Culture)).ShouldBe(sliderValue, 0.000001);
    }

    [Theory]
    [InlineData(99.5, RainTimeValues.OneYear)]
    [InlineData(RainTimeValues.OneYearSliderValue, RainTimeValues.OneYear)]
    public void ConvertBack_SnapsToSpecialRainTimes(double sliderValue, int rainTime)
    {
        _slider.ConvertBack(sliderValue, typeof(int), null!, Culture).ShouldBe(rainTime);
    }

    [Fact]
    public void PermanentOverride_UsesTheGamePermanentValue()
    {
        _permanent.Convert(RainTimeValues.Permanent, typeof(bool), null!, Culture).ShouldBe(true);
        _permanent.Convert(RainTimeValues.OneYear, typeof(bool), null!, Culture).ShouldBe(false);
        _permanent.ConvertBack(true, typeof(int), null!, Culture).ShouldBe(RainTimeValues.Permanent);
        _permanent.ConvertBack(false, typeof(int), null!, Culture).ShouldBe(RainTimeValues.OneYear);
    }

    [Theory]
    [InlineData(60000)]
    [InlineData(RainTimeValues.MaximumNormal)]
    [InlineData(1000000)]
    [InlineData(86400000)]
    public void SliderMapping_RoundTripsRepresentativeValues(int rainTime)
    {
        double sliderValue = (double)_slider.Convert(rainTime, typeof(double), null!, Culture);
        int roundTripped = (int)_slider.ConvertBack(sliderValue, typeof(int), null!, Culture);

        roundTripped.ShouldBe(rainTime);
    }

    [Theory]
    [InlineData(RainTimeValues.Permanent)]
    [InlineData(RainTimeValues.OneYear)]
    [InlineData(3600)]
    [InlineData(216000)]
    public void Display_UsesLocalizedHumanReadableDurations(int rainTime)
    {
        string expected = rainTime switch
        {
            RainTimeValues.Permanent => Language.tool_wp_rain_duration_permanent,
            RainTimeValues.OneYear => Language.tool_wp_rain_duration_one_year,
            3600 => string.Format(Culture, Language.tool_wp_rain_duration_minutes, 1),
            _ => string.Format(Culture, Language.tool_wp_rain_duration_hours, 1),
        };

        _display.Convert(rainTime, typeof(string), null!, Culture).ShouldBe(expected);
    }

    [Fact]
    public void Display_DoesNotTreatLongerDurationsAsPermanent()
    {
        const int rainTime = 31207680;
        string expected = string.Format(Culture, Language.tool_wp_rain_duration_days, 6.02);

        _display.Convert(rainTime, typeof(string), null!, Culture).ShouldBe(expected);
    }
}
