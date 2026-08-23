using System.Globalization;
using Shouldly;
using TEdit.Converters;
using Xunit;

namespace TEdit.Tests.Converters;

public class MaximumTileIndexConverterTests
{
    private readonly MaximumTileIndexConverter _converter = new();

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(4200, 4199)]
    public void Convert_ReturnsLastValidTileIndex(int size, int expected)
    {
        _converter.Convert(size, typeof(double), null!, CultureInfo.InvariantCulture).ShouldBe(expected);
    }
}
